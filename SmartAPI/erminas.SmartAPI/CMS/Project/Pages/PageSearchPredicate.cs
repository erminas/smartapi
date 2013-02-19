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
using erminas.SmartAPI.CMS.Project.Workflows;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS.Project.Pages
{
    public interface IPageSearchFilter
    {
        string ToSearchItemString();
    }

    public abstract class AbstractPageSearchFilter : IPageSearchFilter
    {
        #region EqualityOperatorType enum

        public enum EqualityOperatorType
        {
            Equal,
            NotEqual
        }

        #endregion

        #region IPageSearchPredicate Members

        public abstract string ToSearchItemString();

        #endregion

        protected static string EqualityOperatorToString(EqualityOperatorType type)
        {
            return type == EqualityOperatorType.Equal ? "eq" : "ne";
        }

        protected static string ToSearchItemStringInternal(string key, string value, string @operator)
        {
            const string BASIC_SEARCH_ITEM = @"<SEARCHITEM key=""{0}"" value=""{1}"" operator=""{2}""/>";
            return string.Format(BASIC_SEARCH_ITEM, key, value, @operator);
        }
    }

    public class HeadlineFilter : AbstractPageSearchFilter
    {
        #region OperatorType enum

        public enum OperatorType
        {
            IsLike,
            Contains
        }

        #endregion

        public readonly OperatorType Operator;
        public readonly string Value;

        public HeadlineFilter(OperatorType op, string value)
        {
            Operator = op;
            Value = value;
        }

        public override string ToSearchItemString()
        {
            return ToSearchItemStringInternal("headline", Value, Operator == OperatorType.IsLike ? "like" : "contains");
        }
    }

    public class HeadlineAndContentFilter : AbstractPageSearchFilter
    {
        public readonly EqualityOperatorType Operator;
        public readonly string Value;

        public HeadlineAndContentFilter(EqualityOperatorType @operator, string value)
        {
            Operator = @operator;
            Value = value;
        }

        public override string ToSearchItemString()
        {
            return ToSearchItemStringInternal("keyword", Value, EqualityOperatorToString(Operator));
        }
    }

    public class WorkflowFilter : AbstractPageSearchFilter
    {
        public readonly EqualityOperatorType Operator;
        public readonly Workflow Workflow;

        public WorkflowFilter(EqualityOperatorType @operator, Workflow workflow)
        {
            Operator = @operator;
            Workflow = workflow;
        }

        public override string ToSearchItemString()
        {
            return ToSearchItemStringInternal("workflow", Workflow.Guid.ToRQLString(),
                                              EqualityOperatorToString(Operator));
        }
    }

    public class SpecialPageFilter : AbstractPageSearchFilter
    {
        #region PageCategoryType enum

        public enum PageCategoryType
        {
            Linked,
            Unlinked,
            RecycleBin,
            Active,
            All
        }

        #endregion

        public readonly PageCategoryType PageCategory;

        public SpecialPageFilter(PageCategoryType pageCategory)
        {
            PageCategory = pageCategory;
        }

        public override string ToSearchItemString()
        {
            return ToSearchItemStringInternal("specialpages", PageCategory.ToString().ToLowerInvariant(), "eq");
        }
    }

    public class PageStatusFilter : AbstractPageSearchFilter
    {
        #region PageStatusType enum

        public enum PageStatusType
        {
            SavedAsDraft,
            WaitingForRelease,
            WaitingForCorrection,
            InWorkflow,
            Resubmitted,
            Released
        }

        #endregion

        #region UserType enum

        public enum UserType
        {
            CurrentUser,
            AllUsers
        }

        #endregion

        public readonly PageStatusType PageStatus;

        public readonly UserType User;

        public PageStatusFilter(PageStatusType pageStatus, UserType user)
        {
            PageStatus = pageStatus;
            User = user;
        }

        public override string ToSearchItemString()
        {
            const string STATUS_ITEM = @"<SEARCHITEM key=""pagestate"" value=""{0}"" users=""{1}"" />";
            return string.Format(STATUS_ITEM, PageStatusTypeToString(PageStatus),
                                 User == UserType.CurrentUser ? "myself" : "all");
        }

        private static string PageStatusTypeToString(PageStatusType pageStatus)
        {
            switch (pageStatus)
            {
                case PageStatusType.WaitingForCorrection:
                    return "waitingforcorrection";
                case PageStatusType.WaitingForRelease:
                    return "waitingforrelease";
                case PageStatusType.SavedAsDraft:
                    return "checkedout";
                case PageStatusType.Released:
                    return "released";
                case PageStatusType.Resubmitted:
                    return "resubmitted";
                case PageStatusType.InWorkflow:
                    return "pagesinworkflow";

                default:
                    throw new ArgumentException(String.Format("Unknown page status {0}", pageStatus));
            }
        }
    }
}