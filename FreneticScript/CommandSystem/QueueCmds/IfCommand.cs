using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.CommandSystem.Arguments;
using FreneticScript.TagHandlers;
using System.Reflection;
using System.Reflection.Emit;

namespace FreneticScript.CommandSystem.QueueCmds
{
    class IfCommandData : AbstractCommandEntryData
    {
        public int Result;
    }

    /// <summary>
    /// The if command.
    /// </summary>
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

        /// <summary>
        /// Construct the if commnad.
        /// </summary>
        public IfCommand()
        {
            Name = "if";
            Arguments = "<comparisons>";
            Description = "Executes the following block of commands only if the input is true.";
            IsFlow = true;
            Asyncable = true;
            MinimumArguments = 1;
            MaximumArguments = -1;
            ObjectTypes = new List<Func<TemplateObject, TemplateObject>>();
        }

        /// <summary>
        /// Represents the "TryIfCIL(queue, entry)" method.
        /// </summary>
        public static MethodInfo TryIfCILMethod = typeof(IfCommand).GetMethod("TryIfCIL", new Type[] { typeof(CommandQueue), typeof(CommandEntry) });

        /// <summary>
        /// Adapts a command entry to CIL.
        /// </summary>
        /// <param name="values">The adaptation-relevant values.</param>
        /// <param name="entry">The present entry ID.</param>
        public override void AdaptToCIL(CILAdaptationValues values, int entry)
        {
            CommandEntry cent = values.Entry.Entries[entry];
            if (cent.Arguments[0].ToString() == "\0CALLBACK")
            {
                values.ILGen.Emit(OpCodes.Nop);
                return;
            }
            if (cent.BlockEnd <= 0)
            {
                throw new Exception("Incorrectly defined IF command: no block follows!");
            }
            values.PrepareExecutionCall(entry);
            values.ILGen.Emit(OpCodes.Callvirt, TryIfCILMethod);
            values.ILGen.Emit(OpCodes.Brfalse, values.Entry.AdaptedILPoints[cent.BlockEnd + 2]);
        }

        /// <summary>
        /// Executes the command via CIL.
        /// </summary>
        /// <param name="queue">The command queue involved.</param>
        /// <param name="entry">Entry to be executed.</param>
        public bool TryIfCIL(CommandQueue queue, CommandEntry entry)
        {
            entry.SetData(queue, new IfCommandData() { Result = 0 });
            List<string> parsedargs = new List<string>(entry.Arguments.Count);
            for (int i = 0; i < entry.Arguments.Count; i++)
            {
                parsedargs.Add(entry.GetArgument(queue, i)); // TODO: Don't pre-parse. Parse in TryIf.
            }
            bool success = TryIf(parsedargs);
            if (success)
            {
                if (entry.ShouldShowGood(queue))
                {
                    entry.Good(queue, "If is true, executing...");
                }
                ((IfCommandData)entry.GetData(queue)).Result = 1;
            }
            else
            {
                if (entry.ShouldShowGood(queue))
                {
                    entry.Good(queue, "If is false, doing nothing!");
                }
            }
            return success;
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
                CommandStackEntry cse = queue.CommandStack.Peek();
                CommandEntry ifentry = cse.Entries[entry.BlockStart - 1];
                if (cse.Index < cse.Entries.Length)
                {
                    CommandEntry elseentry = cse.Entries[cse.Index];
                    if (elseentry.Command is ElseCommand)
                    {
                        elseentry.SetData(queue, ifentry.GetData(queue));
                    }
                }
                return;
            }
            bool success = TryIfCIL(queue, entry);
            if (!success)
            {
                queue.CommandStack.Peek().Index = entry.BlockEnd + 1;
            }
        }

        // TODO: better comparison system!
        /// <summary>
        /// Tries input IF to see if it is TRUE or FALSE.
        /// </summary>
        /// <param name="arguments">The input arguments.</param>
        /// <returns>Whether it is true or not.</returns>
        public static bool TryIf(List<string> arguments)
        {
            if (arguments.Count == 0)
            {
                return false;
            }
            if (arguments.Count == 1)
            {
                return arguments[0].ToLowerFast() == "true";
            }
            for (int i = 0; i < arguments.Count; i++)
            {
                if (arguments[i] == "(")
                {
                    List<string> subargs = new List<string>();
                    int count = 0;
                    bool found = false;
                    for (int x = i + 1; x < arguments.Count; x++)
                    {
                        if (arguments[x] == "(")
                        {
                            count++;
                            subargs.Add("(");
                        }
                        else if (arguments[x] == ")")
                        {
                            count--;
                            if (count == -1)
                            {
                                bool cfound = TryIf(subargs);
                                arguments.RemoveRange(i, (x - i) + 1);
                                arguments.Insert(i, cfound.ToString());
                                found = true;
                            }
                            else
                            {
                                subargs.Add(")");
                            }
                        }
                        else
                        {
                            subargs.Add(arguments[x]);
                        }
                    }
                    if (!found)
                    {
                        return false;
                    }
                }
                else if (arguments[i] == ")")
                {
                    return false;
                }
            }
            if (arguments.Count == 1)
            {
                return arguments[0].ToLowerFast() == "true";
            }
            for (int i = 0; i < arguments.Count; i++)
            {
                if (arguments[i] == "||")
                {
                    List<string> beforeargs = new List<string>(i);
                    for (int x = 0; x < i; x++)
                    {
                        beforeargs.Add(arguments[x]);
                    }
                    bool before = TryIf(beforeargs);
                    List<string> afterargs = new List<string>(i);
                    for (int x = i + 1; x < arguments.Count; x++)
                    {
                        afterargs.Add(arguments[x]);
                    }
                    bool after = TryIf(afterargs);
                    return before || after;
                }
                else if (arguments[i] == "&&")
                {
                    List<string> beforeargs = new List<string>(i);
                    for (int x = 0; x < i; x++)
                    {
                        beforeargs.Add(arguments[x]);
                    }
                    bool before = TryIf(beforeargs);
                    List<string> afterargs = new List<string>(i);
                    for (int x = i + 1; x < arguments.Count; x++)
                    {
                        afterargs.Add(arguments[x]);
                    }
                    bool after = TryIf(afterargs);
                    return before && after;
                }
            }
            if (arguments.Count == 1)
            {
                return arguments[0].ToLowerFast() == "true";
            }
            if (arguments.Count == 2)
            {
                return false;
            }
            if (arguments[1] == "==")
            {
                return arguments[0] == arguments[2];
            }
            else if (arguments[1] == "!=")
            {
                return arguments[0] != arguments[2];
            }
            else if (arguments[1] == ">=")
            {
                return FreneticScriptUtilities.StringToDouble(arguments[0]) >= FreneticScriptUtilities.StringToDouble(arguments[2]);
            }
            else if (arguments[1] == "<=")
            {
                return FreneticScriptUtilities.StringToDouble(arguments[0]) <= FreneticScriptUtilities.StringToDouble(arguments[2]);
            }
            else if (arguments[1] == ">")
            {
                return FreneticScriptUtilities.StringToDouble(arguments[0]) > FreneticScriptUtilities.StringToDouble(arguments[2]);
            }
            else if (arguments[1] == "<")
            {
                return FreneticScriptUtilities.StringToDouble(arguments[0]) < FreneticScriptUtilities.StringToDouble(arguments[2]);
            }
            else
            {
                return false;
            }
        }
    }
}
