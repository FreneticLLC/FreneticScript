using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Frenetic.TagHandlers;

namespace Frenetic.CommandSystem.QueueCmds
{
    // <--[command]
    // @Name break
    // @Arguments [number of layers to break]
    // @Short Breaks out of a specified number of braced layers.
    // @Updated 2014/06/24
    // @Authors mcmonkey
    // @Group Queue
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
        }

        public static int StringToInt(string input)
        {
            int output = 0;
            int.TryParse(input, out output);
            return output;
        }

        public override void Execute(CommandEntry entry)
        {
            int count = 1;
            if (entry.Arguments.Count > 0)
            {
                count = StringToInt(entry.GetArgument(0));
            }
            if (count <= 0)
            {
                ShowUsage(entry);
                return;
            }
            CommandEntry Owner = entry.BlockOwner;
            while (entry.Queue.CommandList.Length > 0 && count > 0)
            {
                if (Owner == null)
                {
                    entry.Bad("Tried to break <{text_color.emphasis}>" + count +
                        "<{text_color.base}> more brace" + (count == 1 ? "" : "s") + " than there are!");
                    return;
                }
                CommandEntry compOwner = entry.Queue.GetCommand(0).BlockOwner;
                if (compOwner == Owner)
                {
                    entry.Queue.RemoveCommand(0);
                }
                else
                {
                    Owner = compOwner;
                    count--;
                }
            }
            count--;
            if (count > 0)
            {
                entry.Bad("Tried to break <{text_color.emphasis}>" + count +
                    "<{text_color.base}> more brace" + (count == 1 ? "": "s") + " than there are!");
            }
            else
            {
                entry.Good("Broke through all layers.");
            }
        }
    }
}
