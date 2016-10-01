using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers.Objects;

namespace FreneticScript.TagHandlers.Common
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
        /// Unescapes a string.
        /// </summary>
        /// <param name="input">The escaped string.</param>
        /// <returns>The unescaped string.</returns>
        public static string Unescape(string input)
        {
            return input.Replace("&sq", "\'")
                .Replace("&car", "^")
                .Replace("&quot", "\"")
                .Replace("&gt", ">")
                .Replace("&lt", "<")
                .Replace("&rb", "]")
                .Replace("&lb", "[")
                .Replace("&dot", ".")
                .Replace("&sp", " ")
                .Replace("&colon", ":")
                .Replace("&semi", ";")
                .Replace("&pipe", "|")
                .Replace("&amp", "&"); // TODO: More efficient method
        }

        /// <summary>
        /// Construct the UnescapeTagBase - for internal use only.
        /// </summary>
        public UnescapeTagBase()
        {
            Name = "unescape";
            ResultTypeString = "texttag";
        }

        /// <summary>
        /// Handles the 'unescape' tag.
        /// </summary>
        /// <param name="data">The data to be handled.</param>
        public override TemplateObject HandleOne(TagData data)
        {
            return new TextTag(Unescape(data.GetModifier(0)));
        }
    }
}
