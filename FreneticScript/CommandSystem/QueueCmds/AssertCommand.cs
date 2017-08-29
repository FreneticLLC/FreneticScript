//
// This file is created by Frenetic LLC.
// This code is Copyright (C) 2016-2017 Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using System;
using System.Collections.Generic;
using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Objects;

namespace FreneticScript.CommandSystem.QueueCmds
{
    /// <summary>
    /// The Assert command.
    /// </summary>
    public class AssertCommand : AbstractCommand
    {
        // <--[command]
        // @Name assert
        // @Arguments <requirement> <error message>
        // @Short Throws an error if a requirement is not 'true'.
        // @Updated 2016/04/27
        // @Authors mcmonkey
        // @Group Queue
        // @Minimum 2
        // @Maximum 2
        // @Description
        // Throws an error if a requirement is not 'true'.
        // Effectively equivalent to: "if !<requirement> { error <error message>; }".
        // TODO: Explain more!
        // @Example
        // // This example throws an error immediately.
        // assert false "Bad!";
        // @Example
        // // This example never throws an error.
        // assert true "Good!";
        // @Example
        // // This example sometimes throws an error.
        // assert <{util.random_decimal.is_greater_than[0.5]}> "Randomness is deadly!";
        // -->

        /// <summary>
        /// Constructs the assert command.
        /// </summary>
        public AssertCommand()
        {
            Name = "assert";
            Arguments = "<requirement> <error message>";
            Description = "Throws an error if a requirement is not 'true'.";
            IsFlow = true;
            MinimumArguments = 2;
            MaximumArguments = 2;
            ObjectTypes = new List<Func<TemplateObject, TemplateObject>>()
            {
                BooleanTag.TryFor,
                TextTag.For
            };
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="queue">The command queue involved.</param>
        /// <param name="entry">Entry to be executed.</param>
        public static void Execute(CommandQueue queue, CommandEntry entry)
        {
            TemplateObject arg1 = entry.GetArgumentObject(queue, 0);
            BooleanTag bt = BooleanTag.TryFor(arg1);
            if (bt == null || !bt.Internal)
            {
                queue.HandleError(entry, "Assertion failed: " + TagParser.Escape(entry.GetArgument(queue, 1)));
                return;
            }
            entry.Good(queue, "Assert command passed, assertion valid!");
        }
    }
}
