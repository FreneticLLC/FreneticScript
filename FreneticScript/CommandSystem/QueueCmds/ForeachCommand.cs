using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers.Objects;
using FreneticScript.TagHandlers;
using FreneticScript.CommandSystem.Arguments;

namespace FreneticScript.CommandSystem.QueueCmds
{
    // <--[command]
    // @Name foreach
    // @Arguments 'start'/'stop'/'next' [list to loop through]
    // @Short Executes the following block of commands once foreach item in the given list.
    // @Updated 2014/06/23
    // @Authors mcmonkey
    // @Group Queue
    // @Minimum 1
    // @Maximum 2
    // @Braces allowed
    // @Description
    // The foreach command will loop through the given list and run the included command block
    // once for each entry in the list.
    // It can also be used to stop the looping via the 'stop' argument, or to jump to the next
    // entry in the list and restart the command block via the 'next' argument.
    // TODO: Explain more!
    // @Example
    // // This example runs through the list and echos "one", then "two", then "three" back to the console.
    // foreach start one|two|three
    // {
    //     echo "<{var[foreach_value]}>"
    // }
    // @Example
    // // This example runs through the list and echos "one", then "oner", then "two", then "three", then "threer" back to the console.
    // foreach start one|two|three
    // {
    //     echo "<{var[foreach_value]}>"
    //     if <{var[foreach_value].equals[two]}>
    //     {
    //         foreach next
    //     }
    //     echo "<{var[foreach_value]}>r"
    // }
    // @Example
    // // This example runs through the list and echos "one", then "two", then stops early back to the console.
    // foreach start one|two|three
    // {
    //     echo "<{var[foreach_value]}>"
    //     if <{var[foreach_value].equals[three]}>
    //     {
    //         foreach stop
    //     }
    // }
    // @Example
    // TODO: More examples!
    // @Var foreach_index TextTag returns what iteration (numeric) the foreach is on.
    // @Var foreach_total TextTag returns what iteration (numeric) the foreach is aiming for, and will end on if not stopped early.
    // @Var foreach_value Dynamic returns the current item in the list.
    // @Var foreach_list ListTag returns the full list being looped through.
    // -->
    class ForeachCommandData : AbstractCommandEntryData
    {
        public List<TemplateObject> List;
        public int Index;
    }

    class ForeachCommand : AbstractCommand
    {
        public ForeachCommand()
        {
            Name = "foreach";
            Arguments = "'start'/'stop'/'next' [list to loop through]";
            Description = "Executes the following block of commands once foreach item in the given list.";
            IsFlow = true;
            Asyncable = true;
            MinimumArguments = 1;
            MaximumArguments = 2;
            IsBreakable = true;
            ObjectTypes = new List<Func<TemplateObject, TemplateObject>>()
            {
                (input) =>
                {
                    if (input.ToString() == "\0CALLBACK")
                    {
                        return input;
                    }
                    string inp = input.ToString().ToLowerFast();
                    if (inp == "start" || inp == "stop" || inp == "next")
                    {
                        return new TextTag(input.ToString());
                    }
                    return null;
                },
                (input) =>
                {
                    return ListTag.For(input);
                }
            };
        }

        public override void Execute(CommandEntry entry)
        {
            string type = entry.GetArgument(0);
            if (type == "\0CALLBACK")
            {
                ForeachCommandData dat = (ForeachCommandData)entry.Queue.CommandList[entry.BlockStart - 1].Data;
                dat.Index++;
                if (dat.Index <= dat.List.Count)
                {
                    if (entry.ShouldShowGood())
                    {
                        entry.Good("Foreach looping...: " + dat.Index + "/" + dat.List.Count);
                    }
                    entry.Queue.CommandIndex = entry.BlockStart;
                    return;
                }
                if (entry.ShouldShowGood())
                {
                    entry.Good("Foreach stopping.");
                }
            }
            else if (type.ToLowerFast() == "stop")
            {
                for (int i = 0; i < entry.Queue.CommandList.Length; i++)
                {
                    if (entry.Queue.GetCommand(i).Command is ForeachCommand && entry.Queue.GetCommand(i).Arguments[0].ToString() == "\0CALLBACK")
                    {
                        if (entry.ShouldShowGood())
                        {
                            entry.Good("Stopping a foreach loop.");
                        }
                        entry.Queue.CommandIndex = i + 2;
                        return;
                    }
                }
                entry.Error("Cannot stop foreach: not in one!");
            }
            else if (type.ToLowerFast() == "next")
            {
                for (int i = entry.Queue.CommandIndex - 1; i > 0; i--)
                {
                    if (entry.Queue.GetCommand(i).Command is ForeachCommand && entry.Queue.GetCommand(i).Arguments[0].ToString() == "\0CALLBACK")
                    {
                        if (entry.ShouldShowGood())
                        {
                            entry.Good("Jumping forward in a foreach loop.");
                        }
                        entry.Queue.CommandIndex = i + 1;
                        return;
                    }
                }
                entry.Error("Cannot advance foreach: not in one!");
            }
            else if (type.ToLowerFast() == "start" && entry.Arguments.Count > 1)
            {
                ListTag list = ListTag.For(entry.GetArgument(1));
                int target = list.ListEntries.Count;
                if (target <= 0)
                {
                    entry.Good("Not looping.");
                    entry.Queue.CommandIndex = entry.BlockEnd + 2;
                    return;
                }
                entry.Data = new ForeachCommandData() { Index = 1, List = list.ListEntries };
                if (entry.ShouldShowGood())
                {
                    entry.Good("Foreach looping <{text_color.emphasis}>" + target + "<{text_color.base}> times...");
                }
            }
            else
            {
                ShowUsage(entry);
            }
        }
    }
}
