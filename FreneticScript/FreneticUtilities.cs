//
// This file is created by Frenetic LLC.
// This code is Copyright (C) 2016-2017 Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Reflection;
using System.Reflection.Emit;
using FreneticUtilities.FreneticExtensions;

namespace FreneticScript
{
    /// <summary>
    /// Helper for enum types.
    /// </summary>
    /// <typeparam name="T">The enum type.</typeparam>
    public static class EnumHelper<T> where T: Enum
    {
        /// <summary>
        /// A map of names to values for this enum.
        /// Do not set to this instance, it will construct and fill itself.
        /// </summary>
        public static readonly Dictionary<string, T> NameValueMap;

        /// <summary>
        /// A map of lowercased names to values for this enum.
        /// Do not set to this instance, it will construct and fill itself.
        /// </summary>
        public static readonly Dictionary<string, T> LoweredNameValueMap;

        /// <summary>
        /// A map of values to names for this enum.
        /// Do not set to this instance, it will construct and fill itself.
        /// </summary>
        public static readonly Dictionary<T, string> ValueNameMap;

        /// <summary>
        /// An array of all names for this enum.
        /// Do not set to this instance, it will construct and fill itself.
        /// </summary>
        public static readonly string[] Names;

        /// <summary>
        /// A set of all names for this enum.
        /// Do not set to this instance, it will construct and fill itself.
        /// </summary>
        public static readonly HashSet<string> NameSet;

        /// <summary>
        /// An array of all lowercased names for this enum.
        /// Do not set to this instance, it will construct and fill itself.
        /// </summary>
        public static readonly string[] LoweredNames;

        /// <summary>
        /// A set of all lowercased names for this enum.
        /// Do not set to this instance, it will construct and fill itself.
        /// </summary>
        public static readonly HashSet<string> LoweredNameSet;

        /// <summary>
        /// An array of all values for this enum.
        /// Do not set to this instance, it will construct and fill itself.
        /// </summary>
        public static readonly T[] Values;

        /// <summary>
        /// A set of all values for this enum.
        /// Do not set to this instance, it will construct and fill itself.
        /// </summary>
        public static readonly HashSet<T> ValueSet;

        /// <summary>
        /// Gets the underlying type for the enum type.
        /// </summary>
        public static readonly Type UnderlyingType;
        
        /// <summary>
        /// Whether this is a flags enum.
        /// </summary>
        public static readonly bool IsFlags;

        /// <summary>
        /// A long converter function. Should only be used for very special case situations - usually, a normal cast works fine.
        /// </summary>
        public static readonly Func<T, long> LongConverter;

        /// <summary>
        /// A flag tester function. Ideally there will be a way to do this cleanly without dynamic code gen some day...
        /// (Other than just implementing the mathematical comparison inline).
        /// </summary>
        public static readonly Func<T, T, bool> FlagTester;
        
        static EnumHelper()
        {
            Names = Enum.GetNames(typeof(T));
            NameSet = new HashSet<string>(Names);
            Values = Enum.GetValues(typeof(T)) as T[];
            ValueSet = new HashSet<T>(Values);
            NameValueMap = Names.ToDictionaryWithNoDup(Values);
            LoweredNameValueMap = Names.Select(StringExtensions.ToLowerFast).ToList().ToDictionaryWith(Values);
            ValueNameMap = Values.ToDictionaryWith(Names);
            UnderlyingType = Enum.GetUnderlyingType(typeof(T));
            IsFlags = typeof(T).GetCustomAttributes(typeof(FlagsAttribute), true).Length != 0;
            LongConverter = CreateLongConverter();
            FlagTester = CreateFlagTester();
        }

        /// <summary>
        /// This is a gross hack used since C# handles enum types poorly.
        /// </summary>
        /// <returns>A long converter function.</returns>
        static Func<T, long> CreateLongConverter()
        {
            // long EnumToLong(T val) { return (long)val; }
            DynamicMethod method = new DynamicMethod("EnumToLong", typeof(long), new Type[] { typeof(T) }, typeof(EnumHelper<T>).Module, true);
            ILGenerator ilgen = method.GetILGenerator();
            ilgen.Emit(OpCodes.Ldarg_0);
            ilgen.Emit(OpCodes.Conv_I8);
            ilgen.Emit(OpCodes.Ret);
            return (Func<T, long>)method.CreateDelegate(typeof(Func<T, long>));
        }

