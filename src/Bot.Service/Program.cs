using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Bot.Service.Application.Comments.Services;
using Bot.Service.Application.Consumers;
using Bot.Service.Application.Monitor.Services;
using Bot.Service.Application.Reddit;
using Bot.Service.Application.Reddit.Services;
using Bot.Service.Application.StringSearch.Services;
using Bot.Service.Application.Templates.Services;
using Bot.Service.Common;
using Bot.Service.Common.Models;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;
using IHost = Microsoft.Extensions.Hosting.IHost;

namespace Bot.Service
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                using IHost host = CreateHostBuilder(args).Build();
                host.Run();
            }
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

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging(loggingBuilder =>
                { 
                    loggingBuilder.ClearProviders();
                    
                    var configuration = new ConfigurationBuilder()
                        .AddJsonFile("appsettings.json")
                        .Build();
                    var logger = new LoggerConfiguration()
                        .ReadFrom.Configuration(configuration)
                        .CreateLogger();
                    
                    loggingBuilder.AddSerilog(logger, dispose: true);
                })
                .ConfigureAppConfiguration((hostContext, builder) =>
                {
                    builder.SetBasePath(Directory.GetCurrentDirectory());
                    builder.AddJsonFile("appsettings.json", false, true);
                    builder.AddCommandLine(args);
                    builder.AddEnvironmentVariables();
                    
                    if (hostContext.HostingEnvironment.IsDevelopment())
                    {
                        builder.AddUserSecrets<Program>();
                    }
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<Worker>();
                    services.Configure<AppSettings>(hostContext.Configuration);
                    services.AddSingleton(s => s.GetService<IOptions<AppSettings>>()!.Value);

                    services.AddMassTransit(x =>
                    {
                        x.AddConsumers(typeof(NewCommentTriggerProcessorConsumer).Assembly);
                        
                        x.UsingInMemory((context, cfg) =>
                        {
                            cfg.TransportConcurrencyLimit = 100;
                            cfg.ConfigureEndpoints(context);
                        });
                    });

                    services.AddSingleton<IProcessedCommentStore, ProcessedCommentStore>();
                    services.AddSingleton<IRedditProvider, RedditProvider>();
                    services.AddSingleton<IStringSearcher, StringSearcher>();
                    services.AddSingleton<ISubredditProvider, InMemorySubredditProvider>();
                    services.AddSingleton<ISubredditMonitor, SubredditNewCommentMonitor>();
                    services.AddSingleton<ITemplateProvider, InMemoryTemplateProvider>();
                });
    }
}