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

using System.Collections.Generic;
using System.Xml;
using erminas.SmartAPI.CMS.Project.Workflows;

namespace erminas.SmartAPI.CMS.Project.ContentClasses.Elements
{
    internal abstract class AbstractWorkflowAssignments : ContentClassElement, IWorkflowAssignments
    {
        private readonly WorkflowAssignments _workflowAssignments;

        protected AbstractWorkflowAssignments(IContentClass contentClass, XmlElement xmlElement)
            : base(contentClass, xmlElement)
        {
            _workflowAssignments = new WorkflowAssignments(this);
        }

        public void CreateAndConnectContentWorkflow(string workflowName, params string[] languageVariants)
        {
            _workflowAssignments.CreateAndConnectContentWorkflow(workflowName, languageVariants);
        }

        public void CreateAndConnectContentWorkflow(string workflowName, IEnumerable<ILanguageVariant> languageVariants)
        {
            _workflowAssignments.CreateAndConnectContentWorkflow(workflowName, languageVariants);
        }

        public void CreateAndConnectStructuralworkflow(string workflowName)
        {
            _workflowAssignments.CreateAndConnectStructuralworkflow(workflowName);
        }

        public void DisconnectAllWorkflows()
        {
            _workflowAssignments.DisconnectAllWorkflows();
        }

        public IPreassignedWorkflow GetContentWorkflowFor(string languageVariantId)
        {
            return _workflowAssignments.GetContentWorkflowFor(languageVariantId);
        }

        public IPreassignedWorkflow GetContentWorkflowFor(ILanguageVariant languageVariant)
        {
            return _workflowAssignments.GetContentWorkflowFor(languageVariant);
        }

        public IPreassignedWorkflow GetContentWorkflowForCurrentLanguageVariant()
        {
            return _workflowAssignments.GetContentWorkflowForCurrentLanguageVariant();
        }

        public void InvalidateCache()
        {
            _workflowAssignments.InvalidateCache();
        }

        public void SetContentWorkflow(IWorkflow workflow, IEnumerable<ILanguageVariant> languageVariants)
        {
            _workflowAssignments.SetContentWorkflow(workflow, languageVariants);
        }

        public void SetContentWorkflow(IWorkflow workflow, params string[] languageVariantIds)
        {
            _workflowAssignments.SetContentWorkflow(workflow, languageVariantIds);
        }

        public IPreassignedWorkflow StructuralWorkflow
        {
            get { return _workflowAssignments.StructuralWorkflow; }
            set { _workflowAssignments.StructuralWorkflow = value; }
        }
    }
}