using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Reddit.Controllers;

namespace Bot.Service.Application.Comments.Stores
{
    public interface ICommentStore
    {
        Task Add(string key, Comment comment, CancellationToken cancellationToken = default);
        Task<bool> Contains(string key, Comment comment, CancellationToken cancellationToken = default);

        Task<Comment> Get(
            string key,
            string commentName,
            CancellationToken cancellationToken = default);

        Task Remove(
            string key,
            string commentName,
            CancellationToken cancellationToken = default);

        IReadOnlyDictionary<KeyValuePair<string, string>, Comment> Store { get; }
    }
}