using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Objects;

namespace FreneticScript.CommandSystem
{
    /// <summary>
    /// An abstract class, implementations of this should be used to fire events within the script engine.
    /// </summary>
    public abstract class ScriptEvent
    {
        /// <summary>
        /// Whether this event can be cancelled.
        /// </summary>
        public bool Cancellable = false;

        /// <summary>
        /// Gets the list of CommandScripts that handle an event currently.
        /// </summary>
        /// <param name="_event">The event to get the handlers for.</param>
        /// <returns>The list of handlers.</returns>
        public static List<KeyValuePair<int, CommandScript>> GetHandlers(ScriptEvent _event)
        {
            if (_event == null)
            {
                return new List<KeyValuePair<int, CommandScript>>();
            }
            return _event.Handlers;
        }

        /// <summary>
        /// Set up the script event. For use by the event system itself.
        /// </summary>
        public virtual void Init()
        {
            // Do Nothing
        }

        /// <summary>
        /// Shut down the script event. For use by the event system itself.
        /// </summary>
        public virtual void Destroy()
        {
            // Do Nothing
        }
        
        /// <summary>
        /// All scripts that handle this event.
        /// TODO: SortedSet?
        /// </summary>
        public List<KeyValuePair<int, CommandScript>> Handlers = new List<KeyValuePair<int, CommandScript>>();

        /// <summary>
        /// Register a specific priority with the underlying event.
        /// </summary>
        /// <param name="prio">The priority.</param>
        public virtual void RegisterPriority(int prio)
        {
            // Do Nothing
        }

        /// <summary>
        /// Deregister a specific priority with the underlying event.
        /// </summary>
        /// <param name="prio">The priority.</param>
        public virtual void DeregisterPriority(int prio)
        {
            // Do Nothing
        }

        /// <summary>
        /// Register a new event handler to this script event.
        /// </summary>
        /// <param name="prio">The priority to use.</param>
        /// <param name="script">The script to register to the handler</param>
        public void RegisterEventHandler(int prio, CommandScript script)
        {
            Handlers.Add(new KeyValuePair<int, CommandScript>(prio, script));
            Sort();
            if (Handlers.Count == 1)
            {
                Init();
            }
            RegisterPriority(prio);
        }

        /// <summary>
        /// Removes an event handler by name.
        /// </summary>
        /// <param name="name">The name of the handler to remove.</param>
        /// <returns>Whether there was a removal.</returns>
        public bool RemoveEventHandler(string name)
        {
            for (int i = 0; i < Handlers.Count; i++)
            {
                if (Handlers[i].Value.Name == name)
                {
                    int prio = Handlers[i].Key;
                    Handlers.RemoveAt(i);
                    if (Handlers.Count == 0)
                    {
                        Destroy();
                    }
                    else
                    {
                        for (int x = 0; x < Handlers.Count; x++)
                        {
                            if (Handlers[x].Key == prio)
                            {
                                return true;
                            }
                        }
                        DeregisterPriority(prio);
                    }
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Clears away all event handlers.
        /// </summary>
        public void Clear()
        {
            if (Handlers.Count == 0)
            {
                return;
            }
            Handlers.Clear();
            Destroy();
        }

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
        /// <param name="_system">The command system this event exists within.</param>
        /// <param name="_name">The name of the event.</param>
        /// <param name="cancellable">Whether the event can be cancelled.</param>
        public ScriptEvent(Commands _system, string _name, bool cancellable)
        {
            System = _system;
            Name = _name.ToLowerFast();
            Cancellable = cancellable;
        }

        /// <summary>
        /// Quickly sorts the event handlers.
        /// </summary>
        public void Sort()
        {
            IOrderedEnumerable<KeyValuePair<int, CommandScript>> ordered = Handlers.OrderBy((o) => o.Key);
            Handlers = ordered.ToList();
        }

        /// <summary>
        /// Update all variables from a ran script onto the event itself.
        /// </summary>
        /// <param name="vars">The vars to update.</param>
        public virtual void UpdateVariables(Dictionary<string, TemplateObject> vars)
        {
            Cancelled = BooleanTag.TryFor(vars["cancelled"]).Internal;
        }
        
        /// <summary>
        /// Calls the event.
        /// </summary>
        protected void Call(int prio = int.MinValue)
        {
            for (int i = 0; i < Handlers.Count; i++)
            {
                if (prio == int.MinValue || Handlers[i].Key == prio)
                {
                    // TODO: Handle cancelling stuff here?
                    // IE, don't fire if cancelled and we don't want to fire?
                    CommandScript script = Handlers[i].Value;
                    Dictionary<string, TemplateObject> Variables = new Dictionary<string, TemplateObject>();
                    Variables["context"] = new MapTag(GetVariables());
                    CommandQueue queue;
                    System.ExecuteScript(script, ref Variables, out queue);
                    if (Variables != null && Variables.ContainsKey("context"))
                    {
                        UpdateVariables(MapTag.For(Variables["context"]).Internal);
                    }
                    else
                    {
                        System.Output.Bad("Context undefined for script event '" + TagParser.Escape(Name) + "'?", DebugMode.FULL);
                    }
                    if (i >= Handlers.Count || Handlers[i].Value != script)
                    {
                        i--;
                    }
                }
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

        /// <summary>
        /// Create a copy of this script event, safe to run.
        /// </summary>
        /// <returns>The copy.</returns>
        public virtual ScriptEvent Duplicate()
        {
            return (ScriptEvent)MemberwiseClone();
        }
    }
}
