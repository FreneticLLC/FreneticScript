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
using System.Text;
using FreneticUtilities.FreneticExtensions;
using FreneticScript.CommandSystem.Arguments;
using FreneticScript.ScriptSystems;
using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Objects;

namespace FreneticScript.CommandSystem.QueueCmds;

class IfCommandData : AbstractCommandEntryData
{
    public int Result;
}

/// <summary>The if command.</summary>
public class IfCommand: AbstractCommand
{
    // <--[command]
    // @Name if
    // @Arguments <comparisons>
    // @Short Executes the following block of commands only if the input is true.
    // @Updated 2016/04/28
    // @Authors mcmonkey
    // @Group Queue
    // @Block Always
    // @Minimum 1
    // @Maximum 5
    // @Description
    // Executes the following block of commands only if the input is true.
    // Works with the <@link command else>else command<@/link>.
    // TODO: Explain more!
    // @Example
    // // This example echos "hi".
    // if true
    // {
    //     echo "hi";
    // }
    // @Example
    // // TODO: More examples!
    // -->

    /// <summary>Construct the if commnad.</summary>
    public IfCommand()
    {
        Name = "if";
        Arguments = "<comparisons>";
        Description = "Executes the following block of commands only if the input is true.";
        IsFlow = true;
        Asyncable = true;
        MinimumArguments = 1;
        MaximumArguments = -1;
    }

    /// <summary>Represents the "TryIfCIL(queue, entry)" method.</summary>
    public static MethodInfo TryIfCILMethod = typeof(IfCommand).GetMethod("TryIfCIL", new Type[] { typeof(CommandQueue), typeof(CommandEntry) });

    /// <summary>Adapts a command entry to CIL.</summary>
    /// <param name="values">The adaptation-relevant values.</param>
    /// <param name="entry">The present entry ID.</param>
    public override void AdaptToCIL(CILAdaptationValues values, int entry)
    {
        CommandEntry cent = values.Entry.Entries[entry];
        if (cent.IsCallback)
        {
            values.MarkCommand(entry);
            // TODO: Debug?
            int dodgepoint = cent.BlockEnd + 2;
            while (dodgepoint < values.Entry.Entries.Length)
            {
                CommandEntry tent = values.Entry.Entries[dodgepoint];
                if (tent.Command is ElseCommand)
                {
                    dodgepoint = tent.BlockEnd + 2;
                }
                else
                {
                    break;
                }
            }
            values.ILGen.Emit(OpCodes.Br, values.Entry.AdaptedILPoints[dodgepoint]);
            return;
        }
        if (cent.BlockEnd <= 0)
        {
            throw new Exception("Incorrectly defined IF command: no block follows!");
        }
        values.MarkCommand(entry);
        values.LoadQueue();
        values.LoadEntry(entry);
        values.ILGen.Emit(OpCodes.Call, TryIfCILMethod);
        values.ILGen.Emit(OpCodes.Brfalse, values.Entry.AdaptedILPoints[cent.BlockEnd + 2]);
    }

    /// <summary>Executes the command via CIL.</summary>
    /// <param name="queue">The command queue involved.</param>
    /// <param name="entry">Entry to be executed.</param>
    public static bool TryIfCIL(CommandQueue queue, CommandEntry entry)
    {
        entry.SetData(queue, new IfCommandData() { Result = 0 });
        bool success = TryIf(queue, entry, new List<Argument>(entry.Arguments));
        if (success)
        {
            if (entry.ShouldShowGood(queue))
            {
                entry.GoodOutput(queue, "If is true, executing...");
            }
            ((IfCommandData)entry.GetData(queue)).Result = 1;
        }
        else
        {
            if (entry.ShouldShowGood(queue))
            {
                entry.GoodOutput(queue, "If is false, doing nothing!");
            }
        }
        return success;
    }

    /// <summary>Executes the command.</summary>
    /// <param name="queue">The command queue involved.</param>
    /// <param name="entry">Entry to be executed.</param>
    public static void Execute(CommandQueue queue, CommandEntry entry)
    {
        throw new NotImplementedException("Unknown execution of IF command, invalid!");
    }

    /// <summary>Gets the boolean for a string.</summary>
    /// <param name="error">What error method to use.</param>
    /// <param name="str">The string.</param>
    /// <returns>The boolean.</returns>
    public static bool GetBool(Action<string> error, string str)
    {
        if (str.StartsWith("!"))
        {
            return !GetBool(error, str[1..]);
        }
        string low = str.ToLowerFast();
        if (low == "true")
        {
            return true;
        }
        else if (low == "false")
        {
            return false;
        }
        error("Invalid If: Invalid boolean input!");
        return false;
    }

