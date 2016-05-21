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
    // @Block Allowed
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
    // @Var foreach_value <Dynamic> returns the current item in the list.
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
                verify,
                ListTag.For
            };
        }

        TemplateObject verify(TemplateObject input)
        {
            if (input.ToString() == "\0CALLBACK")
            {
                return input;
            }
            string inp = input.ToString().ToLowerFast();
            if (inp == "start" || inp == "stop" || inp == "next")
            {
                return new TextTag(inp);
            }
            return null;
        }

        public override void Execute(CommandQueue queue, CommandEntry entry)
        {
            string type = entry.GetArgument(queue, 0);
            if (type == "\0CALLBACK")
            {
                CommandStackEntry cse = queue.CommandStack.Peek();
                ForeachCommandData dat = (ForeachCommandData)cse.Entries[entry.BlockStart - 1].GetData(queue);
                dat.Index++;
                queue.SetVariable("foreach_total", new IntegerTag(dat.List.Count));
                queue.SetVariable("foreach_index", new IntegerTag(dat.Index));
                queue.SetVariable("foreach_list", new ListTag(dat.List));
                if (dat.Index <= dat.List.Count)
                {
                    queue.SetVariable("foreach_value", dat.List[dat.Index - 1]);
                    if (entry.ShouldShowGood(queue))
                    {
                        entry.Good(queue, "Foreach looping...: " + dat.Index + "/" + dat.List.Count);
                    }
                    cse.Index = entry.BlockStart;
                    return;
                }
                if (entry.ShouldShowGood(queue))
                {
                    entry.Good(queue, "Foreach stopping.");
                }
            }
            else if (type.ToLowerFast() == "stop")
            {
                CommandStackEntry cse = queue.CommandStack.Peek();
                for (int i = 0; i < cse.Entries.Length; i++)
                {
                    if (queue.GetCommand(i).Command is ForeachCommand && queue.GetCommand(i).Arguments[0].ToString() == "\0CALLBACK")
                    {
                        if (entry.ShouldShowGood(queue))
                        {
                            entry.Good(queue, "Stopping a foreach loop.");
                        }
                        cse.Index = i + 2;
                        return;
                    }
                }
                queue.HandleError(entry, "Cannot stop foreach: not in one!");
            }
            else if (type.ToLowerFast() == "next")
            {
                CommandStackEntry cse = queue.CommandStack.Peek();
                for (int i = cse.Index - 1; i > 0; i--)
                {
                    if (queue.GetCommand(i).Command is ForeachCommand && queue.GetCommand(i).Arguments[0].ToString() == "\0CALLBACK")
                    {
                        if (entry.ShouldShowGood(queue))
                        {
                            entry.Good(queue, "Jumping forward in a foreach loop.");
                        }
                        cse.Index = i + 1;
                        return;
                    }
                }
                queue.HandleError(entry, "Cannot advance foreach: not in one!");
            }
            else if (type.ToLowerFast() == "start" && entry.Arguments.Count > 1)
            {
                ListTag list = ListTag.For(entry.GetArgument(queue, 1));
                int target = list.ListEntries.Count;
                if (target <= 0)
                {
                    entry.Good(queue, "Not looping.");
                    CommandStackEntry cse = queue.CommandStack.Peek();
                    cse.Index = entry.BlockEnd + 2;
                    return;
                }
                entry.SetData(queue, new ForeachCommandData() { Index = 1, List = list.ListEntries });
                queue.SetVariable("foreach_index", new IntegerTag(1));
                queue.SetVariable("foreach_total", new IntegerTag(target));
                queue.SetVariable("foreach_value", list.ListEntries[0]);
                queue.SetVariable("foreach_list", list);
                if (entry.ShouldShowGood(queue))
                {
                    entry.Good(queue, "Foreach looping <{text_color.emphasis}>" + target + "<{text_color.base}> times...");
                }
            }
            else
            {
                ShowUsage(queue, entry);
            }
        }
    }
}
