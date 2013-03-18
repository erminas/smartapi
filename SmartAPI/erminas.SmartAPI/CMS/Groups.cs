using System.Collections.Generic;
using System.Linq;
using System.Xml;
using erminas.SmartAPI.Utils;
using erminas.SmartAPI.Utils.CachedCollections;

namespace erminas.SmartAPI.CMS
{
    public class Group : RedDotObject
    {
        public Group(Session session, XmlElement xmlElement) : base(session, xmlElement)
        {
            EMailAdress = xmlElement.GetAttributeValue("email");
        }

        public string EMailAdress { get; private set; }
    }

    public class Groups : RDList<Group>, ISessionObject
    {
        public Session Session { get; private set; }

        internal Groups(Session session, Caching caching) : base(caching)
        {
            Session = session;
            RetrieveFunc = GetGroups;
        }

        private List<Group> GetGroups()
        {
            const string LIST_GROUPS = @"<ADMINISTRATION><GROUPS action=""list""/></ADMINISTRATION>";
            var xmlDoc = Session.ExecuteRQL(LIST_GROUPS, Session.IODataFormat.LogonGuidOnly);
            return
                (from XmlElement curGroup in xmlDoc.GetElementsByTagName("GROUP") select new Group(Session, curGroup))
                    .ToList();
        }
    }
}
