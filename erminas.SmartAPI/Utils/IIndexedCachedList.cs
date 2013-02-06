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
    ///     An IRDList which additionally indexes its content and provides a method to retrieve content through the index.
    /// </summary>
    /// <typeparam name="T"> (Super)type of stored elements </typeparam>
    /// <typeparam name="TK"> TypeId of the index keys </typeparam>
    public interface IIndexedCachedList<in TK, T> : ICachedList<T> where T : class
    {
        /// <summary>
        ///     Check wether the index contains a specific key. If you want to get an element, if it is available, use TryGet as it is faster than a call to ContainsKey and a Get afterwards.
        /// </summary>
        /// <returns>true, if an entry with the specified key is available, false otherwise</returns>
        bool ContainsKey(TK key);

        /// <summary>
        ///     Get an entry with a specific key value. Throws an exception if entry could not be found.
        /// </summary>
        /// <param name="key"> Key of the entry to get </param>
        /// <exception cref="KeyNotFoundException">Thrown if collection is empty or no element with key could be found</exception>
        /// <exception cref="ArgumentNullException">Thrown if key == null</exception>
        T Get(TK key);

        /// <summary>
        ///     Same as GetByName only as indexer.
        /// </summary>
        T this[TK key] { get; }

        /// <summary>
        ///     Try to get an entry with a specific key value, returns false, if entry could not be found.
        /// </summary>
        /// <param name="name"> Name of the entry to get </param>
        /// <param name="obj"> Output parameter containing the entry on success </param>
        /// <returns> true, if entry could be found, false otherwise. </returns>
        bool TryGet(TK name, out T obj);
    }
}