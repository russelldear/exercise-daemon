﻿using System.Net;
using ExerciseDaemon.Signup.BackgroundWorker;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

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
                    collection.TryAddSingleton<ISubstitutionBinder, SubstitutionBinder>();

                    collection.AddHostedService<TimedBackgroundWorker>();
                })
                .UseKestrel(options => { options.Listen(IPAddress.Any, 5236); })
                .UseUrls("http://*:5236")
                .UseStartup<Startup>()
                .Build();
    }
}
