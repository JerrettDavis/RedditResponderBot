using System.Collections.Generic;

namespace Bot.Service.Application.Reddit.Services
{
    public interface ISubredditProvider
    {
        IEnumerable<string> GetMonitoredSubs();
    }
}