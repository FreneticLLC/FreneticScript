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

namespace FreneticScript.CommandSystem.QueueCmds
{
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

    /// <summary>
    /// The Inject command.
    /// </summary>
    public class InjectCommand : AbstractCommand
    {
        /// <summary>
        /// Constructs the inject command.
        /// </summary>
        public InjectCommand()
        {
            Name = "inject";
            Arguments = "<function to call>";
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

        // TODO: Does this (inject command) have any reason to exist in the modern structure?

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
                queue.HandleError(entry, "Cannot inject function '<{text_color[emphasis]}>" + TagParser.Escape(fname) + "<{text_color[base]}>': it does not exist!");
                return;
            }
            if (entry.ShouldShowGood(queue))
            {
                entry.Good(queue, "Injecting '<{text_color[emphasis]}>" + TagParser.Escape(fname) + "<{text_color[base]}>'...");
            }
            CompiledCommandStackEntry cse = queue.CurrentEntry;
            CompiledCommandStackEntry tcse = script.Created.Duplicate();
            tcse.Debug = cse.Debug; // TODO: Should debug editing be valid?
            queue.CommandStack.Push(tcse);
        }
    }
}
