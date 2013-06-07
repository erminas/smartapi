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