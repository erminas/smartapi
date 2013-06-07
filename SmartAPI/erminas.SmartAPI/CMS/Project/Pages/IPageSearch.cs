// Smart API - .Net programmatic access to RedDot servers
//  
// Copyright (C) 2013 erminas GbR
// 
// This program is free software: you can redistribute it and/or modify it 
// under the terms of the GNU General Public License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License along with this program.
// If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;
using erminas.SmartAPI.CMS.Administration;
using erminas.SmartAPI.CMS.Project.ContentClasses;
using erminas.SmartAPI.CMS.Project.Workflows;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS.Project.Pages
{
    public interface IPageSearch : IProjectObject
    {
        string Category { get; set; }
        IContentClass ContentClass { get; set; }
        DateTime CreatedFrom { get; set; }
        DateTime CreatedTo { get; set; }
        IEnumerable<IPage> Execute();
        string Headline { get; set; }
        bool HeadlineExact { get; set; }
        string Keyword { get; set; }
        bool KeywordExact { get; set; }
        int MaxRecords { get; set; }
        int PageIdFrom { get; set; }
        int PageIdTo { get; set; }
        PageType PageType { get; set; }
        void SetDefaults();
        string Text { get; set; }
        bool TextExact { get; set; }
    }

    internal class PageSearch : IPageSearch
    {
        private const int DEFAULT_MAX_RECORDS = 20000;
        private readonly IProject _project;

        internal PageSearch(IProject project)
        {
            _project = project;
            SetDefaults();
        }

        public string Category { get; set; }
        public IContentClass ContentClass { get; set; }
        public DateTime CreatedFrom { get; set; }

        public DateTime CreatedTo { get; set; }

        public IEnumerable<IPage> Execute()
        {
            var rqlXml = new XmlDocument();
            XmlElement pageElement = rqlXml.CreateElement("PAGE");
            pageElement.SetAttribute("action", "search");
            pageElement.SetAttribute("flags", PageType.ToString());
            pageElement.SetAttribute("maxrecords", MaxRecords.ToString(CultureInfo.InvariantCulture));
            if (Headline != null)
            {
                pageElement.SetAttribute("headline", Headline);
                pageElement.SetAttribute("headlinelike", HeadlineExact ? "0" : "-1");
            }
            if (Category != null)
            {
                pageElement.SetAttribute("section", Category);
            }
            if (Keyword != null)
            {
                pageElement.SetAttribute("keyword", Keyword);
                pageElement.SetAttribute("keywordlike", KeywordExact ? "0" : "-1");
            }
            if (Text != null)
            {
                pageElement.SetAttribute("searchtext", Text);
            }
            if (PageIdFrom != -1)
            {
                pageElement.SetAttribute("pageidfrom", PageIdFrom.ToString(CultureInfo.InvariantCulture));
            }
            if (PageIdTo != -1)
            {
                pageElement.SetAttribute("pageidto", PageIdTo.ToString(CultureInfo.InvariantCulture));
            }
            if (CreatedTo != DateTime.MinValue)
            {
                pageElement.SetAttribute("createdateto", (CreatedTo.ToOADate()).ToString(CultureInfo.InvariantCulture));
            }
            if (CreatedFrom != DateTime.MinValue)
            {
                pageElement.SetAttribute("createdatefrom",
                                         (CreatedFrom.ToOADate()).ToString(CultureInfo.InvariantCulture));
            }
            if (ContentClass != null)
            {
                pageElement.SetAttribute("templateguid", ContentClass.Guid.ToRQLString());
            }

            XmlDocument xmlDoc = _project.ExecuteRQL(pageElement.OuterXml);

            return (from XmlElement curNode in xmlDoc.GetElementsByTagName("PAGE")
                    let nodeWithLanguageVariantId = AddLanguageVariantId(curNode)
                    select new Page(_project, nodeWithLanguageVariantId)).ToList();
        }

        public string Headline { get; set; }

        public bool HeadlineExact { get; set; }

        public string Keyword { get; set; }

        public bool KeywordExact { get; set; }
        public int MaxRecords { get; set; }

        public int PageIdFrom { get; set; }

        public int PageIdTo { get; set; }
        public PageType PageType { get; set; }

        public IProject Project
        {
            get { return _project; }
        }

        public ISession Session
        {
            get { return _project.Session; }
        }

        public void SetDefaults()
        {
            PageType = PageType.All;
            Text = null;
            TextExact = true;
            Category = null;
            Keyword = null;
            KeywordExact = true;
            Headline = null;
            HeadlineExact = true;
            MaxRecords = DEFAULT_MAX_RECORDS;
            PageIdFrom = -1;
            PageIdTo = -1;
            CreatedFrom = DateTime.MinValue;
            CreatedTo = DateTime.MinValue;
            ContentClass = null;
        }

        public string Text { get; set; }

        public bool TextExact { get; set; }

        private XmlElement AddLanguageVariantId(XmlElement curNode)
        {
            curNode.SetAttributeValue("languagevariantid", _project.LanguageVariants.Current.Abbreviation);

            return curNode;
        }
    }

    public class ResultGroup
    {
        public readonly string Group;
        public readonly IEnumerable<Result> Results;

        public ResultGroup(string @group, IEnumerable<Result> results)
        {
            Group = @group;
            Results = results;
        }
    }

    public class Result
    {
        public readonly IContentClass ContentClass;
        public readonly DateTime CreationDate;
        public readonly DateTime DateOfLastChange;
        public readonly IUser LastEditor;
        public readonly IUser OriginalAuthor;
        public readonly IPage Page;

        public Result(IPage page, DateTime creationDate, IUser originalAuthor, DateTime dateOfLastChange,
                      IUser lastEditor, IContentClass contentClass)
        {
            Page = page;
            CreationDate = creationDate;
            OriginalAuthor = originalAuthor;
            DateOfLastChange = dateOfLastChange;
            LastEditor = lastEditor;
            ContentClass = contentClass;
        }

        public DateTime ReleaseDate { get; set; }
        public WorkflowInfo WorkflowInfo { get; set; }
    }

    public class WorkflowInfo
    {
        #region Rejection enum

        public enum Rejection
        {
            NoReleaseReactionDefined = 0,
            ToAuthor = 4,
            ToPreviousReleaseLevel = 8,
            ToAnEligibleLevel = 16,
            SelectionOfReleaseLevelByRejectingUser = 32
        }

        #endregion

        #region RejectionSkippableType enum

        public enum RejectionSkippableType
        {
            NotApplicable = -1,
            RejectionCannotBeSkipped = 0,
            RejectionCanBeSkipped = 1
        }

        #endregion

        #region ReleaseType enum

        public enum ReleaseType
        {
            NoWorkflowReactionSet = 0,
            PageWaitingForReleaseOrPageWasRejected = 1155,
            WebComplianceManagerRejectedAutomatically = 1156
        }

        #endregion

        public readonly int EscalationTimeoutInHours;
        public readonly bool IsEscalationProcedureSet;
        public readonly bool? IsRejectionSkippable;
        public readonly IEnumerable<INote> Notes;
        public readonly RejectionSkippableType RejectionSkippability;
        public readonly Rejection RejectionType;
        public readonly string ReleaseName;
        public readonly IEnumerable<ReleaseInfo> Releases;
        public readonly IWorkflow Workflow;
        public readonly ReleaseType WorkflowReactionTypeResponsibleForRejection;

        public WorkflowInfo(IWorkflow workflow, IEnumerable<ReleaseInfo> releases, string releaseName,
                            Rejection rejectionType, ReleaseType workflowReactionTypeResponsibleForRejection,
                            RejectionSkippableType rejectionSkippability, int escalationTimeoutInHours,
                            IEnumerable<INote> notes)
        {
            Releases = releases;
            ReleaseName = releaseName;
            RejectionType = rejectionType;
            WorkflowReactionTypeResponsibleForRejection = workflowReactionTypeResponsibleForRejection;
            RejectionSkippability = rejectionSkippability;
            EscalationTimeoutInHours = escalationTimeoutInHours;
            Notes = notes;
            Workflow = workflow;
            IsEscalationProcedureSet = EscalationTimeoutInHours > 0;
            switch (rejectionSkippability)
            {
                case RejectionSkippableType.NotApplicable:
                    IsRejectionSkippable = null;
                    break;
                case RejectionSkippableType.RejectionCannotBeSkipped:
                    IsRejectionSkippable = false;
                    break;
                case RejectionSkippableType.RejectionCanBeSkipped:
                    IsRejectionSkippable = true;
                    break;
                default:
                    throw new ArgumentException("Unknown rejection skippability type");
            }
        }
    }

    public class ReleaseInfo
    {
        public readonly int AssentCount;
        public readonly int RequiredAssentCount;
        public readonly IEnumerable<IIUserInfo> Users;

        public ReleaseInfo(int assentCount, int requiredAssentCount, IEnumerable<IIUserInfo> users)
        {
            AssentCount = assentCount;
            RequiredAssentCount = requiredAssentCount;
            Users = users;
        }

        public interface IIUserInfo
        {
            bool HasUserReleasedPage { get; }
            DateTime PageReleaseDate { get; }
            IUser User { get; }
        }

        internal class UserInfo : IIUserInfo
        {
            public UserInfo(IProject project, XmlElement user)
            {
                User = new User(project.Session, user.GetGuid()) {Name = user.GetName()};
                HasUserReleasedPage = user.GetIntAttributeValue("released").GetValueOrDefault() == 1;
                PageReleaseDate = user.GetOADate().GetValueOrDefault();
            }

            public bool HasUserReleasedPage { get; private set; }
            public DateTime PageReleaseDate { get; private set; }
            public IUser User { get; private set; }
        }
    }
}