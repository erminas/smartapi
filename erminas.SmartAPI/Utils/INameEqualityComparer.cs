using System.Collections.Generic;
using erminas.SmartAPI.CMS;

namespace erminas.SmartAPI.Utils
{
    public class NameEqualityComparer<T> : IEqualityComparer<T> where T :IRedDotObject
    {
        public bool Equals(T x, T y)
        {
            return Equals(x.Name, y.Name);
        }

        public int GetHashCode(T obj)
        {
            return obj.Name.GetHashCode();
        }
    }
}
