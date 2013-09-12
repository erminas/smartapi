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
    public class SrcFileConverter : IAttributeConverter<IFile>
    {
        private const string ELTSRC = "eltsrc";
        private const string ELTFOLDERGUID = "eltfolderguid";
        private const string ELTSRCSUBDIRGUID = "eltsrcsubdirguid";

        public IFile ConvertFrom(IProjectObject parent, XmlElement element, RedDotAttribute attribute)
        {
            if (parent == null)
            {
                throw new SmartAPIInternalException("Parent project object may not be null for SrcFileConverter");
            }

            var fileName = element.GetAttributeValue(ELTSRC);

            if (string.IsNullOrEmpty(fileName))
            {
                return null;
            }

            var folder = GetFolder(parent, element);

            return new File(folder, fileName);
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public void WriteTo(IProjectObject parent, XmlElement element, RedDotAttribute attribute, IFile value)
        {
            if (parent == null)
            {
                throw new SmartAPIInternalException("Parent project object may not be null for SrcFileConverter");
            }

            if (value == null)
            {
                ClearFile(element);
                return;
            }

            if (ConverterHelper.AreFromTheSameProject(parent, value))
            {
                SetFromSameProject(element, value);
            }
            else
            {
                if (value.Folder.IsAssetManager)
                {
                    SetValuesFromAssetManagerFolder(parent, element, value);
                }
                else
                {
                    var ownFolder = parent.Project.Folders.GetByName(value.Folder.Name);
                    SetValuesFromTopLevelFolder(parent, element, value, ownFolder);
                }
            }
        }

        private static void ClearFile(XmlElement element)
        {
            element.SetAttributeValue(ELTSRCSUBDIRGUID, null);
            element.SetAttributeValue(ELTSRC, null);
        }

        private static IFolder GetFolder(IProjectObject parent, XmlElement element)
        {
            Guid folderGuid;
            if (!element.TryGetGuid(ELTSRCSUBDIRGUID, out folderGuid))
            {
                folderGuid = element.GetGuid(ELTFOLDERGUID);
            }

            var folder = parent.Project.Folders.AllIncludingSubFolders.GetByGuid(folderGuid);
            return folder;
        }

        private static IAssetManagerFolder GetTopLevelFolder(IProjectObject parent, IAssetManagerFolder value)
        {
            var ownTopLevelFolder = parent.Project.Folders.GetByName(value.ParentFolder.Name) as IAssetManagerFolder;
            if (ownTopLevelFolder == null)
            {
                throw new SmartAPIException(parent.Session.ServerLogin,
                                            string.Format("No asset folder with name {0} found in project {1}",
                                                          value.ParentFolder.Name, parent.Project));
            }
            return ownTopLevelFolder;
        }

        private static void SetFilename(IProjectObject parent, IFile value, IFolder ownFolder)
        {
            var ownFile = ownFolder.Files.GetByNamePattern(value.Name).SingleOrDefault();
            if (ownFile == null)
            {
                throw new SmartAPIException(parent.Session.ServerLogin,
                                            string.Format("No file with name {0} found in folder {1} of project {2}",
                                                          value.Name, ownFolder.Name, parent.Project));
            }
        }

        private static void SetFromSameProject(XmlElement element, IFile value)
        {
            var folderGuid = element.GetGuid(ELTFOLDERGUID);
//TODO at least cms 7.5 stores undefined as value, maybe "" is allowed, too, try this out
            element.SetAttributeValue(ELTSRCSUBDIRGUID,
                                      value.Folder.Guid == folderGuid ? "undefined" : value.Folder.Guid.ToRQLString());

            element.SetAttributeValue(ELTSRC, value.Name);
        }

        private static void SetValuesFromAssetManagerFolder(IProjectObject parent, XmlElement element, IFile value)
        {
            var assetFolder = (IAssetManagerFolder) value.Folder;
            var ownFolder = GetTopLevelFolder(parent, assetFolder);

            if (assetFolder.IsSubFolder)
            {
                SetValuesFromSubFolder(parent, element, value, ownFolder);
            }
            else
            {
                SetValuesFromTopLevelFolder(parent, element, value, ownFolder);
            }
        }

        private static void SetValuesFromSubFolder(IProjectObject parent, XmlElement element, IFile value,
                                                   IAssetManagerFolder ownFolder)
        {
            ownFolder = ownFolder.SubFolders.GetByName(value.Folder.Name);
            element.SetAttributeValue(ELTSRCSUBDIRGUID, ownFolder.Guid.ToRQLString());
            SetFilename(parent, value, ownFolder);
        }

        private static void SetValuesFromTopLevelFolder(IProjectObject parent, XmlElement element, IFile value,
                                                        IFolder ownFolder)
        {
            var folderGuid = element.GetGuid(ELTFOLDERGUID);
            element.SetAttributeValue(ELTSRCSUBDIRGUID,
                                      ownFolder.Guid == folderGuid ? "undefined" : ownFolder.Guid.ToRQLString());

            SetFilename(parent, value, ownFolder);
        }
    }
}