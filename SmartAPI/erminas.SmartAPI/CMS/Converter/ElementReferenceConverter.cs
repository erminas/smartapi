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
using erminas.SmartAPI.CMS.Project;
using erminas.SmartAPI.CMS.Project.ContentClasses;
using erminas.SmartAPI.CMS.Project.ContentClasses.Elements;
using erminas.SmartAPI.Exceptions;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS.Converter
{
    //TODO test with different servers etc
    internal class ElementReferenceConverter : IAttributeConverter<IContentClassElement>
    {
        public IContentClassElement ConvertFrom(IProjectObject parent, XmlElement element, RedDotAttribute attribute)
        {
            Guid projectGuid, ccGuid, elementGuid;
            if (!element.TryGetGuid("eltprojectguid", out projectGuid) ||
                !element.TryGetGuid("elttemplateguid", out ccGuid) ||
                !element.TryGetGuid("eltelementguid", out elementGuid))
            {
                return null;
            }

            string langId = element.GetAttributeValue("eltlanguagevariantid");

            IProject project = parent.Session.ServerManager.Projects.GetByGuid(projectGuid);
            IContentClass contentClass = project.ContentClasses.GetByGuid(ccGuid);

            return contentClass.Elements.GetByGuid(elementGuid);
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public void WriteTo(IProjectObject parent, IXmlReadWriteWrapper element, RedDotAttribute attribute,
                            IContentClassElement value)
        {
            if (parent == null)
            {
                throw new SmartAPIInternalException(string.Format("{0}.WriteTo must be called from a project object",
                                                                  GetType().Name));
            }

            CheckReadOnly(parent.Project, attribute);

            if (value == null)
            {
                SetValuesToNull(element);
                return;
            }

            if (AreFromTheSameServer(parent, value))
            {
                SetValuesFromSameServer(element, value);
                return;
            }

            SetValuesFromOtherServer(parent, element, value);
        }

        private static bool AreFromTheSameServer(IProjectObject parent, IContentClassElement value)
        {
            return parent.Session == value.Session;
        }

        private void CheckReadOnly(IProject projectOfTarget, RedDotAttribute attribute)
        {
            if (IsReadOnly)
            {
                throw new SmartAPIException(projectOfTarget.Session.ServerLogin,
                                            string.Format("Writing to attribute {0} is forbidden", attribute.Description));
            }
        }

        private static IContentClassElement GetReferencedElement(IProjectObject parent, IContentClassElement value)
        {
            var project = parent.Project.Session.ServerManager.Projects[value.ContentClass.Project.Name];
            return ConverterHelper.GetEquivalentContentClassElementFromOtherProject(value, project);
        }

        private static void SetValuesFromOtherServer(IProjectObject parent, IXmlReadWriteWrapper element,
                                                     IContentClassElement value)
        {
            try
            {
                var referencedElement = GetReferencedElement(parent, value);

                SetValuesFromSameServer(element, referencedElement);
            } catch (Exception e)
            {
                throw new SmartAPIException(parent.Session.ServerLogin,
                                            string.Format(
                                                "Can't find project/content class/element {0}/{1}/{2} on server",
                                                value.ContentClass.Project.Name, value.ContentClass.Name, value.Name), e);
            }
        }

        private static void SetValuesFromSameServer(IXmlReadWriteWrapper element, IContentClassElement value)
        {
            //element.SetAttributeValue("eltlanguagevariantid", element.GetAttributeValue());
            element.SetAttributeValue("eltelementguid", value.Guid.ToRQLString());
            element.SetAttributeValue("elttemplateguid", value.ContentClass.Guid.ToRQLString());
            element.SetAttributeValue("eltprojectguid", value.Project.Guid.ToRQLString());
        }

        private static void SetValuesToNull(IXmlReadWriteWrapper element)
        {
            //element.SetAttributeValue("eltlanguagevariantid", null);
            element.SetAttributeValue("eltelementguid", null);
            element.SetAttributeValue("elttemplateguid", null);
            element.SetAttributeValue("eltprojectguid", null);
        }
    }
}