using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers.Objects;
using FreneticScript.TagHandlers;
using FreneticScript.CommandSystem.QueueCmds;

namespace FreneticScript.CommandSystem.CommandEvents
{
    // <--[event]
    // @Name ScriptRanEvent
    // @Fired When a script is soon to be ran (usually via the run command).
    // @Updated 2015/10/28
    // @Authors mcmonkey
    // @Group Command
    // @Cancellable true
    // @Description
    // This event will fire whenever a script is ran, which by default is when <@link command run> is used.
    // This event can be used to control other scripts running on the system.
    // @Var script_name TextTag returns the name of the script about to be ran. // TODO: SCRIPT OBJECT!
    // -->
    /// <summary>
    /// ScriptRanPreEvent, called by the run command.
    /// </summary>
    public class ScriptRanScriptEvent : ScriptEvent
    {
        /// <summary>
        /// Constructs the ScriptRan script event.
        /// </summary>
        /// <param name="system">The relevant command system.</param>
        public ScriptRanScriptEvent(Commands system)
            : base(system, "scriptranevent", true)
        {
        }

        /// <summary>
        /// Register a specific priority with the underlying event.
        /// </summary>
        /// <param name="prio">The priority.</param>
        public override void RegisterPriority(int prio)
        {
            if (!System.TheRunCommand.OnScriptRanEvent.Contains(Run, prio))
            {
                System.TheRunCommand.OnScriptRanEvent.Add(Run, prio);
            }
        }

        /// <summary>
        /// Deregister a specific priority with the underlying event.
        /// </summary>
        /// <param name="prio">The priority.</param>
        public override void DeregisterPriority(int prio)
        {
            if (System.TheRunCommand.OnScriptRanEvent.Contains(Run, prio))
            {
                System.TheRunCommand.OnScriptRanEvent.Remove(Run, prio);
            }
        }

        /// <summary>
        /// Runs the script event with the given input.
        /// </summary>
        /// <param name="oevt">The details of the script to be ran.</param>
        /// <returns>The event details after firing.</returns>
        public void Run(ScriptRanEventArgs oevt)
        {
            ScriptRanScriptEvent evt = (ScriptRanScriptEvent)Duplicate();
            evt.ScriptName = new TextTag(oevt.Script.Name);
            evt.Call(oevt.Priority);
        }

        /// <summary>
        /// The name of the script being ran.
        /// </summary>
        public TextTag ScriptName;

        /// <summary>
        /// Get all variables according the script event's current values.
        /// </summary>
        public override Dictionary<string, TemplateObject> GetVariables()
        {
            Dictionary<string, TemplateObject> vars = base.GetVariables();
            vars.Add("script_name", ScriptName);
            return vars;
        }
    }
}
