using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.CommandSystem;
using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Objects;

namespace FreneticScript.CommandSystem.CommonCmds
{
    class SetCommand : AbstractCommand
    {
        // TODO: Meta!
        public SetCommand()
        {
            Name = "set";
            Arguments = "<CVar to set> <new value> ['force'/'remove'/'do_not_save']";
            Description = "Modifies the value of a specified CVar, or creates a new one.";
            // TODO: make asyncable? Probably with a CVar system lock?
            MinimumArguments = 2;
            MaximumArguments = 3;
            ObjectTypes = new List<Func<TemplateObject, TemplateObject>>()
            {
                (input) =>
                {
                    return new TextTag(input.ToString());
                },
                (input) =>
                {
                    return new TextTag(input.ToString());
                },
                (input) =>
                {
                    string inp = input.ToString().ToLowerFast();
                    if (inp == "force" || inp == "remove" || inp == "do_not_save")
                    {
                        return new TextTag(inp);
                    }
                    return null;
                }
            };
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="entry">Entry to be executed.</param>
        public override void Execute(CommandEntry entry)
        {
            string target = entry.GetArgument(0);
            string newvalue = entry.GetArgument(1);
            string a2 = entry.Arguments.Count > 2 ? entry.GetArgument(2).ToLowerFast() : "";
            bool force = a2 == "force";
            bool remove = a2 == "remove";
            bool do_not_save = a2 == "do_not_save";
            if (remove)
            {
                CVar Cvar = entry.Output.CVarSys.Get(target);
                if (Cvar == null)
                {
                    if (entry.ShouldShowGood())
                    {
                        entry.Good("CVar '<{text_color.emphasis}>" + TagParser.Escape(target) + "<{text_color.base}>' cannot be removed, it doesn't exist!");
                    }
                }
                else if (!Cvar.Flags.HasFlag(CVarFlag.UserMade))
                {
                    entry.Error("CVar '<{text_color.emphasis}>" + TagParser.Escape(Cvar.Name) + "<{text_color.base}>' cannot be removed, it wasn't user made!");
                }
                else
                {
                    Cvar.Set("");
                    entry.Output.CVarSys.CVars.Remove(Cvar.Name);
                    entry.Output.CVarSys.CVarList.Remove(Cvar);
                    if (entry.ShouldShowGood())
                    {
                        entry.Good("<{text_color.info}>CVar '<{text_color.emphasis}>" + TagParser.Escape(Cvar.Name) + "<{text_color.info}>' removed.");
                    }
                }
                return;
            }
            CVar cvar = entry.Output.CVarSys.AbsoluteSet(target, newvalue, force, do_not_save ? CVarFlag.DoNotSave : CVarFlag.None);
            if (cvar.Flags.HasFlag(CVarFlag.ServerControl) && !force)
            {
                entry.Error("CVar '<{text_color.emphasis}>" + TagParser.Escape(cvar.Name) + "<{text_color.base}>' cannot be modified, it is server controlled!");
                return;
            }
            if (cvar.Flags.HasFlag(CVarFlag.ReadOnly))
            {
                entry.Error("CVar '<{text_color.emphasis}>" + TagParser.Escape(cvar.Name) + "<{text_color.base}>' cannot be modified, it is a read-only system variable!");
            }
            else if (cvar.Flags.HasFlag(CVarFlag.InitOnly) && !entry.Output.Initializing)
            {
                entry.Error("CVar '<{text_color.emphasis}>" + TagParser.Escape(cvar.Name) + "<{text_color.base}>' cannot be modified after game initialization.");
            }
            else if (cvar.Flags.HasFlag(CVarFlag.Delayed) && !entry.Output.Initializing)
            {
                entry.Good("CVar '<{text_color.emphasis}>" + TagParser.Escape(cvar.Name) + "<{text_color.base}>' is delayed, and its value will be calculated after the game is reloaded.");
            }
            else
            {
                if (entry.ShouldShowGood())
                {
                    entry.Good("CVar '<{text_color.emphasis}>" + TagParser.Escape(cvar.Name) + "<{text_color.base}>' set to '<{text_color.emphasis}>" + TagParser.Escape(cvar.Value) + "<{text_color.base}>'.");
                }
            }
        }
    }
}
