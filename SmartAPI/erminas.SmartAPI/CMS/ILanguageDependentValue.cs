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
using erminas.SmartAPI.CMS.Project.ContentClasses;
using erminas.SmartAPI.Exceptions;

namespace erminas.SmartAPI.CMS
{
    public interface ILanguageDependentValueBase
    {
        IPartialRedDotProjectObject Parent { get; }
    }

    public interface ILanguageDependentReadValue<out T> : ILanguageDependentValueBase
    {
        T ForCurrentLanguage { get; }
        T ForMainLanguage { get; }
        T this[ILanguageVariant languageVariant] { get; }
        T this[string languageAbbreviation] { get; }
    }

    public interface ILanguageDependentValue<T> : ILanguageDependentReadValue<T>
    {
        T ForAllLanguages { set; }
        new T ForCurrentLanguage { get; set; }
        new T ForMainLanguage { get; set; }
        new T this[ILanguageVariant languageVariant] { get; set; }
        new T this[string languageAbbreviation] { get; set; }
        void SetAllLanguagesFrom(ILanguageDependentValue<T> other);
    }

    internal abstract class AbstractLanguageDependendReadValue<T> : ILanguageDependentReadValue<T>
    {
        protected AbstractLanguageDependendReadValue(IPartialRedDotProjectObject parent)
        {
            Parent = parent;
        }

        public T ForCurrentLanguage
        {
            get { return this[Parent.Project.LanguageVariants.Current]; }
        }

        public T ForMainLanguage
        {
            get { return this[Parent.Project.LanguageVariants.Main]; }
        }

        public T this[ILanguageVariant languageVariant]
        {
            get { return this[languageVariant.Abbreviation]; }
        }

        public abstract T this[string languageAbbreviation] { get; }
        public IPartialRedDotProjectObject Parent { get; private set; }
    }

    internal class LanguageDependentValue<T> : ILanguageDependentValue<T>
    {
        private readonly RedDotAttribute _attribute;
        private readonly IPartialRedDotProjectObject _parent;

        public LanguageDependentValue(IPartialRedDotProjectObject parent, RedDotAttribute attribute)
        {
            _parent = parent;
            _attribute = attribute;
        }

        public T ForAllLanguages
        {
            set
            {
                foreach (var curLanguage in _parent.Project.LanguageVariants)
                {
                    this[curLanguage] = value;
                }
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

        public virtual T this[string languageAbbreviation]
        {
            get
            {
                var xmlElement = ((ILanguageDependentXmlBasedObject) _parent).GetElementForLanguage(languageAbbreviation);
                return _attribute.ReadFrom<T>(_parent, xmlElement);
            }
            set
            {
                var element = ((ILanguageDependentXmlBasedObject) _parent).GetElementForLanguage(languageAbbreviation);
                _attribute.WriteTo(_parent, element, value);
            }
        }

        public IPartialRedDotProjectObject Parent
        {
            get { return _parent; }
        }

        public void SetAllLanguagesFrom(ILanguageDependentValue<T> value)
        {
            CheckLanguageVariantCompatibility(value);
            foreach (var language in Parent.Project.LanguageVariants)
            {
                this[language] = value[language];
            }
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
                                                "Unable to assign values for all languages from project {0} to project {1} as the language variants are incompatible",
                                                value.Parent.Project, _parent.Project));
            }
        }
    }
}