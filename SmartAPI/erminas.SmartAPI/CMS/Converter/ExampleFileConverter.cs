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
    public class ExampleFileConverter : IAttributeConverter<IFile>
    {
        private const string ELTSRC = "eltrdexample";
        private const string ELTFOLDERGUID = "eltfolderguid";
        private const string ELTSRCSUBDIRGUID = "eltrdexamplesubdirguid";

        public IFile ConvertFrom(IProjectObject parent, XmlElement element, RedDotAttribute attribute)
        {
            if (parent == null)
            {
                throw new SmartAPIInternalException("Parent project object may not be null for ExampleFileConverter");
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
                throw new SmartAPIInternalException("Parent project object may not be null for ExampleFileConverter");
            }

            if (value == null)
            {
                ClearFile(element);
                return;
            }

            if (!element.IsAttributeSet(parent, ELTFOLDERGUID))
            {
                throw new SmartAPIException(parent.Session.ServerLogin,
                                            string.Format(
                                                "Cannot set an example file ({0}) without an active folder in element {1}",
                                                value, parent));
            }

            if (ConverterHelper.AreFromTheSameProject(parent, value))
            {
                SetFromSameProject(parent, element, value);
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

        private static void SetFilename(IProjectObject parent, IFile value, XmlElement element, IFolder ownFolder)
        {
            var ownFile = ownFolder.Files.GetByNamePattern(value.Name).SingleOrDefault();
            if (ownFile == null)
            {
                throw new SmartAPIException(parent.Session.ServerLogin,
                                            string.Format("No file with name {0} found in folder {1} of project {2}",
                                                          value.Name, ownFolder.Name, parent.Project));
            }
            element.SetAttributeValue(ELTSRC, value.Name);
        }

        private static void SetFromSameProject(IProjectObject parent , XmlElement element, IFile value)
        {
            var folderGuid = element.GetGuid(ELTFOLDERGUID);
            var topLevelFolder = value.Folder;
            if (value.Folder.IsAssetManager)
            {
                var assetFolder = (IAssetManagerFolder) value.Folder;
                if (assetFolder.IsSubFolder)
                {
                    topLevelFolder = assetFolder.ParentFolder;
                }
            }

            if (topLevelFolder.Guid != folderGuid)
            {
                throw new SmartAPIException(parent.Session.ServerLogin, string.Format("Cannot set sample file '{0}', because it isn't in the current folder branch '{1}/'", value, parent.Project.Folders.GetByGuid(folderGuid).Name));
            }

            //TODO at least cms 7.5 stores undefined as value, maybe "" is allowed, too, try this out
            element.SetAttributeValue(ELTSRCSUBDIRGUID,
                                      value.Folder.Guid == folderGuid ? "undefined" : value.Folder.Guid.ToRQLString());

            element.SetAttributeValue(ELTSRC, value.Name);
        }

        private static void SetValuesFromAssetManagerFolder(IProjectObject parent, XmlElement element, IFile value)
        {
            var assetFolder = (IAssetManagerFolder) value.Folder;
            var folderGuid = element.GetGuid(ELTFOLDERGUID);
            if (assetFolder.IsSubFolder)
            {
                var folder = parent.Project.Folders.GetByGuid(folderGuid) as IAssetManagerFolder;
                if (folder == null)
                {
                    throw new SmartAPIException(parent.Session.ServerLogin,
                                                string.Format(
                                                    "Setting a example file from a subfolder into a non asset manager folder is not possible"));
                }
                if (folder.Name != assetFolder.ParentFolder.Name)
                {
                    throw new SmartAPIException(parent.Session.ServerLogin,
                                                string.Format(
                                                    "Example file not from subfolder of the currently active folder is not allowed (active folder: {0}, file folder {1})",
                                                    folder, value.Folder));
                }

                var ownSubFolder = folder.SubFolders.GetByName(value.Folder.Name);
                element.SetAttributeValue(ELTSRCSUBDIRGUID, ownSubFolder.Guid.ToRQLString());
                SetFilename(parent, value, element, ownSubFolder);
            }
            else
            {
                IFolder ownFolder;
                if (!parent.Project.Folders.TryGetByName(value.Folder.Name, out ownFolder))
                {
                    throw new SmartAPIException(parent.Session.ServerLogin,
                                                string.Format("Missing folder {1} in project {0}", value.Folder.Name,
                                                              parent.Project));
                }
                SetValuesFromTopLevelFolder(parent, element, value, ownFolder);
            }
        }

        private static void SetValuesFromTopLevelFolder(IProjectObject parent, XmlElement element, IFile value,
                                                        IFolder ownFolder)
        {
            var folderGuid = element.GetGuid(ELTFOLDERGUID);

            if (folderGuid != ownFolder.Guid)
            {
                throw new SmartAPIException(parent.Session.ServerLogin,
                                            string.Format(
                                                "Example file not from the folder or one of its subfolders is not allowed"));
            }

            element.SetAttributeValue(ELTSRCSUBDIRGUID, ownFolder.Guid.ToRQLString());
            SetFilename(parent, value, element, ownFolder);
        }
    }
}