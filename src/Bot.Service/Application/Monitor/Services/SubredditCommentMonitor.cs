using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bot.Service.Application.Comments.Models;
using Bot.Service.Application.Comments.Stores;
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
    public class SubredditCommentMonitor : ISubredditMonitor
    {
        private readonly ILogger<SubredditCommentMonitor> _logger;
        private readonly IReceivedCommentStore _receivedCommentStore;
        private readonly IServiceProvider _services;
            
        private readonly IDictionary<string, Subreddit> _monitoredSubreddits;
        private readonly RedditClient _reddit;

        public SubredditCommentMonitor(
            IRedditProvider redditProvider,
            ILogger<SubredditCommentMonitor> logger, 
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
                if (_monitoredSubreddits.ContainsKey(subredditName))
                    return;
                
                _logger.LogInformation("Processing Subreddit {Subreddit}", subredditName);
                var subreddit = _reddit.Subreddit(subredditName);
            
                _logger.LogInformation("Initializing {Subreddit}", subredditName);
                subreddit.Comments.GetNew(limit: 100);
                
                _logger.LogInformation("Starting monitoring for {Subreddit}", subredditName);
                subreddit.Comments.MonitorNew(limit: 100);
                
                _logger.LogInformation("Adding event handler for {Subreddit}", subredditName);
                subreddit.Comments.NewUpdated += CommentsOnNewUpdated;

                _monitoredSubreddits.TryAdd(subredditName, subreddit);
            }, cancellationToken);
        }

        public Task StopMonitoring(
            string subredditName, 
            CancellationToken cancellationToken = default)
        {
            return Task.Run(() =>
            {
                if (!_monitoredSubreddits.ContainsKey(subredditName))
                    return;
                
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
                "Received {Added} added, {Removed} removed, {NewComments} new, and {OldComments} old comments for {Subreddit}", 
                e.Added.Count, 
                e.Removed.Count,
                e.NewComments.Count,
                e.OldComments.Count,
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
                Subreddit = subredditName,
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

        ~SubredditCommentMonitor()
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