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

namespace FreneticScript.TagHandlers.CommonBases
{
    // <--[explanation]
    // @Name Text Tags
    // @Description
    // TextTags are any random text, built into the tag system.
    // TODO: Explain better
    // TODO: Link tag system explanation
    // -->

    /// <summary>
    /// Handles the 'text' tag base.
    /// </summary>
    public class TextTagBase : TemplateTagBase
    {
        // <--[tagbase]
        // @Base text[<TextTag>]
        // @Group Common Base Types
        // @ReturnType TextTag
        // @Returns the input text as a TextTag.
        // <@link explanation Text Tags>What are text tags?<@/link>
        // -->

        /// <summary>
        /// Constructs the tag base data.
        /// </summary>
        public TextTagBase()
        {
            Name = "text";
            ResultTypeString = TextTag.TYPE;
        }

        /// <summary>
        /// Handles the base input for a tag.
        /// </summary>
        /// <param name="data">The tag data.</param>
        /// <returns>The correct object.</returns>
        public static TemplateObject HandleOne(TagData data)
        {
            return new TextTag(data.GetModifierCurrent());
        }
    }
}
