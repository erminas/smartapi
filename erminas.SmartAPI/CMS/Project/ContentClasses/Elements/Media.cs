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
using System.Linq;
using System.Xml;
using erminas.SmartAPI.CMS.Project.ContentClasses.Elements.Attributes;
using erminas.SmartAPI.CMS.Project.Filesystem;

namespace erminas.SmartAPI.CMS.Project.ContentClasses.Elements
{

    #region MediaConversionMode

    public enum MediaConversionMode
    {
        NoConversion = 0,
        Pdf,
        Html
    }

    public static class MediaConversionModeUtils
    {
        public static MediaConversionMode ToMediaConversionMode(string value)
        {
            switch (value.ToUpperInvariant())
            {
                case "NO":
                    return MediaConversionMode.NoConversion;
                case "PDF":
                    return MediaConversionMode.Pdf;
                case "HTML":
                    return MediaConversionMode.Html;
                default:
                    throw new ArgumentException(string.Format("Cannot convert string value {1} to {0}",
                                                              typeof (MediaConversionMode).Name, value));
            }
        }

        public static string ToRQLString(this MediaConversionMode mode)
        {
            switch (mode)
            {
                case MediaConversionMode.NoConversion:
                    return "NO";
                case MediaConversionMode.Pdf:
                    return "PDF";
                case MediaConversionMode.Html:
                    return "HTML";
                default:
                    throw new ArgumentException(string.Format("Unknown {0} value: {1}",
                                                              typeof (MediaConversionMode).Name, mode));
            }
        }
    }

    #endregion

    public class Media : ContentClassElement, ICanBeRequiredForEditing
    {
        internal Media(ContentClass contentClass, XmlElement xmlElement) : base(contentClass, xmlElement)
        {
            CreateAttributes("eltignoreworkflow", "eltlanguageindependent", "eltrequired", "elthideinform",
                             "eltsuffixes", "eltdonotremove", "eltconvert", "eltmaxsize", "eltcompression",
                             "elttargetformat", "eltonlynonwebsources", "eltmaxpicwidth", "eltmaxpicheight",
                             "eltpicwidth", "eltpicheight", "eltpicdepth", "eltfilename", "eltdragdrop", "eltrdexample",
                             "eltrdexamplesubdirguid", "eltsrc", "eltsrcsubdirguid");
// ReSharper disable ObjectCreationAsStatement
            new StringEnumXmlNodeAttribute<MediaConversionMode>(this, "eltconvertmode",
                                                                MediaConversionModeUtils.ToRQLString,
                                                                MediaConversionModeUtils.ToMediaConversionMode);
// ReSharper restore ObjectCreationAsStatement
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

        public override ContentClassCategory Category
        {
            get { return ContentClassCategory.Content; }
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

        public MediaConversionMode ConversionModeForSelectedDocuments
        {
            get { return ((StringEnumXmlNodeAttribute<MediaConversionMode>) GetAttribute("eltconvertmode")).Value; }
            set { ((StringEnumXmlNodeAttribute<MediaConversionMode>) GetAttribute("eltconvertmode")).Value = value; }
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

        public bool IsConvertingOnlyNonWebCompatibleFiles
        {
            get { return GetAttributeValue<bool>("eltonlynonwebsources"); }
            set { SetAttributeValue("eltonlynonwebsources", value); }
        }

        public bool IsLanguageIndependent
        {
            get { return GetAttributeValue<bool>("eltlanguageindependent"); }
            set { SetAttributeValue("eltlanguageindependent", value); }
        }

        public bool IsLinkNotAutomaticallyRemoved
        {
            get { return GetAttributeValue<bool>("eltdonotremove"); }
            set { SetAttributeValue("eltdonotremove", value); }
        }

        public bool IsNotRelevantForWorklow
        {
            get { return GetAttributeValue<bool>("eltignoreworkflow"); }
            set { SetAttributeValue("eltignoreworkflow", value); }
        }

        public bool IsNotUsedInForm
        {
            get { return GetAttributeValue<bool>("elthideinform"); }
            set { SetAttributeValue("elthideinform", value); }
        }

        public bool IsScaledOrConverted
        {
            get { return GetAttributeValue<bool>("eltconvert"); }
            set { SetAttributeValue("eltconvert", value); }
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

        public File SampleFile
        {
            get
            {
                var folderAttr = (FolderXmlNodeAttribute) GetAttribute("eltrdexamplesubdirguid");
                string srcName = ((StringXmlNodeAttribute) GetAttribute("eltrdexample")).Value;
                if (folderAttr.Value == null || string.IsNullOrEmpty(srcName))
                {
                    return null;
                }
                return folderAttr.Value.GetFilesByNamePattern(srcName).First(x => x.Name == srcName);
            }

            set
            {
                ((StringXmlNodeAttribute) GetAttribute("eltrdexample")).Value = value.Name;
                ((FolderXmlNodeAttribute) GetAttribute("eltrdexamplesubdirguid")).Value = value.Folder;
            }
        }

        public File SrcFile
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

        public TargetFormat TargetFormat
        {
            get { return ((StringEnumXmlNodeAttribute<TargetFormat>) GetAttribute("elttargetformat")).Value; }
            set { ((StringEnumXmlNodeAttribute<TargetFormat>) GetAttribute("elttargetformat")).Value = value; }
        }

        #region ICanBeRequiredForEditing Members

        public bool IsEditingMandatory
        {
            get { return GetAttributeValue<bool>("eltrequired"); }
            set { SetAttributeValue("eltrequired", value); }
        }

        #endregion
    }
}