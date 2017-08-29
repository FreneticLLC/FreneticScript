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
using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Objects;

namespace FreneticScript.CommandSystem.Arguments
{
    /// <summary>
    /// An Argument in the command system.
    /// </summary>
    public class Argument
    {
        /// <summary>
        /// The parts that build up the argument.
        /// </summary>
        public List<ArgumentBit> Bits = new List<ArgumentBit>();

        /// <summary>
        /// Whether the argument was input with "quotes" around it.
        /// </summary>
        public bool WasQuoted = true;
        
        /// <summary>
        /// Gets the resultant type of this argument.
        /// </summary>
        /// <param name="values">The relevant variable set.</param>
        /// <returns>The tag type.</returns>
        public TagType ReturnType(CILAdaptationValues values)
        {
            if (Bits.Count == 1)
            {
                return Bits[0].ReturnType(values);
            }
            else
            {
                return Bits[0].CommandSystem.TagSystem.Type_Text;
            }
        }

        /// <summary>
        /// Parse the argument, reading any tags or other special data.
        /// </summary>
        /// <param name="error">What to invoke if there is an error.</param>
        /// <param name="cse">The command stack entry.</param>
        /// <returns>The parsed final text.</returns>
        public TemplateObject Parse(Action<string> error, CompiledCommandStackEntry cse)
        {
            // TODO: Compile this cleverly. This if and some other parts can probably be pre-computed!
            if (Bits.Count == 1)
            {
                return Bits[0].Parse(error, cse);
            }
            StringBuilder built = new StringBuilder();
            for (int i = 0; i < Bits.Count; i++)
            {
                built.Append(Bits[i].Parse(error, cse).ToString());
            }
            return new TextTag(built.ToString());
        }

        /// <summary>
        /// Returns the argument as plain input text.
        /// </summary>
        /// <returns>The plain input text.</returns>
        public override string ToString()
        {
            if (Bits.Count == 1)
            {
                return Bits[0].ToString();
            }
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < Bits.Count; i++)
            {
                sb.Append(Bits[i].ToString());
            }
            return sb.ToString();
        }
    }
}
