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

/// <summary>Handles the 'null' tag base.</summary>
public class NullTagBase : TemplateTagBase
{
    // <--[tagbase]
    // @Base null
    // @Group Common Base Types
    // @ReturnType NullTag
    // @Returns a NullTag.
    // -->

    /// <summary>Constructs the tag base data.</summary>
    public NullTagBase()
    {
        Name = "null";
        ResultTypeString = NullTag.TYPE;
    }

    /// <summary>Handles the base input for a tag.</summary>
    /// <param name="data">The tag data.</param>
    /// <returns>The correct object.</returns>
    public static NullTag HandleOne(TagData data)
    {
        return NullTag.NULL_VALUE;
    }
}
