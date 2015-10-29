using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Frenetic.TagHandlers.Objects;
using Frenetic.TagHandlers;

namespace Frenetic.CommandSystem.CommandEvents
{
    // <--[event]
    // @Name ScriptRanPreEvent
    // @Fired When a script is ran (usually via the run command).
    // @Updated 2015/10/28
    // @Authors mcmonkey
    // @Group Command
    // @Cancellable true
    // @Description
    // This event will fire whenever a script is ran, which by default is when <@link command run> is used.
    // This event can be used to control other scripts running on the system.
    // @Var script_name TextTag returns the name of the script about to be ran.
    // -->
    /// <summary>
    /// ScriptRanPreEvent, called by the run command.
    /// </summary>
    public class ScriptRanPreScriptEvent : ScriptEvent
    {
        /// <summary>
        /// Constructs the ScriptRan script event.
        /// </summary>
        /// <param name="system">The relevant command system.</param>
        public ScriptRanPreScriptEvent(Commands system)
            : base(system, "scriptranpreevent", true)
        {
            system.TheRunCommand.OnScriptRanPreEvent += (e) => { e.Cancelled = Run(e.ScriptName).Cancelled; }; // TODO: Allow determing script name
        }

        /// <summary>
        /// Runs the script event with the given input.
        /// </summary>
        /// <param name="scrName">The name of the script to run.</param>
        /// <returns>The event details after firing.</returns>
        public ScriptRanPreScriptEvent Run(string scrName) // TODO: SCRIPT OBJECT!
        {
            ScriptRanPreScriptEvent evt = (ScriptRanPreScriptEvent)Duplicate();
            evt.ScriptName = new TextTag(scrName);
            evt.Call();
            return evt;
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

        /// <summary>
        /// Applies a determination string to the event.
        /// </summary>
        /// <param name="determ">What was determined.</param>
        /// <param name="determLow">A lowercase copy of the determination.</param>
        /// <param name="mode">What debugmode to use.</param>
        public override void ApplyDetermination(string determ, string determLow, DebugMode mode)
        {
            base.ApplyDetermination(determ, determLow, mode);
        }
    }
}
