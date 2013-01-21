using System.Collections.Generic;
using System.Xml;
using erminas.SmartAPI.CMS;

namespace erminas.SmartAPI
{
    public abstract class AbstractWorkflowPreassignable : CCElement
    {
        private readonly WorkflowPreassignable _workflowPreassignable;
        internal void CreateAndPreassignContentWorkflow(string workflowName, IEnumerable<LanguageVariant> languageVariants)
        {
            _workflowPreassignable.CreateAndPreassignContentWorkflow(workflowName, languageVariants);
        }

        public void CreateAndPreassignContentWorkflow(string workflowName, params string[] languageVariants)
        {
            _workflowPreassignable.CreateAndPreassignContentWorkflow(workflowName, languageVariants);
        }

        internal void CreateAndPreassignStructuralworkflow(string workflowName)
        {
            _workflowPreassignable.CreateAndPreassignStructuralworkflow(workflowName);
        }

        public void DisconnectAllWorkflows()
        {
            _workflowPreassignable.DisconnectAllWorkflows();
        }

        internal PreassignedWorkflow GetPreassignedContentWorkflowFor(string languageVariantId)
        {
            return _workflowPreassignable.GetPreassignedContentWorkflowFor(languageVariantId);
        }

        internal PreassignedWorkflow GetPreassignedContentWorkflowFor(LanguageVariant languageVariant)
        {
            return _workflowPreassignable.GetPreassignedContentWorkflowFor(languageVariant);
        }

        internal PreassignedWorkflow GetPreassignedContentWorkflowForCurrentLanguageVariant()
        {
            return _workflowPreassignable.GetPreassignedContentWorkflowForCurrentLanguageVariant();
        }

        internal void PreassignContentWorkflow(Workflow workflow, IEnumerable<LanguageVariant> languageVariants)
        {
            _workflowPreassignable.PreassignContentWorkflow(workflow, languageVariants);
        }

        internal void PreassignContentWorkflow(Workflow workflow, params string[] languageVariantIds)
        {
            _workflowPreassignable.PreassignContentWorkflow(workflow, languageVariantIds);
        }

        internal PreassignedWorkflow PreassignedStructuralWorkflow
        {
            get { return _workflowPreassignable.PreassignedStructuralWorkflow; }
        }

        internal void PreassignStructuralWorkflow(Workflow workflow)
        {
            _workflowPreassignable.PreassignStructuralWorkflow(workflow);
        }

        protected AbstractWorkflowPreassignable(ContentClass contentClass, XmlElement xmlElement) : base(contentClass, xmlElement)
        {
            _workflowPreassignable = new WorkflowPreassignable(this); ;
        }


    }
}
