using erminas.SmartAPI.CMS.PageElements;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS
{
    public interface ILinkTarget : IRedDotObject
    {
        IRDList<ILinkElement> ReferencedBy { get; }
    }
}