using Reddit;

namespace Bot.Service.Application.Reddit.Services
{
    public interface IRedditProvider
    {
        RedditClient GetClient();
    }
}