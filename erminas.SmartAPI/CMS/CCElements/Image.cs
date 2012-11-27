/*
 * Smart API - .Net programatical access to RedDot servers
 * Copyright (C) 2012  erminas GbR 
 *
 * This program is free software: you can redistribute it and/or modify it 
 * under the terms of the GNU General Public License as published by the Free Software Foundation,
 * either version 3 of the License, or (at your option) any later version.
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the GNU General Public License for more details. 
 *
 * You should have received a copy of the GNU General Public License along with this program.
 * If not, see <http://www.gnu.org/licenses/>. 
 */

using System.Linq;
using System.Xml;
using erminas.SmartAPI.CMS.CCElements.Attributes;

namespace erminas.SmartAPI.CMS.CCElements
{
    public class Image : CCExtendedContentElement
    {
        public Image(ContentClass cc, XmlNode node)
            : base(cc, node)
        {
            CreateAttributes("eltonlyhrefvalue", "eltmaxsize", "eltconvert", "elttargetformat",
                             "eltmaxpicwidth", "eltmaxpicheight", "eltpicwidth", "eltpicheight", "eltpicdepth",
                             "eltfilename", "eltsuffixes", "eltcompression",
                             "eltfolderguid", "eltrdexamplesubdirguid",
                             "eltrdexample", "eltdragdrop",
                             "eltwidth", "eltheight",
                             "eltborder", "eltautoborder", "eltautowidth",
                             "eltautoheight", "eltvspace", "elthspace",
                             "eltalt",
                             "eltusermap", "eltsupplement", "eltsrcsubdirguid",
                             "eltsrc");
            new EnumXmlNodeAttribute<AltType>(this, "eltpresetalt");
            new StringEnumXmlNodeAttribute<ImageAlignment>(this, "eltalign", ImageAlignmentUtils.ToRQLString,
                                                           ImageAlignmentUtils.ToImageAlignment);
            //todo how to handle folder/subdirfolder ...
        }

        public string Supplement
        {
            get { return ((StringXmlNodeAttribute) GetAttribute("eltsupplement")).Value; }
            set { ((StringXmlNodeAttribute) GetAttribute("eltsupplement")).Value = value; }
        }

        public string VSpace
        {
            get { return ((StringXmlNodeAttribute) GetAttribute("eltvspace")).Value; }
            set { ((StringXmlNodeAttribute) GetAttribute("eltvspace")).Value = value; }
        }

        public string HSpace
        {
            get { return ((StringXmlNodeAttribute) GetAttribute("elthspace")).Value; }
            set { ((StringXmlNodeAttribute) GetAttribute("elthspace")).Value = value; }
        }


        public ImageAlignment Align
        {
            get { return ((StringEnumXmlNodeAttribute<ImageAlignment>) GetAttribute("eltalign")).Value; }
            set { ((StringEnumXmlNodeAttribute<ImageAlignment>) GetAttribute("eltalign")).Value = value; }
        }

        public string Usemap
        {
            get { return ((StringXmlNodeAttribute) GetAttribute("eltusermap")).Value; }
            set { ((StringXmlNodeAttribute) GetAttribute("eltusermap")).Value = value; }
        }

        public AltType AltRequierement
        {
            get { return ((EnumXmlNodeAttribute<AltType>) GetAttribute("eltpresetalt")).Value; }
            set { ((EnumXmlNodeAttribute<AltType>) GetAttribute("eltpresetalt")).Value = value; }
        }

        public string AltText
        {
            get { return ((StringXmlNodeAttribute) GetAttribute("eltalt")).Value; }
            set { ((StringXmlNodeAttribute) GetAttribute("eltalt")).Value = value; }
        }

        public bool IsHeightAutomaticallyInsertedIntoPage
        {
            get { return ((BoolXmlNodeAttribute) GetAttribute("eltautoheight")).Value; }
            set { ((BoolXmlNodeAttribute) GetAttribute("eltautoheight")).Value = value; }
        }

