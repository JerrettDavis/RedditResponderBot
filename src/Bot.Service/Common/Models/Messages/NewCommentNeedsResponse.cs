using System.Collections.Generic;
using Bot.Service.Application.StringSearch.Models;
using Reddit.Controllers;

namespace Bot.Service.Common.Models.Messages
{
    // ReSharper disable once InconsistentNaming
    /// <summary>
    /// The message used by <see cref="MassTransit"/> to alert consumers that
    /// a comment has triggered one or more <see cref="SearchTemplateBase">Search Templates</see>
    /// and is in need of a response.
    /// </summary>
    public interface NewCommentNeedsResponse
    {
        string Comment { get; set; }
        public IEnumerable<SearchTemplate> SearchTemplates { get; set; }
        public IEnumerable<RandomResponseSearchTemplate> RandomResponseSearchTemplates { get; set; }
    }
}