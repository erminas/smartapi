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
using erminas.SmartAPI.Utils;
using erminas.SmartAPI.Utils.CachedCollections;

namespace erminas.SmartAPI.CMS.Project.ContentClasses
{
    public interface IPageDefinitions : IRDList<IPageDefinition>, IProjectObject
    {
        IContentClass ContentClass { get; }
    }

    internal class PageDefinitions : RDList<IPageDefinition>, IPageDefinitions
    {
        private readonly IContentClass _contentClass;

        internal PageDefinitions(IContentClass contentClass, Caching caching) : base(caching)
        {
            _contentClass = contentClass;
            RetrieveFunc = GetPageDefinitions;
        }

        public IContentClass ContentClass
        {
            get { return _contentClass; }
        }

        public IProject Project
        {
            get { return _contentClass.Project; }
        }

        public ISession Session
        {
            get { return _contentClass.Session; }
        }

        private List<IPageDefinition> GetPageDefinitions()
        {
            const string LOAD_PREASSIGNMENT = @"<TEMPLATELIST action=""load"" withpagedefinitions=""1""/>";

            var xmlDoc = Project.ExecuteRQL(LOAD_PREASSIGNMENT);
            const string PAGE_DEFINITIONS_XPATH = "//TEMPLATE[@guid='{0}']/PAGEDEFINITIONS/PAGEDEFINITION";
            var pageDefs = xmlDoc.SelectNodes(PAGE_DEFINITIONS_XPATH.RQLFormat(_contentClass));

            return
                (from XmlElement curPageDef in pageDefs select new PageDefinition(_contentClass, curPageDef))
                    .Cast<IPageDefinition>().ToList();
        }
    }
}