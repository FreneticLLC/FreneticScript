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

namespace FreneticScript
{
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
            long.TryParse(input, out long output);
            return output;
        }

        /// <summary>
        /// Converts a string value to the integer value it represents.
        /// Returns 0 if the string does not represent an integer.
        /// </summary>
        /// <param name="input">The string to get the value from.</param>
        /// <returns>an integer value.</returns>
        public static int StringToInt(string input)
        {
            int.TryParse(input, out int output);
            return output;
        }

        /// <summary>
        /// Converts a string value to the double value it represents.
        /// Returns 0 if the string does not represent a double.
        /// </summary>
        /// <param name="input">The string to get the value from.</param>
        /// <returns>a double value.</returns>
        public static double StringToDouble(string input)
        {
            double.TryParse(input, out double output);
            return output;
        }

        /// <summary>
        /// Converts a string value to the float value it represents.
        /// Returns 0 if the string does not represent a float.
        /// </summary>
        /// <param name="input">The string to get the value from.</param>
        /// <returns>a float value.</returns>
        public static float StringToFloat(string input)
        {
            float.TryParse(input, out float output);
            return output;
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
