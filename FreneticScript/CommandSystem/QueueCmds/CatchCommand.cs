using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FreneticScript.CommandSystem.QueueCmds
{
    class CatchCommand: AbstractCommand
    {
        // <--[command]
        // @Name catch
        // @Arguments
        // @Short Contains exception handling code, only allowed to follow a <@link command try>try<@/link> block.
        // @Updated 2016/04/27
        // @Authors mcmonkey
        // @Group Queue
        // @Minimum 0
        // @Maximum 1
        // @Description
        // Contains exception handling code, only allowed to follow a <@link command try>try<@/link> block.
        // TODO: Explain more!
        // @Example
        // // This example catches errors and echoes them to the console.
        // try
        // {
        //     error "Hi there!";
        // }
        // catch
        // {
        //     echo "ERROR: <{[error_message]}>";
        // }
        // @Example
        // // TODO: More examples!
        // @Var stack_trace TextTag The error message that was caught, with full script tracing information.
        // -->
        public CatchCommand()
        {
            Name = "catch";
            Arguments = "";
            Description = "Contains exception handling code, only allowed to follow a try block.";
            IsFlow = true;
            Asyncable = true;
            MinimumArguments = 0;
            MaximumArguments = 1;
            ObjectTypes = new List<Func<TagHandlers.TemplateObject, TagHandlers.TemplateObject>>();
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="queue">The command queue involved.</param>
        /// <param name="entry">Entry to be executed.</param>
        public override void Execute(CommandQueue queue, CommandEntry entry)
        {
            if (entry.Arguments.Count > 0 && entry.Arguments[0].ToString() == "\0CALLBACK")
            {
                if (entry.ShouldShowGood(queue))
                {
                    entry.Good(queue, "Completed catch successfully.");
                }
                return;
            }
            queue.CommandStack.Peek().Index = entry.BlockEnd + 2;
            if (entry.ShouldShowGood(queue))
            {
                entry.Good(queue, "Passing catch without executing.");
            }
        }
    }
}
