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

using System.Collections.Generic;
using System.Linq;
using System.Xml;
using erminas.SmartAPI.Exceptions;
using erminas.SmartAPI.Utils;
using erminas.SmartAPI.Utils.CachedCollections;

namespace erminas.SmartAPI.CMS.Project
{
    public interface ILanguageVariants : IIndexedRDList<string, ILanguageVariant>, IProjectObject
    {
        ILanguageVariant Current { get; set; }
        ILanguageVariant Main { get; }


        //TODO support for email?
        void Create(string name, ISystemLocale locale, ICharset charset, bool useRfcLanguageIdForDeliveryServer = false, TextDirection textDirection = TextDirection.LeftToRight, ILanguageVariant adoptContentFrom = null,
            ILanguageVariant adtopWorkflowFrom = null);
    }

    internal class LanguageVariants : IndexedRDList<string, ILanguageVariant>, ILanguageVariants
    {
        private readonly IProject _project;
        private ILanguageVariant _currentLanguageVariant;

        internal LanguageVariants(IProject project, Caching caching) : base(variant => variant.Abbreviation, caching)
        {
            _project = project;
            RetrieveFunc = GetLanguageVariants;
        }

        /// <summary>
        ///     Retrieves or selects the active language variant.
        /// </summary>
        public ILanguageVariant Current
        {
            get
            {
                EnsureListIsLoaded();
                return _currentLanguageVariant;
            }
            set
            {
                if (_currentLanguageVariant == value)
                {
                    return;
                }
                const string SELECT_LANGUAGE = @"<LANGUAGEVARIANT action=""setactive"" guid=""{0}""/>";
                XmlDocument xmlDoc = _project.ExecuteRQL(SELECT_LANGUAGE.RQLFormat(value.Guid.ToRQLString()),
                                                         RqlType.SessionKeyInProject);
                if (!xmlDoc.IsContainingOk())
                {
                    throw new SmartAPIException(Session.ServerLogin,
                                                string.Format("Could not load language variant '{0}' for project {1}",
                                                              value.Abbreviation, this));
                }
                if (_currentLanguageVariant != null)
                {
                    ((LanguageVariant) _currentLanguageVariant).IsCurrentLanguageVariant = false;
                }
                ((LanguageVariant) value).IsCurrentLanguageVariant = true;
                _currentLanguageVariant = value;
            }
        }

        public ILanguageVariant Main
        {
            get { return this.First(variant => variant.IsMainLanguage); }
        }

        //TODO use configuration object?
        public void Create(string name, ISystemLocale locale, ICharset charset, bool useRfcLanguageIdForDeliveryServer = false, TextDirection textDirection = TextDirection.LeftToRight, ILanguageVariant adoptContentFrom = null,
            ILanguageVariant adtopWorkflowFrom = null)
        {
            const string CREATE_LANGUAGE_VARIANT =
                @"<LANGUAGEVARIANT action=""addnew"" name=""{0}"" language=""{1}"" defaultlanguagevariant=""0"" codetable=""{2}"" rfclanguageid=""{3}"" userfclanguageidfordeliveryserver=""{4}"" languagefrom=""{5}"" contentworkflowlanguagefrom=""{6}"" textdirection=""{7}"" />";

            var response = Project.ExecuteRQL(CREATE_LANGUAGE_VARIANT.RQLFormat(name, locale.LanguageAbbreviation, charset.Codepage,
                locale.RFCLanguageId, useRfcLanguageIdForDeliveryServer, adtopWorkflowFrom, adoptContentFrom,
                textDirection));

            if (response.GetElementsByTagName("LANGUAGEVARIANT").Count == 0)
            {
                throw new SmartAPIException(Session.ServerLogin, string.Format("Could not create language variant '{0}' for project {1}", name, Project));
            }

            InvalidateCache();
        }

        public IProject Project
        {
            get { return _project; }
        }

        public ISession Session
        {
            get { return _project.Session; }
        }

        private List<ILanguageVariant> GetLanguageVariants()
        {
            const string LIST_LANGUAGE_VARIANTS =
                @"<PROJECT projectguid=""{0}""><LANGUAGEVARIANTS action=""list""/></PROJECT>";
            XmlDocument xmlDoc = Project.ExecuteRQL(LIST_LANGUAGE_VARIANTS.RQLFormat(Project));
            XmlNodeList xmlNodes = xmlDoc.GetElementsByTagName("LANGUAGEVARIANT");
            var languageVariants = new List<ILanguageVariant>();

            foreach (XmlElement curNode in xmlNodes)
            {
                var variant = new LanguageVariant(Project, curNode);
                languageVariants.Add(variant);
                if (variant.IsCurrentLanguageVariant)
                {
                    _currentLanguageVariant = variant;
                }
            }

            return languageVariants;
        }
    }
}