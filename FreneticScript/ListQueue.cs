using System;
using System.Collections.Generic;
using System.Linq;

namespace Frenetic
{
    /// <summary>
    /// Holds an array, managing it both like a list and like a queue depending on need.
    /// </summary>
    public class ListQueue<T>
    {
        /// <summary>
        /// The internal array.
        /// NOTE: INTERNAL USE ONLY.
        /// </summary>
        public T[] Objects;

        /// <summary>
        /// The head of the array.
        /// IE, the index of the first real object.
        /// NOTE: INTERNAL USE ONLY.
        /// </summary>
        public int Head;

        /// <summary>
        /// The true length of the array, not counting things before the head or after the end.
        /// </summary>
        public int Length;

        /// <summary>
        /// How far the <see cref="Head"/> can get before the buffer is forcibly shortened.
        /// Defaults to 500.
        /// </summary>
        public int Max = 500;
        
        /// <summary>
        /// Access a member of the <see cref="ListQueue{T}"/>
        /// </summary>
        /// <param name="index">The position in the public side of the list to read from.</param>
        /// <returns>The read object.</returns>
        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= Length)
                {
                    throw new IndexOutOfRangeException();
                }
                return Objects[Head + index];
            }
            set
            {
                if (index < 0 || index >= Length)
                {
                    throw new IndexOutOfRangeException();
                }
                Objects[Head + index] = value;
            }
        }

        /// <summary>
        /// Initializes the <see cref="ListQueue{T}"/> with specified initial entries.
        /// </summary>
        /// <param name="entries">.</param>
        public ListQueue(IList<T> entries)
        {
            Objects = new T[entries.Count];
            // TODO: Internal memcpy?
            for (int i = 0; i < entries.Count; i++)
            {
                Objects[i] = entries[i];
            }
            Head = 0;
            Length = entries.Count;
        }

        /// <summary>
        /// Initializes the <see cref="ListQueue{T}"/> with a specified capacity.
        /// </summary>
        /// <param name="capacity">The number of items to expect to hold, is non-definite.</param>
        public ListQueue(int capacity = 100)
        {
            Objects = new T[capacity];
            Head = 0;
            Length = 0;
        }
        
        /// <summary>
        /// Returns the item at head of the queue, without removing it, if it has one.
        /// </summary>
        /// <returns>The item found.</returns>
        public T Peek()
        {
            if (Length <= 0)
            {
                throw new InvalidOperationException("Cannot read an empty queue");
            }
            return Objects[Head];
        }

        /// <summary>
        /// Takes the first object off the queue and removes it, if it has one.
        /// On rare occasions, expensive (if head reaches the max).
        /// </summary>
        /// <returns>The head object.</returns>
        public T Pop()
        {
            T obj = Peek();
            Head++;
            Length--;
            if (Length == 0)
            {
                Head = 0;
            }
            else if (Head > Max)
            {
                MoveHeadBack();
            }
            return obj;
        }

        /// <summary>
        /// Adds an object to the end of the queue.
        /// On rare occasions, expensive (if internal buffer must expand).
        /// </summary>
        /// M<param name="obj">The object to push onto the queue.</param>
        public void Push(T obj)
        {
            int target = Head + Length;
            if (target >= Objects.Length)
            {
                Expand(Objects.Length); // Double in size
                target = Head + Length;
            }
            Objects[target] = obj;
            Length++;
        }

        /// <summary>
        /// Return the <see cref="Head"/> to a 0 index.
        /// Expensive.
        /// </summary>
        public void MoveHeadBack()
        {
            T[] objs = new T[Objects.Length];
            Array.Copy(Objects, Head, objs, 0, Length);
            Head = 0;
            Objects = objs;
        }

        /// <summary>
        /// Expands the <see cref="ListQueue{T}"/>'s internal buffer.
        /// Expensive.
        /// </summary>
        /// <param name="amount">The amount to expand it by.</param>
        public void Expand(int amount)
        {
            if (amount <= 0)
            {
                amount = 1; // If we are told to expand by zero or less, just expand by one. This allows for easy handling of size-doubling an empty ListQueue.
            }
            T[] newobjs = new T[Objects.Length + amount];
            Array.Copy(Objects, Head, newobjs, 0, Length);
            Head = 0;
            Objects = newobjs;
        }

        /// <summary>
        /// Effectively empties the ListQueue by scrapping the original internal buffer, and setting Length and Head to 0.
        /// Mildly expensive.
        /// </summary>
        public void Clear()
        {
            Objects = new T[Objects.Length];
            Head = 0;
            Length = 0;
        }

        /// <summary>
        /// Inserts entries to the buffer.
        /// Expensive.
        /// </summary>
        /// <param name="index">The place to start inserting at.</param>
        /// <param name="entries">The entries to insert.</param>
        public void Insert(int index, T[] entries)
        {
            if (index < 0 || index > Length)
            {
                throw new IndexOutOfRangeException();
            }
            else if (entries.Length == 0)
            {
                return;
            }
            else if (index == Length)
            {
                if (entries.Length + Length > Objects.Length)
                {
                    Expand(entries.Length);
                }
                Array.Copy(entries.ToArray(), 0, Objects, Head + Length, entries.Length);
                Length += entries.Length;
            }
            else if (index == 0)
            {
                if (Length + entries.Length <= Objects.Length)
                {
                    if (Head != entries.Length)
                    {
                        Array.Copy(Objects, Head, Objects, entries.Length, Length);
                    }
                    Array.Copy(entries, 0, Objects, 0, entries.Length);
                }
                else
                {
                    T[] nobjs = new T[Objects.Length + entries.Length];
                    Array.Copy(entries, 0, nobjs, 0, entries.Length);
                    Array.Copy(Objects, Head, nobjs, entries.Length, Length);
                    Objects = nobjs;
                }
                Length += entries.Length;
                Head = 0;
            }
            else
            {
                if (Length + entries.Length <= Objects.Length)
                {
                    Array.Copy(Objects, Head, Objects, 0, index);
                    Array.Copy(Objects, Head, Objects, entries.Length + index, Length);
                    Array.Copy(entries, 0, Objects, index, entries.Length);
                }
                else
                {
                    T[] nobjs = new T[Objects.Length + entries.Length];
                    Array.Copy(Objects, Head, nobjs, 0, index);
                    Array.Copy(entries, 0, nobjs, index, entries.Length);
                    Array.Copy(Objects, Head, nobjs, entries.Length + index, Length);
                    Objects = nobjs;
                }
                Head = 0;
                Length += entries.Length;
            }
        }

        // TODO: RemoveRange(x, y)
    }
}
