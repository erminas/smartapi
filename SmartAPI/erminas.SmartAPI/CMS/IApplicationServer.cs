using System;
using System.Xml;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS
{
    internal class ApplicationServer : PartialRedDotObject, IApplicationServer
    {
        private string _from;
        private string _ipAddress;
        private string _tempPath;
        private bool _isServerCheckDataLoaded;

        public ApplicationServer(Session session, Guid guid) : base(session, guid)
        {
        }

        internal ApplicationServer(Session session, XmlElement element) : base(session, element)
        {
            LoadXml();
        }

        public string From
        {
            get { return LazyLoad(ref _from); }
            internal set { _from = value; }
        }

        public string IpAddress
        {
            get { return LazyLoad(ref _ipAddress); }
            internal set { _ipAddress = value; }
        }

        public string TempDirectoryPath
        {
            get
            {
                EnsureServerCheck();
                return _tempPath;
            }
        }

        public override void Refresh()
        {
            _isServerCheckDataLoaded = false;
            base.Refresh();
            EnsureServerCheck();
        }

        protected override void LoadWholeObject()
        {
            LoadXml();
        }

        protected override XmlElement RetrieveWholeObject()
        {
            const string LOAD_APPLICATION_SERVER =
                @"<ADMINISTRATION><EDITORIALSERVER action=""load"" guid=""{0}""/></ADMINISTRATION>";

            XmlDocument xmlDoc = Session.ExecuteRQL(LOAD_APPLICATION_SERVER.RQLFormat(this));
            return xmlDoc.GetSingleElement("EDITORIALSERVER");
        }

        private void EnsureServerCheck()
        {
            if (_isServerCheckDataLoaded)
            {
                return;
            }

            const string CHECK_SERVER =
                @"<ADMINISTRATION><EDITORIALSERVER action=""check"" guid=""{0}""/></ADMINISTRATION>";

            var xmlDoc = Session.ExecuteRQL(CHECK_SERVER.RQLFormat(this));
            var element  = xmlDoc.GetSingleElement("EDITORIALSERVER");

            _tempPath = element.GetAttributeValue("temppath");

            _isServerCheckDataLoaded = true;
        }

        private void LoadXml()
        {
            _from = XmlElement.GetAttributeValue("adress");
            _ipAddress = XmlElement.GetAttributeValue("ip");
        }
    }

    public interface IApplicationServer : IPartialRedDotObject
    {
        string From { get; }
        string IpAddress { get; }
        string TempDirectoryPath { get; }
    }
}