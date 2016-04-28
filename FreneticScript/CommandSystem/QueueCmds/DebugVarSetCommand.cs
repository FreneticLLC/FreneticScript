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
                (input) => new TextTag(input.ToString()),
                (input) => new TextTag(input.ToString()),
                (input) => input
            };
        }

        /// <summary>
        /// Executs the command.
        /// </summary>
        /// <param name="queue">The command queue involved.</param>
        /// <param name="entry">The entry to execute with.</param>
        public override void Execute(CommandQueue queue, CommandEntry entry)
        {
            string var = entry.GetArgument(queue, 0);
            string[] dat = var.SplitFast('.');
            TemplateObject tvar = queue.GetVariable(dat[0]);
            if (tvar == null)
            {
                queue.HandleError(entry, "Unknown variable '" + TagParser.Escape(dat[0]) + "'.");
                return;
            }
            string[] sdat = new string[dat.Length - 1];
            Array.Copy(dat, 1, sdat, 0, sdat.Length);
            string setter = entry.GetArgument(queue, 1);
            TemplateObject val = entry.GetArgumentObject(queue, 2);
            switch (setter)
            {
                case "=":
                    tvar.Set(sdat, val);
                    break;
                case "+=":
                    tvar.Add(sdat, val);
                    break;
                case "_=":
                    tvar.Subtract(sdat, val);
                    break;
                case "*=":
                    tvar.Multiply(sdat, val);
                    break;
                case "/=":
                    tvar.Divide(sdat, val);
                    break;
            }
        }
    }
}
