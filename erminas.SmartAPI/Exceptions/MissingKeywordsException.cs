using System;
using System.Collections.Generic;
using System.Linq;
using erminas.SmartAPI.CMS;

namespace erminas.SmartAPI.Exceptions
{
    [Serializable]
    public class MissingKeywordsException : PageStatusException
    {
        public MissingKeywordsException(Page page, IEnumerable<string> keywordMessages)
            : base(page, BuildMessage(keywordMessages))
        {
        }

        private static string BuildMessage(IEnumerable<string> keywordMessages)
        {
            return keywordMessages.Aggregate("", (s, s1) => (s.Any() ? s + "\n" : s) + s1);
        }
    }
}