using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Objects;

namespace FreneticScript.CommandSystem.QueueCmds
{
    // <--[command]
    // @Name break
    // @Arguments [number of layers to break]
    // @Short Breaks out of a specified number of braced layers.
    // @Updated 2014/06/24
    // @Authors mcmonkey
    // @Group Queue
    // @Minimum 0
    // @Maximum 1
    // @Description
    // Whenever <@link command foreach>foreach stop<@/link> or <@link command function>function stop<@/link> or
    // whatever specific stop command isn't enough, the break command is available to smash through as many
    // braced layers as required, all at once. It handles any braced chunks, including <@link command if>if<@/link>
    // chunks.
    // TODO: Explain more!
    // @Example
    // // This example breaks the if layer it is in, without running the following echo.
    // if true
    // {
    //     break;
    //     echo "This will never show";
    // }
    // @Example
    // // This example breaks the if layers it is in, without running the following echos.
    // if true
    // {
    //     if true
    //     {
    //         break 2;
    //         echo "This will never show";
    //     }
    //     echo "This also will never show";
    // }
    // @Example
    // TODO: More examples!
    // -->
    public class BreakCommand : AbstractCommand
    {
        public BreakCommand()
        {
            Name = "break";
            Arguments = "[number of layers to break]";
            Description = "Breaks out of a specified number of braced layers.";
            IsFlow = true;
            Asyncable = true;
            MinimumArguments = 0;
            MaximumArguments = 1;
            ObjectTypes = new List<Func<TemplateObject, TemplateObject>>()
            {
                NumberTag.TryFor
            };
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="queue">The command queue involved.</param>
        /// <param name="entry">Entry to be executed.</param>
        public static void Execute(CommandQueue queue, CommandEntry entry)
        {
            int count = 1;
            if (entry.Arguments.Count > 0)
            {
                IntegerTag inter = IntegerTag.TryFor(entry.GetArgumentObject(queue, 0));
                if (inter != null)
                {
                    count = (int)inter.Internal;
                }
            }
            if (count <= 0)
            {
                ShowUsage(queue, entry);
                return;
            }
            for (int i = 0; i < count; i++)
            {
                CommandStackEntry cse = queue.CurrentEntry;
                for (int ind = cse.Index; ind < cse.Entries.Length; ind++)
                {
                    CommandEntry tentry = cse.Entries[ind];
                    if (tentry.Command.IsBreakable && tentry.Arguments[0].ToString() == "\0CALLBACK")
                    {
                        cse.Index = ind + 1;
                        goto completed;
                    }
                }
                if (queue.CommandStack.Count > 1)
                {
                    queue.CommandStack.Pop();
                }
                else
                {
                    queue.HandleError(entry, "Not in that many blocks!");
                    return;
                }
                completed:
                continue;
            }
            entry.Good(queue, "Broke free successfully.");
        }
    }
}
