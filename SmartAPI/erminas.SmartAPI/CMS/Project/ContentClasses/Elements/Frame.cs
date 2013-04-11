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
using erminas.SmartAPI.CMS.Project.ContentClasses.Elements.Attributes;

namespace erminas.SmartAPI.CMS.Project.ContentClasses.Elements
{

    #region Scrolling

    public enum Scrolling
    {
        NotSet = 0,
        Yes,
        No,
        Auto
    }

    public static class ScrollingUtils
    {
        public static string ToRQLString(this Scrolling value)
        {
            switch (value)
            {
                case Scrolling.NotSet:
                    return "";
                case Scrolling.Auto:
                    return "auto";
                case Scrolling.Yes:
                    return "yes";
                case Scrolling.No:
                    return "no";
                default:
                    throw new ArgumentException(string.Format("Unknown {0} value: {1}", typeof (Scrolling).Name, value));
            }
        }

        public static Scrolling ToScrolling(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return Scrolling.NotSet;
            }
            switch (value.ToUpperInvariant())
            {
                case "AUTO":
                    return Scrolling.Auto;
                case "YES":
                    return Scrolling.Yes;
                case "NO":
                    return Scrolling.No;
                default:
                    throw new ArgumentException(string.Format("Cannot convert string value {1} to {0}",
                                                              typeof (Scrolling).Name, value));
            }
        }
    }

    #endregion

    #region Frameborder

    public enum Frameborder
    {
        NotSet = 0,
        Yes,
        No
    }

    public static class FrameborderUtils
    {
        public static Frameborder ToFrameborder(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return Frameborder.NotSet;
            }
            switch (value.ToUpperInvariant())
            {
                case "YES":
                    return Frameborder.Yes;
                case "NO":
                    return Frameborder.No;
                default:
                    throw new ArgumentException(string.Format("Cannot convert string value {1} to {0}",
                                                              typeof (Frameborder).Name, value));
            }
        }

        public static string ToRQLString(this Frameborder value)
        {
            switch (value)
            {
                case Frameborder.NotSet:
                    return string.Empty;
                case Frameborder.Yes:
                    return "yes";
                case Frameborder.No:
                    return "no";
                default:
                    throw new ArgumentException(string.Format("Unknown {0} value: {1}", typeof (Frameborder).Name, value));
            }
        }
    }

    #endregion

    public interface IFrame : IWorkflowAssignments, IContentClassPreassignable
    {
        string FrameName { get; set; }
        Frameborder Frameborder { get; set; }
        bool IsNotResizing { get; set; }
        bool IsSyntaxConformingToXHtml { get; set; }
        string MarginHeight { get; set; }
        string MarginWidth { get; set; }
        Scrolling Scrolling { get; set; }
        string Src { get; set; }
        string Supplement { get; set; }
    }

    internal class Frame : AbstractWorkflowAssignments, IFrame
    {
        internal Frame(IContentClass contentClass, XmlElement xmlElement) : base(contentClass, xmlElement)
        {
            CreateAttributes("eltxhtmlcompliant", "eltframename", "eltmarginwidth", "eltmarginheight", "eltscrolling",
                             "eltsrc", "eltsupplement", "eltframeborder", "eltnoresize");

            PreassignedContentClasses = new PreassignedContentClassesAndPageDefinitions(this);
        }

        public override ContentClassCategory Category
        {
            get { return ContentClassCategory.Structural; }
        }

        public string FrameName
        {
            get { return GetAttributeValue<string>("eltframename"); }
            set { SetAttributeValue("eltframename", value); }
        }

        public Frameborder Frameborder
        {
            get { return ((StringEnumXmlNodeAttribute<Frameborder>) GetAttribute("eltframeborder")).Value; }
            set { ((StringEnumXmlNodeAttribute<Frameborder>) GetAttribute("eltframeborder")).Value = value; }
        }

        public bool IsNotResizing
        {
            get { return GetAttributeValue<bool>("eltnoresize"); }
            set { SetAttributeValue("eltnoresize", value); }
        }

        public bool IsSyntaxConformingToXHtml
        {
            get { return GetAttributeValue<bool>("eltxhtmlcompliant"); }
            set { SetAttributeValue("eltxhtmlcompliant", value); }
        }

        public string MarginHeight
        {
            get { return GetAttributeValue<string>("eltmarginheight"); }
            set { SetAttributeValue("eltmarginheight", value); }
        }

        public string MarginWidth
        {
            get { return GetAttributeValue<string>("eltmarginwidth"); }
            set { SetAttributeValue("eltmarginwidth", value); }
        }

        public PreassignedContentClassesAndPageDefinitions PreassignedContentClasses { get; private set; }

        public Scrolling Scrolling
        {
            get { return ((StringEnumXmlNodeAttribute<Scrolling>) GetAttribute("eltscrolling")).Value; }
            set { ((StringEnumXmlNodeAttribute<Scrolling>) GetAttribute("eltscrolling")).Value = value; }
        }

        public string Src
        {
            get { return GetAttributeValue<string>("eltsrc"); }
            set { SetAttributeValue("eltsrc", value); }
        }

        public string Supplement
        {
            get { return GetAttributeValue<string>("eltsupplement"); }
            set { SetAttributeValue("eltsupplement", value); }
        }
    }
}