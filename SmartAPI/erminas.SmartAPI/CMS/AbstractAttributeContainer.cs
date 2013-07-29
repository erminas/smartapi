using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml;
using erminas.SmartAPI.Exceptions;

namespace erminas.SmartAPI.CMS
{
    internal abstract class AbstractAttributeContainer : ISessionObject, IXmlBasedObject
    {
        private readonly ISession _session;

        internal AbstractAttributeContainer(ISession session)
        {
            _session = session;
        }

        internal AbstractAttributeContainer(ISession session, XmlElement xmlElement)
        {
            _session = session;
            if (xmlElement == null)
            {
                throw new SmartAPIInternalException("XmlElement is null");
            }
            XmlElement = xmlElement;
        }

        public virtual XmlElement XmlElement { get; protected set; }

        protected T GetAttributeValue<T>([CallerMemberName] string callerName = "")
        {
            var attribute = GetRedDotAttributeOfCallerMember(callerName);
            return attribute.ReadFrom<T>(XmlElement);
        }

        protected RedDotAttribute GetRedDotAttributeOfCallerMember(string callerName)
        {
            const bool USE_INHERITED_ATTRIBUTES = true;

            var property = GetType().GetProperty(callerName);
            return (RedDotAttribute)property.GetCustomAttributes(typeof(RedDotAttribute), USE_INHERITED_ATTRIBUTES).First();
        }

        protected void SetAttributeValue<T>(T value, [CallerMemberName] string callerName = "")
        {
            var attribute = GetRedDotAttributeOfCallerMember(callerName);
            attribute.WriteTo(XmlElement, value);
        }

        public virtual ISession Session
        {
            get { return _session; }
        }
    }
}