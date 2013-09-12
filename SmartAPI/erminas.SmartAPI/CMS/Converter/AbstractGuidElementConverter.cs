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
using erminas.SmartAPI.Exceptions;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS.Converter
{
    public abstract class AbstractGuidElementConverter<T> : IAttributeConverter<T>
        where T : class, IRedDotObject, IProjectObject
    {
        public virtual T ConvertFrom(IProjectObject parent, XmlElement element, RedDotAttribute attribute)
        {
            if (parent == null)
            {
                throw new SmartAPIInternalException(string.Format(
                    "{0}.ConvertFrom must be called from a project object", GetType().Name));
            }

            Guid guid;
            return element.TryGetGuid(attribute.ElementName, out guid)
                       ? GetFromGuid(parent, element, attribute, guid)
                       : null;
        }

        public virtual bool IsReadOnly
        {
            get { return false; }
        }

        public virtual void WriteTo(IProjectObject parent, XmlElement element, RedDotAttribute attribute, T value)
        {
            if (parent == null)
            {
                throw new SmartAPIInternalException(string.Format("{0}.WriteTo must be called from a project object",
                                                                  GetType().Name));
            }

            CheckReadOnly(parent.Project, attribute);

            if (value == null)
            {
                element.SetAttributeValue(attribute.ElementName, null);
                return;
            }

            if (ConverterHelper.AreFromTheSameProject(parent, value))
            {
                element.SetAttributeValue(attribute.ElementName, value.Guid.ToRQLString());
            }
            else
            {
                T resolvedValue = GetFromName(parent, element, attribute, value);
                element.SetAttributeValue(attribute.ElementName,
                                          resolvedValue != null ? resolvedValue.Guid.ToRQLString() : null);
            }
        }

        protected void CheckReadOnly(IProject projectOfTarget, RedDotAttribute attribute)
        {
            if (IsReadOnly)
            {
                throw new SmartAPIException(projectOfTarget.Session.ServerLogin,
                                            string.Format("Writing to attribute {0} is forbidden", attribute.Description));
            }
        }

        protected abstract T GetFromGuid(IProjectObject parent, XmlElement element, RedDotAttribute attribute, Guid guid);

        protected abstract T GetFromName(IProjectObject parent, XmlElement element, RedDotAttribute attribute, T value);
    }
}