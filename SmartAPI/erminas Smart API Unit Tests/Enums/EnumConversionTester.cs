using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace erminas_Smart_API_Unit_Tests.Enums
{
    internal class EnumConversionTester<T> where T : struct, IConvertible
    {
        public static void TestConversionWithoutExcludedValues(Func<T, string> toString, Func<string, T> toEnum, params T[] exclude)
        {
            var values = Enum.GetValues(typeof (T));
            foreach (var curValue in values)
            {
                if (exclude.Any(excludedValue => Equals(excludedValue,curValue)))
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