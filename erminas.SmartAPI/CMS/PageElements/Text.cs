/*
 * Smart API - .Net programatical access to RedDot servers
 * Copyright (C) 2012  erminas GbR 
 *
 * This program is free software: you can redistribute it and/or modify it 
 * under the terms of the GNU General Public License as published by the Free Software Foundation,
 * either version 3 of the License, or (at your option) any later version.
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the GNU General Public License for more details. 
 *
 * You should have received a copy of the GNU General Public License along with this program.
 * If not, see <http://www.gnu.org/licenses/>. 
 */

using System;
using System.Web;
using System.Xml;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS.PageElements
{
    public abstract class Text : AbstractValueElement<String>
    {
        private string _description;

        protected Text(Project project, Guid guid)
            : base(project, guid)
        {
        }

        protected Text(Project project, XmlElement xmlElement)
            : base(project, xmlElement)
        {
            LoadXml();
        }

        public string Description
        {
            get { return LazyLoad(ref _description); }
        }

        protected override string FromString(string value)
        {
            return value;
        }

        private void LoadXml()
        {
            InitIfPresent(ref _description, "reddotdescription", x => x);
        }

        public override void Commit()
        {
            string xmlNodeValue = GetXmlNodeValue();
            string htmlEncode = string.IsNullOrEmpty(xmlNodeValue) ? Session.SESSIONKEY_PLACEHOLDER : HttpUtility.UrlEncode(xmlNodeValue);
            XmlDocument xmlDoc =
                Project.ExecuteRQL(string.Format(SAVE_VALUE, Guid.ToRQLString(), htmlEncode, (int)Type));
            if (xmlDoc.GetElementsByTagName("ELT").Count != 1 && !xmlDoc.InnerXml.Contains(Guid.ToRQLString()))
            {
                throw new Exception(String.Format("Could not save element {0}", Guid.ToRQLString()));
            }
        }

        protected sealed override void LoadWholeValueElement()
        {
            LoadXml();
            const string LOAD_VALUE = @"<ELT action=""load"" guid=""{0}"" extendedinfo=""""/>";
            string result = Project.Session.CmsClient.ExecuteRql(LOAD_VALUE.RQLFormat(this), CmsClient.IODataFormat.FormattedText);
            _value = HttpUtility.UrlDecode(result);
        }

        protected sealed override string FromXmlNodeValue(string arg)
        {
            return null;
        }
    }
}