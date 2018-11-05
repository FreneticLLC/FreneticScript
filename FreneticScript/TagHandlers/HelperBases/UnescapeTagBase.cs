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
using FreneticScript.TagHandlers.Objects;
using FreneticUtilities.FreneticExtensions;

namespace FreneticScript.TagHandlers.HelperBases
{
    /// <summary>
    /// Unescapes questionable text input.
    /// </summary>
    public class UnescapeTagBase : TemplateTagBase
    {
        // <--[tagbase]
        // @Base unescape[<TextTag>]
        // @Group Mathematics
        // @ReturnType TextTag
        // @Returns an unescaped textual copy of the input escaped text.
        // -->

        /// <summary>
        /// All standard unescape codes.
        /// </summary>
        public static readonly Dictionary<string, char> Unescapes = EscapeTagBase.Escapes.SwapKeyValue();

        /// <summary>
        /// Unescapes a string.
        /// </summary>
        /// <param name="input">The escaped string.</param>
        /// <returns>The unescaped string.</returns>
        public static string Unescape(string input)
        {
            StringBuilder unescaped = new StringBuilder(input.Length);
            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] == '&')
                {
                    int x;
                    for (x = i; x < input.Length; x++)
                    {
                        if (input[x] == ',')
                        {
                            break;
                        }
                    }
                    string key = input.Substring(i + 1, x - i - 1);
                    if (Unescapes.TryGetValue(key, out char code))
                    {
                        unescaped.Append(code);
                        i = x;
                        continue;
                    }
                }
                unescaped.Append(input[i]);
            }
            return unescaped.ToString();
        }

        /// <summary>
        /// Construct the UnescapeTagBase - for internal use only.
        /// </summary>
        public UnescapeTagBase()
        {
            Name = "unescape";
            ResultTypeString = TextTag.TYPE;
        }

        /// <summary>
        /// Handles the 'unescape' tag.
        /// </summary>
        /// <param name="data">The data to be handled.</param>
        public static TemplateObject HandleOne(TagData data)
        {
            return new TextTag(Unescape(data.GetModifierCurrent()));
        }
    }
}
