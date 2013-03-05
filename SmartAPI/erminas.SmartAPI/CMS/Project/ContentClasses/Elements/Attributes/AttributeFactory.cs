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
using System.Collections.Generic;
using erminas.SmartAPI.Exceptions;

namespace erminas.SmartAPI.CMS.Project.ContentClasses.Elements.Attributes
{
    public abstract class AttributeFactory
    {
        private static readonly Dictionary<string, AttributeFactory> FACTORIES =
            new Dictionary<string, AttributeFactory>
                {
                    {"adoptheadlinetoalllanguages", new BoolAttributeFactory()},
                    {"approverequired", new BoolAttributeFactory()},
                    {"bordercolor", new StringAttributeFactory()},
                    {"borderstyle", new StringAttributeFactory()},
                    {"borderwidth", new StringAttributeFactory()},
                    {"description", new StringAttributeFactory()},
                    {"eltalt", new StringAttributeFactory()},
                    {"eltautoborder", new BoolAttributeFactory()},
                    {"eltautoheight", new BoolAttributeFactory()},
                    {"eltautowidth", new BoolAttributeFactory()},
                    {"eltbeginmark", new StringAttributeFactory()},
                    {"eltbincolumnname", new StringAttributeFactory()},
                    {"eltborder", new StringAttributeFactory()},
                    {"eltcolclose", new StringAttributeFactory()},
                    {"eltcolopen", new StringAttributeFactory()},
                    {"eltcolumnname", new StringAttributeFactory()},
                    {"eltcompression", new StringAttributeFactory()},
                    {"eltconvert", new BoolAttributeFactory()},
                    {"eltconvertmode", new StringAttributeFactory()},
                    {"eltcoords", new StringAttributeFactory()},
                    {"eltcrlftobr", new BoolAttributeFactory()},
                    {"eltdeactivatetextfilter", new BoolAttributeFactory()},
                    {"eltdefaulttext", new StringAttributeFactory()},
                    {"eltdefaultvalue", new StringAttributeFactory()},
                    {"eltdepth", new StringAttributeFactory()},
                    {"eltdirectedit", new BoolAttributeFactory()},
                    {"eltdonothtmlencode", new BoolAttributeFactory()},
                    {"eltdonotremove", new BoolAttributeFactory()},
                    {"eltdragdrop", new BoolAttributeFactory()},
                    {"eltdropouts", new StringAttributeFactory()},
                    {"eltendmark", new StringAttributeFactory()},
                    {"eltevalcalledpage", new BoolAttributeFactory()},
                    {"eltextendedlist", new BoolAttributeFactory()},
                    {"eltfilename", new StringAttributeFactory()},
                    {"eltfolderguid", new FolderAttributeFactory()},
                    {"eltformatting", new StringAttributeFactory()},
                    {"eltformatno", new DateTimeFormatAttributeFactory()},
                    {"eltfontbold", new BoolAttributeFactory()},
                    {"eltfontclass", new StringAttributeFactory()},
                    {"eltfontcolor", new StringAttributeFactory()},
                    {"eltfontface", new StringAttributeFactory()},
                    {"eltfontsize", new StringAttributeFactory()},
                    {"eltframename", new StringAttributeFactory()},
                    {
                        "eltframeborder",
                        new StringEnumAttributeFactory<Frameborder>(FrameborderUtils.ToRQLString,
                                                                    FrameborderUtils.ToFrameborder)
                    },
                    {"eltheight", new StringAttributeFactory()},
                    {"elthideinform", new BoolAttributeFactory()},
                    {
                        "elthittype",
                        new StringEnumAttributeFactory<HitListType>(HitListTypeUtils.ToRQLString,
                                                                    HitListTypeUtils.ToHitListType)
                    },
                    {"elthspace", new StringAttributeFactory()},
                    {"eltignoreworkflow", new BoolAttributeFactory()},
                    {"eltimagesupplement", new StringAttributeFactory()},
                    {"eltinvisibleinclient", new BoolAttributeFactory()},
                    {"eltinvisibleinpage", new BoolAttributeFactory()},
                    {"eltisdynamic", new BoolAttributeFactory()},
                    {"eltislistentry", new BoolAttributeFactory()},
                    {"eltisreffield", new BoolAttributeFactory()},
                    {"eltistargetcontainer", new BoolAttributeFactory()},
                    {"eltkeywordseparator", new StringAttributeFactory()},
                    {"eltlanguagedependentvalue", new BoolAttributeFactory()},
                    {"eltlanguagedependentname", new BoolAttributeFactory()},
                    {"eltlanguageindependent", new BoolAttributeFactory()},
                    {"eltlanguagevariantguid", new LanguageVariantAttributeFactory()},
                    {"eltlcid", new LocaleAttributeFactory()},
                    {
                        "eltlisttype",
                        new StringEnumAttributeFactory<ListType>(
                            new Dictionary<ListType, string>
                                {
                                    {ListType.DisplayAsLink, "Display as link"},
                                    {ListType.Supplement, "Supplement"},
                                    {ListType.None, "Not set"}
                                }, ListTypeUtils.ToRQLString, ListTypeUtils.ToListType)
                    },
                    {"eltmarginheight", new StringAttributeFactory()},
                    {"eltmarginwidth", new StringAttributeFactory()},
                    {"eltmaxpicheight", new StringAttributeFactory()},
                    {"eltmaxpicwidth", new StringAttributeFactory()},
                    {"eltmaxsize", new StringAttributeFactory()},
                    {"eltmediatypeattribute", new EnumAttributeWithCustomValuesFactory<MediaTypeAttribute>()},
                    {"eltmediatypename", new StringAttributeFactory()},
                    {"eltname", new StringAttributeFactory()},
                    {"eltnoresize", new BoolAttributeFactory()},
                    {"eltonlyhrefvalue", new BoolAttributeFactory()},
                    {"eltonlynonwebsources", new BoolAttributeFactory()},
                    {"eltoptionlistdata", new StringAttributeFactory()},
                    {"eltorderby", new EnumAttributeFactory<SortMode>(null)},
                    {"eltprojectvariantguid", new ProjectVariantAttributeFactory()},
                    {"eltparentelementguid", new ElementAttributeFactory()},
                    {"eltpicdepth", new StringAttributeFactory()},
                    {"eltpicheight", new StringAttributeFactory()},
                    {"eltpicwidth", new StringAttributeFactory()},
                    {"eltrddescription", new StringAttributeFactory()},
                    {"eltrdexample", new StringAttributeFactory()},
                    {"eltrdexamplesubdirguid", new FolderAttributeFactory()},
                    {"eltrelatedfolderguid", new FolderAttributeFactory()},
                    {"eltrequired", new BoolAttributeFactory()},
                    {"eltrowclose", new StringAttributeFactory()},
                    {"eltrowopen", new StringAttributeFactory()},
                    {
                        "eltscrolling",
                        new StringEnumAttributeFactory<Scrolling>(ScrollingUtils.ToRQLString, ScrollingUtils.ToScrolling)
                    },
                    {"eltsearchdepth", new StringAttributeFactory()},
                    {"eltshape", new StringAttributeFactory()},
                    {"eltsrc", new StringAttributeFactory()},
                    {"eltsrcsubdirguid", new FolderAttributeFactory()},
                    {"eltsubtype", new InfoElementAttributeFactory()},
                    {"eltsuffixes", new StringAttributeFactory()},
                    {"eltsupplement", new StringAttributeFactory()},
                    {
                        "elttarget",
                        new StringEnumAttributeFactory<HtmlTarget>(HtmlTargetUtils.ToRQLString, HtmlTargetUtils.ToHtmlTarget)
                    },
                    {"elttableclose", new StringAttributeFactory()},
                    {"elttablename", new StringAttributeFactory()},
                    {"elttableopen", new StringAttributeFactory()},
                    {
                        "elttargetformat",
                        new StringEnumAttributeFactory<TargetFormat>(TargetFormatUtils.ToRQLString,
                                                                     TargetFormatUtils.ToTargetFormat)
                    },
                    {"eltusemainlink", new BoolAttributeFactory()},
                    {"eltuserdefinedallowed", new BoolAttributeFactory()},
                    {"eltuserfc3066", new BoolAttributeFactory()},
                    {"eltusermap", new StringAttributeFactory()},
                    {"eltvspace", new StringAttributeFactory()},
                    {"eltwholetext", new BoolAttributeFactory()},
                    {"eltwidth", new StringAttributeFactory()},
                    {"eltxhtmlcompliant", new BoolAttributeFactory()},
                    {"eltxslfile", new StringAttributeFactory()},
                    {"framesetafterlist", new BoolAttributeFactory()},
                    {"guid", new StringAttributeFactory()},
                    {"ignoreglobalworkflow", new BoolAttributeFactory()},
                    {"keywordrequired", new BoolAttributeFactory()},
                    {"languagevariantid", new StringAttributeFactory()},
                    {"name", new StringAttributeFactory()},
                    {"praefixguid", new SyllableAttributeFactory()},
                    {"requiredcategory", new CategoryAttributeFactory()},
                    {"selectinnewpage", new BoolAttributeFactory()},
                    {"showpagerange", new BoolAttributeFactory()},
                    {"suffixguid", new SyllableAttributeFactory()},
                    {"usedefaultrangesettings", new BoolAttributeFactory()},
                };

