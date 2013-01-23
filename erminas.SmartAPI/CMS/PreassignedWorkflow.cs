using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS
{
    public class PreassignedWorkflow : IWorkflow
    {
        public readonly IContentClassElement ElementPreassignedTo;
        private readonly Workflow _workflow;
        private ReadOnlyCollection<LanguageVariant> _languageVariants;

        internal PreassignedWorkflow(IContentClassElement elementPreassignedTo, Workflow workflow)
        {
            ElementPreassignedTo = elementPreassignedTo;
            _workflow = workflow;
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

        public ReadOnlyCollection<LanguageVariant> LanguageVariants
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

        public XmlElement XmlNode
        {
            get { return _workflow.XmlNode; }
            set { _workflow.XmlNode = value; }
        }

        public IEnumerable<WorkFlowAction> Actions()
        {
            return _workflow.Actions();
        }

        public bool CanBeInherited
        {
            get { return _workflow.CanBeInherited; }
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

        public string Name
        {
            get { return _workflow.Name; }
            set { _workflow.Name = value; }
        }

        public void Refresh()
        {
            _workflow.Refresh();
            RefreshLanguageVariants();
        }

        private void InvalidateCache()
        {
            _languageVariants = null;
        }

        public void EnsureInitialization()
        {
            _workflow.EnsureInitialization();
        }

        private ReadOnlyCollection<LanguageVariant> GetPreAssignmentLanguageVariants()
        {
            const string LOAD_LANGUAGES =
                @"<WORKFLOW guid=""{0}""><LANGUAGEVARIANTS action=""workflowexisting"" linkguid=""{1}""/></WORKFLOW>";

            Project project = ElementPreassignedTo.ContentClass.Project;

            var xmlDoc = project.ExecuteRQL(LOAD_LANGUAGES.RQLFormat(_workflow, ElementPreassignedTo));
            var languageVariants = xmlDoc.GetElementsByTagName("LANGUAGEVARIANT");

            return (from XmlElement curLanguage in languageVariants
                    select project.LanguageVariants[curLanguage.GetAttributeValue("language")]).ToList().AsReadOnly();
        }

        public void DisconnectWorkflowFromLinkCompletely()
        {
            DisconnectWorkflowFromLinkForLanguages(LanguageVariants);
        }

        public void DisconnectWorkflowFromLinkForLanguages(IEnumerable<LanguageVariant> languageVariants)
        {
            const string UNLINK_WORKFLOW =
                @"<WORKFLOW sessionkey=""{0}""><LINK action=""unlink"" guid=""{1}""><WORKFLOW guid=""{2}""><LANGUAGEVARIANTS>{3}</LANGUAGEVARIANTS></WORKFLOW></LINK></WORKFLOW>";

            Session session = ElementPreassignedTo.ContentClass.Project.Session;

            string query = UNLINK_WORKFLOW.RQLFormat(session.SessionKey, ElementPreassignedTo, _workflow, languageVariants);

            InvalidateCache();
            session.ExecuteRql(query, Session.IODataFormat.LogonGuidOnly);
        }

        public void DisconnectWorkflowFromLinkForLanguages(params string[] languageVariants)
        {
            Project project = ElementPreassignedTo.ContentClass.Project;

            IEnumerable<LanguageVariant> languages = languageVariants.Select(language => project.LanguageVariants[language]);
            DisconnectWorkflowFromLinkForLanguages(languages);
        }

        private void RefreshLanguageVariants()
        {
            _languageVariants = GetPreAssignmentLanguageVariants();
        }
    }
}