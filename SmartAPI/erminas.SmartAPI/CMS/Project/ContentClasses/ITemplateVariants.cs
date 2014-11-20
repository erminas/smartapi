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
using erminas.SmartAPI.Exceptions;
using erminas.SmartAPI.Utils;
using erminas.SmartAPI.Utils.CachedCollections;

namespace erminas.SmartAPI.CMS.Project.ContentClasses
{
    internal class TemplateVariants : NameIndexedRDList<ITemplateVariant>, ITemplateVariants
    {
        private readonly IContentClass _contentClass;

        internal TemplateVariants(IContentClass contentClass, Caching caching) : base(caching)
        {
            _contentClass = contentClass;
            RetrieveFunc = GetTemplateVariants;
        }

        public IContentClass ContentClass
        {
            get { return _contentClass; }
        }

        public void CreateTemplateVariant(TemplateVariantCreationOptions options)
        {
            if (!options.IsValid)
            {
                throw new SmartAPIException(Session.ServerLogin, "Missing Name value for template creation");
            }

            const string CREATE_TEMPLATE_VARIANT = @"<TEMPLATE action=""assign"" guid=""{0}"">
<TEMPLATEVARIANTS action=""addnew"">
<TEMPLATEVARIANT name=""{1}"" description="""" code=""{2}"" fileextension=""{3}"" insertstylesheetinpage=""{4}"" nostartendmarkers=""{5}"" containerpagereference=""{6}"">{2}</TEMPLATEVARIANT>
</TEMPLATEVARIANTS>
</TEMPLATE>";

            var command = CREATE_TEMPLATE_VARIANT.SecureRQLFormat(ContentClass, options.Name, options.Data,
                options.FileExtension, options.IsStylesheetIncludedInHeader, !options.ContainsAreaMarksInPage,
                options.HasContainerPageReference);

            Project.ExecuteRQL(command, RqlType.SessionKeyInProject);

            InvalidateCache();
        }

        public IProject Project
        {
            get { return _contentClass.Project; }
        }

        public ISession Session
        {
            get { return _contentClass.Session; }
        }

        private List<ITemplateVariant> GetTemplateVariants()
        {
            const string LIST_CC_TEMPLATES =
                @"<PROJECT><TEMPLATE guid=""{0}""><TEMPLATEVARIANTS action=""list"" withstylesheets=""0""/></TEMPLATE></PROJECT>";
            var xmlDoc = Project.ExecuteRQL(LIST_CC_TEMPLATES.RQLFormat(_contentClass));
            var variants = xmlDoc.GetElementsByTagName("TEMPLATEVARIANT");
            return
                (from XmlElement curVariant in variants
                 select (ITemplateVariant) new TemplateVariant(_contentClass, curVariant)).ToList();
        }
    }

    public class TemplateVariantCreationOptions
    {
        public TemplateVariantCreationOptions(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
        public string FileExtension { get; set; }
        public bool IsStylesheetIncludedInHeader { get; set; }
        public bool HasContainerPageReference { get; set; }
        public bool ContainsAreaMarksInPage { get; set; }
        public bool IsValid { get { return Name != null; }}
        public string Data { get; set; }
    }

    public interface ITemplateVariants : IIndexedRDList<string, ITemplateVariant>, IProjectObject
    {
        IContentClass ContentClass { get; }
        void CreateTemplateVariant(TemplateVariantCreationOptions options);
    }
}