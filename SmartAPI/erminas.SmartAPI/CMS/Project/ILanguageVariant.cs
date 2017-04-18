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

using System.ComponentModel;
using System.Xml;
using erminas.SmartAPI.CMS.Converter;
using erminas.SmartAPI.Exceptions;
using erminas.SmartAPI.Utils;

namespace erminas.SmartAPI.CMS.Project
{
    public interface ILanguageVariant : IRedDotObject, IProjectObject, IDeletable
    {
        new string Name { get; set; }
        string Abbreviation { get; set; }

        string RFCLanguageId { get; set; }
        bool IsCurrentLanguageVariant { get; }
        bool IsMainLanguage { get; set; }
        void Select();

        void Commit();
    }

    internal class LanguageVariant : RedDotProjectObject, ILanguageVariant
    {
        private bool _isCurrentLanguageVariant;

        internal LanguageVariant(IProject project, XmlElement xmlElement) : base(project, xmlElement)
        {
            LoadXml();
        }

        [RedDot("name")]
        public string Name
        {
            get { return GetAttributeValue<string>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("language")]
        public string Abbreviation
        {

            get { return GetAttributeValue<string>(); }
            set { SetAttributeValue(value);}
        }

        public bool IsCurrentLanguageVariant
        {
            get { return _isCurrentLanguageVariant; }
            internal set { _isCurrentLanguageVariant = value; }
        }
        
        [RedDot("ismainlanguage")]
        public bool IsMainLanguage
        {
            get { return GetAttributeValue<bool>(); }
            set { SetAttributeValue(value);}
        }

        [RedDot("rfclanguageid")]
        public string RFCLanguageId{ get { return GetAttributeValue<string>(); } set{SetAttributeValue(value);} }

        [RedDot("textdirection", ConverterType = typeof(StringEnumConverter<TextDirection>))]
        public TextDirection TextDirection { get { return GetAttributeValue<TextDirection>(); } }

        public void Select()
        {
            Project.LanguageVariants.Current = this;
        }

        private void LoadXml()
        {
            InitIfPresent(ref _isCurrentLanguageVariant, "checked", BoolConvert);
        }

        public void Delete()
        {
            const string DELETE_LANGUAGE_VARIANT = @"<LANGUAGEVARIANTS action=""delete""><LANGUAGEVARIANT guid=""{0}"" /></LANGUAGEVARIANTS>";
            var xmlDoc = Project.ExecuteRQL(DELETE_LANGUAGE_VARIANT.RQLFormat(Guid.ToRQLString()), RqlType.SessionKeyInProject);
            if (!xmlDoc.IsContainingOk())
            {
                throw new SmartAPIException(Session.ServerLogin, string.Format("Could not delete language variant {0}", this));
            }
        }

        public void Commit()
        {
            var saveString = GetSaveString(XmlReadWriteWrapper.MergedElement);
            //TODO is das mit default beim schreiben nur bei neueren versionen  so?
            saveString = saveString.Replace("ismainlanguage", "defaultlanguagevariant");
            var xmlDoc = Project.ExecuteRQL(saveString, RqlType.SessionKeyInProject);

            Project.LanguageVariants.InvalidateCache();

            if (xmlDoc.GetElementsByTagName("LANGUAGEVARIANT").Count != 1)
            {
                throw new SmartAPIException(Session.ServerLogin, string.Format("Could not change language variant {0}", this));
            }
        }
    }
}