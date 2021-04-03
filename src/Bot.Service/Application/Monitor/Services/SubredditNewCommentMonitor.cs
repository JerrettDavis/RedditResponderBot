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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bot.Service.Application.Comments.Models;
using Bot.Service.Application.Comments.Services;
using Bot.Service.Application.Reddit.Services;
using Bot.Service.Common.Models.Messages;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Reddit;
using Reddit.Controllers;
using Reddit.Controllers.EventArgs;
using Reddit.Exceptions;

namespace Bot.Service.Application.Monitor.Services
{
    public class SubredditNewCommentMonitor : ISubredditMonitor
    {
        private readonly ILogger<SubredditNewCommentMonitor> _logger;
        private readonly IReceivedCommentStore _receivedCommentStore;
        private readonly IServiceProvider _services;
            
        private readonly IDictionary<string, Subreddit> _monitoredSubreddits;
        private readonly RedditClient _reddit;

        public SubredditNewCommentMonitor(
            IRedditProvider redditProvider,
            ILogger<SubredditNewCommentMonitor> logger, 
            IServiceProvider services, 
            IReceivedCommentStore receivedCommentStore)
        {
            _reddit = redditProvider.GetClient();
            _logger = logger;
            _services = services;
            _receivedCommentStore = receivedCommentStore;

            _monitoredSubreddits = new ConcurrentDictionary<string,Subreddit>();
        }

        public Task Monitor(
            string subredditName, 
            CancellationToken cancellationToken = default)
        {
            return Task.Run(() =>
            {
                _logger.LogInformation("Processing Subreddit {Subreddit}", subredditName);
                var subreddit = _reddit.Subreddit(subredditName);
            
                _logger.LogInformation("Initializing {Subreddit}", subredditName);
                subreddit.Comments.GetNew(limit: 100);
                
                _logger.LogInformation("Starting monitoring for {Subreddit}", subredditName);
                subreddit.Comments.MonitorNew(limit: 100);
                
                _logger.LogInformation("Adding event handler for {Subreddit}", subredditName);
                subreddit.Comments.NewUpdated += CommentsOnNewUpdated;

                _monitoredSubreddits.Add(subredditName, subreddit);
            }, cancellationToken);
        }

        public Task StopMonitoring(
            string subredditName, 
            CancellationToken cancellationToken = default)
        {
            return Task.Run(() =>
            {
                var subreddit = _monitoredSubreddits[subredditName];

                subreddit.Comments.MonitorNew();
                subreddit.Comments.NewUpdated -= CommentsOnNewUpdated;

                _monitoredSubreddits.Remove(subredditName);
            }, cancellationToken);
        }
        
        private async Task NewCommentsReceived(object? sender, CommentsUpdateEventArgs e)
        {
            var subredditName = ((global::Reddit.Controllers.Comments) sender!).SubKey;
            _logger.LogInformation(
                "Received {Count} new comments for {Subreddit}", 
                e.Added.Count, 
                subredditName);

            var oldComments = new List<string>();
            var commentTasks = new List<Task>();
            foreach (var old in e.OldComments)
            {
                oldComments.Add(old.Fullname);
                commentTasks.Add(_receivedCommentStore.Add(CommentStoreConstants.OldCommentsQueue, old));
            }
            
            var newComments = new List<string>();
            foreach (var newComment in e.NewComments)
            {
                newComments.Add(newComment.Fullname);
                commentTasks.Add(_receivedCommentStore.Add(CommentStoreConstants.NewCommentsQueue, newComment));
            }
            
            var addedComments = new List<string>();
            foreach (var added in e.Added)
            {
                addedComments.Add(added.Fullname);
                commentTasks.Add(_receivedCommentStore.Add(CommentStoreConstants.AddedQueue, added));
            }
            
            var removedComments = new List<string>();
            foreach (var removed in e.Removed)
            {
                removedComments.Add(removed.Fullname);
                commentTasks.Add(_receivedCommentStore.Add(CommentStoreConstants.RemovedQueue, removed));
            }

            await Task.WhenAll(commentTasks);
            

            using var scope = _services.CreateScope();
            var endpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();
            
            await endpoint.Publish<CommentsReceived>(new
            {
                Subreddit = _monitoredSubreddits[subredditName],
                OldComments = oldComments,
                NewComments = newComments,
                Added = addedComments,
                Removed = removedComments
            });
        }

        private async void CommentsOnNewUpdated(object? sender, CommentsUpdateEventArgs args)
        {
            try
            {
                await NewCommentsReceived(sender, args);
            }
            catch (RedditBadRequestException ex)
            {
                var subreddit = ((global::Reddit.Controllers.Comments) sender!).SubKey;
                _logger.LogError(ex, 
                    "Something went wrong with request for {Subreddit} listener!",
                    subreddit);

                throw;
            }
        }
        
        private void ReleaseUnmanagedResources()
        {
            foreach (var (_, subreddit) in _monitoredSubreddits)
            {
                subreddit.Comments.MonitorNew();
                subreddit.Comments.NewUpdated -= CommentsOnNewUpdated;
            }
        }

        ~SubredditNewCommentMonitor()
        {
            ReleaseUnmanagedResources();
        }

        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }
    }
}