using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Objects;
using FreneticScript.CommandSystem.Arguments;

namespace FreneticScript.CommandSystem.QueueCmds
{
    // <--[command]
    // @Name repeat
    // @Arguments <times to repeat>/'stop'/'next'
    // @Short Executes the following block of commands a specified number of times.
    // @Updated 2014/06/23
    // @Authors mcmonkey
    // @Group Queue
    // @Minimum 1
    // @Maximum 1
    // @Braces allowed
    // @Description
    // The repeat command will loop the given number of times and execute the included command block
    // each time it loops.
    // It can also be used to stop the looping via the 'stop' argument, or to jump to the next
    // entry in the list and restart the command block via the 'next' argument.
    // TODO: Explain more!
    // @Example
    // // This example runs through the list and echos "1/3", then "2/3", then "3/3" back to the console.
    // repeat 3
    // {
    //     echo "<{var[repeat_index]}>/<{var[repeat_total]}>"
    // }
    // @Example
    // // This example runs through the list and echos "1", then "1r", then "2", then "3", then "3r" back to the console.
    // repeat 3
    // {
    //     echo "<{var[repeat_index]}>"
    //     if <{var[repeat_index].equals[2]}>
    //     {
    //         repeat next
    //     }
    //     echo "<{var[repeat_index]}>r"
    // }
    // @Example
    // // This example runs through the list and echos "1", then "2", then stops early.
    // repeat 3
    // {
    //     if <{var[repeat_index].equals[3]}>
    //     {
    //         repeat stop
    //     }
    //     echo "<{var[repeat_index]}>"
    // }
    // @Example
    // // TODO: More examples!
    // @Var repeat_index IntegerTag returns what iteration (numeric) the repeat is on.
    // @Var repeat_total IntegerTag returns what iteration (numeric) the repeat is aiming for, and will end on if not stopped early.
    // -->
    class RepeatCommandData : AbstractCommandEntryData
    {
        public int Index;
        public int Total;
    }

    class RepeatCommand : AbstractCommand
    {
        public RepeatCommand()
        {
            Name = "repeat";
            Arguments = "<times to repeat>/'stop'/'next'";
            Description = "Executes the following block of commands a specified number of times.";
            IsFlow = true;
            Asyncable = true;
            MinimumArguments = 1;
            MaximumArguments = 1;
            IsBreakable = true;
            ObjectTypes = new List<Func<TemplateObject, TemplateObject>>()
            {
                (input) =>
                {
                    if (input.ToString() == "\0CALLBACK")
                    {
                        return input;
                    }
                    int rep;
                    if (int.TryParse(input.ToString(), out rep))
                    {
                        return new IntegerTag(rep);
                    }
                    return new TextTag(input.ToString());
                }
            };
        }
        
        public override void Execute(CommandEntry entry)
        {
            string count = entry.GetArgument(0);
            if (count == "\0CALLBACK")
            {
                CommandStackEntry cse = entry.Queue.CommandStack.Peek();
                RepeatCommandData dat = (RepeatCommandData)cse.Entries[entry.BlockStart - 1].Data;
                dat.Index++;
                entry.Queue.SetVariable("repeat_index", new IntegerTag(dat.Index));
                entry.Queue.SetVariable("repeat_total", new IntegerTag(dat.Total));
                if (dat.Index <= dat.Total)
                {
                    if (entry.ShouldShowGood())
                    {
                        entry.Good("Repeating...: " + dat.Index + "/" + dat.Total);
                    }
                    cse.Index = entry.BlockStart;
                    return;
                }
                if (entry.ShouldShowGood())
                {
                    entry.Good("Repeat stopping.");
                }
            }
            else if (count.ToLowerFast() == "stop")
            {
                CommandStackEntry cse = entry.Queue.CommandStack.Peek();
                for (int i = 0; i < cse.Entries.Length; i++)
                {
                    if (entry.Queue.GetCommand(i).Command is RepeatCommand && entry.Queue.GetCommand(i).Arguments[0].ToString() == "\0CALLBACK")
                    {
                        if (entry.ShouldShowGood())
                        {
                            entry.Good("Stopping a repeat loop.");
                        }
                        cse.Index = i + 2;
                        return;
                    }
                }
                entry.Error("Cannot stop repeat: not in one!");
            }
            else if (count.ToLowerFast() == "next")
            {
                CommandStackEntry cse = entry.Queue.CommandStack.Peek();
                for (int i = cse.Index - 1; i > 0; i--)
                {
                    if (entry.Queue.GetCommand(i).Command is RepeatCommand && entry.Queue.GetCommand(i).Arguments[0].ToString() == "\0CALLBACK")
                    {
                        if (entry.ShouldShowGood())
                        {
                            entry.Good("Jumping forward in a repeat loop.");
                        }
                        cse.Index = i + 1;
                        return;
                    }
                }
                entry.Error("Cannot advance repeat: not in one!");
            }
            else
            {
                int target = (int)IntegerTag.TryFor(count).Internal; // TODO: Maybe a null check?
                if (target <= 0)
                {
                    if (entry.ShouldShowGood())
                    {
                        entry.Good("Not repeating.");
                    }
                    CommandStackEntry cse = entry.Queue.CommandStack.Peek();
                    cse.Index = entry.BlockEnd + 1;
                    return;
                }
                entry.Data = new RepeatCommandData() { Index = 1, Total = target };
                entry.Queue.SetVariable("repeat_index", new IntegerTag(1));
                entry.Queue.SetVariable("repeat_total", new IntegerTag(target));
                if (entry.ShouldShowGood())
                {
                    entry.Good("Repeating <{text_color.emphasis}>" + target + "<{text_color.base}> times...");
                }
            }
        }
    }
}
