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

/// <summary>Handles the 'dynamic' tag base.</summary>
public class DynamicTagBase : TemplateTagBase
{
    // <--[tagbase]
    // @Base dynamic[<DynamicTag>]
    // @Group Common Base Types
    // @ReturnType DynamicTag
    // @Returns the input data as a DynamicTag.
    // -->

    /// <summary>Constructs the tag base data.</summary>
    public DynamicTagBase()
    {
        Name = "dynamic";
        ResultTypeString = DynamicTag.TYPE;
    }

    /// <summary>Handles the base input for a tag.</summary>
    /// <param name="data">The tag data.</param>
    /// <returns>The correct object.</returns>
    public static DynamicTag HandleOne(TagData data)
    {
        return DynamicTag.CreateFor(data.GetModifierObjectCurrent(), data);
    }
}
