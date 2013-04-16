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
using erminas.SmartAPI.CMS.Project.Workflows;
using erminas.SmartAPI.Utils.CachedCollections;

namespace erminas.SmartAPI.CMS.Project
{
    public interface IProjectWorkflows : IIndexedRDList<string, IWorkflow>
    {
        IGlobalWorkflows Global { get; }
    }

    internal class ProjectWorkflow : NameIndexedRDList<IWorkflow>, IProjectWorkflows
    {
        private readonly IProject _project;

        internal ProjectWorkflow(IProject project, Caching caching) : base(caching)
        {
            _project = project;
            Global = new GlobalWorkflows(project);
            RetrieveFunc = GetWorkflows;
        }

        public IGlobalWorkflows Global { get; private set; }

        private List<IWorkflow> GetWorkflows()
        {
            const string LIST_WORKFLOWS = @"<WORKFLOWS action=""list"" listglobalworkflow=""1""/>";
            var xmlDoc = _project.ExecuteRQL(LIST_WORKFLOWS);
            return (from XmlElement curWorkflow in xmlDoc.GetElementsByTagName("WORKFLOW")
                    select (IWorkflow) new Workflow(_project, curWorkflow)).ToList();
        }
    }
}