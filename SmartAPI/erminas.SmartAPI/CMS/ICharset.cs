using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS
{
    public interface ICharset : ISessionObject
    {
        string Name { get;  }
        string Label { get;  }
        int Codepage { get; }
    }

    internal sealed class Charset : ICharset
    {
        public Charset(ISession session, XmlElement xmlElement)
        {
            Session = session;
            Codepage = xmlElement.GetIntAttributeValue("codepage").GetValueOrDefault();
            Name = xmlElement.GetName();
            Label = xmlElement.GetAttributeValue("label");
        }
        public string Name { get; private set; }
        public string Label { get; private set; }
        public int Codepage { get; private set; }
        public ISession Session { get; private set; }
    }
}
