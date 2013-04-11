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

using System.Collections.Generic;
using System.Web.Script.Serialization;
using erminas.SmartAPI.Utils;
using log4net;

namespace erminas.SmartAPI.CMS.Project.ContentClasses.Elements.Attributes
{
    internal abstract class RDXmlNodeAttribute : IRDAttribute
    {
        #region ElementDescriptions

        [ScriptIgnore] public static readonly Dictionary<string, string> ELEMENT_DESCRIPTION =
            new Dictionary<string, string>
                {
                    {"adoptheadlinetoalllanguages", "Changing headline is effective for all language variants"},
                    {"eltname", "Name"},
                    {"eltbeginmark", "Start mark for automatic processing"},
                    {"eltcrlftobr", "CRLF -> <BR>"},
                    {"eltdefaultvalue", "Default Value"},
                    {"eltdonothtmlencode", "Do not convert characters to HTML"},
                    {"elteditorialelement", "Editorial element"},
                    {"eltendmark", "End mark for automatic processing"},
                    {"eltevalcalledpage", "Use data of page in target container"},
                    {"eltextendedlist", "Transfer element contents of following pages "},
                    {"eltfolderguid", "Folder"},
                    {"eltdepth", "Nesting level"},
                    {"eltsearchdepth", "Max. search depth"},
                    {"eltdropouts", "Max. number of errors"},
                    {"elttableopen", "Start of table"},
                    {"elttableclose", "End of table"},
                    {"eltrowopen", "Start of row"},
                    {"eltrowclose", "End of row"},
                    {"eltcolopen", "Start of column"},
                    {"eltcolclose", "End of column"},
                    {"eltxslfile", "XSL stylesheet"},
                    {"eltid", "Element Id"},
                    {"eltignoreworkflow", "Not relevant for workflow"},
                    {"eltinvisibleinclient", "Hide in project structure"},
                    {"eltinvisibleinpage", "Not visible on published page"},
                    {"eltisdynamic", "Dynamic element"},
                    {"eltislink", "Element is linked"},
                    {"eltshape", "Shape"},
                    {"eltcoords", "Coords"},
                    {"eltlanguageindependent", "Language-variant independent content"},
                    {"eltonlyhrefvalue", "Insert path and file name only"},
                    {"eltparentelementguid", "Child element of"},
                    {"eltparentelementname", "Child element of"},
                    {"eltpicsalllanguages", "Language-variant independent content"},
                    {"eltrddescription", "Description"},
                    {"eltrdexample", "SampleText Value"},
                    {"eltrequired", "Editing mandatory"},
                    {"elttype", "Element TypeId"},
                    {"eltuserdefinedallowed", "Values can be changed in dialog"},
                    {"languagevariantid", "Language variant"},
                    {"parentguid", "Parent element"},
                    {"parenttable", "Table type of the parent element"},
                    {"templateguid", "GUID of the content class to which the element belongs"},
                    {"guid", "GUID of the element"},
                    {"elthideinform", "Do not use in form"},
                    {"eltdragdrop", "Activate Drag & Drop"},
                    {"eltxhtmlcompliant", "Syntax which conforms to XHTML in the preview and SmartEdit"},
                    {"eltdefaultsuffix", "Default Suffix"},
                    {"elttargetcontainerguid", "HtmlTarget container"},
                    //TODO
                    {"elttargetcontainertype", "HtmlTarget container type"},
                    //TODO
                    {"eltautoborder", "Automatically insert border into Page"},
                    {"eltautowidth", "Automatically insert width into Page"},
                    {"eltautoheight", "Automatically insert height into Page"},
                    {"eltpresetalt", "Alt - Requirement"},
                    // --> enum
                    {"eltconvert", "Scaling/Conversion"},
                    {"eltfontclass", "Font class"},
                    {"eltfontsize", "Font size"},
                    {"eltfontbold", "Bold"},
                    {"eltfontface", "Font face"},
                    {"eltfontcolor", "Font color"},
                    {"eltimagesupplement", "Image link supplement"},
                    {"eltvalue", "Value"},
                    {"elttargetformat", "HtmlTarget DateTimeFormat"},
                    {"elttarget", "HtmlTarget"},
                    {"eltcompression", "Quality"},
                    {"eltmaxpicwidth", "Image width: Automatic maximum scaling"},
                    {"eltmaxpicheight", "Image height: Automatic maximum scaling"},
                    {"eltpicwidth", "Exact image size: width"},
                    {"eltpicheight", "Exact image size: height"},
                    {"eltpicdepth", "Color depth"},
                    {"eltsuffixes", "Eligible Suffixes"},
                    {"name", "Name"},
                    {"eltmaxsize", "Maximum file size"},
                    {"eltfilename", "Name (regular expression)"},
                    {"suffixguid", "Default suffix of this page"},
                    {"prefixguid", "Default prefix of this page"},
                    {"eltalign", "Align"},
                    {"eltonlynonwebsources", "Convert only non-Web compatible files"},
                    //TODO
                    {"eltwidth", "Width"},
                    //HTML
                    {"eltheight", "Height"},
                    //HTML
                    {"eltborder", "Border"},
                    //HTML
                    {"eltalt", "Alt"},
                    {"elteditoroptions", "Editor options"},
                    //TODO combined enum
                    {"eltsrc", "SRC"},
                    {"eltvspace", "VSpace"},
                    {"elthspace", "HSpace"},
                    {"eltusermap", "Usemap"},
                    {"eltsupplement", "Supplement"},
                    //Option List
                    {"eltlanguagedependentvalue", "Values dependent on language variant"},
                    {"eltlanguagedependentname", "Names dependent on language variant"},
                    {"eltorderby", "Sort mode"},
                    {"eltoptionlistdata", "Entries"},
                    //...
                    {"eltcolumnname", "Column"},
                    {"elttablename", "Table"},
                    {"eltbincolumnname", "Data field for binary data"},
                    {"eltformatting", "User defined format"},
                    {"eltisreffield", "Reference field"},
                    {"eltislistentry", "Hit List"},
                    {"eltlisttype", "List TypeId"},
                    //->string
            
                    {"eltcategorytype", "Category TypeId"},
                    {"eltdirectedit", "Activate DirectEdit"},
                    {"eltformatno", "DateTimeFormat"},
                    {"eltframename", "Name"},
                    {"eltmarginwidth", "Margin width"},
                    {"eltmarginheight", "Margin height"},
                    {"eltscrolling", "Scrolling"},
                    {"eltnoresize", "No resize"},
                    {"eltframeborder", "Frameborder"},
                    {"elthittype", "Hit List type"},
                    {"eltlcid", "Locale"},
                    {"eltdonotremove", "Do not remove link automatically"},
                    {"eltistargetcontainer", "HtmlTarget container"},
                    {"eltdefaulttext", "Default text"},
                    {"eltdefaulttextguid", "Default text"},
                    {"eltusemainlink", "Use main link"},
                    {"eltkeywordseparator", "Separator"},
                    {"eltusessl", "Use SSL"},
                    {"eltuserfc3066", "Use RFC3066"},
                    {"eltsubtype", "Metainfo TypeId"},
                    {"eltformatbutton", "DateTimeFormat"},
                    {"eltsrcsubdirguid", "SRC Folder"},
                    {"eltrdexamplesubdirguid", "SampleText Folder"},
                    {"eltrdexampleguid", "SampleText"},
                    {"eltdeactivatetextfilter", "Deactivate text filter"},
                    {"eltwholetext", "Use entire text if no matching tags can be found"},
                    {"eltrelatedfolderguid", "Publication folder"},
                    {"eltcolumniotype", "Special data field format"},
                    {"eltelementguid", "Element"},
                    {"eltprojectvariantguid", "Project Variant"},
                    {"eltlanguagevariantguid", "Language Variant"},
                    {"elttemplateguid", "Content class"},
                    {"eltprojectguid", "Project"},
                    //project content -> element
                    //{"eltlanguagevariantid",""},
                                                                                                               
                    //{"eltrefelementguid",""},
                    //{"eltrefelementtype",""},
                    {"eltconvertmode", "Convert selected documents"},
                    {"ignoreglobalworkflow", "Not relevant for global content workflow"},
                    {"keywordrequired", "Keyword required"},
                    {"requiredcategory", "Keyword required from category"},
                    {"selectinnewpage", "Available via the shortcut menu in SmartEdit"}
                    //{"",""},
                    //{"",""},
                    //{"",""},
                    //{"",""},
                    //{"",""},
                };

