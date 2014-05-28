using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using erminas.SmartAPI.CMS;

namespace erminas.SmartAPI.Utils
{
    public static class LinqExtension
    {
        public static void Delete<T>(this IEnumerable<T> collection) where T : IDeletable
        {
            foreach (var t in collection)
            {
                t.Delete();
            }
        }

        public static void Delete<T>(this IEnumerable<T> collection, Func<T, bool> predicate) where T : IDeletable
        {
            foreach (var t in collection.Where(predicate))
            {
                t.Delete();
            }
        }
    }
}
