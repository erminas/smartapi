using System.Collections.Generic;

namespace erminas.SmartAPI.CMS.PageElements
{
    public interface IMultiLinkElement : ILinkElement
    {
        void ConnectPages(IEnumerable<IPage> pages);

        void DisconnectPages(IEnumerable<IPage> pages);
    }
}