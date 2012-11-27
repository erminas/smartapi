/*
 * Smart API - .Net programatical access to RedDot servers
 * Copyright (C) 2012  erminas GbR 
 *
 * This program is free software: you can redistribute it and/or modify it 
 * under the terms of the GNU General Public License as published by the Free Software Foundation,
 * either version 3 of the License, or (at your option) any later version.
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the GNU General Public License for more details. 
 *
 * You should have received a copy of the GNU General Public License along with this program.
 * If not, see <http://www.gnu.org/licenses/>. 
 */

using System;
using erminas.SmartAPI.CMS;

namespace erminas.SmartAPI.Utils
{
    /// <summary>
    ///   A IIndexedCachedList for IRedDotObjects which provides convenience functions to get objects by guid and name. Those functions don't necessarily use an index, so if you need indexed access through guid, use the Guid as key in a <see
    ///    cref="IndexedCachedList{TK,T}" /> . If you need indexed access through name, use <see cref="NameIndexedRDList{T}" />
    /// </summary>
    /// <typeparam name="T"> </typeparam>
    public interface IRDList<T> : ICachedList<T> where T : class, IRedDotObject
    {
        T GetByGuid(Guid guid);
        bool TryGetByGuid(Guid guid, out T output);

        /// <summary>
        ///   Get the first element with Name == name
        /// </summary>
        T GetByName(string name);

        /// <summary>
        ///   Try to get the first element with Name == name
        /// </summary>
        /// <returns> true, if an element could be found, false otherwise </returns>
        bool TryGetByName(string name, out T output);

        bool ContainsGuid(Guid guid);

        bool ContainsName(string name);
    }
}