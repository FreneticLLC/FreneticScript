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
using System.Runtime.CompilerServices;
using System.Text;
using FreneticUtilities.FreneticToolkit;
using FreneticScript.ScriptSystems;
using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.CommonBases;
using FreneticScript.TagHandlers.HelperBases;
using FreneticScript.TagHandlers.Objects;

namespace FreneticScript.CommandSystem.Arguments;

/// <summary>Part of an argument that contains tags.</summary>
public class TagArgumentBit: ArgumentBit
{
    /// <summary>The <see cref="PrepParse(Action{string}, CompiledCommandRunnable)"/> method.</summary>
    public static MethodInfo TagArgumentBit_PrepParse = typeof(TagArgumentBit).GetMethod(nameof(PrepParse));

    /// <summary>The <see cref="Data"/> field.</summary>
    public static FieldInfo TagArgumentBit_Data = typeof(TagArgumentBit).GetField(nameof(Data));

    /// <summary>Gets the <see cref="TagHandler"/> for this TagArgumentBit.</summary>
    public TagHandler TagSystem
    {
        get
        {
            return Engine.TagSystem;
        }
    }

    /// <summary>The pieces that make up the tag.</summary>
    public TagBit[] Bits;

    /// <summary>The method that gets the result of this TagArgumentBit.</summary>
    public MethodInfo GetResultMethod;

    /// <summary>The type that will be returned by the <see cref="GetResultMethod"/>.</summary>
    public TagReturnType CompiledReturnType;

    /// <summary>Constructs a TagArgumentBit.</summary>
    /// <param name="system">The relevant command system.</param>
    /// <param name="bits">The tag bits.</param>
    public TagArgumentBit(ScriptEngine system, TagBit[] bits)
    {
        Engine = system;
        Bits = bits;
    }

    /// <summary>Generates a compiled call to the TagArgumentBit.</summary>
    /// <param name="ilgen">The IL Generator.</param>
    /// <param name="tab_loc">The TagArgumentBit helper local-variable location.</param>
    /// <param name="load_Error">The OpCode to load the error object.</param>
    /// <param name="load_Runnable">The OpCode to load the runnable object.</param>
    /// <param name="obj_loc">The TemplateObject helper local-variable location.</param>
    /// <param name="return_raw">Whether a raw value should be returned.</param>
    public void GenerateCall(ILGeneratorTracker ilgen, int tab_loc, OpCode load_Error, OpCode load_Runnable, int obj_loc, bool return_raw)
    {
        ilgen.Emit(OpCodes.Stloc, tab_loc); // Store the TAB to the proper location
        Label exceptionLabel = default;
        if (Data.HasFallback)
        {
            exceptionLabel = ilgen.BeginExceptionBlock(); // try {
        }
        ilgen.Emit(OpCodes.Ldloc, tab_loc); // Load the tag onto stack
        ilgen.Emit(load_Error); // Load the error object onto stack
        ilgen.Emit(load_Runnable); // Load the runnable object onto stack.
        ilgen.Emit(OpCodes.Call, TagArgumentBit_PrepParse); // Call the PrepParse method (pulls TagArgumentBit + Error + CSE from stack)
        ilgen.Emit(OpCodes.Ldloc, tab_loc); // Load the tag onto stack
        ilgen.Emit(OpCodes.Ldfld, TagArgumentBit_Data); // Read 'data' (from current tab)
        ilgen.Emit(load_Runnable); // Load the runnable object onto stack.
        ilgen.Emit(load_Error); // Load the error object onto stack.
        ilgen.Emit(OpCodes.Call, GetResultMethod, 3); // Call the GetResultMethod (takes three params: (TagData, CompiledCommandRunnable, Action<string>), and returns a TemplateObject).
        if (CompiledReturnType.IsRaw && !return_raw)
        {
            ilgen.Emit(OpCodes.Newobj, CompiledReturnType.Type.RawInternalConstructor); // Handle raw translation if needed.
        }
        ilgen.Emit(OpCodes.Stloc, obj_loc); // Store the TemplateObject where it belongs
        if (Data.HasFallback)
        {
            ilgen.Emit(OpCodes.Leave, exceptionLabel); // }
            ilgen.BeginCatchBlock(typeof(TagErrorInducedException)); // catch (Exception ex) {
            ilgen.Emit(OpCodes.Pop); // pop the exception off stack
            ilgen.Emit(OpCodes.Ldloc, tab_loc); // Load the tag onto stack
            ilgen.Emit(OpCodes.Ldfld, TagArgumentBit_Data); // Read 'data' (from current tab)
            ilgen.Emit(OpCodes.Ldfld, TagData.Field_Fallback); // Read 'data'.Fallback field
            ilgen.Emit(load_Error); // Load the error object onto stack
            ilgen.Emit(load_Runnable); // Load the runnable object onto stack.
            ilgen.Emit(OpCodes.Callvirt, ArgumentCompiler.Argument_Parse); // Virtual call the Argument.Parse method, which returns a TemplateObject
            ilgen.Emit(OpCodes.Ldloc, tab_loc); // Load the tag onto stack
            ilgen.Emit(OpCodes.Ldfld, TagArgumentBit_Data); // Read 'data' (from current tab)
            ilgen.Emit(OpCodes.Call, CompiledReturnType.Type.CreatorMethod); // Validate type
            if (CompiledReturnType.IsRaw && return_raw)
            {
                ilgen.Emit(OpCodes.Ldfld, CompiledReturnType.Type.RawInternalField); // Handle raw translation if needed.
            }
            ilgen.Emit(OpCodes.Stloc, obj_loc); // Store the TemplateObject where it belongs
            ilgen.EndExceptionBlock(); // }
        }
    }

