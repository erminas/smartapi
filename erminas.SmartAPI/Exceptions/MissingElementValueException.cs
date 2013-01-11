using System;
using System.Collections.Generic;
using System.Linq;
using erminas.SmartAPI.CMS;

namespace erminas.SmartAPI.Exceptions
{
    [Serializable]
    public class MissingElementValueException : PageStatusException
    {
        public MissingElementValueException(Page page, IEnumerable<string> names) : base(page, BuildMessage(names))
        {
        }

        private static string BuildMessage(IEnumerable<string> names)
        {
            const string MESSAGE = "Missing values for the following mandatory elements: {0}";
            string missingElements = names.Aggregate("", (s, s1) => s + (s.Any() ? ", " : "") + s1 );
            
            return string.Format(MESSAGE, missingElements);
        }
    }
}
