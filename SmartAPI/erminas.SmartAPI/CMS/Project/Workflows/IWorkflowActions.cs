using System.Collections.Generic;
using System.Linq;
using System.Xml;
using erminas.SmartAPI.Utils.CachedCollections;

namespace erminas.SmartAPI.CMS.Project.Workflows
{
    internal class WorkflowActions : RDList<IWorkFlowAction>, IWorkflowActions
    {
        private readonly Workflow _workflow;

        public IWorkflow Workflow
        {
            get { return _workflow; }
        }

        internal WorkflowActions(Workflow workflow, Caching caching) : base(caching)
        {
            _workflow = workflow;
            RetrieveFunc = GetWorkflowActions;
        }

        private List<IWorkFlowAction> GetWorkflowActions()
        {
            return (from XmlElement node in _workflow.RetrieveObjectInternal().SelectNodes("descendant::NODE")
                    select (IWorkFlowAction) new WorkFlowAction(Project, node)).ToList();
        }

        public ISession Session { get { return _workflow.Session; } }
        public IProject Project { get { return _workflow.Project; } }
    }

    public interface IWorkflowActions : IRDList<IWorkFlowAction>, IProjectObject
    {
        IWorkflow Workflow { get; }
    }
}