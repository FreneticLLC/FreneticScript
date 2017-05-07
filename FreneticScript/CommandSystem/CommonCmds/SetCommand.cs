using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.CommandSystem;
using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Objects;

namespace FreneticScript.CommandSystem.CommonCmds
{
    public class SetCommand : AbstractCommand
    {
        // TODO: Meta!

        /// <summary>
        /// Constructs the set command.
        /// </summary>
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
                TextTag.For,
                TextTag.For,
                Verify
            };
        }

        TemplateObject Verify(TemplateObject input)
        {
            string inp = input.ToString().ToLowerFast();
            if (inp == "force" || inp == "remove" || inp == "do_not_save")
            {
                return new TextTag(inp);
            }
            return null;
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="queue">The command queue involved.</param>
        /// <param name="entry">Entry to be executed.</param>
        public static void Execute(CommandQueue queue, CommandEntry entry)
        {
            string target = entry.GetArgument(queue, 0);
            string newvalue = entry.GetArgument(queue, 1);
            string a2 = entry.Arguments.Count > 2 ? entry.GetArgument(queue, 2).ToLowerFast() : "";
            bool force = a2 == "force";
            bool remove = a2 == "remove";
            bool do_not_save = a2 == "do_not_save";
            if (remove)
            {
                CVar Cvar = queue.CommandSystem.Output.CVarSys.Get(target);
                if (Cvar == null)
                {
                    if (entry.ShouldShowGood(queue))
                    {
                        entry.Good(queue, "CVar '<{text_color[emphasis]}>" + TagParser.Escape(target) + "<{text_color[base]}>' cannot be removed, it doesn't exist!");
                    }
                }
                else if (!Cvar.Flags.HasFlag(CVarFlag.UserMade))
                {
                    queue.HandleError(entry, "CVar '<{text_color[emphasis]}>" + TagParser.Escape(Cvar.Name) + "<{text_color[base]}>' cannot be removed, it wasn't user made!");
                }
                else
                {
                    Cvar.Set("");
                    queue.CommandSystem.Output.CVarSys.CVars.Remove(Cvar.Name);
                    queue.CommandSystem.Output.CVarSys.CVarList.Remove(Cvar);
                    if (entry.ShouldShowGood(queue))
                    {
                        entry.Good(queue, "<{text_color[info]}>CVar '<{text_color[emphasis]}>" + TagParser.Escape(Cvar.Name) + "<{text_color[info]}>' removed.");
                    }
                }
                return;
            }
            CVar cvar = queue.CommandSystem.Output.CVarSys.AbsoluteSet(target, newvalue, force, do_not_save ? CVarFlag.DoNotSave : CVarFlag.None);
            if (cvar.Flags.HasFlag(CVarFlag.ServerControl) && !force)
            {
                queue.HandleError(entry, "CVar '<{text_color[emphasis]}>" + TagParser.Escape(cvar.Name) + "<{text_color[base]}>' cannot be modified, it is server controlled!");
                return;
            }
            if (cvar.Flags.HasFlag(CVarFlag.ReadOnly))
            {
                queue.HandleError(entry, "CVar '<{text_color[emphasis]}>" + TagParser.Escape(cvar.Name) + "<{text_color[base]}>' cannot be modified, it is a read-only system variable!");
            }
            else if (cvar.Flags.HasFlag(CVarFlag.InitOnly) && !queue.CommandSystem.Output.Initializing)
            {
                queue.HandleError(entry, "CVar '<{text_color[emphasis]}>" + TagParser.Escape(cvar.Name) + "<{text_color[base]}>' cannot be modified after game initialization.");
            }
            else if (cvar.Flags.HasFlag(CVarFlag.Delayed) && !queue.CommandSystem.Output.Initializing)
            {
                entry.Good(queue, "CVar '<{text_color[emphasis]}>" + TagParser.Escape(cvar.Name) + "<{text_color[base]}>' is delayed, and its value will be calculated after the game is reloaded.");
            }
            else
            {
                if (entry.ShouldShowGood(queue))
                {
                    entry.Good(queue, "CVar '<{text_color[emphasis]}>" + TagParser.Escape(cvar.Name) + "<{text_color[base]}>' set to '<{text_color[emphasis]}>" + TagParser.Escape(cvar.Value) + "<{text_color[base]}>'.");
                }
            }
        }
    }
}
