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
            ResultTypeString = TextTag.TYPE;
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
            if (Colors.TryGetValue(data.GetModifierCurrent().ToLowerFast(), out Func<string> getter))
            {
                return new TextTag(getter());
            }
            data.Error("Invalid text color specified!");
            return NullTag.NULL_VALUE;
        }
    }
}
