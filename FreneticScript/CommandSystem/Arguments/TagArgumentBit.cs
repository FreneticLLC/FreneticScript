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
using FreneticScript.TagHandlers.Common;
using FreneticScript.TagHandlers.Objects;
using FreneticScript.TagHandlers;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace FreneticScript.CommandSystem.Arguments
{
    /// <summary>
    /// Part of an argument that contains tags.
    /// </summary>
    public class TagArgumentBit: ArgumentBit
    {
        /// <summary>
        /// The pieces that make up the tag.
        /// </summary>
        public TagBit[] Bits;

        /// <summary>
        /// The method that gets the result of this TagArgumentBit.
        /// </summary>
        public MethodInfo GetResultMethod;

        /// <summary>
        /// Calls the method directly.
        /// </summary>
        public MethodHandler GetResultHelper;

        /// <summary>
        /// Parse the tag.
        /// </summary>
        /// <param name="data">The relevant tag data.</param>
        /// <returns>The parsed final object.</returns>
        public delegate TemplateObject MethodHandler(TagData data);

        /// <summary>
        /// Constructs a TagArgumentBit.
        /// </summary>
        /// <param name="system">The relevant command system.</param>
        /// <param name="bits">The tag bits.</param>
        public TagArgumentBit(Commands system, TagBit[] bits)
        {
            CommandSystem = system;
            Bits = bits;
        }

        /// <summary>
        /// Gets the resultant type of this argument bit.
        /// </summary>
        /// <param name="values">The relevant variable set.</param>
        /// <returns>The tag type.</returns>
        public override TagType ReturnType(CILAdaptationValues values)
        {
            if (Bits.Length == 1)
            {
                // TODO: Generic handler?
                if (Start is LvarTagBase)
                {
                    int var = (int)(((Bits[0].Variable.Bits[0]) as TextArgumentBit).InputValue as IntegerTag).Internal;
                    // TODO: Simpler lookup for var types. Probably a var type array.
                    for (int n = 0; n < values.CLVariables.Count; n++)
                    {
                        for (int i = 0; i < values.CLVariables[n].LVariables.Count; i++)
                        {
                            if (values.CLVariables[n].LVariables[i].Item1 == var)
                            {
                                return values.CLVariables[n].LVariables[i].Item3;
                            }
                        }
                    }
                    return null; // TODO: Error?
                }
                return Start.ResultType;
            }
            return Bits[Bits.Length - 1].TagHandler.Meta.ReturnTypeResult;
        }

        /// <summary>
        /// The tag to fall back on if this tag fails.
        /// </summary>
        public Argument Fallback;

        /// <summary>
        /// The starting point for this tag.
        /// </summary>
        public TemplateTagBase Start = null;

        /// <summary>
        /// The approximate tag data object to be used.
        /// </summary>
        public TagData Data;

        /// <summary>
        /// Preps the parsing of a <see cref="TagArgumentBit"/>.
        /// </summary>
        /// <param name="error">What to invoke if there is an error.</param>
        /// <param name="cse">The command stack entry.</param>
        public void PrepParse(Action<string> error, CompiledCommandStackEntry cse)
        {
            Data.Error = error;
            Data.cInd = 0;
            Data.Remaining = Bits.Length;
            Data.CSE = cse;
        }

        /// <summary>
        /// Parse the argument part, reading any tags.
        /// </summary>
        /// <param name="error">What to invoke if there is an error.</param>
        /// <param name="cse">The command stack entry.</param>
        /// <returns>The parsed final object.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public sealed override TemplateObject Parse(Action<string> error, CompiledCommandStackEntry cse)
        {
            // TODO: This isn't very thread safe.
            Data.Error = error;
            Data.cInd = 0;
            Data.Remaining = Bits.Length;
            Data.CSE = cse;
            return GetResultHelper(Data);
        }

        /// <summary>
        /// Returns the tag as tag input text.
        /// </summary>
        /// <returns>Tag input text.</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<");
            for (int i = 0; i < Bits.Length; i++)
            {
                sb.Append(Bits[i].ToString());
                if (i + 1 < Bits.Length)
                {
                    sb.Append(".");
                }
            }
            sb.Append(">");
            return sb.ToString();
        }
    }
}
