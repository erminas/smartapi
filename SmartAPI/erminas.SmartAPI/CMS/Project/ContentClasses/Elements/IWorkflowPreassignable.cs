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
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using erminas.SmartAPI.CMS.Project.Workflows;
using erminas.SmartAPI.Exceptions;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS.Project.ContentClasses.Elements
{
    public interface IWorkflowAssignable : IRedDotObject, IProjectObject
    {
    }

    public interface IWorkflowAssignments
    {
        void CreateAndConnectContentWorkflow(string workflowName, params string[] languageVariants);

        void CreateAndConnectContentWorkflow(string workflowName, IEnumerable<ILanguageVariant> languageVariants);

        void CreateAndConnectStructuralworkflow(string workflowName);
        void DisconnectAllWorkflows();
        IPreassignedWorkflow GetContentWorkflowFor(string languageVariantId);
        IPreassignedWorkflow GetContentWorkflowFor(ILanguageVariant languageVariant);
        IPreassignedWorkflow GetContentWorkflowForCurrentLanguageVariant();
        void InvalidateCache();
        void SetContentWorkflow(IWorkflow workflow, IEnumerable<ILanguageVariant> languageVariants);
        void SetContentWorkflow(IWorkflow workflow, params string[] languageVariantIds);
        IPreassignedWorkflow StructuralWorkflow { get; set; }
    }

    internal class WorkflowAssignments : IWorkflowAssignments
    {
        private readonly IWorkflowAssignable _element;

        private readonly Dictionary<ILanguageVariant, IPreassignedWorkflow> _preassignedContentWorkflows =
            new Dictionary<ILanguageVariant, IPreassignedWorkflow>();

        private bool _isStructuralWorkflowLoaded;
        private PreassignedWorkflow _preassignedStructuralWorkflow;

        internal WorkflowAssignments(IWorkflowAssignable element)
        {
            _element = element;
        }

        public void CreateAndConnectContentWorkflow(string workflowName, params string[] languageVariants)
        {
            CreateAndConnectContentWorkflow(workflowName,
                                            languageVariants.Select(
                                                curVariant => _element.Project.LanguageVariants[curVariant]));
        }

        public void CreateAndConnectContentWorkflow(string workflowName, IEnumerable<ILanguageVariant> languageVariants)
        {
            const bool IS_STRUCTURAL_WORKFLOW = false;
            ExecuteCreateAndPreassignWorkflow(workflowName, languageVariants, IS_STRUCTURAL_WORKFLOW);
        }

        public void CreateAndConnectStructuralworkflow(string workflowName)
        {
            const bool IS_STRUCTURAL_WORKFLOW = true;
            ExecuteCreateAndPreassignWorkflow(workflowName, new[] {_element.Project.LanguageVariants.Current},
                                              IS_STRUCTURAL_WORKFLOW);
        }

        public void DisconnectAllWorkflows()
        {
            if (StructuralWorkflow != null)
            {
                StructuralWorkflow.DisconnectFromLinkCompletely();
            }
            var workflows = new HashSet<Guid>();
            foreach (var curLang in _element.Project.LanguageVariants)
            {
                var workflow = GetContentWorkflowFor(curLang);
                if (workflow == null)
                {
                    continue;
                }
                if (workflows.Contains(workflow.Guid))
                {
                    continue;
                }
                workflow.DisconnectFromLinkCompletely();
                workflows.Add(workflow.Guid);
            }
            InvalidateCache();
        }

        public IPreassignedWorkflow GetContentWorkflowFor(string languageVariantId)
        {
            ILanguageVariant languageVariant = _element.Project.LanguageVariants[languageVariantId];
            return GetContentWorkflowFor(languageVariant);
        }

        public IPreassignedWorkflow GetContentWorkflowFor(ILanguageVariant languageVariant)
        {
            IPreassignedWorkflow workflow;
            if (!_preassignedContentWorkflows.TryGetValue(languageVariant, out workflow))
            {
                workflow = GetPreassignedContentWorkflow(languageVariant);

                if (workflow == null)
                {
                    _preassignedContentWorkflows[languageVariant] = null;
                    return null;
                }

                foreach (var curLanguage in workflow.LanguageVariantsPreassignedTo)
                {
                    _preassignedContentWorkflows[curLanguage] = workflow;
                }
            }
            return workflow;
        }

        public IPreassignedWorkflow GetContentWorkflowForCurrentLanguageVariant()
        {
            return GetContentWorkflowFor(_element.Project.LanguageVariants.Current);
        }

        public void InvalidateCache()
        {
            _preassignedStructuralWorkflow = null;
            _isStructuralWorkflowLoaded = false;
            _preassignedContentWorkflows.Clear();
        }

        public void SetContentWorkflow(IWorkflow workflow, IEnumerable<ILanguageVariant> languageVariants)
        {
            if (workflow.IsStructureWorkflow)
            {
                throw new SmartAPIException(_element.Session.ServerLogin,
                                            "Workflow for preassignment is a structural workflow, although a content workflow is expected");
            }

            ExecutePreassignWorkflow(workflow, languageVariants);
        }

        public void SetContentWorkflow(IWorkflow workflow, params string[] languageVariantIds)
        {
            var languageVariants =
                languageVariantIds.Select(
                    curLanguageVariantId => _element.Project.LanguageVariants[curLanguageVariantId]);
            SetContentWorkflow(workflow, languageVariants);
        }

        public IPreassignedWorkflow StructuralWorkflow
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
            set
            {
                if (!value.IsStructureWorkflow)
                {
                    throw new SmartAPIException(_element.Session.ServerLogin,
                                                "Workflow for preassignment is not a structural workflow");
                }

                ExecutePreassignWorkflow(value, null);
            }
        }

        private void ExecuteCreateAndPreassignWorkflow(string workflowName,
                                                       IEnumerable<ILanguageVariant> languageVariants, bool isStructural)
        {
            const string CREATE_AND_ASSIGN_WORKFLOW =
                @"<WORKFLOW sessionkey=""{0}""><LINK guid=""{1}"" action=""assign""><WORKFLOW action=""addnew"" structureworkflow=""{2}"" guid="""" name=""{3}""><LANGUAGEVARIANTS>{4}</LANGUAGEVARIANTS></WORKFLOW></LINK></WORKFLOW>";
            var session = _element.Project.Session;
            string query = CREATE_AND_ASSIGN_WORKFLOW.RQLFormat(session.SessionKey, _element, isStructural, workflowName,
                                                                languageVariants);
            session.ExecuteRQLRaw(query, RQL.IODataFormat.LogonGuidOnly);
        }

        private PreassignedWorkflow ExecuteLoadWorkflow(string LOAD_WORKFLOW)
        {
            IProject project = _element.Project;
            var xmlDoc = project.ExecuteRQL(LOAD_WORKFLOW.RQLFormat(project.Session.SessionKey, _element));
            var workflowElement = (XmlElement) xmlDoc.SelectSingleNode("//WORKFLOW[@guid!='']");

            return workflowElement != null
                       ? new PreassignedWorkflow(_element, new Workflow(project, workflowElement))
                       : null;
        }

        private void ExecutePreassignWorkflow(IWorkflow workflow, IEnumerable<ILanguageVariant> languageVariants)
        {
            const string PREASSIGN_WORKFLOW =
                @"<WORKFLOW sessionkey=""{0}""><LINK guid=""{1}"" action=""assign""><WORKFLOW action=""addnew"" structureworkflow=""{2}"" guid=""{3}"" name=""{4}""><LANGUAGEVARIANTS>{5}</LANGUAGEVARIANTS></WORKFLOW></LINK></WORKFLOW>";

            var session = _element.Project.Session;
            session.ExecuteRQLRaw(
                PREASSIGN_WORKFLOW.RQLFormat(session.SessionKey, _element, workflow.IsStructureWorkflow, workflow,
                                             workflow.Name, languageVariants), RQL.IODataFormat.LogonGuidOnly);

            InvalidateCache();
        }

        private PreassignedWorkflow GetPreassignedContentWorkflow(ILanguageVariant languageVariant)
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

        private PreassignedWorkflow LoadStructuralWorkflow()
        {
            const string LOAD_WORKFLOW =
                @"<WORKFLOW sessionkey=""{0}""><LINK guid=""{1}""><WORKFLOW action=""load"" structureworkflow=""1""/></LINK></WORKFLOW>";
            return ExecuteLoadWorkflow(LOAD_WORKFLOW);
        }
    }
}