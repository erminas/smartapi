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
using erminas.SmartAPI.CMS;

namespace erminas.SmartAPI.Utils.CachedCollections
{
    /// <summary>
    ///     A IIndexedCachedList for IRedDotObjects which provides convenience functions to get objects by guid and name. Those functions don't necessarily use an index, so if you need indexed access through guid, use the Guid as key in a
    ///     <see
    ///         cref="IndexedCachedList{TK,T}" />
    ///     . If you need indexed access through name, use <see cref="NameIndexedRDList{T}" />
    /// </summary>
    /// <typeparam name="T"> </typeparam>
    public interface IRDList<T> : IRDEnumerable<T>, ICachedList<T> where T : class, IRedDotObject
    {
        new IRDList<T> Refreshed();

        new int Count { get; }
        /// <summary>
        ///     Same as <see cref="WaitFor" /> in <see cref="ICachedList{T}" /> but with an IRDList as predicate input.
        ///     Provided for convenience.
        /// </summary>
        void WaitFor(Predicate<IRDList<T>> predicate, TimeSpan wait, TimeSpan retryPeriod);
    }
}