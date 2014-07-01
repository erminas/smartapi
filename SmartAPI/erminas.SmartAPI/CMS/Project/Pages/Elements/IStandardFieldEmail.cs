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
using erminas.SmartAPI.CMS.Project.ContentClasses.Elements;

namespace erminas.SmartAPI.CMS.Project.Pages.Elements
{
    public interface IStandardFieldEmail : IStandardField<string>
    {
    }

    [PageElementType(ElementType.StandardFieldEmail)]
    internal class StandardFieldEmail : StandardField<string>, IStandardFieldEmail
    {
        private static Regex _emailVerificationRegex = new Regex("^[^@]+@[^@]+$");

        internal StandardFieldEmail(IProject project, XmlElement xmlElement) : base(project, xmlElement)
        {
            LoadXml();
        }

        public StandardFieldEmail(IProject project, Guid guid, ILanguageVariant languageVariant)
            : base(project, guid, languageVariant)
        {
        }

        protected override string FromString(string value)
        {
            if (!_emailVerificationRegex.IsMatch(value))
            {
                throw new ArgumentException(string.Format("Invalid email adress: {0}", value));
            }
            return value;
        }

        protected override void LoadWholeStandardField()
        {
            LoadXml();
        }

        private void LoadXml()
        {
          // -> is in javascript regex format  EnsuredInit(ref _emailVerificationRegex, "eltverifytermregexp", x => new Regex(x));
        }
    }
}