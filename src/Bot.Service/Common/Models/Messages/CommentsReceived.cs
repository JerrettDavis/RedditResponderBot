using System.Collections.Generic;
using MassTransit;

namespace Bot.Service.Common.Models.Messages
{
    // ReSharper disable once InconsistentNaming
    /// <summary>
    /// The message used by <see cref="MassTransit"/> to alert consumers that
    /// new comments have been received.
    /// </summary>
    public interface CommentsReceived
    {
        string Subreddit { get; set; }
        IEnumerable<string> OldComments { get; set; }
        IEnumerable<string> NewComments { get; set; }
        IEnumerable<string> Added { get; set; }
        IEnumerable<string> Removed { get; set; }
    }
}