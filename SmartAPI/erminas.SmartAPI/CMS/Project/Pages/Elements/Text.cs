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
using System.Security;
using System.Web;
using System.Xml;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS.Project.Pages.Elements
{
    public abstract class Text : PageElement, IValueElement<string>
    {
        private string _description;
        private string _value;

        protected Text(Project project, Guid guid, LanguageVariant languageVariant)
            : base(project, guid, languageVariant)
        {
        }

        protected Text(Project project, XmlElement xmlElement) : base(project, xmlElement)
        {
            LoadXml();
        }

        public void Commit()
        {
            string htmlEncodedValue = string.IsNullOrEmpty(_value)
                                          ? Session.SESSIONKEY_PLACEHOLDER
                                          : HttpUtility.HtmlEncode(_value);

            const string SAVE_VALUE =
                @"<ELT translationmode=""0"" extendedinfo="""" reddotcacheguid="""" action=""save"" guid=""{0}"" pageid=""{1}"" id="""" index="""" type=""{2}"">{3}</ELT>";
            Project.Select();
            Project.Session.ExecuteRql(SAVE_VALUE.RQLFormat(this, Page.Id, (int)Type, htmlEncodedValue), Session.IODataFormat.FormattedText);
        }

        public string Description
        {
            get { return LazyLoad(ref _description); }
        }

        public void SetValueFromString(string value)
        {
            Value = value;
        }

        public string Value
        {
            get
            {
                return LazyLoad(ref _value);
            }
            set { _value = value; }
        }

        protected override void LoadWholePageElement()
        {
            LoadXml();
            using (new LanguageContext(LanguageVariant))
            {
                const string LOAD_VALUE = @"<ELT action=""load"" guid=""{0}"" extendedinfo=""""/>";
                Project.Select();
                string result = Project.Session.ExecuteRql(LOAD_VALUE.RQLFormat(this),
                                                           Session.IODataFormat.FormattedText);
                _value = HttpUtility.HtmlDecode(result);
            }
        }

        private void LoadXml()
        {
            InitIfPresent(ref _description, "reddotdescription", x => x);
        }
    }
}