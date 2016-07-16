using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Objects;
using FreneticScript.CommandSystem.Arguments;
using System.Reflection;
using System.Reflection.Emit;

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

    /// <summary>
    /// The repeat command.
    /// </summary>
    public class RepeatCommand : AbstractCommand
    {
        /// <summary>
        /// Constructs the repeat command.
        /// </summary>
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
                verify
            };
        }

        TemplateObject verify(TemplateObject input)
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
            string inp = input.ToString().ToLowerFast();
            if (inp == "stop" || inp == "next")
            {
                return new TextTag(inp);
            }
            return null;
        }

        /// <summary>
        /// Represents the "TryRepeatCIL(queue, entry)" method.
        /// </summary>
        public static MethodInfo TryRepeatCILMethod = typeof(RepeatCommand).GetMethod("TryRepeatCIL", new Type[] { typeof(CommandQueue), typeof(CommandEntry) });

        /// <summary>
        /// Represents the "TryRepeatNumberedCIL(queue, entry)" method.
        /// </summary>
        public static MethodInfo TryRepeatNumberedCILMethod = typeof(RepeatCommand).GetMethod("TryRepeatNumberedCIL", new Type[] { typeof(CommandQueue), typeof(CommandEntry) });

        /// <summary>
        /// Adapts a command entry to CIL.
        /// </summary>
        /// <param name="values">The adaptation-relevant values.</param>
        /// <param name="entry">The present entry ID.</param>
        public override void AdaptToCIL(CILAdaptationValues values, int entry)
        {
            CommandEntry cent = values.Entry.Entries[entry];
            string arg = cent.Arguments[0].ToString();
            if (arg == "\0CALLBACK")
            {
                values.PrepareExecutionCall(entry);
                values.ILGen.Emit(OpCodes.Callvirt, TryRepeatCILMethod);
                values.ILGen.Emit(OpCodes.Brfalse, values.Entry.AdaptedILPoints[cent.BlockEnd + 2]);
                values.ILGen.Emit(OpCodes.Br, values.Entry.AdaptedILPoints[cent.BlockStart]);
            }
            else if (arg == "stop")
            {
                for (int i = entry - 1; i >= 0; i--)
                {
                    if (!(values.Entry.Entries[i].Command is RepeatCommand))
                    {
                        continue;
                    }
                    string a0 = values.Entry.Entries[i].Arguments[0].ToString();
                    if (a0 != "stop" && a0 != "next" && a0 != "\0CALLBACK" && values.Entry.Entries[i].InnerCommandBlock != null)
                    {
                        // TODO: Debug output?
                        values.ILGen.Emit(OpCodes.Br, values.Entry.AdaptedILPoints[values.Entry.Entries[i].BlockEnd + 2]);
                        return;
                    }
                }
                throw new Exception("Invalid 'repeat stop' command: not inside a repeat block!");
            }
            else if (arg == "next")
            {
                for (int i = entry - 1; i >= 0; i--)
                {
                    if (!(values.Entry.Entries[i].Command is RepeatCommand))
                    {
                        continue;
                    }
                    string a0 = values.Entry.Entries[i].Arguments[0].ToString();
                    if (a0 != "stop" && a0 != "next" && a0 != "\0CALLBACK" && values.Entry.Entries[i].InnerCommandBlock != null)
                    {
                        // TODO: Debug output?
                        values.ILGen.Emit(OpCodes.Br, values.Entry.AdaptedILPoints[values.Entry.Entries[i].BlockEnd + 1]);
                        return;
                    }
                }
                throw new Exception("Invalid 'repeat next' command: not inside a repeat block!");
            }
            else
            {
                values.PrepareExecutionCall(entry);
                values.ILGen.Emit(OpCodes.Callvirt, TryRepeatNumberedCILMethod);
                values.ILGen.Emit(OpCodes.Brfalse, values.Entry.AdaptedILPoints[cent.BlockEnd + 2]);
            }
        }

        /// <summary>
        /// Executes the callback part of the repeat command.
        /// </summary>
        /// <param name="queue">The command queue involved.</param>
        /// <param name="entry">Entry to be executed.</param>
        public bool TryRepeatCIL(CommandQueue queue, CommandEntry entry)
        {
            CommandStackEntry cse = queue.CommandStack.Peek();
            RepeatCommandData dat = (RepeatCommandData)cse.Entries[entry.BlockStart - 1].GetData(queue);
            dat.Index++;
            queue.SetVariable("repeat_index", new IntegerTag(dat.Index));
            queue.SetVariable("repeat_total", new IntegerTag(dat.Total));
            if (dat.Index <= dat.Total)
            {
                if (entry.ShouldShowGood(queue))
                {
                    entry.Good(queue, "Repeating...: " + dat.Index + "/" + dat.Total);
                }
                cse.Index = entry.BlockStart;
                return true;
            }
            if (entry.ShouldShowGood(queue))
            {
                entry.Good(queue, "Repeat stopping.");
            }
            return false;
        }

        /// <summary>
        /// Executes the numbered input part of the repeat command.
        /// </summary>
        /// <param name="queue">The command queue involved.</param>
        /// <param name="entry">Entry to be executed.</param>
        public bool TryRepeatNumberedCIL(CommandQueue queue, CommandEntry entry)
        {
            int target = (int)IntegerTag.TryFor(entry.GetArgumentObject(queue, 0)).Internal;
            if (target <= 0)
            {
                if (entry.ShouldShowGood(queue))
                {
                    entry.Good(queue, "Not repeating.");
                }
                return false;
            }
            entry.SetData(queue, new RepeatCommandData() { Index = 1, Total = target });
            queue.SetVariable("repeat_index", new IntegerTag(1));
            queue.SetVariable("repeat_total", new IntegerTag(target));
            if (entry.ShouldShowGood(queue))
            {
                entry.Good(queue, "Repeating <{text_color.emphasis}>" + target + "<{text_color.base}> times...");
            }
            return true;
        }
        
        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="queue">The command queue involved.</param>
        /// <param name="entry">Entry to be executed.</param>
        public override void Execute(CommandQueue queue, CommandEntry entry)
        {
            if (entry.Arguments[0].ToString() == "\0CALLBACK")
            {
                TryRepeatCIL(queue, entry);
                return;
            }
            string clow = entry.Arguments[0].ToString().ToLowerFast();
            if (clow == "stop")
            {
                // TODO: ???
                CommandStackEntry cse = queue.CommandStack.Peek();
                for (int i = 0; i < cse.Entries.Length; i++)
                {
                    if (queue.GetCommand(i).Command is RepeatCommand && queue.GetCommand(i).Arguments[0].ToString() == "\0CALLBACK")
                    {
                        if (entry.ShouldShowGood(queue))
                        {
                            entry.Good(queue, "Stopping a repeat loop.");
                        }
                        cse.Index = i + 2;
                        return;
                    }
                }
                queue.HandleError(entry, "Cannot stop repeat: not in one!");
                return;
            }
            else if (clow == "next")
            {
                // TODO: ???
                CommandStackEntry cse = queue.CommandStack.Peek();
                for (int i = cse.Index - 1; i > 0; i--)
                {
                    if (queue.GetCommand(i).Command is RepeatCommand && queue.GetCommand(i).Arguments[0].ToString() == "\0CALLBACK")
                    {
                        if (entry.ShouldShowGood(queue))
                        {
                            entry.Good(queue, "Jumping forward in a repeat loop.");
                        }
                        cse.Index = i + 1;
                        return;
                    }
                }
                queue.HandleError(entry, "Cannot advance repeat: not in one!");
                return;
            }
            if (!TryRepeatNumberedCIL(queue, entry))
            {
                CommandStackEntry cse = queue.CommandStack.Peek();
                cse.Index = entry.BlockEnd + 1;
            }
        }
    }
}
