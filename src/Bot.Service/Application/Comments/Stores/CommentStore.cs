using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Reddit.Controllers;

namespace Bot.Service.Application.Comments.Stores
{
    // TODO: Make this implement its own IDictionary interface
    public class CommentStore : ICommentStore
    {
        private readonly ConcurrentDictionary<KeyValuePair<string, string>, Comment> _comments;

        public IReadOnlyDictionary<KeyValuePair<string, string>, Comment> Store => _comments;

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

        public virtual Task Remove(
            string key,
            string commentName,
            CancellationToken cancellationToken = default)
        {
            return Task.Run(() =>
            {
                _comments.TryRemove(new KeyValuePair<string, string>(key, commentName), out _);
            }, cancellationToken);
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