//
// This file is part of FreneticScript, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of the MIT license.
// See README.md or LICENSE.txt in the FreneticScript source root for the contents of the license.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers.Objects;
using FreneticScript.TagHandlers;
using FreneticScript.CommandSystem.Arguments;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using FreneticUtilities.FreneticExtensions;

namespace FreneticScript.CommandSystem.QueueCmds
{
    class ForeachCommandData : AbstractCommandEntryData
    {
        public List<TemplateObject> List;
        public int Index;
    }

    /// <summary>
    /// Command to loop through a list.
    /// </summary>
    public class ForeachCommand : AbstractCommand
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
        // @Var foreach_value DynamicTag returns the current item in the list.
        // @Var foreach_list ListTag returns the full list being looped through.
        // -->

        /// <summary>
        /// Construct the foreach command.
        /// </summary>
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
            SaveMode = CommandSaveMode.REQUIRED_NAME;
            ObjectTypes = new List<Func<TemplateObject, TemplateObject>>()
            {
                Verify,
                ListTag.CreateFor
            };
        }

        TemplateObject Verify(TemplateObject input)
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

        /// <summary>
        /// Represents the <see cref="TryRepeatCIL(CommandQueue, CommandEntry, int)"/> method.
        /// </summary>
        public static MethodInfo TryRepeatCILMethod = typeof(ForeachCommand).GetMethod(nameof(TryRepeatCIL));

        /// <summary>
        /// Represents the <see cref="TryRepeatCILNoDebug(CommandQueue, int, int)"/> method.
        /// </summary>
        public static MethodInfo TryRepeatCILMethodNoDebug = typeof(ForeachCommand).GetMethod(nameof(TryRepeatCILNoDebug));

        /// <summary>
        /// Represents the <see cref="TryRepeatNumberedCIL(CommandQueue, CommandEntry, int)"/> method.
        /// </summary>
        public static MethodInfo TryRepeatNumberedCILMethod = typeof(ForeachCommand).GetMethod(nameof(TryRepeatNumberedCIL));

        /// <summary>
        /// Adapts a command entry to CIL.
        /// </summary>
        /// <param name="values">The adaptation-relevant values.</param>
        /// <param name="entry">The present entry ID.</param>
        public override void AdaptToCIL(CILAdaptationValues values, int entry)
        {
            CommandEntry cent = values.CommandAt(entry);
            string arg = cent.Arguments[0].ToString();
            if (arg == "\0CALLBACK")
            {
                int lvar_ind_loc = GetSaveLoc(values, entry);
                values.LoadQueue();
                bool db = cent.DBMode <= DebugMode.FULL;
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
            }
            else if (arg == "stop")
            {
                for (int i = entry - 1; i >= 0; i--)
                {
                    if (!(values.Entry.Entries[i].Command is ForeachCommand))
                    {
                        continue;
                    }
                    string a0 = values.Entry.Entries[i].Arguments[0].ToString();
                    if (a0 == "start" && values.Entry.Entries[i].InnerCommandBlock != null)
                    {
                        // TODO: Debug output?
                        values.ILGen.Emit(OpCodes.Br, values.Entry.AdaptedILPoints[values.Entry.Entries[i].BlockEnd + 2]);
                        return;
                    }
                }
                throw new ErrorInducedException("Invalid 'foreach stop' command: not inside a foreach block!");
            }
            else if (arg == "next")
            {
                for (int i = entry - 1; i >= 0; i--)
                {
                    if (!(values.Entry.Entries[i].Command is ForeachCommand))
                    {
                        continue;
                    }
                    string a0 = values.Entry.Entries[i].Arguments[0].ToString();
                    if (a0 == "start" && values.Entry.Entries[i].InnerCommandBlock != null)
                    {
                        // TODO: Debug output?
                        values.ILGen.Emit(OpCodes.Br, values.Entry.AdaptedILPoints[values.Entry.Entries[i].BlockEnd + 1]);
                        return;
                    }
                }
                throw new ErrorInducedException("Invalid 'foreach next' command: not inside a foreach block!");
            }
            else if (arg == "start")
            {
                int lvar_ind_loc = cent.VarLoc(cent.GetSaveNameNoParse("foreach_value"));
                values.LoadQueue();
                values.LoadEntry(entry);
                values.ILGen.Emit(OpCodes.Ldc_I4, lvar_ind_loc);
                values.ILGen.Emit(OpCodes.Call, TryRepeatNumberedCILMethod);
                values.ILGen.Emit(OpCodes.Brfalse, values.Entry.AdaptedILPoints[cent.BlockEnd + 2]);
            }
            else
            {
                throw new ErrorInducedException("Invalid 'foreach' command: unknown argument: " + arg);
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
            // TODO: scope properly!
            PreAdaptSaveMode(values, entry, false, cent.System.TagSystem.Types.Type_Dynamic, true);
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
            ForeachCommandData dat = cse.EntryData[entry_ind] as ForeachCommandData;
            if (++dat.Index < dat.List.Count)
            {
                (cse.LocalVariables[ri].Internal as DynamicTag).Internal = dat.List[dat.Index];
                return true;
            }
            return false;
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
            ForeachCommandData dat = cse.EntryData[entry.BlockStart - 1] as ForeachCommandData;
            if (++dat.Index < dat.List.Count)
            {
                (cse.LocalVariables[ri].Internal as DynamicTag).Internal = dat.List[dat.Index];
                if (entry.ShouldShowGood(queue))
                {
                    entry.GoodOutput(queue, "Looping...: " + TextStyle.Separate + dat.Index + TextStyle.Base + "/" + TextStyle.Separate + dat.List.Count);
                }
                return true;
            }
            if (entry.ShouldShowGood(queue))
            {
                entry.GoodOutput(queue, "Foreach stopping.");
            }
            return false;
        }

        /// <summary>
        /// Executes the numbered input part of the repeat command.
        /// </summary>
        /// <param name="queue">The command queue involved.</param>
        /// <param name="entry">Entry to be executed.</param>
        /// <param name="ri">Repeat Index location.</param>
        public static bool TryRepeatNumberedCIL(CommandQueue queue, CommandEntry entry, int ri)
        {
            ListTag list = ListTag.CreateFor(entry.GetArgumentObject(queue, 1));
            if (list.Internal.Count == 0)
            {
                if (entry.ShouldShowGood(queue))
                {
                    entry.GoodOutput(queue, "Not looping.");
                }
                return false;
            }
            entry.SetData(queue, new ForeachCommandData() { Index = 0, List = list.Internal });
            CompiledCommandStackEntry ccse = queue.CurrentStackEntry;
            ccse.LocalVariables[ri].Internal = new DynamicTag(list.Internal[0]);
            if (entry.ShouldShowGood(queue))
            {
                entry.GoodOutput(queue, "Looping " + TextStyle.Separate + list.Internal.Count + TextStyle.Base + " times...");
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
            queue.HandleError(entry, "Cannot Execute() a foreach, must compile!");
        }
    }
}
