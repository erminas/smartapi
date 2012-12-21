using System.Web;

namespace erminas.SmartAPI.CMS
{
    public struct EMail
    {
        public string To;

        public string Message
        {
            set { HtmlEncodedMessage = HttpUtility.HtmlEncode(value); }
        }

        public string HtmlEncodedMessage { get; private set; }

        public string Subject
        {
            set { HtmlEncodedSubject = HttpUtility.HtmlEncode(value); }
        }

        public string HtmlEncodedSubject { get; private set; }
    }
}