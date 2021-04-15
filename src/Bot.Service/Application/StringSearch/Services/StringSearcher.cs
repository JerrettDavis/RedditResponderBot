using System.Collections.Generic;
using System.Linq;
using Bot.Service.Application.StringSearch.Models;
using Bot.Service.Application.Templates.Services;

namespace Bot.Service.Application.StringSearch.Services
{
    public class StringSearcher : IStringSearcher
    {
        private readonly ITemplateProvider _templateProvider;


        public StringSearcher(ITemplateProvider templateProvider)
        {
            _templateProvider = templateProvider;
        }

        public IEnumerable<SearchTemplateBase> GetApplicableTemplates(string comment)
        {
            return _templateProvider.GetTemplates()
                .Where(t => t.Applies(comment, false));
        }
    }
}