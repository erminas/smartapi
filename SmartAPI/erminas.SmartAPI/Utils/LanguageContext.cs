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
using erminas.SmartAPI.CMS.Project;

namespace erminas.SmartAPI.Utils
{
    /// <summary>
    ///     Utility class to execute code in a specific language variant and restore the original selection of the language variant afterwards.
    /// </summary>
    /// <example>
    ///     project.LanguageVariants["ENG"].Select(); ... using(new LanguageContext(project.LanguageVariants["DEU"])) { //the following code is executed with the German language variant selected ... } //the following code is executed with the UK English language variant selected ...
    /// </example>
    public sealed class LanguageContext : IDisposable
    {
        private readonly ILanguageVariant _origLang;

        /// <summary>
        ///     Selects a language variant and restores the previously selected language variant on dispose.
        /// </summary>
        /// <param name="lang"> </param>
        public LanguageContext(ILanguageVariant lang)
        {
            _origLang = lang.Project.LanguageVariants.Current;

            lang.Select();
        }

        /// <summary>
        ///     Just restores the language context on dispose. Does not set a specific language variant on construction.
        /// </summary>
        public LanguageContext(Project project)
        {
            _origLang = project.LanguageVariants.Current;
        }

        #region IDisposable Members

        public void Dispose()
        {
            _origLang.Select();
        }

        #endregion
    }
}