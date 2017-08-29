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
            ObjectTypes = new List<Func<TemplateObject, TemplateObject>>()
            {
                TextTag.For,
                BooleanTag.TryFor
            };
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="queue">The command queue involved.</param>
        /// <param name="entry">Entry to be executed.</param>
        public static void Execute(CommandQueue queue, CommandEntry entry)
        {
            if (entry.Arguments.Count > 1 && BooleanTag.TryFor(entry.GetArgumentObject(queue, 1)).Internal)
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
