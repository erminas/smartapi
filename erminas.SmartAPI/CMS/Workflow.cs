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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using erminas.SmartAPI.Exceptions;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS
{
    public interface IWorkflow : IPartialRedDotObject
    {
        IEnumerable<WorkFlowAction> Actions();
        bool CanBeInherited { get; }
        bool IsGlobal { get; }
        bool IsStructureWorkflow { get; }
    }

    public class Workflow : PartialRedDotObject, IWorkflow
    {
        public readonly Project Project;
        private IEnumerable<WorkFlowAction> _actions;
        private bool _canBeInherited;
        private bool _isGlobal;
        private bool _isStructureWorkflow;

        internal Workflow(Project project, XmlElement xmlElement) : base(xmlElement)
        {
            Project = project;
            LoadXml();
            //if we don't have any actions, the element might have been initialized with only partial information,
            //so a refresh is needed to access actions.
            //todo schoener waeren getrennte constructoren oder so
            if (!_actions.Any())
            {
                _actions = null;
            }
        }

        public Workflow(Project project, Guid guid) : base(guid)
        {
            Project = project;
        }

        // TODO: Add a more useful action method which retains the 'flow' of the workflow!
        public IEnumerable<WorkFlowAction> Actions()
        {
            if (_actions != null)
            {
                return _actions;
            }
            Refresh();
            return _actions;
        }

        public bool CanBeInherited
        {
            get { return LazyLoad(ref _canBeInherited); }
        }

        public void Delete()
        {
            const string DELETE_WORKFLOW = @"<WORKFLOW sessionkey=""{0}"" action=""delete"" guid=""{1}""/>";
            var session = Project.Session;
            var reply = session.ExecuteRql(DELETE_WORKFLOW.RQLFormat(session, this), Session.IODataFormat.LogonGuidOnly);

            if (!reply.Contains("ok"))
            {
                throw new SmartAPIException(Project.Session.ServerLogin,
                                            string.Format("Could not delete workflow {0}", this));
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
            _actions = null;
        }

        protected override XmlElement RetrieveWholeObject()
        {
            const string LOAD_WORKFLOW = @"<WORKFLOW action=""load"" guid=""{0}""/>";

            return
                (XmlElement)
                Project.ExecuteRQL(String.Format(LOAD_WORKFLOW, Guid.ToRQLString())).GetElementsByTagName("WORKFLOW")[0];
        }

        private void LoadActions()
        {
            _actions =
                (from XmlElement node in XmlElement.SelectNodes("descendant::NODE") select new WorkFlowAction(node))
                    .ToList();
        }

        private void LoadXml()
        {
            InitIfPresent(ref _canBeInherited, "inherit", BoolConvert);
            EnsuredInit(ref _isStructureWorkflow, "structureworkflow", BoolConvert);
            InitIfPresent(ref _isGlobal, "global", BoolConvert);
            LoadActions();
        }
    }
}