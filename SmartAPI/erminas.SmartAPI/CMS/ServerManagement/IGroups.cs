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
using erminas.SmartAPI.Utils.CachedCollections;

namespace erminas.SmartAPI.CMS.ServerManagement
{
    internal class Groups : NameIndexedRDList<IGroup>, IGroups
    {
        private readonly ISession _session;

        public Groups(ISession session, Caching caching) : base(caching)
        {
            _session = session;
            RetrieveFunc = GetGroups;
        }

        public ISession Session
        {
            get { return _session; }
        }

        private List<IGroup> GetGroups()
        {
            const string LIST_GROUPS = @"<ADMINISTRATION><GROUPS action=""list""/></ADMINISTRATION>";
            XmlDocument xmlDoc = _session.ExecuteRQL(LIST_GROUPS, RQL.IODataFormat.LogonGuidOnly);
            return
                (from XmlElement curGroup in xmlDoc.GetElementsByTagName("GROUP")
                 select (IGroup) new Group(_session, curGroup)).ToList();
        }
    }

    public interface IGroups : IIndexedRDList<string, IGroup>, ISessionObject
    {
    }
}