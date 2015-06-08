using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Frenetic.CommandSystem;
using Frenetic.TagHandlers;

namespace Frenetic.CommandSystem.CommonCmds
{
    class SetCommand: AbstractCommand
    {
        public SetCommand()
        {
            Name = "set";
            Arguments = "<CVar to set> <new value> (force/remove)";
            Description = "Modifies the value of a specified CVar, or creates a new one.";
            // TODO: make asyncable
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="entry">Entry to be executed</param>
        public override void Execute(CommandEntry entry)
        {
            if (entry.Arguments.Count < 2)
            {
                ShowUsage(entry);
            }
            else
            {
                string target = entry.GetArgument(0);
                string newvalue = entry.GetArgument(1);
                string a2 = entry.Arguments.Count > 2 ? entry.GetArgument(2).ToLower(): "";
                bool force = a2 == "force";
                bool remove = a2 == "remove";
                if (remove)
                {
                    CVar Cvar = entry.Output.CVarSys.Get(target);
                    if (Cvar == null)
                    {
                        entry.Good("CVar '<{color.emphasis}>" + TagParser.Escape(target)
                            + "<{color.base}>' cannot be removed, it doesn't exist!");
                    }
                    else if (!Cvar.Flags.HasFlag(CVarFlag.UserMade))
                    {
                        entry.Bad("CVar '<{color.emphasis}>" + TagParser.Escape(Cvar.Name)
                            + "<{color.base}>' cannot be removed, it wasn't user made!");
                    }
                    else
                    {
                        Cvar.Set("");
                        entry.Output.CVarSys.CVars.Remove(Cvar.Name);
                        entry.Output.CVarSys.CVarList.Remove(Cvar);
                        entry.Good("<{color.info}>CVar '<{color.emphasis}>" + TagParser.Escape(Cvar.Name) +
                            "<{color.info}>' removed.");
                    }
                    return;
                }
                CVar cvar = entry.Output.CVarSys.AbsoluteSet(target, newvalue);
                if (cvar.Flags.HasFlag(CVarFlag.ServerControl) && !force)
                {
                    entry.Bad("CVar '<{color.emphasis}>" + TagParser.Escape(cvar.Name)
                        + "<{color.base}>' cannot be modified, it is server controlled!");
                    return;
                }
                if (cvar.Flags.HasFlag(CVarFlag.ReadOnly))
                {
                    entry.Bad("CVar '<{color.emphasis}>" + TagParser.Escape(cvar.Name)
                        + "<{color.base}>' cannot be modified, it is a read-only system variable!");
                }
                else if (cvar.Flags.HasFlag(CVarFlag.InitOnly) && !entry.Output.Initializing)
                {
                    entry.Bad("CVar '<{color.emphasis}>" + TagParser.Escape(cvar.Name)
                        + "<{color.base}>' cannot be modified after game initialization.");
                }
                else if (cvar.Flags.HasFlag(CVarFlag.Delayed) && !entry.Output.Initializing)
                {
                    entry.Good("<{color.info}>CVar '<{color.emphasis}>" + TagParser.Escape(cvar.Name) +
                        "<{color.info}>' is delayed, and its value will be calculated after the game is reloaded.");
                }
                else
                {
                    entry.Good("<{color.info}>CVar '<{color.emphasis}>" + TagParser.Escape(cvar.Name) +
                        "<{color.info}>' set to '<{color.emphasis}>" + TagParser.Escape(cvar.Value) + "<{color.info}>'.");
                }
            }
        }
    }
}
