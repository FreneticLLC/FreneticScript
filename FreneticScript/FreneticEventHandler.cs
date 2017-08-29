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
using FreneticScript.CommandSystem.QueueCmds;

namespace FreneticScript
{
    /// <summary>
    /// Handles events, with a simple priority system.
    /// Not particularly asyncable.
    /// Fires in priority order.
    /// </summary>
    /// <typeparam name="T">The event arguments type to use.</typeparam>
    public class FreneticScriptEventHandler<T> where T : FreneticEventArgs
    {
        /// <summary>
        /// All handlers in the event.
        /// TODO: Type choice - is this the most efficient option - for both reading and adding?
        /// </summary>
        public List<FreneticScriptEventEntry<T>> InternalActions = new List<FreneticScriptEventEntry<T>>();

        /// <summary>
        /// Fires the event handlers.
        /// </summary>
        /// <param name="args">The arguments to fire with.</param>
        public void Fire(T args)
        {
            foreach (FreneticScriptEventEntry<T> a in InternalActions)
            {
                args.Priority = a.Priority;
                a.Act(args);
            }
        }

        /// <summary>
        /// Adds an event entry with a specific priority.
        /// </summary>
        /// <param name="act">The action to run.</param>
        /// <param name="priority">The priority of the action.</param>
        public void Add(Action<T> act, int priority)
        {
            FreneticScriptEventEntry<T> fee = new FreneticScriptEventEntry<T>(act) { Priority = priority };
            int ind = 0;
            for (int i = 0; i < InternalActions.Count; i++)
            {
                if (InternalActions[i].Priority > priority)
                {
                    ind = i;
                    break;
                }
            }
            InternalActions.Insert(ind, fee);
        }

        /// <summary>
        /// Returns whether the given action is contained with the given priority.
        /// </summary>
        /// <param name="act">The action to test for.</param>
        /// <param name="priority">The priority of the action.</param>
        /// <returns></returns>
        public bool Contains(Action<T> act, int priority)
        {
            FreneticScriptEventEntry<T> fee = new FreneticScriptEventEntry<T>(act) { Priority = priority };
            return InternalActions.Contains(fee);
        }

        /// <summary>
        /// Remove an event entry with a specific priority.
        /// </summary>
        /// <param name="act">The action to no longer run.</param>
        /// <param name="priority">The priority of the action.</param>
        public void Remove(Action<T> act, int priority)
        {
            InternalActions.Remove(new FreneticScriptEventEntry<T>(act) { Priority = priority });
        }

        /// <summary>
        /// Adds an action to an event, with a default priority of exactly zero (0).
        /// </summary>
        /// <param name="evt">The original event.</param>
        /// <param name="act">The action to add.</param>
        /// <returns>The input event.</returns>
        public static FreneticScriptEventHandler<T> operator +(FreneticScriptEventHandler<T> evt, Action<T> act)
        {
            if (evt == null)
            {
                evt = new FreneticScriptEventHandler<T>();
            }
            evt.Add(act, 0);
            return evt;
        }

        /// <summary>
        /// Removes an action from an event.
        /// </summary>
        /// <param name="evt">The original event.</param>
        /// <param name="act">The action to remove.</param>
        /// <returns>The input event.</returns>
        public static FreneticScriptEventHandler<T> operator -(FreneticScriptEventHandler<T> evt, Action<T> act)
        {
            evt.InternalActions.Remove(new FreneticScriptEventEntry<T>(act) { Priority = 0 });
            return evt;
        }
    }

    /// <summary>
    /// Represents a prioritized event entry.
    /// </summary>
    /// <typeparam name="T">The type of the event arguments.</typeparam>
    public class FreneticScriptEventEntry<T> : IComparable<FreneticScriptEventEntry<T>>, IEquatable<FreneticScriptEventEntry<T>>
    {
        /// <summary>
        /// Constructs a prioritized event entry.
        /// </summary>
        /// <param name="a">The action to use.</param>
        public FreneticScriptEventEntry(Action<T> a)
        {
            Act = a;
        }

        /// <summary>
        /// The priority by which the action is run.
        /// </summary>
        public int Priority;

        /// <summary>
        /// The action used.
        /// </summary>
        public Action<T> Act;

        /// <summary>
        /// Compares this event entry to another.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns>The relative priority.</returns>
        public int CompareTo(FreneticScriptEventEntry<T> other)
        {
            if (ReferenceEquals(other, null))
            {
                return -1;
            }
            return other.Priority == Priority ? 0 : (other.Priority < Priority ? 1 : -1);
        }

        /// <summary>
        /// Returns whether this event entry is equal to another.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns>Whether they are equal.</returns>
        public bool Equals(FreneticScriptEventEntry<T> other)
        {
            if (ReferenceEquals(other, null))
            {
                return false;
            }
            return other.Act == Act && other.Priority == Priority;
        }

        /// <summary>
        /// Returns whether this event entry is equal to another.
        /// </summary>
        /// <param name="obj">The other.</param>
        /// <returns>Whether they are equal.</returns>
        public override bool Equals(object obj)
        {
            return obj is FreneticScriptEventEntry<T> && Act == ((FreneticScriptEventEntry<T>)obj).Act && Priority == ((FreneticScriptEventEntry<T>)obj).Priority;
        }

        /// <summary>
        /// Returns the hash code (merely the priority) of this event enty.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Priority;
        }

        /// <summary>
        /// Returns whether two event entries are equal.
        /// </summary>
        /// <param name="x">The first entry.</param>
        /// <param name="y">The second entry.</param>
        /// <returns>Whether they are equal.</returns>
        public static bool operator ==(FreneticScriptEventEntry<T> x, FreneticScriptEventEntry<T> y)
        {
            if (ReferenceEquals(x, null))
            {
                return ReferenceEquals(y, null);
            }
            return x.Equals(y);
        }

        /// <summary>
        /// Returns whether two event entries are NOT equal.
        /// </summary>
        /// <param name="x">The first entry.</param>
        /// <param name="y">The second entry.</param>
        /// <returns>Whether they are NOT equal.</returns>
        public static bool operator !=(FreneticScriptEventEntry<T> x, FreneticScriptEventEntry<T> y)
        {
            if (ReferenceEquals(x, null))
            {
                return !ReferenceEquals(y, null);
            }
            return !x.Equals(y);
        }
    }

    /// <summary>
    /// Represents the arguments to a Frenetic event.
    /// </summary>
    public class FreneticEventArgs : EventArgs
    {
        /// <summary>
        /// The priority this event fired with.
        /// </summary>
        public int Priority;
    }
}
