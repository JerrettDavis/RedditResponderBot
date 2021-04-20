using System;
using Bot.Service.Common.Extensions.Startup;
using System.IO;
using Bot.Service.Application.Comments.Services;
using Bot.Service.Application.Consumers;
using Bot.Service.Application.Monitor.Services;
using Bot.Service.Application.Reddit.Services;
using Bot.Service.Application.StringSearch.Services;
using Bot.Service.Application.Templates.Services;
using Bot.Service.BackgroundWorkers;
using Bot.Service.Common.Extensions.Startup;
using Bot.Service.Common.Models;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Bot.Service
{
    /// <summary>
    /// The main class of the application
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Called by the dotnet runtime
        /// </summary>
        /// <param name="args">Arguments for the application</param>
        public static void Main(string[] args)
        {
            try
            {
                // Let's get the host built and ready to roll.
                using IHost host = CreateHostBuilder(args).Build();
                // Fire up the application
                host.Run();
            }
            // This will catch any-and-all uncaught errors from the application.
            catch (Exception ex)
            {
                if (Log.Logger == null || Log.Logger.GetType().Name == "SilentLogger")
                {
                    Log.Logger = new LoggerConfiguration()
                        .MinimumLevel.Debug()
                        .WriteTo.Console()
                        .CreateLogger();
                }

                Log.Fatal(ex, "Host terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        /// <summary>
        /// Gets the application host configured and ready to go
        /// </summary>
        /// <param name="args">The arguments to pass to the application</param>
        /// <returns>The configured host builder</returns>
        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .AddLogging()
                .AddConfiguration(args)
                .AddServices();
    }
}