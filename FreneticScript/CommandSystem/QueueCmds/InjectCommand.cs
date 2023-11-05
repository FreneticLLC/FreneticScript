//
// This file is part of FreneticScript, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of the MIT license.
// See README.md or LICENSE.txt in the FreneticScript source root for the contents of the license.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticUtilities.FreneticExtensions;
using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Objects;

namespace FreneticScript.CommandSystem.QueueCmds;

// <--[command]
// @Name inject
// @Arguments <function to inject>
// @Short Runs a function inside the current variable base.
// @Updated 2016/04/28
// @Authors mcmonkey
// @Group Queue
// @Minimum 1
// @Maximum 1
// @Description
// Injects a function created by the <@link command function>function<@/link> command.
// IMPORTANT NOTE: Generally, the <@link command call>call<@/link> command should be used instead of this.
// TODO: Explain more!
// @Example
// // This example injects the function 'helloworld'.
// inject helloworld;
// @Example
// TODO: More examples!
// -->

// TODO: Remove? Make compatible with CIL update somehow?

/// <summary>The Inject command.</summary>
public class InjectCommand : AbstractCommand
{
    /// <summary>Constructs the inject command.</summary>
    public InjectCommand()
    {
        Name = "inject";
        Arguments = "<function to call>";
        Description = "Runs a function.";
        Asyncable = true;
        MinimumArguments = 1;
        MaximumArguments = 1;
        ObjectTypes = new Action<ArgumentValidation>[]
        {
            TextTag.Validator
        };
    }

    // TODO: Does this (inject command) have any reason to exist in the modern structure?

    /// <summary>Executes the command.</summary>
    /// <param name="queue">The command queue involved.</param>
    /// <param name="entry">Entry to be executed.</param>
    public static void Execute(CommandQueue queue, CommandEntry entry)
    {
        queue.HandleError(entry, "Inject command is pending rewrite or deletion.");
        /*
        string fname = entry.GetArgument(queue, 0);
        fname = fname.ToLowerFast();
        CommandScript script = queue.Engine.GetFunction(fname);
        if (script == null)
        {
            queue.HandleError(entry, "Cannot inject function '" + TextStyle.Separate + fname + TextStyle.Base + "': it does not exist!");
            return;
        }
        if (entry.ShouldShowGood(queue))
        {
            entry.GoodOutput(queue, "Injecting '" + TextStyle.Separate + fname + TextStyle.Base + "'...");
        }
        CompiledCommandStackEntry cse = queue.CurrentStackEntry;
        CompiledCommandStackEntry tcse = script.Compiled.Duplicate();
        tcse.Debug = cse.Debug; // TODO: Should debug editing be valid?
        queue.CommandStack.Push(tcse);
        */
    }
}
