using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Objects;
using System.Reflection;
using System.Reflection.Emit;

namespace FreneticScript.CommandSystem.QueueCmds
{
    /// <summary>
    /// Sets a var, debuggishly!
    /// </summary>
    public class DebugVarSetCommand : AbstractCommand
    {
        // NOTE: Intentionally no meta!
        
        /// <summary>
        /// Adapts a command entry to CIL.
        /// </summary>
        /// <param name="values">The adaptation-relevant values.</param>
        /// <param name="entry">The present entry ID.</param>
        public override void AdaptToCIL(CILAdaptationValues values, int entry)
        {
            // TODO: Debug this!
            // TODO: Type verification?
            values.MarkCommand(entry);
            CommandEntry cent = values.Entry.Entries[entry];
            string vn = cent.Arguments[0].ToString().ToLowerFast();
            string[] dat = vn.SplitFast('.');
            StringBuilder res = new StringBuilder(vn.Length);
            for (int i = 1; i < dat.Length; i++)
            {
                res.Append(dat[i]);
                if (i + 1 < dat.Length)
                {
                    res.Append('.');
                }
            }
            vn = dat[0];
            int lvarloc = cent.VarLoc(vn);
            string mode = cent.Arguments[1].ToString();
            values.ILGen.Emit(OpCodes.Ldc_I4, lvarloc);
            values.ILGen.Emit(OpCodes.Ldstr, res.ToString());
            values.LoadQueue();
            values.LoadEntry(entry);
            switch (mode)
            {
                case "=":
                    values.ILGen.Emit(OpCodes.Call, Method_SetImmediate);
                    break;
                case "+=":
                    values.ILGen.Emit(OpCodes.Call, Method_AddImmediate);
                    break;
                case "-=":
                    values.ILGen.Emit(OpCodes.Call, Method_SubtractImmediate);
                    break;
                case "*=":
                    values.ILGen.Emit(OpCodes.Call, Method_MultiplyImmediate);
                    break;
                case "/=":
                    values.ILGen.Emit(OpCodes.Call, Method_DivideImmediate);
                    break;
                default:
                    throw new NotSupportedException("That setter mode (" + mode + ") is not available!");
            }
        }

        static MethodInfo Method_SetImmediate = typeof(DebugVarSetCommand).GetMethod("SetImmediate");
        static MethodInfo Method_AddImmediate = typeof(DebugVarSetCommand).GetMethod("AddImmediate");
        static MethodInfo Method_SubtractImmediate = typeof(DebugVarSetCommand).GetMethod("SubtractImmediate");
        static MethodInfo Method_MultiplyImmediate = typeof(DebugVarSetCommand).GetMethod("MultiplyImmediate");
        static MethodInfo Method_DivideImmediate = typeof(DebugVarSetCommand).GetMethod("DivideImmediate");

        /// <summary>
        /// Immediately sets a var, for compiler reasons.
        /// </summary>
        /// <param name="loc">The var location.</param>
        /// <param name="sdat">The split data variable.</param>
        /// <param name="queue">The relevant queue.</param>
        /// <param name="entry">The relevant entry.</param>
        public static void SetImmediate(int loc, string sdat, CommandQueue queue, CommandEntry entry)
        {
            (queue.CommandStack.Peek() as CompiledCommandStackEntry).LocalVariables[loc].Internal.Set(sdat.Length == 0 ? new string[0] : sdat.SplitFast('.'), entry.GetArgumentObject(queue, 2));
        }

        /// <summary>
        /// Immediately adds a var, for compiler reasons.
        /// </summary>
        /// <param name="loc">The var location.</param>
        /// <param name="sdat">The split data variable.</param>
        /// <param name="queue">The relevant queue.</param>
        /// <param name="entry">The relevant entry.</param>
        public static void AddImmediate(int loc, string sdat, CommandQueue queue, CommandEntry entry)
        {
            (queue.CommandStack.Peek() as CompiledCommandStackEntry).LocalVariables[loc].Internal.Add(sdat.Length == 0 ? new string[0] : sdat.SplitFast('.'), entry.GetArgumentObject(queue, 2));
        }

        /// <summary>
        /// Immediately subtracts a var, for compiler reasons.
        /// </summary>
        /// <param name="loc">The var location.</param>
        /// <param name="sdat">The split data variable.</param>
        /// <param name="queue">The relevant queue.</param>
        /// <param name="entry">The relevant entry.</param>
        public static void SubtractImmediate(int loc, string sdat, CommandQueue queue, CommandEntry entry)
        {
            (queue.CommandStack.Peek() as CompiledCommandStackEntry).LocalVariables[loc].Internal.Subtract(sdat.Length == 0 ? new string[0] : sdat.SplitFast('.'), entry.GetArgumentObject(queue, 2));
        }

        /// <summary>
        /// Immediately multiplies a var, for compiler reasons.
        /// </summary>
        /// <param name="loc">The var location.</param>
        /// <param name="sdat">The split data variable.</param>
        /// <param name="queue">The relevant queue.</param>
        /// <param name="entry">The relevant entry.</param>
        public static void MultiplyImmediate(int loc, string sdat, CommandQueue queue, CommandEntry entry)
        {
            (queue.CommandStack.Peek() as CompiledCommandStackEntry).LocalVariables[loc].Internal.Multiply(sdat.Length == 0 ? new string[0] : sdat.SplitFast('.'), entry.GetArgumentObject(queue, 2));
        }

        /// <summary>
        /// Immediately divides a var, for compiler reasons.
        /// </summary>
        /// <param name="loc">The var location.</param>
        /// <param name="sdat">The split data variable.</param>
        /// <param name="queue">The relevant queue.</param>
        /// <param name="entry">The relevant entry.</param>
        public static void DIvideImmediate(int loc, string sdat, CommandQueue queue, CommandEntry entry)
        {
            (queue.CommandStack.Peek() as CompiledCommandStackEntry).LocalVariables[loc].Internal.Divide(sdat.Length == 0 ? new string[0] : sdat.SplitFast('.'), entry.GetArgumentObject(queue, 2));
        }

        /// <summary>
        /// Constructs the command.
        /// </summary>
        public DebugVarSetCommand()
        {
            Name = "\0DebugVarSet";
            Arguments = "<invalid command name>";
            Description = "Sets or modifies a variable.";
            IsDebug = true;
            IsFlow = true;
            Asyncable = true;
            MinimumArguments = 3;
            MaximumArguments = 3;
            ObjectTypes = new List<Func<TemplateObject, TemplateObject>>()
            {
                PreLowerBaseVar,
                TextTag.For,
                TemplateObject.Basic_For
            };
        }

        /// <summary>
        /// Pre-lowers the base variable.
        /// </summary>
        /// <param name="inp">The input variable block.</param>
        public TemplateObject PreLowerBaseVar(TemplateObject inp)
        {
            string ins = inp.ToString();
            string[] dat = ins.SplitFast('.');
            dat[0] = dat[0].ToLowerFast();
            StringBuilder sb = new StringBuilder(ins.Length);
            for (int i = 0; i < dat.Length; i++)
            {
                sb.Append(dat[i]);
                if (i + 1 < dat.Length)
                {
                    sb.Append('.');
                }
            }
            return new TextTag(sb.ToString());
        }
        
        /// <summary>
        /// Executs the command.
        /// </summary>
        /// <param name="queue">The command queue involved.</param>
        /// <param name="entry">The entry to execute with.</param>
        public override void Execute(CommandQueue queue, CommandEntry entry)
        {
            queue.HandleError(entry, "This command MUST be compiled!");
        }
    }
}
