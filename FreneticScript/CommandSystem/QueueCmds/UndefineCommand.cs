using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.CommandSystem;
using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Objects;

namespace FreneticScript.CommandSystem.CommonCmds
{
    class UndefineCommand : AbstractCommand
    {
        public UndefineCommand()
        {
            Name = "undefine";
            Arguments = "<variable to remove>";
            Description = "Removes the specified queue variable.";
            IsFlow = true;
            Asyncable = true;
            MinimumArguments = 1;
            MaximumArguments = 1;
            ObjectTypes = new List<Func<TemplateObject, TemplateObject>>()
            {
                (input) =>
                {
                    return new TextTag(input.ToString());
                }
            };
        }

        public override void Execute(CommandEntry entry)
        {
            string target = entry.GetArgument(0);
            if (entry.Queue.Variables.ContainsKey(target.ToLowerFast()))
            {
                entry.Queue.Variables.Remove(target.ToLowerFast());
                if (entry.ShouldShowGood())
                {
                    entry.Good("Queue variable '<{text_color.emphasis}>" + TagParser.Escape(target.ToLowerFast()) + "<{text_color.base}>' removed'.");
                }
            }
            else
            {
                entry.Error("Unknown queue variable '<{text_color.emphasis}>" + TagParser.Escape(target.ToLowerFast()) + "<{text_color.base}>'.");
            }
        }
    }
}
