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
using System.Xml;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS.CCElements.Attributes
{
    public class ElementReferenceAttribute : IRDAttribute
    {
        private readonly CCElement _parent;

        public ElementReferenceAttribute(CCElement parent)
        {
            _parent = parent;
            parent.RegisterAttribute(this);
        }

        public CCElement Value
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

        public string Name
        {
            get { return "__elementreferenceattribute"; }
        }

        public object DisplayObject
        {
            get
            {
                CCElement ccElement = Value;
                if (ccElement == null)
                {
                    return "not set";
                }
                return string.Format("Element {0} of content class {1} in project {2}", ccElement.Name,
                                     ccElement.ContentClass.Name, ccElement.ContentClass.Project.Name);
            }
        }

        public string Description
        {
            get { return "Element"; }
        }

        public bool IsAssignableFrom(IRDAttribute o, out string reason)
        {
            reason = string.Empty;
            return o is ElementReferenceAttribute;
        }

        public void Assign(IRDAttribute o)
        {
            var other = (ElementReferenceAttribute) o;

            CCElement otherCCElement = other.Value;
            try
            {
                Project project =
                    _parent.ContentClass.Project.Session.Projects[otherCCElement.ContentClass.Project.Name];
                ContentClass cc = project.ContentClasses.GetByName(otherCCElement.ContentClass.Name);
                Value = cc.Elements[otherCCElement.LanguageVariant.Language].GetByName(otherCCElement.Name);
            } catch (Exception e)
            {
                throw new Exception(
                    string.Format("Can't find project/content class/element {0}/{1}/{2} on server",
                                  otherCCElement.ContentClass.Project.Name, otherCCElement.ContentClass.Name,
                                  otherCCElement.Name), e);
            }
        }

        public void Refresh()
        {
        }

        #endregion
    }
}