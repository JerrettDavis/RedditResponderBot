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
using System.Reflection.Metadata;
using Bot.Service.Application.StringSearch.Models;

namespace Bot.Service.Application.StringSearch.Services
{
    public class TemplateProvider : ITemplateProvider
    {
        public IEnumerable<SearchTemplate> GetTemplates()
        {
            return new[]
            {
                new SearchTemplate
                {
                    TemplateName = "Ted Cruz",
                    Triggers = new[] {"Ted Cruz"},
                    Response =
                        "I do not like that man Ted Cruz,\n\nI do not like his far-right views.\n\nI do not like his stupid chin,\n\nI do not like his smarmy grin.\n\nI do not like him with a beard,\n\nI do not like him freshly sheared.\n\nI do not like Ted Cruz at all,\n\nThat man Ted Cruz can suck my balls.\n\n---\n^^Hi! ^^I'm ^^a ^^bot. ^^Feel ^^free ^^to ^^block ^^me ^^from ^^your ^^subreddit ^^if ^^you're ^^getting ^^spammed. ^^You ^^can ^^also ^^DM ^^my ^^creator ^^/u/jdsfighter ^^to ^^have ^^your ^^sub ^^added ^^to ^^the ^^blocklist."
                }
            };
        }
    }
}