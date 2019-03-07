//
// This file is part of FreneticScript, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of the MIT license.
// See README.md or LICENSE.txt in the FreneticScript source root for the contents of the license.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Objects;
using System.Reflection;
using System.Reflection.Emit;
using FreneticUtilities.FreneticExtensions;
using FreneticScript.ScriptSystems;

namespace FreneticScript.CommandSystem.QueueCmds
{
    // <--[command]
    // @Name call
    // @Arguments <function to call> [--variable value ...]
    // @Short Runs a function.
    // @Updated 2016/04/27
    // @Authors mcmonkey
    // @Group Queue
    // @Minimum 1
    // @Maximum 1
    // @ReturnsValue true
    // @VarEqual MapTag the map of variables tracked at the end of the function call.
    // @Description
    // Activates a function created by the <@link command function>function<@/link> command.
    // TODO: Explain more!
    // @Example
    // // This example calls the function 'helloworld'.
    // call helloworld;
    // @Example
    // // This example calls the function 'outputme' with variable 'text' set to 'hello world'.
    // call outputme --text "hello world";
    // @Example
    // This example echoes the result of the function 'getinfo', from the tracked variable 'result'. Note that unexpected functionality may occur if 'getinfo' delays the variable set.
    // info ^= call getinfo;
    // echo <{[info].[result]}>;
    // @Example
    // This example echoes the result of the function 'getinfo', from the tracked variable 'result', after its full delayed run cycle.
    // info ^= &call getinfo;
    // echo <{[info].[result]}>;
    // @Example
    // TODO: More examples!
    // -->
    
    /// <summary>
    /// The Call command.
    /// </summary>
    public class CallCommand : AbstractCommand
    {
        /// <summary>
        /// Constructs the call command.
        /// </summary>
        public CallCommand()
        {
            Name = "call";
            Arguments = "<function to call> [--<variable> <value> ...]";
            Description = "Runs a function.";
            Asyncable = true;
            MinimumArguments = 1;
            MaximumArguments = 1;
            ObjectTypes = new List<Func<TemplateObject, TemplateObject>>()
            {
                TemplateObject.Basic_For
            };
        }

        /// <summary>
        /// Adapts a command entry to CIL.
        /// </summary>
        /// <param name="values">The adaptation-relevant values.</param>
        /// <param name="entry">The present entry ID.</param>
        public override void AdaptToCIL(CILAdaptationValues values, int entry)
        {
            base.AdaptToCIL(values, entry);
            values.ILGen.Emit(OpCodes.Ret);
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="queue">The command queue involved.</param>
        /// <param name="entry">Entry to be executed.</param>
        public static void Execute(CommandQueue queue, CommandEntry entry)
        {
            TemplateObject obj = entry.GetArgumentObject(queue, 0);
            FunctionTag function = FunctionTag.CreateFor(obj, queue.GetTagData());
            if (function == null)
            {
                queue.HandleError(entry, "Cannot call function '" + TextStyle.Separate + obj.ToString() + TextStyle.Base + "': it does not exist!");
                return;
            }
            CommandScript script = function.Internal;
            if (entry.ShouldShowGood(queue))
            {
                entry.GoodOutput(queue, "Calling '" + function.GetDebugString() + TextStyle.Base + "'...");
            }
            CompiledCommandStackEntry cse = script.Compiled.Duplicate();
            if (cse.Entries.Length > 0)
            {
                Dictionary<string, int> varlookup = cse.Entries[0].VarLookup;
                foreach (string var in entry.NamedArguments.Keys)
                {
                    if (!var.StartsWithNull())
                    {
                        if (varlookup.TryGetValue(var, out int varx))
                        {
                            // TODO: Type verification!
                            cse.LocalVariables[varx].Internal = entry.GetNamedArgumentObject(queue, var);
                        }
                    }
                }
            }
            if (entry.NamedArguments.ContainsKey(CommandEntry.SAVE_NAME_ARG_ID))
            {
                bool sgood = entry.ShouldShowGood(queue);
                string vname = entry.NamedArguments[CommandEntry.SAVE_NAME_ARG_ID].ToString();
                if (sgood)
                {
                    entry.GoodOutput(queue, "Noticing variable track for " + vname + ".");
                }
                CompiledCommandStackEntry ccse = queue.CurrentStackEntry;
                int x = -1;
                if (!entry.VarLookup.TryGetValue(vname, out x))
                {
                    queue.HandleError(entry, "Invalid save-to variable: " + vname + "!");
                    return;
                }
                cse.Callback = () =>
                {
                    if (cse.Entries.Length > 0)
                    {
                        MapTag mt = new MapTag();
                        Dictionary<string, int> varlookup = cse.Entries[0].VarLookup;
                        foreach (KeyValuePair<string, int> vara in varlookup)
                        {
                            if (cse.LocalVariables[vara.Value].Internal != null)
                            {
                                mt.Internal.Add(vara.Key, cse.LocalVariables[vara.Value].Internal);
                            }
                        }
                        ccse.LocalVariables[x].Internal = mt;
                    }
                    if (sgood)
                    {
                        entry.GoodOutput(queue, "Call complete.");
                    }
                };
            }
            queue.CommandStack.Push(cse);
        }
    }
}
