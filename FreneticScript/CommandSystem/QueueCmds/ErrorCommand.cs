//
// This file is part of FreneticScript, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of the MIT license.
// See README.md or LICENSE.txt in the FreneticScript source root for the contents of the license.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.CommandSystem;
using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Objects;

namespace FreneticScript.CommandSystem.QueueCmds
{
    /// <summary>
    /// The Error command.
    /// </summary>
    public class ErrorCommand : AbstractCommand
    {
        // <--[command]
        // @Name error
        // @Arguments <error message>
        // @Short Throws an error on the current command queue.
        // @Updated 2016/04/28
        // @Authors mcmonkey
        // @Group Queue
        // @Minimum 1
        // @Maximum 1
        // @Description
        // Throws an error on the current command queue.
        // TODO: Explain more!
        // @Example
        // // This example throws the error "RIP".
        // error "RIP";
        // @Example
        // // TODO: More examples!
        // -->

        /// <summary>
        /// Constructs the error command.
        /// </summary>
        public ErrorCommand()
        {
            Name = "error";
            Arguments = "<error message> [exception]";
            Description = "Throws an error on the current command queue.";
            IsFlow = true;
            Asyncable = true;
            MinimumArguments = 1;
            MaximumArguments = 2;
            ObjectTypes = new Action<ArgumentValidation>[]
            {
                TextTag.Validator,
                BooleanTag.Validator
            };
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="queue">The command queue involved.</param>
        /// <param name="entry">Entry to be executed.</param>
        public static void Execute(CommandQueue queue, CommandEntry entry)
        {
            if (entry.Arguments.Length > 1 && BooleanTag.TryFor(entry.GetArgumentObject(queue, 1)).Internal)
            {
                throw new Exception("FreneticScript induced exception: '" + entry.GetArgument(queue, 0) + "'");
            }
            else
            {
                queue.HandleError(entry, entry.GetArgument(queue, 0));
            }
        }
    }
}
