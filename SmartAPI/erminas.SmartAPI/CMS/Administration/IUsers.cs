﻿// Smart API - .Net programmatic access to RedDot servers
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

namespace erminas.SmartAPI.CMS.Administration
{
    public interface IUsers : IIndexedRDList<String, IUser>
    {
        IUser Create(string name, string password);
    }

    internal class Users : NameIndexedRDList<IUser>, IUsers
    {
        private readonly ISession _session;

        internal Users(ISession session, Caching caching) : base(caching)
        {
            _session = session;
            RetrieveFunc = GetUsers;
        }

        public IUser Create(string name, string password)
        {
            const string CREATE_USER =
                @"<ADMINISTRATION><USER action=""addnew"" name=""{0}"" pw=""{1}"" lcid=""{2}"" userlanguage=""{3}""></USER></ADMINISTRATION>";
            _session.ExecuteRQL(CREATE_USER.SecureRQLFormat(name, password, _session.StandardLocale,
                                                            _session.StandardLocale.LanguageAbbreviation));
            Refresh();
            return this[name];
        }

        private List<IUser> GetUsers()
        {
            const string LIST_USERS = @"<ADMINISTRATION><USERS action=""list""/></ADMINISTRATION>";
            var userListDoc = _session.ExecuteRQL(LIST_USERS);

            return (from XmlElement curUserElement in userListDoc.GetElementsByTagName("USER")
                    select (IUser) new User(_session, curUserElement)).ToList();
        }
    }
}