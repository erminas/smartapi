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
using erminas.SmartAPI.Exceptions;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS.Project.ContentClasses.Elements.Attributes
{
    public class ElementReferenceAttribute : IRDAttribute
    {
        private readonly ContentClassElement _parent;

        public ElementReferenceAttribute(ContentClassElement parent)
        {
            _parent = parent;
            parent.RegisterAttribute(this);
        }

        public ContentClassElement Value
        {
            get
            {
                Guid projectGuid, ccGuid, elementGuid;
                XmlElement xmlNode = _parent.XmlElement;
                if (!xmlNode.TryGetGuid("eltprojectguid", out projectGuid) ||
                    !xmlNode.TryGetGuid("elttemplateguid", out ccGuid) ||
                    !xmlNode.TryGetGuid("eltelementguid", out elementGuid))
                {
                    return null;
                }

                string langId = xmlNode.GetAttributeValue("eltlanguagevariantid");

                Project project = _parent.ContentClass.Project.Session.Projects.GetByGuid(projectGuid);
                ContentClass contentClass = project.ContentClasses.GetByGuid(ccGuid);

                return contentClass.Elements[langId].GetByGuid(elementGuid);
            }
            set
            {
                XmlElement xmlNode = _parent.XmlElement;
                xmlNode.SetAttributeValue("eltlanguagevariantid", value.LanguageVariant.Language);
                xmlNode.SetAttributeValue("eltelementguid", value.Guid.ToRQLString());
                xmlNode.SetAttributeValue("elttemplateguid", value.ContentClass.Guid.ToRQLString());
                xmlNode.SetAttributeValue("eltprojectguid", value.ContentClass.Project.Guid.ToRQLString());
            }
        }

        #region IRDAttribute Members

        public void Assign(IRDAttribute o)
        {
            var other = (ElementReferenceAttribute) o;

            ContentClassElement otherContentClassElement = other.Value;
            try
            {
                Project project =
                    _parent.ContentClass.Project.Session.Projects[otherContentClassElement.ContentClass.Project.Name];
                ContentClass cc = project.ContentClasses.GetByName(otherContentClassElement.ContentClass.Name);
                Value =
                    cc.Elements[otherContentClassElement.LanguageVariant.Language].GetByName(
                        otherContentClassElement.Name);
            } catch (Exception e)
            {
                throw new SmartAPIException(_parent.ContentClass.Project.Session.ServerLogin, 
                    string.Format("Can't find project/content class/element {0}/{1}/{2} on server",
                                  otherContentClassElement.ContentClass.Project.Name,
                                  otherContentClassElement.ContentClass.Name, otherContentClassElement.Name), e);
            }
        }

        public string Description
        {
            get { return "Element"; }
        }

        public object DisplayObject
        {
            get
            {
                ContentClassElement contentClassElement = Value;
                if (contentClassElement == null)
                {
                    return "not set";
                }
                return string.Format("Element {0} of content class {1} in project {2}", contentClassElement.Name,
                                     contentClassElement.ContentClass.Name,
                                     contentClassElement.ContentClass.Project.Name);
            }
        }

        public bool IsAssignableFrom(IRDAttribute o, out string reason)
        {
            reason = string.Empty;
            return o is ElementReferenceAttribute;
        }

        public string Name
        {
            get { return "__elementreferenceattribute"; }
        }

        public void Refresh()
        {
        }

        #endregion
    }
}