//
// This file is part of FreneticScript, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of the MIT license.
// See README.md or LICENSE.txt in the FreneticScript source root for the contents of the license.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers.Objects;
using FreneticScript.TagHandlers;
using FreneticScript.CommandSystem.QueueCmds;

namespace FreneticScript.CommandSystem.CommandEvents
{
    /// <summary>
    /// ScriptRanPostEvent, called by the run command.
    /// </summary>
    public class ScriptRanPostScriptEvent : ScriptEvent
    {
        /// <summary>
        /// Constructs the ScriptRan script event.
        /// </summary>
        /// <param name="system">The relevant command system.</param>
        public ScriptRanPostScriptEvent(Commands system)
            : base(system, "scriptranpostevent", false)
        {
        }

        /// <summary>
        /// Register a specific priority with the underlying event.
        /// </summary>
        /// <param name="prio">The priority.</param>
        public override void RegisterPriority(int prio)
        {
            if (!System.TheRunFileCommand.OnScriptRanPostEvent.Contains(Run, prio))
            {
                System.TheRunFileCommand.OnScriptRanPostEvent.Add(Run, prio);
            }
        }

        /// <summary>
        /// Deregister a specific priority with the underlying event.
        /// </summary>
        /// <param name="prio">The priority.</param>
        public override void DeregisterPriority(int prio)
        {
            if (System.TheRunFileCommand.OnScriptRanPostEvent.Contains(Run, prio))
            {
                System.TheRunFileCommand.OnScriptRanPostEvent.Remove(Run, prio);
            }
        }

        /// <summary>
        /// Runs the script event with the given input.
        /// </summary>
        /// <param name="oevt">The details to the script that was ran.</param>
        /// <returns>The event details after firing.</returns>
        public void Run(ScriptRanPostEventArgs oevt)
        {
            ScriptRanPostScriptEvent evt = (ScriptRanPostScriptEvent)Duplicate();
            evt.ScriptRan = oevt.Script;
            evt.CallByPriority(oevt.Priority);
        }

        /// <summary>
        /// The script that was ran.
        /// </summary>
        public CommandScript ScriptRan;

        /// <summary>
        /// Get all variables according the script event's current values.
        /// </summary>
        public override Dictionary<string, TemplateObject> GetVariables()
        {
            Dictionary<string, TemplateObject> vars = base.GetVariables();
            vars.Add("script", new FunctionTag(ScriptRan));
            return vars;
        }
    }
}
