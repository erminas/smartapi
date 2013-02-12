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
using erminas.SmartAPI.CMS.Project.Pages.Elements;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS.Project.ContentClasses.Elements
{
    internal class TargetContainerPreassignment
    {
        private readonly IContentClassElement _element;
        private Pages.Elements.Container _cachedTargetContainer;

        internal TargetContainerPreassignment(IContentClassElement element)
        {
            _element = element;
        }

        internal bool IsDisplayingConnectedPagesInTargetContainerOfMainLinkIfAvailable
        {
            get { return _element.XmlElement.GetBoolAttributeValue("usepagemainlinktargetcontainer").GetValueOrDefault(); }
            set { _element.XmlElement.SetAttributeValue("usepagemainlinktargetcontainer", value.ToRQLString()); }
        }

        internal Pages.Elements.Container TargetContainer
        {
            get
            {
                Guid guid;
                if (!_element.XmlElement.TryGetGuid("elttargetcontainerguid", out guid))
                {
                    return null;
                }

                if (_cachedTargetContainer != null && _cachedTargetContainer.Guid == guid)
                {
                    return _cachedTargetContainer;
                }

                return
                    _cachedTargetContainer =
                    (Pages.Elements.Container) PageElement.CreateElement(_element.ContentClass.Project, guid, _element.LanguageVariant);
            }
            set
            {
                _element.XmlElement.SetAttributeValue("elttargetcontainerguid",
                                                      value == null ? null : value.Guid.ToRQLString());
            }
        }
    }
}