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
using System.Xml;
using erminas.SmartAPI.CMS.Administration.Language;
using erminas.SmartAPI.CMS.Project.ContentClasses.Elements.Attributes;

namespace erminas.SmartAPI.CMS.Project.ContentClasses.Elements
{
    public enum FileSizeUnit
    {
        Bytes = 0,
        KBytes,
        MBytes
    }

    public interface IAttribute : IContentClassElement
    {
        IDateTimeFormat DateTimeFormat { get; set; }
        FileSizeUnit FileSizeUnit { get; set; }
        ISystemLocale Locale { get; set; }
        string ReferencedElementName { get; set; }
        MediaTypeAttributeType SelectedAttributeType { get; set; }
    }

    internal class Attribute : ContentClassElement, IAttribute
    {
        internal Attribute(IContentClass contentClass, XmlElement xmlElement) : base(contentClass, xmlElement)
        {
            CreateAttributes("eltmediatypename", "eltmediatypeattribute", "eltlcid", "eltformatno");
// ReSharper disable ObjectCreationAsStatement
            new StringEnumXmlNodeAttribute<FileSizeUnit>(this, "eltformatting", x => x.ToString(),
                                                         // ReSharper restore ObjectCreationAsStatement
                                                         //emptybuffer -> workaround for error in server
                                                         x =>
                                                         x.Contains("EmptyBuffer")
                                                             ? default(FileSizeUnit)
                                                             : (FileSizeUnit) Enum.Parse(typeof (FileSizeUnit), x));
        }

        public override ContentClassCategory Category
        {
            get { return ContentClassCategory.Meta; }
        }

        public IDateTimeFormat DateTimeFormat
        {
            get { return ((DateTimeFormatAttribute) GetAttribute("eltformatno")).Value; }
            set { ((DateTimeFormatAttribute) GetAttribute("eltformatno")).Value = value; }
        }

        public FileSizeUnit FileSizeUnit
        {
            get { return ((StringEnumXmlNodeAttribute<FileSizeUnit>) GetAttribute("eltformatting")).Value; }
            set { ((StringEnumXmlNodeAttribute<FileSizeUnit>) GetAttribute("eltformatting")).Value = value; }
        }

        public ISystemLocale Locale
        {
            get { return ((LocaleXmlNodeAttribute) GetAttribute("eltlcid")).Value; }
            set { ((LocaleXmlNodeAttribute) GetAttribute("eltlcid")).Value = value; }
        }

        public string ReferencedElementName
        {
            get { return GetAttributeValue<string>("eltmediatypename"); }
            set { SetAttributeValue("eltmediatypename", value); }
        }

        public MediaTypeAttributeType SelectedAttributeType
        {
            get { return ((EnumXmlNodeAttribute<MediaTypeAttributeType>) GetAttribute("eltmediatypeattribute")).Value; }
            set { ((EnumXmlNodeAttribute<MediaTypeAttributeType>) GetAttribute("eltmediatypeattribute")).Value = value; }
        }
    }

    // ReSharper disable InconsistentNaming
    public enum MediaTypeAttributeType
    {
        None = 0,
        Application_name_OFFICE = 23,
        Author_OFFICE = 4,
        Author_PDF = 50,
        Bytes_OFFICE = 34,
        Category_OFFICE = 30,
        Changed_on_PDF = 48,
        Characters_OFFICE = 29,
        Characters_with_spaces_OFFICE = 41,
        Color_depth_IMG = 11,
        Comments_OFFICE = 16,
        Company_OFFICE = 33,
        Created_on_PDF = 3,
        Created_with_PDF = 46,
        Encrypted_PDF = 18,
        Entry_date = 7,
        File_name = 1,
        File_size = 2,
        Format_OFFICE = 31,
        Height_IMG = 9,
        Hidden_slides_OFFICE = 39,
        Keywords_PDF = 45,
        Last_editor = 6,
        Linearized_PDF = 19,
        Lines_OFFICE = 35,
        Manager_OFFICE = 32,
        Modification_date = 5,
        Multimedia_clips_OFFICE = 40,
        Notes_OFFICE = 38,
        Number_of_pages_PDF = 27,
        Original_author = 8,
        Paragraphs_OFFICE = 36,
        Revision_number_OFFICE = 22,
        Slides_OFFICE = 37,
        Subject_OFFICE = 20,
        Template_OFFICE = 21,
        Thumbnail_100x100 = 44,
        Thumbnail_50x50 = 43,
        Title_OFFICE = 12,
        Total_editing_time_OFFICE = 26,
        Width_IMG = 10,
        Words_OFFICE = 28,
    }

    // ReSharper restore InconsistentNaming
}