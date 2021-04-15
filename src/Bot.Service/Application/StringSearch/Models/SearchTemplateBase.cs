using System;
using System.Collections.Generic;
using System.Linq;

namespace Bot.Service.Application.StringSearch.Models
{
    public class SearchTemplateBase
    {
        public SearchTemplateBase()
        {
            Triggers = new HashSet<string>();
        }

        public string TemplateName { get; init; } = null!;
        public IEnumerable<string> Triggers { get; init; }

        public bool Applies(string searchString, bool caseSensitive = true)
        {
            return Triggers.Any(t => searchString.Contains(t, caseSensitive ? 
                StringComparison.InvariantCulture : 
                StringComparison.InvariantCultureIgnoreCase));
        }
    }
}