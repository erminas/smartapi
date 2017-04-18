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
using erminas.SmartAPI.Exceptions;
using erminas.SmartAPI.Utils;
using erminas.SmartAPI.Utils.CachedCollections;

namespace erminas.SmartAPI.CMS.ServerManagement
{
    public interface IGroup : IPartialRedDotObject, IDeletable, ISessionObject
    {
        string EMailAdress { get; set; }
        new string Name { get; set; }

        /// <summary>
        ///     All users in this group, indexed by name. The list is cached by default.
        /// </summary>
        IIndexedRDList<string, IUser> Users { get; }

        void Commit();
    }

    public static class GroupFactory
    {
        public static IGroup CreateFromGuid(ISession session, Guid guid)
        {
            return new Group(session, guid);
        }
    }

    internal class Group : PartialRedDotObject, IGroup
    {
        private string _email;

        internal Group(ISession session, XmlElement xmlElement) : base(session, xmlElement)
        {
            Init();
            LoadXml();
        }

        internal Group(ISession session, Guid guid) : base(session, guid)
        {
            Init();
        }

        public void Commit()
        {
            const string SAVE_GROUP =
                @"<ADMINISTRATION><GROUP action=""save"" guid=""{0}"" name=""{1}"" email=""{2}""/></ADMINISTRATION>";

            var xmlDoc = Session.ExecuteRQL(SAVE_GROUP.SecureRQLFormat(this, Name, EMailAdress));
            if (!xmlDoc.InnerText.Contains(Guid.ToRQLString()))
            {
                throw new SmartAPIException(Session.ServerLogin, string.Format("Could not save group {0}", this));
            }
        }

        public void Delete()
        {
            const string DELETE_GROUP = @"<ADMINISTRATION><GROUP action=""delete"" guid=""{0}""/></ADMINISTRATION>";
            var xmlDoc = Session.ExecuteRQL(DELETE_GROUP.RQLFormat(this));
            if (!xmlDoc.IsContainingOk())
            {
                throw new SmartAPIException(Session.ServerLogin, string.Format("Could not delete group {0}", this));
            }
        }

        public string EMailAdress
        {
            get { return LazyLoad(ref _email); }
            set
            {
                EnsureInitialization();
                _email = value;
            }
        }

        public new string Name
        {
            get { return base.Name; }
            set
            {
                EnsureInitialization();
                base.Name = value;
            }
        }

        public IIndexedRDList<string, IUser> Users { get; private set; }

        private void Init()
        {
            Users = new NameIndexedRDList<IUser>(GetUsers, Caching.Enabled);
        }

        private List<IUser> GetUsers()
        {
            const string LOAD_USERS =
                @"<ADMINISTRATION><GROUP guid=""{0}""><USERS action=""list""/></GROUP></ADMINISTRATION>";
            var xmlDoc = Session.ExecuteRQL(LOAD_USERS.RQLFormat(this));

            var userElements = xmlDoc.GetElementsByTagName("USER");
            return
            (from XmlElement curUser in userElements
                select
                (IUser)
                new User(Session, curUser.GetGuid())
                {
                    Name = curUser.GetName(),
                    FullName = curUser.GetAttributeValue("fullname"),
                    EMail = curUser.GetAttributeValue("email")
                }).ToList();
        }

        protected override void LoadWholeObject()
        {
            LoadXml();
        }

        protected override XmlElement RetrieveWholeObject()
        {
            const string LOAD_GROUP = @"<ADMINISTRATION><GROUP action=""load"" guid=""{0}""/></ADMINISTRATION>";
            return Session.ExecuteRQL(LOAD_GROUP.RQLFormat(this)).GetSingleElement("GROUP");
        }

        private void LoadXml()
        {
            _email = _xmlElement.GetAttributeValue("email");
        }
    }
}