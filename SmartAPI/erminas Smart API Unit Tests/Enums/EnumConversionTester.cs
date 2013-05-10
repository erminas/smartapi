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
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace erminas_Smart_API_Unit_Tests.Enums
{
    internal class EnumConversionTester<T> where T : struct, IConvertible
    {
        public static void TestConversionWithoutExcludedValues(Func<T, string> toString, Func<string, T> toEnum,
                                                               params T[] exclude)
        {
            var values = Enum.GetValues(typeof (T));
            foreach (var curValue in values)
            {
                if (exclude.Any(excludedValue => Equals(excludedValue, curValue)))
                {
                    continue;
                }
                string curString = toString((T) curValue);
                T value = toEnum(curString);
                Assert.AreEqual(curValue, value,
                                string.Format(
                                    "Error in enum conversion for {0}: value {1} as string = \"{2}\" as enum = \"{3}\"",
                                    typeof (T).Name, curValue, curString, value));
            }
        }
    }
}