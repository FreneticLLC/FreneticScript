//
// This file is part of FreneticScript, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of the MIT license.
// See README.md or LICENSE.txt in the FreneticScript source root for the contents of the license.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using FreneticUtilities.FreneticExtensions;
using FreneticScript.CommandSystem.Arguments;
using FreneticScript.ScriptSystems;
using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Objects;

namespace FreneticScript.CommandSystem.QueueCmds
{
    class ForeachCommandData : AbstractCommandEntryData
    {
        public List<TemplateObject> List;
        public int Index;
    }

    /// <summary>Command to loop through a list.</summary>
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

        /// <summary>Construct the foreach command.</summary>
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
            SaveMode = CommandSaveMode.MUST_SPECIFY;
        }

        /// <summary>Represents the <see cref="TryForeachCIL(CommandQueue, CommandEntry, DynamicTag)"/> method.</summary>
        public static MethodInfo TryForeachCILMethod = typeof(ForeachCommand).GetMethod(nameof(TryForeachCIL));

        /// <summary>Represents the <see cref="TryForeachCILNoDebug(CommandQueue, int, DynamicTag)"/> method.</summary>
        public static MethodInfo TryForeachCILMethodNoDebug = typeof(ForeachCommand).GetMethod(nameof(TryForeachCILNoDebug));

        /// <summary>Represents the <see cref="TryForeachNumberedCIL(CommandQueue, CommandEntry, DynamicTag)"/> method.</summary>
        public static MethodInfo TryForeachNumberedCILMethod = typeof(ForeachCommand).GetMethod(nameof(TryForeachNumberedCIL));

        /// <summary>Represents the <see cref="TryForeachNumberedCIL_NoDebug(CommandQueue, CommandEntry, DynamicTag)"/> method.</summary>
        public static MethodInfo TryForeachNumberedCIL_NoDebugMethod = typeof(ForeachCommand).GetMethod(nameof(TryForeachNumberedCIL_NoDebug));

        /// <summary>Represents the <see cref="DebugStop(CommandQueue, CommandEntry)"/> method.</summary>
        public static MethodInfo DebugStopMethod = typeof(ForeachCommand).GetMethod(nameof(DebugStop));

        /// <summary>Represents the <see cref="DebugNext(CommandQueue, CommandEntry)"/> method.</summary>
        public static MethodInfo DebugNextMethod = typeof(ForeachCommand).GetMethod(nameof(DebugNext));

        /// <summary>Represents the <see cref="CreateListItem"/> method.</summary>
        public static MethodInfo CreateListItemMethod = typeof(ForeachCommand).GetMethod(nameof(CreateListItem));

        /// <summary>Adapts a command entry to CIL.</summary>
        /// <param name="values">The adaptation-relevant values.</param>
        /// <param name="entry">The present entry ID.</param>
        public override void AdaptToCIL(CILAdaptationValues values, int entry)
        {
            CommandEntry cent = values.CommandAt(entry);
            bool db = cent.DBMode <= DebugMode.FULL;
            if (cent.IsCallback)
            {
                int lvar_ind_loc = GetSaveLoc(values, entry);
                values.LoadQueue();
                if (db)
                {
                    values.LoadEntry(entry);
                }
                else
                {
                    values.ILGen.Emit(OpCodes.Ldc_I4, cent.BlockStart - 1);
                }
                values.LoadLocalVariable(lvar_ind_loc);
                values.ILGen.Emit(OpCodes.Call, db ? TryForeachCILMethod : TryForeachCILMethodNoDebug);
                values.ILGen.Emit(OpCodes.Brtrue, values.Entry.AdaptedILPoints[cent.BlockStart]);
                return;
            }
            string arg = cent.Arguments[0].ToString();
            if (arg == "stop")
            {
                for (int i = entry - 1; i >= 0; i--)
                {
                    CommandEntry nextEntry = values.Entry.Entries[i];
                    if (nextEntry.Command is not ForeachCommand || nextEntry.IsCallback)
                    {
                        continue;
                    }
                    string a0 = nextEntry.Arguments[0].ToString();
                    if (a0 == "start" && nextEntry.InnerCommandBlock != null)
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
                throw new ErrorInducedException("Invalid 'foreach stop' command: not inside a foreach block!");
            }
            else if (arg == "next")
            {
                for (int i = entry - 1; i >= 0; i--)
                {
                    CommandEntry nextEntry = values.Entry.Entries[i];
                    if (nextEntry.Command is not ForeachCommand || nextEntry.IsCallback)
                    {
                        continue;
                    }
                    string a0 = nextEntry.Arguments[0].ToString();
                    if (a0 == "start" && nextEntry.InnerCommandBlock != null)
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
                throw new ErrorInducedException("Invalid 'foreach next' command: not inside a foreach block!");
            }
            else if (arg == "start")
            {
                SingleCILVariable locVar = cent.VarLookup[cent.GetSaveNameNoParse("foreach_value")];
                values.LoadQueue();
                values.LoadEntry(entry);
                values.LoadRunnable();
                values.ILGen.Emit(OpCodes.Call, CreateListItemMethod);
                values.ILGen.Emit(OpCodes.Stfld, locVar.Field);
                values.LoadLocalVariable(locVar.Index);
                values.ILGen.Emit(OpCodes.Call, db ? TryForeachNumberedCILMethod : TryForeachNumberedCIL_NoDebugMethod);
                values.ILGen.Emit(OpCodes.Brfalse, values.Entry.AdaptedILPoints[cent.BlockEnd + 2]);
            }
            else
            {
                throw new ErrorInducedException("Invalid 'foreach' command: unknown argument: " + arg);
            }
        }

        /// <summary>Prepares to adapt a command entry to CIL.</summary>
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
            // TODO: scope properly!
            PreAdaptSaveMode(values, entry, false, cent.System.TagSystem.Types.Type_Dynamic, true);
        }

        /// <summary>Shows debug for a foreach 'stop' command.</summary>
        /// <param name="queue">The command queue.</param>
        /// <param name="entry">The command entry.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DebugStop(CommandQueue queue, CommandEntry entry)
        {
            if (entry.ShouldShowGood(queue))
            {
                entry.GoodOutput(queue, "Foreach loop stopped successfully.");
            }
        }

        /// <summary>Shows debug for a foreach 'next' command.</summary>
        /// <param name="queue">The command queue.</param>
        /// <param name="entry">The command entry.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DebugNext(CommandQueue queue, CommandEntry entry)
        {
            if (entry.ShouldShowGood(queue))
            {
                entry.GoodOutput(queue, "Foreach loop jumping to next iteration.");
            }
        }

        /// <summary>Executes the callback part of the foreach command, without debug output.</summary>
        /// <param name="queue">The command queue involved.</param>
        /// <param name="entry_ind">Entry to be executed.</param>
        /// <param name="listItem">Dynamic tag to hold the item in the list.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryForeachCILNoDebug(CommandQueue queue, int entry_ind, DynamicTag listItem)
        {
            ForeachCommandData dat = queue.CurrentRunnable.EntryData[entry_ind] as ForeachCommandData;
            if (++dat.Index < dat.List.Count)
            {
                listItem.Internal = dat.List[dat.Index];
                return true;
            }
            return false;
        }

        /// <summary>Executes the callback part of the foreach command.</summary>
        /// <param name="queue">The command queue involved.</param>
        /// <param name="entry">Entry to be executed.</param>
        /// <param name="listItem">Dynamic tag to hold the item in the list.</param>
        public static bool TryForeachCIL(CommandQueue queue, CommandEntry entry, DynamicTag listItem)
        {
            ForeachCommandData dat = queue.CurrentRunnable.EntryData[entry.BlockStart - 1] as ForeachCommandData;
            if (++dat.Index < dat.List.Count)
            {
                listItem.Internal = dat.List[dat.Index];
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

        /// <summary>Executes the list input part of the foreached command, without debug.</summary>
        /// <param name="queue">The command queue involved.</param>
        /// <param name="entry">Entry to be executed.</param>
        /// <param name="listItem">Dynamic tag to hold the item in the list.</param>
        public static bool TryForeachNumberedCIL_NoDebug(CommandQueue queue, CommandEntry entry, DynamicTag listItem)
        {
            ListTag list = ListTag.CreateFor(entry.GetArgumentObject(queue, 1));
            if (list.Internal.Count == 0)
            {
                return false;
            }
            entry.SetData(queue, new ForeachCommandData() { Index = 0, List = list.Internal });
            listItem.Internal = list.Internal[0];
            return true;
        }

        /// <summary>Executes the list input part of the foreached command.</summary>
        /// <param name="queue">The command queue involved.</param>
        /// <param name="entry">Entry to be executed.</param>
        /// <param name="listItem">Dynamic tag to hold the item in the list.</param>
        public static bool TryForeachNumberedCIL(CommandQueue queue, CommandEntry entry, DynamicTag listItem)
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
            listItem.Internal = list.Internal[0];
            if (entry.ShouldShowGood(queue))
            {
                entry.GoodOutput(queue, "Looping " + TextStyle.Separate + list.Internal.Count + TextStyle.Base + " times...");
            }
            return true;
        }

        /// <summary>Creates a repeat index object.</summary>
        /// <returns>The index object.</returns>
        public static DynamicTag CreateListItem()
        {
            return new DynamicTag(NullTag.NULL_VALUE);
        }

        /// <summary>Executes the command.</summary>
        /// <param name="queue">The command queue involved.</param>
        /// <param name="entry">Entry to be executed.</param>
        public static void Execute(CommandQueue queue, CommandEntry entry)
        {
            queue.HandleError(entry, "Cannot Execute() a foreach, must compile!");
        }
    }
}
