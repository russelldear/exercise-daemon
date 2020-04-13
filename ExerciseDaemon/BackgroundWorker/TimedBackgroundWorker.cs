using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ExerciseDaemon.BackgroundWorker
{
    public class TimedBackgroundWorker : IHostedService, IDisposable
    {
        private const int FrequencySeconds = 120;

        private readonly DaemonTask _daemonTask;
        private readonly ILogger<TimedBackgroundWorker> _logger;
        private Timer _timer;

        public TimedBackgroundWorker(DaemonTask daemonTask, ILogger<TimedBackgroundWorker> logger)
        {
            _daemonTask = daemonTask;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(FrequencySeconds));

            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            _logger.LogInformation("Timed background worker starting.");

            _daemonTask.Run().Wait();
            
            _logger.LogInformation("Timed background worker ending.");
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
