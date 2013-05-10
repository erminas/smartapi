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

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using erminas.SmartAPI.Exceptions;
using erminas.SmartAPI.Utils;
using erminas.SmartAPI.Utils.CachedCollections;

namespace erminas.SmartAPI.CMS.Project.ContentClasses
{
    internal class ProjectVariantAssignment : IProjectVariantAssignment
    {
        private readonly IContentClass _contentClass;

        private readonly IProjectVariant _projectVariant;
        private readonly ITemplateVariant _templateVariant;

        internal ProjectVariantAssignment(IContentClass contentClass, IProjectVariant projectVariant,
                                          ITemplateVariant templateVariant)
        {
            _contentClass = contentClass;
            _projectVariant = projectVariant;
            _templateVariant = templateVariant;
        }

        public IContentClass ContentClass
        {
            get { return _contentClass; }
        }

        public IProjectVariant ProjectVariant
        {
            get { return _projectVariant; }
        }

        public ITemplateVariant TemplateVariant
        {
            get { return _templateVariant; }
        }
    }

    public interface IProjectVariantAssignment
    {
        IContentClass ContentClass { get; }
        IProjectVariant ProjectVariant { get; }
        ITemplateVariant TemplateVariant { get; }
    }

    internal class ProjectVariantAssignments : IndexedCachedList<IProjectVariant, IProjectVariantAssignment>,
                                               IProjectVariantAssignments
    {
        private readonly IContentClass _contentClass;

        internal ProjectVariantAssignments(IContentClass contentClass, Caching caching)
            : base(assignment => assignment.ProjectVariant, Caching.Enabled)
        {
            _contentClass = contentClass;
            RetrieveFunc = GetAssignments;
        }

        public void Assign(ITemplateVariant templateVariant, IProjectVariant projectVariant)
        {
            Assign(new Dictionary<ITemplateVariant, IProjectVariant> {{templateVariant, projectVariant}});
        }

        public void Assign(IDictionary<ITemplateVariant, IProjectVariant> assignments)
        {
            const string ASSIGN_PROJECT_VARIANT =
                @"<TEMPLATE guid=""{0}""><TEMPLATEVARIANTS>{1}</TEMPLATEVARIANTS></TEMPLATE>";
            const string SINGLE_ASSIGNMENT =
                @"<TEMPLATEVARIANT guid=""{0}""><PROJECTVARIANTS action=""assign""><PROJECTVARIANT donotgenerate=""0"" donotusetidy=""0"" guid=""{1}"" /></PROJECTVARIANTS></TEMPLATEVARIANT>";

            var builder = new StringBuilder();
            foreach (var curEntry in assignments)
            {
                builder.Append(SINGLE_ASSIGNMENT.RQLFormat(curEntry.Key, curEntry.Value));
            }

            Project.ExecuteRQL(ASSIGN_PROJECT_VARIANT.RQLFormat(_contentClass, builder), RqlType.SessionKeyInProject);
            InvalidateCache();
        }

        public IContentClass ContentClass
        {
            get { return _contentClass; }
        }

        public IProject Project
        {
            get { return _contentClass.Project; }
        }

        public ISession Session
        {
            get { return _contentClass.Session; }
        }

        /// <summary>
        ///     Get all assignments of templates to project variants of this content class.
        /// </summary>
        private List<IProjectVariantAssignment> GetAssignments()
        {
            const string PROJECT_VARIANT_ASSIGNMENT =
                @"<TEMPLATE guid=""{0}"" ><TEMPLATEVARIANTS withstylesheets=""1"" action=""projectvariantslist"" /></TEMPLATE>";

            try
            {
                var xmlDoc = Project.ExecuteRQL(PROJECT_VARIANT_ASSIGNMENT.RQLFormat(_contentClass),
                                                RqlType.SessionKeyInProject);
                using (new CachingContext<IProjectVariant>(Project.ProjectVariants, Caching.Enabled))
                {
                    return (from XmlElement curElement in xmlDoc.GetElementsByTagName("TEMPLATEVARIANT")
                            select
                                (IProjectVariantAssignment)
                                new ProjectVariantAssignment(_contentClass,
                                                             Project.ProjectVariants.GetByGuid(
                                                                 curElement.GetGuid("projectvariantguid")),
                                                             new TemplateVariant(_contentClass, curElement.GetGuid())))
                        .ToList();
                }
            } catch (RQLException e)
            {
                throw new SmartAPIException(Session.ServerLogin, "Could not get project variant assignments", e);
            }
        }

        IEnumerable<IProjectVariantAssignment> IProjectVariantAssignments.this[ITemplateVariant templateVariant]
        {
            get
            {
                return
                    (from curAssignment in this
                     where curAssignment.TemplateVariant.Equals(templateVariant)
                     select curAssignment).ToList();
            }
        }
    }

    public interface IProjectVariantAssignments : IProjectObject,
                                                  IIndexedCachedList<IProjectVariant, IProjectVariantAssignment>
    {
        void Assign(ITemplateVariant templateVariant, IProjectVariant projectVariant);
        void Assign(IDictionary<ITemplateVariant, IProjectVariant> assignments);
        IContentClass ContentClass { get; }

        IEnumerable<IProjectVariantAssignment> this[ITemplateVariant templateVariant] { get; }
    }
}