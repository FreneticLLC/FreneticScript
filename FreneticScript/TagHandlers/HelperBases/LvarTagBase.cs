//
// This file is part of FreneticScript, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of the MIT license.
// See README.md or LICENSE.txt in the FreneticScript source root for the contents of the license.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using FreneticUtilities.FreneticToolkit;
using FreneticScript.CommandSystem;
using FreneticScript.CommandSystem.Arguments;
using FreneticScript.ScriptSystems;
using FreneticScript.TagHandlers.Objects;

namespace FreneticScript.TagHandlers.HelperBases;

/// <summary>Handles internal compiled var tags.</summary>
public class LvarTagBase : TemplateTagBase
{
    // No meta: compiled only.

    /// <summary>Construct the Lvar tag base.</summary>
    public LvarTagBase()
    {
        Name = "\0lvar";
    }

    /// <summary>Adapts the tag base to CIL.</summary>
    /// <param name="ilgen">IL Generator.</param>
    /// <param name="tab">The TagArgumentBit.</param>
    /// <param name="values">Related adaptation values.</param>
    /// <returns>Whether any adaptation was done.</returns>
    public override bool AdaptToCIL(ILGeneratorTracker ilgen, TagArgumentBit tab, CILAdaptationValues values)
    {
        int index = (int)((tab.Bits[0].Variable.Bits[0] as TextArgumentBit).InputValue as IntegerTag).Internal;
        ilgen.Emit(OpCodes.Ldarg_0); // Load argument: TagData
        ilgen.Emit(OpCodes.Ldfld, TagData.Field_TagData_Runnable); // Load TagData.Runnable
        ilgen.Emit(OpCodes.Ldfld, values.LocalVariableData(index).Field); // Load Runnable.Var
        return true;
    }

    /// <summary>Adapts the var tag base for compiling.</summary>
    /// <param name="ccse">The compiled CSE.</param>
    /// <param name="tab">The TagArgumentBit.</param>
    /// <param name="i">The command index.</param>
    /// <param name="values">Related adaptation values.</param>
    public override TagReturnType Adapt(CompiledCommandStackEntry ccse, TagArgumentBit tab, int i, CILAdaptationValues values)
    {
        int index = (int)((tab.Bits[0].Variable.Bits[0] as TextArgumentBit).InputValue as IntegerTag).Internal;
        return values.LocalVariableType(index);
    }

    /// <summary>Handles a single entry.</summary>
    /// <param name="data">The data.</param>
    /// <returns>The result.</returns>
    public static TemplateObject HandleOne(TagData data)
    {
        throw new NotImplementedException("LVar was called incorrectly!");
    }
}
