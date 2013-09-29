using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using erminas.SmartAPI.CMS.ServerManagement;
using erminas.SmartAPI.Exceptions;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS.Project.Pages
{
    internal class PagePublishJob : IPagePublishJob
    {
        internal PagePublishJob(IPage page)
        {
            Page = page;
            LanguageVariants = new List<ILanguageVariant>();
            ProjectVariants = new List<IProjectVariant>();
        }

        public ISession Session { get { return Page.Session; } }
        public IProject Project { get { return Page.Project; } }
        public IPage Page { get; private set; }
        public bool IsPublishingAllFollowingPages { get; set; }
        public bool IsPublishingRelatedPages { get; set; }
        public bool IsSendingEmailOnCompletion { get; set; }
        public IUser EMailReceipient { get; set; }
        public IApplicationServer Server { get; set; }
        public DateTime PublishAccordingTo { get; set; }
        public DateTime PublishOn { get; set; }
        public IEnumerable<ILanguageVariant> LanguageVariants { get; set; }
        public IEnumerable<IProjectVariant> ProjectVariants { get; set; }
        public void RunAsync()
        {
            CheckValidity();

            const string PUBLISH = @"<PROJECT guid=""{0}"" sessionkey=""{10}""><PAGE guid=""{1}""><EXPORTJOB action=""save"" email=""{2}"" toppriority=""0"" generatenextpages=""{3}"" generaterelativepages=""{4}"" reddotserver=""{5}"" application="""" generatedate=""{6}"" startgenerationat=""{7}""><LANGUAGEVARIANTS action=""checkassigning"">{8}</LANGUAGEVARIANTS><PROJECTVARIANTS action=""checkassigning"">{9}</PROJECTVARIANTS></EXPORTJOB></PAGE></PROJECT>";
            
            var languageVariants = GetLanguageVariantsString();
            var projectVariants = GetProjectVariantsString();

            var publishAccordingToString = ToRQLString(PublishAccordingTo);
            var publishOnString = ToRQLString(PublishOn);

            var query = PUBLISH.RQLFormat(Project, Page, IsSendingEmailOnCompletion ? EMailReceipient : null,
                                          IsPublishingAllFollowingPages, IsPublishingRelatedPages, Server, publishAccordingToString,
                                          publishOnString, languageVariants, projectVariants, Session.SessionKey);

            Project.ExecuteRQL(query);
        }

        private void CheckValidity()
        {
            if (LanguageVariants == null || !LanguageVariants.Any())
            {
                throw new SmartAPIException(Session.ServerLogin,
                                            string.Format(
                                                "Missing language variant assignment for the publication of page {0}", Page));
            }

            if (ProjectVariants == null || !ProjectVariants.Any())
            {
                throw new SmartAPIException(Session.ServerLogin,
                                            string.Format("Missing project variant assignment for the publication of page {0}",
                                                          Page));
            }
        }

        private static string ToRQLString(DateTime dt)
        {
            return dt != default(DateTime)
                       ? dt.ToString("M/d/yyyy", CultureInfo.InvariantCulture)
                       : RQL.SESSIONKEY_PLACEHOLDER;
        }

        private string GetLanguageVariantsString()
        {
            const string SINGLE_LANGUAGE_VARIANT = @"<LANGUAGEVARIANT guid=""{0}"" checked=""{1}""/>";

            var checkedLanguageVariants = LanguageVariants.Aggregate("",
                                                                     (s, variant) =>
                                                                     s + SINGLE_LANGUAGE_VARIANT.RQLFormat(variant.Guid.ToRQLString(), true));
            var uncheckedLanguageVariants = Project.LanguageVariants.Except(LanguageVariants)
                                                   .Aggregate("",
                                                              (s, variant) =>
                                                              s + SINGLE_LANGUAGE_VARIANT.RQLFormat(variant.Guid.ToRQLString(), false));

            var languageVariants = checkedLanguageVariants + uncheckedLanguageVariants;
            return languageVariants;
        }

        private string GetProjectVariantsString()
        {
            const string SINGLE_PROJECT_VARIANT = @"<PROJECTVARIANT guid=""{0}"" checked=""{1}""/>";

            var checkedProjectVariants = ProjectVariants.Aggregate("",
                                                                   (s, variant) =>
                                                                   s + SINGLE_PROJECT_VARIANT.RQLFormat(variant, true));

            var uncheckedProjectVariants = Project.ProjectVariants.Except(ProjectVariants)
                                                  .Aggregate("",
                                                             (s, variant) =>
                                                             s + SINGLE_PROJECT_VARIANT.RQLFormat(variant, false));
            var projectVariants = checkedProjectVariants + uncheckedProjectVariants;
            return projectVariants;
        }
    }

    public interface IPagePublishJob : IProjectObject
    {
        IPage Page { get; }
        
        bool IsPublishingAllFollowingPages { get; set; }
        bool IsPublishingRelatedPages { get; set; }
        bool IsSendingEmailOnCompletion { get; set; }

        IUser EMailReceipient { get; set; }
        IApplicationServer Server { get; set; }
        
        DateTime PublishAccordingTo { get; set; }
        DateTime PublishOn { get; set; }
        IEnumerable<ILanguageVariant> LanguageVariants{ get; set; }
        IEnumerable<IProjectVariant> ProjectVariants { get; set; }

        void RunAsync();
    }
}