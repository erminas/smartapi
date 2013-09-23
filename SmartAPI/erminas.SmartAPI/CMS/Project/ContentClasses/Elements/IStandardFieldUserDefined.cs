// SmartAPI - .Net programmatic access to RedDot servers
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
using System.Text.RegularExpressions;
using System.Xml;

namespace erminas.SmartAPI.CMS.Project.ContentClasses.Elements
{
    public interface IStandardFieldUserDefined : IStandardFieldNonDate
    {
        Regex RegularExpression { get; set; }
    }

    internal class StandardFieldUserDefined : StandardFieldNonDate, IStandardFieldUserDefined
    {
        internal StandardFieldUserDefined(IContentClass contentClass, XmlElement xmlElement)
            : base(contentClass, xmlElement)
        {
        }

        public Regex RegularExpression
        {
            get { return new Regex(RegexAttribute ?? ""); }
            set
            {
                string str = value.ToString();
                if (string.IsNullOrEmpty(str))
                {
                    throw new ArgumentException("Empty pattern not allowed for user defined standard fields.");
                }
                RegexAttribute = str;
            }
        }

        [RedDot("eltverifytermregexp")]
        private string RegexAttribute
        {
            get { return GetAttributeValue<string>(); }
            set { SetAttributeValue(value); }
        }
    }
}