    // TODO: better comparison system!
    /// <summary>Tries input IF to see if it is TRUE or FALSE.</summary>
    /// <param name="queue">The command queue of relevance.</param>
    /// <param name="entry">The command entry of relevance.</param>
    /// <param name="arguments">The input arguments.</param>
    /// <returns>Whether it is true or not.</returns>
    public static bool TryIf(CommandQueue queue, CommandEntry entry, List<Argument> arguments)
    {
        if (arguments.Count == 0)
        {
            queue.HandleError(entry, "Invalid IF: No arguments!");
            return false;
        }
        if (arguments.Count == 1)
        {
            TemplateObject theto = arguments[0].Parse(queue.Error, queue.CurrentRunnable);
            if (theto is BooleanTag)
            {
                return (theto as BooleanTag).Internal;
            }
            return GetBool(queue.Error, theto.ToString());
        }
        for (int i = 0; i < arguments.Count; i++)
        {
            string astr = arguments[i].ToString();
            if (astr == "(")
            {
                List<Argument> subargs = new();
                int count = 0;
                bool found = false;
                for (int x = i + 1; x < arguments.Count; x++)
                {
                    string xstr = arguments[x].ToString();
                    if (xstr == "(")
                    {
                        count++;
                        subargs.Add(arguments[x]);
                    }
                    else if (xstr == ")")
                    {
                        count--;
                        if (count == -1)
                        {
                            bool cfound = TryIf(queue, entry, subargs);
                            arguments.RemoveRange(i, (x - i) + 1);
                            Argument narg = new() { Bits = new ArgumentBit[] { new TextArgumentBit(cfound, queue.Engine) } };
                            arguments.Insert(i, narg);
                            found = true;
                        }
                        else
                        {
                            subargs.Add(arguments[x]);
                        }
                    }
                    else
                    {
                        subargs.Add(arguments[x]);
                    }
                }
                if (!found)
                {
                    queue.HandleError(entry, "Invalid IF: Inconsistent parenthesis!");
                    return false;
                }
            }
            else if (astr == ")")
            {
                return false;
            }
        }
        if (arguments.Count == 1)
        {
            TemplateObject theto = arguments[0].Parse(queue.Error, queue.CurrentRunnable);
            if (theto is BooleanTag)
            {
                return (theto as BooleanTag).Internal;
            }
            return GetBool(queue.Error, theto.ToString());
        }
        for (int i = 0; i < arguments.Count; i++)
        {
            string astr = arguments[i].ToString();
            if (astr == "||")
            {
                List<Argument> beforeargs = new(i);
                for (int x = 0; x < i; x++)
                {
                    beforeargs.Add(arguments[x]);
                }
                if (TryIf(queue, entry, beforeargs))
                {
                    return true;
                }
                List<Argument> afterargs = new(arguments.Count - i);
                for (int x = i + 1; x < arguments.Count; x++)
                {
                    afterargs.Add(arguments[x]);
                }
                return TryIf(queue, entry, afterargs);
            }
            else if (astr == "&&")
            {
                List<Argument> beforeargs = new(i);
                for (int x = 0; x < i; x++)
                {
                    beforeargs.Add(arguments[x]);
                }
                if (!TryIf(queue, entry, beforeargs))
                {
                    return false;
                }
                List<Argument> afterargs = new(arguments.Count - i);
                for (int x = i + 1; x < arguments.Count; x++)
                {
                    afterargs.Add(arguments[x]);
                }
                return TryIf(queue, entry, afterargs);
            }
        }
        if (arguments.Count == 2)
        {
            queue.HandleError(entry, "Invalid IF: Two-argument input unclear in intent!");
            return false;
        }
        TemplateObject arg1 = arguments[0].Parse(queue.Error, queue.CurrentRunnable);
        NumberTag n1 = NumberTag.TryFor(arg1);
        TemplateObject arg2 = arguments[2].Parse(queue.Error, queue.CurrentRunnable);
        NumberTag n2 = NumberTag.TryFor(arg2);
        string comp = arguments[1].ToString();
        if (comp == "==")
        {
            if (n1 != null && n2 != null)
            {
                return n1.Internal == n2.Internal;
            }
            return arg1.ToString() == arg2.ToString();
        }
        else if (comp == "!=")
        {
            if (n1 != null && n2 != null)
            {
                return n1.Internal != n2.Internal;
            }
            return arg1.ToString() != arg2.ToString();
        }
        else if (comp == ">=")
        {
            return n1.Internal >= n2.Internal;
        }
        else if (comp == "<=")
        {
            return n1.Internal <= n2.Internal;
        }
        else if (comp == ">")
        {
            return n1.Internal > n2.Internal;
        }
        else if (comp == "<")
        {
            return n1.Internal < n2.Internal;
        }
        else
        {
            queue.HandleError(entry, "Invalid IF: Unknown comparison system!");
            return false;
        }
    }
}
