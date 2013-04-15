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

using System.Linq;
using System.Xml;
using erminas.SmartAPI.CMS.Project.ContentClasses.Elements.Attributes;
using erminas.SmartAPI.CMS.Project.Filesystem;

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
// todo use list<string> instead
        string EligibleSuffixes { get; set; }

        string HSpace { get; set; }
        string HtmlHeight { get; set; }
        string HtmlWidth { get; set; }
        bool IsBorderAutomaticallyInsertedIntoPage { get; set; }
        bool IsHeightAutomaticallyInsertedIntoPage { get; set; }
        bool IsOnlyPathAndFilenameInserted { get; set; }
        bool IsScaledOrConverted { get; set; }
        bool IsWidthAutomaticallyInsertedIntoPage { get; set; }
        int? MaxFileSizeInKB { get; set; }
        string Quality { get; set; }
        string RequiredNamePattern { get; set; }
        int? RequiredPictureHeight { get; set; }
        int? RequiredPictureWidth { get; set; }
        IFile SrcFile { get; set; }
        string Supplement { get; set; }
        TargetFormat TargetFormat { get; set; }
        string Usemap { get; set; }
        string VSpace { get; set; }
    }

    internal class Image : ExtendedContentClassContentElement, IImage
    {
        internal Image(IContentClass contentClass, XmlElement xmlElement) : base(contentClass, xmlElement)
        {
            CreateAttributes("eltonlyhrefvalue", "eltmaxsize", "eltconvert", "elttargetformat", "eltmaxpicwidth",
                             "eltmaxpicheight", "eltpicwidth", "eltpicheight", "eltpicdepth", "eltfilename",
                             "eltsuffixes", "eltcompression", "eltfolderguid", "eltrdexamplesubdirguid", "eltrdexample",
                             "eltdragdrop", "eltwidth", "eltheight", "eltborder", "eltautoborder", "eltautowidth",
                             "eltautoheight", "eltvspace", "elthspace", "eltalt", "eltusermap", "eltsupplement",
                             "eltsrcsubdirguid", "eltsrc");
// ReSharper disable ObjectCreationAsStatement
            new EnumXmlNodeAttribute<AltType>(this, "eltpresetalt");
            new StringEnumXmlNodeAttribute<ImageAlignment>(this, "eltalign", ImageAlignmentUtils.ToRQLString,
                                                           ImageAlignmentUtils.ToImageAlignment);
// ReSharper restore ObjectCreationAsStatement
            //todo how to handle folder/subdirfolder ...
        }

        public ImageAlignment Align
        {
            get { return ((StringEnumXmlNodeAttribute<ImageAlignment>) GetAttribute("eltalign")).Value; }
            set { ((StringEnumXmlNodeAttribute<ImageAlignment>) GetAttribute("eltalign")).Value = value; }
        }

        public AltType AltRequierement
        {
            get { return ((EnumXmlNodeAttribute<AltType>) GetAttribute("eltpresetalt")).Value; }
            set { ((EnumXmlNodeAttribute<AltType>) GetAttribute("eltpresetalt")).Value = value; }
        }

        public string AltText
        {
            get { return GetAttributeValue<string>("eltalt"); }
            set { SetAttributeValue("eltalt", value); }
        }

        public int? AutomaticMaximumScalingHeight
        {
            get
            {
                string tmp = ((StringXmlNodeAttribute) GetAttribute("eltmaxpicheight")).Value;
                return string.IsNullOrEmpty(tmp) ? (int?) null : int.Parse(tmp);
            }
            set { ((StringXmlNodeAttribute) GetAttribute("eltmaxpicheight")).Value = value.ToString(); }
        }

        public int? AutomaticMaximumScalingWidth
        {
            get
            {
                string tmp = ((StringXmlNodeAttribute) GetAttribute("eltmaxpicwidth")).Value;
                return string.IsNullOrEmpty(tmp) ? (int?) null : int.Parse(tmp);
            }
            set { ((StringXmlNodeAttribute) GetAttribute("eltmaxpicwidth")).Value = value.ToString(); }
        }

        public string Border
        {
            get { return GetAttributeValue<string>("eltborder"); }
            set { SetAttributeValue("eltborder", value); }
        }

        public int? ColorDepthInBit
        {
            get
            {
                string tmp = ((StringXmlNodeAttribute) GetAttribute("eltpicdepth")).Value;
                return string.IsNullOrEmpty(tmp) ? (int?) null : int.Parse(tmp);
            }
            set { ((StringXmlNodeAttribute) GetAttribute("eltpicdepth")).Value = value.ToString(); }
        }

        /// <summary>
        ///     All eligible suffixes separated by ";"
        /// </summary>
        // todo use list<string> instead
        public string EligibleSuffixes
        {
            get { return GetAttributeValue<string>("eltsuffixes"); }
            set { SetAttributeValue("eltsuffixes", value); }
        }

        public string HSpace
        {
            get { return GetAttributeValue<string>("elthspace"); }
            set { SetAttributeValue("elthspace", value); }
        }

        public string HtmlHeight
        {
            get { return GetAttributeValue<string>("eltheight"); }
            set { SetAttributeValue("eltheight", value); }
        }

        public string HtmlWidth
        {
            get { return GetAttributeValue<string>("eltwidth"); }
            set { SetAttributeValue("eltwidth", value); }
        }

        public bool IsBorderAutomaticallyInsertedIntoPage
        {
            get { return GetAttributeValue<bool>("eltautoborder"); }
            set { SetAttributeValue("eltautoborder", value); }
        }

        public bool IsHeightAutomaticallyInsertedIntoPage
        {
            get { return GetAttributeValue<bool>("eltautoheight"); }
            set { SetAttributeValue("eltautoheight", value); }
        }

        public bool IsOnlyPathAndFilenameInserted
        {
            get { return GetAttributeValue<bool>("eltonlyhrefvalue"); }
            set { SetAttributeValue("eltonlyhrefvalue", value); }
        }

        public bool IsScaledOrConverted
        {
            get { return GetAttributeValue<bool>("eltconvert"); }
            set { SetAttributeValue("eltconvert", value); }
        }

        public bool IsWidthAutomaticallyInsertedIntoPage
        {
            get { return GetAttributeValue<bool>("eltautowidth"); }
            set { SetAttributeValue("eltautowidth", value); }
        }

        public int? MaxFileSizeInKB
        {
            get
            {
                string tmp = ((StringXmlNodeAttribute) GetAttribute("eltmaxsize")).Value;
                return string.IsNullOrEmpty(tmp) ? (int?) null : int.Parse(tmp);
            }
            set { ((StringXmlNodeAttribute) GetAttribute("eltmaxsize")).Value = value.ToString(); }
        }

        public string Quality
        {
            get { return GetAttributeValue<string>("eltcompression"); }
            set { SetAttributeValue("eltcompression", value); }
        }

        public string RequiredNamePattern
        {
            get { return GetAttributeValue<string>("eltfilename"); }
            set { SetAttributeValue("eltfilename", value); }
        }

        public int? RequiredPictureHeight
        {
            get
            {
                string tmp = ((StringXmlNodeAttribute) GetAttribute("eltpicheight")).Value;
                return string.IsNullOrEmpty(tmp) ? (int?) null : int.Parse(tmp);
            }
            set { ((StringXmlNodeAttribute) GetAttribute("eltpicheight")).Value = value.ToString(); }
        }

        public int? RequiredPictureWidth
        {
            get
            {
                string tmp = ((StringXmlNodeAttribute) GetAttribute("eltpicwidth")).Value;
                return string.IsNullOrEmpty(tmp) ? (int?) null : int.Parse(tmp);
            }
            set { ((StringXmlNodeAttribute) GetAttribute("eltpicwidth")).Value = value.ToString(); }
        }

        public IFile SrcFile
        {
            get
            {
                var folderAttr = (FolderXmlNodeAttribute) GetAttribute("eltsrcsubdirguid");
                string srcName = ((StringXmlNodeAttribute) GetAttribute("eltsrc")).Value;
                if (folderAttr.Value == null || string.IsNullOrEmpty(srcName))
                {
                    return null;
                }
                return folderAttr.Value.GetFilesByNamePattern(srcName).First(x => x.Name == srcName);
            }

            set
            {
                ((StringXmlNodeAttribute) GetAttribute("eltsrc")).Value = value != null ? value.Name : "";
                if (value != null)
                {
                    ((FolderXmlNodeAttribute) GetAttribute("eltsrcsubdirguid")).Value = value.Folder;
                }
            }
        }

        public string Supplement
        {
            get { return GetAttributeValue<string>("eltsupplement"); }
            set { SetAttributeValue("eltsupplement", value); }
        }

        public TargetFormat TargetFormat
        {
            get { return ((StringEnumXmlNodeAttribute<TargetFormat>) GetAttribute("elttargetformat")).Value; }
            set { ((StringEnumXmlNodeAttribute<TargetFormat>) GetAttribute("elttargetformat")).Value = value; }
        }

        public string Usemap
        {
            get { return GetAttributeValue<string>("eltusermap"); }
            set { SetAttributeValue("eltusermap", value); }
        }

        public string VSpace
        {
            get { return GetAttributeValue<string>("eltvspace"); }
            set { SetAttributeValue("eltvspace", value); }
        }
    }
}