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
using System.Xml;
using erminas.SmartAPI.Exceptions;
using erminas.SmartAPI.Utils;
using erminas.SmartAPI.Utils.CachedCollections;

namespace erminas.SmartAPI.CMS.Project.ContentClasses
{
    public interface IContentClassEditableAreaSettings : IProjectObject, IXmlBasedObject, ICached
    {
        [RedDot("bordercolor")]
        string BorderColor { get; set; }

        [RedDot("borderstyle")]
        string BorderStyle { get; set; }

        [RedDot("borderwidth")]
        string BorderWidth { get; set; }

        void Commit();

        [RedDot("usedefaultrangesettings")]
        bool IsUsingBorderDefinitionFromProjectSetting { get; set; }

        [RedDot("showpagerange")]
        bool IsUsingBordersToHighlightPages { get; set; }
    }

    /// <summary>
    ///     Represents editable area configuration of a content class.
    /// </summary>
    internal class CCEditableAreaSettings : AbstractAttributeContainer, IContentClassEditableAreaSettings
    {
        private readonly IContentClass _parent;

        internal CCEditableAreaSettings(IContentClass parent) : base(parent.Session)
        {
            _parent = parent;
        }

        public string BorderColor
        {
            get { return GetAttributeValue<string>(); }
            set { SetAttributeValue(value); }
        }

        public string BorderStyle
        {
            get { return GetAttributeValue<string>(); }
            set { SetAttributeValue(value); }
        }

        public string BorderWidth
        {
            get { return GetAttributeValue<string>(); }
            set { SetAttributeValue(value); }
        }

        public void Commit()
        {
            const string SAVE_CC_SETTINGS = @"<TEMPLATE guid=""{0}"">{1}</TEMPLATE>";
            var query = SAVE_CC_SETTINGS.RQLFormat(_parent, RedDotObject.GetSaveString((XmlElement) XmlElement.Clone()));

            var result = _parent.Project.ExecuteRQL(query, RqlType.SessionKeyInProject);

            if (result.GetElementsByTagName("SETTINGS").Count != 1)
            {
                throw new SmartAPIException(Session.ServerLogin,
                                            String.Format("Could not save settings for content class {0}", _parent));
            }
        }

        public void InvalidateCache()
        {
            XmlElement = null;
        }

        public bool IsUsingBorderDefinitionFromProjectSetting
        {
            get { return GetAttributeValue<bool>(); }
            set { SetAttributeValue(value); }
        }

        public bool IsUsingBordersToHighlightPages
        {
            get { return GetAttributeValue<bool>(); }
            set { SetAttributeValue(value); }
        }

        public IProject Project
        {
            get { return _parent.Project; }
        }

        public void Refresh()
        {
            InvalidateCache();
            XmlElement = RetrieveWholeObject();
        }

        public override XmlElement XmlElement
        {
            get { return base.XmlElement ?? (XmlElement = RetrieveWholeObject()); }
            protected internal set { base.XmlElement = value; }
        }

        private XmlElement RetrieveWholeObject()
        {
            const string LOAD_CC_SETTINGS = @"<TEMPLATE guid=""{0}""><SETTINGS action=""load""/></TEMPLATE>";
            var xmlDoc = _parent.Project.ExecuteRQL(LOAD_CC_SETTINGS.RQLFormat(_parent));
            var node = xmlDoc.GetSingleElement("SETTINGS");
            if (node == null)
            {
                throw new SmartAPIException(_parent.Session.ServerLogin,
                                            String.Format("Could not load settings for content class {0}", _parent));
            }

            return node;
        }
    }
}