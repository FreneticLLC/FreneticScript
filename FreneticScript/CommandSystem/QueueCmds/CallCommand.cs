using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Objects;
using System.Reflection;
using System.Reflection.Emit;

namespace FreneticScript.CommandSystem.QueueCmds
{
    // <--[command]
    // @Name call
    // @Arguments <function to call> [-variable value ...]
    // @Short Runs a function.
    // @Updated 2016/04/27
    // @Authors mcmonkey
    // @Group Queue
    // @Minimum 1
    // @Maximum -1
    // @ReturnsValue true
    // @VarEqual MapTag the map of variables tracked at the end of the function call.
    // @Description
    // Activates a function created by the <@link command function>function<@/link> command.
    // TODO: Explain more!
    // @Example
    // // This example calls the function 'helloworld'.
    // call helloworld;
    // @Example
    // // This example calls the function 'outputme' with variable 'text' set to 'hello world'.
    // call outputme -text "hello world";
    // @Example
    // This example echoes the result of the function 'getinfo', from the tracked variable 'result'. Note that unexpected functionality may occur if 'getinfo' delays the variable set.
    // info ^= call getinfo;
    // echo <{[info].[result]}>;
    // @Example
    // This example echoes the result of the function 'getinfo', from the tracked variable 'result', after its full delayed run cycle.
    // info ^= &call getinfo;
    // echo <{[info].[result]}>;
    // @Example
    // TODO: More examples!
    // -->
    
    /// <summary>
    /// The Call command.
    /// </summary>
    public class CallCommand : AbstractCommand
    {
        /// <summary>
        /// Constructs the call command.
        /// </summary>
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
                TextTag.For
            };
        }

        /// <summary>
        /// Adapts a command entry to CIL.
        /// </summary>
        /// <param name="values">The adaptation-relevant values.</param>
        /// <param name="entry">The present entry ID.</param>
        public override void AdaptToCIL(CILAdaptationValues values, int entry)
        {
            base.AdaptToCIL(values, entry);
            values.ILGen.Emit(OpCodes.Ret);
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="queue">The command queue involved.</param>
        /// <param name="entry">Entry to be executed.</param>
        public static void Execute(CommandQueue queue, CommandEntry entry)
        {
            string fname = entry.GetArgument(queue, 0);
            fname = fname.ToLowerFast();
            CommandScript script = queue.CommandSystem.GetFunction(fname);
            if (script == null)
            {
                queue.HandleError(entry, "Cannot call function '<{text_color[emphasis]}>" + TagParser.Escape(fname) + "<{text_color[base]}>': it does not exist!");
                return;
            }
            if (entry.ShouldShowGood(queue))
            {
                entry.Good(queue, "Calling '<{text_color[emphasis]}>" + TagParser.Escape(fname) + "<{text_color[base]}>'...");
            }
            // TODO: Restore variable sending!
            CompiledCommandStackEntry cse = script.Created.Duplicate();
            foreach (string var in entry.NamedArguments.Keys)
            {
                if (!var.StartsWithNull())
                {
                    /*
                    if (cse.Variables.ContainsKey(var))
                    {
                        vars[var].Internal = entry.GetNamedArgumentObject(queue, var);
                    }
                    else
                    {
                        vars[var] = new ObjectHolder() { Internal = entry.GetNamedArgumentObject(queue, var) };
                    }
                    */
                }
            }
            if (entry.NamedArguments.ContainsKey("\0varname"))
            {
                bool sgood = entry.ShouldShowGood(queue);
                string vname = entry.GetNamedArgumentObject(queue, "\0varname").ToString(); // TODO: Should this go through parsing at all? Probably not!
                if (sgood)
                {
                    entry.Good(queue, "Noticing variable track for " + vname + ".");
                }
                // TODO: Save the variable to the queue properly!
                cse.Callback = () =>
                {
                    /*
                    if (existingVars.ContainsKey(vname))
                    {
                        existingVars[vname].Internal = new MapTag(cse.Variables);
                    }
                    else
                    {
                        existingVars[vname] = new ObjectHolder() { Internal = new MapTag(cse.Variables) };
                    }
                    */
                    if (sgood)
                    {
                        entry.Good(queue, "Call complete.");
                    }
                };
            }
            queue.CommandStack.Push(cse);
        }
    }
}
