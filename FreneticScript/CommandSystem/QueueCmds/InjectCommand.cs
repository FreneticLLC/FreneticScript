using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Objects;

namespace FreneticScript.CommandSystem.QueueCmds
{
    // <--[command]
    // @Name inject
    // @Arguments <function to inject>
    // @Short Runs a function inside the current variable base.
    // @Updated 2016/04/28
    // @Authors mcmonkey
    // @Group Queue
    // @Minimum 1
    // @Maximum 1
    // @Description
    // Injects a function created by the <@link command function>function<@/link> command.
    // IMPORTANT NOTE: Generally, the <@link command call>call<@/link> command should be used instead of this.
    // TODO: Explain more!
    // @Example
    // // This example injects the function 'helloworld'.
    // inject helloworld;
    // @Example
    // TODO: More examples!
    // -->

    // TODO: Remove? Make compatible with CIL update somehow?

    class InjectCommand : AbstractCommand
    {
        public InjectCommand()
        {
            Name = "inject";
            Arguments = "<function to call>";
            Description = "Runs a function.";
            IsFlow = true;
            Asyncable = true;
            MinimumArguments = 1;
            MaximumArguments = 1;
            ObjectTypes = new List<Func<TemplateObject, TemplateObject>>()
            {
                TextTag.For
            };
        }

        public override void Execute(CommandQueue queue, CommandEntry entry)
        {
            string fname = entry.GetArgument(queue, 0);
            fname = fname.ToLowerFast();
            CommandScript script = queue.CommandSystem.GetFunction(fname);
            if (script == null)
            {
                queue.HandleError(entry, "Cannot inject function '<{text_color[emphasis]}>" + TagParser.Escape(fname) + "<{text_color[base]}>': it does not exist!");
                return;
            }
            if (entry.ShouldShowGood(queue))
            {
                entry.Good(queue, "Injecting '<{text_color[emphasis]}>" + TagParser.Escape(fname) + "<{text_color[base]}>'...");
            }
            CommandStackEntry cse = queue.CommandStack.Peek();
            CommandStackEntry tcse = script.Created.Duplicate();
            tcse.Debug = cse.Debug;
            tcse.Variables = cse.Variables;
            queue.CommandStack.Push(tcse);
        }
    }
}
