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


        public TriggeredCommentResponderConsumer(
            ILogger<TriggeredCommentResponderConsumer> logger)
        {
            _logger = logger;
        }

        public Task Consume(ConsumeContext<NewCommentNeedsResponse> context)
        {
            _logger.LogInformation("Responding to {Author}", context.Message.Comment.Author);
            var tasks = context.Message.Templates.Select(template =>
                Task.Run(async () =>
                {
                    try
                    {
                        await context.Message.Comment.ReplyAsync(template.Response);
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