        public static void AddFactory(string attributeName, AttributeFactory factory)
        {
            FACTORIES.Add(attributeName, factory);
        }

        protected abstract RDXmlNodeAttribute CreateAttributeInternal(IAttributeContainer element, string name);

        internal static IRDAttribute CreateAttribute(IAttributeContainer element, string attributeName)
        {
            try
            {
                AttributeFactory factory = FACTORIES[attributeName];
                return factory.CreateAttributeInternal(element, attributeName);
            } catch (KeyNotFoundException)
            {
                throw new MissingAttributeException(attributeName);
            }
        }
    }

    public class MissingAttributeException : SmartAPIInternalException
    {
        public MissingAttributeException(string attributeName) : base("Missing attribute definition: " + attributeName)
        {
        }
    }

    internal class LanguageVariantAttributeFactory : AttributeFactory
    {
        protected override RDXmlNodeAttribute CreateAttributeInternal(IAttributeContainer element, string name)
        {
            return new LanguageVariantAttribute((ContentClassElement) element, name);
        }
    }

    internal class ProjectVariantAttributeFactory : AttributeFactory
    {
        protected override RDXmlNodeAttribute CreateAttributeInternal(IAttributeContainer element, string name)
        {
            return new ProjectVariantAttribute((ContentClassElement) element, name);
        }
    }

