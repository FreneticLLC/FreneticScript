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
using FreneticScript.ScriptSystems;
using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Objects;

namespace FreneticScript.CommandSystem.Arguments;

/// <summary>An Argument in the command system.</summary>
public class Argument
{
    /// <summary>Construct an <see cref="Argument"/>.</summary>
    public Argument()
    {
        TrueForm = this;
    }

    /// <summary>Construct an <see cref="Argument"/> with a specified bit array.</summary>
    /// <param name="_bits">The bits array input.</param>
    public Argument(params ArgumentBit[] _bits)
    {
        Bits = _bits;
        TrueForm = this;
    }

    /// <summary>Empty argument bit array.</summary>
    public static readonly ArgumentBit[] EMPTY_BITS = [];

    /// <summary>The parts that build up the argument.</summary>
    public ArgumentBit[] Bits = EMPTY_BITS;

    /// <summary>Whether the argument was input with "quotes" around it.</summary>
    public bool WasQuoted = true;

    /// <summary>The "true form" of this argument.</summary>
    public Argument TrueForm;

    /// <summary>The compiled parse method (if available).</summary>
    public MethodInfo CompiledParseMethod;

    /// <summary>The compiled raw parse method (if available).</summary>
    public MethodInfo RawParseMethod;

    /// <summary>Parse the argument, reading any tags or other special data.</summary>
    /// <param name="error">What to invoke if there is an error.</param>
    /// <param name="runnable">The command runnable.</param>
    /// <returns>The parsed final text.</returns>
    public virtual TemplateObject Parse(Action<string> error, CompiledCommandRunnable runnable)
    {
        if (TrueForm == null)
        {
            throw new ErrorInducedException("Arguments must be compiled before usage!");
        }
        return TrueForm.Parse(error, runnable);
    }

    /// <summary>Returns the argument as plain input text.</summary>
    /// <returns>The plain input text.</returns>
    public override string ToString()
    {
        if (Bits.Length == 1)
        {
            return Bits[0].ToString();
        }
        StringBuilder sb = new();
        for (int i = 0; i < Bits.Length; i++)
        {
            sb.Append(Bits[i].ToString());
        }
        return sb.ToString();
    }
}
