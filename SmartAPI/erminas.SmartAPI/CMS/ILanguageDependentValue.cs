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

using System.Linq;
using erminas.SmartAPI.CMS.Project;
using erminas.SmartAPI.Exceptions;

namespace erminas.SmartAPI.CMS
{
    internal class LanguageDependentValue<T> : ILanguageDependentValue<T>
    {
        private readonly RedDotAttribute _attribute;
        private readonly ILanguageDependentPartialRedDotObject _parent;

        public LanguageDependentValue(ILanguageDependentPartialRedDotObject parent, RedDotAttribute attribute)
        {
            _parent = parent;
            _attribute = attribute;
        }

        public void AssignForAllLanguages(ILanguageDependentValue<T> value)
        {
            CheckLanguageVariantCompatibility(value);
            foreach (var language in Parent.Project.LanguageVariants)
            {
                this[language] = value[language];
            }
        }

        public T ForCurrentLanguage
        {
            get { return this[_parent.Project.LanguageVariants.Current]; }
            set { this[_parent.Project.LanguageVariants.Current] = value; }
        }

        public T ForMainLanguage
        {
            get { return this[_parent.Project.LanguageVariants.Main]; }
            set { this[_parent.Project.LanguageVariants.Main] = value; }
        }

        public T this[ILanguageVariant languageVariant]
        {
            get { return this[languageVariant.Abbreviation]; }
            set { this[languageVariant.Abbreviation] = value; }
        }

        public T this[string languageAbbreviation]
        {
            get
            {
                var xmlElement = _parent.GetXmlElementForLanguage(languageAbbreviation);
                return _attribute.ReadFrom<T>(_parent, xmlElement);
            }
            set
            {
                var xmlElement = _parent.GetXmlElementForLanguage(languageAbbreviation);
                _attribute.WriteTo(_parent, xmlElement, value);
            }
        }

        public ILanguageDependentPartialRedDotObject Parent
        {
            get { return _parent; }
        }

        private void CheckLanguageVariantCompatibility(ILanguageDependentValue<T> value)
        {
            if (_parent.Session == value.Parent.Session && _parent.Project.Equals(value.Parent.Project))
            {
                return;
            }
            if (
                Parent.Project.LanguageVariants.Any(
                    lang => !value.Parent.Project.LanguageVariants.ContainsName(lang.Name)))
            {
                throw new SmartAPIException(Parent.Session.ServerLogin,
                                            string.Format(
                                                "Unable to assign values for all languages from project {0} to project {1} as the language variants are incompatible"));
            }
        }
    }

    public interface ILanguageDependentValueBase
    {
        ILanguageDependentPartialRedDotObject Parent { get; }
    }

    public interface ILanguageDependentValue<T> : ILanguageDependentValueBase
    {
        void AssignForAllLanguages(ILanguageDependentValue<T> value);
        T ForCurrentLanguage { get; set; }
        T ForMainLanguage { get; set; }
        T this[ILanguageVariant languageVariant] { get; set; }
        T this[string languageAbbreviation] { get; set; }
    }
}