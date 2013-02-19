// Smart API - .Net programmatic access to RedDot servers
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
using erminas.SmartAPI.CMS.Project.Pages;

namespace erminas.SmartAPI.Exceptions
{
    [Serializable]
    public class MissingElementValueException : PageStatusException
    {
        public MissingElementValueException(Page page, IEnumerable<string> names) : base(page, BuildMessage(names))
        {
        }

        private static string BuildMessage(IEnumerable<string> names)
        {
            const string MESSAGE = "Missing values for the following mandatory elements: {0}";
            string missingElements = names.Aggregate("", (s, s1) => s + (s.Any() ? ", " : "") + s1);

            return string.Format(MESSAGE, missingElements);
        }
    }
}