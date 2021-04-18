using Bot.Service.Application.Comments.Stores;
using Bot.Service.Application.Consumers;
using Bot.Service.Application.Monitor.Services;
using Bot.Service.Application.Reddit.Services;
using Bot.Service.Application.StringSearch.Services;
using Bot.Service.Application.Templates.Services;
using Bot.Service.BackgroundWorkers;
using Bot.Service.Common.Models;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Bot.Service.Common.Extensions.Startup
{
    public static class ServicesHostingBuilderExtensions
    {
        /// <summary>
        /// Adds all the necessary services to the <see cref="IHostBuilder"/>.
        /// </summary>
        /// <param name="builder">The application host builder</param>
        /// <returns>The IHostBuilder with the services configured</returns>
        public static IHostBuilder AddServices(this IHostBuilder builder)
        {
            return builder.ConfigureServices((hostContext, services) =>
            {
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
                services.AddMassTransitHostedService();
                  
                services.AddSingleton<ICommentStore, CommentStore>();
                services.AddSingleton<IProcessedCommentStore, ProcessedCommentStore>();
                services.AddSingleton<IReceivedCommentStore, ReceivedCommentStore>();
                services.AddSingleton<ICommentStore>(s => s.GetRequiredService<IProcessedCommentStore>());
                services.AddSingleton<ICommentStore>(s => s.GetRequiredService<IReceivedCommentStore>());                    

                services.AddSingleton<IRedditProvider, RedditProvider>();
                services.AddSingleton<IStringSearcher, StringSearcher>();
                services.AddSingleton<ISubredditProvider, InMemorySubredditProvider>();
                services.AddSingleton<ISubredditMonitor, SubredditCommentMonitor>();
                services.AddSingleton<ITemplateProvider, InMemoryTemplateProvider>();
                    
                services.AddHostedService<ClientStoreCleanupWorker>();
                services.AddHostedService<Worker>();
                    
                services.Configure<AppSettings>(hostContext.Configuration);
            });
        } 
    }
}