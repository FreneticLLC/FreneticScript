//
// This file is part of FreneticScript, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of the MIT license.
// See README.md or LICENSE.txt in the FreneticScript source root for the contents of the license.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using FreneticUtilities.FreneticExtensions;
using FreneticUtilities.FreneticToolkit;
using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Objects;

namespace FreneticScript.CommandSystem;

/// <summary>An abstract class, implementations of this should be used to fire events within the script engine.</summary>
public abstract class ScriptEvent
{
    /// <summary>Whether this event can be cancelled.</summary>
    public bool Cancellable = false;

    /// <summary>Helper class for being the source of a priority.</summary>
    public class PrioritySourceObject : IEquatable<PrioritySourceObject>
    {
        /// <summary>The relevant event.</summary>
        public ScriptEvent Event;

        /// <summary>The priority level.</summary>
        public double Priority;

        /// <summary>Gets a hash code for the source object.</summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode()
        {
            return Event.Name.GetHashCode() + Priority.GetHashCode();
        }

        /// <summary>Compares whether the source object and another object are equal.</summary>
        /// <param name="obj">The other object.</param>
        /// <returns>Whether they are equal.</returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            return obj is PrioritySourceObject pso && Equals(pso);
        }

        /// <summary>Compares whether the source object and another are equal.</summary>
        /// <param name="other">The other source object.</param>
        /// <returns>Whether they are equal.</returns>
        public bool Equals(PrioritySourceObject other)
        {
            if (other == null)
            {
                return false;
            }
            return Event.Name == other.Event.Name && Priority == other.Priority;
        }

