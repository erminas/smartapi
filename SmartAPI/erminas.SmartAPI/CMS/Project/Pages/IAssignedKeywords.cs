using System.Collections.Generic;
using erminas.SmartAPI.CMS.Project.Keywords;
using erminas.SmartAPI.Utils.CachedCollections;

namespace erminas.SmartAPI.CMS.Project.Pages
{
    public interface IAssignedKeywords : IRDList<IKeyword>
    {
        void Add(IKeyword keyword);
        void AddRange(IEnumerable<IKeyword> keywords);
        void Remove(IKeyword keyword);
        void Clear();
        void Set(IEnumerable<IKeyword> keywords);
    }
}
