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
using erminas.SmartAPI.CMS.Project.Folder;
using erminas.SmartAPI.CMS.ServerManagement;

namespace erminas.SmartAPI.CMS.Project.ContentClasses
{
    public interface IContentClassVersion : IRedDotObject, IProjectObject
    {
        IContentClass ContentClass { get; }
        ContentClassVersionType CreationType { get; }

        /// <summary>
        ///     Time the version was created
        /// </summary>
        DateTime Date { get; }

        /// <summary>
        ///     Description text
        /// </summary>
        string DescriptionInCurrentDisplayLanguage { get; }

        IContentClassFolder Folder { get; }
        IUser User { get; }
        string Username { get; }
    }
}