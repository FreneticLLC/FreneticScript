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

namespace FreneticScript
{
    /// <summary>
    /// Helper for enum types. Do not use with non-enum types!
    /// </summary>
    /// <typeparam name="T">The enum type.</typeparam>
    public static class EnumHelper<T>
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
            if (!typeof(T).IsEnum)
            {
                throw new Exception("EnumHelper got non-enum type parameter: " + typeof(T).FullName);
            }
            Names = Enum.GetNames(typeof(T));
            NameSet = new HashSet<string>(Names);
            Values = Enum.GetValues(typeof(T)) as T[];
            ValueSet = new HashSet<T>(Values);
            NameValueMap = Names.ToDictionaryWithNoDup(Values);
            LoweredNameValueMap = Names.Select(StringExtensions.ToLowerFastFS).ToList().ToDictionaryWith(Values);
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
            return LoweredNameValueMap.TryGetValue(name.ToLowerFastFS(), out val);
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
            return LoweredNameValueMap[name.ToLowerFastFS()];
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
            return LoweredNameSet.Contains(name.ToLowerFastFS());
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
    /// Adds some extensions to strings.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Rapidly converts a string to a lowercase representation.
        /// </summary>
        /// <param name="input">The original string.</param>
        /// <returns>A lowercase version.</returns>
        public static string ToLowerFastFS(this string input)
        {
            char[] dt = input.ToCharArray();
            for (int i = 0; i < dt.Length; i++)
            {
                if (dt[i] >= 'A' && dt[i] <= 'Z')
                {
                    dt[i] = (char)(dt[i] - ('A' - 'a'));
                }
            }
            return new string(dt);
        }

        /// <summary>
        /// Returns whether the string starts with a null character.
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <returns>A boolean.</returns>
        public static bool StartsWithNullFS(this string input)
        {
            return input.Length > 0 && input[0] == '\0';
        }

        /// <summary>
        /// Quickly split a string.
        /// </summary>
        /// <param name="input">The original string.</param>
        /// <param name="splitter">What to split it by.</param>
        /// <param name="count">The maximum number of times to split it.</param>
        /// <returns>The split string pieces.</returns>
        public static string[] SplitFastFS(this string input, char splitter, int count = int.MaxValue)
        {
            int len = input.Length;
            int c = 0;
            for (int i = 0; i < len; i++)
            {
                if (input[i] == splitter)
                {
                    c++;
                }
            }
            c = ((c > count) ? count : c);
            string[] res = new string[c + 1];
            int start = 0;
            int x = 0;
            for (int i = 0; i < len && x < c; i++)
            {
                if (input[i] == splitter)
                {
                    res[x++] = input.Substring(start, i - start);
                    start = i + 1;
                }
            }
            res[x] = input.Substring(start);
            return res;
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
            string[] bdat = input.SplitFastFS(' ');
            if (bdat.Length != 3)
            {
                return null;
            }
            string[] ymd = bdat[0].SplitFastFS('/');
            if (ymd.Length != 3)
            {
                return null;
            }
            int year = StringToInt(ymd[0]);
            int month = StringToInt(ymd[1]);
            int day = StringToInt(ymd[2]);
            string[] hmsm = bdat[1].SplitFastFS(':');
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
                string[] offsetinfo = bdat[2].SplitFastFS('-');
                string[] subinf = offsetinfo[1].SplitFastFS(':');
                if (subinf.Length != 2)
                {
                    return null;
                }
                offH = -StringToInt(subinf[0]);
                offM = -StringToInt(subinf[1]);
            }
            else
            {
                string[] offsetinfo = bdat[2].SplitFastFS('+');
                if (offsetinfo.Length != 2)
                {
                    return null;
                }
                string[] subinf = offsetinfo[1].SplitFastFS(':');
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
                utcoffset = "-" + Pad(((int)Math.Abs(Math.Floor(dt.Offset.TotalHours))).ToString(), '0', 2) + ":" + Pad(dt.Offset.Minutes.ToString(), '0', 2);
            }
            else
            {
                utcoffset = "+" + Pad(((int)Math.Floor(dt.Offset.TotalHours)).ToString(), '0', 2) + ":" + Pad(dt.Offset.Minutes.ToString(), '0', 2);
            }
            return Pad(dt.Year.ToString(), '0', 4) + "/" + Pad(dt.Month.ToString(), '0', 2) + "/" +
                Pad(dt.Day.ToString(), '0', 2) + " " + Pad(dt.Hour.ToString(), '0', 2) + ":" +
                Pad(dt.Minute.ToString(), '0', 2) + ":" + Pad(dt.Second.ToString(), '0', 2) + (ms ? ":" + Pad(dt.Millisecond.ToString(), '0', 4): "") + " UTC" + utcoffset;
        }

        /// <summary>
        /// Pads a string to a specified length with a specified input, on a specified side.
        /// </summary>
        /// <param name="input">The original string.</param>
        /// <param name="padding">The symbol to pad with.</param>
        /// <param name="length">How far to pad it to.</param>
        /// <param name="left">Whether to pad left (true), or right (false).</param>
        /// <returns>The padded string.</returns>
        public static string Pad(string input, char padding, int length, bool left = true)
        {
            int targetlength = length - input.Length;
            StringBuilder pad = new StringBuilder(targetlength <= 0 ? 1 : targetlength);
            for (int i = 0; i < targetlength; i++)
            {
                pad.Append(padding);
            }
            if (left)
            {
                return pad + input;
            }
            else
            {
                return input + pad;
            }
        }
    }
}
