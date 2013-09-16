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
    public interface IMedia : IContentClassElement, ICanBeRequiredForEditing
    {
        int? AutomaticMaximumScalingHeight { get; set; }
        int? AutomaticMaximumScalingWidth { get; set; }
        int? ColorDepthInBit { get; set; }
        new void CommitInCurrentLanguage();
        new void CommitInLanguage(string languageAbbreviation);
        MediaConversionMode ConversionModeForSelectedDocuments { get; set; }

        /// <summary>
        ///     All eligible suffixes separated by ";"
        /// </summary>
        string EligibleSuffixes { get; set; }

        IFolder Folder { get; set; }

        bool IsConvertingOnlyNonWebCompatibleFiles { get; set; }
        bool IsDragAndDropActivated { get; set; }
        bool IsLanguageIndependent { get; set; }
        bool IsLinkNotAutomaticallyRemoved { get; set; }
        bool IsNotRelevantForWorklow { get; set; }
        bool IsNotUsedInForm { get; set; }
        bool IsScaledOrConverted { get; set; }
        int? MaxFileSizeInKB { get; set; }
        string Quality { get; set; }
        string RequiredNamePattern { get; set; }
        int? RequiredPictureHeight { get; set; }
        int? RequiredPictureWidth { get; set; }
        IFile SampleFile { get; set; }
        IFile SrcFile { get; set; }
        TargetFormat TargetFormat { get; set; }
    }

    internal class Media : ContentClassElement, IMedia
    {
        internal Media(IContentClass contentClass, XmlElement xmlElement) : base(contentClass, xmlElement)
        {
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

        public override ContentClassCategory Category
        {
            get { return ContentClassCategory.Content; }
        }

        [RedDot("eltpicdepth")]
        public int? ColorDepthInBit
        {
            get { return GetAttributeValue<int?>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltconvertmode", ConverterType = typeof (StringEnumConverter<MediaConversionMode>))]
        public MediaConversionMode ConversionModeForSelectedDocuments
        {
            get { return GetAttributeValue<MediaConversionMode>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltsuffixes")]
        public string EligibleSuffixes
        {
            get { return GetAttributeValue<string>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltfolderguid", ConverterType = typeof (FolderConverter))]
        public IFolder Folder
        {
            get { return GetAttributeValue<IFolder>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltonlynonwebsources")]
        public bool IsConvertingOnlyNonWebCompatibleFiles
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

        [RedDot("eltlanguageindependent")]
        public bool IsLanguageIndependent
        {
            get { return GetAttributeValue<bool>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltdonotremove")]
        public bool IsLinkNotAutomaticallyRemoved
        {
            get { return GetAttributeValue<bool>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltignoreworkflow")]
        public bool IsNotRelevantForWorklow
        {
            get { return GetAttributeValue<bool>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("elthideinform")]
        public bool IsNotUsedInForm
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
        public IFile SampleFile
        {
            get { return GetAttributeValue<IFile>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("__srcfile", ConverterType = typeof (SrcFileConverter))]
        public IFile SrcFile
        {
            get { return GetAttributeValue<IFile>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("elttargetformat", ConverterType = typeof (StringEnumConverter<TargetFormat>))]
        public TargetFormat TargetFormat
        {
            get { return GetAttributeValue<TargetFormat>(); }
            set { SetAttributeValue(value); }
        }

        #region ICanBeRequiredForEditing Members

        [RedDot("eltrequired")]
        public bool IsEditingMandatory
        {
            get { return GetAttributeValue<bool>(); }
            set { SetAttributeValue(value); }
        }

        #endregion
    }
}