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

using System.Collections.Generic;
using System.Linq;
using System.Xml;
using erminas.SmartAPI.Utils.CachedCollections;

namespace erminas.SmartAPI.CMS.Project
{
    public interface IDatabaseConnections : IIndexedRDList<string, IDatabaseConnection>, IProjectObject
    {
    }

    internal class DatabaseConnections : NameIndexedRDList<IDatabaseConnection>, IDatabaseConnections
    {
        private readonly IProject _project;

        internal DatabaseConnections(IProject project, Caching caching) : base(caching)
        {
            _project = project;
            RetrieveFunc = GetDatabaseConnections;
        }

        public IProject Project
        {
            get { return _project; }
        }

        public ISession Session
        {
            get { return _project.Session; }
        }

        private List<IDatabaseConnection> GetDatabaseConnections()
        {
            const string LIST_DATABASE_CONNECTION = @"<DATABASES action=""list""/>";
            XmlDocument xmlDoc = _project.ExecuteRQL(LIST_DATABASE_CONNECTION, RqlType.SessionKeyInProject);
            XmlNodeList xmlNodes = xmlDoc.GetElementsByTagName("DATABASE");

            return
                (from XmlElement curNode in xmlNodes
                 select (IDatabaseConnection) new DatabaseConnection(_project, curNode)).ToList();
        }
    }
}