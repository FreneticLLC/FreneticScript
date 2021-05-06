//
// This file is part of FreneticScript, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of the MIT license.
// See README.md or LICENSE.txt in the FreneticScript source root for the contents of the license.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticUtilities.FreneticExtensions;
using FreneticScript.TagHandlers.Objects;

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
        public static TextTag HandleOne(TagData data)
        {
            return new TextTag(Unescape(data.GetModifierCurrent()));
        }
    }
}
