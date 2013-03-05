// Smart API - .Net programmatic access to RedDot servers
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

namespace erminas.SmartAPI.CMS.Project.ContentClasses.Elements.Attributes
{
    public class ProjectVariantAttribute : AbstractGuidXmlNodeAttribute<ProjectVariant>
    {
        public ProjectVariantAttribute(ContentClassElement parent, string name)
            : base(parent.ContentClass.Project.Session, parent, name)
        {
        }

        protected override string GetTypeDescription()
        {
            return "project variant";
        }

        protected override ProjectVariant RetrieveByGuid(Guid guid)
        {
            return ((ContentClassElement) Parent).ContentClass.Project.ProjectVariants.GetByGuid(guid);
        }

        protected override ProjectVariant RetrieveByName(string name)
        {
            return ((ContentClassElement) Parent).ContentClass.Project.ProjectVariants[name];
        }
    }
}