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
using FreneticScript.ScriptSystems;
using FreneticScript.TagHandlers.Objects;
using FreneticScript.UtilitySystems;

namespace FreneticScript.CommandSystem.QueueCmds;

// <--[command]
// @Name function
// @Arguments 'stop'/'define'/'undefine' [name of function] ['quiet_fail']
// @Short Creates a new function of the following command block, and adds it to the script cache.
// @Updated 2014/06/23
// @Authors mcmonkey
// @Group Queue
// @Block Allowed
// @Minimum 1
// @Maximum 3
// @Description
// The function command will define the included command block to be a function which can be activated
// by the <@link command call>call<@/link> command.
// Add the 'quiet_fail' argument to not produce an error message if the function already exists.
// Use "function stop" inside a function to end the function call without killing the queue that started it.
// TODO: Explain more!
// @Example
// // This example creates function "helloworld" which, when called, outputs "hello world", then stops before it can output a "!".
// function define helloworld
// {
//     echo "hello world";
//     function stop;
//     echo "!";
// }
// @Example
// // This example creates function "outputme" which, when called with variable 'text', outputs the specified text.
// function define outputme
// {
//     require text;
//     echo <{var[text]}>;
// }
// @Example
// // This example creates function "getinfo" which, puts the result of an information lookup in the variable 'result' for later usage.
// function define outputme
// {
//     var result = "This is the result!";
// }
// @Example
// TODO: More examples!
// -->

/// <summary>The Function command.</summary>
public class FunctionCommand : AbstractCommand
{
    /// <summary>Adjust list of commands that are formed by an inner block.</summary>
    /// <param name="entry">The producing entry.</param>
    /// <param name="input">The block of commands.</param>
    /// <param name="fblock">The final block to add to the entry.</param>
    public override void AdaptBlockFollowers(CommandEntry entry, List<CommandEntry> input, List<CommandEntry> fblock)
    {
        entry.BlockEnd -= input.Count;
        input.Clear();
        base.AdaptBlockFollowers(entry, input, fblock);
        fblock.Add(GetFollower(entry));
    }

    /// <summary>Constructs the function command.</summary>
    public FunctionCommand()
    {
        Name = "function";
        Arguments = "'stop'/'define'/'undefine' [name of function]";
        Description = "Creates a new function of the following command block, and adds it to the script cache.";
        Asyncable = true;
        MinimumArguments = 1;
        MaximumArguments = 3;
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
        string arg0 = cent.Arguments[0].ToString();
        if (arg0 == "define")
        {
            values.PushVarSet();
            // TODO: Forcibly compress the block to an argument, instead of compiling the sub-block
        }
        else if (arg0 != "undefine" && arg0 != "stop")
        {
            throw new ErrorInducedException("First argument must be 'define', 'undefine', or 'stop'.");
        }
    }

    /// <summary>Represents the <see cref="DebugStop(CommandQueue, CommandEntry)"/> method.</summary>
    public static MethodInfo DebugStopMethod = typeof(FunctionCommand).GetMethod(nameof(DebugStop));

    /// <summary>Represents the <see cref="DebugCallback(CommandQueue, CommandEntry)"/> method.</summary>
    public static MethodInfo DebugCallbackMethod = typeof(FunctionCommand).GetMethod(nameof(DebugCallback));

    /// <summary>Represents the <see cref="Undefine(CommandEntry, CommandQueue)"/> method.</summary>
    public static MethodInfo UndefineMethod = typeof(FunctionCommand).GetMethod(nameof(Undefine));

    /// <summary>Represents the <see cref="Define(CommandEntry, CommandQueue)"/> method.</summary>
    public static MethodInfo DefineMethod = typeof(FunctionCommand).GetMethod(nameof(Define));

    /// <summary>Shows debug for a function 'stop' command.</summary>
    /// <param name="queue">The command queue.</param>
    /// <param name="entry">The command entry.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void DebugStop(CommandQueue queue, CommandEntry entry)
    {
        if (entry.ShouldShowGood(queue))
        {
            entry.GoodOutput(queue, "Stopping a function call.");
        }
    }

    /// <summary>Shows debug for a function callback.</summary>
    /// <param name="queue">The command queue.</param>
    /// <param name="entry">The command entry.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void DebugCallback(CommandQueue queue, CommandEntry entry)
    {
        if (entry.ShouldShowGood(queue))
        {
            entry.GoodOutput(queue, "Completed function call.");
        }
    }

