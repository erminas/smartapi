using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using erminas.SmartAPI.Utils;
using erminas.SmartAPI.Utils.CachedCollections;

namespace erminas.SmartAPI.CMS.Project.Pages.Elements
{
    public enum Linking
    {
        AsConnection,
        AsReference
    }

    internal class LinkConnections : RDList<IPage>, ILinkConnections
    {
        private readonly ILinkElement _element;
        private bool _isNeedingRefresh;

        protected internal LinkConnections(ILinkElement element, Caching caching) : base(caching)
        {
            _element = element;
            RetrieveFunc = GetConnectedPages;
        }

        public void Clear()
        {
            if (IsReferencing)
            {
                SetReference(null);
            }
            else
            {
                RemoveRange(this);
            }
        }

        public bool IsReference
        {
            get { return _element.LinkType == LinkType.Reference; }
        }

        public bool IsReferencing
        {
            get { return LinkType == LinkType.Reference; }
        }

        public LinkType LinkType
        {
            get
            {
                if (_isNeedingRefresh)
                {
                    _element.Refresh();
                    _isNeedingRefresh = false;
                }
                return _element.LinkType;
            }
        }

        public IProject Project
        {
            get { return _element.Project; }
        }

        public ILinkTarget Reference
        {
            get { throw new NotImplementedException(); }
            set { SetReference(value); }
        }

        public void Remove(IPage page)
        {
            RemoveRange(new[] {page});
        }

        public ISession Session
        {
            get { return _element.Session; }
        }

        public virtual void Set(ILinkTarget target, Linking linking)
        {
            if (linking == Linking.AsReference)
            {
                Reference = target;
            }
            else
            {
                SaveConnection((IPage) target);
            }
        }

        public void Connect(IPage page)
        {
            SaveConnection(page);
        }

        protected void RemoveRange(IEnumerable<IPage> pages)
        {
            const string DISCONNECT_PAGES = @"<LINK action=""save"" guid=""{0}""><PAGES>{1}</PAGES></LINK>";
            const string SINGLE_PAGE = @"<PAGE deleted=""1"" guid=""{0}"" />";

            var pagesStr = pages.Aggregate("", (x, page) => x + string.Format(SINGLE_PAGE, StringConversion.ToRQLString((Guid) page.Guid)));
            Project.ExecuteRQL(DISCONNECT_PAGES.RQLFormat(_element, pagesStr));
            InvalidateCache();
        }

        protected void SaveConnection(IPage page)
        {
            const string CONNECT_PREPARE =
                @"<LINK action=""save"" reddotcacheguid="""" guid=""{0}"" value=""" + RQL.SESSIONKEY_PLACEHOLDER +
                @""" />";

            Project.ExecuteRQL(CONNECT_PREPARE.RQLFormat(_element));

            const string CONNECT =
                @"<LINKSFROM action=""save"" pageid="""" pageguid=""{0}"" reddotcacheguid=""""><LINK guid=""{1}""/></LINKSFROM>";

            Project.ExecuteRQL(CONNECT.RQLFormat(page, _element));

            InvalidateCache();
            if (LinkType == LinkType.Reference)
            {
                _isNeedingRefresh = true;
            }
        }

        private List<IPage> GetConnectedPages()
        {
            const string LIST_LINKED_PAGES = @"<LINK guid=""{0}""><PAGES action=""list"" /></LINK>";
            var xmlDoc = Project.ExecuteRQL(LIST_LINKED_PAGES.RQLFormat(_element));
            return (from XmlElement curPage in xmlDoc.GetElementsByTagName("PAGE")
                    let page =
                        (IPage)
                        new Page(Project, curPage.GetGuid(), _element.LanguageVariant)
                            {
                                Id = curPage.GetIntAttributeValue("id").GetValueOrDefault(),
                                Headline = curPage.GetAttributeValue("headline")
                            }
                    select page).ToList();
        }

        private void ReferenceElement(ILinkElement element)
        {
            const string LINK_TO_ELEMENT =
                @"<PAGE><LINK action=""assign"" guid=""{0}""><LINK guid=""{1}"" /></LINK></PAGE>";
            //we can't really check the success, because an empty iodata element is returned on success as on (at least some) errors
            Project.ExecuteRQL(LINK_TO_ELEMENT.RQLFormat(_element, element));

            InvalidateCache();
            if (LinkType != LinkType.Reference)
            {
                _isNeedingRefresh = true;
            }
        }

        private void ReferencePage(IPage target)
        {
            const string LINK_TO_PAGE =
                @"<PAGE><LINK action=""reference"" guid=""{0}""><PAGE guid=""{1}"" /></LINK></PAGE>";
            //we can't really check the success, because an empty iodata element is returned on success as on (at least some) errors
            Project.ExecuteRQL(LINK_TO_PAGE.RQLFormat(_element, target));

            InvalidateCache();
            if (LinkType != LinkType.Reference)
            {
                _isNeedingRefresh = true;
            }
        }

        private void SetReference(ILinkTarget target)
        {
            if (target == null)
            {
                UnlinkReference();
            }
            else
            {
                if (target is IPage)
                {
                    ReferencePage((IPage) target);
                }
                else
                {
                    ReferenceElement((ILinkElement) target);
                }
            }
        }

        private void UnlinkReference()
        {
            const string UNLINK_ELEMENT =
                @"<LINK guid=""{0}""><LINK action=""unlink"" reddotcacheguid=""""/><URL action=""unlink""/></LINK>";
            //we can't really check the success, because an empty iodata element is returned on success as on (at least some) errors
            Project.ExecuteRQL(UNLINK_ELEMENT.RQLFormat(_element));

            InvalidateCache();
            _isNeedingRefresh = true;
        }
    }

    public interface ILinkConnections : IRDList<IPage>, IProjectObject
    {
        void Clear();

        bool IsReferencing { get; }
        LinkType LinkType { get; }
        ILinkTarget Reference { get; set; }
        void Remove(IPage page);
        void Set(ILinkTarget page, Linking linking);
        void Connect(IPage page);
    }
}