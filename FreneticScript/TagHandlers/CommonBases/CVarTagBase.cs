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

namespace FreneticScript.TagHandlers.CommonBases
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
            ResultTypeString = CVarTag.TYPE;
        }
        
        /// <summary>
        /// Handles a 'cvar' tag.
        /// </summary>
        /// <param name="data">The data to be handled.</param>
        public static TemplateObject HandleOne(TagData data)
        {
            return CVarTag.For(data.GetModifierObjectCurrent(), data);
        }
    }
}
