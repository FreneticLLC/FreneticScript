//
// This file is part of FreneticScript, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of the MIT license.
// See README.md or LICENSE.txt in the FreneticScript source root for the contents of the license.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Objects;
using FreneticScript.CommandSystem.Arguments;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using FreneticUtilities.FreneticExtensions;
using FreneticScript.ScriptSystems;

namespace FreneticScript.CommandSystem.QueueCmds
{
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
            SaveMode = CommandSaveMode.MUST_SPECIFY;
            ObjectTypes = new List<Func<TemplateObject, TemplateObject>>()
            {
                Verify
            };
        }

        TemplateObject Verify(TemplateObject input)
        {
            if (long.TryParse(input.ToString(), out long rep))
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
        /// Represents the <see cref="TryRepeatCIL(CommandQueue, CommandEntry, int)"/> method.
        /// </summary>
        public static MethodInfo TryRepeatCILMethod = typeof(RepeatCommand).GetMethod(nameof(TryRepeatCIL));

        /// <summary>
        /// Represents the <see cref="TryRepeatCILNoDebug(CommandQueue, int, int)"/> method.
        /// </summary>
        public static MethodInfo TryRepeatCILMethodNoDebug = typeof(RepeatCommand).GetMethod(nameof(TryRepeatCILNoDebug));

        /// <summary>
        /// Represents the <see cref="TryRepeatNumberedCIL(CommandQueue, CommandEntry, int)"/> method.
        /// </summary>
        public static MethodInfo TryRepeatNumberedCILMethod = typeof(RepeatCommand).GetMethod(nameof(TryRepeatNumberedCIL));

        /// <summary>
        /// Represents the <see cref="TryRepeatNumberedCIL_NoDebug(CommandQueue, CommandEntry, int)"/> method.
        /// </summary>
        public static MethodInfo TryRepeatNumberedCIL_NoDebugMethod = typeof(RepeatCommand).GetMethod(nameof(TryRepeatNumberedCIL_NoDebug));

        /// <summary>
        /// Represents the <see cref="DebugStop(CommandQueue, CommandEntry)"/> method.
        /// </summary>
        public static MethodInfo DebugStopMethod = typeof(RepeatCommand).GetMethod(nameof(DebugStop));

        /// <summary>
        /// Represents the <see cref="DebugNext(CommandQueue, CommandEntry)"/> method.
        /// </summary>
        public static MethodInfo DebugNextMethod = typeof(RepeatCommand).GetMethod(nameof(DebugNext));

        /// <summary>
        /// Adapts a command entry to CIL.
        /// </summary>
        /// <param name="values">The adaptation-relevant values.</param>
        /// <param name="entry">The present entry ID.</param>
        public override void AdaptToCIL(CILAdaptationValues values, int entry)
        {
            CommandEntry cent = values.CommandAt(entry);
            bool db = cent.DBMode <= DebugMode.FULL;
            if (cent.IsCallback)
            {
                string sn = values.Entry.Entries[cent.BlockStart - 1].GetSaveNameNoParse("repeat_index");
                int lvar_ind_loc = cent.VarLoc(sn);
                values.LoadQueue();
                if (db)
                {
                    values.LoadEntry(entry);
                }
                else
                {
                    values.ILGen.Emit(OpCodes.Ldc_I4, cent.BlockStart - 1);
                }
                values.ILGen.Emit(OpCodes.Ldc_I4, lvar_ind_loc);
                values.ILGen.Emit(OpCodes.Call, db ? TryRepeatCILMethod : TryRepeatCILMethodNoDebug);
                values.ILGen.Emit(OpCodes.Brtrue, values.Entry.AdaptedILPoints[cent.BlockStart]);
                return;
            }
            string arg = cent.Arguments[0].ToString();
            if (arg == "stop")
            {
                for (int i = entry - 1; i >= 0; i--)
                {
                    CommandEntry nextEntry = values.Entry.Entries[i];
                    if (!(nextEntry.Command is RepeatCommand) || nextEntry.IsCallback)
                    {
                        continue;
                    }
                    string a0 = nextEntry.Arguments[0].ToString();
                    if (a0 != "stop" && a0 != "next" && nextEntry.InnerCommandBlock != null)
                    {
                        if (db)
                        {
                            values.LoadQueue();
                            values.LoadEntry(entry);
                            values.ILGen.Emit(OpCodes.Call, DebugStopMethod);
                        }
                        values.ILGen.Emit(OpCodes.Br, values.Entry.AdaptedILPoints[nextEntry.BlockEnd + 2]);
                        return;
                    }
                }
                throw new ErrorInducedException("Invalid 'repeat stop' command: not inside a repeat block!");
            }
            else if (arg == "next")
            {
                for (int i = entry - 1; i >= 0; i--)
                {
                    CommandEntry nextEntry = values.Entry.Entries[i];
                    if (!(nextEntry.Command is RepeatCommand) || nextEntry.IsCallback)
                    {
                        continue;
                    }
                    string a0 = nextEntry.ToString();
                    if (a0 != "stop" && a0 != "next" && nextEntry.InnerCommandBlock != null)
                    {
                        if (db)
                        {
                            values.LoadQueue();
                            values.LoadEntry(entry);
                            values.ILGen.Emit(OpCodes.Call, DebugNextMethod);
                        }
                        values.ILGen.Emit(OpCodes.Br, values.Entry.AdaptedILPoints[nextEntry.BlockEnd + 1]);
                        return;
                    }
                }
                throw new ErrorInducedException("Invalid 'repeat next' command: not inside a repeat block!");
            }
            else
            {
                int lvar_ind_loc = cent.VarLoc(cent.GetSaveNameNoParse("repeat_index"));
                values.LoadQueue();
                values.LoadEntry(entry);
                values.ILGen.Emit(OpCodes.Ldc_I4, lvar_ind_loc);
                values.ILGen.Emit(OpCodes.Call, db ? TryRepeatNumberedCILMethod : TryRepeatNumberedCIL_NoDebugMethod);
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
            if (cent.IsCallback)
            {
                values.PopVarSet();
                return;
            }
            string arg = cent.Arguments[0].ToString();
            if (arg == "next" || arg == "stop")
            {
                return;
            }
            values.PushVarSet();
            string sn = cent.GetSaveNameNoParse("repeat_index");
            // TODO: scope properly!
            if (values.LocalVariableLocation(sn) >= 0)
            {
                throw new ErrorInducedException("Already have a repeat_index var (labeled '" + sn + "')?!");
            }
            TagType type = cent.TagSystem.Types.Type_Integer;
            values.AddVariable(sn, type);
        }

        /// <summary>
        /// Shows debug for a repeat 'stop' command.
        /// </summary>
        /// <param name="queue">The command queue.</param>
        /// <param name="entry">The command entry.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DebugStop(CommandQueue queue, CommandEntry entry)
        {
            if (entry.ShouldShowGood(queue))
            {
                entry.GoodOutput(queue, "Repeat loop stopped successfully.");
            }
        }

        /// <summary>
        /// Shows debug for a repeat 'next' command.
        /// </summary>
        /// <param name="queue">The command queue.</param>
        /// <param name="entry">The command entry.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DebugNext(CommandQueue queue, CommandEntry entry)
        {
            if (entry.ShouldShowGood(queue))
            {
                entry.GoodOutput(queue, "Repeat loop jumping to next iteration.");
            }
        }

        /// <summary>
        /// Executes the callback part of the repeat command, without debug output.
        /// </summary>
        /// <param name="queue">The command queue involved.</param>
        /// <param name="entry_ind">Entry to be executed.</param>
        /// <param name="ri">Repeat Index location.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryRepeatCILNoDebug(CommandQueue queue, int entry_ind, int ri)
        {
            CompiledCommandStackEntry cse = queue.CurrentStackEntry;
            RepeatCommandData dat = cse.EntryData[entry_ind] as RepeatCommandData;
            return ((cse.LocalVariables[ri].Internal as IntegerTag).Internal = ++dat.Index) <= dat.Total;
        }

        /// <summary>
        /// Executes the callback part of the repeat command.
        /// </summary>
        /// <param name="queue">The command queue involved.</param>
        /// <param name="entry">Entry to be executed.</param>
        /// <param name="ri">Repeat Index location.</param>
        public static bool TryRepeatCIL(CommandQueue queue, CommandEntry entry, int ri)
        {
            CompiledCommandStackEntry cse = queue.CurrentStackEntry;
            RepeatCommandData dat = cse.EntryData[entry.BlockStart - 1] as RepeatCommandData;
            (cse.LocalVariables[ri].Internal as IntegerTag).Internal = ++dat.Index;
            if (dat.Index <= dat.Total)
            {
                if (entry.ShouldShowGood(queue))
                {
                    entry.GoodOutput(queue, "Repeating...: " + TextStyle.Separate + dat.Index +  TextStyle.Base + "/" + TextStyle.Separate + dat.Total);
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
        /// Executes the numbered input part of the repeat command, without debug.
        /// </summary>
        /// <param name="queue">The command queue involved.</param>
        /// <param name="entry">Entry to be executed.</param>
        /// <param name="ri">Repeat Index location.</param>
        public static bool TryRepeatNumberedCIL_NoDebug(CommandQueue queue, CommandEntry entry, int ri)
        {
            int target = (int)IntegerTag.TryFor(entry.GetArgumentObject(queue, 0)).Internal;
            if (target <= 0)
            {
                return false;
            }
            entry.SetData(queue, new RepeatCommandData() { Index = 1, Total = target });
            CompiledCommandStackEntry ccse = queue.CurrentStackEntry;
            ccse.LocalVariables[ri].Internal = new IntegerTag(1);
            return true;
        }

        /// <summary>
        /// Executes the numbered input part of the repeat command.
        /// </summary>
        /// <param name="queue">The command queue involved.</param>
        /// <param name="entry">Entry to be executed.</param>
        /// <param name="ri">Repeat Index location.</param>
        public static bool TryRepeatNumberedCIL(CommandQueue queue, CommandEntry entry, int ri)
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
            CompiledCommandStackEntry ccse = queue.CurrentStackEntry;
            ccse.LocalVariables[ri].Internal = new IntegerTag(1);
            if (entry.ShouldShowGood(queue))
            {
                entry.GoodOutput(queue, "Repeating " + TextStyle.Separate + target + TextStyle.Base + " times...");
            }
            return true;
        }
        
        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="queue">The command queue involved.</param>
        /// <param name="entry">Entry to be executed.</param>
        public static void Execute(CommandQueue queue, CommandEntry entry)
        {
            queue.HandleError(entry, "Cannot Execute() a repeat, must compile!");
        }
    }
}
