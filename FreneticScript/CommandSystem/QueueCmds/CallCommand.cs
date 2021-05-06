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
using FreneticUtilities.FreneticExtensions;
using FreneticScript.ScriptSystems;
using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Objects;

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
            CompiledCommandRunnable runnable = script.Compiled.ReferenceCompiledRunnable.Duplicate();
            if (runnable.Entry.Entries.Length > 0)
            {
                Dictionary<string, SingleCILVariable> varlookup = runnable.Entry.Entries[0].VarLookup;
                foreach (string var in entry.NamedArguments.Keys)
                {
                    if (!var.StartsWithNull())
                    {
                        if (varlookup.TryGetValue(var, out SingleCILVariable varx))
                        {
                            // TODO: Type verification!
                            runnable.Entry.GetSetter(varx.Index).Invoke(runnable, entry.GetNamedArgumentObject(queue, var));
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
                CompiledCommandRunnable curRunnable = queue.CurrentRunnable;
                if (!entry.VarLookup.TryGetValue(vname, out SingleCILVariable locVar))
                {
                    queue.HandleError(entry, "Invalid save-to variable: " + vname + "!");
                    return;
                }
                runnable.Callback = () =>
                {
                    // TODO: Fix!
                    /*if (runnable.Entry.Entries.Length > 0)
                    {
                        MapTag mt = new MapTag();
                        Dictionary<string, SingleCILVariable> varlookup = runnable.Entry.Entries[0].VarLookup;
                        foreach (SingleCILVariable vara in varlookup.Values)
                        {
                            if (runnable.LocalVariables[vara.Index].Internal != null)
                            {
                                mt.Internal.Add(vara.Name, runnable.LocalVariables[vara.Index].Internal);
                            }
                        }
                        curRunnable.LocalVariables[locVar.Index].Internal = mt;
                    }*/
                    if (sgood)
                    {
                        entry.GoodOutput(queue, "Call complete.");
                    }
                };
            }
            queue.RunningStack.Push(runnable);
        }
    }
}
