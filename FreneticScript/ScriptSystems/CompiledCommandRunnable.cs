//
// This file is part of FreneticScript, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of the MIT license.
// See README.md or LICENSE.txt in the FreneticScript source root for the contents of the license.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Reflection.Emit;
using FreneticScript.CommandSystem;

namespace FreneticScript.ScriptSystems
{
    /// <summary>
    /// Abstract class for compiled runnables.
    /// </summary>
    public abstract class CompiledCommandRunnable
    {
        /// <summary>
        /// This class's <see cref="Run(CommandQueue)"/> method.
        /// </summary>
        public static readonly MethodInfo RunMethod = typeof(CompiledCommandRunnable).GetMethod(nameof(CompiledCommandRunnable.Run), new Type[] { typeof(CommandQueue) });

        /// <summary>
        /// This class's <see cref="Entry"/> field.
        /// </summary>
        public static readonly FieldInfo EntryField = typeof(CompiledCommandRunnable).GetField(nameof(Entry));

        /// <summary>
        /// Represents the <see cref="LocalVariables"/> field.
        /// </summary>
        public static readonly FieldInfo LocalVariablesField = typeof(CompiledCommandRunnable).GetField(nameof(LocalVariables));

        /// <summary>
        /// Represents the <see cref="Index"/> field.
        /// </summary>
        public static readonly FieldInfo IndexField = typeof(CompiledCommandRunnable).GetField(nameof(Index));

        /// <summary>
        /// Runs the runnable.
        /// </summary>
        /// <param name="queue">The queue to run on.</param>
        public abstract void Run(CommandQueue queue);

        /// <summary>
        /// Variables local to the compiled function.
        /// </summary>
        public ObjectHolder[] LocalVariables;
        
        /// <summary>
        /// How much debug information this portion of the stack should show.
        /// </summary>
        public DebugMode Debug;

        /// <summary>
        /// Run this when the runnable STOPs.
        /// </summary>
        public Action Callback;

        /// <summary>
        /// The index of the currently running command.
        /// </summary>
        public int Index;

        /// <summary>
        /// Gets the current command entry, or null.
        /// </summary>
        public CommandEntry CurrentCommandEntry
        {
            get
            {
                return Entry.At(Index);
            }
        }

        /// <summary>
        /// All entry data available in this currently running section.
        /// </summary>
        public AbstractCommandEntryData[] EntryData;

        /// <summary>
        /// The base stack entry.
        /// </summary>
        public readonly CompiledCommandStackEntry Entry;

        /// <summary>
        /// The current queue, or null.
        /// </summary>
        public CommandQueue CurrentQueue = null;

        /// <summary>
        /// Duplicates the runnable object.
        /// </summary>
        /// <returns>The duplicate.</returns>
        public CompiledCommandRunnable Duplicate()
        {
            CompiledCommandRunnable newCopy = MemberwiseClone() as CompiledCommandRunnable;
            ObjectHolder[] origLvars = LocalVariables;
            ObjectHolder[] lvars = newCopy.LocalVariables = new ObjectHolder[origLvars.Length];
            for (int i = 0; i < lvars.Length; i++)
            {
                lvars[i] = new ObjectHolder() { Internal = origLvars[i].Internal };
            }
            newCopy.EntryData = new AbstractCommandEntryData[EntryData.Length];
            return newCopy;
        }
    }
}
