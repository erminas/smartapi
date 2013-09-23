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

using System.Xml;

namespace erminas.SmartAPI.CMS.Project
{
    public interface ILanguageVariant : IRedDotObject, IProjectObject
    {
        string Abbreviation { get; }
        bool IsCurrentLanguageVariant { get; }
        bool IsMainLanguage { get; }
        void Select();
    }

    internal class LanguageVariant : RedDotProjectObject, ILanguageVariant
    {
        private string _abbreviation;
        private bool _isCurrentLanguageVariant;
        private bool _isMainLanguage;

        internal LanguageVariant(IProject project, XmlElement xmlElement) : base(project, xmlElement)
        {
            LoadXml();
        }

        public string Abbreviation
        {
            get { return _abbreviation; }
        }

        public bool IsCurrentLanguageVariant
        {
            get { return _isCurrentLanguageVariant; }
            internal set { _isCurrentLanguageVariant = value; }
        }

        public bool IsMainLanguage
        {
            get { return _isMainLanguage; }
        }

        public void Select()
        {
            Project.LanguageVariants.Current = this;
        }

        private void LoadXml()
        {
            InitIfPresent(ref _isCurrentLanguageVariant, "checked", BoolConvert);
            InitIfPresent(ref _abbreviation, "language", x => x);
            InitIfPresent(ref _isMainLanguage, "ismainlanguage", BoolConvert);
        }
    }
}