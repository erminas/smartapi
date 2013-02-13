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
using System.Xml;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS.Project
{
    public class ProjectVariant : PartialRedDotProjectObject
    {
        public ProjectVariant(Project project, Guid guid) : base(project, guid)
        {
        }

        internal ProjectVariant(Project project, XmlElement xmlElement) : base(project, xmlElement)
        {
        }

        public bool IsUsedAsDisplayFormat
        {
            get
            {
                string value = XmlElement.GetAttributeValue("checked");
                return value != null && value == "1";
            }
        }

        public bool IsUserDisplayVariant
        {
            get
            {
                string value = XmlElement.GetAttributeValue("userdisplayvariant");
                return value == "1";
            }
        }
        
        protected override void LoadWholeObject()
        {
        }

        protected override XmlElement RetrieveWholeObject()
        {
            const string LOAD_PROJECT_VARIANT =
                @"<PROJECT><PROJECTVARIANTS action=""load""><PROJECTVARIANT guid=""{0}"" /></PROJECTVARIANTS></PROJECT>";

            XmlDocument xmlDoc = Project.ExecuteRQL(string.Format(LOAD_PROJECT_VARIANT, Guid.ToRQLString()));
            return (XmlElement) xmlDoc.GetElementsByTagName("PROJECTVARIANT")[0];
        }
    }
}