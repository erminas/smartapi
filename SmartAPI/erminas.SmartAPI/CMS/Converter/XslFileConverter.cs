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
using erminas.SmartAPI.CMS.Project.Folder;
using erminas.SmartAPI.Exceptions;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS.Converter
{
    public class XslFileConverter : IAttributeConverter<IFile>
    {
        private const string ELTXSLFILE = "eltxslfile";
        private const string ELTFOLDERGUID = "eltfolderguid";

        public IFile ConvertFrom(IProjectObject parent, XmlElement element, RedDotAttribute attribute)
        {
            ConverterHelper.EnsureValidProjectObject(parent);

            Guid folderGuid;
            if (!element.TryGetGuid(ELTFOLDERGUID, out folderGuid) || !element.IsAttributeSet(parent, ELTXSLFILE))
            {
                return null;
            }

            var folder = parent.Project.Folders.GetByGuid(folderGuid);
            return new File(folder, element.GetAttributeValue(ELTXSLFILE));
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public void WriteTo(IProjectObject parent, XmlElement element, RedDotAttribute attribute, IFile value)
        {
            ConverterHelper.EnsureValidProjectObject(parent);

            ConverterHelper.CheckReadOnly(this, parent, attribute);

            if (value == null)
            {
                SetEmptyValues(element);
                return;
            }

            if (ConverterHelper.AreFromTheSameProject(parent, value))
            {
                SetValuesFromSameProject(element, value);
                return;
            }

            SetValuesFromDifferentProjects(parent, element, attribute, value);
        }

        private static void SetEmptyValues(XmlElement element)
        {
            element.SetAttributeValue(ELTFOLDERGUID, null);
            element.SetAttributeValue(ELTXSLFILE, null);
        }

        private static void SetValuesFromDifferentProjects(IProjectObject parent, XmlElement element,
                                                           RedDotAttribute attribute, IFile value)
        {
            IFolder ownFolder;
            if (!parent.Project.Folders.TryGetByName(value.Folder.Name, out ownFolder))
            {
                throw new SmartAPIException(parent.Session.ServerLogin,
                                            string.Format("No such folder {0} in project {1} for setting of {2}",
                                                          value.Folder.Name, parent.Project, attribute.Description));
            }

            var ownFile = ownFolder.Files.GetByNamePattern(value.Name).SingleOrDefault();
            if (ownFile == null)
            {
                throw new SmartAPIException(parent.Session.ServerLogin,
                                            string.Format(
                                                "No such file {3} in folder {0} in project {1} for setting of {2}",
                                                value.Folder.Name, parent.Project, attribute.Description, value.Name));
            }

            element.SetAttributeValue(ELTFOLDERGUID, ownFolder.Guid.ToRQLString());
            element.SetAttributeValue(ELTXSLFILE, ownFile.Name);
        }

        private static void SetValuesFromSameProject(XmlElement element, IFile value)
        {
            element.SetAttributeValue(ELTFOLDERGUID, value.Folder.Guid.ToRQLString());
            element.SetAttributeValue(ELTXSLFILE, value.Name);
        }
    }
}