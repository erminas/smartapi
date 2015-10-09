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

namespace erminas.SmartAPI.CMS.Project.ContentClasses.Elements
{
    public interface IAnchor : IWorkflowAssignments, ICanBeRequiredForEditing, IContentClassPreassignable, IReferencePreassignable, IReferencePreassignTarget
    {
        string DescriptionInCurrentDisplayLanguage { get; set; }

        ILanguageDependentValue<string> ExampleText { get; }

        HtmlTarget HtmlTarget { get; set; }

        bool IsCrlfConvertedToBr { get; set; }

        bool IsDisplayingConnectedPagesInTargetContainerOfMainLinkIfAvailable { get; set; }

        bool IsDynamic { get; set; }

        bool IsLanguageIndependent { get; set; }

        bool IsLinkNotAutomaticallyRemoved { get; set; }

        bool IsNotConvertingCharactersToHtml { get; set; }

        bool IsNotRelevantForWorklow { get; set; }

        bool IsOnlyPathAndFilenameInserted { get; set; }

        bool IsSyntaxConformingToXHtml { get; set; }

        bool IsTransferingContentOfFollowingPages { get; set; }

        ILanguageVariant LanguageVariantToSwitchTo { get; set; }

        Pages.Elements.IContainer PreassignedTargetContainer { get; set; }

        IProjectVariant ProjectVariantToSwitchTo { get; set; }

        string Supplement { get; set; }

        string Value { get; set; }

        new void CommitInCurrentLanguage();

        new void CommitInLanguage(string languageAbbreviation);
    }

    internal class Anchor : AbstractWorkflowAssignments, IAnchor
    {
        private readonly ReferencePreassignment _referencePreassignment;
        private readonly TargetContainerPreassignment _targetContainerPreassignment;

        protected Anchor(IContentClass contentClass, XmlElement xmlElement) : base(contentClass, xmlElement)
        {
            PreassignedContentClasses = new PreassignedContentClassesAndPageDefinitions(this);
            _targetContainerPreassignment = new TargetContainerPreassignment(this);
            _referencePreassignment = new ReferencePreassignment(this);
        }

        public override void Refresh()
        {
            _referencePreassignment.InvalidateCache();
            base.Refresh();
        }

        public override ContentClassCategory Category
        {
            get { return ContentClassCategory.Structural; }
        }

        [RedDot("eltrddescription")]
        public string DescriptionInCurrentDisplayLanguage
        {
            get { return GetAttributeValue<string>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltrdexample")]
        public ILanguageDependentValue<string> ExampleText
        {
            get { return GetAttributeValue<ILanguageDependentValue<string>>(); }
        }

        [RedDot("elttarget", ConverterType = typeof (StringEnumConverter<HtmlTarget>))]
        public HtmlTarget HtmlTarget
        {
            get { return GetAttributeValue<HtmlTarget>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltcrlftobr")]
        public bool IsCrlfConvertedToBr
        {
            get { return GetAttributeValue<bool>(); }
            set { SetAttributeValue(value); }
        }

        public bool IsDisplayingConnectedPagesInTargetContainerOfMainLinkIfAvailable
        {
            get { return _targetContainerPreassignment.IsDisplayingConnectedPagesInTargetContainerOfMainLinkIfAvailable; }
            set { _targetContainerPreassignment.IsDisplayingConnectedPagesInTargetContainerOfMainLinkIfAvailable = value; }
        }

        [RedDot("eltisdynamic")]
        public bool IsDynamic
        {
            get { return GetAttributeValue<bool>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltrequired")]
        public bool IsEditingMandatory
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

        [RedDot("eltdonothtmlencode")]
        public bool IsNotConvertingCharactersToHtml
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

        [RedDot("eltextendedlist")]
        public bool IsTransferingContentOfFollowingPages
        {
            get { return GetAttributeValue<bool>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltlanguagevariantguid", ConverterType = typeof (LanguageVariantConverter))]
        public ILanguageVariant LanguageVariantToSwitchTo
        {
            get { return GetAttributeValue<ILanguageVariant>(); }
            set { SetAttributeValue(value); }
        }

        public PreassignedContentClassesAndPageDefinitions PreassignedContentClasses { get; }

        public Pages.Elements.IContainer PreassignedTargetContainer
        {
            get { return _targetContainerPreassignment.TargetContainer; }
            set { _targetContainerPreassignment.TargetContainer = value; }
        }

        [RedDot("eltprojectvariantguid", ConverterType = typeof (ProjectVariantConverter))]
        public IProjectVariant ProjectVariantToSwitchTo
        {
            get { return GetAttributeValue<IProjectVariant>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltsupplement")]
        public string Supplement
        {
            get { return GetAttributeValue<string>(); }
            set { SetAttributeValue(value); }
        }

        [RedDot("eltvalue")]
        public string Value
        {
            get { return GetAttributeValue<string>(); }
            set { SetAttributeValue(value); }
        }

        public IReferencePreassignTarget PreassignedReference
        {
            get { return _referencePreassignment.Target; }
            set { _referencePreassignment.Target = value; }
        }
    }
}