        /// <summary>
        /// This is a gross hack used since C# handles enum types poorly.
        /// </summary>
        /// <returns>A flag tester function.</returns>
        static Func<T, T, bool> CreateFlagTester()
        {
            // bool FlagTester(T one, T two) { return (one & two) == two; }
            DynamicMethod method = new DynamicMethod("FlagTester", typeof(bool), new Type[] { typeof(T), typeof(T) }, typeof(EnumHelper<T>).Module, true);
            ILGenerator ilgen = method.GetILGenerator();
            ilgen.Emit(OpCodes.Ldarg_0);
            ilgen.Emit(OpCodes.Ldarg_1);
            ilgen.Emit(OpCodes.And);
            ilgen.Emit(OpCodes.Ldarg_1);
            ilgen.Emit(OpCodes.Ceq);
            ilgen.Emit(OpCodes.Ret);
            return (Func<T, T, bool>)method.CreateDelegate(typeof(Func<T, T, bool>));
        }

        /// <summary>
        /// Gets the value for the name, ignoring case.
        /// Returns whether the name was found.
        /// </summary>
        /// <param name="name">The name input.</param>
        /// <param name="val">The value output (when returning true).</param>
        /// <returns>Success state.</returns>
        public static bool TryParseIgnoreCase(string name, out T val)
        {
            return LoweredNameValueMap.TryGetValue(name.ToLowerFast(), out val);
        }

        /// <summary>
        /// Gets the value for the name.
        /// Returns whether the name was found.
        /// </summary>
        /// <param name="name">The name input.</param>
        /// <param name="val">The value output (when returning true).</param>
        /// <returns>Success state.</returns>
        public static bool TryParse(string name, out T val)
        {
            return NameValueMap.TryGetValue(name, out val);
        }

        /// <summary>
        /// Gets the value for the name, ignoring case.
        /// Throws an exception if name is invalid.
        /// </summary>
        /// <param name="name">The name to look up.</param>
        /// <returns>The enum value.</returns>
        public static T ParseIgnoreCase(string name)
        {
            return LoweredNameValueMap[name.ToLowerFast()];
        }

        /// <summary>
        /// Gets the value for the name.
        /// Throws an exception if name is invalid.
        /// </summary>
        /// <param name="name">The name to look up.</param>
        /// <returns>The enum value.</returns>
        public static T Parse(string name)
        {
            return NameValueMap[name];
        }

        /// <summary>
        /// Returns whether the name is defined in the enumeration, ignoring case.
        /// </summary>
        /// <param name="name">The name to test.</param>
        /// <returns>Whether it's defined.</returns>
        public static bool IsNameDefinedIgnoreCase(string name)
        {
            return LoweredNameSet.Contains(name.ToLowerFast());
        }

        /// <summary>
        /// Returns whether the name is defined in the enumeration.
        /// </summary>
        /// <param name="name">The name to test.</param>
        /// <returns>Whether it's defined.</returns>
        public static bool IsNameDefined(string name)
        {
            return Names.Contains(name);
        }

        /// <summary>
        /// Returns whether the value is defined in the enumeration.
        /// </summary>
        /// <param name="val">The value to test.</param>
        /// <returns>Whether it's defined.</returns>
        public static bool IsDefined(T val)
        {
            return ValueSet.Contains(val);
        }

        /// <summary>
        /// Returns whether the mainVal (as a bitflag set) has the required testVal (as a bitflag set).
        /// </summary>
        /// <param name="mainVal">The set of flags present.</param>
        /// <param name="testVal">The set of flags required.</param>
        /// <returns>Whether the flags are present as required.</returns>
        public static bool HasFlag(T mainVal, T testVal)
        {
            return FlagTester(mainVal, testVal);
        }

