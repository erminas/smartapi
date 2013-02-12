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
using erminas.SmartAPI.CMS.CCElements.Attributes;

namespace erminas.SmartAPI.CMS.CCElements
{
    public class SiteMap : ContentClassElement
    {
        internal SiteMap(ContentClass contentClass, XmlElement xmlElement) : base(contentClass, xmlElement)
        {
            CreateAttributes("eltxhtmlcompliant", "eltdepth", "eltsearchdepth", "elttableopen", "elttableclose",
                             "eltrowopen", "eltrowclose", "eltcolopen", "eltcolclose", "eltdropouts", "eltxslfile",
                             "eltfolderguid");
// ReSharper disable ObjectCreationAsStatement
            new EnumXmlNodeAttribute<SiteMapFormat>(this, "eltformat");
// ReSharper restore ObjectCreationAsStatement
        }

        public override ContentClassCategory Category
        {
            get { return ContentClassCategory.Structural; }
        }

        public string EndOfColumn
        {
            get { return GetAttributeValue<string>("eltcolclose"); }
            set
            {
                SetAttributeValue("eltcolclose", value);
            }
        }

        public string EndOfLine
        {
            get { return GetAttributeValue<string>("eltrowclose"); }
            set
            {
                SetAttributeValue("eltrowclose", value);
            }
        }

        public string EndOfTable
        {
            get { return GetAttributeValue<string>("elttableclose"); }
            set
            {
                SetAttributeValue("elttableclose", value);
            }
        }

        public SiteMapFormat Format
        {
            get { return ((EnumXmlNodeAttribute<SiteMapFormat>) GetAttribute("eltformat")).Value; }
            set { ((EnumXmlNodeAttribute<SiteMapFormat>) GetAttribute("eltformat")).Value = value; }
        }

        public bool IsSyntaxConformingToXHtml
        {
            get { return GetAttributeValue<bool>("eltxhtmlcompliant"); }
            set { SetAttributeValue("eltxhtmlcompliant", value); }
        }

        public int? MaxErrorCount
        {
            get
            {
                string tmp = ((StringXmlNodeAttribute) GetAttribute("eltdropouts")).Value;
                return string.IsNullOrEmpty(tmp) ? (int?) null : int.Parse(tmp);
            }
            set { ((StringXmlNodeAttribute) GetAttribute("eltdropouts")).Value = value.ToString(); }
        }

        public int? MaxSearchDepth
        {
            get
            {
                string tmp = ((StringXmlNodeAttribute) GetAttribute("eltsearchdepth")).Value;
                return string.IsNullOrEmpty(tmp) ? (int?) null : int.Parse(tmp);
            }
            set { ((StringXmlNodeAttribute) GetAttribute("eltsearchdepth")).Value = value.ToString(); }
        }

        public int? NestingLevel
        {
            get
            {
                string tmp = ((StringXmlNodeAttribute) GetAttribute("eltdepth")).Value;
                return string.IsNullOrEmpty(tmp) ? (int?) null : int.Parse(tmp);
            }
            set { ((StringXmlNodeAttribute) GetAttribute("eltdepth")).Value = value.ToString(); }
        }

        public string StartOfColumn
        {
            get { return GetAttributeValue<string>("eltcolopen"); }
            set
            {
                SetAttributeValue("eltcolopen", value);
            }
        }

        public string StartOfLine
        {
            get { return GetAttributeValue<string>("eltrowopen"); }
            set
            {
                SetAttributeValue("eltrowopen", value);
            }
        }

        public string StartOfTable
        {
            get { return GetAttributeValue<string>("elttableopen"); }
            set
            {
                SetAttributeValue("elttableopen", value);
            }
        }

        public File XslStyleSheet
        {
            get
            {
                var folderAttr = (FolderXmlNodeAttribute) GetAttribute("eltfolderguid");
                string srcName = ((StringXmlNodeAttribute) GetAttribute("eltxslfile")).Value;
                if (folderAttr.Value == null || string.IsNullOrEmpty(srcName))
                {
                    return null;
                }
                return folderAttr.Value.GetFilesByNamePattern(srcName).First(x => x.Name == srcName);
            }

            set
            {
                ((StringXmlNodeAttribute) GetAttribute("eltxslfile")).Value = value.Name;
                ((FolderXmlNodeAttribute) GetAttribute("eltfolderguid")).Value = value.Folder;
            }
        }
    }

    public enum SiteMapFormat
    {
        HTMLCode = 0,
        XMLStructure = 1,
    }
}