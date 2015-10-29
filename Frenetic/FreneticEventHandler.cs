using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Frenetic
{
    /// <summary>
    /// Handles events, with a simple priority system.
    /// Not particularly asyncable.
    /// Fires in the event added, can be fired in an alternately order by directly adding to <see cref="First"/> or <see cref="Last"/>.
    /// </summary>
    /// <typeparam name="T">The event arguments type to use.</typeparam>
    public class FreneticEventHandler<T> where T: EventArgs
    {
        /// <summary>
        /// The handlers that are fired earliest.
        /// </summary>
        public List<Action<T>> First = new List<Action<T>>();

        /// <summary>
        /// The handlers that are fired after the early handlers.
        /// </summary>
        public List<Action<T>> Normal = new List<Action<T>>();

        /// <summary>
        /// The handlers that are fired last.
        /// </summary>
        public List<Action<T>> Last = new List<Action<T>>();
        
        /// <summary>
        /// Fires the event handlers.
        /// </summary>
        /// <param name="args">The arguments to fire with.</param>
        public void Fire(T args)
        {
            foreach (Action<T> a in First)
            {
                a(args);
            }
            foreach (Action<T> a in Normal)
            {
                a(args);
            }
            foreach (Action<T> a in Last)
            {
                a(args);
            }
        }

        /// <summary>
        /// Adds an action to an event.
        /// </summary>
        /// <param name="evt">The original event.</param>
        /// <param name="act">The action to add.</param>
        /// <returns>The input event.</returns>
        public static FreneticEventHandler<T> operator +(FreneticEventHandler<T> evt, Action<T> act)
        {
            if (evt == null)
            {
                evt = new FreneticEventHandler<T>();
            }
            evt.Normal.Add(act);
            return evt;
        }

        /// <summary>
        /// Removes an action from an event.
        /// </summary>
        /// <param name="evt">The original event.</param>
        /// <param name="act">The action to remove.</param>
        /// <returns>The input event.</returns>
        public static FreneticEventHandler<T> operator -(FreneticEventHandler<T> evt, Action<T> act)
        {
            evt.Normal.Remove(act);
            return evt;
        }
    }
}
