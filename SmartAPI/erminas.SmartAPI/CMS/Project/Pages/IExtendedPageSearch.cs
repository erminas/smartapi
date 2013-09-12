// SmartAPI - .Net programmatic access to RedDot servers
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
using System.Linq;
using System.Xml;
using erminas.SmartAPI.CMS.Administration;
using erminas.SmartAPI.CMS.Project.ContentClasses;
using erminas.SmartAPI.CMS.Project.Workflows;
using erminas.SmartAPI.Exceptions;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS.Project.Pages
{
    /// <summary>
    ///     An extended page search more powerful than the normale <see cref="PageSearch" /> that can be refined by adding filters to it (e.g. on the page status).
    ///     <example>
    ///         <code>
    /// <pre>
    ///     Project project = ...;
    ///     var search = project.CreateExtendedPageSearch();
    ///     search.Filters.Add(new HeadlineFilter(HeadlineFilter.OperatorType.IsLike, "sometext"));
    ///     var results = search.Execute();
    /// </pre>
    /// </code>
    ///     </example>
    /// </summary>
    public interface IExtendedPageSearch : IProjectObject
    {
        int Count();
        List<ResultGroup> Execute();
        List<IPageSearchFilter> Filters { get; }
        GroupBy GroupBy { get; set; }
        SortDirection GroupSortDirection { get; set; }
        ILanguageVariant LanguageVariant { get; set; }
        int MaxHits { get; set; }
        OrderBy OrderBy { get; set; }
        SortDirection OrderDirection { get; set; }
        IUser User { get; set; }
    }

    internal class ExtendedPageSearch : IExtendedPageSearch
    {
        private readonly List<IPageSearchFilter> _filters = new List<IPageSearchFilter>();

        private readonly IProject _project;

        public ExtendedPageSearch(IProject project)
        {
            _project = project;
        }

        public int Count()
        {
            XmlDocument xmlDoc = RunQuery(true);
            return
                ((XmlElement) xmlDoc.GetElementsByTagName("PAGES")[0]).GetIntAttributeValue("hits").GetValueOrDefault();
        }

        public List<ResultGroup> Execute()
        {
            return ToResult(RunQuery(false));
        }

        public List<IPageSearchFilter> Filters
        {
            get { return _filters; }
        }

        public GroupBy GroupBy { get; set; }

        public SortDirection GroupSortDirection { get; set; }

        public ILanguageVariant LanguageVariant { get; set; }

        public int MaxHits { get; set; }

        public OrderBy OrderBy { get; set; }

        public SortDirection OrderDirection { get; set; }

        public IProject Project
        {
            get { return _project; }
        }

        public ISession Session
        {
            get { return Project.Session; }
        }

        public IUser User { get; set; }

        private static string GroupByTypeToString(GroupBy type)
        {
            switch (type)
            {
                case GroupBy.ContentClass:
                    return "contentclass";
                case GroupBy.CreatedBy:
                    return "createdby";
                case GroupBy.ChangedBy:
                    return "changedby";
                case GroupBy.None:
                    return "";
                default:
                    throw new ArgumentException(String.Format("Unknown group by type: {0}", type));
            }
        }

        private ILanguageVariant LanguageVariantOfSearchResults
        {
            get { return LanguageVariant ?? _project.LanguageVariants.Current; }
        }

        private static string OrderByTypeToString(OrderBy order)
        {
            switch (order)
            {
                case OrderBy.PageId:
                    return "pageid";
                case OrderBy.Headline:
                    return "headline";
                case OrderBy.CreationDate:
                    return "createdate";
                case OrderBy.ChangeDate:
                    return "changedate";
                case OrderBy.ContentClass:
                    return "contentclass";
                default:
                    throw new ArgumentException(String.Format("Unknown sort direction: {0}", order));
            }
        }

        private XmlDocument RunQuery(bool isCountOnly)
        {
            if (Session.ServerVersion < new Version(10, 0) && Filters.Any(filter => filter is WorkflowFilter))
            {
                throw new InvalidServerVersionException(Session.ServerLogin,
                                                        "Searches for pages in workflow are not supported for RedDot servers with version < 10");
            }
            const string XSEARCH = @"<PAGE action=""xsearch"" {0}>{1}</PAGE>";
            string arguments = "";
            if (GroupBy != GroupBy.None)
            {
                arguments += "groupby=\"" + GroupByTypeToString(GroupBy) + "\" ";
                arguments += "groupdirection=\"" + SortDirectionToString(GroupSortDirection) + "\" ";
            }

            if (LanguageVariant != null)
            {
                arguments += "languagevariantid=\"" + LanguageVariant.Abbreviation + "\" ";
            }

            if (isCountOnly)
            {
                arguments += @"option=""countresults"" ";
            }

            arguments += "maxhits=\"" + MaxHits + "\" ";
            arguments += "orderby=\"" + OrderByTypeToString(OrderBy) + "\" ";
            arguments += "orderdirection=\"" + SortDirectionToString(OrderDirection) + "\" ";
            arguments += "pagesize=\"-1\" ";

            arguments += "projectguid=\"" + _project.Guid.ToRQLString() + "\" ";
            if (User != null)
            {
                arguments += "userguid=\"" + User.Guid.ToRQLString() + "\" ";
            }

            string searchItems = "";
            if (Filters.Any())
            {
                searchItems = "<SEARCHITEMS>";
                searchItems += Filters.Aggregate("", (value, curPred) => value + curPred.ToSearchItemString());
                searchItems += "</SEARCHITEMS>";
            }

            return _project.ExecuteRQL(string.Format(XSEARCH, arguments, searchItems));
        }

        private static string SortDirectionToString(SortDirection dir)
        {
            switch (dir)
            {
                case SortDirection.Ascending:
                    return "ASC";
                case SortDirection.Descending:
                    return "DESC";
                default:
                    throw new ArgumentException(String.Format("Unknown sort direction: {0}", dir));
            }
        }

        private IEnumerable<ReleaseInfo> ToReleases(XmlElement releases)
        {
            return (from XmlElement curRelease in releases
                    let users = curRelease.GetElementsByTagName("USER")
                    select
                        new ReleaseInfo(curRelease.GetIntAttributeValue("assentcount").GetValueOrDefault(),
                                        curRelease.GetIntAttributeValue("requiredassentcount").GetValueOrDefault(),
                                        from XmlElement curUser in releases.GetElementsByTagName("USER")
                                        select new ReleaseInfo.UserInfo(_project, curUser))).ToList();
        }

        private List<ResultGroup> ToResult(XmlDocument doc)
        {
            XmlNodeList groups = doc.GetElementsByTagName("GROUP");
            if (groups.Count == 0)
            {
                return new List<ResultGroup> {new ResultGroup(null, ToResult(doc.GetElementsByTagName("PAGE")))};
            }
            return (from XmlElement curGroup in groups
                    select
                        new ResultGroup(curGroup.GetAttributeValue("value"),
                                        ToResult(curGroup.GetElementsByTagName("PAGE")))).ToList();
        }

        private IEnumerable<Result> ToResult(XmlNodeList pages)
        {
            return (from XmlElement curPage in pages
                    let creation = (XmlElement) curPage.GetElementsByTagName("CREATION")[0]
                    let change = (XmlElement) curPage.GetElementsByTagName("CHANGE")[0]
                    let release = (XmlElement) curPage.GetElementsByTagName("RELEASE")[0]
                    let contentClass = (XmlElement) curPage.GetElementsByTagName("CONTENTCLASS")[0]
                    select
                        new Result(
                        new Page(_project, curPage.GetGuid(), LanguageVariantOfSearchResults)
                            {
                                Headline = curPage.GetAttributeValue("headline"),
                                Status = ((PageState) int.Parse(curPage.GetAttributeValue("status")))
                            },
                        // ReSharper disable PossibleInvalidOperationException
                        creation.GetOADate().Value,
                        new User(_project.Session, ((XmlElement) creation.GetElementsByTagName("USER")[0]).GetGuid()),
                        change.GetOADate().Value, // ReSharper restore PossibleInvalidOperationException
                        new User(_project.Session, ((XmlElement) change.GetElementsByTagName("USER")[0]).GetGuid()),
                        new ContentClass(_project, contentClass.GetGuid())
                            {
                                Name = contentClass.GetAttributeValue("name")
                            })
                            {
                                WorkflowInfo = ToWorkflow(curPage.GetElementsByTagName("WORKFLOW"))
                            }).ToList();
        }

        private WorkflowInfo ToWorkflow(XmlNodeList workflows)
        {
            if (workflows.Count == 0)
            {
                return null;
            }

            var workflowElement = (XmlElement) workflows[0];
            int? skipable = workflowElement.GetIntAttributeValue("skipable");
            var workflow = new Workflow(_project, workflowElement.GetGuid())
                {
                    Name = workflowElement.GetAttributeValue("name")
                };
            return new WorkflowInfo(workflow,
                                    ToReleases((XmlElement) workflowElement.GetElementsByTagName("RELEASES")[0]),
                                    workflowElement.GetAttributeValue("releasename"),
                                    (WorkflowInfo.Rejection) workflowElement.GetIntAttributeValue("rejectiontype"),
                                    (WorkflowInfo.ReleaseType) workflowElement.GetIntAttributeValue("releasetype"),
                                    skipable == null
                                        ? WorkflowInfo.RejectionSkippableType.NotApplicable
                                        : (WorkflowInfo.RejectionSkippableType) skipable.Value,
                                    workflowElement.GetIntAttributeValue("escalationtimeout").Value,
                                    from XmlElement curSupplement in workflowElement.GetElementsByTagName("SUPPLEMENT")
                                    select new Note(workflow, curSupplement));
        }
    }

    public enum SortDirection
    {
        Ascending = 0,
        Descending
    }

    public enum OrderBy
    {
        Headline = 0,
        PageId,
        CreationDate,
        ChangeDate,
        ContentClass
    }

    public enum GroupBy
    {
        None = 0,
        ContentClass,
        CreatedBy,
        ChangedBy
    }
}