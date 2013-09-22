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

using System.Collections.Generic;
using System.Linq;
using System.Xml;
using erminas.SmartAPI.CMS.Converter;
using erminas.SmartAPI.CMS.Project.Folder;

namespace erminas.SmartAPI.CMS.Project.ContentClasses.Elements
{
    public interface IImage : IExtendedContentClassContentElement
    {
        ImageAlignment Align { get; set; }
        AltType AltRequierement { get; set; }
        string AltText { get; set; }
        int? AutomaticMaximumScalingHeight { get; set; }
        int? AutomaticMaximumScalingWidth { get; set; }
        string Border { get; set; }
        int? ColorDepthInBit { get; set; }

        /// <summary>
        ///     All eligible suffixes separated by ";"
        /// </summary>
        string EligibleSuffixes { get; set; }

        IList<string> EligibleSuffixesList { get; set; }

        IFolder Folder { get; set; }
        string HSpace { get; set; }
        string HtmlHeight { get; set; }
        string HtmlWidth { get; set; }
        bool IsBorderAutomaticallyInsertedIntoPage { get; set; }
        bool IsDragAndDropActivated { get; set; }
        bool IsHeightAutomaticallyInsertedIntoPage { get; set; }
        bool IsOnlyPathAndFilenameInserted { get; set; }
        bool IsScaledOrConverted { get; set; }
        bool IsWidthAutomaticallyInsertedIntoPage { get; set; }
        int? MaxFileSizeInKB { get; set; }
        string Quality { get; set; }
        string RequiredNamePattern { get; set; }
        int? RequiredPictureHeight { get; set; }
        int? RequiredPictureWidth { get; set; }
        ILanguageDependentValue<IFile> SampleImage { get; }
        ILanguageDependentValue<IFile> SrcFile { get; }
        string Supplement { get; set; }
        TargetFormat TargetFormat { get; set; }
        string Usemap { get; set; }
        string VSpace { get; set; }
    }

    //todo share code with IMedia
    internal class Image : ExtendedContentClassContentElement, IImage
    {
        internal Image(IContentClass contentClass, XmlElement xmlElement) : base(contentClass, xmlElement)
        {
        }

        [RedDot("eltalign", ConverterType = typeof (StringEnumConverter<ImageAlignment>))]
        public ImageAlignment Align
        {
            get { return GetAttributeValue<ImageAlignment>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltpresetalt", ConverterType = typeof (EnumConverter<AltType>))]
        public AltType AltRequierement
        {
            get { return GetAttributeValue<AltType>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltalt")]
        public string AltText
        {
            get { return GetAttributeValue<string>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltmaxpicheight")]
        public int? AutomaticMaximumScalingHeight
        {
            get { return GetAttributeValue<int?>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltmaxpicwidth")]
        public int? AutomaticMaximumScalingWidth
        {
            get { return GetAttributeValue<int?>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltborder")]
        public string Border
        {
            get { return GetAttributeValue<string>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltpicdepth")]
        public int? ColorDepthInBit
        {
            get { return GetAttributeValue<int?>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltsuffixes")]
        public string EligibleSuffixes
        {
            get { return GetAttributeValue<string>(); }
            set { SetAttributeValue(value); }
        }

        public IList<string> EligibleSuffixesList
        {
            get
            {
                var suffixes = EligibleSuffixes;
                if (string.IsNullOrEmpty(suffixes))
                {
                    return new List<string>();
                }
                return suffixes.Split(';').Where(s => !string.IsNullOrEmpty(s)).ToList();
            }
            set { EligibleSuffixes = value == null ? null : string.Join(";", value); }
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
        public string HtmlHeight
        {
            get { return GetAttributeValue<string>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltwidth")]
        public string HtmlWidth
        {
            get { return GetAttributeValue<string>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltautoborder")]
        public bool IsBorderAutomaticallyInsertedIntoPage
        {
            get { return GetAttributeValue<bool>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltdragdrop")]
        public bool IsDragAndDropActivated
        {
            get { return GetAttributeValue<bool>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltautoheight")]
        public bool IsHeightAutomaticallyInsertedIntoPage
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

        [RedDot("eltconvert")]
        public bool IsScaledOrConverted
        {
            get { return GetAttributeValue<bool>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltautowidth")]
        public bool IsWidthAutomaticallyInsertedIntoPage
        {
            get { return GetAttributeValue<bool>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltmaxsize")]
        public int? MaxFileSizeInKB
        {
            get { return GetAttributeValue<int?>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltcompression")]
        public string Quality
        {
            get { return GetAttributeValue<string>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltfilename")]
        public string RequiredNamePattern
        {
            get { return GetAttributeValue<string>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltpicheight")]
        public int? RequiredPictureHeight
        {
            get { return GetAttributeValue<int?>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltpicwidth")]
        public int? RequiredPictureWidth
        {
            get { return GetAttributeValue<int?>(); }
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

        [RedDot("elttargetformat", ConverterType = typeof (StringEnumConverter<TargetFormat>))]
        public TargetFormat TargetFormat
        {
            get { return GetAttributeValue<TargetFormat>(); }
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
    }
}