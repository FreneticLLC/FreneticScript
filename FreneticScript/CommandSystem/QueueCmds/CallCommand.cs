using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Objects;

namespace FreneticScript.CommandSystem.QueueCmds
{
    // <--[command]
    // @Name call
    // @Arguments <function to call> [-variable value ...]
    // @Short Runs a function.
    // @Updated 2014/06/23
    // @Authors mcmonkey
    // @Group Queue
    // @Minium 1
    // @Maximum -1
    // @Description
    // Activates a function created by the <@link command function>function<@/link> command.
    // TODO: Explain more!
    // @Example
    // // This example calls the function 'helloworld'.
    // call helloworld
    // @Example
    // // This example calls the function 'outputme' with variable 'text' set to 'hello world'.
    // call outputme -text "hello world"
    // @Example
    // TODO: More examples!
    // @Tags
    // <{var[call_determinations]}> returns what the called function determined, if anything.
    // @BlockVars
    // call_determinations ListTag
    // -->

    class CallCommand : AbstractCommand
    {
        public CallCommand()
        {
            Name = "call";
            Arguments = "<function to call> [-<variable> <value> ...]";
            Description = "Runs a function.";
            IsFlow = true;
            Asyncable = true;
            MinimumArguments = 1;
            MaximumArguments = -1;
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
            string fname = entry.GetArgument(0);
            fname = fname.ToLowerFast();
            CommandScript script = entry.Queue.CommandSystem.GetFunction(fname);
            if (script == null)
            {
                entry.Error("Cannot call function '<{text_color.emphasis}>" + TagParser.Escape(fname) + "<{text_color.base}>': it does not exist!");
                return;
            }
            if (entry.ShouldShowGood())
            {
                entry.Good("Calling '<{text_color.emphasis}>" + TagParser.Escape(fname) + "<{text_color.base}>'...");
            }
            CommandQueue queue = script.ToQueue(entry.Command.CommandSystem);
            foreach (string var in entry.NamedArguments.Keys)
            {
                queue.SetVariable(var, entry.GetNamedArgumentObject(var));
            }
            queue.Debug = DebugMode.MINIMAL;
            queue.Outputsystem = entry.Queue.Outputsystem;
            queue.Execute();
            if (entry.WaitFor && entry.Queue.WaitingOn != null)
            {
                if (!queue.Running)
                {
                    entry.Queue.WaitingOn = null;
                }
                else
                {
                    EntryFinisher fin = new EntryFinisher() { Entry = entry };
                    queue.Complete += new EventHandler<CommandQueueEventArgs>(fin.Complete);
                }
            }
            ListTag list = new ListTag(queue.Determinations);
            entry.Queue.SetVariable("call_determinations", list);
        }
    }
}
