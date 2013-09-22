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
using erminas.SmartAPI.CMS.Converter;
using erminas.SmartAPI.CMS.Project.Folder;

namespace erminas.SmartAPI.CMS.Project.ContentClasses.Elements
{
    public enum Direction
    {
        Forward = 0,
        Back = 1
    }

    public enum Appearance
    {
        StandardField = 0,
        Image = 1
    }

    public interface IBrowse : IContentClassElement
    {
        BrowseAlignment Align { get; set; }
        string AltText { get; set; }
        Appearance Appearance { get; set; }
        string Border { get; set; }
        string DefaultValue { get; set; }
        string DescriptionInCurrentDisplayLanguage { get; set; }
        Direction Direction { get; set; }
        IFolder Folder { get; set; }
        string HSpace { get; set; }
        string Height { get; set; }
        bool IsAltPreassignedAutomatically { get; set; }
        bool IsLanguageIndependent { get; set; }
        bool IsOnlyPathAndFilenameInserted { get; set; }
        bool IsSyntaxConformingToXHtml { get; set; }
        ILanguageDependentValue<IFile> SampleImage { get; }
        ILanguageDependentValue<IFile> SrcFile { get; }
        string Supplement { get; set; }
        string Usemap { get; set; }
        string VSpace { get; set; }
        string Width { get; set; }
    }

    internal class Browse : ContentClassElement, IBrowse
    {
        internal Browse(IContentClass contentClass, XmlElement xmlElement) : base(contentClass, xmlElement)
        {
        }

        [RedDot("eltalign", ConverterType = typeof (StringEnumConverter<BrowseAlignment>))]
        public BrowseAlignment Align
        {
            get { return GetAttributeValue<BrowseAlignment>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltalt")]
        public string AltText
        {
            get { return GetAttributeValue<string>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltnextpagetype", ConverterType = typeof (EnumConverter<Appearance>))]
        public Appearance Appearance
        {
            get { return GetAttributeValue<Appearance>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltborder")]
        public string Border
        {
            get { return GetAttributeValue<string>(); }
            set { SetAttributeValue(value); }
        }

        public override ContentClassCategory Category
        {
            get { return ContentClassCategory.Structural; }
        }

        [RedDot("eltdefaultvalue")]
        public string DefaultValue
        {
            get { return GetAttributeValue<string>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltrddescription")]
        public string DescriptionInCurrentDisplayLanguage
        {
            get { return GetAttributeValue<string>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltdirection", ConverterType = typeof (EnumConverter<Direction>))]
        public Direction Direction
        {
            get { return GetAttributeValue<Direction>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltfolderguid", ConverterType = typeof (FolderConverter))]
        public IFolder Folder
        {
            get { return GetAttributeValue<IFolder>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("elthspace")]
        public string HSpace
        {
            get { return GetAttributeValue<string>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltheight")]
        public string Height
        {
            get { return GetAttributeValue<string>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltpresetalt")]
        public bool IsAltPreassignedAutomatically
        {
            get { return GetAttributeValue<bool>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltlanguageindependent")]
        public bool IsLanguageIndependent
        {
            get { return GetAttributeValue<bool>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltonlyhrefvalue")]
        public bool IsOnlyPathAndFilenameInserted
        {
            get { return GetAttributeValue<bool>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltxhtmlcompliant")]
        public bool IsSyntaxConformingToXHtml
        {
            get { return GetAttributeValue<bool>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("__examplefile", ConverterType = typeof (ExampleFileConverter), DependsOn = "eltfolderguid")]
        public ILanguageDependentValue<IFile> SampleImage
        {
            get { return GetAttributeValue<ILanguageDependentValue<IFile>>(); }
        }

        [RedDot("__srcfile", ConverterType = typeof (SrcFileConverter))]
        public ILanguageDependentValue<IFile> SrcFile
        {
            get { return GetAttributeValue<ILanguageDependentValue<IFile>>(); }
        }

        [RedDot("eltsupplement")]
        public string Supplement
        {
            get { return GetAttributeValue<string>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltusermap")]
        public string Usemap
        {
            get { return GetAttributeValue<string>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltvspace")]
        public string VSpace
        {
            get { return GetAttributeValue<string>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltwidth")]
        public string Width
        {
            get { return GetAttributeValue<string>(); }
            set { SetAttributeValue(value); }
        }
    }
}