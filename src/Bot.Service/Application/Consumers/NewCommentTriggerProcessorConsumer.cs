using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bot.Service.Application.Comments.Models;
using Bot.Service.Application.Comments.Stores;
using Bot.Service.Application.Reddit.Services;
using Bot.Service.Application.StringSearch.Models;
using Bot.Service.Application.StringSearch.Services;
using Bot.Service.Common.Extensions;
using Bot.Service.Common.Models.Messages;
using MassTransit;
using Microsoft.Extensions.Logging;
using Reddit.Controllers;

namespace Bot.Service.Application.Consumers
{
    public class NewCommentTriggerProcessorConsumer : IConsumer<CommentsReceived>
    {
        private const string QueueKey = "ProcessedAddedComments";
        private readonly IProcessedCommentStore _processedCommentStore;
        private readonly IReceivedCommentStore _receivedCommentStore;
        private readonly IProcessedCommentStore _comments;
        private readonly IStringSearcher _searcher;
        private readonly ILogger<NewCommentTriggerProcessorConsumer> _logger;

        private readonly string _me;

        public NewCommentTriggerProcessorConsumer(
            IProcessedCommentStore comments,
            IRedditProvider reddit, 
            IStringSearcher searcher, 
            ILogger<NewCommentTriggerProcessorConsumer> logger, 
            IReceivedCommentStore receivedCommentStore, 
            IProcessedCommentStore processedCommentStore)
        {
            _comments = comments;
            _searcher = searcher;
            _logger = logger;
            _receivedCommentStore = receivedCommentStore;
            _processedCommentStore = processedCommentStore;

            _me = reddit.GetClient().Account.Me.Name;
        }

        public async Task Consume(ConsumeContext<CommentsReceived> context)
        {
            _logger.LogInformation(
                "Handling {Count} new comments from {Subreddit}", 
                context.Message.Added.Count(), 
                context.Message.Subreddit);

            var addedTasks = context.Message.Added.Select(async a =>
                await _receivedCommentStore.Get(
                    CommentStoreConstants.AddedQueue,
                    a,
                    context.CancellationToken))
                .ToList();
            await Task.WhenAll(addedTasks);

            var added = addedTasks.Select(a => a.Result);
            var filtered = await GetFilteredComments(
                added, 
                context.CancellationToken);
            var triggered = (await GetTriggeredComments(
                filtered,
                context.CancellationToken))
                .ToList();
            
            _logger.LogInformation("Found {Count} applicable comments", triggered.Count);

            await triggered.ForEachAsync(async c =>
            {
                await _processedCommentStore.Add(CommentStoreConstants.AddedQueue, c.Comment, context.CancellationToken);
                await context.Publish<NewCommentNeedsResponse>(new
                {
                    Comment = c.Comment.Fullname, 
                    SearchTemplates = c.Templates
                        .Where(t => t is not RandomResponseSearchTemplate and SearchTemplate) // Lol probably need to iron this out soon
                        .Cast<SearchTemplate>(),
                    RandomResponseSearchTemplates = c.Templates
                        .Where(t => t is RandomResponseSearchTemplate)
                        .Cast<RandomResponseSearchTemplate>()
                });
            }, cancellationToken: context.CancellationToken);
        }

        private async Task<IEnumerable<Comment>> GetFilteredComments(
            IEnumerable<Comment> comments, 
            CancellationToken cancellationToken)
        {
            var filterTask = comments.Select(async f =>
                new
                {
                    Comment = f,
                    AlreadyProcessed = await _comments.Contains(QueueKey, f, cancellationToken)
                })
                .ToList();
            await Task.WhenAll(filterTask);

            return filterTask.Select(f => f.Result)
                .Where(f => !f.AlreadyProcessed &&
                            f.Comment.Author != _me)
                .Select(f => f.Comment);
        }

        private Task<IEnumerable<TriggeredComment>> GetTriggeredComments(
            IEnumerable<Comment> comments,
            CancellationToken cancellationToken)
        {
            return Task.Run(() => comments.Select(c => 
                    new TriggeredComment(c, _searcher.GetApplicableTemplates(c.Body)))
                .Where(p => p.Templates.Any()), cancellationToken);
        }
        
        private class TriggeredComment
        {
            public TriggeredComment(
                Comment comment, 
                IEnumerable<SearchTemplateBase> templates)
            {
                Comment = comment;
                Templates = templates;
            }

            public Comment Comment { get; }
            public IEnumerable<SearchTemplateBase> Templates { get; }
        }
    }
}