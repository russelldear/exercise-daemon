using System.Net;
using ExerciseDaemon.BackgroundWorker;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Serilog;

namespace ExerciseDaemon
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
                    collection.TryAddSingleton<ISubstitutionBinder, SubstitutionBinder>();

                    collection.AddHostedService<TimedBackgroundWorker>();
                })
                .UseKestrel(options => { options.Listen(IPAddress.Any, 5236); })
                .UseSerilog((hostingContext, loggerConfiguration) =>
                    loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration)
                )
                .UseUrls("http://*:5236")
                .UseStartup<Startup>()
                .Build();
    }
}
