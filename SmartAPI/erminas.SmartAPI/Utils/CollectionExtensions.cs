// SmartAPI - .Net programmatic access to RedDot servers
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
using System.Linq;

namespace erminas.SmartAPI.Utils
{
    public static class CollectionExtensions
    {
        /// <summary>
        ///     Check, if the dictionary contains a key and return the value, if it does. If it doesn't, the method creates a new value via the delegate
        ///     <see
        ///         cref="value" />
        ///     , stores it in the dictionary under <see cref="key" /> and returns it.
        /// </summary>
        /// <typeparam name="TKey"> Type of keys in the dictionary </typeparam>
        /// <typeparam name="TValue"> Type of the values in the dictionary </typeparam>
        /// <param name="dictionary"> The dictionary </param>
        /// <param name="key"> Key to lookup/store </param>
        /// <param name="value"> Delegate which optionally returns/creates the value, if none is found in the dictionary </param>
        /// <returns> The retrieved or newly created value </returns>
        public static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key,
                                                    Func<TValue> value)
        {
            TValue result;
            if (!dictionary.TryGetValue(key, out result))
            {
                result = value();
                dictionary.Add(key, result);
            }
            return result;
        }

        public static bool SetEquals<T>(this IEnumerable<T> first, IEnumerable<T> second)
        {
            var firstList = first as IList<T> ?? first.ToList();
            var secondList = second as IList<T> ?? second.ToList();

            return !firstList.Except(secondList).Any() && !secondList.Except(firstList).Any();
        }
    }
}