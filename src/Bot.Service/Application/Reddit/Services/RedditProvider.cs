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