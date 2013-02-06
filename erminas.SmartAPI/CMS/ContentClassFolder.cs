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

using System.Collections.Generic;
using System.Linq;
using System.Xml;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS
{
    /// <summary>
    ///     A folder containing content classes.
    /// </summary>
    public class ContentClassFolder : RedDotObject
    {
        private readonly Project _project;

        public ContentClassFolder(Project project, XmlElement xmlElement) : base(xmlElement)
        {
            ContentClasses = new NameIndexedRDList<ContentClass>(GetContentClasses, Caching.Enabled);
            _project = project;
        }

        /// <summary>
        ///     All content classes contained in this folder, indexed by name. The list is cached by default.
        /// </summary>
        public NameIndexedRDList<ContentClass> ContentClasses { get; private set; }

        private List<ContentClass> GetContentClasses()
        {
            // RQL for listing all content classes of a folder. 
            // One Parameter: Folder Guid: {0}
            const string LIST_CC_OF_FOLDER = @"<TEMPLATES folderguid=""{0}"" action=""list""/>";

            XMLDoc = _project.ExecuteRQL(string.Format(LIST_CC_OF_FOLDER, Guid.ToRQLString()));

            return
                (from XmlElement curNode in XMLDoc.GetElementsByTagName("TEMPLATE")
                 select new ContentClass(_project, curNode)).ToList();
        }
    }
}