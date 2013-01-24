using System;
using System.Runtime.Serialization;
using erminas.SmartAPI.CMS;

namespace erminas.SmartAPI.Exceptions
{
    [Serializable]
    public class PageStatusException : SmartAPIException
    {
        public Page Page { get; private set; }

        internal PageStatusException(Page page) : base(page.Project.Session.ServerLogin)
        {
            Page = page;
        }

        internal PageStatusException(Page page, string message)
            : base(page.Project.Session.ServerLogin, message)
        {
            Page = page;
        }

        internal PageStatusException(Page page, string message, Exception innerException)
            : base(page.Project.Session.ServerLogin, message, innerException)
        {
            Page = page;
        }

        internal PageStatusException(Page page, SerializationInfo info, StreamingContext context)
            : base(page.Project.Session.ServerLogin, info, context)
        {
            Page = page;
        }
    }
}