        /// <summary>
        /// Gets the name for a value (if it is defined).
        /// Returns success state.
        /// </summary>
        /// <param name="val">The value.</param>
        /// <param name="name">The name output (when returning true).</param>
        /// <returns>Success state.</returns>
        public static bool TryGetName(T val, out string name)
        {
            return ValueNameMap.TryGetValue(val, out name);
        }

        /// <summary>
        /// Gets the name for a value (if it is defined).
        /// </summary>
        /// <param name="val">The value.</param>
        /// <returns>The name.</returns>
        public static string GetName(T val)
        {
            return ValueNameMap[val];
        }
    }

    /// <summary>
    /// Helper for enumerable types.
    /// </summary>
    public static class EnumerablesHelper
    {
        /// <summary>
        /// Creates a dictionary mapping the keys array to the values array, such that keys[i] maps to values[i], for all integer "i" in range.
        /// The two input lists MUST have the same size.
        /// This will throw an exception if there are duplicate keys.
        /// </summary>
        /// <typeparam name="TKey">Key type.</typeparam>
        /// <typeparam name="TValue">Value type.</typeparam>
        /// <param name="keys">Key list.</param>
        /// <param name="values">Value list.</param>
        /// <returns>Dictionary.</returns>
        public static Dictionary<TKey, TValue> ToDictionaryWithNoDup<TKey, TValue>(this IList<TKey> keys, IList<TValue> values)
        {
            int len = keys.Count;
            if (len != values.Count)
            {
                throw new ArgumentException("Value list does not have same length as key list!", nameof(values));
            }
            Dictionary<TKey, TValue> dic = new Dictionary<TKey, TValue>(len * 2);
            for (int i = 0; i < len; i++)
            {
                dic.Add(keys[i], values[i]);
            }
            return dic;
        }

        /// <summary>
        /// Creates a dictionary mapping the keys array to the values array, such that keys[i] maps to values[i], for all integer "i" in range.
        /// The two input lists MUST have the same size.
        /// </summary>
        /// <typeparam name="TKey">Key type.</typeparam>
        /// <typeparam name="TValue">Value type.</typeparam>
        /// <param name="keys">Key list.</param>
        /// <param name="values">Value list.</param>
        /// <returns>Dictionary.</returns>
        public static Dictionary<TKey, TValue> ToDictionaryWith<TKey, TValue>(this IList<TKey> keys, IList<TValue> values)
        {
            int len = keys.Count;
            if (len != values.Count)
            {
                throw new ArgumentException("Value list does not have same length as key list!", nameof(values));
            }
            Dictionary<TKey, TValue> dic = new Dictionary<TKey, TValue>(len * 2);
            for (int i = 0; i < len; i++)
            {
                dic[keys[i]] = values[i];
            }
            return dic;
        }
    }

    /// <summary>
    /// Utilities for FreneticScript.
    /// </summary>
    public class FreneticScriptUtilities
    {
        /// <summary>
        /// The encoding used by FrenetiCScript.
        /// </summary>
        public static Encoding Enc = new UTF8Encoding(false);

        /// <summary>
        /// Converts a string value to the long-integer value it represents.
        /// Returns 0 if the string does not represent a long-integer.
        /// </summary>
        /// <param name="input">The string to get the value from.</param>
        /// <returns>a long-integer value.</returns>
        public static long StringToLong(string input)
        {
            if (long.TryParse(input, out long output))
            {
                return output;
            }
            return 0;
        }

        /// <summary>
        /// Converts a string value to the integer value it represents.
        /// Returns 0 if the string does not represent an integer.
        /// </summary>
        /// <param name="input">The string to get the value from.</param>
        /// <returns>an integer value.</returns>
        public static int StringToInt(string input)
        {
            if (int.TryParse(input, out int output))
            {
                return output;
            }
            return 0;
        }

