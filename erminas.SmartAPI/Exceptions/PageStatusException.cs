using System;
using System.Runtime.Serialization;
using erminas.SmartAPI.CMS;

namespace erminas.SmartAPI.Exceptions
{
    [Serializable]
    public class PageStatusException : SmartAPIException
    {
        public Page Page { get; private set; }

        public PageStatusException(Page page)
        {
            Page = page;
        }

        public PageStatusException(Page page, string message) : base(message)
        {
            Page = page;
        }

        public PageStatusException(Page page, string message, Exception innerException)
            : base(message, innerException)
        {
            Page = page;
        }

        public PageStatusException(Page page, SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Page = page;
        }
    }
}