    /// <summary>Gets the resultant type of this argument bit.</summary>
    /// <param name="values">The relevant variable set.</param>
    /// <returns>The tag type.</returns>
    public override TagReturnType ReturnType(CILAdaptationValues values)
    {
        if (Bits.Length == 1)
        {
            // TODO: Generic handler?
            if (Start is LvarTagBase)
            {
                int var = (int)(((Bits[0].Variable.Bits[0]) as TextArgumentBit).InputValue as IntegerTag).Internal;
                return values.LocalVariableType(var);
            }
            return Start.ResultType;
        }
        return Bits[^1].TagHandler.Meta.ReturnTypeResult;
    }

    /// <summary>The tag to fall back on if this tag fails.</summary>
    public Argument Fallback;

    /// <summary>The starting point for this tag.</summary>
    public TemplateTagBase Start = null;

    /// <summary>The approximate tag data object to be used.</summary>
    public TagData Data;

    /// <summary>Preps the parsing of a <see cref="TagArgumentBit"/>.</summary>
    /// <param name="error">What to invoke if there is an error.</param>
    /// <param name="runnable">The command runnable.</param>
    public void PrepParse(Action<string> error, CompiledCommandRunnable runnable)
    {
        // TODO: This isn't very thread safe.
        Data.ErrorHandler = error;
        Data.cInd = 0;
        Data.Runnable = runnable;
    }

    /// <summary>
    /// Parse the argument part, reading any tags or other special data.
    /// For TagArgumentBit objects, this will result in a failure exception (as it should be compiled normally).
    /// </summary>
    /// <param name="error">What to invoke if there is an error.</param>
    /// <param name="runnable">The command runnable.</param>
    /// <returns>The parsed final text.</returns>
    public override TemplateObject Parse(Action<string> error, CompiledCommandRunnable runnable)
    {
        throw new NotSupportedException("Use the compiled argument parse handler.");
    }

    /// <summary>Returns the tag as tag input text, highlighting a specific index. Does not include wrapping tag marks.</summary>
    /// <param name="index">The index to highlight at.</param>
    /// <param name="highlight">The highlight string.</param>
    /// <returns>Tag input text.</returns>
    public string HighlightString(int index, string highlight)
    {
        StringBuilder sb = new();
        for (int i = 0; i < Bits.Length; i++)
        {
            if (i == index)
            {
                sb.Append(highlight);
            }
            sb.Append(Bits[i].ToString());
            if (i + 1 < Bits.Length)
            {
                sb.Append('.');
            }
        }
        return sb.ToString();
    }

    /// <summary>Returns the tag as tag input text.</summary>
    /// <returns>Tag input text.</returns>
    public override string ToString()
    {
        StringBuilder sb = new();
        sb.Append('<');
        for (int i = 0; i < Bits.Length; i++)
        {
            sb.Append(Bits[i].ToString());
            if (i + 1 < Bits.Length)
            {
                sb.Append('.');
            }
        }
        sb.Append('>');
        return sb.ToString();
    }
}