        #endregion

        protected readonly IAttributeContainer Parent;

        protected RDXmlNodeAttribute(IAttributeContainer parent, string name, bool initValue = true)
        {
            Parent = parent;
            Name = name;

            if (initValue)
            {
                string attr = parent.XmlElement.GetAttributeValue(name);
                if (attr != null && attr != Session.SESSIONKEY_PLACEHOLDER && attr != ("#" + Parent.Session.SessionKey))
                {
                    UpdateValue(attr);
                }
            }
            parent.RegisterAttribute(this);
        }

        #region IRDAttribute Members

        public abstract void Assign(IRDAttribute o);

        public string Description
        {
            get
            {
                string desc;
                bool hasValue = ELEMENT_DESCRIPTION.TryGetValue(Name, out desc);
                if (!hasValue)
                {
                    LogManager.GetLogger("DESCRIPTION").Debug("no description value for " + Name);
                }
                return hasValue ? desc : Name;
            }
        }

        public abstract object DisplayObject { get; }

        public override bool Equals(object o)
        {
            var attr = o as RDXmlNodeAttribute;
            if (attr == null || !attr.Name.Equals(Name))
            {
                return false;
            }
            string v = GetXmlNodeValue();
            string ov = attr.GetXmlNodeValue();
            return string.IsNullOrEmpty(v) ? string.IsNullOrEmpty(ov) : GetXmlNodeValue() == attr.GetXmlNodeValue();
        }

        public abstract bool IsAssignableFrom(IRDAttribute o, out string reason);
        public string Name { get; private set; }

        public void Refresh()
        {
            string xmlNodeValue = GetXmlNodeValue();

            UpdateValue(string.IsNullOrEmpty(xmlNodeValue) || xmlNodeValue == Session.SESSIONKEY_PLACEHOLDER
                            ? null
                            : xmlNodeValue);
        }

        #endregion

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public virtual string GetXmlNodeValue()
        {
            string value = Parent.XmlElement.GetAttributeValue(Name);
            return string.IsNullOrEmpty(value) ? null : value;
        }

        protected virtual void SetValue(string value)
        {
            UpdateValue(value);
            SetXmlNodeValue(value);
        }

        protected virtual void SetXmlNodeValue(string value)
        {
            Parent.XmlElement.SetAttributeValue(Name,
                                                string.IsNullOrEmpty(value) ? Session.SESSIONKEY_PLACEHOLDER : value);
        }

        protected abstract void UpdateValue(string value);
    }
}