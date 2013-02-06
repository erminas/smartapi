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
using System.Collections.Generic;

namespace erminas.SmartAPI.Utils
{
    /// <summary>
    ///     Interface for collections of objects retrieved from the RedDot server with transparent caching. If caching is enabled the list of objects is retrieved from the server and subsequent access is done on the cache of this list. If caching is disabled the list of objects is retrieved from the server every time it gets accessed.
    /// </summary>
    /// <typeparam name="T"> TypeId of objects in the list </typeparam>
    public interface ICachedList<out T> : IEnumerable<T>, ICaching where T : class
    {
        int Count { get; }

        /// <summary>
        ///     Get an element of the list at a specific position.
        /// </summary>
        /// <param name="pos"> Position of the element in the list </param>
        /// <returns> Element at position pos </returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown, if pos is not a valid index></exception>
        T GetByPosition(int pos);

        /// <summary>
        ///     True == caching is enabled False == caching is disabled
        /// </summary>
        bool IsCachingEnabled { get; set; }

        ICachedList<T> Refreshed();
    }
}