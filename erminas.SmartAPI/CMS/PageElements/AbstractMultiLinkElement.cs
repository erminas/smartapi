using System;
using System.Collections.Generic;
using System.Xml;

namespace erminas.SmartAPI.CMS.PageElements
{
    public abstract class AbstractMultiLinkElement : AbstractLinkElement, IMultiLinkElement
    {
        protected AbstractMultiLinkElement(Project project, Guid guid, LanguageVariant languageVariant)
            : base(project, guid, languageVariant)
        {
        }

        protected AbstractMultiLinkElement(Project project, XmlElement xmlElement) : base(project, xmlElement)
        {
        }

        #region IMultiLinkElement Members

        public void ConnectPages(IEnumerable<IPage> pages)
        {
            foreach (IPage curPage in pages)
            {
                Connect(curPage);
            }
        }

        public new void DisconnectPages(IEnumerable<IPage> pages)
        {
            base.DisconnectPages(pages);
        }

        #endregion
    }
}