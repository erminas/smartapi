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

using erminas.SmartAPI.CMS.Project;
using erminas.SmartAPI.CMS.Project.ContentClasses;
using erminas.SmartAPI.CMS.Project.ContentClasses.Elements;
using erminas.SmartAPI.CMS.Project.Folder;
using erminas.SmartAPI.Exceptions;

namespace erminas.SmartAPI.CMS.Converter
{
    internal static class ConverterHelper
    {
        public static void CheckReadOnly<T>(IAttributeConverter<T> converter, IProjectObject po,
                                            RedDotAttribute attribute) where T : IProjectObject
        {
            if (converter.IsReadOnly || attribute.IsReadOnly)
            {
                throw new SmartAPIException(po.Session.ServerLogin,
                                            string.Format("Writing to attribute {0} is forbidden", attribute.Description));
            }
        }

        public static void EnsureValidProjectObject(IProjectObject parent)
        {
            if (parent == null)
            {
                throw new SmartAPIInternalException("Converter called with invalid project object");
            }
        }

        internal static bool AreFromTheSameProject<T>(IProjectObject projectOfTarget, T value)
            where T : class, IProjectObject
        {
            return projectOfTarget.Session == value.Session && projectOfTarget.Project.Equals(value.Project);
        }

        internal static IContentClassElement GetEquivalentContentClassElementFromOtherProject(IContentClassElement cc,
                                                                                              IProject otherProject)
        {
            var folderName = cc.ContentClass.Folder.Name;
            IContentClassFolder otherFolder;
            if (!otherProject.ContentClassFolders.TryGetByName(folderName, out otherFolder))
            {
                throw new SmartAPIException(otherProject.Session.ServerLogin,
                                            string.Format("Missing content class folder {0} for project {1}", folderName,
                                                          otherProject));
            }

            IContentClass otherContentClass;
            if (!otherFolder.ContentClasses.TryGetByName(cc.ContentClass.Name, out otherContentClass))
            {
                throw new SmartAPIException(otherProject.Session.ServerLogin,
                                            string.Format("Missing content class {0} in  folder {1} for project {2}",
                                                          cc.ContentClass.Name, folderName, otherProject));
            }

            IContentClassElement otherElement;
            if (!otherContentClass.Elements.TryGetByName(cc.Name, out otherElement))
            {
                throw new SmartAPIException(otherProject.Session.ServerLogin,
                                            string.Format(
                                                "Missing element {3} for content class {0} in  folder {1} for project {2}",
                                                cc.ContentClass.Name, folderName, otherProject, cc.Name));
            }

            return otherElement;
        }
    }
}