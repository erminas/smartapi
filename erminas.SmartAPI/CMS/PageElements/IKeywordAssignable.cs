using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS.PageElements
{
    public interface IKeywordAssignable
    {
        void AssignKeyword(Keyword keyword);
        void UnassignKeyword(Keyword keyword);

        IRDList<Keyword> AssignedKeywords { get; }
    }
}
