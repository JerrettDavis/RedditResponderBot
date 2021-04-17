using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bot.Service.Application.Comments.Stores;
using Bot.Service.Common.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Bot.Service.BackgroundWorkers
{
    /// <summary>
    /// Used to monitor in-memory comment stores and cleanup stale records.
    /// </summary>
    public class ClientStoreCleanupWorker : BackgroundService
    {
        private readonly ILogger<ClientStoreCleanupWorker> _logger;
        private readonly IEnumerable<ICommentStore> _commentStores;

        public ClientStoreCleanupWorker(
            ILogger<ClientStoreCleanupWorker> logger, 
            IEnumerable<ICommentStore> commentStores)
        {
            _logger = logger;
            _commentStores = commentStores;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // We want to wait a minute before we start
                await Task.Delay(60000, stoppingToken);
                
                _logger.LogInformation("Starting client store cleanup process");

                await _commentStores.ForEachAsync(async cs =>
                {
                    _logger.LogInformation("Cleaning {Store}", cs.GetType().FullName);
                    // Get the newest entry in the collection
                    var latest = cs.Store.Values
                        .OrderByDescending(c => c.Created)
                        .FirstOrDefault();


                    if (latest == null)
                        return;
                    
                    // Get all comments more than 5 minutes older than the latest comment
                    var oldComments = cs.Store.Where(c => 
                        latest.Created - c.Value.Created > TimeSpan.FromMinutes(5))
                        .Select(c => c.Key)
                        .ToList();
                    
                    // Remove all the old comments
                    _logger.LogInformation("Removing {Count} comments from {Store}", oldComments.Count, cs.GetType().FullName);
                    await oldComments.ForEachAsync(async c =>
                    {
                        var (key, value) = c;
                        await cs.Remove(key, value, stoppingToken);
                    }, cancellationToken: stoppingToken);
                }, cancellationToken: stoppingToken);
            }
        }
    }
}