        public bool IsWidthAutomaticallyInsertedIntoPage
        {
            get { return ((BoolXmlNodeAttribute) GetAttribute("eltautowidth")).Value; }
            set { ((BoolXmlNodeAttribute) GetAttribute("eltautowidth")).Value = value; }
        }

        public bool IsBorderAutomaticallyInsertedIntoPage
        {
            get { return ((BoolXmlNodeAttribute) GetAttribute("eltautoborder")).Value; }
            set { ((BoolXmlNodeAttribute) GetAttribute("eltautoborder")).Value = value; }
        }

        public string Border
        {
            get { return ((StringXmlNodeAttribute) GetAttribute("eltborder")).Value; }
            set { ((StringXmlNodeAttribute) GetAttribute("eltborder")).Value = value; }
        }

        public string HtmlHeight
        {
            get { return ((StringXmlNodeAttribute) GetAttribute("eltheight")).Value; }
            set { ((StringXmlNodeAttribute) GetAttribute("eltheight")).Value = value; }
        }

        public string HtmlWidth
        {
            get { return ((StringXmlNodeAttribute) GetAttribute("eltwidth")).Value; }
            set { ((StringXmlNodeAttribute) GetAttribute("eltwidth")).Value = value; }
        }

        /// <summary>
        ///   All eligible suffixes separated by ";"
        /// </summary>
        // todo use list<string> instead
        public string EligibleSuffixes
        {
            get { return ((StringXmlNodeAttribute) GetAttribute("eltsuffixes")).Value; }
            set { ((StringXmlNodeAttribute) GetAttribute("eltsuffixes")).Value = value; }
        }

        public string Quality
        {
            get { return ((StringXmlNodeAttribute) GetAttribute("eltcompression")).Value; }
            set { ((StringXmlNodeAttribute) GetAttribute("eltcompression")).Value = value; }
        }

        public string RequiredNamePattern
        {
            get { return ((StringXmlNodeAttribute) GetAttribute("eltfilename")).Value; }
            set { ((StringXmlNodeAttribute) GetAttribute("eltfilename")).Value = value; }
        }

        public File SrcFile
        {
            get
            {
                var folderAttr = (FolderXmlNodeAttribute) GetAttribute("eltsrcsubdirguid");
                var srcName = ((StringXmlNodeAttribute) GetAttribute("eltsrc")).Value;
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

        public bool IsOnlyPathAndFilenameInserted
        {
            get { return ((BoolXmlNodeAttribute) GetAttribute("eltonlyhrefvalue")).Value; }
            set { ((BoolXmlNodeAttribute) GetAttribute("eltonlyhrefvalue")).Value = value; }
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

        public bool IsScaledOrConverted
        {
            get { return ((BoolXmlNodeAttribute) GetAttribute("eltconvert")).Value; }
            set { ((BoolXmlNodeAttribute) GetAttribute("eltconvert")).Value = value; }
        }

        public TargetFormat TargetFormat
        {
            get { return ((StringEnumXmlNodeAttribute<TargetFormat>) GetAttribute("elttargetformat")).Value; }
            set { ((StringEnumXmlNodeAttribute<TargetFormat>) GetAttribute("elttargetformat")).Value = value; }
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

        public int? AutomaticMaximumScalingHeight
        {
            get
            {
                string tmp = ((StringXmlNodeAttribute) GetAttribute("eltmaxpicheight")).Value;
                return string.IsNullOrEmpty(tmp) ? (int?) null : int.Parse(tmp);
            }
            set { ((StringXmlNodeAttribute) GetAttribute("eltmaxpicheight")).Value = value.ToString(); }
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

        public int? ColorDepthInBit
        {
            get
            {
                string tmp = ((StringXmlNodeAttribute) GetAttribute("eltpicdepth")).Value;
                return string.IsNullOrEmpty(tmp) ? (int?) null : int.Parse(tmp);
            }
            set { ((StringXmlNodeAttribute) GetAttribute("eltpicdepth")).Value = value.ToString(); }
        }
    }
}