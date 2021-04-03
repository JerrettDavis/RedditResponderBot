using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bot.Service.Application.Monitor.Services;
using Bot.Service.Application.Reddit.Services;
using Bot.Service.Common.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Bot.Service
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly ISubredditProvider _subredditProvider;
        private readonly ISubredditMonitor _monitor;
        
        public Worker(
            ILogger<Worker> logger,
            ISubredditProvider subredditProvider, 
            ISubredditMonitor monitor)
        {
            _logger = logger;
            _subredditProvider = subredditProvider;
            _monitor = monitor;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting bot");

            var subredditNames = _subredditProvider.GetMonitoredSubs().ToList();
            
            await subredditNames.ForEachAsync(async subreddit =>
            {
                await _monitor.Monitor(subreddit, stoppingToken);
            }, cancellationToken: stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {Time}", DateTimeOffset.Now);

                await Task.Delay(1000, stoppingToken);
            }
        }

        public sealed override void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _monitor.Dispose();
            }
        }
    }
}