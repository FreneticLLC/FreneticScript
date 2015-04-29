using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Frenetic.TagHandlers;
using Frenetic.TagHandlers.Objects;

namespace Frenetic.CommandSystem
{
    /// <summary>
    /// An abstract class, implementations of this should be used to fire events within the script engine.
    /// </summary>
    public abstract class ScriptEvent
    {
        /// <summary>
        /// Gets the list of CommandScripts that handle an event currently.
        /// </summary>
        /// <param name="_event">The event to get the handlers for</param>
        /// <returns>The list of handlers</returns>
        public static List<CommandScript> GetHandlers(ScriptEvent _event)
        {
            if (_event == null)
            {
                return new List<CommandScript>();
            }
            return _event.Handlers;
        }

        /// <summary>
        /// All scripts that handle this event.
        /// </summary>
        public List<CommandScript> Handlers = new List<CommandScript>();

        /// <summary>
        /// The command system in use.
        /// </summary>
        public Commands System;

        /// <summary>
        /// Whether the script event has been cancelled.
        /// </summary>
        public bool Cancelled = false;

        /// <summary>
        /// Constructs the script event's base data.
        /// Called only by implementing script events.
        /// </summary>
        /// <param name="_system">The command system this event exists within</param>
        /// <param name="_name">The name of the event</param>
        /// <param name="_handlers">Any pre-existing handlers for this event</param>
        public ScriptEvent(Commands _system, string _name, List<CommandScript> _handlers)
        {
            Handlers.AddRange(_handlers);
            System = _system;
            Name = _name.ToLower();
        }

        /// <summary>
        /// Calls the event. Returns whether it was cancelled.
        /// </summary>
        /// <returns>Whether to cancel</returns>
        public bool Call()
        {
            for (int i = 0; i < Handlers.Count; i++)
            {
                CommandScript script = Handlers[i];
                Dictionary<string, TemplateObject> Variables = GetVariables();
                CommandQueue queue;
                foreach (string determ in System.ExecuteScript(script, Variables, out queue))
                {
                    ApplyDetermination(determ, queue.Debug);
                }
                if (i >= Handlers.Count || Handlers[i] != script)
                {
                    i--;
                }
            }
            return Cancelled;
        }

        /// <summary>
        /// Applies a determination string to the event.
        /// </summary>
        /// <param name="determ">What was determined</param>
        /// <param name="mode">What debugmode to use</param>
        public virtual void ApplyDetermination(string determ, DebugMode mode)
        {
            if (determ != null && determ.ToLower() == "cancelled")
            {
                Cancelled = true;
            }
            else if (determ != null && determ.ToLower() == "uncancelled")
            {
                Cancelled = false;
            }
            else
            {
                System.Output.Bad("Unknown determination '<{color.emphasis}>" + TagParser.Escape(determ) + "<{color.base}>'.", mode);
            }
        }

        /// <summary>
        /// Get all variables according the script event's current values.
        /// </summary>
        public virtual Dictionary<string, TemplateObject> GetVariables()
        {
            Dictionary<string, TemplateObject> vars = new Dictionary<string, TemplateObject>();
            vars.Add("cancelled", new TextTag(Cancelled ? "true": "false"));
            return vars;
        }

        /// <summary>
        /// The name of this event.
        /// </summary>
        public readonly string Name;
    }
}
