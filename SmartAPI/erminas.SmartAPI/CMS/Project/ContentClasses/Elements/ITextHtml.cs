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
using System.Xml;
using erminas.SmartAPI.CMS.Converter;

namespace erminas.SmartAPI.CMS.Project.ContentClasses.Elements
{
    [Flags]
    public enum EditorSettings
    {
        NotSet = 0,
        FontBold = 1,
        FontItalic = 2,
        FontUnderline = 4,
        FontSize = 8,
        FontFace = 16,
        Superscript = 32,
        Subscript = 64,
        AlignLeft = 128,
        AlignMiddle = 256,
        AlignRight = 512,
        InsertTab = 1024,
        RemoveTab = 2048,
        FontForeColor = 4096,
        FontBackColor = 8192,
        List = 16384,
        InsertFormatted = 32768,
        InsertTable = 65536,
        InsertLink = 131072,
        InsertJumpMark = 262144,
        InsertImage = 524288,
        DefineJumpMark = 1048576,
        AlignJustify = 2097152,
        InsertHorizontalLine = 4194304,
        InsertExternURL = 8388608,
        EditTarget = 16777216,
        DoNotAllowWrapping = 33554432,
        SpecialCharTable = 67108864,
        DragDrop = 134217728,
        Acronym = 268435456,
        UserDefinedColors = 536870912,
        SpellChecking = 1073741824
    }

    public interface ITextHtml : IText
    {
        EditorSettings TextEditorSettings { get; set; }

        string FixedStyleSheet { get; set; }

        bool IncludeStyleSheetInPageHeader { get; set; }
    }

    internal class TextHtml : Text, ITextHtml
    {
        internal TextHtml(IContentClass contentClass, XmlElement xmlElement) : base(contentClass, xmlElement)
        {
            //TODO checken, ob die werte in editoroptions nicht invertiert enthalten sind
        }

        [RedDot("elteditoroptions", ConverterType = typeof (EnumConverter<EditorSettings>))]
        public EditorSettings TextEditorSettings
        {
            get { return GetAttributeValue<EditorSettings>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltstylesheetdata")]
        public string FixedStyleSheet
        {
            get { return GetAttributeValue<string>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltinsertstylesheetinpage")]
        public bool IncludeStyleSheetInPageHeader
        {
            get { return GetAttributeValue<bool>(); }
            set { SetAttributeValue(value); }
        }
    }
}