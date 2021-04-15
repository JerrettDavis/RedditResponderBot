using System.Collections.Generic;
using Bot.Service.Application.StringSearch.Models;
using Reddit.Controllers;

namespace Bot.Service.Common.Models.Messages
{
    // ReSharper disable once InconsistentNaming
    public interface NewCommentNeedsResponse
    {
        string Comment { get; set; }
        public IEnumerable<SearchTemplateBase> Templates { get; set; }
    }
}