using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Objects;

namespace FreneticScript.CommandSystem.QueueCmds
{
    /// <summary>
    /// Sets a var, debuggishly!
    /// </summary>
    public class DebugVarSetCommand : AbstractCommand
    {
        // NOTE: Intentionally no meta!

        // TODO: Compile this as much as possible!

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

        public static Dictionary<string, Action<TemplateObject, string[], TemplateObject>> Setters = new Dictionary<string, Action<TemplateObject, string[], TemplateObject>>();

        static DebugVarSetCommand()
        {
            Setters["="] = SetEqual;
            Setters["+="] = SetAdd;
            Setters["-="] = SetSubtract;
            Setters["*="] = SetMultiply;
            Setters["/="] = SetDivide;
        }

        static void SetEqual(TemplateObject tvar, string[] sdat, TemplateObject val)
        {
            tvar.Set(sdat, val);
        }

        static void SetAdd(TemplateObject tvar, string[] sdat, TemplateObject val)
        {
            tvar.Add(sdat, val);
        }

        static void SetSubtract(TemplateObject tvar, string[] sdat, TemplateObject val)
        {
            tvar.Subtract(sdat, val);
        }

        static void SetMultiply(TemplateObject tvar, string[] sdat, TemplateObject val)
        {
            tvar.Multiply(sdat, val);
        }

        static void SetDivide(TemplateObject tvar, string[] sdat, TemplateObject val)
        {
            tvar.Divide(sdat, val);
        }

        /// <summary>
        /// Executs the command.
        /// </summary>
        /// <param name="queue">The command queue involved.</param>
        /// <param name="entry">The entry to execute with.</param>
        public override void Execute(CommandQueue queue, CommandEntry entry)
        {
            string var = entry.Arguments[0].ToString();
            string[] dat = var.SplitFast('.');
            TemplateObject tvar = queue.GetVariablePreLowered(dat[0]);
            if (tvar == null)
            {
                queue.HandleError(entry, "Unknown variable '" + TagParser.Escape(dat[0]) + "'.");
                return;
            }
            string[] sdat = new string[dat.Length - 1];
            if (sdat.Length > 0)
            {
                Array.Copy(dat, 1, sdat, 0, sdat.Length);
            }
            string setter = entry.Arguments[1].ToString();
            TemplateObject val = entry.GetArgumentObject(queue, 2);
            Action<TemplateObject, string[], TemplateObject> handler;
            if (Setters.TryGetValue(setter, out handler))
            {
                handler(tvar, sdat, val);
            }
            else
            {
                queue.HandleError(entry, "Invalid var setter: " + setter);
            }
        }
    }
}
