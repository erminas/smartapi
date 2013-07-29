using System.Collections.Generic;
using System.Linq;
using System.Xml;
using erminas.SmartAPI.Utils;
using erminas.SmartAPI.Utils.CachedCollections;

namespace erminas.SmartAPI.CMS.Project.ContentClasses
{
    internal class ContentClassVersions : RDList<IContentClassVersion>, IContentClassVersions
    {
        private readonly IContentClass _contentClass;

        internal ContentClassVersions(IContentClass contentClass, Caching caching) : base(caching)
        {
            _contentClass = contentClass;
            RetrieveFunc = GetVersions;
        }

        public IContentClass ContentClass
        {
            get { return _contentClass; }
        }

        /// <summary>
        ///     Versioning information for the latest version of the content class.
        /// </summary>
        public IContentClassVersion Current
        {
            get { return this.FirstOrDefault(); }
        }

        public IProject Project
        {
            get { return _contentClass.Project; }
        }

        public ISession Session
        {
            get { return _contentClass.Session; }
        }

        private List<IContentClassVersion> GetVersions()
        {
            const string LIST_VERSIONS =
                @"<PROJECT><TEMPLATE guid=""{0}""><ARCHIVE action=""list""/></TEMPLATE></PROJECT>";

            var xmlDoc = Project.ExecuteRQL(LIST_VERSIONS.RQLFormat(_contentClass));
            var versionNodes = xmlDoc.GetElementsByTagName("VERSION");

            return (from XmlElement curVersion in versionNodes
                    let cc = (IContentClassVersion) new ContentClass.ContentClassVersion(_contentClass, curVersion)
                    orderby cc.Date descending
                    select cc).ToList();
        }
    }

    public interface IContentClassVersions : IRDList<IContentClassVersion>, IProjectObject
    {
        IContentClassVersion Current { get; }
    }
}