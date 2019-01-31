//
// This file is part of FreneticScript, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of the MIT license.
// See README.md or LICENSE.txt in the FreneticScript source root for the contents of the license.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers.CommonBases;
using FreneticScript.TagHandlers.HelperBases;
using FreneticScript.TagHandlers.Objects;
using FreneticScript.TagHandlers;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace FreneticScript.CommandSystem.Arguments
{
    /// <summary>
    /// Part of an argument that contains tags.
    /// </summary>
    public class TagArgumentBit: ArgumentBit
    {
        /// <summary>
        /// The <see cref="PrepParse(Action{string}, CompiledCommandStackEntry)"/> method.
        /// </summary>
        public static MethodInfo TagArgumentBit_PrepParse = typeof(TagArgumentBit).GetMethod(nameof(PrepParse));

        /// <summary>
        /// The <see cref="Data"/> field.
        /// </summary>
        public static FieldInfo TagArgumentBit_Data = typeof(TagArgumentBit).GetField(nameof(Data));

        /// <summary>
        /// Gets the <see cref="TagHandler"/> for this TagArgumentBit.
        /// </summary>
        public TagHandler TagSystem
        {
            get
            {
                return CommandSystem.TagSystem;
            }
        }

        /// <summary>
        /// The pieces that make up the tag.
        /// </summary>
        public TagBit[] Bits;

        /// <summary>
        /// The method that gets the result of this TagArgumentBit.
        /// </summary>
        public MethodInfo GetResultMethod;
        
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
        /// Generates a compiled call to the TagArgumentBit.
        /// </summary>
        /// <param name="ilgen">The IL Generator.</param>
        /// <param name="tab_loc">The TagArgumentBit helper local-variable location.</param>
        /// <param name="load_Error">The OpCode to load the error object.</param>
        /// <param name="load_Cse">The OpCode to load the CSE object.</param>
        /// <param name="obj_loc">The TemplateObject helper local-variable location.</param>
        public void GenerateCall(CILAdaptationValues.ILGeneratorTracker ilgen, int tab_loc, OpCode load_Error, OpCode load_Cse, int obj_loc)
        {
            ilgen.Emit(OpCodes.Stloc, tab_loc); // Store the TAB to the proper location
            Label exceptionLabel = ilgen.BeginExceptionBlock(); // try {
            ilgen.Emit(OpCodes.Ldloc, tab_loc); // Load the tag onto stack
            ilgen.Emit(OpCodes.Dup); // Duplicate the tag for repeated usage
            ilgen.Emit(load_Error); // Load the error object onto stack
            ilgen.Emit(load_Cse); // Load the CSE object onto stack.
            ilgen.Emit(OpCodes.Call, TagArgumentBit_PrepParse); // Call the PrepParse method (pulls TagArgumentBit + Error + CSE from stack)
            ilgen.Emit(OpCodes.Ldfld, TagArgumentBit_Data); // Read 'data' (from current tab, gathered from duplicate above)
            ilgen.Emit(OpCodes.Call, GetResultMethod); // Call the GetResultMethod (takes one param: TagData, returns a TemplateObject).
            ilgen.Emit(OpCodes.Stloc, obj_loc); // Store the TemplateObject where it belongs
            ilgen.Emit(OpCodes.Leave, exceptionLabel); // }
            ilgen.BeginCatchBlock(typeof(TagErrorInducedException)); // catch (Exception ex) {
            ilgen.Emit(OpCodes.Pop); // pop the exception off stack
            ilgen.Emit(OpCodes.Ldloc, tab_loc); // Load the tag onto stack
            ilgen.Emit(OpCodes.Ldfld, TagArgumentBit_Data); // Read 'data' (from current tab, gathered from duplicate above)
            ilgen.Emit(OpCodes.Ldfld, TagData.Field_Fallback); // Read 'data'.Fallback field
            ilgen.Emit(load_Error); // Load the error object onto stack
            ilgen.Emit(load_Cse); // Load the CSE object onto stack.
            ilgen.Emit(OpCodes.Callvirt, Argument.Argument_Parse); // Virtual call the Argument.Parse method, which returns a TemplateObject
            ilgen.Emit(OpCodes.Stloc, obj_loc); // Store the TemplateObject where it belongs
            ilgen.EndExceptionBlock(); // }
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
                    return values.LocalVariableType(var);
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
            // TODO: This isn't very thread safe.
            Data.ErrorHandler = error;
            Data.cInd = 0;
            Data.CSE = cse;
        }

        /// <summary>
        /// Parse the argument part, reading any tags or other special data.
        /// For TagArgumentBit objects, this will result in a failure exception (as it should be compiled normally).
        /// </summary>
        /// <param name="error">What to invoke if there is an error.</param>
        /// <param name="cse">The command stack entry.</param>
        /// <returns>The parsed final text.</returns>
        public override TemplateObject Parse(Action<string> error, CompiledCommandStackEntry cse)
        {
            throw new NotSupportedException("Use the compiled argument parse handler.");
        }

        /// <summary>
        /// Returns the tag as tag input text, highlighting a specific index. Does not include wrapping tag marks.
        /// </summary>
        /// <param name="index">The index to highlight at.</param>
        /// <param name="highlight">The highlight string.</param>
        /// <returns>Tag input text.</returns>
        public string HighlightString(int index, string highlight)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < Bits.Length; i++)
            {
                if (i == index)
                {
                    sb.Append(highlight);
                }
                sb.Append(Bits[i].ToString());
                if (i + 1 < Bits.Length)
                {
                    sb.Append(".");
                }
            }
            return sb.ToString();
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
