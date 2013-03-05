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

using System.Linq;
using System.Xml;
using erminas.SmartAPI.CMS.Project.ContentClasses.Elements.Attributes;
using erminas.SmartAPI.CMS.Project.Filesystem;

namespace erminas.SmartAPI.CMS.Project.ContentClasses.Elements
{
    public class Background : ContentClassElement
    {
        internal Background(ContentClass cc, XmlElement xmlElement) : base(cc, xmlElement)
        {
            CreateAttributes("eltignoreworkflow", "eltlanguageindependent", "elthideinform", "eltinvisibleinclient",
                             "eltdragdrop", "eltsrcsubdirguid", "eltsrc", "eltfolderguid");
        }

        public override ContentClassCategory Category
        {
            get { return ContentClassCategory.Content; }
        }

        public bool IsDragAndDropActivated
        {
            get { return GetAttributeValue<bool>("eltdragdrop"); }
            set { SetAttributeValue("eltdragdrop", value); }
        }

        public bool IsHiddenInProjectStructure
        {
            get { return GetAttributeValue<bool>("eltinvisibleinclient"); }
            set { SetAttributeValue("eltinvisibleinclient", value); }
        }

        public bool IsLanguageIndependent
        {
            get { return GetAttributeValue<bool>("eltlanguageindependent"); }
            set { SetAttributeValue("eltlanguageindependent", value); }
        }

        public bool IsNotRelevantForWorklow
        {
            get { return GetAttributeValue<bool>("eltignoreworkflow"); }
            set { SetAttributeValue("eltignoreworkflow", value); }
        }

        public bool IsNotUsedInForm
        {
            get { return GetAttributeValue<bool>("elthideinform"); }
            set { SetAttributeValue("elthideinform", value); }
        }

        public File SrcFile
        {
            get
            {
                var folderAttr = (FolderXmlNodeAttribute) GetAttribute("eltsrcsubdirguid");
                string srcName = ((StringXmlNodeAttribute) GetAttribute("eltsrc")).Value;
                if (folderAttr.Value == null || string.IsNullOrEmpty(srcName))
                {
                    return null;
                }
                return folderAttr.Value.GetFilesByNamePattern(srcName).First(x => x.Name == srcName);
            }

            set
            {
                ((StringXmlNodeAttribute) GetAttribute("eltsrc")).Value = value != null ? value.Name : "";
                if (value != null)
                {
                    ((FolderXmlNodeAttribute) GetAttribute("eltsrcsubdirguid")).Value = value.Folder;
                }
            }
        }
    }
}