    internal class InfoElementAttributeFactory : AttributeFactory
    {
        protected override RDXmlNodeAttribute CreateAttributeInternal(IAttributeContainer element, string name)
        {
            return new InfoElementAttribute((ContentClassElement) element, name);
        }
    }

    internal class DateTimeFormatAttributeFactory : AttributeFactory
    {
        protected override RDXmlNodeAttribute CreateAttributeInternal(IAttributeContainer element, string name)
        {
            return new DateTimeFormatAttribute((ContentClassElement) element, name);
        }
    }

    internal class LocaleAttributeFactory : AttributeFactory
    {
        protected override RDXmlNodeAttribute CreateAttributeInternal(IAttributeContainer element, string name)
        {
            return new LocaleXmlNodeAttribute((ContentClassElement) element, name);
        }
    }

    public class BoolAttributeFactory : AttributeFactory
    {
        protected override RDXmlNodeAttribute CreateAttributeInternal(IAttributeContainer element, string name)
        {
            return new BoolXmlNodeAttribute(element, name);
        }
    }

    public class StringAttributeFactory : AttributeFactory
    {
        protected override RDXmlNodeAttribute CreateAttributeInternal(IAttributeContainer element, string name)
        {
            return new StringXmlNodeAttribute(element, name);
        }
    }

    //public class IntAttributeFactory : AttributeFactory
    //{
    //    protected override RDXmlNodeAttribute CreateAttributeInternal(IAttributeContainer element, string name)
    //    {
    //        return new IntXmlNodeAttribute(element, name);
    //    }
    //}

    public class FolderAttributeFactory : AttributeFactory
    {
        protected override RDXmlNodeAttribute CreateAttributeInternal(IAttributeContainer element, string name)
        {
            return new FolderXmlNodeAttribute((ContentClassElement) element, name);
        }
    }

    public class EnumAttributeFactory<T> : AttributeFactory where T : struct, IConvertible
    {
        private readonly Dictionary<T, string> _displayStrings;

        public EnumAttributeFactory() : this(null)
        {
        }

        public EnumAttributeFactory(Dictionary<T, string> displayStrings)
        {
            _displayStrings = displayStrings;
        }

        protected override RDXmlNodeAttribute CreateAttributeInternal(IAttributeContainer element, string name)
        {
            return new EnumXmlNodeAttribute<T>(element, name, _displayStrings);
        }
    }

    public class EnumAttributeWithCustomValuesFactory<T> : AttributeFactory where T : struct, IConvertible
    {
        protected readonly Dictionary<T, string> DisplayStrings;

        public EnumAttributeWithCustomValuesFactory() : this(null)
        {
        }

        public EnumAttributeWithCustomValuesFactory(Dictionary<T, string> displayStrings)
        {
            DisplayStrings = displayStrings;
        }

        protected override RDXmlNodeAttribute CreateAttributeInternal(IAttributeContainer element, string name)
        {
            return new EnumWithCustomValuesXmlNodeAttribute<T>(element, name, DisplayStrings);
        }
    }

    public class StringEnumAttributeFactory<T> : EnumAttributeWithCustomValuesFactory<T> where T : struct, IConvertible
    {
        private readonly Func<string, T> _fromString;
        private readonly Func<T, string> _toString;

        public StringEnumAttributeFactory(Func<T, string> toString, Func<string, T> fromString)
        {
            _toString = toString;
            _fromString = fromString;
        }

        public StringEnumAttributeFactory(Dictionary<T, string> displayStrings, Func<T, string> toString,
                                          Func<string, T> fromString) : base(displayStrings)
        {
            _toString = toString;
            _fromString = fromString;
        }

        protected override RDXmlNodeAttribute CreateAttributeInternal(IAttributeContainer element, string name)
        {
            return new StringEnumXmlNodeAttribute<T>(element, name, DisplayStrings, _toString, _fromString);
        }
    }

    public class ElementAttributeFactory : AttributeFactory
    {
        protected override RDXmlNodeAttribute CreateAttributeInternal(IAttributeContainer element, string name)
        {
            return new ElementXmlNodeAttribute((ContentClassElement) element, name);
        }
    }

    public class SyllableAttributeFactory : AttributeFactory
    {
        protected override RDXmlNodeAttribute CreateAttributeInternal(IAttributeContainer element, string name)
        {
            return new SyllableXmlNodeAttribute((ContentClass) element, name);
        }
    }

    public class CategoryAttributeFactory : AttributeFactory
    {
        protected override RDXmlNodeAttribute CreateAttributeInternal(IAttributeContainer element, string name)
        {
            return new CategoryXmlNodeAttribute((ContentClass) element, name);
        }
    }
}