    /// <summary>Adapts a command entry to CIL.</summary>
    /// <param name="values">The adaptation-relevant values.</param>
    /// <param name="entry">The relevant entry ID.</param>
    public override void AdaptToCIL(CILAdaptationValues values, int entry)
    {
        CommandEntry cent = values.Entry.Entries[entry];
        bool db = cent.DBMode <= DebugMode.FULL;
        if (cent.IsCallback)
        {
            if (db)
            {
                values.LoadQueue();
                values.LoadEntry(entry);
                values.ILGen.Emit(OpCodes.Call, DebugCallbackMethod);
            }
            return;
        }
        string arg0 = cent.Arguments[0].ToString();
        if (arg0 == "stop")
        {
            for (int i = entry - 1; i >= 0; i--)
            {
                CommandEntry nextEntry = values.Entry.Entries[i];
                if (nextEntry.Command is not FunctionCommand)
                {
                    continue;
                }
                if (nextEntry.IsCallback)
                {
                    if (db)
                    {
                        values.LoadQueue();
                        values.LoadEntry(entry);
                        values.ILGen.Emit(OpCodes.Call, DebugStopMethod);
                    }
                    values.ILGen.Emit(OpCodes.Br, values.Entry.AdaptedILPoints[i + 2]);
                    return;
                }
            }
            throw new ErrorInducedException("Invalid 'function stop' command: not inside a function block!");
        }
        else if (arg0 == "undefine")
        {
            if (cent.Arguments.Length < 2)
            {
                throw new ErrorInducedException("Invalid 'function undefine' command: must label what function to undefine (as an additional argument)!");
            }
            values.LoadEntry(entry);
            values.LoadQueue();
            values.ILGen.Emit(OpCodes.Call, UndefineMethod);
        }
        else // "define"
        {
            if (cent.Arguments.Length < 2)
            {
                throw new ErrorInducedException("Invalid 'function define' command: must name the function to define (as an additional argument)!");
            }
            if (cent.InnerCommandBlock == null)
            {
                throw new ErrorInducedException("Invalid 'function define' command: must be followed by a block-function to define!");
            }
            values.LoadEntry(entry);
            values.LoadQueue();
            values.ILGen.Emit(OpCodes.Call, DefineMethod);
            values.ILGen.Emit(OpCodes.Br, values.Entry.AdaptedILPoints[cent.BlockEnd + 2]);
        }
    }

    /// <summary>Executes an undefine.</summary>
    /// <param name="queue">The command queue involved.</param>
    /// <param name="entry">Entry to be executed.</param>
    public static void Undefine(CommandEntry entry, CommandQueue queue)
    {
        string name = entry.GetArgument(queue, 1).ToLowerFast();
        if (!queue.Engine.Functions.Remove(name))
        {
            if (BooleanTag.TryFor(entry.GetNamedArgumentObject(queue, "quiet_fail"))?.Internal ?? false)
            {
                if (entry.ShouldShowGood(queue))
                {
                    entry.GoodOutput(queue, "Function '" + TextStyle.Separate + name + TextStyle.Base + "' doesn't exist!");
                }
            }
            else
            {
                queue.HandleError(entry, "Function '" + TextStyle.Separate + name + TextStyle.Base + "' doesn't exist!");
            }
        }
        else
        {
            if (entry.ShouldShowGood(queue))
            {
                entry.GoodOutput(queue, "Function '" + TextStyle.Separate + name + TextStyle.Base + "' undefined.");
            }
        }
    }

    /// <summary>Executes a define.</summary>
    /// <param name="queue">The command queue involved.</param>
    /// <param name="entry">Entry to be executed.</param>
    public static void Define(CommandEntry entry, CommandQueue queue)
    {
        string name = entry.GetArgument(queue, 1).ToLowerFast();
        if (queue.Engine.Functions.ContainsKey(name))
        {
            if (BooleanTag.TryFor(entry.GetNamedArgumentObject(queue, "quiet_fail"))?.Internal ?? false)
            {
                if (entry.ShouldShowGood(queue))
                {
                    entry.GoodOutput(queue, "Function '" + TextStyle.Separate + name + TextStyle.Base + "' already exists!");
                }
            }
            else
            {
                queue.HandleError(entry, "Function '" + TextStyle.Separate + name + TextStyle.Base + "' already exists!");
            }
        }
        else
        {
            queue.Engine.Functions.Add(name, new CommandScript(name, CommandScript.TYPE_NAME_FUNCTION, entry.InnerCommandBlock, entry.System, entry.BlockStart, entry.DBMode));
            if (entry.ShouldShowGood(queue))
            {
                entry.GoodOutput(queue, "Function '" + TextStyle.Separate + name + TextStyle.Base + "' defined.");
            }
        }
    }

    /// <summary>Executes the command.</summary>
    /// <param name="queue">The command queue involved.</param>
    /// <param name="entry">Entry to be executed.</param>
    public static void Execute(CommandQueue queue, CommandEntry entry)
    {
        queue.HandleError(entry, "Cannot Execute() a function command, must compile!");
    }
}
