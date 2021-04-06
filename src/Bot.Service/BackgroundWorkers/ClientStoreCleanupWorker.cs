// TedCruzResponderBot - Simple real-time chat application.
// Copyright (C) 2021  Jerrett D. Davis
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bot.Service.Application.Comments.Services;
using Bot.Service.Common.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Bot.Service.BackgroundWorkers
{
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