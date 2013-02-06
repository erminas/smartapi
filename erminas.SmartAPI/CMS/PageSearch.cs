// Smart API - .Net programatical access to RedDot servers
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
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS
{
    public class PageSearch
    {
        private const int DEFAULT_MAX_RECORDS = 20000;
        private readonly Project _project;

        internal PageSearch(Project project)
        {
            _project = project;
            SetDefaults();
        }

        public Page.PageType PageType { get; set; }

        public string Headline { get; set; }

        public bool HeadlineExact { get; set; }

        public string Category { get; set; }

        public string Keyword { get; set; }

        public bool KeywordExact { get; set; }

        public string Text { get; set; }

        public bool TextExact { get; set; }

        public int PageIdFrom { get; set; }

        public int PageIdTo { get; set; }

        public DateTime CreatedFrom { get; set; }

        public DateTime CreatedTo { get; set; }

        public int MaxRecords { get; set; }

        public ContentClass ContentClass { get; set; }

        public void SetDefaults()
        {
            PageType = Page.PageType.All;
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

        public List<Page> Execute()
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

        private XmlElement AddLanguageVariantId(XmlElement curNode)
        {
            curNode.SetAttributeValue("languagevariantid", _project.CurrentLanguageVariant.Language);

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
        public readonly ContentClass ContentClass;
        public readonly DateTime CreationDate;
        public readonly DateTime DateOfLastChange;
        public readonly User LastEditor;
        public readonly User OriginalAuthor;
        public readonly Page Page;

        public Result(Page page, DateTime creationDate, User originalAuthor, DateTime dateOfLastChange, User lastEditor,
                      ContentClass contentClass)
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
        public readonly IEnumerable<Note> Notes;
        public readonly RejectionSkippableType RejectionSkippability;
        public readonly Rejection RejectionType;
        public readonly string ReleaseName;
        public readonly IEnumerable<ReleaseInfo> Releases;
        public readonly Workflow Workflow;
        public readonly ReleaseType WorkflowReactionTypeResponsibleForRejection;

        public WorkflowInfo(Workflow workflow, IEnumerable<ReleaseInfo> releases, string releaseName,
                            Rejection rejectionType, ReleaseType workflowReactionTypeResponsibleForRejection,
                            RejectionSkippableType rejectionSkippability, int escalationTimeoutInHours,
                            IEnumerable<Note> notes)
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
        public readonly IEnumerable<UserInfo> Users;

        public ReleaseInfo(int assentCount, int requiredAssentCount, IEnumerable<UserInfo> users)
        {
            AssentCount = assentCount;
            RequiredAssentCount = requiredAssentCount;
            Users = users;
        }

        #region Nested type: UserInfo

        public class UserInfo
        {
            public readonly bool HasUserReleasedPage;
            public readonly DateTime PageReleaseDate;
            public readonly User User;

            public UserInfo(Project project, XmlElement user)
            {
                User = new User(project.Session, user.GetGuid()) {Name = user.GetName()};
                HasUserReleasedPage = user.GetIntAttributeValue("released").GetValueOrDefault() == 1;
                PageReleaseDate = user.GetOADate().GetValueOrDefault();
            }
        }

        #endregion
    }
}