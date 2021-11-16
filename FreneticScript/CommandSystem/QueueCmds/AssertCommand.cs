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

namespace FreneticScript.CommandSystem.QueueCmds
{
    /// <summary>The Assert command.</summary>
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

        /// <summary>Constructs the assert command.</summary>
        public AssertCommand()
        {
            Name = "assert";
            Arguments = "<requirement> <error message>";
            Description = "Throws an error if a requirement is not 'true'.";
            IsFlow = true;
            MinimumArguments = 2;
            MaximumArguments = 2;
            ObjectTypes = new Action<ArgumentValidation>[]
            {
                BooleanTag.Validator,
                TextTag.Validator
            };
        }

        /// <summary>Executes the command.</summary>
        /// <param name="queue">The command queue involved.</param>
        /// <param name="entry">Entry to be executed.</param>
        public static void Execute(CommandQueue queue, CommandEntry entry)
        {
            TemplateObject arg1 = entry.GetArgumentObject(queue, 0);
            BooleanTag bt = BooleanTag.TryFor(arg1);
            if (bt == null || !bt.Internal)
            {
                queue.HandleError(entry, "Assertion failed: " + entry.GetArgument(queue, 1));
                return;
            }
            entry.GoodOutput(queue, "Assert command passed, assertion valid!");
        }
    }
}
