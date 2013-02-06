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

using System;
using erminas.SmartAPI.CMS.PageElements;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS
{
    internal class TargetContainerPreassignment
    {
        private readonly IContentClassElement _element;
        private Container _cachedTargetContainer;

        internal TargetContainerPreassignment(IContentClassElement element)
        {
            _element = element;
        }

        internal Container TargetContainer
        {
            get
            {
                Guid guid;
                if (!_element.XmlNode.TryGetGuid("elttargetcontainerguid", out guid))
                {
                    return null;
                }

                if (_cachedTargetContainer != null && _cachedTargetContainer.Guid == guid)
                {
                    return _cachedTargetContainer;
                }

                return
                    _cachedTargetContainer =
                    (Container) PageElement.CreateElement(_element.ContentClass.Project, guid, _element.LanguageVariant);
            }
            set
            {
                _element.XmlNode.SetAttributeValue("elttargetcontainerguid",
                                                   value == null ? null : value.Guid.ToRQLString());
            }
        }

        internal bool IsDisplayingConnectedPagesInTargetContainerOfMainLinkIfAvailable
        {
            get { return _element.XmlNode.GetBoolAttributeValue("usepagemainlinktargetcontainer").GetValueOrDefault(); }
            set { _element.XmlNode.SetAttributeValue("usepagemainlinktargetcontainer", value.ToRQLString()); }
        }
    }
}