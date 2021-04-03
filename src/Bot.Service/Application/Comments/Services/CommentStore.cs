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

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Reddit.Controllers;

namespace Bot.Service.Application.Comments.Services
{
    public class CommentStore : ICommentStore
    {
        private readonly IDictionary<KeyValuePair<string, string>, Comment> _comments;

        public CommentStore()
        {
            _comments = new ConcurrentDictionary<KeyValuePair<string, string>, Comment>();
        }
        
        public virtual Task Add(
            string key, 
            Comment comment, 
            CancellationToken cancellationToken = default)
        {
            return Task.Run(() =>
            {
                _comments.TryAdd(
                    new KeyValuePair<string, string>(key, comment.Fullname), 
                    comment);
            }, cancellationToken);
        }

        public virtual Task<Comment> Get(
            string key,
            string commentName,
            CancellationToken cancellationToken = default)
        {
            return Task.Run(() => _comments[new KeyValuePair<string, string>(key, commentName)], cancellationToken);
        } 

        public virtual Task<bool> Contains(
            string key, 
            Comment comment, 
            CancellationToken cancellationToken = default)
        {
            return Task.Run(() => _comments.ContainsKey(
                    new KeyValuePair<string, string>(key, comment.Fullname)), 
                cancellationToken);
        }
    }
}