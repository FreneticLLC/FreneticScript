//
// This file is part of FreneticScript, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of the MIT license.
// See README.md or LICENSE.txt in the FreneticScript source root for the contents of the license.
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
            Colors.Add("emphasis", () => TextStyle.Separate);
            Colors.Add("cmdhelp", () => TextStyle.Commandhelp);
            Colors.Add("simple", () => TextStyle.Simple);
            Colors.Add("info", () => TextStyle.Importantinfo);
            Colors.Add("standout", () => TextStyle.Standout);
            Colors.Add("warning", () => TextStyle.Warning);
            Colors.Add("base", () => TextStyle.Base);
        }

        /// <summary>
        /// All colors known by this tag.
        /// </summary>
        public Dictionary<string, Func<string>> Colors = new Dictionary<string, Func<string>>();

        /// <summary>
        /// Handles a 'color' tag.
        /// </summary>
        /// <param name="data">The data to be handled.</param>
        public TextTag HandleOneObjective(TagData data)
        {
            if (Colors.TryGetValue(data.GetModifierCurrent().ToLowerFast(), out Func<string> getter))
            {
                return new TextTag(getter());
            }
            throw data.Error("Invalid text color specified!");
        }
    }
}
