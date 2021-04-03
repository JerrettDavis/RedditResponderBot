using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bot.Service.Application.Monitor.Services;
using Bot.Service.Application.Reddit.Services;
using Bot.Service.Application.StringSearch.Models;
using Bot.Service.Application.StringSearch.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Reddit.Controllers;
using Reddit.Controllers.EventArgs;
using Reddit.Exceptions;

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

            var monitorTasks = subredditNames.Select(n => 
                _monitor.Monitor(n, stoppingToken))
                .ToList();

            await Task.WhenAll(monitorTasks);

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {Time}", DateTimeOffset.Now);

                await Task.Delay(1000, stoppingToken);
            }

            _monitor.Dispose();
        }
    }
}