//
// This file is created by Frenetic LLC.
// This code is Copyright (C) 2016-2017 Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Objects;

namespace FreneticScript.CommandSystem.QueueCmds
{
    /// <summary>
    /// The Event command.
    /// </summary>
    public class EventCommand : AbstractCommand
    {
        /// <summary>
        /// Adjust list of commands that are formed by an inner block.
        /// </summary>
        /// <param name="entry">The producing entry.</param>
        /// <param name="input">The block of commands.</param>
        /// <param name="fblock">The final block to add to the entry.</param>
        public override void AdaptBlockFollowers(CommandEntry entry, List<CommandEntry> input, List<CommandEntry> fblock)
        {
            input.Clear();
            base.AdaptBlockFollowers(entry, input, fblock);
        }

        // <--[command]
        // @Name event
        // @Arguments 'add'/'remove'/'clear' <name of event>/'all' <name of event handler> [priority] ['quiet_fail']
        // @Short Creates a new function of the following command block, and adds it to the specified event's handler.
        // @Updated 2016/04/28
        // @Authors mcmonkey
        // @Group Queue
        // @Block Allowed
        // @Minimum 1
        // @Maximum 5
        // @Description
        // Creates a new function of the following command block, and adds it to the specified event's handler.
        // TODO: Explain more!
        // @Example
        // // This example echos the name of the script every time a script is ran.
        // event add scriptranpostevent WhenAScriptIsRun 0
        // {
        //     echo <{[context].[script_name]}>;
        // }
        // @Example
        // // TODO: More examples!
        // -->

        /// <summary>
        /// Constructs the event command.
        /// </summary>
        public EventCommand()
        {
            Name = "event";
            Arguments = "'add'/'remove'/'clear' <name of event>/'all' <name of event handler> [priority] ['quiet_fail']";
            Description = "Creates a new function of the following command block, and adds it to the specified event's handler.";
            IsFlow = true;
            Asyncable = true;
            MinimumArguments = 1;
            MaximumArguments = 5;
            ObjectTypes = new List<Func<TemplateObject, TemplateObject>>()
            {
                Verify1,
                TextTag.For,
                TextTag.For,
                IntegerTag.TryFor,
                Verify2
            };
        }

        TemplateObject Verify1(TemplateObject input)
        {
            if (input.ToString() == "\0CALLBACK")
            {
                return input;
            }
            string inp = input.ToString().ToLowerFastFS();
            if (inp == "add" || inp == "remove" || inp == "clear")
            {
                return new TextTag(inp);
            }
            return null;
        }

        TemplateObject Verify2(TemplateObject input)
        {
            string inp = input.ToString().ToLowerFastFS();
            if (inp == "quiet_fail")
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
            if (entry.Arguments[0].ToString() == "\0CALLBACK")
            {
                return;
            }
            string type = entry.GetArgument(queue, 0).ToLowerFastFS();
            string eventname = entry.GetArgument(queue, 1).ToLowerFastFS();
            if (type == "clear" && eventname == "all")
            {
                foreach (KeyValuePair<string, ScriptEvent> evt in queue.CommandSystem.Events)
                {
                    evt.Value.Handlers.Clear();
                }
                if (entry.ShouldShowGood(queue))
                {
                    entry.Good(queue, "Cleared all events.");
                }
                return;
            }
            if (!queue.CommandSystem.Events.TryGetValue(eventname, out ScriptEvent theEvent))
            {
                queue.HandleError(entry, "Unknown event '<{text_color[emphasis]}>" + TagParser.Escape(eventname) + "<{text_color[base]}>'.");
                return;
            }
            if (type == "clear")
            {
                int count = theEvent.Handlers.Count;
                theEvent.Handlers.Clear();
                if (entry.ShouldShowGood(queue))
                {
                    entry.Good(queue, "Cleared <{text_color[emphasis]}>" + count + "<{text_color[base]}> event handler" + (count == 1 ? "." : "s."));
                }
            }
            else if (type == "remove")
            {
                if (entry.Arguments.Count < 3)
                {
                    ShowUsage(queue, entry);
                    return;
                }
                string name = entry.GetArgument(queue, 2).ToLowerFastFS();
                bool success = theEvent.RemoveEventHandler("eventhandler_" + theEvent.Name + "_" + name);
                if (success)
                {
                    if (entry.ShouldShowGood(queue))
                    {
                        entry.Good(queue, "Removed event handler '<{text_color[emphasis]}>" + TagParser.Escape(name) + "<{text_color[base]}>'.");
                    }
                }
                else
                {
                    if (entry.Arguments.Count > 3 && entry.GetArgument(queue, 3).ToLowerFastFS() == "quiet_fail")
                    {
                        if (entry.ShouldShowGood(queue))
                        {
                            entry.Good(queue, "Unknown event handler '<{text_color[emphasis]}>" + TagParser.Escape(name) + "<{text_color[base]}>'.");
                        }
                    }
                    else
                    {
                        queue.HandleError(entry, "Unknown event handler '<{text_color[emphasis]}>" + TagParser.Escape(name) + "<{text_color[base]}>'.");
                    }
                }
            }
            else if (type == "add")
            {
                if (entry.Arguments.Count < 3)
                {
                    ShowUsage(queue, entry);
                    return;
                }
                string name = entry.GetArgument(queue, 2).ToLowerFastFS();
                if (entry.InnerCommandBlock == null)
                {
                    queue.HandleError(entry, "Event command invalid: No block follows!");
                    return;
                }
                bool success = false;
                for (int i = 0; i < theEvent.Handlers.Count; i++)
                {
                    if (theEvent.Handlers[i].Value.Name == "eventhandler_" + theEvent.Name + "_" + name)
                    {
                        success = true;
                        break;
                    }
                }
                int priority = 0;
                if (entry.Arguments.Count > 3)
                {
                    IntegerTag inter = IntegerTag.TryFor(entry.GetArgumentObject(queue, 3));
                    if (inter != null)
                    {
                        priority = (int)inter.Internal;
                    }
                }
                if (success)
                {
                    if (entry.Arguments.Count > 4 && entry.GetArgument(queue, 4).ToLowerFastFS() == "quiet_fail")
                    {
                        if (entry.ShouldShowGood(queue))
                        {
                            entry.Good(queue, "Handler '<{text_color[emphasis]}>" + TagParser.Escape(name) + "<{text_color[base]}>' already exists!");
                        }
                    }
                    else
                    {
                        queue.HandleError(entry, "Handler '<{text_color[emphasis]}>" + TagParser.Escape(name) + "<{text_color[base]}>' already exists!");
                    }
                }
                else
                {
                    theEvent.RegisterEventHandler(priority, new CommandScript("eventhandler_" + theEvent.Name + "_" + name, entry.InnerCommandBlock, entry.BlockStart, DebugMode.MINIMAL));
                    entry.Good(queue, "Handler '<{text_color[emphasis]}>" + TagParser.Escape(name) +
                        "<{text_color[base]}>' defined for event '<{text_color[emphasis]}>" + TagParser.Escape(theEvent.Name) + "<{text_color[base]}>'.");
                }
            }
            else
            {
                ShowUsage(queue, entry);
            }
        }
    }
}
