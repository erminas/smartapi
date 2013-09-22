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
using System.Xml;
using erminas.SmartAPI.Exceptions;
using erminas.SmartAPI.Utils;
using erminas.SmartAPI.Utils.CachedCollections;

namespace erminas.SmartAPI.CMS.Project.Workflows
{
    public interface IWorkflow : IPartialRedDotObject, IProjectObject, IDeletable
    {
        IWorkflowActions Actions { get; }
        bool CanBeInherited { get; }
        bool IsGlobal { get; }
        bool IsStructureWorkflow { get; }
    }

    internal class Workflow : PartialRedDotProjectObject, IWorkflow
    {
        private bool _canBeInherited;
        private bool _isGlobal;
        private bool _isStructureWorkflow;

        internal Workflow(IProject project, XmlElement xmlElement) : base(project, xmlElement)
        {
            LoadXml();
            Actions = new WorkflowActions(this, Caching.Enabled);
        }

        public Workflow(IProject project, Guid guid) : base(project, guid)
        {
            Actions = new WorkflowActions(this, Caching.Enabled);
        }

        // TODO: Add a more useful action method which retains the 'flow' of the workflow!
        public IWorkflowActions Actions { get; private set; }

        public bool CanBeInherited
        {
            get { return LazyLoad(ref _canBeInherited); }
        }

        public void Delete()
        {
            const string DELETE_WORKFLOW = @"<WORKFLOW sessionkey=""{0}"" action=""delete"" guid=""{1}""/>";
            var session = Project.Session;
            var reply = session.ExecuteRQLRaw(DELETE_WORKFLOW.RQLFormat(session, this), RQL.IODataFormat.LogonGuidOnly);

            if (!reply.Contains("ok"))
            {
                throw new SmartAPIException(Session.ServerLogin, string.Format("Could not delete workflow {0}", this));
            }
        }

        public bool IsGlobal
        {
            get { return LazyLoad(ref _isGlobal); }
        }

        public bool IsStructureWorkflow
        {
            get { return LazyLoad(ref _isStructureWorkflow); }
        }

        protected override void LoadWholeObject()
        {
            LoadXml();
        }

        protected override XmlElement RetrieveWholeObject()
        {
            const string LOAD_WORKFLOW = @"<WORKFLOW action=""load"" guid=""{0}""/>";

            return
                (XmlElement)
                Project.ExecuteRQL(String.Format(LOAD_WORKFLOW, Guid.ToRQLString())).GetElementsByTagName("WORKFLOW")[0];
        }

        internal XmlElement RetrieveObjectInternal()
        {
            return RetrieveWholeObject();
        }

        private void LoadXml()
        {
            InitIfPresent(ref _canBeInherited, "inherit", BoolConvert);
            EnsuredInit(ref _isStructureWorkflow, "structureworkflow", BoolConvert);
            InitIfPresent(ref _isGlobal, "global", BoolConvert);
        }
    }
}