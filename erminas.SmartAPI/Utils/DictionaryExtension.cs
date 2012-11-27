using System;
using System.Collections.Generic;

namespace erminas.Utilities
{
    public static class DictionaryExtension
    {
        /// <summary>
        /// Check, if the dictionary contains a key and return the value, if it does.
        /// If it doesn't, the method creates a new value via the delegate <see cref="value"/>,
        /// stores it in the dictionary under <see cref="key"/> and returns it.
        /// </summary>
        /// <typeparam name="TKey">Type of keys in the dictionary</typeparam>
        /// <typeparam name="TValue">Type of the values in the dictionary</typeparam>
        /// <param name="dictionary">The dictionary</param>
        /// <param name="key">Key to lookup/store</param>
        /// <param name="value">Delegate which optionally returns/creates the value, if none is found in the dictionary</param>
        /// <returns>The retrieved or newly created value</returns>
        public static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey,TValue> dictionary, TKey key, Func<TValue> value)
        {
            TValue result;
            if (!dictionary.TryGetValue(key, out result))
            {
                result = value();
                dictionary.Add(key, result);
            }
            return result;
        }
    }
}
