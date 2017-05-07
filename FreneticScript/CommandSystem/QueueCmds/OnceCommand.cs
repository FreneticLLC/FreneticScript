using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Objects;

namespace FreneticScript.CommandSystem.QueueCmds
{
    /// <summary>
    /// The Once command.
    /// </summary>
    public class OnceCommand : AbstractCommand
    {
        // <--[command]
        // @Name once
        // @Arguments <identifer> ['error'/'warning'/'quiet']
        // @Short Runs a block precisely once per reload.
        // @Updated 2016/04/27
        // @Authors mcmonkey
        // @Group Queue
        // @Block Always
        // @Minimum 2
        // @Maximum 2
        // @Description
        // Runs a block precisely once per reload.
        // Optionally specify how to react when ran more than once: with an error, with a warning, or just quietly not running it again.
        // Default reaction is error.
        // TODO: Explain more!
        // @Example
        // // This example runs once.
        // once MyScript
        // {
        //     echo "Hi!";
        // }
        // @Example
        // // This example throws an error.
        // once MyScript { echo "hi!"; }
        // once MyScript { echo "This won't show!"; }
        // @Example
        // // This example echos "hi!" once.
        // once MyScript { echo "hi!"; }
        // once MyScript quiet { echo "This won't show!"; }
        // -->

        /// <summary>
        /// Constructs the once command.
        /// </summary>
        public OnceCommand()
        {
            Name = "once";
            Arguments = "<identifer> ['error'/'warning'/'quiet']";
            Description = "Runs a block precisely once per reload.";
            IsFlow = true;
            MinimumArguments = 1;
            MaximumArguments = 2;
            ObjectTypes = new List<Func<TemplateObject, TemplateObject>>()
            {
                Lower,
                TestValidity
            };
        }

        TemplateObject Lower(TemplateObject input)
        {
            string val = input.ToString();
            if (input.ToString() == "\0CALLBACK")
            {
                return input;
            }
            return new TextTag(val.ToLowerFast());
        }

        TemplateObject TestValidity(TemplateObject input)
        {
            string val = input.ToString();
            string low = val.ToLowerFast();
            if (low == "error" || low == "warning" || low == "quiet")
            {
                return new TextTag(low);
            }
            return null;
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="queue">The command queue involved.</param>
        /// <param name="entry">Entry to be executed.</param>
        public static void Execute(CommandQueue queue, CommandEntry entry)
        {
            if (entry.Arguments[0].ToString() == "\0CALLBACK")
            {
                return;
            }
            string id = entry.GetArgument(queue, 0).ToLowerFast();
            if (queue.CommandSystem.OnceBlocks.Add(id))
            {
                if (entry.ShouldShowGood(queue))
                {
                    entry.Good(queue, "Once block has not yet ran, continuing.");
                }
                return;
            }
            string errorMode = entry.Arguments.Count > 1 ? entry.GetArgument(queue, 1).ToLowerFast() : "error";
            if (errorMode == "quiet")
            {
                if (entry.ShouldShowGood(queue))
                {
                    entry.Good(queue, "Once block repeated, ignoring: " + TagParser.Escape(id));
                }
                queue.CurrentEntry.Index = entry.BlockEnd + 1;
            }
            else if (errorMode == "warning")
            {
                entry.Bad(queue, "Once block repeated: " + TagParser.Escape(id));
                queue.CurrentEntry.Index = entry.BlockEnd + 1;
            }
            else
            {
                queue.HandleError(entry, "Once block repeated: " + id);
            }
        }
    }
}
