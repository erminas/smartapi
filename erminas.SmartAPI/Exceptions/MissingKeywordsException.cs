// Smart API - .Net programatical access to RedDot servers
//  
// Copyright (C) 2013 erminas GbR
// 
// This program is free software: you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License along with this program.
// If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using erminas.SmartAPI.CMS;

namespace erminas.SmartAPI.Exceptions
{
    [Serializable]
    public class MissingKeywordsException : PageStatusException
    {
        public MissingKeywordsException(Page page, IEnumerable<string> keywordMessages)
            : base(page, BuildMessage(keywordMessages))
        {
        }

        private static string BuildMessage(IEnumerable<string> keywordMessages)
        {
            return keywordMessages.Aggregate("", (s, s1) => (s.Any() ? s + "\n" : s) + s1);
        }
    }
}