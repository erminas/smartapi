using System;
using System.Collections.Generic;
using System.Xml;

namespace erminas.SmartAPI.CMS.PageElements
{
    public abstract class AbstractMultiLinkElement : AbstractLinkElement, IMultiLinkElement
    {
        protected AbstractMultiLinkElement(Project project, Guid guid) : base(project, guid)
        {
        }

        protected AbstractMultiLinkElement(Project project, XmlElement xmlElement) : base(project, xmlElement)
        {
        }
        
        public void ConnectPages(IEnumerable<IPage> pages)
        {
            foreach(var curPage in pages)
            {
                Connect(curPage);
            }
        }

        public new void DisconnectPages(IEnumerable<IPage> pages)
        {
            base.DisconnectPages(pages);
        }
    }
}
