using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Frenetic.TagHandlers.Objects;

namespace Frenetic.TagHandlers.Common
{
    /// <summary>
    /// Escapes questionable text input.
    /// </summary>
    public class EscapeTags : TemplateTags
    {
        // <--[tagbase]
        // @Base escape[<TextTag>]
        // @Group Mathematics
        // @ReturnType TextTag
        // @Returns an escaped textual copy of the input text.
        // -->

        // TODO: Full explanation!

        /// <summary>
        /// Escapes a string.
        /// </summary>
        /// <param name="input">The unescaped string.</param>
        /// <returns>The escaped string.</returns>
        public static string Escape(string input)
        {
            return input.Replace("&", "&amp")
                .Replace("|", "&pipe")
                .Replace(";", "&semi")
                .Replace(":", "&colon")
                .Replace(" ", "&sp")
                .Replace(".", "&dot")
                .Replace("[", "&lb")
                .Replace("]", "&rb")
                .Replace("<", "&lt")
                .Replace(">", "&gt")
                .Replace("\"", "&quot")
                .Replace("\'", "&sq"); // TODO: More efficient method
        }

        /// <summary>
        /// Construct the EscapeTags - for internal use only.
        /// </summary>
        public EscapeTags()
        {
            Name = "escape";
        }

        /// <summary>
        /// Handles the escape tag.
        /// </summary>
        /// <param name="data">The data to be handled.</param>
        public override string Handle(TagData data)
        {
            string modif = data.GetModifier(0);
            return new TextTag(Escape(modif)).Handle(data.Shrink());
        }
    }
}
