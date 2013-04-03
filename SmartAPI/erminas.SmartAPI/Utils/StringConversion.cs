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
using System.Linq;
using System.Security;
using erminas.SmartAPI.CMS;
using erminas.SmartAPI.CMS.Administration.Language;
using erminas.SmartAPI.CMS.Project;

namespace erminas.SmartAPI.Utils
{
    public static class StringConversion
    {
        public static T ToEnum<T>(this string value) where T : struct, IConvertible
        {
            return (T)Enum.Parse(typeof (T), value);
        }

        public static string SecureRQLFormat(this string value, params object[] args)
        {
            IEnumerable<object> newArgs = from x in args select SecureConvertRQL(x);
            return string.Format(value, newArgs.ToArray());
        }

        private static object SecureConvertRQL(object o)
        {
            var s = o as string;
            return s != null ? SecurityElement.Escape(s) : ConvertRQL(o);
        }

        public static string RQLFormat(this string value, params object[] args)
        {
            IEnumerable<object> newArgs = from x in args select ConvertRQL(x);
            return string.Format(value, newArgs.ToArray());
        }

        /// <summary>
        ///     Converts a Guid to the format expected by the RedDot server. ALWAYS use this format when sending a Guid to the server.
        /// </summary>
        /// <param name="guid"> Guid to convert </param>
        /// <returns> String representation of the Guid in the format expected by a RedDot server </returns>
        public static string ToRQLString(this Guid guid)
        {
            //uppercase conversion is needed for SOME queries (RedDot server will show strange behaviour on them otherwise).
            return guid.ToString("N").ToUpperInvariant();
        }

        /// <summary>
        ///     Converts a bool value to the format expected by the RedDot server
        /// </summary>
        public static string ToRQLString(this Boolean value)
        {
            return value ? "1" : "0";
        }

        private static object ConvertRQL(object o)
        {
            if (o is Guid)
            {
                return ((Guid) o).ToRQLString();
            }

            if (o is Boolean)
            {
                return ((Boolean) o).ToRQLString();
            }

            var session = o as Session;
            if (session != null)
            {
                return session.SessionKey;
            }

            var languageVariant = o as LanguageVariant;
            if (languageVariant != null)
            {
                return languageVariant.Language;
            }

            var locale = o as Locale;
            if (locale != null)
            {
                return locale.LCID;
            }

            var variants = o as IEnumerable<LanguageVariant>;
            if (variants != null)
            {
                const string SINGLE_LANGUAGE = @"<LANGUAGEVARIANT language=""{0}""/>";
                string languages = variants.Aggregate("",
                                                      (s, variant) => s + SINGLE_LANGUAGE.RQLFormat(variant.Language));
                return languages;
            }

            return o is IRedDotObject ? ((IRedDotObject) o).Guid.ToRQLString() : o;
        }
    }
}