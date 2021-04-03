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
using System.Linq;
using System.Threading.Tasks;
using Bot.Service.Application.Comments.Models;
using Bot.Service.Application.Comments.Services;
using Bot.Service.Common.Models.Messages;
using MassTransit;
using Microsoft.Extensions.Logging;
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
                        await comment.ReplyAsync(template.Response);
                    }
                    catch (RedditRateLimitException)
                    {
                        _logger.LogWarning("Looks like he we hit the rate limit. Requeue the comment...");
                        await context.Publish(context.Message, context.CancellationToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("Something went wrong: {Exception}. Lets ignore this one...", ex);
                    }
                },context.CancellationToken));

            return Task.WhenAll(tasks);
        }
    }
}