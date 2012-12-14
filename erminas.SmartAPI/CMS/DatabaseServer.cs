/*
 * Smart API - .Net programatical access to RedDot servers
 * Copyright (C) 2012  erminas GbR 
 *
 * This program is free software: you can redistribute it and/or modify it 
 * under the terms of the GNU General Public License as published by the Free Software Foundation,
 * either version 3 of the License, or (at your option) any later version.
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the GNU General Public License for more details. 
 *
 * You should have received a copy of the GNU General Public License along with this program.
 * If not, see <http://www.gnu.org/licenses/>. 
 */

using System;
using System.Xml;

namespace erminas.SmartAPI.CMS
{
    /// <summary>
    ///   A database server entry in RedDot.
    /// </summary>
    public class DatabaseServer : PartialRedDotObject
    {
        // ReSharper disable InconsistentNaming

        #region DbTypeId enum

        public enum DbTypeId
        {
            Jet3 = 1,
            Jet4 = 2,
            MS_SQL_Server = 3,
            MS_Oracle_OLEDB = 4,
            ODBC = 5,
            Oracle_OLEDB = 8
        }

        #endregion

        // ReSharper restore InconsistentNaming

        private DbTypeId _dBType;

        private bool _isCreateAllowed;
        private Guid _productGuid;

        public DatabaseServer(Session session, Guid guid) : base(guid)
        {
            Session = session;
        }

        public DatabaseServer(Session session, XmlElement xmlElement) : base(xmlElement)
        {
            Session = session;
            LoadXml();
        }

        public bool IsCreateAllowed
        {
            get { return LazyLoad(ref _isCreateAllowed); }
            set { _isCreateAllowed = value; }
        }

        public Guid ProductGuid
        {
            get { return LazyLoad(ref _productGuid); }
        }

        public DbTypeId DBType
        {
            get { return LazyLoad(ref _dBType); }
        }

        public Session Session { get; set; }

        private void LoadXml()
        {
            InitIfPresent(ref _isCreateAllowed, "createallowed", BoolConvert);
            InitIfPresent(ref _productGuid, "productguid", GuidConvert);
            InitIfPresent(ref _dBType, "dbtypeid", x => (DbTypeId) int.Parse(x));
        }

        protected override void LoadWholeObject()
        {
            LoadXml();
        }

        protected override XmlElement RetrieveWholeObject()
        {
            return Session.DatabaseServers.GetByGuid(Guid).XmlNode;
        }
    }
}