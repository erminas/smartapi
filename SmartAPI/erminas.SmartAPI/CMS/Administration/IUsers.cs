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
        private readonly Session _session;

        internal Users(Session session, Caching caching) : base(caching)
        {
            _session = session;
            RetrieveFunc = GetUsers;
        }

        private List<IUser> GetUsers()
        {
            const string LIST_USERS = @"<ADMINISTRATION><USERS action=""list""/></ADMINISTRATION>";
            var userListDoc = _session.ExecuteRQL(LIST_USERS);

            return
                (from XmlElement curUserElement in userListDoc.GetElementsByTagName("USER")
                 select (IUser)new User(_session, curUserElement)).ToList();
        }

        public IUser Create(string name, string password)
        {
            const string CREATE_USER = @"<ADMINISTRATION><USER action=""addnew"" name=""{0}"" pw=""{1}"" lcid=""{2}"" userlanguage=""{3}""></USER></ADMINISTRATION>";
            _session.ExecuteRQL(CREATE_USER.SecureRQLFormat(name, password, _session.StandardLocale, _session.StandardLocale.LanguageAbbreviation));
            Refresh();
            return this[name];
        }
    }
}
