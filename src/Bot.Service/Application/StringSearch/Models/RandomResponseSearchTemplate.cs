using System.Collections.Generic;

namespace Bot.Service.Application.StringSearch.Models
{
    public class RandomResponseSearchTemplate : SearchTemplateBase
    {
        public RandomResponseSearchTemplate()
        {
            Responses = new HashSet<string>();
        }

        public IEnumerable<string> Responses { get; init; }
    }
}