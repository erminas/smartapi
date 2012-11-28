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
using erminas.SmartAPI.Exceptions;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS
{
    /// <summary>
    ///   A user in the RedDot system.
    /// </summary>
    public class User : PartialRedDotObject
    {
        private readonly CmsClient _cmsClient;
        private Guid _accountSystemGuid;
        private string _acs;
        private string _action;
        private string _description;
        private string _dialogLanguageId;
        private string _disablePassword;
        private string _email;
        private string _flags1;
        private string _flags2;
        private string _fullname;
        private string _id;
        private string _invertDirectEdit;
        private string _isServerManager;
        private string _lcid;
        private string _lm;
        private string _loginDate;
        private string _maxLevel;
        private string _maxLogin;
        private string _navigationType;
        private string _preferredEditor;
        private string _te;
        private string _userLanguage;
        private string _userLimits;

        public User(CmsClient cmsClient, Guid userGuid) : base(userGuid)
        {
            _cmsClient = cmsClient;
        }

        /// <summary>
        ///   Reads user data from XML-Element "USER" like: <pre>...</pre>
        /// </summary>
        /// <exception cref="RedDotDataException">Thrown if element doesn't contain valid data.</exception>
        /// <param name="cmsClient">The cms client used to retrieve this user</param>
        /// <param name="xmlNode"> USER XML-Element to get data from </param>
        public User(CmsClient cmsClient, XmlNode xmlNode)
            : base(xmlNode)
        {
            _cmsClient = cmsClient;
            // TODO: Read all data

            LoadXml(xmlNode);
        }

        // TODO: Nothing checked in here

        protected override void LoadXml(XmlNode node)
        {
            XmlAttributeCollection attr = node.Attributes;
            if (attr != null)
            {
                try
                {
                    _action = node.GetAttributeValue("action");
                    _id = node.GetAttributeValue("id");
                    Name = node.GetAttributeValue("name");
                    _fullname = node.GetAttributeValue("fullname");
                    _description = node.GetAttributeValue("description");
                    _flags1 = node.GetAttributeValue("flags1");
                    _flags2 = node.GetAttributeValue("flags2");
                    _maxLevel = node.GetAttributeValue("maxlevel");
                    _email = node.GetAttributeValue("email");
                    _acs = node.GetAttributeValue("acs");
                    _dialogLanguageId = node.GetAttributeValue("dialoglanguageid");
                    _userLanguage = node.GetAttributeValue("userlanguage");
                    _isServerManager = node.GetAttributeValue("isservermanager");
                    _loginDate = node.GetAttributeValue("logindate");
                    _te = node.GetAttributeValue("te");
                    _lm = node.GetAttributeValue("lm");
                    _navigationType = node.GetAttributeValue("navigationtype");
                    _lcid = node.GetAttributeValue("lcid");
                    _maxLogin = node.GetAttributeValue("maxlogin");
                    _preferredEditor = node.GetAttributeValue("preferrededitor");
                    _invertDirectEdit = node.GetAttributeValue("invertdirectedit");
                    _disablePassword = node.GetAttributeValue("disablepassword");
                    _userLimits = node.GetAttributeValue("userlimits");
                    if (node.GetAttributeValue("accountsystemguid") != null)
                    {
                        _accountSystemGuid = GuidConvert(node.GetAttributeValue("accountsystemguid"));
                    }
                }
                catch (Exception e)
                {
                    // couldn't read data
                    throw new RedDotDataException("Couldn't read content class data..", e);
                }
            }
        }

        protected override XmlNode RetrieveWholeObject()
        {
            const string LOAD_USER = @"<ADMINISTRATION><USER action=""load"" guid=""{0}""/></ADMINISTRATION>";
            string answer = _cmsClient.ExecuteRql(String.Format(LOAD_USER, Guid.ToRQLString()),
                                                  CmsClient.IODataFormat.LogonGuidOnly);
            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(answer);

            return xmlDocument.GetElementsByTagName("USER")[0];
        }

        #region Properties

        public Guid AccountSystemGuid
        {
            get { return LazyLoad(ref _accountSystemGuid); }
        }

        public string Action
        {
            get { return LazyLoad(ref _action); }
        }

        public string Id
        {
            get { return LazyLoad(ref _id); }
        }

        public string Fullname
        {
            get { return LazyLoad(ref _fullname); }
        }

        public string Description
        {
            get { return LazyLoad(ref _description); }
        }

        public string Flags1
        {
            get { return LazyLoad(ref _flags1); }
        }

        public string Flags2
        {
            get { return LazyLoad(ref _flags2); }
        }

        public string MaxLevel
        {
            get { return LazyLoad(ref _maxLevel); }
        }

        public string Acs
        {
            get { return LazyLoad(ref _acs); }
        }

        public string DialogLanguageId
        {
            get { return LazyLoad(ref _dialogLanguageId); }
        }

        public string UserLanguage
        {
            get { return LazyLoad(ref _userLanguage); }
        }

        public string IsServerManager
        {
            get { return LazyLoad(ref _isServerManager); }
        }

        public string LoginDate
        {
            get { return LazyLoad(ref _loginDate); }
        }

        public string Te
        {
            get { return LazyLoad(ref _te); }
        }

        public string Lm
        {
            get { return LazyLoad(ref _lm); }
        }

        public string NavigationType
        {
            get { return LazyLoad(ref _navigationType); }
        }

        public string Lcid
        {
            get { return LazyLoad(ref _lcid); }
        }

        public string MaxLogin
        {
            get { return LazyLoad(ref _maxLogin); }
        }

        public string PreferredEditor
        {
            get { return LazyLoad(ref _preferredEditor); }
        }

        public string InvertDirectEdit
        {
            get { return LazyLoad(ref _invertDirectEdit); }
        }

        public string DisablePassword
        {
            get { return LazyLoad(ref _disablePassword); }
        }

        public string UserLimits
        {
            get { return LazyLoad(ref _userLimits); }
        }

        public string EMail
        {
            get { return LazyLoad(ref _email); }
        }

        #endregion
    }
}