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

namespace FreneticScript.TagHandlers.CommonBases;

// <--[explanation]
// @Name Lists
// @Description
// A list is a type of tag that contains multiple <@link explanation text tags>Text Tags<@/link>.
// TODO: Explain better!
// -->

/// <summary>Handles the 'list' tag base.</summary>
public class ListTagBase : TemplateTagBase
{
    // <--[tagbase]
    // @Base list[<ListTag>]
    // @Group Mathematics
    // @ReturnType ListTag
    // @Returns the specified input as a list.
    // <@link explanation lists>What are lists?<@/link>
    // -->

    /// <summary>Constructs the tag base data.</summary>
    public ListTagBase()
    {
        Name = "list";
        ResultTypeString = ListTag.TYPE;
    }

    /// <summary>Handles the base input for a tag.</summary>
    /// <param name="data">The tag data.</param>
    /// <returns>The correct object.</returns>
    public static ListTag HandleOne(TagData data)
    {
        return ListTag.CreateFor(data.GetModifierObjectCurrent());
    }
}
