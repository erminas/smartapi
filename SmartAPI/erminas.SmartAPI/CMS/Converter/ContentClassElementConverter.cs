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
using System.Linq;
using System.Xml;
using erminas.SmartAPI.CMS.Project;
using erminas.SmartAPI.CMS.Project.ContentClasses.Elements;
using erminas.SmartAPI.Exceptions;

namespace erminas.SmartAPI.CMS.Converter
{
    public class ContentClassElementConverter : AbstractGuidElementConverter<IContentClassElement>
    {
        protected override IContentClassElement GetFromGuid(IProjectObject parent, XmlElement element,
                                                            RedDotAttribute attribute, Guid guid)
        {
            if (!(parent is IContentClassElement))
            {
                throw new SmartAPIInternalException("Element converter can only be used from IContentClassElement");
            }

            var parentCcElement = (IContentClassElement) parent;
            return
                parentCcElement.ContentClass.Elements[parentCcElement.LanguageVariant.Abbreviation].FirstOrDefault(
                    x => x.Guid.Equals(guid));
        }

        protected override IContentClassElement GetFromName(IProjectObject parent, XmlElement element,
                                                            RedDotAttribute attribute, IContentClassElement value)
        {
            return ConverterHelper.GetEquivalentContentClassElementFromOtherProject(value, parent.Project);
        }
    }
}