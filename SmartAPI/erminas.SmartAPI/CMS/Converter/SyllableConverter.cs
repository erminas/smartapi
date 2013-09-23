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

namespace erminas.SmartAPI.CMS.Converter
{
    internal class SyllableConverter : AbstractGuidElementConverter<ISyllable>
    {
        protected override ISyllable GetFromGuid(IProjectObject parent, XmlElement element, RedDotAttribute attribute,
                                                 Guid guid)
        {
            return parent.Project.Syllables.GetByGuid(guid);
        }

        protected override ISyllable GetFromName(IProjectObject parent, IXmlReadWriteWrapper element,
                                                 RedDotAttribute attribute, ISyllable value)
        {
            ISyllable output;
            if (!parent.Project.Syllables.TryGetByName(value.Name, out output))
            {
                throw new SmartAPIException(parent.Session.ServerLogin,
                                            string.Format("Could not find a syllable with name {0} in project {1}",
                                                          value.Name, parent));
            }

            return output;
        }
    }
}