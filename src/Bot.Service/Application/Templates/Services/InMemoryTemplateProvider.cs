// TedCruzResponderBot - Simple real-time chat application.
// Copyright (C) 2021  Jerrett D. Davis
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Linq;
using Bot.Service.Application.StringSearch.Models;
using Bot.Service.Application.StringSearch.Services;
using Bot.Service.Common;
using Bot.Service.Common.Models;
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

        public IEnumerable<SearchTemplate> GetTemplates()
        {
            var templates = new List<TemplatePrototype>();
            
            _configuration.GetSection(AppConstants.TemplateConfigurationSection)
                .Bind(templates);

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
        }
    }
}