using System;

namespace erminas.SmartAPI.Exceptions
{
    public enum PageDeletionError
    {
        Unknown = 0,
        NoRightToDeletePage,
        ElementsOfPageStillGetReferenced
    }

    [Serializable]
    public class PageDeletionException : Exception
    {
        public readonly PageDeletionError Error;

        public PageDeletionException(RQLException e)
            : base(e.Message, e)
        {
            switch(e.ErrorCode)
            {
                case ErrorCode.RDError2910:
                    Error = PageDeletionError.ElementsOfPageStillGetReferenced;
                    break;
                case ErrorCode.RDError15805:
                    Error = PageDeletionError.NoRightToDeletePage;
                    break;
                default:
                    Error = PageDeletionError.Unknown;
                    break;
            }
        }

        public PageDeletionException(string message) : base (message)
        {
            Error = PageDeletionError.Unknown;
        }
    }
}
