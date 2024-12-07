//
// This file is part of FreneticScript, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of the MIT license.
// See README.md or LICENSE.txt in the FreneticScript source root for the contents of the license.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticUtilities.FreneticExtensions;
using FreneticScript.TagHandlers.Objects;
using FreneticScript.UtilitySystems;

namespace FreneticScript.CommandSystem.QueueCmds;

/// <summary>The Event command.</summary>
public class EventCommand : AbstractCommand
{
    /// <summary>Adjust list of commands that are formed by an inner block.</summary>
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

    /// <summary>Constructs the event command.</summary>
    public EventCommand()
    {
        Name = "event";
        Arguments = "'add'/'remove'/'clear' <name of event>/'all' <name of event handler> [priority]";
        Description = "Creates a new function of the following command block, and adds it to the specified event's handler.";
        Asyncable = true;
        MinimumArguments = 1;
        MaximumArguments = 4;
        ObjectTypes =
        [
            Verify1,
            TextTag.Validator,
            TextTag.Validator,
            IntegerTag.Validator
        ];
    }

    // TODO: Compile

    void Verify1(ArgumentValidation validation)
    {
        string inp = validation.ObjectValue.ToString().ToLowerFast();
        if (inp == "add" || inp == "remove" || inp == "clear")
        {
            validation.ObjectValue = new TextTag(inp);
        }
        else
        {
            validation.ErrorResult = "Input to first argument must be 'add', 'remove', or 'clear'.";
        }
    }

    /// <summary>Executes the command.</summary>
    /// <param name="queue">The command queue involved.</param>
    /// <param name="entry">Entry to be executed.</param>
    public static void Execute(CommandQueue queue, CommandEntry entry)
    {
        if (entry.IsCallback)
        {
            return;
        }
        string type = entry.GetArgument(queue, 0).ToLowerFast();
        string eventname = entry.GetArgument(queue, 1).ToLowerFast();
        if (type == "clear" && eventname == "all")
        {
            foreach (KeyValuePair<string, ScriptEvent> evt in queue.Engine.Events)
            {
                evt.Value.Clear();
            }
            if (entry.ShouldShowGood(queue))
            {
                entry.GoodOutput(queue, "Cleared all events.");
            }
            return;
        }
        if (!queue.Engine.Events.TryGetValue(eventname, out ScriptEvent theEvent))
        {
            queue.HandleError(entry, "Unknown event '" + TextStyle.Separate + eventname + TextStyle.Base + "'.");
            return;
        }
        if (type == "clear")
        {
            theEvent.Clear();
            if (entry.ShouldShowGood(queue))
            {
                entry.GoodOutput(queue, "Cleared event '" + TextStyle.Separate + eventname + TextStyle.Base + "' of all handlers.");
            }
        }
        else if (type == "remove")
        {
            if (entry.Arguments.Length < 3)
            {
                ShowUsage(queue, entry);
                return;
            }
            string name = entry.GetArgument(queue, 2).ToLowerFast();
            bool success = theEvent.RemoveEventHandler(name);
            if (success)
            {
                if (entry.ShouldShowGood(queue))
                {
                    entry.GoodOutput(queue, "Removed event handler '" + TextStyle.Separate + name + TextStyle.Base + "'.");
                }
            }
            else
            {
                if (BooleanTag.TryFor(entry.GetNamedArgumentObject(queue, "quiet_fail"))?.Internal ?? false)
                {
                    if (entry.ShouldShowGood(queue))
                    {
                        entry.GoodOutput(queue, "Unknown event handler '" + TextStyle.Separate + name + TextStyle.Base + "'.");
                    }
                }
                else
                {
                    queue.HandleError(entry, "Unknown event handler '" + TextStyle.Separate + name + TextStyle.Base + "'.");
                }
            }
        }
        else if (type == "add")
        {
            if (entry.Arguments.Length < 3)
            {
                ShowUsage(queue, entry);
                return;
            }
            string name = entry.GetArgument(queue, 2).ToLowerFast();
            if (entry.InnerCommandBlock == null)
            {
                queue.HandleError(entry, "Event command invalid: No block follows!");
                return;
            }
            if (theEvent.HasHandler(name))
            {
                if (BooleanTag.TryFor(entry.GetNamedArgumentObject(queue, "quiet_fail"))?.Internal ?? false)
                {
                    if (entry.ShouldShowGood(queue))
                    {
                        entry.GoodOutput(queue, "Handler '" + TextStyle.Separate + name + TextStyle.Base + "' already exists!");
                    }
                }
                else
                {
                    queue.HandleError(entry, "Handler '" + TextStyle.Separate + name + TextStyle.Base + "' already exists!");
                }
            }
            double priority = 0;
            if (entry.Arguments.Length > 3)
            {
                priority = NumberTag.For(entry.GetArgumentObject(queue, 3), queue.Error).Internal;
            }
            List<CommandEntry> entries = new(entry.InnerCommandBlock.Length + 2);
            MapTag expectedContext = new();
            expectedContext.Internal.Add("context", entry.System.TagTypes.Type_Map.TagForm);
            entries.Add(entry.System.TheRequireCommand.GenerateEntry(expectedContext, entry.ScriptName, entry.ScriptLine));
            entries.AddRange(entry.InnerCommandBlock);
            CommandScript script = new(theEvent.Name + "__handler__" + name,
                CommandScript.TYPE_NAME_EVENT, [.. entries], entry.System, entry.BlockStart, DebugMode.MINIMAL);
            theEvent.RegisterEventHandler(priority, script, name);
            entry.GoodOutput(queue, "Handler '" + TextStyle.Separate + name + "" + TextStyle.Base
                + "' defined for event '" + TextStyle.Separate + theEvent.Name + TextStyle.Base + "'.");
        }
        else
        {
            ShowUsage(queue, entry);
        }
    }
}
