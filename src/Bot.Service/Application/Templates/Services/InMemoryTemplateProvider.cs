using System.Collections.Generic;
using System.Linq;
using Bot.Service.Application.StringSearch.Models;
using Bot.Service.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Bot.Service.Application.Templates.Services
{
    public class InMemoryTemplateProvider : ITemplateProvider
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<InMemoryTemplateProvider> _logger;
        private readonly IEnumerable<SearchTemplateBase> _templates;

        public InMemoryTemplateProvider(
            IConfiguration configuration,
            ILogger<InMemoryTemplateProvider> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _templates = InitializeTemplates();
        }

        private IEnumerable<SearchTemplateBase> InitializeTemplates()
        {
            _logger.LogInformation("Initializing Templates...");
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
                }).ToList();
            var randomResponseSearchTemplate = templates
                .Where(t => t.Responses != null && t.Responses.Any())
                .Select(t => new RandomResponseSearchTemplate 
                {
                    TemplateName = t.TemplateName,
                    Triggers = t.Triggers,
                    Responses = t.Responses!
                }).ToList();

            var transformed = new List<SearchTemplateBase>();
            
            transformed.AddRange(searchTemplates);
            transformed.AddRange(randomResponseSearchTemplate);
            
            _logger.LogInformation(
                "Templates loaded! Found {TemplateCount}, " +
                "{SearchTemplateCount} SearchTemplates, " +
                "{RandomResponseTemplateCount} RandomResponseSearchTemplate", 
                transformed.Count, 
                searchTemplates.Count, 
                randomResponseSearchTemplate.Count);

            return transformed;
        }

        public IEnumerable<SearchTemplateBase> GetTemplates()
        {
            return _templates;
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class TemplatePrototype
        {
            public string TemplateName { get; set; } = null!;
            public IEnumerable<string> Triggers { get; set; } = null!;
            public string Response { get; set; } = null!;
            // ReSharper disable once UnusedAutoPropertyAccessor.Local
            public IEnumerable<string>? Responses { get; set; }
        }
    }
}