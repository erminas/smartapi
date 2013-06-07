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
using System.Linq;
using System.Xml;
using erminas.SmartAPI.Utils.CachedCollections;

namespace erminas.SmartAPI.CMS.Project.Workflows
{
    internal class WorkflowActions : RDList<IWorkFlowAction>, IWorkflowActions
    {
        private readonly Workflow _workflow;

        internal WorkflowActions(Workflow workflow, Caching caching) : base(caching)
        {
            _workflow = workflow;
            RetrieveFunc = GetWorkflowActions;
        }

        public IProject Project
        {
            get { return _workflow.Project; }
        }

        public ISession Session
        {
            get { return _workflow.Session; }
        }

        public IWorkflow Workflow
        {
            get { return _workflow; }
        }

        private List<IWorkFlowAction> GetWorkflowActions()
        {
            return (from XmlElement node in _workflow.RetrieveObjectInternal().SelectNodes("descendant::NODE")
                    select (IWorkFlowAction) new WorkFlowAction(Project, node)).ToList();
        }
    }

    public interface IWorkflowActions : IRDList<IWorkFlowAction>, IProjectObject
    {
        IWorkflow Workflow { get; }
    }
}