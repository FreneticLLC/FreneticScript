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

namespace FreneticScript.TagHandlers.Common
{
    /// <summary>
    /// Escapes questionable text input.
    /// </summary>
    public class EscapeTagBase : TemplateTagBase
    {
        // <--[tagbase]
        // @Base escape[<TextTag>]
        // @Group Mathematics
        // @ReturnType TextTag
        // @Returns an escaped textual copy of the input text.
        // -->

        // TODO: Full explanation!

        /// <summary>
        /// All standard escape codes.
        /// </summary>
        public static readonly Dictionary<char, string> Escapes = new Dictionary<char, string>()
        {
            { '&', "amp" },
            { '|', "pipe" },
            { ';', "semi" },
            { ':', "colon" },
            { ' ', "sp" },
            { '.', "dot" },
            { '[', "lb" },
            { ']', "rb" },
            { '<', "lt" },
            { '>', "gt" },
            { '^', "caret" },
            { '\'', "sq" },
            { '\"', "quot" },
            { '\r', "carriage" },
            { '\n', "newline" }
        };

        /// <summary>
        /// Escapes a string.
        /// </summary>
        /// <param name="input">The unescaped string.</param>
        /// <returns>The escaped string.</returns>
        public static string Escape(string input)
        {
            StringBuilder escaped = new StringBuilder(input.Length);
            for (int i = 0; i < input.Length; i++)
            {
                if ((input[i] >= 'a' && input[i] <= 'z')
                    || (input[i] >= 'A' && input[i] <= 'Z')
                    || (input[i] >= '0' && input[i] <= '9')
                    || (input[i] > 127)) // ignore non-ASCII entirely
                {
                    escaped.Append(input[i]);
                }
                else if (Escapes.TryGetValue(input[i], out string replace))
                {
                    escaped.Append('&').Append(replace).Append(',');
                }
                else
                {
                    escaped.Append(input[i]);
                }
            }
            return escaped.ToString();
        }

        /// <summary>
        /// Construct the EscapeTags - for internal use only.
        /// </summary>
        public EscapeTagBase()
        {
            Name = "escape";
            ResultTypeString = "texttag";
        }

        /// <summary>
        /// Handles the escape tag.
        /// </summary>
        /// <param name="data">The data to be handled.</param>
        public static TemplateObject HandleOne(TagData data)
        {
            string modif = data.GetModifierCurrent();
            return new TextTag(Escape(modif));
        }
    }
}
