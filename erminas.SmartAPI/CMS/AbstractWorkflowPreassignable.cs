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

namespace erminas.SmartAPI.CMS
{
    public abstract class AbstractWorkflowPreassignable : CCElement
    {
        private readonly WorkflowPreassignable _workflowPreassignable;

        protected AbstractWorkflowPreassignable(ContentClass contentClass, XmlElement xmlElement)
            : base(contentClass, xmlElement)
        {
            _workflowPreassignable = new WorkflowPreassignable(this);
        }

        public void CreateAndPreassignContentWorkflow(string workflowName, IEnumerable<LanguageVariant> languageVariants)
        {
            _workflowPreassignable.CreateAndPreassignContentWorkflow(workflowName, languageVariants);
        }

        public void CreateAndPreassignContentWorkflow(string workflowName, params string[] languageVariants)
        {
            _workflowPreassignable.CreateAndPreassignContentWorkflow(workflowName, languageVariants);
        }

        public void CreateAndPreassignStructuralworkflow(string workflowName)
        {
            _workflowPreassignable.CreateAndPreassignStructuralworkflow(workflowName);
        }

        public void DisconnectAllWorkflows()
        {
            _workflowPreassignable.DisconnectAllWorkflows();
        }

        public PreassignedWorkflow GetPreassignedContentWorkflowFor(string languageVariantId)
        {
            return _workflowPreassignable.GetPreassignedContentWorkflowFor(languageVariantId);
        }

        public PreassignedWorkflow GetPreassignedContentWorkflowFor(LanguageVariant languageVariant)
        {
            return _workflowPreassignable.GetPreassignedContentWorkflowFor(languageVariant);
        }

        public PreassignedWorkflow GetPreassignedContentWorkflowForCurrentLanguageVariant()
        {
            return _workflowPreassignable.GetPreassignedContentWorkflowForCurrentLanguageVariant();
        }

        public void PreassignContentWorkflow(Workflow workflow, IEnumerable<LanguageVariant> languageVariants)
        {
            _workflowPreassignable.PreassignContentWorkflow(workflow, languageVariants);
        }

        public void PreassignContentWorkflow(Workflow workflow, params string[] languageVariantIds)
        {
            _workflowPreassignable.PreassignContentWorkflow(workflow, languageVariantIds);
        }

        public void PreassignStructuralWorkflow(Workflow workflow)
        {
            _workflowPreassignable.PreassignStructuralWorkflow(workflow);
        }

        public PreassignedWorkflow PreassignedStructuralWorkflow
        {
            get { return _workflowPreassignable.PreassignedStructuralWorkflow; }
        }
    }
}