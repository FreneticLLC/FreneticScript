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
// @Name Maps
// @Description
// A map is a relationship between textual names and object values.
// TODO: Explain better!
// -->

/// <summary>Handles the 'map' tag base.</summary>
public class MapTagBase : TemplateTagBase
{
    // <--[tagbase]
    // @Base map[<MapTag>]
    // @Group Mathematics
    // @ReturnType MapTag
    // @Returns the specified input as a map.
    // <@link explanation maps>What are maps?<@/link>
    // -->

    /// <summary>Constructs the tag base data.</summary>
    public MapTagBase()
    {
        Name = "map";
        ResultTypeString = MapTag.TYPE;
    }

    /// <summary>Handles the base input for a tag.</summary>
    /// <param name="data">The tag data.</param>
    /// <returns>The correct object.</returns>
    public static MapTag HandleOne(TagData data)
    {
        return MapTag.CreateFor(data.GetModifierObjectCurrent(), data);
    }
}
