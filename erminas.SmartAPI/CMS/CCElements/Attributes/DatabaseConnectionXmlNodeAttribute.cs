// Smart API - .Net programatical access to RedDot servers
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

namespace erminas.SmartAPI.CMS.CCElements.Attributes
{
    public class DatabaseConnectionXmlNodeAttribute : AbstractGuidXmlNodeAttribute<DatabaseConnection>
    {
        public DatabaseConnectionXmlNodeAttribute(CCElement parent, string name) : base(parent, name)
        {
        }

        protected override DatabaseConnection RetrieveByGuid(Guid guid)
        {
            return ((CCElement) Parent).ContentClass.Project.DatabaseConnections.GetByGuid(guid);
        }

        protected override DatabaseConnection RetrieveByName(string name)
        {
            return ((CCElement) Parent).ContentClass.Project.DatabaseConnections.GetByName(name);
        }

        protected override string GetTypeDescription()
        {
            return "database connection";
        }
    }
}