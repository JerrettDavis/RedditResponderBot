using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
        private readonly IRedditProvider _redditProvider;
        private readonly IStringSearcher _searcher;
        private readonly ISubredditProvider _subredditProvider;

        private readonly IDictionary<string, Comment> _comments;
        private readonly ConcurrentQueue<EnqueuedComment> _queue;

        private readonly ConcurrentBag<Subreddit> _monitoredSubreddits;

        private string? _me;

        public Worker(
            ILogger<Worker> logger,
            IRedditProvider redditProvider,
            IStringSearcher searcher,
            ISubredditProvider subredditProvider)
        {
            _logger = logger;
            _redditProvider = redditProvider;
            _searcher = searcher;
            _subredditProvider = subredditProvider;

            _comments = new ConcurrentDictionary<string, Comment>();
            _queue = new ConcurrentQueue<EnqueuedComment>();
            _monitoredSubreddits = new ConcurrentBag<Subreddit>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting bot");

            var reddit = _redditProvider.GetClient();
            var subredditNames = _subredditProvider.GetMonitoredSubs().ToList();

            _me = reddit.Account.Me.Name;

            var subs = new ConcurrentBag<Subreddit>();

            // Initialize all the subreddits first
            Parallel.ForEach(
                subredditNames, 
                new ParallelOptions {CancellationToken = stoppingToken},
                subredditName =>
                {
                    _logger.LogInformation("Processing Subreddit {Subreddit}", subredditName);
                    var subreddit = reddit.Subreddit(subredditName);

                    try
                    {
                        _logger.LogInformation("Initializing {Subreddit}", subredditName);
                        subreddit.Comments.GetNew(limit: 100);
                        
                        _logger.LogInformation("Starting monitoring for {Subreddit}", subredditName);
                        subreddit.Comments.MonitorNew(limit: 100);
                        
                        _logger.LogInformation("Adding event handler for {Subreddit}", subredditName);
                        subreddit.Comments.NewUpdated += C_NewCommentsUpdated;
                    }
                    catch (RedditBadRequestException ex)
                    {
                        _logger.LogError(ex, "Something went wrong with request for {Subreddit} listener!",
                            subreddit.Name);
                    }

                    _monitoredSubreddits.Add(subreddit);
                });

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {Time}", DateTimeOffset.Now);

                while (!_queue.IsEmpty)
                {
                    _logger.LogInformation("Trying to dequeue comment");
                    if (!_queue.TryDequeue(out var dequeued))
                        continue;

                    _logger.LogInformation("Comment successfully dequeued");
                    var tasks = dequeued.Templates.Select(template =>
                            Task.Run(async () =>
                            {
                                _logger.LogInformation("Replying to post with matching template");
                                try
                                {
                                    await dequeued.Comment.ReplyAsync(template.Response);
                                }
                                catch (RedditRateLimitException)
                                {
                                    _logger.LogWarning("Looks like he we hit the rate limit. Requeue the comment...");
                                    _queue.Enqueue(dequeued);
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError("Something went wrong: {Exception}. Lets ignore this one...", ex);
                                }
                            }, stoppingToken))
                        .ToList();

                    await Task.WhenAll(tasks);
                }

                await Task.Delay(1000, stoppingToken);
            }

            foreach (var subreddit in _monitoredSubreddits)
            {
                subreddit.Comments.MonitorNew();
                subreddit.Comments.NewUpdated -= C_NewCommentsUpdated;
            }
        }

        private void C_NewCommentsUpdated(object sender, CommentsUpdateEventArgs e)
        {
            _logger.LogInformation("Received {Count} new comments", e.Added.Count);

            var filtered = e.Added
                .Where(comment => !_comments.ContainsKey(comment.Fullname) &&
                                  comment.Author != _me)
                .Select(p => new EnqueuedComment(p, _searcher.GetApplicableTemplates(p.Body)))
                .Where(p => p.Templates.Any());

            foreach (var comment in filtered)
            {
                _queue.Enqueue(comment);
                _comments.Add(comment.Comment.Fullname, comment.Comment);
                _logger.LogInformation("New Comment | Author {Author}, Comment {Comment}", comment.Comment.Author,
                    comment.Comment.Body);
            }
        }

        private class EnqueuedComment
        {
            public EnqueuedComment(Comment comment, IEnumerable<SearchTemplate> templates)
            {
                Comment = comment;
                Templates = templates;
            }

            public Comment Comment { get; }
            public IEnumerable<SearchTemplate> Templates { get; }
        }
    }
}