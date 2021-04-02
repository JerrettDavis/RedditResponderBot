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

namespace Bot.Service.Application.StringSearch.Services
{
    public class StringSearcher : IStringSearcher
    {
        private readonly ITemplateProvider _templateProvider;


        public StringSearcher(ITemplateProvider templateProvider)
        {
            _templateProvider = templateProvider;
        }

        public IEnumerable<SearchTemplate> GetApplicableTemplates(string comment)
        {
            return _templateProvider.GetTemplates()
                .Where(t => t.Applies(comment, false));
        }
    }
}