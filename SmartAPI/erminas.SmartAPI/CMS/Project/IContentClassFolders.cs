using erminas.SmartAPI.CMS.Project.Folder;
using erminas.SmartAPI.Utils.CachedCollections;

namespace erminas.SmartAPI.CMS.Project
{
    public interface IContentClassFolders : IIndexedRDList<string, IContentClassFolder>, IProjectObject
    {
        IIndexedRDList<string, IContentClassFolder> Broken { get; }
    }
}