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
    internal class ServerManager : IServerManager
    {
        private readonly Session _session;

        internal ServerManager(Session session)
        {
            _session = session;

            Groups = new Groups(_session, Caching.Enabled);
            Projects = new Projects(_session, Caching.Enabled);
            DatabaseServers = new DatabaseServers(_session, Caching.Enabled);
            Users = new Users(_session, Caching.Enabled);
            Modules = new IndexedRDList<ModuleType, IModule>(GetModules, x => x.Type, Caching.Enabled);
            AsynchronousProcesses = new RDList<IAsynchronousProcess>(GetAsynchronousProcesses, Caching.Disabled);
            ApplicationServers = new ApplicationServers(_session, Caching.Enabled);
        }

        public IApplicationServers ApplicationServers { get; private set; }

        public IRDList<IAsynchronousProcess> AsynchronousProcesses { get; private set; }
        public IDatabaseServers DatabaseServers { get; private set; }
        public IGroups Groups { get; private set; }
        public IIndexedRDList<ModuleType, IModule> Modules { get; private set; }

        public IProjects Projects { get; private set; }
        public IUsers Users { get; private set; }

        private List<IAsynchronousProcess> GetAsynchronousProcesses()
        {
            const string LIST_PROCESSES = @"<ADMINISTRATION><ASYNCQUEUE action=""list"" project=""""/></ADMINISTRATION>";
            XmlDocument xmlDoc = _session.ExecuteRQL(LIST_PROCESSES);
            return (from XmlElement curProcess in xmlDoc.GetElementsByTagName("ASYNCQUEUE")
                    select (IAsynchronousProcess) new AsynchronousProcess(_session, curProcess)).ToList();
        }

        private List<IModule> GetModules()
        {
            const string LIST_MODULES = @"<ADMINISTRATION><MODULES action=""list"" /></ADMINISTRATION>";
            XmlDocument xmlDoc = _session.ExecuteRQL(LIST_MODULES);

            //we need to create an intermediate list, because the XmlNodeList returned by GetElementsByTagName gets changed in the linq/ToList() expression.
            //the change to the list occurs due to the cloning on the XmlElements in Module->AbstractAttributeContainer c'tor.
            //i have no idea why that changes the list as the same approach works without a problem everywhere else without the need for the intermediate list.
            List<XmlElement> moduleElements = xmlDoc.GetElementsByTagName("MODULE").OfType<XmlElement>().ToList();
            return
                (from XmlElement curModule in moduleElements select (IModule) new Module(_session, curModule)).ToList();
        }
    }

    public interface IServerManager
    {
        IApplicationServers ApplicationServers { get; }

        /// <summary>
        ///     The asynchronous processes running on the server. The list is _NOT_ cached by default.
        /// </summary>
        /// <remarks>
        ///     Caching is disabled by default.
        /// </remarks>
        IRDList<IAsynchronousProcess> AsynchronousProcesses { get; }

        IDatabaseServers DatabaseServers { get; }
        IGroups Groups { get; }

        IIndexedRDList<ModuleType, IModule> Modules { get; }
        IProjects Projects { get; }
        IUsers Users { get; }
    }
}