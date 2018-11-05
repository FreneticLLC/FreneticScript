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
using System.Reflection;
using System.Reflection.Emit;

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
            IsFlow = true;
            Asyncable = true;
            MinimumArguments = 1;
            MaximumArguments = 1;
            ObjectTypes = new List<Func<TemplateObject, TemplateObject>>()
            {
                TextTag.For
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
            string fname = entry.GetArgument(queue, 0);
            fname = fname.ToLowerFastFS();
            CommandScript script = queue.CommandSystem.GetFunction(fname);
            if (script == null)
            {
                queue.HandleError(entry, "Cannot call function '<{text_color[emphasis]}>" + TagParser.Escape(fname) + "<{text_color[base]}>': it does not exist!");
                return;
            }
            if (entry.ShouldShowGood(queue))
            {
                entry.Good(queue, "Calling '<{text_color[emphasis]}>" + TagParser.Escape(fname) + "<{text_color[base]}>'...");
            }
            CompiledCommandStackEntry cse = script.Compiled.Duplicate();
            if (cse.Entries.Length > 0)
            {
                Dictionary<string, int> varlookup = cse.Entries[0].VarLookup;
                foreach (string var in entry.NamedArguments.Keys)
                {
                    if (!var.StartsWithNullFS())
                    {
                        if (varlookup.TryGetValue(var, out int varx))
                        {
                            // TODO: Type verification!
                            if (cse.LocalVariables[varx] != null)
                            {
                                cse.LocalVariables[varx].Internal = entry.GetNamedArgumentObject(queue, var);
                            }
                            else
                            {
                                cse.LocalVariables[varx] = new ObjectHolder() { Internal = entry.GetNamedArgumentObject(queue, var) };
                            }
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
                    entry.Good(queue, "Noticing variable track for " + vname + ".");
                }
                CompiledCommandStackEntry ccse = queue.CurrentEntry;
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
                            if (cse.LocalVariables[vara.Value]?.Internal != null)
                            {
                                mt.Internal.Add(vara.Key, cse.LocalVariables[vara.Value].Internal);
                            }
                        }
                        if (ccse.LocalVariables[x] != null)
                        {
                            ccse.LocalVariables[x].Internal = mt;
                        }
                        else
                        {
                            ccse.LocalVariables[x] = new ObjectHolder() { Internal = mt };
                        }
                    }
                    if (sgood)
                    {
                        entry.Good(queue, "Call complete.");
                    }
                };
            }
            queue.CommandStack.Push(cse);
        }
    }
}
