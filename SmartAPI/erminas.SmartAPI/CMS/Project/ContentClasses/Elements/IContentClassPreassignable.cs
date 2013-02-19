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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using erminas.SmartAPI.Exceptions;
using erminas.SmartAPI.Utils;
using erminas.SmartAPI.Utils.CachedCollections;

namespace erminas.SmartAPI.CMS.Project.ContentClasses.Elements
{
    public interface IContentClassPreassignable : IContentClassElement
    {
        PreassignedContentClassesAndPageDefinitions PreassignedContentClasses { get; }
    }

    public class PreassignedContentClassesAndPageDefinitions : ICaching
    {
        public readonly IContentClassPreassignable Element;

        private List<ContentClass> _contentClasses;
        private bool _isDirty = true;
        private List<IPageDefinition> _pageDefinitions;

        internal PreassignedContentClassesAndPageDefinitions(IContentClassPreassignable element)
        {
            Element = element;
            var contentClassModifier = new Modifier<ContentClass>(GetContentClasses,
                                                                  contentClasses => Set(contentClasses, PageDefinitions),
                                                                  Add, Remove);
            ContentClasses = new Preassigned<ContentClass>(contentClassModifier);

            var pageDefinitionModifier = new Modifier<IPageDefinition>(GetPageDefinitions,
                                                                       pageDefinitions =>
                                                                       Set(ContentClasses, pageDefinitions), Add, Remove);
            PageDefinitions = new Preassigned<IPageDefinition>(pageDefinitionModifier);
        }

        public Preassigned<ContentClass> ContentClasses { get; private set; }

        public void InvalidateCache()
        {
            _isDirty = true;
        }

        public Preassigned<IPageDefinition> PageDefinitions { get; private set; }

        public void Refresh()
        {
            XmlNodeList contentClasses, pageDefinitions;
            ExecuteLoadQuery(out contentClasses, out pageDefinitions);

            _contentClasses = ExtractContentClasses(contentClasses);
            _pageDefinitions = ExtractPageDefinitions(pageDefinitions);
        }

        public void Set(IEnumerable<ContentClass> contentClasses, IEnumerable<IPageDefinition> pageDefinitions)
        {
            var assignmentQuery = ToAssignmentQuery(contentClasses, pageDefinitions);
            ExecuteAssignmentQuery(assignmentQuery);

            InvalidateCache();
        }

        private void Add(IPageDefinition pageDefinition)
        {
            Set(ContentClasses, PageDefinitions.Union(new[] {pageDefinition}));
        }

        private void Add(ContentClass contentClass)
        {
            Set(ContentClasses.Union(new List<ContentClass> {contentClass}), PageDefinitions);
        }

        private void ExecuteAssignmentQuery(string assignmentQuery)
        {
            var xmlDoc = Element.ContentClass.Project.ExecuteRQL(assignmentQuery);

            if (!xmlDoc.InnerText.Contains("ok"))
            {
                throw new SmartAPIException(Element.Session.ServerLogin,
                                            string.Format("Could not set presassigned content classes for {0}", Element));
            }
        }

        private void ExecuteLoadQuery(out XmlNodeList contentClasses, out XmlNodeList pageDefinitions)
        {
            const string LOAD_PREASSIGNMENT =
                @"<TEMPLATELIST action=""load"" linkguid=""{0}"" assigntemplates=""1"" withpagedefinitions=""1""/>";

            var xmlDoc = Element.ContentClass.Project.ExecuteRQL(LOAD_PREASSIGNMENT.RQLFormat(Element));
            contentClasses = xmlDoc.SelectNodes("//TEMPLATE[@selectinnewpage='1']");
            pageDefinitions = xmlDoc.SelectNodes("//PAGEDEFINITION[@selectinnewpage='1']");
        }

        private List<ContentClass> ExtractContentClasses(XmlNodeList contentClasses)
        {
            return (from XmlElement curContentClass in contentClasses
                    select
                        new ContentClass(Element.ContentClass.Project, curContentClass.GetGuid())
                            {
                                Name = curContentClass.GetName(),
                                Description = curContentClass.GetAttributeValue("description")
                            }).ToList();
        }

        private List<IPageDefinition> ExtractPageDefinitions(XmlNodeList pageDefinitions)
        {
            return (from XmlElement curPageDefinition in pageDefinitions
                    select
                        new PageDefinition(
                        new ContentClass(Element.ContentClass.Project, curPageDefinition.GetGuid("templateguid")),
                        curPageDefinition)).Cast<IPageDefinition>().ToList();
        }

        private IEnumerable<ContentClass> GetContentClasses()
        {
            if (_isDirty)
            {
                Refresh();
            }
            return _contentClasses;
        }

        private IEnumerable<IPageDefinition> GetPageDefinitions()
        {
            if (_isDirty)
            {
                Refresh();
            }
            return _pageDefinitions;
        }

        private void Remove(ContentClass contentClass)
        {
            Set(ContentClasses.Except(new[] {contentClass}), PageDefinitions);
        }

        private void Remove(IPageDefinition pageDefinition)
        {
            Set(ContentClasses, PageDefinitions.Except(new[] {pageDefinition}));
        }

        private string ToAssignmentQuery(IEnumerable<ContentClass> contentClasses,
                                         IEnumerable<IPageDefinition> pageDefinitions)
        {
            contentClasses = contentClasses ?? new List<ContentClass>();
            pageDefinitions = pageDefinitions ?? new List<IPageDefinition>();

            const string SINGLE_PREASSIGNMENT = @"<TEMPLATE guid=""{0}""/>";

            string assignments = contentClasses.Cast<IRedDotObject>()
                                               .Union(pageDefinitions)
                                               .Aggregate("", (s, o) => s + SINGLE_PREASSIGNMENT.RQLFormat(o));

            const string SET_PREASSIGNMENT_ENVELOPE =
                @"<TEMPLATE><ELEMENT action=""assign"" guid=""{0}""><TEMPLATES extendedrestriction=""{1}"">{2}</TEMPLATES></ELEMENT></TEMPLATE>";
            string assignmentQuery = SET_PREASSIGNMENT_ENVELOPE.RQLFormat(Element, false, assignments);
            return assignmentQuery;
        }

        //Also apply this restriction to connecting existing pages 
        internal class Modifier<T> where T : IRedDotObject
        {
            internal readonly Action<T> Add;
            internal readonly Func<IEnumerable<T>> Get;
            internal readonly Action<T> Remove;
            internal readonly Action<IEnumerable<T>> Set;

            internal Modifier(Func<IEnumerable<T>> get, Action<IEnumerable<T>> set, Action<T> add, Action<T> remove)
            {
                Get = get;
                Set = set;
                Add = add;
                Remove = remove;
            }
        }
    }

    public interface IPageDefinition : IRedDotObject
    {
        ContentClass ContentClass { get; }
    }

    public class Preassigned<T> : IEnumerable<T> where T : IRedDotObject
    {
        private readonly PreassignedContentClassesAndPageDefinitions.Modifier<T> _parent;

        internal Preassigned(PreassignedContentClassesAndPageDefinitions.Modifier<T> parent)
        {
            _parent = parent;
        }

        public void Add(T element)
        {
            _parent.Add(element);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _parent.Get().GetEnumerator();
        }

        public void Remove(T element)
        {
            _parent.Remove(element);
        }

        public void Set(IEnumerable<T> elements)
        {
            _parent.Set(elements);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _parent.Get().GetEnumerator();
        }
    }
}