using System;
using System.Threading;
using System.Threading.Tasks;

namespace Bot.Service.Application.Monitor.Services
{
    public interface ISubredditMonitor : IDisposable
    {
        Task Monitor(string subreddit, CancellationToken cancellationToken = default);
        Task StopMonitoring(string subreddit, CancellationToken cancellationToken = default);
    }
}