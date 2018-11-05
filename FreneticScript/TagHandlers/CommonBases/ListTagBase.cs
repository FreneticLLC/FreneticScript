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
    // @Name Lists
    // @Description
    // A list is a type of tag that contains multiple <@link explanation text tags>Text Tags<@/link>.
    // TODO: Explain better!
    // -->

    /// <summary>
    /// Handles the 'list' tag base.
    /// </summary>
    public class ListTagBase : TemplateTagBase
    {
        // <--[tagbase]
        // @Base list[<ListTag>]
        // @Group Mathematics
        // @ReturnType ListTag
        // @Returns the specified input as a list.
        // <@link explanation lists>What are lists?<@/link>
        // -->

        /// <summary>
        /// Constructs the tag base data.
        /// </summary>
        public ListTagBase()
        {
            Name = "list";
            ResultTypeString = "listtag";
        }

        /// <summary>
        /// Handles the base input for a tag.
        /// </summary>
        /// <param name="data">The tag data.</param>
        /// <returns>The correct object.</returns>
        public static TemplateObject HandleOne(TagData data)
        {
            return ListTag.CreateFor(data.GetModifierObjectCurrent());
        }
    }
}
