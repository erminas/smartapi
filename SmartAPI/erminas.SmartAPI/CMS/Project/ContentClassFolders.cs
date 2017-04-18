using System.Collections.Generic;
using System.Linq;
using System.Xml;
using erminas.SmartAPI.CMS.Project.Folder;
using erminas.SmartAPI.Utils;
using erminas.SmartAPI.Utils.CachedCollections;

namespace erminas.SmartAPI.CMS.Project
{
    internal class ContentClassFolders : NameIndexedRDList<IContentClassFolder>, IContentClassFolders
    {
        private readonly Project _project;

        internal ContentClassFolders(Project project, Caching caching) : base(caching)
        {
            _project = project;
            RetrieveFunc = GetContentClassFolders;
            Broken = new NameIndexedRDList<IContentClassFolder>(GetBrokenFolders, Caching.Enabled);
        }

        public IIndexedRDList<string, IContentClassFolder> Broken { get; }

        public IProject Project
        {
            get { return _project; }
        }

        public ISession Session
        {
            get { return _project.Session; }
        }

        private List<IContentClassFolder> GetBrokenFolders()
        {
            const string TREE =
                @"<TREESEGMENT type=""project.4000"" action=""load"" guid=""4AF89E44535511D4BDAB004005312B7C"" descent=""app"" parentguid=""""/>";
            var result = Project.ExecuteRQL(TREE);
            var guids = this.Select(folder => folder.Guid)
                .ToList();
            return (from XmlElement element in result.GetElementsByTagName("SEGMENT")
                let curGuid = element.GetGuid()
                where !guids.Contains(curGuid)
                select (IContentClassFolder) new ContentClassFolder(_project, curGuid)
                {
                    Name = element.GetAttributeValue("value"),
                    IsBroken = true
                }).ToList();
        }

        private List<IContentClassFolder> GetContentClassFolders()
        {
            const string LIST_CC_FOLDERS_OF_PROJECT = @"<TEMPLATEGROUPS action=""load"" />";
            //TODO project.execute
            var xmlDoc = Session.ExecuteRQLInProjectContext(LIST_CC_FOLDERS_OF_PROJECT, _project.Guid);
            var xmlNodes = xmlDoc.GetElementsByTagName("GROUP");

            return
            (from XmlElement curNode in xmlNodes
                select (IContentClassFolder) new ContentClassFolder(_project, curNode)).ToList();
        }
    }
}