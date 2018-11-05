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
