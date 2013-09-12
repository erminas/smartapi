using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using erminas.SmartAPI.CMS;

namespace erminas.SmartAPI.Utils.CachedCollections
{
    public static class PartialRedDotObjectExtension
    {

        public static void WaitFor<T>(this T partialRedDotObject, Predicate<T> predicate,
                                      TimeSpan wait, TimeSpan retryPeriod) where T : IPartialRedDotObject
        {
            Wait.For(() =>
                {
                    partialRedDotObject.Refresh();
                    return predicate(partialRedDotObject);
                }, wait, retryPeriod);
        }
    }
}
