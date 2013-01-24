using System;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.Exceptions
{
    public enum PageDeletionError
    {
        Unknown = 0,
        NoRightToDeletePage,
        ElementsOfPageStillGetReferenced
    }

    [Serializable]
    public class PageDeletionException : SmartAPIException
    {
        public readonly PageDeletionError Error;

        internal PageDeletionException(RQLException e) : base(e.Server, e.Message, e)
        {
            switch (e.ErrorCode)
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

        internal PageDeletionException(ServerLogin login, string message) : base(login, message)
        {
            Error = PageDeletionError.Unknown;
        }
    }
}