// Smart API - .Net programatical access to RedDot servers
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

namespace erminas.SmartAPI.Utils
{
    /// <summary>
    ///     Utility class to temporarily ensure that caching on a cached list is enabled/disabled and restore the caching state afterwards. On construction the caching state is set and on disposable the original state gets restored.
    /// </summary>
    /// <example>
    ///     In this example the caching for the ContentClasses list in a project gets disabled and then temporarily enabled for a specific scope through the use of a CachingContext.
    ///     <code>project.ContentClasses.IsCachingEnabled = false;
    ///                                                                                                                                                                               ...
    ///                                                                                                                                                                               using(new CachingContext&lt;ContentClass&gt;(project.ContentClasses, Caching.Enabled)) 
    ///                                                                                                                                                                               {
    ///                                                                                                                                                                               ...
    ///                                                                                                                                                                               }</code>
    /// </example>
    /// <typeparam name="T"> </typeparam>
    public sealed class CachingContext<T> : IDisposable where T : class
    {
        private readonly ICachedList<T> _cachedList;
        private readonly bool _wasCachingEnabled;

        public CachingContext(ICachedList<T> cachedList, Caching caching)
        {
            _cachedList = cachedList;
            _wasCachingEnabled = cachedList.IsCachingEnabled;
            _cachedList.IsCachingEnabled = caching == Caching.Enabled;
        }

        #region IDisposable Members

        public void Dispose()
        {
            _cachedList.IsCachingEnabled = _wasCachingEnabled;
        }

        #endregion
    }
}