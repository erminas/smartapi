// Smart API - .Net programatical access to RedDot servers
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
using System.Linq;
using System.Xml;
using erminas.SmartAPI.Exceptions;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS
{
    internal class WorkflowPreassignable
    {
        private readonly IContentClassElement _element;

        private readonly Dictionary<LanguageVariant, PreassignedWorkflow> _preassignedContentWorkflows =
            new Dictionary<LanguageVariant, PreassignedWorkflow>();

        private bool _isStructuralWorkflowLoaded;
        private PreassignedWorkflow _preassignedStructuralWorkflow;

        internal WorkflowPreassignable(IContentClassElement element)
        {
            _element = element;
        }

        internal PreassignedWorkflow PreassignedStructuralWorkflow
        {
            get
            {
                if (!_isStructuralWorkflowLoaded)
                {
                    _preassignedStructuralWorkflow = LoadStructuralWorkflow();
                    _isStructuralWorkflowLoaded = true;
                }
                return _preassignedStructuralWorkflow;
            }
        }

        private PreassignedWorkflow LoadStructuralWorkflow()
        {
            const string LOAD_WORKFLOW =
                @"<WORKFLOW sessionkey=""{0}""><LINK guid=""{1}""><WORKFLOW action=""load"" structureworkflow=""1""/></LINK></WORKFLOW>";
            return ExecuteLoadWorkflow(LOAD_WORKFLOW);
        }

        private PreassignedWorkflow ExecuteLoadWorkflow(string LOAD_WORKFLOW)
        {
            Project project = _element.ContentClass.Project;
            var xmlDoc = project.ExecuteRQL(LOAD_WORKFLOW.RQLFormat(project.Session.SessionKey, _element));
            var workflowElement = (XmlElement) xmlDoc.SelectSingleNode("//WORKFLOW[@guid!='']");

            return workflowElement != null
                       ? new PreassignedWorkflow(_element, new Workflow(project, workflowElement))
                       : null;
        }

        internal PreassignedWorkflow GetPreassignedContentWorkflowFor(string languageVariantId)
        {
            LanguageVariant languageVariant = _element.ContentClass.Project.LanguageVariants[languageVariantId];
            return GetPreassignedContentWorkflowFor(languageVariant);
        }

        internal PreassignedWorkflow GetPreassignedContentWorkflowFor(LanguageVariant languageVariant)
        {
            PreassignedWorkflow workflow;
            if (!_preassignedContentWorkflows.TryGetValue(languageVariant, out workflow))
            {
                workflow = GetPreassignedContentWorkflow(languageVariant);

                if (workflow == null)
                {
                    _preassignedContentWorkflows[languageVariant] = null;
                    return null;
                }

                foreach (var curLanguage in workflow.LanguageVariants)
                {
                    _preassignedContentWorkflows[curLanguage] = workflow;
                }
            }
            return workflow;
        }

        internal void PreassignContentWorkflow(Workflow workflow, IEnumerable<LanguageVariant> languageVariants)
        {
            if (workflow.IsStructureWorkflow)
            {
                throw new SmartAPIException(_element.ContentClass.Project.Session.ServerLogin,
                                            "Workflow for preassignment is a structural workflow, although a content workflow is expected");
            }

            ExecutePreassignWorkflow(workflow, languageVariants);
        }

        internal void CreateAndPreassignContentWorkflow(string workflowName,
                                                        IEnumerable<LanguageVariant> languageVariants)
        {
            const bool IS_STRUCTURAL_WORKFLOW = false;
            ExecuteCreateAndPreassignWorkflow(workflowName, languageVariants, IS_STRUCTURAL_WORKFLOW);
        }

        internal void CreateAndPreassignStructuralworkflow(string workflowName)
        {
            const bool IS_STRUCTURAL_WORKFLOW = true;
            ExecuteCreateAndPreassignWorkflow(workflowName, new[] {_element.ContentClass.Project.CurrentLanguageVariant},
                                              IS_STRUCTURAL_WORKFLOW);
        }

        private void ExecuteCreateAndPreassignWorkflow(string workflowName,
                                                       IEnumerable<LanguageVariant> languageVariants, bool isStructural)
        {
            const string CREATE_AND_ASSIGN_WORKFLOW =
                @"<WORKFLOW sessionkey=""{0}""><LINK guid=""{1}"" action=""assign""><WORKFLOW action=""addnew"" structureworkflow=""{2}"" guid="""" name=""{3}""><LANGUAGEVARIANTS>{4}</LANGUAGEVARIANTS></WORKFLOW></LINK></WORKFLOW>";
            var session = _element.ContentClass.Project.Session;
            string query = CREATE_AND_ASSIGN_WORKFLOW.RQLFormat(session.SessionKey, _element, isStructural, workflowName,
                                                                languageVariants);
            session.ExecuteRql(query, Session.IODataFormat.LogonGuidOnly);
        }

        private void ExecutePreassignWorkflow(Workflow workflow, IEnumerable<LanguageVariant> languageVariants)
        {
            const string PREASSIGN_WORKFLOW =
                @"<WORKFLOW sessionkey=""{0}""><LINK guid=""{1}"" action=""assign""><WORKFLOW action=""addnew"" structureworkflow=""{2}"" guid=""{3}"" name=""{4}""><LANGUAGEVARIANTS>{5}</LANGUAGEVARIANTS></WORKFLOW></LINK></WORKFLOW>";

            var session = _element.ContentClass.Project.Session;
            session.ExecuteRql(
                PREASSIGN_WORKFLOW.RQLFormat(session.SessionKey, _element, workflow.IsStructureWorkflow, workflow,
                                             workflow.Name, languageVariants), Session.IODataFormat.LogonGuidOnly);

            InvalidateCache();
        }

        internal void PreassignStructuralWorkflow(Workflow workflow)
        {
            if (!workflow.IsStructureWorkflow)
            {
                throw new SmartAPIException(_element.ContentClass.Project.Session.ServerLogin,
                                            "Workflow for preassignment is not a structural workflow");
            }

            ExecutePreassignWorkflow(workflow, null);
        }

        internal void PreassignContentWorkflow(Workflow workflow, params string[] languageVariantIds)
        {
            var languageVariants =
                languageVariantIds.Select(
                    curLanguageVariantId => _element.ContentClass.Project.LanguageVariants[curLanguageVariantId]);
            PreassignContentWorkflow(workflow, languageVariants);
        }

        internal PreassignedWorkflow GetPreassignedContentWorkflowForCurrentLanguageVariant()
        {
            return GetPreassignedContentWorkflowFor(_element.ContentClass.Project.CurrentLanguageVariant);
        }

        internal void InvalidateCache()
        {
            _preassignedStructuralWorkflow = null;
            _isStructuralWorkflowLoaded = false;
            _preassignedContentWorkflows.Clear();
        }

        private PreassignedWorkflow GetPreassignedContentWorkflow(LanguageVariant languageVariant)
        {
            using (new LanguageContext(languageVariant))
            {
                return LoadPreassignedContentWorkflow();
            }
        }

        private PreassignedWorkflow LoadPreassignedContentWorkflow()
        {
            const string LOAD_CONTENT_WORKFLOW =
                @"<WORKFLOW sessionkey=""{0}""><LINK guid=""{1}""><WORKFLOW action=""load"" /></LINK></WORKFLOW>";
            return ExecuteLoadWorkflow(LOAD_CONTENT_WORKFLOW);
        }

        public void DisconnectAllWorkflows()
        {
            if (PreassignedStructuralWorkflow != null)
            {
                PreassignedStructuralWorkflow.DisconnectWorkflowFromLinkCompletely();
            }
            var workflows = new HashSet<Guid>();
            foreach (var curLang in _element.ContentClass.Project.LanguageVariants)
            {
                var workflow = GetPreassignedContentWorkflowFor(curLang);
                if (workflow == null)
                {
                    continue;
                }
                if (workflows.Contains(workflow.Guid))
                {
                    continue;
                }
                workflow.DisconnectWorkflowFromLinkCompletely();
                workflows.Add(workflow.Guid);
            }
            InvalidateCache();
        }

        public void CreateAndPreassignContentWorkflow(string workflowName, params string[] languageVariants)
        {
            CreateAndPreassignContentWorkflow(workflowName,
                                              languageVariants.Select(
                                                  curVariant =>
                                                  _element.ContentClass.Project.LanguageVariants[curVariant]));
        }
    }
}