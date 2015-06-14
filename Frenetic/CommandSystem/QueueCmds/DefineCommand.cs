using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Frenetic.CommandSystem;
using Frenetic.TagHandlers;
using Frenetic.TagHandlers.Objects;

namespace Frenetic.CommandSystem.CommonCmds
{
    // <--[command]
    // @Name define
    // @Arguments <variable to set> <new value>
    // @Short Modifies the value of a specified queue variable, or creates a new one.
    // @Updated 2014/06/22
    // @Authors mcmonkey
    // @Group Queue
    // @Description
    // The define command sets a <@link explanation Queue Variables>queue variable<@/link>
    // onto the queue it is running in.
    // TODO: Explain more!
    // @Example
    // // This example sets variable "name" to "value"
    // define name value
    // @Example
    // TODO: More examples!
    // Var <Dynamic> TextTag returns the value of the set definition.
    // -->
    class DefineCommand: AbstractCommand
    {
        public DefineCommand()
        {
            Name = "define";
            Arguments = "<Variable to set> <new value>";
            Description = "Modifies the value of a specified queue variable, or creates a new one.";
            IsFlow = true;
            Asyncable = true;
        }

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
                entry.Queue.SetVariable(target, new TextTag(newvalue));
                entry.Good("Queue variable '<{color.emphasis}>" + TagParser.Escape(target.ToLower()) +
                    "<{color.base}>' set to '<{color.emphasis}>" + TagParser.Escape(newvalue) + "<{color.base}>'.");
            }
        }
    }
}
