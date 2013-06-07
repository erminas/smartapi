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

namespace erminas.SmartAPI.CMS.Project
{
    public interface ISyllables : IIndexedRDList<string, ISyllable>, IProjectObject
    {
    }

    internal class Syllables : NameIndexedRDList<ISyllable>, ISyllables
    {
        private readonly IProject _project;

        internal Syllables(IProject project, Caching caching) : base(caching)
        {
            RetrieveFunc = GetSyllables;
            _project = project;
        }

        public IProject Project
        {
            get { return _project; }
        }

        public ISession Session
        {
            get { return _project.Session; }
        }

        private List<ISyllable> GetSyllables()
        {
            var xmlDoc = Project.ExecuteRQL(@"<SYLLABLES action=""list""/>", RqlType.SessionKeyInProject);
            var syllablelist = xmlDoc.GetElementsByTagName("SYLLABLE");
            return (from XmlElement curNode in syllablelist select (ISyllable) new Syllable(Project, curNode)).ToList();
        }
    }
}