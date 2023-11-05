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

namespace FreneticScript.TagHandlers.HelperBases;

/// <summary>Handles the 'from_saved' tag base.</summary>
public class FromSavedTagBase : TemplateTagBase
{
    // <--[tagbase]
    // @Base from_saved[<TextTag>]
    // @Group Common Base Types
    // @ReturnType DynamicTag
    // @Returns the saved copy of a tag input converted to the correct tag type.
    // -->

    /// <summary>Constructs the tag base data.</summary>
    public FromSavedTagBase()
    {
        Name = "from_saved";
        ResultTypeString = DynamicTag.TYPE;
    }

    /// <summary>Handles the base input for a tag.</summary>
    /// <param name="data">The tag data.</param>
    /// <returns>The correct object.</returns>
    public static DynamicTag HandleOne(TagData data)
    {
        return new DynamicTag(data.TagSystem.ParseFromSaved(data.GetModifierCurrent(), data));
    }
}
