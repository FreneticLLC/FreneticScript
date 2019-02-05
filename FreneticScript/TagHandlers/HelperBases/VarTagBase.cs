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
using FreneticScript.CommandSystem;
using FreneticScript.CommandSystem.Arguments;
using FreneticUtilities.FreneticExtensions;

namespace FreneticScript.TagHandlers.HelperBases
{
    // <--[explanation]
    // @Name Queue Variables
    // @Description
    // Any given <@link explanation queue>queue<@/link> can have defined variables.
    // Variables are defined primarily by the <@link command define>define<@/link> command,
    // but can be added by other tags and commands, such as the <@link command repeat>repeat<@/link> command.
    // To use a queue variable in a tag, simply use the tag <@link tag var[<TextTag>]><{var[<TextTag>]}><@/link>.
    // TODO: Explain better!
    // -->

    /// <summary>
    /// Handles the 'var' tag base.
    /// </summary>
    public class VarTagBase : TemplateTagBase
    {
        // <--[tagbase]
        // @Base var[<TextTag>]
        // @Group Variables
        // @ReturnType <Dynamic>
        // @Returns the specified variable from the queue.
        // <@link explanation Queue Variables>What are queue variables?<@/link>
        // -->

        /// <summary>
        /// Constructs the tag base data.
        /// </summary>
        public VarTagBase()
        {
            Name = "var";
        }

        /// <summary>
        /// Handles the base input for a tag.
        /// </summary>
        /// <param name="data">The tag data.</param>
        /// <returns>The correct object.</returns>
        public static TemplateObject HandleOne(TagData data)
        {
            throw new NotImplementedException("Var tag MUST be compiled!");
        }

        /// <summary>
        /// Adapts the var tag base for compiling.
        /// </summary>
        /// <param name="ccse">The compiled CSE.</param>
        /// <param name="tab">The TagArgumentBit.</param>
        /// <param name="i">The command index.</param>
        public override TagType Adapt(CompiledCommandStackEntry ccse, TagArgumentBit tab, int i)
        {
            string vn = tab.Bits[0].Variable.ToString().ToLowerFast();
            CommandEntry entry = ccse.Entries[i];
            for (int n = 0; n < entry.CILVars.Length; n++)
            {
                for (int x = 0; x < entry.CILVars[n].LVariables.Count; x++)
                {
                    if (entry.CILVars[n].LVariables[x].Item2 == vn)
                    {
                        tab.Start = ccse.Entries[i].Command.Engine.TagSystem.LVar;
                        tab.Bits[0].Key = "\0lvar";
                        tab.Bits[0].Handler = null;
                        tab.Bits[0].OVar = tab.Bits[0].Variable;
                        tab.Bits[0].Variable = new Argument() { WasQuoted = false, Bits = new ArgumentBit[] { new TextArgumentBit(entry.CILVars[n].LVariables[x].Item1) } };
                        return entry.CILVars[n].LVariables[x].Item3;
                    }
                }
            }
            throw new ErrorInducedException("Var tag cannot compile: unknown variable name input '" + vn + "' (That variable name cannot be found. Have you declared it in this script section? Consider using the 'require' command.)");
        }
    }
}
