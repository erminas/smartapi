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
using erminas.SmartAPI.CMS.ServerManagement;
using erminas.SmartAPI.Exceptions;
using erminas.SmartAPI.Utils;
using erminas.SmartAPI.Utils.CachedCollections;

namespace erminas.SmartAPI.CMS.Project
{
    public interface IProjectGroups : IIndexedRDList<string, IGroup>, IProjectObject
    {
        void Add(IGroup group);
        void AddRange(IEnumerable<IGroup> group);
        void Clear();
        IGroup CreateAndAdd(string groupName, string groupEmailAdress);
        new IProjectGroups Refreshed();
        void Remove(IGroup group);
        void Set(IEnumerable<IGroup> group);
    }

    internal class ProjectGroups : NameIndexedRDList<IGroup>, IProjectGroups
    {
        private const string SINGLE_GROUP = @"<GROUP guid=""{0}"" />";
        private readonly IProject _project;

        internal ProjectGroups(IProject project, Caching caching) : base(caching)
        {
            _project = project;
            RetrieveFunc = GetAssignedGroups;
        }

        public void Add(IGroup @group)
        {
            AddRange(new[] {@group});
        }

        public void AddRange(IEnumerable<IGroup> groups)
        {
            const string ASSIGN_GROUPS =
                @"<ADMINISTRATION action=""assign""><PROJECT guid=""{0}"">{1}</PROJECT></ADMINISTRATION>";

            var groupsList = groups as IList<IGroup> ?? groups.ToList();
            if (!groupsList.Any())
            {
                return;
            }
            var groupsPart = groupsList.Aggregate("", (s, @group) => s + SINGLE_GROUP.RQLFormat(@group));

            var xmlDoc = Session.ExecuteRQL(ASSIGN_GROUPS.RQLFormat(Project, groupsPart));

            if (!xmlDoc.IsContainingOk())
            {
                var errorGroups = groupsList.Aggregate("", (s, @group) => s + @group.ToString() + ";");
                throw new SmartAPIException(Session.ServerLogin,
                                            string.Format("Could not add group(s) to {0}: {1}", Project, errorGroups));
            }
            InvalidateCache();
        }

        public void Clear()
        {
            RemoveRange(this);
        }

        public IGroup CreateAndAdd(string groupName, string groupEmailAdress)
        {
            const string CREATE_GROUP =
                @"<ADMINISTRATION><PROJECT guid=""{0}"" action=""assign""><GROUPS action=""addnew""><GROUP name=""{1}"" email=""{2}""/></GROUPS></PROJECT></ADMINISTRATION>";

            var xmlDoc = Session.ExecuteRQL(CREATE_GROUP.SecureRQLFormat(_project, groupName, groupEmailAdress));

            var groupGuid = xmlDoc.GetSingleElement("GROUP").GetGuid();
            InvalidateCache();
            return GroupFactory.CreateFromGuid(Session, groupGuid);
        }

        public IProject Project
        {
            get { return _project; }
        }

        public void Remove(IGroup @group)
        {
            RemoveRange(new[] {@group});
        }

        public ISession Session
        {
            get { return _project.Session; }
        }

        public void Set(IEnumerable<IGroup> newGroups)
        {
            var curGroups = this.ToList();
            var newGroupsList = newGroups as IList<IGroup> ?? newGroups.ToList();

            RemoveRange(curGroups.Except(newGroupsList));
            AddRange(newGroupsList.Except(curGroups));
        }

        private List<IGroup> GetAssignedGroups()
        {
            const string LIST_GROUPS = @"<PROJECT><GROUPS action=""list""/></PROJECT>";

            var xmlDoc = Session.ExecuteRQLInProjectContext(LIST_GROUPS, _project.Guid);
            return
                (from XmlElement curGroup in xmlDoc.GetElementsByTagName("GROUP")
                 select (IGroup) new Group(Session, curGroup)).ToList();
        }

        IProjectGroups IProjectGroups.Refreshed()
        {
            return this;
        }

        private void RemoveRange(IEnumerable<IGroup> groups)
        {
            const string UNASSIGN_GROUPS =
                @"<ADMINISTRATION action=""unlink""><PROJECT guid=""{0}"">{1}</PROJECT></ADMINISTRATION>";

            var groupsList = groups as IList<IGroup> ?? groups.ToList();
            if (!groupsList.Any())
            {
                return;
            }
            var groupsPart = groupsList.Aggregate("", (s, @group) => s + SINGLE_GROUP.RQLFormat(@group));

            var xmlDoc = Session.ExecuteRQL(UNASSIGN_GROUPS.RQLFormat(Project, groupsPart));
            //7.5 sends empty reply
            if (Session.ServerVersion >= new Version(9, 0) && !xmlDoc.IsContainingOk())
            {
                var errorGroups = groupsList.Aggregate("", (s, @group) => s + @group.ToString() + ";");
                throw new SmartAPIException(Session.ServerLogin,
                                            string.Format("Could not remove group(s) from {0}: {1}", Project,
                                                          errorGroups));
            }
            InvalidateCache();
        }
    }
}