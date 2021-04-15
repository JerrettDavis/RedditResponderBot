using System.Collections.Generic;

namespace Bot.Service.Common.Models.Messages
{
    // ReSharper disable once InconsistentNaming
    public interface CommentsReceived
    {
        string Subreddit { get; set; }
        IEnumerable<string> OldComments { get; set; }
        IEnumerable<string> NewComments { get; set; }
        IEnumerable<string> Added { get; set; }
        IEnumerable<string> Removed { get; set; }
    }
}