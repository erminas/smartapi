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

using System.Collections.Generic;
using System.Linq;
using System.Xml;
using erminas.SmartAPI.Exceptions;
using erminas.SmartAPI.Utils.CachedCollections;

namespace erminas.SmartAPI.CMS.Project
{
    public interface IProjectVariants : IIndexedRDList<string, IProjectVariant>, IProjectObject
    {
        /// <summary>
        ///     Get the project variant used as display format (preview).
        /// </summary>
        IProjectVariant DisplayFormat { get; }
    }

    internal class ProjectVariants : NameIndexedRDList<IProjectVariant>, IProjectVariants
    {
        private readonly IProject _project;

        internal ProjectVariants(IProject project, Caching caching) : base(caching)
        {
            _project = project;
            RetrieveFunc = GetProjectVariants;
        }

        public IProjectVariant DisplayFormat
        {
            get { return this.FirstOrDefault(x => x.IsUsedAsDisplayFormat); }
        }

        public IProject Project
        {
            get { return _project; }
        }

        public ISession Session
        {
            get { return _project.Session; }
        }

        private List<IProjectVariant> GetProjectVariants()
        {
            const string LIST_PROJECT_VARIANTS = @"<PROJECT><PROJECTVARIANTS action=""list""/></PROJECT>";
            XmlDocument xmlDoc = Project.ExecuteRQL(LIST_PROJECT_VARIANTS);
            var variants = xmlDoc.GetElementsByTagName("PROJECTVARIANTS")[0] as XmlElement;
            if (variants == null)
            {
                throw new SmartAPIException(Session.ServerLogin,
                                            string.Format("Could not load project variants of project {0}", this));
            }
            return (from XmlElement variant in variants.GetElementsByTagName("PROJECTVARIANT")
                    select (IProjectVariant) new ProjectVariant(Project, variant)).ToList();
        }
    }
}