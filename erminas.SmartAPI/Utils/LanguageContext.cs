using System;
using erminas.SmartAPI.CMS;

namespace erminas.SmartAPI.Utils
{
    /// <summary>
    ///   Utility class to execute code in a specific language variant and restore the original selection of the language variant afterwards.
    /// </summary>
    /// <example>
    ///   project.LanguageVariants["ENG"].Select(); ... using(new LanguageContext(project.LanguageVariants["DEU"])) { //the following code is executed with the German language variant selected ... } //the following code is executed with the UK English language variant selected ...
    /// </example>
    public class LanguageContext : IDisposable
    {
        private readonly LanguageVariant _origLang;

        /// <summary>
        ///   Selects a language variant and restores the previously selected language variant on dispose.
        /// </summary>
        /// <param name="lang"> </param>
        public LanguageContext(LanguageVariant lang)
        {
            _origLang = lang.Project.CurrentLanguageVariant;

            lang.Select();
        }

        /// <summary>
        ///   Just restores the language context on dispose. Does not set a specific language variant on construction.
        /// </summary>
        public LanguageContext(Project project)
        {
            _origLang = project.CurrentLanguageVariant;
        }

        #region IDisposable Members

        public void Dispose()
        {
            _origLang.Select();
        }

        #endregion
    }
}