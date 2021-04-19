using System.Collections.Generic;
using Bot.Service.Application.StringSearch.Models;

namespace Bot.Service.Application.StringSearch.Services
{
    public interface IStringSearcher
    {
        public IEnumerable<SearchTemplateBase> GetApplicableTemplates(string comment);
    }
}