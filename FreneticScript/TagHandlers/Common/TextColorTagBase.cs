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
            Colors.Add("emphasis", () => TextStyle.Color_Separate);
            Colors.Add("cmdhelp", () => TextStyle.Color_Commandhelp);
            Colors.Add("simple", () => TextStyle.Color_Simple);
            Colors.Add("info", () => TextStyle.Color_Importantinfo);
            Colors.Add("standout", () => TextStyle.Color_Standout);
            Colors.Add("warning", () => TextStyle.Color_Warning);
            Colors.Add("base", () => TextStyle.Color_Base);
        }

        /// <summary>
        /// All colors known by this tag.
        /// </summary>
        public Dictionary<string, Func<string>> Colors = new Dictionary<string, Func<string>>();

        /// <summary>
        /// Handles a 'color' tag.
        /// </summary>
        /// <param name="data">The data to be handled.</param>
        public TemplateObject HandleOneObjective(TagData data)
        {
            if (Colors.TryGetValue(data.GetModifier(0).ToLowerFastFS(), out Func<string> getter))
            {
                return new TextTag(getter());
            }
            data.Error("Invalid text color specified!");
            return new NullTag();
        }
    }
}
