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

using System;
using System.Collections.Generic;
using System.Linq;

namespace Bot.Service.Application.StringSearch.Models
{
    public class SearchTemplate
    {
        public SearchTemplate()
        {
            Triggers = new HashSet<string>();
        }

        public string TemplateName { get; init; } = null!;
        public IEnumerable<string> Triggers { get; init; }
        public string Response { get; init; } = null!;

        public bool Applies(string searchString, bool caseSensitive = true)
        {
            return Triggers.Any(t => searchString.Contains(t, caseSensitive ? 
                StringComparison.InvariantCulture : 
                StringComparison.InvariantCultureIgnoreCase));
        }
    }
}