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
    //     break
    //     echo "This will never show"
    // }
    // @Example
    // // This example breaks the if layers it is in, without running the following echos.
    // if true
    // {
    //     if true
    //     {
    //         break 2
    //         echo "This will never show"
    //     }
    //     echo "This also will never show"
    // }
    // @Example
    // TODO: More examples!
    // -->
    class BreakCommand: AbstractCommand
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
                (input) =>
                {
                    return NumberTag.TryFor(input);
                }
            };
        }
        
        public override void Execute(CommandEntry entry)
        {
            int count = 1;
            if (entry.Arguments.Count > 0)
            {
                IntegerTag inter = IntegerTag.TryFor(entry.GetArgumentObject(0));
                if (inter != null)
                {
                    count = (int)inter.Internal;
                }
            }
            if (count <= 0)
            {
                ShowUsage(entry);
                return;
            }
            for (int i = 0; i < count; i++)
            {
                for (int ind = entry.Queue.CommandIndex; ind < entry.Queue.CommandList.Length; ind++)
                {
                    CommandEntry tentry = entry.Queue.CommandList[ind];
                    if (tentry.Command.IsBreakable && tentry.Arguments[0].ToString() == "\0CALLBACK")
                    {
                        entry.Queue.CommandIndex = ind + 1;
                        goto completed;
                    }
                }
                entry.Error("Not in that many blocks!");
                return;
                completed:
                continue;
            }
            entry.Good("Broke free successfully.");
        }
    }
}
