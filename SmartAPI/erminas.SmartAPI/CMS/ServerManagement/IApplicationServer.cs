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
using System.Xml;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS.ServerManagement
{
    internal class ApplicationServer : PartialRedDotObject, IApplicationServer
    {
        private string _from;
        private string _ipAddress;
        private bool _isServerCheckDataLoaded;
        private string _tempPath;

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

        public override void Refresh()
        {
            _isServerCheckDataLoaded = false;
            base.Refresh();
            EnsureServerCheck();
        }

        public string TempDirectoryPath
        {
            get
            {
                EnsureServerCheck();
                return _tempPath;
            }
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
            var element = xmlDoc.GetSingleElement("EDITORIALSERVER");

            _tempPath = element.GetAttributeValue("temppath");

            _isServerCheckDataLoaded = true;
        }

        private void LoadXml()
        {
            _from = _xmlElement.GetAttributeValue("adress");
            _ipAddress = _xmlElement.GetAttributeValue("ip");
        }
    }

    public interface IApplicationServer : IPartialRedDotObject
    {
        string From { get; }
        string IpAddress { get; }
        string TempDirectoryPath { get; }
    }
}