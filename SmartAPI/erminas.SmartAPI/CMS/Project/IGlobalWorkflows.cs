using System;
using System.Collections.Generic;
using erminas.SmartAPI.CMS.Project.ContentClasses.Elements;
using erminas.SmartAPI.CMS.Project.Workflows;

namespace erminas.SmartAPI.CMS.Project
{
    public interface IGlobalWorkflows : IWorkflowAssignments, IProjectObject
    {
    }

    internal class GlobalWorkflows : IGlobalWorkflows
    {
        private class ProjectWrapper : IWorkflowAssignable
        {
            private readonly Project _project;

            internal ProjectWrapper(Project project)
            {
                _project = project;
            }

            public Guid Guid { get { return Project.Guid; } }
            public string Name { get { return Project.Name; } }
            public Session Session { get { return Project.Session; } }
            public Project Project { get { return _project; } }
        }

        private readonly Project _project;
        private readonly WorkflowAssignments _workflowAssignments;
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

        internal GlobalWorkflows(Project project)
        {
            _project = project;
            _workflowAssignments = new WorkflowAssignments(new ProjectWrapper(project));
        }

        public Session Session { get { return _project.Session; } }
        public Project Project { get { return _project; } }
    }
}
