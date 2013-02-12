// Smart API - .Net programmatic access to RedDot servers
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

namespace erminas.SmartAPI.CMS.Administration
{
    /// <summary>
    ///     A database connection entry in the RedDot server.
    /// </summary>
    public class DatabaseConnection : PartialRedDotObject
    {
        public DatabaseConnection(Project.Project project, Guid guid) : base(guid)
        {
            Project = project;
        }

        internal DatabaseConnection(Project.Project project, XmlElement xmlElement) : base(xmlElement)
        {
            Project = project;
            LoadXml();
        }

        protected override void LoadWholeObject()
        {
            LoadXml();
        }

        protected override XmlElement RetrieveWholeObject()
        {
            const string LOAD_DATABASE_CONNECTION = @"<DATABASE action=""load"" guid=""{0}""/>";
            XmlDocument xmlDoc = Project.ExecuteRQL(String.Format(LOAD_DATABASE_CONNECTION, Guid.ToRQLString()),
                                                    CMS.Project.Project.RqlType.SessionKeyInProject);
            XmlNodeList xmlNodes = xmlDoc.GetElementsByTagName("DATABASE");
            if (xmlNodes.Count != 1)
            {
                throw new ArgumentException("Could not find database connection with guid " + Guid.ToRQLString());
            }
            return (XmlElement) xmlNodes[0];
        }

        private void LoadXml()
        {
            InitIfPresent(ref _description, "description", x => x);
            InitIfPresent(ref _databaseServer, "databaseserverguid",
                          x => new DatabaseServer(Project.Session, GuidConvert(x)));
            InitIfPresent(ref _databaseName, "databasename", x => x);
        }

        #region Properties

        /// <summary>
        ///     Name of the database used in the connection
        /// </summary>
        public string DatabaseName
        {
            get { return LazyLoad(ref _databaseName); }
        }

        /// <summary>
        ///     Database server used in the connection.
        /// </summary>
        public DatabaseServer DatabaseServer
        {
            get { return LazyLoad(ref _databaseServer); }
        }

        /// <summary>
        ///     Description of the database connection
        /// </summary>
        public string Description
        {
            get { return LazyLoad(ref _description); }
        }

        public Project.Project Project { get; set; }

        #endregion

        #region Fields

        private string _databaseName;
        private DatabaseServer _databaseServer;
        private string _description;

        #endregion
    }
}