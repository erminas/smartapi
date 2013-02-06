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
using erminas.SmartAPI.CMS.CCElements.Attributes;

namespace erminas.SmartAPI.CMS.CCElements
{
    public enum HtmlTarget
    {
        None = 0,
        Blank,
        Parent,
        Top,
        Self
    }

    public static class HtmlTargetUtils
    {
        public static HtmlTarget ToHtmlTarget(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return HtmlTarget.None;
            }
            switch (value.ToUpperInvariant())
            {
                case "_BLANK":
                    return HtmlTarget.Blank;
                case "_PARENT":
                    return HtmlTarget.Parent;
                case "_TOP":
                    return HtmlTarget.Top;
                case "_SELF":
                    return HtmlTarget.Self;
                default:
                    throw new ArgumentException(string.Format("Cannot convert string value {1} to {0}",
                                                              typeof (HtmlTarget).Name, value));
            }
        }

        public static string ToRQLString(this HtmlTarget value)
        {
            switch (value)
            {
                case HtmlTarget.None:
                    return string.Empty;
                case HtmlTarget.Blank:
                    return "_blank";
                case HtmlTarget.Parent:
                    return "_parent";
                case HtmlTarget.Top:
                    return "_top";
                case HtmlTarget.Self:
                    return "_self";
                default:
                    throw new ArgumentException(string.Format("Unknown {0} value: {1}", typeof (HtmlTarget).Name, value));
            }
        }
    }

    public class Anchor : AbstractWorkflowPreassignable, ICanBeRequiredForEditing, IContentClassPreassignable
    {
        private readonly TargetContainerPreassignment _targetContainerPreassignment;

        protected Anchor(ContentClass contentClass, XmlElement xmlElement) : base(contentClass, xmlElement)
        {
            CreateAttributes("eltignoreworkflow", "eltisdynamic", "eltdonotremove", "eltxhtmlcompliant",
                             "eltdonothtmlencode", "eltlanguageindependent", "eltlanguagevariantguid",
                             "eltprojectvariantguid", "eltfolderguid", "eltcrlftobr", "eltonlyhrefvalue", "eltrequired",
                             "eltsupplement", "eltrdexample", "eltrdexamplesubdirguid", "eltrddescription", "elttarget");
// ReSharper disable ObjectCreationAsStatement
            new StringXmlNodeAttribute(this, "eltvalue");
// ReSharper restore ObjectCreationAsStatement
            PreassignedContentClasses = new PreassignedContentClassesAndPageDefinitions(this);
            _targetContainerPreassignment = new TargetContainerPreassignment(this);
        }

        public override ContentClassCategory Category
        {
            get { return ContentClassCategory.Structural; }
        }

        public string Description
        {
            get { return ((StringXmlNodeAttribute) GetAttribute("eltrddescription")).Value; }
            set { ((StringXmlNodeAttribute) GetAttribute("eltrddescription")).Value = value; }
        }

        public string ExampleText
        {
            get { return ((StringXmlNodeAttribute) GetAttribute("eltrdexample")).Value; }
            set { ((StringXmlNodeAttribute) GetAttribute("eltrdexample")).Value = value; }
        }

        public HtmlTarget HtmlTarget
        {
            get { return ((StringEnumXmlNodeAttribute<HtmlTarget>) GetAttribute("elttarget")).Value; }
            set { ((StringEnumXmlNodeAttribute<HtmlTarget>) GetAttribute("elttarget")).Value = value; }
        }

        public bool IsCrlfConvertedToBr
        {
            get { return ((BoolXmlNodeAttribute) GetAttribute("eltcrlftobr")).Value; }
            set { ((BoolXmlNodeAttribute) GetAttribute("eltcrlftobr")).Value = value; }
        }

        public bool IsDisplayingConnectedPagesInTargetContainerOfMainLinkIfAvailable
        {
            get { return _targetContainerPreassignment.IsDisplayingConnectedPagesInTargetContainerOfMainLinkIfAvailable; }
            set { _targetContainerPreassignment.IsDisplayingConnectedPagesInTargetContainerOfMainLinkIfAvailable = value; }
        }

        public bool IsDynamic
        {
            get { return ((BoolXmlNodeAttribute) GetAttribute("eltisdynamic")).Value; }
            set { ((BoolXmlNodeAttribute) GetAttribute("eltisdynamic")).Value = value; }
        }

        public bool IsLanguageIndependent
        {
            get { return ((BoolXmlNodeAttribute) GetAttribute("eltlanguageindependent")).Value; }
            set { ((BoolXmlNodeAttribute) GetAttribute("eltlanguageindependent")).Value = value; }
        }

        public bool IsLinkNotAutomaticallyRemoved
        {
            get { return ((BoolXmlNodeAttribute) GetAttribute("eltdonotremove")).Value; }
            set { ((BoolXmlNodeAttribute) GetAttribute("eltdonotremove")).Value = value; }
        }

        public bool IsNotConvertingCharactersToHtml
        {
            get { return ((BoolXmlNodeAttribute) GetAttribute("eltdonothtmlencode")).Value; }
            set { ((BoolXmlNodeAttribute) GetAttribute("eltdonothtmlencode")).Value = value; }
        }

        public bool IsNotRelevantForWorklow
        {
            get { return ((BoolXmlNodeAttribute) GetAttribute("eltignoreworkflow")).Value; }
            set { ((BoolXmlNodeAttribute) GetAttribute("eltignoreworkflow")).Value = value; }
        }

        public bool IsOnlyPathAndFilenameInserted
        {
            get { return ((BoolXmlNodeAttribute) GetAttribute("eltonlyhrefvalue")).Value; }
            set { ((BoolXmlNodeAttribute) GetAttribute("eltonlyhrefvalue")).Value = value; }
        }

        public bool IsSyntaxConformingToXHtml
        {
            get { return ((BoolXmlNodeAttribute) GetAttribute("eltxhtmlcompliant")).Value; }
            set { ((BoolXmlNodeAttribute) GetAttribute("eltxhtmlcompliant")).Value = value; }
        }

        public LanguageVariant LanguageVariantToSwitchTo
        {
            get { return ((LanguageVariantAttribute) GetAttribute("eltlanguagevariantguid")).Value; }
            set { ((LanguageVariantAttribute) GetAttribute("eltlanguagevariantguid")).Value = value; }
        }

        public PreassignedContentClassesAndPageDefinitions PreassignedContentClasses { get; private set; }

        public PageElements.Container PreassignedTargetContainer
        {
            get { return _targetContainerPreassignment.TargetContainer; }
            set { _targetContainerPreassignment.TargetContainer = value; }
        }

        public ProjectVariant ProjectVariantToSwitchTo
        {
            get { return ((ProjectVariantAttribute) GetAttribute("eltprojectvariantguid")).Value; }
            set { ((ProjectVariantAttribute) GetAttribute("eltprojectvariantguid")).Value = value; }
        }

        public string Supplement
        {
            get { return ((StringXmlNodeAttribute) GetAttribute("eltsupplement")).Value; }
            set { ((StringXmlNodeAttribute) GetAttribute("eltsupplement")).Value = value; }
        }

        #region ICanBeRequiredForEditing Members

        public bool IsEditingMandatory
        {
            get { return ((BoolXmlNodeAttribute) GetAttribute("eltrequired")).Value; }
            set { ((BoolXmlNodeAttribute) GetAttribute("eltrequired")).Value = value; }
        }

        #endregion
    }
}