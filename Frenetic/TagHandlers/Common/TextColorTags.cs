using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Frenetic.TagHandlers.Objects;

namespace Frenetic.TagHandlers.Common
{
    /// <summary>
    /// Returns the various default colors within the tag system.
    /// </summary>
    public class TextColorTags : TemplateTagBase
    {
        // <--[tagbase]
        // @Base text_color
        // @Group Text Helpers
        // @ReturnType TextColorTag
        // @Returns a generic color class full of specific helpful color tags,
        // such as <@link tag TextColorTag.emphasis><{text_color.emphasis}><@/link>.
        // TODO: Link full rundown of text colors.
        // -->

        /// <summary>
        /// Construct the ColorTags - for internal use only.
        /// </summary>
        public TextColorTags()
        {
            Name = "text_color";
        }

        /// <summary>
        /// Handles a 'color' tag.
        /// </summary>
        /// <param name="data">The data to be handled.</param>
        public override string Handle(TagData data)
        {
            data.Shrink();
            if (data.Input.Count == 0)
            {
                return ToString();
            }
            switch (data.Input[0])
            {
                // <--[tag]
                // @Name TextColorTag.emphasis
                // @Group Colors
                // @ReturnType TextTag
                // @Returns the color codes for 'emphasis' (default: ^r^5).
                // TODO: Link full rundown of text colors.
                // -->
                case "emphasis":
                    return new TextTag(TextStyle.Color_Separate).Handle(data.Shrink());
                // <--[tag]
                // @Name TextColorTag.cmdhelp
                // @Group Colors
                // @ReturnType TextTag
                // @Returns the color codes for 'command help' (default: ^r^0^h^1).
                // TODO: Link full rundown of text colors.
                // -->
                case "cmdhelp":
                    return new TextTag(TextStyle.Color_Commandhelp).Handle(data.Shrink());
                // <--[tag]
                // @Name TextColorTag.simple
                // @Group Colors
                // @ReturnType TextTag
                // @Returns the 'simple default' color code (default: ^r^7).
                // TODO: Link full rundown of text colors.
                // -->
                case "simple":
                    return new TextTag(TextStyle.Color_Simple).Handle(data.Shrink());
                // <--[tag]
                // @Name TextColorTag.info
                // @Group Colors
                // @ReturnType TextTag
                // @Returns the 'important information' color code (default: ^r^3).
                // TODO: Link full rundown of text colors.
                // -->
                case "info":
                    return new TextTag(TextStyle.Color_Importantinfo).Handle(data.Shrink());
                // <--[tag]
                // @Name TextColorTag.standout
                // @Group Colors
                // @ReturnType TextTag
                // @Returns the 'standout' color code (default: ^r^0^h^5).
                // TODO: Link full rundown of text colors.
                // -->
                case "standout":
                    return new TextTag(TextStyle.Color_Standout).Handle(data.Shrink());
                // <--[tag]
                // @Name TextColorTag.warning
                // @Group Colors
                // @ReturnType TextTag
                // @Returns the 'warning' color code (default: ^r^0^h^1).
                // TODO: Link full rundown of text colors.
                // -->
                case "warning":
                    return new TextTag(TextStyle.Color_Warning).Handle(data.Shrink());
                // <--[tag]
                // @Name TextColorTag.base
                // @Group Colors
                // @ReturnType TextTag
                // @Returns the base/default color code (depends on situation where tag is called).
                // TODO: Link full rundown of text colors.
                // -->
                case "base":
                    return new TextTag(data.BaseColor).Handle(data.Shrink());
                default:
                    return new TextTag(ToString()).Handle(data);
            }
        }
    }
}
