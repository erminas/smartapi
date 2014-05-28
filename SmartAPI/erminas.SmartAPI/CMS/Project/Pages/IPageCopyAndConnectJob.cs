using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using erminas.SmartAPI.CMS.Project.Pages.Elements;
using erminas.SmartAPI.Exceptions;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS.Project.Pages
{
    [Flags]
    public enum PageCopyAndConnectFlags
    {
        None = 0,
        AlsoCopyCompleteTree = 1,
        CopyElementContentsInsteadOfReferencing = 2,
        AdoptFileNames = 4,
        AdoptPublicationSettingsOfLinkElements = 8,
        AdoptAuthorizationsOfLinkElements = 16,
        AdoptAuthorizationsOfOtherElements = 32,
        AdoptProjectVariantAssignments = 64,
        AdoptTargetContainterReferenceFromNewlyCreatedContainer = 128
    }

    /// <summary>
    ///  Preliminary version of a page copy and connect job. Currently only RunAsync() is supported, so you have to check for its success yourself,
    /// if you need to.
    /// </summary>
    [Obsolete("This interface is experimental and will change in a future version")]
    public interface IPageCopyAndConnectJob : IAsyncJob
    {
        IPage PageToCopy { get; set; }
        ILinkElement ConnectionTarget { get; set; }
        PageCopyAndConnectFlags Flags { get; set; }
    }

    internal class PageCopyAndConnectJob : AbstractAsyncProjectJob, IPageCopyAndConnectJob
    {
        private PageCopyAndConnectFlags _flags;

        public PageCopyAndConnectJob(IPage page, ILinkElement linkElement, PageCopyAndConnectFlags flags) : base(page.Project)
        {
            PageToCopy = page;
            ConnectionTarget = linkElement;
            _flags = flags;
        }

        public override void RunAsync()
        {
            try
            {
                const string QUERY =
                    @"<LINK action=""assign"" guid=""{0}"" reddotcacheguid=""""><PAGE action=""copy"" guid=""{1}"" copymode=""{2}"" /></LINK>";
                Project.Session.ExecuteRQL(QUERY.RQLFormat(ConnectionTarget, PageToCopy, (int) _flags), RQL.IODataFormat.SessionKeyAndLogonGuid);
            }
            catch (RQLException e)
            {
                if (e.ErrorCode != ErrorCode.Unknown)
                {
                    throw;
                }
                //The following "error" messages occur for ENG and DEU, as i don't know all messages for all languages, we just ignore all errors for now ...
                
                //The copy job is added to the processing queue. The procedure can take some time.
                
                //Der Kopiervorgang wird in die Prozesswarteschlange eingefügt. Die Ausführung kann einige Zeit dauern.
            }
        }

        public PageCopyAndConnectFlags Flags { get { return _flags; } set { _flags = value; } }

        public override void RunSync(TimeSpan maxWait)
        {
            throw new NotImplementedException("not yet implemented, please use RunAsync() for now");
        }

        public IPage PageToCopy { get; set; }
        public ILinkElement ConnectionTarget { get; set; }
    }
}
