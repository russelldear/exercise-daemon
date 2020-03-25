using ExerciseDaemon.Signup.BackgroundWorker;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace ExerciseDaemon.Signup
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureServices((context, collection) =>
                {
                    collection.AddHostedService<TimedBackgroundWorker>();
                })
                .UseStartup<Startup>()
                .Build();
    }
}
