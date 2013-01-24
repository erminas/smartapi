using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using erminas.SmartAPI.Exceptions;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS
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

        public Preassigned<IPageDefinition> PageDefinitions { get; private set; }

        public void InvalidateCache()
        {
            _isDirty = true;
        }

        public void Refresh()
        {
            XmlNodeList contentClasses, pageDefinitions;
            ExecuteLoadQuery(out contentClasses, out pageDefinitions);

            _contentClasses = ExtractContentClasses(contentClasses);
            _pageDefinitions = ExtractPageDefinitions(pageDefinitions);
        }

        private void Remove(ContentClass contentClass)
        {
            Set(ContentClasses.Except(new[] {contentClass}), PageDefinitions);
        }

        private void Remove(IPageDefinition pageDefinition)
        {
            Set(ContentClasses, PageDefinitions.Except(new[] {pageDefinition}));
        }

        private void Add(IPageDefinition pageDefinition)
        {
            Set(ContentClasses, PageDefinitions.Union(new[] {pageDefinition}));
        }

        private List<IPageDefinition> ExtractPageDefinitions(XmlNodeList pageDefinitions)
        {
            return (from XmlElement curPageDefinition in pageDefinitions
                    select
                        new PageDefinition(
                        new ContentClass(Element.ContentClass.Project, curPageDefinition.GetGuid("templateguid")),
                        curPageDefinition)).Cast<IPageDefinition>().ToList();
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

        private void ExecuteLoadQuery(out XmlNodeList contentClasses, out XmlNodeList pageDefinitions)
        {
            const string LOAD_PREASSIGNMENT =
                @"<TEMPLATELIST action=""load"" linkguid=""{0}"" assigntemplates=""1"" withpagedefinitions=""1""/>";

            var xmlDoc = Element.ContentClass.Project.ExecuteRQL(LOAD_PREASSIGNMENT.RQLFormat(Element));
            contentClasses = xmlDoc.SelectNodes("//TEMPLATE[@selectinnewpage='1']");
            pageDefinitions = xmlDoc.SelectNodes("//PAGEDEFINITION[@selectinnewpage='1']");
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

        public void Set(IEnumerable<ContentClass> contentClasses, IEnumerable<IPageDefinition> pageDefinitions)
        {
            var assignmentQuery = ToAssignmentQuery(contentClasses, pageDefinitions);
            ExecuteAssignmentQuery(assignmentQuery);

            InvalidateCache();
        }

        private void ExecuteAssignmentQuery(string assignmentQuery)
        {
            var xmlDoc = Element.ContentClass.Project.ExecuteRQL(assignmentQuery);

            if (!xmlDoc.InnerText.Contains("ok"))
            {
                throw new SmartAPIException(Element.ContentClass.Project.Session.ServerLogin, string.Format("Could not set presassigned content classes for {0}", Element));
            }
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

        private void Add(ContentClass contentClass)
        {
            Set(ContentClasses.Union(new List<ContentClass> {contentClass}), PageDefinitions);
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

        public IEnumerator<T> GetEnumerator()
        {
            return _parent.Get().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _parent.Get().GetEnumerator();
        }

        public void Set(IEnumerable<T> elements)
        {
            _parent.Set(elements);
        }

        public void Add(T element)
        {
            _parent.Add(element);
        }

        public void Remove(T element)
        {
            _parent.Remove(element);
        }
    }
}