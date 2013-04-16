using System;
using erminas.SmartAPI.CMS;

namespace erminas.SmartAPI.Utils.CachedCollections
{
    public static class RDListExtensions
    {
        public static void DeleteIfExists<T>(this IRDList<T> list, string name) where T : class, IRedDotObject, IDeletable
        {
            T value;
            if (list.TryGetByName(name, out value))
            {
                value.Delete();
            }
        }
    }
}
