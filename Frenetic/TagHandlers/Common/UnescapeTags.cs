using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Frenetic.TagHandlers.Objects;

namespace Frenetic.TagHandlers.Common
{
    /// <summary>
    /// Unescapes questionable text input.
    /// </summary>
    public class UnescapeTags : TemplateTags
    {
        // TODO: META

        /// <summary>
        /// Unescapes a string.
        /// </summary>
        /// <param name="input">The escaped string.</param>
        /// <returns>The unescaped string.</returns>
        public static string Unescape(string input)
        {
            return input.Replace("&sq", "\'")
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
        /// Construct the UnescapeTags - for internal use only.
        /// </summary>
        public UnescapeTags()
        {
            Name = "unescape";
        }

        /// <summary>
        /// Handles the 'unescape' tag.
        /// </summary>
        /// <param name="data">The data to be handled.</param>
        public override string Handle(TagData data)
        {
            string modif = data.GetModifier(0);
            return new TextTag(Unescape(modif)).Handle(data.Shrink());
        }
    }
}
