using System;
using System.Linq;
using System.Threading.Tasks;
using Bot.Service.Application.Comments.Models;
using Bot.Service.Application.Comments.Stores;
using Bot.Service.Application.StringSearch.Models;
using Bot.Service.Common.Models.Messages;
using MassTransit;
using Microsoft.Extensions.Logging;
using Reddit.Controllers;
using Reddit.Exceptions;

namespace Bot.Service.Application.Consumers
{
    public class TriggeredCommentResponderConsumer : 
        IConsumer<NewCommentNeedsResponse>
    {
        private readonly ILogger<TriggeredCommentResponderConsumer> _logger;
        private readonly IProcessedCommentStore _processedCommentStore;

        public TriggeredCommentResponderConsumer(
            ILogger<TriggeredCommentResponderConsumer> logger, 
            IProcessedCommentStore processedCommentStore)
        {
            _logger = logger;
            _processedCommentStore = processedCommentStore;
        }

        public Task Consume(ConsumeContext<NewCommentNeedsResponse> context)
        {
            _logger.LogInformation("Received response request for {Comment}", context.Message.Comment);
            var tasks = context.Message.Templates.Select(template =>
                Task.Run(async () =>
                {
                    try
                    {
                        var comment = await _processedCommentStore.Get(
                            CommentStoreConstants.AddedQueue,
                            context.Message.Comment, 
                            context.CancellationToken);
                        _logger.LogInformation("Responding to {Author}", comment.Author);
                        await HandleReply(comment, template);
                    }
                    catch (RedditRateLimitException)
                    {
                        _logger.LogWarning("Looks like he we hit the rate limit. Requeue the comment...");
                        await context.Publish(context.Message, context.CancellationToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Something went wrong. Lets ignore this one...");
                    }
                },context.CancellationToken));

            return Task.WhenAll(tasks);
        }

        // TODO: This needs to be handled more gracefully
        private static async Task HandleReply(Comment comment, SearchTemplateBase template)
        {
            switch (template)
            {
                case SearchTemplate s:
                    await comment.ReplyAsync(s.Response);
                    break;
                case RandomResponseSearchTemplate r:
                    var responses = r.Responses.ToList();
                    var random = new Random();
                    var randomIndex = random.Next(responses.Count);
                    await comment.ReplyAsync(responses[randomIndex]);
                    break;
            }
        }
        
    }
}