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

using System.Globalization;
using erminas.SmartAPI.CMS.Administration.Language;

namespace erminas.SmartAPI.CMS.Project.ContentClasses.Elements.Attributes
{
    internal class LocaleXmlNodeAttribute : RDXmlNodeAttribute
    {
        #region locales

        //public static Dictionary<int, string> _locales = new Dictionary<int, string>
        //                                                      {
        //                                                          {-1, "(Server)"},
        //                                                          {1052, "Albania&nbsp;(Albanian)"},
        //                                                          {5121, "Algeria&nbsp;(Arabic)"},
        //                                                          {11274, "Argentina&nbsp;(Spanish)"},
        //                                                          {3081, "Australia&nbsp;(English)"},
        //                                                          {3079, "Austria&nbsp;(German)"},
        //                                                          {15361, "Bahrain&nbsp;(Arabic)"},
        //                                                          {1059, "Belarus&nbsp;(Belarusian)"},
        //                                                          {2060, "Belgium&nbsp;(French)"},
        //                                                          {2067, "Belgium&nbsp;(Dutch)"},
        //                                                          {10249, "Belize&nbsp;(English)"},
        //                                                          {16394, "Bolivia&nbsp;(Spanish)"},
        //                                                          {1046, "Brazil&nbsp;(Portuguese)"},
        //                                                          {2110, "Brunei Darussalam&nbsp;(Malay)"},
        //                                                          {1026, "Bulgaria&nbsp;(Bulgarian)"},
        //                                                          {4105, "Canada&nbsp;(English)"},
        //                                                          {3084, "Canada&nbsp;(French)"},
        //                                                          {9225, "Caribbean&nbsp;(English)"},
        //                                                          {13322, "Chile&nbsp;(Spanish)"},
        //                                                          {9226, "Colombia&nbsp;(Spanish)"},
        //                                                          {5130, "Costa Rica&nbsp;(Spanish)"},
        //                                                          {1050, "Croatia&nbsp;(Croatian)"},
        //                                                          {1029, "Czech Republic&nbsp;(Czech)"},
        //                                                          {1030, "Denmark&nbsp;(Danish)"},
        //                                                          {7178, "Dominican Republic&nbsp;(Spanish)"},
        //                                                          {12298, "Ecuador&nbsp;(Spanish)"},
        //                                                          {3073, "Egypt&nbsp;(Arabic)"},
        //                                                          {17418, "El Salvador&nbsp;(Spanish)"},
        //                                                          {1061, "Estonia&nbsp;(Estonian)"},
        //                                                          {1080, "Faeroe Islands&nbsp;(Faeroese)"},
        //                                                          {1035, "Finland&nbsp;(Finnish)"},
        //                                                          {2077, "Finland&nbsp;(Swedish)"},
        //                                                          {1036, "France&nbsp;(French)"},
        //                                                          {1031, "Germany&nbsp;(German)"},
        //                                                          {1032, "Greece&nbsp;(Greek)"},
        //                                                          {4106, "Guatemala&nbsp;(Spanish)"},
        //                                                          {18442, "Honduras&nbsp;(Spanish)"},
        //                                                          {3076, "Hong Kong&nbsp;(Chinese)"},
        //                                                          {1038, "Hungary&nbsp;(Hungarian)"},
        //                                                          {1039, "Iceland&nbsp;(Icelandic)"},
        //                                                          {1081, "India&nbsp;(Hindi)"},
        //                                                          {1057, "Indonesia&nbsp;(Indonesian)"},
        //                                                          {1065, "Iran&nbsp;(Farsi)"},
        //                                                          {2049, "Iraq&nbsp;(Arabic)"},
        //                                                          {6153, "Ireland&nbsp;(English)"},
        //                                                          {1037, "Israel&nbsp;(Hebrew)"},
        //                                                          {1040, "Italy&nbsp;(Italian)"},
        //                                                          {8201, "Jamaica&nbsp;(English)"},
        //                                                          {1041, "Japan&nbsp;(Japanese)"},
        //                                                          {11265, "Jordan&nbsp;(Arabic)"},
        //                                                          {1089, "Kenya&nbsp;(Swahili)"},
        //                                                          {1042, "Korea&nbsp;(Korean)"},
        //                                                          {13313, "Kuwait&nbsp;(Arabic)"},
        //                                                          {1062, "Latvia&nbsp;(Latvian)"},
        //                                                          {12289, "Lebanon&nbsp;(Arabic)"},
        //                                                          {4097, "Libya&nbsp;(Arabic)"},
        //                                                          {5127, "Liechtenstein&nbsp;(German)"},
        //                                                          {1063, "Lithuania&nbsp;(Lithuanian)"},
        //                                                          {4103, "Luxembourg&nbsp;(German)"},
        //                                                          {5132, "Luxembourg&nbsp;(French)"},
        //                                                          {5124, "Macau&nbsp;(Chinese)"},
        //                                                          {1071, "Macedonia&nbsp;(Macedonian)"},
        //                                                          {1086, "Malaysia&nbsp;(Malay)"},
        //                                                          {2058, "Mexico&nbsp;(Spanish)"},
        //                                                          {6145, "Morocco&nbsp;(Arabic)"},
        //                                                          {1043, "Netherlands&nbsp;(Dutch)"},
        //                                                          {5129, "New Zealand&nbsp;(English)"},
        //                                                          {19466, "Nicaragua&nbsp;(Spanish)"},
        //                                                          {1044, "Norway&nbsp;(Norwegian)"},
        //                                                          {8193, "Oman&nbsp;(Arabic)"},
        //                                                          {1056, "Pakistan&nbsp;(Urdu)"},
        //                                                          {6154, "Panama&nbsp;(Spanish)"},
        //                                                          {15370, "Paraguay&nbsp;(Spanish)"},
        //                                                          {10250, "Peru&nbsp;(Spanish)"},
        //                                                          {13321, "Philippines&nbsp;(English)"},
        //                                                          {1045, "Poland&nbsp;(Polish)"},
        //                                                          {2070, "Portugal&nbsp;(Portuguese)"},
        //                                                          {2052, "PRC&nbsp;(Chinese)"},
        //                                                          {20490, "Puerto Rico&nbsp;(Spanish)"},
        //                                                          {16385, "Qatar&nbsp;(Arabic)"},
        //                                                          {1048, "Romania&nbsp;(Romanian)"},
        //                                                          {1049, "Russia&nbsp;(Russian)"},
        //                                                          {1025, "Saudi Arabia&nbsp;(Arabic)"},
        //                                                          {3098, "Serbia (Latin)&nbsp;(Serbian)"},
        //                                                          {4100, "Singapore&nbsp;(Chinese)"},
        //                                                          {1051, "Slovakia&nbsp;(Slovak)"},
        //                                                          {1060, "Slovenia&nbsp;(Slovene)"},
        //                                                          {1078, "South Africa&nbsp;(Afrikaans)"},
        //                                                          {7177, "South Africa&nbsp;(English)"},
        //                                                          {1027, "Spain&nbsp;(Catalan)"},
        //                                                          {1069, "Spain&nbsp;(Basque)"},
        //                                                          {1034, "Spain&nbsp;(Spanish)"},
        //                                                          {1053, "Sweden&nbsp;(Swedish)"},
        //                                                          {2064, "Switzerland&nbsp;(Italian)"},
        //                                                          {4108, "Switzerland&nbsp;(French)"},
        //                                                          {2055, "Switzerland&nbsp;(German)"},
        //                                                          {10241, "Syria&nbsp;(Arabic)"},
        //                                                          {1028, "Taiwan&nbsp;(Chinese)"},
        //                                                          {1054, "Thailand&nbsp;(Thai)"},
        //                                                          {11273, "Trinidad&nbsp;(English)"},
        //                                                          {7169, "Tunisia&nbsp;(Arabic)"},
        //                                                          {1055, "Turkey&nbsp;(Turkish)"},
        //                                                          {14337, "U.A.E.&nbsp;(Arabic)"},
        //                                                          {1058, "Ukraine&nbsp;(Ukrainian)"},
        //                                                          {2057, "United Kingdom&nbsp;(English)"},
        //                                                          {1033, "United States&nbsp;(English)"},
        //                                                          {14346, "Uruguay&nbsp;(Spanish)"},
        //                                                          {8202, "Venezuela&nbsp;(Spanish)"},
        //                                                          {1066, "Viet Nam&nbsp;(Vietnamese)"},
        //                                                          {9217, "Yemen&nbsp;(Arabic)"},
        //                                                      };

