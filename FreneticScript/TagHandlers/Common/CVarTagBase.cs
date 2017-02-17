using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers.Objects;

namespace FreneticScript.TagHandlers.Common
{
    /// <summary>
    /// Returns CVar information.
    /// </summary>
    public class CVarTagBase : TemplateTagBase
    {
        // <--[tagbase]
        // @Base cvar[<TextTag>]
        // @Group Variables
        // @ReturnType CVarTag
        // @Returns the specified global control variable.
        // <@link explanation cvars>What are CVars?<@/link>
        // -->

        /// <summary>
        /// Construct the CVarTags - for internal use only.
        /// </summary>
        public CVarTagBase()
        {
            Name = "cvar";
            ResultTypeString = "cvartag";
        }
        
        /// <summary>
        /// Handles a 'cvar' tag.
        /// </summary>
        /// <param name="data">The data to be handled.</param>
        public static TemplateObject HandleOne(TagData data)
        {
            return CVarTag.For(data, data.GetModifierObject(0));
        }
    }
}
