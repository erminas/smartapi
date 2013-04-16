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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml;
using erminas.SmartAPI.CMS.Project.Workflows;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS.Project.ContentClasses.Elements
{
    public interface IPreassignedWorkflow : IWorkflow
    {
        IWorkflowAssignable ElementPreassignedTo { get; }
        IEnumerable<ILanguageVariant> LanguageVariantsPreassignedTo { get; }
        void DisconnectFromLinkCompletely();
        void DisconnectFromLinkForLanguages(IEnumerable<ILanguageVariant> languageVariants);
        void DisconnectFromLinkForLanguages(params string[] languageVariants);
        void EnsureInitialization();
    }

    internal class PreassignedWorkflow : IPreassignedWorkflow
    {
        public IWorkflowAssignable ElementPreassignedTo { get; private set; }
        private readonly Workflow _workflow;
        private ReadOnlyCollection<ILanguageVariant> _languageVariants;

        internal PreassignedWorkflow(IWorkflowAssignable elementPreassignedTo, Workflow workflow)
        {
            ElementPreassignedTo = elementPreassignedTo;
            _workflow = workflow;
        }

        public IWorkflowActions Actions
        {
            get { return _workflow.Actions; }
        }

        public bool CanBeInherited
        {
            get { return _workflow.CanBeInherited; }
        }

        public void DisconnectFromLinkCompletely()
        {
            DisconnectFromLinkForLanguages(LanguageVariantsPreassignedTo);
        }

        public void DisconnectFromLinkForLanguages(IEnumerable<ILanguageVariant> languageVariants)
        {
            const string UNLINK_WORKFLOW =
                @"<WORKFLOW sessionkey=""{0}""><LINK action=""unlink"" guid=""{1}""><WORKFLOW guid=""{2}""><LANGUAGEVARIANTS>{3}</LANGUAGEVARIANTS></WORKFLOW></LINK></WORKFLOW>";

            ISession session = ElementPreassignedTo.Session;

            string query = UNLINK_WORKFLOW.RQLFormat(session.SessionKey, ElementPreassignedTo, _workflow,
                                                     languageVariants);

            session.ExecuteRql(query, RQL.IODataFormat.LogonGuidOnly);

            InvalidateCache();
        }

        public void DisconnectFromLinkForLanguages(params string[] languageVariants)
        {
            IProject project = ElementPreassignedTo.Project;

            IEnumerable<ILanguageVariant> languages =
                languageVariants.Select(language => project.LanguageVariants[language]);
            DisconnectFromLinkForLanguages(languages);
        }

        public void EnsureInitialization()
        {
            _workflow.EnsureInitialization();
        }

        public override bool Equals(object o)
        {
            var workflow = o as Workflow;
            return workflow != null && workflow.Guid == Guid;
        }

        public override int GetHashCode()
        {
            return _workflow.GetHashCode();
        }

        public void Delete()
        {
            _workflow.Delete();
        }

        public Guid Guid
        {
            get { return _workflow.Guid; }
            set { _workflow.Guid = value; }
        }

        public bool IsGlobal
        {
            get { return _workflow.IsGlobal; }
        }

        public bool IsStructureWorkflow
        {
            get { return _workflow.IsStructureWorkflow; }
        }

        public IEnumerable<ILanguageVariant> LanguageVariantsPreassignedTo
        {
            get
            {
                if (_languageVariants == null)
                {
                    RefreshLanguageVariants();
                }

                return _languageVariants;
            }
        }

        public string Name
        {
            get { return _workflow.Name; }
            set { _workflow.Name = value; }
        }

        public IProject Project
        {
            get { return _workflow.Project; }
        }

        public void Refresh()
        {
            _workflow.Refresh();
            RefreshLanguageVariants();
        }

        public ISession Session
        {
            get { return _workflow.Session; }
        }

        public XmlElement XmlNode
        {
            get { return _workflow.XmlElement; }
            set { _workflow.XmlElement = value; }
        }

        private ReadOnlyCollection<ILanguageVariant> GetPreAssignmentLanguageVariants()
        {
            const string LOAD_LANGUAGES =
                @"<WORKFLOW guid=""{0}""><LANGUAGEVARIANTS action=""workflowexisting"" linkguid=""{1}""/></WORKFLOW>";

            IProject project = ElementPreassignedTo.Project;

            var xmlDoc = project.ExecuteRQL(LOAD_LANGUAGES.RQLFormat(_workflow, ElementPreassignedTo));
            var languageVariants = xmlDoc.GetElementsByTagName("LANGUAGEVARIANT");

            return (from XmlElement curLanguage in languageVariants
                    select project.LanguageVariants[curLanguage.GetAttributeValue("language")]).ToList().AsReadOnly();
        }

        private void InvalidateCache()
        {
            _languageVariants = null;
        }

        private void RefreshLanguageVariants()
        {
            _languageVariants = GetPreAssignmentLanguageVariants();
        }
    }
}