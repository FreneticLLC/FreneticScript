//
// This file is part of FreneticScript, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of the MIT license.
// See README.md or LICENSE.txt in the FreneticScript source root for the contents of the license.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Objects;
using FreneticUtilities.FreneticExtensions;
using FreneticUtilities.FreneticToolkit;

namespace FreneticScript.CommandSystem.QueueCmds
{
    /// <summary>
    /// A command to allow running scripts from the script folder.
    /// </summary>
    public class RunfileCommand : AbstractCommand
    {
        // TODO: Meta!
        // @Waitable

        /// <summary>
        /// Constructs the run command.
        /// </summary>
        public RunfileCommand(FreneticEventHelper eventHelper)
        {
            Name = "runfile";
            Arguments = "<script to run>";
            Description = "Runs a script file.";
            // TODO: DEFINITION ARGS
            Waitable = true;
            Asyncable = true;
            MinimumArguments = 1;
            MaximumArguments = 1;
            ObjectTypes = new Action<ArgumentValidation>[]
            {
                TextTag.Validator
            };
            OnScriptRanPreEvent = new FreneticEvent<ScriptRanPreEventArgs>(eventHelper);
            OnScriptRanEvent = new FreneticEvent<ScriptRanEventArgs>(eventHelper);
            OnScriptRanPostEvent = new FreneticEvent<ScriptRanPostEventArgs>(eventHelper);
        }

        // TODO: Store these events elsewhere!

        /// <summary>
        /// The first event fired in a sequence of three.
        /// <para/>Fires when a a script is going to be ran, cancellable.
        /// <para/>Contains the name of the script only.
        /// <para/>Second: <see cref="OnScriptRanEvent"/>.
        /// <para/>Third: <see cref="OnScriptRanPostEvent"/>.
        /// </summary>
        public FreneticEvent<ScriptRanPreEventArgs> OnScriptRanPreEvent;

        /// <summary>
        /// The second event fired in a sequence of three.
        /// <para/>Fires when a a script is about to be ran, cancellable.
        /// <para/>Contains a validly constructed <see cref="CommandScript"/> object.
        /// <para/>First: <see cref="OnScriptRanPreEvent"/>.
        /// <para/>Third: <see cref="OnScriptRanPostEvent"/>.
        /// </summary>
        public FreneticEvent<ScriptRanEventArgs> OnScriptRanEvent;

        /// <summary>
        /// The third event fired in a sequence of three.
        /// <para/>Fires when a a script has been ran, monitor-only.
        /// <para/>First: <see cref="OnScriptRanPreEvent"/>.
        /// <para/>Second: <see cref="OnScriptRanEvent"/>.
        /// </summary>
        public FreneticEvent<ScriptRanPostEventArgs> OnScriptRanPostEvent ;

        /// <summary>
        /// Executes the run command.
        /// </summary>
        /// <param name="queue">The command queue involved.</param>
        /// <param name="entry">The command details to be ran.</param>
        public static void Execute(CommandQueue queue, CommandEntry entry)
        {
            string fname = entry.GetArgument(queue, 0).ToLowerFast();
            ScriptRanPreEventArgs args = new ScriptRanPreEventArgs() { ScriptName = fname };
            RunfileCommand rcmd = entry.Command as RunfileCommand;
            if (rcmd.OnScriptRanPreEvent != null)
            {
                rcmd.OnScriptRanPreEvent.Fire(args);
            }
            if (args.Cancelled)
            {
                entry.BadOutput(queue, "Script running cancelled via the ScriptRanPreEvent.");
                if (entry.WaitFor && queue.WaitingOn == entry)
                {
                    queue.WaitingOn = null;
                }
                return;
            }
            CommandScript script = queue.Engine.GetScriptFile(args.ScriptName, out string status);
            if (script != null)
            {
                ScriptRanEventArgs args2 = new ScriptRanEventArgs() { Script = script };
                if (rcmd.OnScriptRanEvent != null)
                {
                    rcmd.OnScriptRanEvent.Fire(args2);
                }
                if (args2.Cancelled)
                {
                    entry.BadOutput(queue, "Script running cancelled via the ScriptRanEvent.");
                    if (entry.WaitFor && queue.WaitingOn == entry)
                    {
                        queue.WaitingOn = null;
                    }
                    return;
                }
                if (script == null)
                {
                    entry.BadOutput(queue, "Script running nullified via the ScriptRanEvent.");
                    if (entry.WaitFor && queue.WaitingOn == entry)
                    {
                        queue.WaitingOn = null;
                    }
                    return;
                }
                script = args2.Script;
                if (entry.ShouldShowGood(queue))
                {
                    entry.GoodOutput(queue, "Running '" + TextStyle.Separate + fname + TextStyle.Base + "'...");
                }
                Dictionary<string, TemplateObject> vars = new Dictionary<string, TemplateObject>();
                queue.Engine.ExecuteScript(script, ref vars, out CommandQueue nqueue);
                if (entry.WaitFor && queue.WaitingOn == entry)
                {
                    if (!nqueue.Running)
                    {
                        queue.WaitingOn = null;
                    }
                    else
                    {
                        EntryFinisher fin = new EntryFinisher() { Entry = entry, Queue = queue };
                        nqueue.Complete += fin.Complete;
                    }
                }
                ScriptRanPostEventArgs args4 = new ScriptRanPostEventArgs() { Script = script };
                if (rcmd.OnScriptRanPostEvent != null)
                {
                    rcmd.OnScriptRanPostEvent.Fire(args4);
                }
                // TODO: queue.SetVariable("run_variables", new MapTag(nqueue.LowestVariables)); // TODO: use the ^= syntax here.
            }
            else
            {
                queue.HandleError(entry, "Cannot run script '" + TextStyle.Separate + fname + TextStyle.Base + "': " + status + "!");
                if (entry.WaitFor && queue.WaitingOn == entry)
                { 
                    queue.WaitingOn = null;
                }
            }
        }
    }


    /// <summary>
    /// A mini-class used for the callback for &amp;waitable commands.
    /// </summary>
    public class EntryFinisher
    {
        /// <summary>
        /// The entry being waited on.
        /// </summary>
        public CommandEntry Entry;

        /// <summary>
        /// The relevant queue.
        /// </summary>
        public CommandQueue Queue;

        /// <summary>
        /// Add this function as a callback to complete entry.
        /// </summary>
        public void Complete(object sender, CommandQueueEventArgs args)
        {
            if (Queue.WaitingOn == Entry)
            {
                Queue.WaitingOn = null;
            }
        }
    }

    /// <summary>
    /// Fires when a a script is going to be ran, cancellable.
    /// </summary>
    public class ScriptRanPreEventArgs: FreneticEventArgs
    {
        /// <summary>
        /// The name of the script requested to be run.
        /// </summary>
        public string ScriptName;

        /// <summary>
        /// Whether the script should be prevented from running.
        /// </summary>
        public bool Cancelled = false;
    }

    /// <summary>
    /// Fires when a a script is about to be ran, cancellable.
    /// </summary>
    public class ScriptRanEventArgs : FreneticEventArgs
    {
        /// <summary>
        /// The script that will be ran.
        /// Do not edit.
        /// </summary>
        public CommandScript Script;

        /// <summary>
        /// Whether the script should be prevented from running.
        /// </summary>
        public bool Cancelled = false;
    }
    
    /// <summary>
    /// Fires when a a script has been ran, monitor-only.
    /// </summary>
    public class ScriptRanPostEventArgs : FreneticEventArgs
    {
        /// <summary>
        /// The script that was ran.
        /// Do not edit.
        /// </summary>
        public CommandScript Script;
    }
}
