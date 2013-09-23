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
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using erminas.SmartAPI.Utils;
using erminas.SmartAPI.Utils.CachedCollections;

namespace erminas.SmartAPI.CMS.ServerManagement
{
    internal class ApplicationServers : RDList<IApplicationServer>, IApplicationServers
    {
        private readonly Lazy<IApplicationServer> _currentApplicationServer;
        private readonly Session _session;

        public ApplicationServers(Session session, Caching caching) : base(caching)
        {
            _session = session;
            RetrieveFunc = GetApplicationServers;
            _currentApplicationServer = new Lazy<IApplicationServer>(GetCurrentApplicationServer);
        }

        public IApplicationServer Current
        {
            get { return _currentApplicationServer.Value; }
        }

        public ISession Session
        {
            get { return _session; }
        }

        private List<IApplicationServer> GetApplicationServers()
        {
            const string LIST_APPLICATION_SERVERS =
                @"<ADMINISTRATION><EDITORIALSERVERS action=""list""/></ADMINISTRATION>";
            XmlDocument xmlDoc = _session.ExecuteRQL(LIST_APPLICATION_SERVERS);

            XmlNodeList editorialServers = xmlDoc.GetElementsByTagName("EDITORIALSERVER");
            return (from XmlElement curServer in editorialServers
                    select
                        (IApplicationServer)
                        new ApplicationServer(_session, curServer.GetGuid())
                            {
                                Name = curServer.GetName(),
                                IpAddress = curServer.GetAttributeValue("ip")
                            }).ToList();
        }

        private IApplicationServer GetCurrentApplicationServer()
        {
            const string LOAD_APPLICATIONSERVER =
                @"<ADMINISTRATION><EDITORIALSERVER action=""check""/></ADMINISTRATION>";
            XmlDocument reply = _session.ExecuteRQL(LOAD_APPLICATIONSERVER, RQL.IODataFormat.Plain);
            return new ApplicationServer(_session, reply.GetSingleElement("EDITORIALSERVER").GetGuid("serverguid"));
        }
    }

    public interface IApplicationServers : IRDList<IApplicationServer>, ISessionObject
    {
        IApplicationServer Current { get; }
    }
}