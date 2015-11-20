using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Frenetic.CommandSystem.QueueCmds;

namespace Frenetic
{
    /// <summary>
    /// Handles events, with a simple priority system.
    /// Not particularly asyncable.
    /// Fires in priority order.
    /// </summary>
    /// <typeparam name="T">The event arguments type to use.</typeparam>
    public class FreneticEventHandler<T> where T: EventArgs
    {
        /// <summary>
        /// All handlers in the event.
        /// </summary>
        public SortedSet<FreneticEventEntry<T>> InternalActions = new SortedSet<FreneticEventEntry<T>>();
        
        /// <summary>
        /// Fires the event handlers.
        /// </summary>
        /// <param name="args">The arguments to fire with.</param>
        public void Fire(T args)
        {
            foreach (FreneticEventEntry<T> a in InternalActions)
            {
                a.Act(a.Priority, args);
            }
        }

        /// <summary>
        /// Adds an event entry with a specific priority.
        /// </summary>
        /// <param name="act">The action to run.</param>
        /// <param name="priority">The priority of the action.</param>
        public void Add(Action<int, T> act, int priority)
        {
            InternalActions.Add(new FreneticEventEntry<T>(act) { Priority = priority });
        }
        
        /// <summary>
        /// Returns whether the given action is contained with the given priority.
        /// </summary>
        /// <param name="act">The action to test for.</param>
        /// <param name="priority">The priority of the action.</param>
        /// <returns></returns>
        public bool Contains(Action<int, T> act, int priority)
        {
            return InternalActions.Contains(new FreneticEventEntry<T>(act) { Priority = priority });
        }

        /// <summary>
        /// Remove an event entry with a specific priority.
        /// </summary>
        /// <param name="act">The action to no longer run.</param>
        /// <param name="priority">The priority of the action.</param>
        public void Remove(Action<int, T> act, int priority)
        {
            InternalActions.Remove(new FreneticEventEntry<T>(act) { Priority = priority });
        }

        /// <summary>
        /// Adds an action to an event, with a default priority of exactly zero (0).
        /// </summary>
        /// <param name="evt">The original event.</param>
        /// <param name="act">The action to add.</param>
        /// <returns>The input event.</returns>
        public static FreneticEventHandler<T> operator +(FreneticEventHandler<T> evt, Action<int, T> act)
        {
            if (evt == null)
            {
                evt = new FreneticEventHandler<T>();
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
        public static FreneticEventHandler<T> operator -(FreneticEventHandler<T> evt, Action<int, T> act)
        {
            evt.InternalActions.Remove(new FreneticEventEntry<T>(act) { Priority = 0 });
            return evt;
        }
    }
    
    /// <summary>
    /// Represents a prioritized event entry.
    /// </summary>
    /// <typeparam name="T">The type of the event arguments.</typeparam>
    public class FreneticEventEntry<T>: IComparable<FreneticEventEntry<T>>, IEquatable<FreneticEventEntry<T>>
    {
        /// <summary>
        /// Constructs a prioritized event entry.
        /// </summary>
        /// <param name="a">The action to use.</param>
        public FreneticEventEntry(Action<int, T> a)
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
        public Action<int, T> Act;

        /// <summary>
        /// Compares this event entry to another.
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns>The relative priority.</returns>
        public int CompareTo(FreneticEventEntry<T> other)
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
        public bool Equals(FreneticEventEntry<T> other)
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
            return obj is FreneticEventEntry<T> && Act == ((FreneticEventEntry<T>)obj).Act && Priority == ((FreneticEventEntry<T>)obj).Priority;
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
        public static bool operator ==(FreneticEventEntry<T> x, FreneticEventEntry<T> y)
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
        public static bool operator !=(FreneticEventEntry<T> x, FreneticEventEntry<T> y)
        {
            if (ReferenceEquals(x, null))
            {
                return !ReferenceEquals(y, null);
            }
            return !x.Equals(y);
        }
    }
}
