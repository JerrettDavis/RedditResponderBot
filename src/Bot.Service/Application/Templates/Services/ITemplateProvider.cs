using System.Collections.Generic;
using Bot.Service.Application.StringSearch.Models;

namespace Bot.Service.Application.Templates.Services
{
    public interface ITemplateProvider
    {
        IEnumerable<SearchTemplateBase> GetTemplates();
    }
}