        #endregion

        private int? _lcid;

        public LocaleXmlNodeAttribute(IContentClassElement contentClass, string name) : base(contentClass, name)
        {
        }

        public override void Assign(IRDAttribute o)
        {
            var attr = (LocaleXmlNodeAttribute) o;
            string xmlNodeValue = attr.GetXmlNodeValue();
            //workaround, because sometimes the server responds with this invalid value
            if (xmlNodeValue.Contains("EmptyBuffer"))
            {
                xmlNodeValue = "";
            }
            SetValue(xmlNodeValue);
        }

        public override object DisplayObject
        {
            get
            {
                return _lcid == null
                           ? null
                           : ((IContentClassElement) Parent).ContentClass.Project.Session.Locales[_lcid.Value];
            }
        }

        public override bool IsAssignableFrom(IRDAttribute o, out string reason)
        {
            reason = "";
            return o is LocaleXmlNodeAttribute;
        }

        public ISystemLocale Value
        {
            get
            {
                return _lcid == null
                           ? null
                           : ((IContentClassElement) Parent).ContentClass.Project.Session.Locales[_lcid.Value];
            }
            set { SetValue(value == null ? null : value.LCID.ToString(CultureInfo.InvariantCulture)); }
        }

        protected override void UpdateValue(string value)
        {
            _lcid = value == null || value == "#" + Parent.Session.SessionKey || value.Contains("EmptyBuffer") ? (int?) null : int.Parse(value);
        }
    }
}