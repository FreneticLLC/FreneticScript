using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Objects;

namespace FreneticScript.CommandSystem.QueueCmds
{
    class EventCommand : AbstractCommand
    {
        public override void AdaptBlockFollowers(CommandEntry entry, List<CommandEntry> input, List<CommandEntry> fblock)
        {
            input.Clear();
            base.AdaptBlockFollowers(entry, input, fblock);
        }

        // TODO: Meta!
        public EventCommand()
        {
            Name = "event";
            Arguments = "'add'/'remove'/'clear' <name of event>/'all' <name of event handler> <priority> ['quiet_fail']";
            Description = "Creates a new function of the following command block, and adds it to the specified event's handler.";
            IsFlow = true;
            Asyncable = true;
            MinimumArguments = 2;
            MaximumArguments = 5;
            ObjectTypes = new List<Func<TemplateObject, TemplateObject>>()
            {
                (input) =>
                {
                    if (input.ToString() == "\0CALLBACK")
                    {
                        return input;
                    }
                    string inp = input.ToString().ToLowerFast();
                    if (inp == "add" || inp == "remove" || inp == "clear")
                    {
                        return new TextTag(inp);
                    }
                    return null;
                },
                (input) =>
                {
                    return new TextTag(input.ToString());
                },
                (input) =>
                {
                    return IntegerTag.TryFor(input);
                },
                (input) =>
                {
                    string inp = input.ToString().ToLowerFast();
                    if (inp == "quiet_fail")
                    {
                        return new TextTag(input.ToString());
                    }
                    return null;
                }
            };
        }

        public override void Execute(CommandEntry entry)
        {
            if (entry.Arguments[0].ToString() == "\0CALLBACK")
            {
                return;
            }
            string type = entry.GetArgument(0).ToLowerFast();
            string eventname = entry.GetArgument(1).ToLowerFast();
            if (type == "clear" && eventname == "all")
            {
                foreach (KeyValuePair<string, ScriptEvent> evt in entry.Queue.CommandSystem.Events)
                {
                    evt.Value.Handlers.Clear();
                }
                if (entry.ShouldShowGood())
                {
                    entry.Good("Cleared all events.");
                }
                return;
            }
            ScriptEvent theEvent;
            if (!entry.Queue.CommandSystem.Events.TryGetValue(eventname, out theEvent))
            {
                entry.Error("Unknown event '<{text_color.emphasis}>" + TagParser.Escape(eventname) + "<{text_color.base}>'.");
                return;
            }
            if (type == "clear")
            {
                int count = theEvent.Handlers.Count;
                theEvent.Handlers.Clear();
                if (entry.ShouldShowGood())
                {
                    entry.Good("Cleared <{text_color.emphasis}>" + count + "<{text_color.base}> event handler" + (count == 1 ? "." : "s."));
                }
            }
            else if (type == "remove")
            {
                if (entry.Arguments.Count < 3)
                {
                    ShowUsage(entry);
                    return;
                }
                string name = entry.GetArgument(2).ToLowerFast();
                bool success = theEvent.RemoveEventHandler("eventhandler_" + theEvent.Name + "_" + name);
                if (success)
                {
                    if (entry.ShouldShowGood())
                    {
                        entry.Good("Removed event handler '<{text_color.emphasis}>" + TagParser.Escape(name) + "<{text_color.base}>'.");
                    }
                }
                else
                {
                    if (entry.Arguments.Count > 3 && entry.GetArgument(3).ToLowerFast() == "quiet_fail")
                    {
                        if (entry.ShouldShowGood())
                        {
                            entry.Good("Unknown event handler '<{text_color.emphasis}>" + TagParser.Escape(name) + "<{text_color.base}>'.");
                        }
                    }
                    else
                    {
                        entry.Error("Unknown event handler '<{text_color.emphasis}>" + TagParser.Escape(name) + "<{text_color.base}>'.");
                    }
                }
            }
            else if (type == "add")
            {
                if (entry.Arguments.Count < 3)
                {
                    ShowUsage(entry);
                    return;
                }
                string name = entry.GetArgument(2).ToLowerFast();
                if (entry.InnerCommandBlock == null)
                {
                    entry.Error("Event command invalid: No block follows!");
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
                    IntegerTag inter = IntegerTag.TryFor(entry.GetArgumentObject(3));
                    if (inter != null)
                    {
                        priority = (int)inter.Internal;
                    }
                }
                if (success)
                {
                    if (entry.Arguments.Count > 4 && entry.GetArgument(4).ToLowerFast() == "quiet_fail")
                    {
                        if (entry.ShouldShowGood())
                        {
                            entry.Good("Handler '<{text_color.emphasis}>" + TagParser.Escape(name) + "<{text_color.base}>' already exists!");
                        }
                    }
                    else
                    {
                        entry.Error("Handler '<{text_color.emphasis}>" + TagParser.Escape(name) + "<{text_color.base}>' already exists!");
                    }
                }
                else
                {
                    theEvent.RegisterEventHandler(priority, new CommandScript("eventhandler_" + theEvent.Name + "_" + name, entry.InnerCommandBlock, entry.BlockStart) { Debug = DebugMode.MINIMAL });
                    entry.Good("Handler '<{text_color.emphasis}>" + TagParser.Escape(name) +
                        "<{text_color.base}>' defined for event '<{text_color.emphasis}>" + TagParser.Escape(theEvent.Name) + "<{text_color.base}>'.");
                }
            }
            else
            {
                ShowUsage(entry);
            }
        }
    }
}
