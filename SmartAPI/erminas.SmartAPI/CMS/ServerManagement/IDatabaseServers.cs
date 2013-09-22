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

using System.Collections.Generic;
using System.Linq;
using System.Xml;
using erminas.SmartAPI.CMS.Administration;
using erminas.SmartAPI.Utils.CachedCollections;

namespace erminas.SmartAPI.CMS.ServerManagement
{
    internal class DatabaseServers : NameIndexedRDList<IDatabaseServer>, IDatabaseServers
    {
        private readonly ISession _session;

        internal DatabaseServers(ISession session, Caching caching) : base(caching)
        {
            _session = session;
            RetrieveFunc = GetDatabaseServers;
        }

        private List<IDatabaseServer> GetDatabaseServers()
        {
            //TODO isession vs session
            using (new ServerManagementContext((Session) _session))
            {
                const string LIST_DATABASE_SERVERS =
                    @"<ADMINISTRATION><DATABASESERVERS action=""list"" /></ADMINISTRATION>";
                XmlDocument xmlDoc = _session.ExecuteRQL(LIST_DATABASE_SERVERS, RQL.IODataFormat.SessionKeyAndLogonGuid);

                XmlNodeList xmlNodes = xmlDoc.GetElementsByTagName("DATABASESERVER");
                return
                    (from XmlElement curNode in xmlNodes select (IDatabaseServer) new DatabaseServer(_session, curNode))
                        .ToList();
            }
        }
    }

    public interface IDatabaseServers : IIndexedRDList<string, IDatabaseServer>
    {
    }
}