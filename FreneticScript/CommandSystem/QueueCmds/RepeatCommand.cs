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
    //     echo "<{var[repeat_index]}>/<{var[repeat_total]}>";
    // }
    // @Example
    // // This example runs through the list and echos "1", then "1r", then "2", then "3", then "3r" back to the console.
    // repeat 3
    // {
    //     echo "<{var[repeat_index]}>";
    //     if <{var[repeat_index].equals[2]}>
    //     {
    //         repeat next;
    //     }
    //     echo "<{var[repeat_index]}>r";
    // }
    // @Example
    // // This example runs through the list and echos "1", then "2", then stops early.
    // repeat 3
    // {
    //     if <{var[repeat_index].equals[3]}>
    //     {
    //         repeat stop;
    //     }
    //     echo "<{var[repeat_index]}>";
    // }
    // @Example
    // // TODO: More examples!
    // @Save repeat_index IntegerTag returns what iteration (numeric) the repeat is on.
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
        public static MethodInfo TryRepeatCILMethod = typeof(RepeatCommand).GetMethod("TryRepeatCIL");

        /// <summary>
        /// Represents the "TryRepeatCILNoDebug(queue, entry)" method.
        /// </summary>
        public static MethodInfo TryRepeatCILMethodNoDebug = typeof(RepeatCommand).GetMethod("TryRepeatCILNoDebug");

        /// <summary>
        /// Represents the "TryRepeatNumberedCIL(queue, entry)" method.
        /// </summary>
        public static MethodInfo TryRepeatNumberedCILMethod = typeof(RepeatCommand).GetMethod("TryRepeatNumberedCIL");

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
                string sn = values.Entry.Entries[cent.BlockStart - 1].GetSaveNameNoParse("repeat_index");
                string sn_tot = values.Entry.Entries[cent.BlockStart - 1].GetSaveNameNoParse("repeat_total", "save_total");
                int lvar_ind_loc = cent.VarLoc(sn);
                int lvar_tot_loc = cent.VarLoc(sn_tot);
                values.LoadQueue();
                values.LoadEntry(entry);
                values.ILGen.Emit(OpCodes.Ldc_I4, lvar_ind_loc);
                values.ILGen.Emit(OpCodes.Ldc_I4, lvar_tot_loc);
                values.ILGen.Emit(OpCodes.Call, values.Entry.Debug <= DebugMode.FULL ? TryRepeatCILMethod : TryRepeatCILMethodNoDebug);
                values.ILGen.Emit(OpCodes.Brtrue, values.Entry.AdaptedILPoints[cent.BlockStart]);
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
                int lvar_ind_loc = cent.VarLoc(cent.GetSaveNameNoParse("repeat_index"));
                int lvar_tot_loc = cent.VarLoc(cent.GetSaveNameNoParse("repeat_total", "save_total"));
                values.LoadQueue();
                values.LoadEntry(entry);
                values.ILGen.Emit(OpCodes.Ldc_I4, lvar_ind_loc);
                values.ILGen.Emit(OpCodes.Ldc_I4, lvar_tot_loc);
                values.ILGen.Emit(OpCodes.Call, TryRepeatNumberedCILMethod);
                values.ILGen.Emit(OpCodes.Brfalse, values.Entry.AdaptedILPoints[cent.BlockEnd + 2]);
            }
        }

        /// <summary>
        /// Prepares to adapt a command entry to CIL.
        /// </summary>
        /// <param name="values">The adaptation-relevant values.</param>
        /// <param name="entry">The present entry ID.</param>
        public override void PreAdaptToCIL(CILAdaptationValues values, int entry)
        {
            CommandEntry cent = values.Entry.Entries[entry];
            string arg = cent.Arguments[0].ToString();
            if (arg == "\0CALLBACK")
            {
                values.PopVarSet();
                return;
            }
            if (arg == "next" || arg == "stop")
            {
                return;
            }
            values.PushVarSet();
            string sn = cent.GetSaveNameNoParse("repeat_index");
            string sn_tot = cent.GetSaveNameNoParse("repeat_total", "save_total");
            // TODO: scope properly!
            if (values.LocalVariableLocation(sn) >= 0)
            {
                throw new Exception("On script line " + cent.ScriptLine + " (" + cent.CommandLine + "), error occured: Already have a repeat_index var (labeled '" + sn + "')?!");
            }
            if (values.LocalVariableLocation(sn_tot) >= 0)
            {
                throw new Exception("On script line " + cent.ScriptLine + " (" + cent.CommandLine + "), error occured: Already have a repeat_total var (labeled '" + sn_tot + "')?!");
            }
            TagType type = cent.System.TagSystem.Type_Integer;
            values.AddVariable(sn, type);
            values.AddVariable(sn_tot, type);
        }

        /// <summary>
        /// Executes the callback part of the repeat command, without debug output.
        /// </summary>
        /// <param name="queue">The command queue involved.</param>
        /// <param name="entry">Entry to be executed.</param>
        /// <param name="ri">Repeat Index location.</param>
        /// <param name="rt">Repeat Total Location.</param>
        public static bool TryRepeatCILNoDebug(CommandQueue queue, CommandEntry entry, int ri, int rt)
        {
            CompiledCommandStackEntry cse = queue.CurrentEntry;
            RepeatCommandData dat = cse.EntryData[entry.BlockStart - 1] as RepeatCommandData;
            dat.Index++;
            (cse.LocalVariables[ri].Internal as IntegerTag).Internal = dat.Index;
            return dat.Index <= dat.Total;
        }
        /// <summary>
        /// Executes the callback part of the repeat command.
        /// </summary>
        /// <param name="queue">The command queue involved.</param>
        /// <param name="entry">Entry to be executed.</param>
        /// <param name="ri">Repeat Index location.</param>
        /// <param name="rt">Repeat Total Location.</param>
        public static bool TryRepeatCIL(CommandQueue queue, CommandEntry entry, int ri, int rt)
        {
            CompiledCommandStackEntry cse = queue.CurrentEntry;
            RepeatCommandData dat = cse.EntryData[entry.BlockStart - 1] as RepeatCommandData;
            dat.Index++;
            (cse.LocalVariables[ri].Internal as IntegerTag).Internal = dat.Index;
            if (dat.Index <= dat.Total)
            {
                if (entry.ShouldShowGood(queue))
                {
                    entry.GoodOutput(queue, "Repeating...: " + TextStyle.Color_Separate + dat.Index +  TextStyle.Color_Base + "/" + TextStyle.Color_Separate + dat.Total);
                }
                return true;
            }
            if (entry.ShouldShowGood(queue))
            {
                entry.GoodOutput(queue, "Repeat stopping.");
            }
            return false;
        }

        /// <summary>
        /// Executes the numbered input part of the repeat command.
        /// </summary>
        /// <param name="queue">The command queue involved.</param>
        /// <param name="entry">Entry to be executed.</param>
        /// <param name="ri">Repeat Index location.</param>
        /// <param name="rt">Repeat Total Location.</param>
        public static bool TryRepeatNumberedCIL(CommandQueue queue, CommandEntry entry, int ri, int rt)
        {
            int target = (int)IntegerTag.TryFor(entry.GetArgumentObject(queue, 0)).Internal;
            if (target <= 0)
            {
                if (entry.ShouldShowGood(queue))
                {
                    entry.GoodOutput(queue, "Not repeating.");
                }
                return false;
            }
            entry.SetData(queue, new RepeatCommandData() { Index = 1, Total = target });
            CompiledCommandStackEntry ccse = queue.CurrentEntry;
            ccse.LocalVariables[ri].Internal = new IntegerTag(1);
            ccse.LocalVariables[rt].Internal = new IntegerTag(target);
            if (entry.ShouldShowGood(queue))
            {
                entry.GoodOutput(queue, "Repeating " + TextStyle.Color_Separate + target + TextStyle.Color_Base + " times...");
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
            queue.HandleError(entry, "Cannot Execute() a repeat, must compile!");
        }
    }
}
