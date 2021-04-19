using System;
using Bot.Service.Common.Models;
using Microsoft.Extensions.Logging;
using Reddit;

namespace Bot.Service.Application.Reddit.Services
{
    public class RedditProvider : IRedditProvider
    {
        private readonly RedditClient _client;

        public RedditProvider(AppSettings appSettings, ILogger<RedditProvider> logger)
        {
            if (string.IsNullOrWhiteSpace(appSettings.AppId) || 
                string.IsNullOrWhiteSpace(appSettings.RefreshToken))
                throw new ArgumentException("You must supply an AppId and RefreshToken");
            
            logger.LogInformation("Getting Reddit client for AppId: {AppId} and Refresh Token: {RefreshToken}",
                appSettings.AppId, appSettings.RefreshToken);
            
            _client = new RedditClient(appSettings.AppId, appSettings.RefreshToken);
        }

        public RedditClient GetClient()
        {
            return _client;
        }
    }
}