using System.Collections.Generic;
using erminas.SmartAPI.Utils.CachedCollections;

namespace erminas.SmartAPI.CMS.Project.Pages.Elements
{
    internal class MultiLinkConnections : LinkConnections, IMultilinkConnections
    {
        internal MultiLinkConnections(ILinkElement element, Caching caching) : base(element, caching)
        {
        }

        public void Add(IPage page)
        {
            SaveConnection(page);
        }

        public void AddRange(IEnumerable<IPage> pages)
        {
            foreach (var curPage in pages)
            {
                SaveConnection(curPage);
            }
        }

        public new void RemoveRange(IEnumerable<IPage> pages)
        {
            base.RemoveRange(pages);
        }

        public override void Set(ILinkTarget target, Linking linking)
        {
            if (linking == Linking.AsConnection)
            {
                Clear();
            }
            base.Set(target, linking);
        }
    }

    public interface IMultilinkConnections : ILinkConnections
    {
        void Add(IPage page);
        void AddRange(IEnumerable<IPage> pages);
        void RemoveRange(IEnumerable<IPage> pages);
    }
}