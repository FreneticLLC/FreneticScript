using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers.Objects;

namespace FreneticScript.TagHandlers.Common
{
    /// <summary>
    /// Returns the various default colors within the tag system.
    /// </summary>
    public class TextColorTagBase : TemplateTagBase
    {
        // <--[tagbase]
        // @Base text_color[<TextTag>]
        // @Group Text Helpers
        // @ReturnType TextTag
        // @Returns the internal color code for the specified color name.
        // @Other TODO: Link full rundown of text colors.
        // -->

        /// <summary>
        /// Construct the TextColorTagBase - for internal use only.
        /// </summary>
        public TextColorTagBase()
        {
            Name = "text_color";
            ResultTypeString = "texttag";
        }

        /// <summary>
        /// Handles a 'color' tag.
        /// </summary>
        /// <param name="data">The data to be handled.</param>
        public override TemplateObject HandleOne(TagData data)
        {
            switch (data.GetModifier(0).ToLowerFast())
            {
                // <--[tag]
                // @Name TextColorTag.emphasis
                // @Group Colors
                // @ReturnType TextTag
                // @Returns the color codes for 'emphasis' (default: ^r^5).
                // @Other TODO: Link full rundown of text colors.
                // -->
                case "emphasis":
                    return new TextTag(TextStyle.Color_Separate);
                // <--[tag]
                // @Name TextColorTag.cmdhelp
                // @Group Colors
                // @ReturnType TextTag
                // @Returns the color codes for 'command help' (default: ^r^0^h^1).
                // @Other TODO: Link full rundown of text colors.
                // -->
                case "cmdhelp":
                    return new TextTag(TextStyle.Color_Commandhelp);
                // <--[tag]
                // @Name TextColorTag.simple
                // @Group Colors
                // @ReturnType TextTag
                // @Returns the 'simple default' color code (default: ^r^7).
                // @Other TODO: Link full rundown of text colors.
                // -->
                case "simple":
                    return new TextTag(TextStyle.Color_Simple);
                // <--[tag]
                // @Name TextColorTag.info
                // @Group Colors
                // @ReturnType TextTag
                // @Returns the 'important information' color code (default: ^r^3).
                // @Other TODO: Link full rundown of text colors.
                // -->
                case "info":
                    return new TextTag(TextStyle.Color_Importantinfo);
                // <--[tag]
                // @Name TextColorTag.standout
                // @Group Colors
                // @ReturnType TextTag
                // @Returns the 'standout' color code (default: ^r^0^h^5).
                // @Other TODO: Link full rundown of text colors.
                // -->
                case "standout":
                    return new TextTag(TextStyle.Color_Standout);
                // <--[tag]
                // @Name TextColorTag.warning
                // @Group Colors
                // @ReturnType TextTag
                // @Returns the 'warning' color code (default: ^r^0^h^1).
                // @Other TODO: Link full rundown of text colors.
                // -->
                case "warning":
                    return new TextTag(TextStyle.Color_Warning);
                // <--[tag]
                // @Name TextColorTag.base
                // @Group Colors
                // @ReturnType TextTag
                // @Returns the base/default color code (depends on situation where tag is called).
                // @Other TODO: Link full rundown of text colors.
                // -->
                case "base":
                    return new TextTag(data.BaseColor);
                default:
                    data.Error("Invalid text color specified!");
                    return new NullTag();
            }
        }
    }
}
