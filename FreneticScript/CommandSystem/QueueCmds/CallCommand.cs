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
    // <{var[determinations]}> returns what the called function determined, if anything.
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

        public override void Execute(CommandQueue queue, CommandEntry entry)
        {
            string fname = entry.GetArgument(queue, 0);
            fname = fname.ToLowerFast();
            CommandScript script = queue.CommandSystem.GetFunction(fname);
            if (script == null)
            {
                queue.HandleError(entry, "Cannot call function '<{text_color.emphasis}>" + TagParser.Escape(fname) + "<{text_color.base}>': it does not exist!");
                return;
            }
            if (entry.ShouldShowGood(queue))
            {
                entry.Good(queue, "Calling '<{text_color.emphasis}>" + TagParser.Escape(fname) + "<{text_color.base}>'...");
            }
            Dictionary<string, TemplateObject> vars = new Dictionary<string, TemplateObject>();
            foreach (string var in entry.NamedArguments.Keys)
            {
                vars[var] = entry.GetNamedArgumentObject(queue, var);
            }
            queue.PushToStack(script.Commands, DebugMode.MINIMAL, vars);
        }
    }
}