        /// <summary>
        /// Converts a string value to the double value it represents.
        /// Returns 0 if the string does not represent a double.
        /// </summary>
        /// <param name="input">The string to get the value from.</param>
        /// <returns>a double value.</returns>
        public static double StringToDouble(string input)
        {
            if (double.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out double output))
            {
                return output;
            }
            return 0.0;
        }

        /// <summary>
        /// Converts a string value to the float value it represents.
        /// Returns 0 if the string does not represent a float.
        /// </summary>
        /// <param name="input">The string to get the value from.</param>
        /// <returns>a float value.</returns>
        public static float StringToFloat(string input)
        {
            if (float.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out float output))
            {
                return output;
            }
            return 0f;
        }

        /// <summary>
        /// Converts a string to a date-time.
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <returns>The date-time.</returns>
        public static DateTimeOffset? StringToDateTime(string input)
        {
            string[] bdat = input.SplitFast(' ');
            if (bdat.Length != 3)
            {
                return null;
            }
            string[] ymd = bdat[0].SplitFast('/');
            if (ymd.Length != 3)
            {
                return null;
            }
            int year = StringToInt(ymd[0]);
            int month = StringToInt(ymd[1]);
            int day = StringToInt(ymd[2]);
            string[] hmsm = bdat[1].SplitFast(':');
            if (hmsm.Length != 3 && hmsm.Length != 4)
            {
                return null;
            }
            int hour = StringToInt(hmsm[0]);
            int minute = StringToInt(hmsm[1]);
            int second = StringToInt(hmsm[2]);
            int millisecond = hmsm.Length == 4 ? StringToInt(hmsm[3]) : 0;
            int offH = 0;
            int offM = 0;
            if (bdat[2].Contains('-'))
            {
                string[] offsetinfo = bdat[2].SplitFast('-');
                string[] subinf = offsetinfo[1].SplitFast(':');
                if (subinf.Length != 2)
                {
                    return null;
                }
                offH = -StringToInt(subinf[0]);
                offM = -StringToInt(subinf[1]);
            }
            else
            {
                string[] offsetinfo = bdat[2].SplitFast('+');
                if (offsetinfo.Length != 2)
                {
                    return null;
                }
                string[] subinf = offsetinfo[1].SplitFast(':');
                if (subinf.Length != 2)
                {
                    return null;
                }
                offH = StringToInt(subinf[0]);
                offM = StringToInt(subinf[1]);
            }
            TimeSpan offs = new TimeSpan(offH, offM, 0);
            DateTimeOffset dto = new DateTimeOffset(year, month, day, hour, minute, second, millisecond, offs);
            return dto;
        }

        /// <summary>
        /// Used for <see cref="DateTimeToString(DateTimeOffset, bool)"/>.
        /// </summary>
        private static string QPad(string input, int length = 2)
        {
            return input.PadLeft(length, '0');
        }

        /// <summary>
        /// Returns a string representation of the specified time.
        /// </summary>
        /// <param name="dt">The datetime object.</param>
        /// <param name="ms">Whether to include milliseconds.</param>
        /// <returns>The time as a string.</returns>
        public static string DateTimeToString(DateTimeOffset dt, bool ms)
        {
            string utcoffset;
            if (dt.Offset.TotalMilliseconds < 0)
            {
                utcoffset = "-" + QPad(((int)Math.Abs(Math.Floor(dt.Offset.TotalHours))).ToString()) + ":" + QPad(dt.Offset.Minutes.ToString());
            }
            else
            {
                utcoffset = "+" + QPad(((int)Math.Floor(dt.Offset.TotalHours)).ToString()) + ":" + QPad(dt.Offset.Minutes.ToString());
            }
            return QPad(dt.Year.ToString(), 4) + "/" + QPad(dt.Month.ToString()) + "/" +
                QPad(dt.Day.ToString()) + " " + QPad(dt.Hour.ToString()) + ":" +
                QPad(dt.Minute.ToString()) + ":" + QPad(dt.Second.ToString()) + (ms ? ":" + QPad(dt.Millisecond.ToString(), 4): "") + " UTC" + utcoffset;
        }
    }
}
