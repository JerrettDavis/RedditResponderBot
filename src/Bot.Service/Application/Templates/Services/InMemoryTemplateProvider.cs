using System.Collections.Generic;
using System.Linq;
using Bot.Service.Application.StringSearch.Models;
using Bot.Service.Common;
using Microsoft.Extensions.Configuration;

namespace Bot.Service.Application.Templates.Services
{
    public class InMemoryTemplateProvider : ITemplateProvider
    {
        private readonly IConfiguration _configuration;

        public InMemoryTemplateProvider(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IEnumerable<SearchTemplateBase> GetTemplates()
        {
            var templates = new List<TemplatePrototype>();
            
            _configuration.GetSection(AppConstants.TemplateConfigurationSection)
                .Bind(templates);

            var searchTemplates = templates
                .Where(t => !string.IsNullOrWhiteSpace(t.Response) &&
                            (t.Responses == null ||
                            !t.Responses.Any()))
                .Select(t => new SearchTemplate
                {
                    TemplateName = t.TemplateName,
                    Triggers = t.Triggers,
                    Response = t.Response
                });
            var randomResponseSearchTemplate = templates
                .Where(t => t.Responses != null && t.Responses.Any())
                .Select(t => new RandomResponseSearchTemplate 
                {
                    TemplateName = t.TemplateName,
                    Triggers = t.Triggers,
                    Responses = t.Responses!
                });

            var transformed = new List<SearchTemplateBase>();
            
            transformed.AddRange(searchTemplates);
            transformed.AddRange(randomResponseSearchTemplate);

            return templates.Select(t => new SearchTemplate
            {
                TemplateName = t.TemplateName,
                Triggers = t.Triggers,
                Response = t.Response
            });
        }

        private class TemplatePrototype
        {
            public string TemplateName { get; set; } = null!;
            public IEnumerable<string> Triggers { get; set; } = null!;
            public string Response { get; set; } = null!;
            public IEnumerable<string>? Responses { get; set; }
        }
    }
}