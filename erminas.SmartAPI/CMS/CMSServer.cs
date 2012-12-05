using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;

namespace erminas.SmartAPI.CMS
{
    public class CMSServer : PartialRedDotObject
    {
        #region ActiveID enum

        public enum ActiveID
        {
            Active = 0,
            Inactive = 1,
            Blocked_version_number_less_than_current_version_number_server_originally_active = 2,
            Blocked_version_number_less_than_current_version_number_server_originally_inactive = 3,
            Blocked_version_number_higher_than_current_version_number_server_originally_active = 4,
            Blocked_version_number_higher_than_current_version_number_server_originally_inactive = 5
        }

        #endregion

        private string _serverAddress;
        private ActiveID _activeState;

        public CMSServer(Session session, Guid guid)
            : base(guid)
        {
            Session = session;
        }

        public CMSServer(Session session, XmlElement xmlElement)
            : base(xmlElement)
        {
            Session = session;
            LoadXml();
        }

        public ActiveID ActiveState
        {
            get { return LazyLoad(ref _activeState); }
        }

        // This is documented as th IP address in RQL documentation, but as the user can set this value manually in CMS this can be the e.g. servername and does not have tob be a valid IP-Address 
        public string ServerAddress
        {
            get { return LazyLoad(ref _serverAddress); }
        }

        public Session Session { get; set; }

        protected override void LoadWholeObject()
        {
            LoadXml();
        }

        private void LoadXml()
        {
            InitIfPresent(ref _serverAddress, "ip", x => x);
            InitIfPresent(ref _activeState, "active", x => (ActiveID)int.Parse(x));
        }

        protected override XmlElement RetrieveWholeObject()
        {
            return Session.CMSServers.GetByGuid(Guid).XmlNode;
        }
    }
}
