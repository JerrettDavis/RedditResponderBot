using System.Collections.Generic;
using Bot.Service.Application.StringSearch.Models;

namespace Bot.Service.Application.Templates.Services
{
    public interface ITemplateProvider
    {
        /// <summary>
        /// Gets configured comment response templates
        /// </summary>
        /// <returns>An enumerable of all configured application</returns>
        IEnumerable<SearchTemplateBase> GetTemplates();
    }
}