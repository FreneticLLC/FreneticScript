//
// This file is part of FreneticScript, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of the MIT license.
// See README.md or LICENSE.txt in the FreneticScript source root for the contents of the license.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.CommandSystem.QueueCmds;
using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Objects;

namespace FreneticScript.CommandSystem.CommandEvents
{
    // <--[event]
    // @Name ScriptRanPreEvent
    // @Fired When a script will be ran (usually via the runfile command).
    // @Updated 2015/10/28
    // @Authors mcmonkey
    // @Group Command
    // @Cancellable true
    // @Description
    // This event will fire whenever a script is ran, which by default is when <@link command runfile> is used.
    // This event can be used to control other scripts running on the system.
    // @Var script_name TextTag returns the name of the script about to be ran.
    // -->
    /// <summary>ScriptRanPreEvent, called by the run command.</summary>
    public class ScriptRanPreScriptEvent : ScriptEvent
    {
        /// <summary>Constructs the ScriptRan script event.</summary>
        /// <param name="system">The relevant command system.</param>
        public ScriptRanPreScriptEvent(ScriptEngine system)
            : base(system, "scriptranpreevent", true)
        {
        }

        /// <summary>Register a specific priority with the underlying event.</summary>
        /// <param name="prio">The priority.</param>
        public override void RegisterPriority(double prio)
        {
            PrioritySourceObject source = new(this, prio);
            if (!Engine.TheRunFileCommand.OnScriptRanPreEvent.IsHandledBySource(source))
            {
                Engine.TheRunFileCommand.OnScriptRanPreEvent.AddEvent(Run, source, prio);
            }
        }

        /// <summary>Deregister a specific priority with the underlying event.</summary>
        /// <param name="prio">The priority.</param>
        public override void DeregisterPriority(double prio)
        {
            Engine.TheRunFileCommand.OnScriptRanPreEvent.RemoveBySource(new PrioritySourceObject(this, prio));
        }

        /// <summary>Runs the script event with the given input.</summary>
        /// <param name="oevt">The details of the script to be ran.</param>
        /// <returns>The event details after firing.</returns>
        public void Run(ScriptRanPreEventArgs oevt)
        {
            ScriptRanPreScriptEvent evt = (ScriptRanPreScriptEvent)Duplicate();
            evt.ScriptName = new TextTag(oevt.ScriptName);
            evt.CallByPriority(oevt.Priority);
        }

        /// <summary>The name of the script being ran.</summary>
        public TextTag ScriptName;

        /// <summary>Get all variables according the script event's current values.</summary>
        public override Dictionary<string, TemplateObject> GetVariables()
        {
            Dictionary<string, TemplateObject> vars = base.GetVariables();
            vars.Add("script_name", ScriptName);
            return vars;
        }
    }
}