        /// <summary>Constructs see <see cref="PrioritySourceObject"/>.</summary>
        /// <param name="_event">The relevant event.</param>
        /// <param name="_priority">The relevant priority.</param>
        public PrioritySourceObject(ScriptEvent _event, double _priority)
        {
            Event = _event;
            Priority = _priority;
        }
    }

    /// <summary>Set up the script event. For use by the event system itself.</summary>
    public virtual void Init()
    {
        // Do Nothing
    }

    /// <summary>Shut down the script event. For use by the event system itself.</summary>
    public virtual void Destroy()
    {
        // Do Nothing
    }

    /// <summary>Represents the index of a script in the event handler list.</summary>
    public class ScriptIndex
    {
        /// <summary>The script set object itself.</summary>
        public ScriptSet SetObject;

        /// <summary>The index within the script set.</summary>
        public int SetIndex;
    }

    /// <summary>Represents a set of scripts with the same priority.</summary>
    public class ScriptSet
    {
        /// <summary>The script priority.</summary>
        public double Priority;

        /// <summary>The index within the handler list.</summary>
        public int Index;

        /// <summary>The scripts contained in the set.</summary>
        public List<KeyValuePair<ScriptIndex, CommandScript>> Scripts = new();
    }

    /// <summary>All scripts that handle this event.</summary>
    public List<ScriptSet> Handlers = new();

    /// <summary>A map of all handler names to handler script indices.</summary>
    public Dictionary<string, ScriptIndex> HandlerNames = new();

    /// <summary>Register a specific priority with the underlying event.</summary>
    /// <param name="prio">The priority.</param>
    public virtual void RegisterPriority(double prio)
    {
        // Do Nothing
    }

    /// <summary>Deregister a specific priority with the underlying event.</summary>
    /// <param name="prio">The priority.</param>
    public virtual void DeregisterPriority(double prio)
    {
        // Do Nothing
    }

    /// <summary>Returns whether the event has a handler by the given name.</summary>
    /// <param name="name">The handler name.</param>
    /// <returns>True if the handler is present, or false if not.</returns>
    public bool HasHandler(string name)
    {
        return HandlerNames.ContainsKey(name);
    }

    /// <summary>Register a new event handler to this script event.</summary>
    /// <param name="prio">The priority to use.</param>
    /// <param name="script">The script to register to the handler.</param>
    /// <param name="name">The name of the event to register.</param>
    public void RegisterEventHandler(double prio, CommandScript script, string name)
    {
        name = name.ToLowerFast();
        if (HasHandler(name))
        {
            RemoveEventHandler(name);
        }
        ScriptSet set;
        int index;
        for (index = 0; index < Handlers.Count; index++)
        {
            if (Handlers[index].Priority == prio)
            {
                set = Handlers[index];
                goto buildset;
            }
            else if (Handlers[index].Priority > prio)
            {
                set = new ScriptSet();
                Handlers.Insert(index, set);
                goto buildset;
            }
        }
        set = new ScriptSet();
        Handlers.Add(set);
        buildset:
        ScriptIndex indexObject = new() { SetObject = set, SetIndex = set.Scripts.Count };
        HandlerNames.Add(name, indexObject);
        set.Scripts.Add(new KeyValuePair<ScriptIndex, CommandScript>(indexObject, script));
        if (Handlers.Count == 1)
        {
            Init();
        }
        else
        {
            for (int i = index + 1; i < Handlers.Count; i++)
            {
                Handlers[i].Index++;
            }
        }
        if (set.Scripts.Count == 1)
        {
            RegisterPriority(prio);
        }
    }

    /// <summary>Removes an event handler by name.</summary>
    /// <param name="name">The name of the handler to remove.</param>
    /// <returns>Whether there was a removal.</returns>
    public bool RemoveEventHandler(string name)
    {
        name = name.ToLowerFast();
        if (!HandlerNames.TryGetValue(name, out ScriptIndex index))
        {
            return false;
        }
        HandlerNames.Remove(name);
        List<KeyValuePair<ScriptIndex, CommandScript>> scriptsInSet = index.SetObject.Scripts;
        scriptsInSet.RemoveAt(index.SetIndex);
        if (scriptsInSet.Count == 0)
        {
            Handlers.RemoveAt(index.SetObject.Index);
            if (ProcessingPatch != null && index.SetObject.Index <= ProcessingPatch.SetObject.Index)
            {
                ProcessingPatch.SetObject.Index--;
            }
            for (int i = index.SetObject.Index; i < Handlers.Count; i++)
            {
                Handlers[i].Index--;
            }
            DeregisterPriority(index.SetObject.Priority);
            if (Handlers.Count == 0)
            {
                Destroy();
            }
        }
        else
        {
            if (ProcessingPatch != null && index.SetObject.Index == ProcessingPatch.SetObject.Index && ProcessingPatch.SetIndex >= index.SetIndex)
            {
                ProcessingPatch.SetIndex--;
            }
            for (int i = index.SetIndex; i < scriptsInSet.Count; i++)
            {
                scriptsInSet[i].Key.SetIndex--;
            }
        }
        if (index == CurrentlyProcessing)
        {
            ProcessingPatch = CurrentlyProcessing;
        }
        return true;
    }

    /// <summary>Clears away all event handlers.</summary>
    public void Clear()
    {
        if (Handlers.Count == 0)
        {
            return;
        }
        for (int i = 0; i < Handlers.Count; i++)
        {
            DeregisterPriority(Handlers[i].Priority);
        }
        Handlers.Clear();
        Destroy();
    }

    /// <summary>The command system in use.</summary>
    public ScriptEngine Engine;

    /// <summary>Whether the script event has been cancelled.</summary>
    public bool Cancelled = false;

    /// <summary>
    /// Constructs the script event's base data.
    /// Called only by implementing script events.
    /// </summary>
    /// <param name="_system">The command system this event exists within.</param>
    /// <param name="_name">The name of the event.</param>
    /// <param name="cancellable">Whether the event can be cancelled.</param>
    public ScriptEvent(ScriptEngine _system, string _name, bool cancellable)
    {
        Engine = _system;
        Name = _name.ToLowerFast();
        Cancellable = cancellable;
    }

    /// <summary>Update all variables from a ran script onto the event itself.</summary>
    /// <param name="vars">The vars to update.</param>
    public virtual void UpdateVariables(Dictionary<string, TemplateObject> vars)
    {
        Cancelled = BooleanTag.TryFor(vars["cancelled"]).Internal;
    }

    private ScriptIndex ProcessingPatch;

    private ScriptIndex CurrentlyProcessing;

    /// <summary>Calls a specific set of events.</summary>
    /// <param name="set">The set.</param>
    public void CallSet(ScriptSet set)
    {
        for (int i = 0; i < set.Scripts.Count; i++)
        {
            CurrentlyProcessing = set.Scripts[i].Key;
            try
            {
                // TODO: Handle cancelling stuff here?
                // IE, don't fire if cancelled and we don't want to fire?
                Dictionary<string, TemplateObject> Variables = new()
                    {
                        {  "context", new MapTag(GetVariables()) }
                    };
                Engine.ExecuteScript(set.Scripts[i].Value, ref Variables, out CommandQueue queue, DebugMode.MINIMAL);
                // TODO: Dedicated ContextTag type, with special handling of set/get, but can parse down to a MapTag.
                if (Variables != null && Variables.TryGetValue("context", out TemplateObject value))
                {
                    UpdateVariables(MapTag.For(value).Internal);
                }
                else
                {
                    Engine.Context.BadOutput("Context undefined for script event '" + Name + "'?");
                }
            }
            catch (Exception ex)
            {
                FreneticScriptUtilities.CheckException(ex);
                Engine.Context.BadOutput("Exception running script event: " + ex.ToString());
            }
            if (ProcessingPatch != null)
            {
                i = ProcessingPatch.SetIndex;
                ProcessingPatch = null;
            }
        }
        CurrentlyProcessing = null;
    }

    /// <summary>Calls the event, in full.</summary>
    public void CallFully()
    {
        for (int i = 0; i < Handlers.Count; i++)
        {
            CallSet(Handlers[i]);
            i = Handlers[i].Index;
        }
    }

    /// <summary>Calls the event.</summary>
    /// <param name="prio">The priority to call.</param>
    public void CallByPriority(double prio)
    {
        for (int i = 0; i < Handlers.Count; i++)
        {
            if (Handlers[i].Priority == prio)
            {
                CallSet(Handlers[i]);
                return;
            }
        }
    }

    /// <summary>Get all variables according the script event's current values.</summary>
    public virtual Dictionary<string, TemplateObject> GetVariables()
    {
        Dictionary<string, TemplateObject> vars = new()
        {
            { "cancelled", new TextTag(Cancelled ? "true": "false") }
        };
        return vars;
    }

    /// <summary>The name of this event.</summary>
    public readonly string Name;

    /// <summary>Create a copy of this script event, safe to run.</summary>
    /// <returns>The copy.</returns>
    public virtual ScriptEvent Duplicate()
    {
        return (ScriptEvent)MemberwiseClone();
    }
}
