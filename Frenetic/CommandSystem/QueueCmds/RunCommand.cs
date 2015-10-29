using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Frenetic.TagHandlers;
using Frenetic.TagHandlers.Objects;
using Frenetic.CommandSystem.CommandEvents;

namespace Frenetic.CommandSystem.QueueCmds
{
    /// <summary>
    /// A command to allow running scripts from the script folder.
    /// </summary>
    public class RunCommand : AbstractCommand
    {
        // TODO: Docs
        // @Waitable

        /// <summary>
        /// Constructs the run command.
        /// </summary>
        public RunCommand()
        {
            Name = "run";
            Arguments = "<script to run>";
            Description = "Runs a script file.";
            // TODO: DEFINITION ARGS
            IsFlow = true;
            Waitable = true;
            Asyncable = true;
        }

        /// <summary>
        /// The first event fired in a sequence of four.
        /// <para/>Fires when a a script is going to be ran, cancellable.
        /// <para/>Contains the name of the script only.
        /// <para/>Second: <see cref="OnScriptRanEvent"/>.
        /// Third: <see cref="OnScriptRanMonitorEvent"/>.
        /// Fourth: <see cref="OnScriptRanPostEvent"/>
        /// </summary>
        public EventHandler<ScriptRanPreEventArgs> OnScriptRanPreEvent;

        /// <summary>
        /// The second event fired in a sequence of four.
        /// <para/>Fires when a a script is about to be ran, cancellable.
        /// <para/>Contains a validly constructed <see cref="CommandScript"/> object.
        /// <para/>First: <see cref="OnScriptRanPreEvent"/>.
        /// Third: <see cref="OnScriptRanMonitorEvent"/>.
        /// Fourth: <see cref="OnScriptRanPostEvent"/>.
        /// </summary>
        public EventHandler<ScriptRanEventArgs> OnScriptRanEvent;

        /// <summary>
        /// The third event fired in a sequence of four.
        /// <para/>Fires when a a script is about to be ran, monitor-only.
        /// <para/>Contains a validly constructed <see cref="CommandScript"/> object.
        /// <para/>First: <see cref="OnScriptRanPreEvent"/>.
        /// Second: <see cref="OnScriptRanEvent"/>.
        /// Fourth: <see cref="OnScriptRanPostEvent"/>.
        /// </summary>
        public EventHandler<ScriptRanMonitorEventArgs> OnScriptRanMonitorEvent;

        /// <summary>
        /// The four event fired in a sequence of four.
        /// <para/>Fires when a a script has been ran, monitor-only.
        /// <para/>First: <see cref="OnScriptRanPreEvent"/>.
        /// Second: <see cref="OnScriptRanEvent"/>.
        /// Third: <see cref="OnScriptRanMonitorEvent"/>.
        /// </summary>
        public EventHandler<ScriptRanPostEventArgs> OnScriptRanPostEvent;

        /// <summary>
        /// Executes the run command.
        /// </summary>
        /// <param name="entry">The command details to be ran.</param>
        public override void Execute(CommandEntry entry)
        {
            if (entry.Arguments.Count < 1)
            {
                ShowUsage(entry);
                entry.Finished = true;
                return;
            }
            string fname = entry.GetArgument(0).ToLower();
            ScriptRanPreEventArgs args = new ScriptRanPreEventArgs();
            args.ScriptName = fname;
            if (OnScriptRanPreEvent != null)
            {
                OnScriptRanPreEvent(this, args);
            }
            if (args.Cancelled)
            {
                entry.Bad("Script running cancelled via the ScriptRanPreEvent.");
                return;
            }
            CommandScript script = entry.Queue.CommandSystem.GetScript(args.ScriptName);
            if (script != null)
            {
                ScriptRanEventArgs args2 = new ScriptRanEventArgs();
                args2.Script = script;
                if (OnScriptRanEvent != null)
                {
                    OnScriptRanEvent(this, args2);
                }
                if (args2.Cancelled)
                {
                    entry.Bad("Script running cancelled via the ScriptRanEvent.");
                    return;
                }
                if (script == null)
                {
                    entry.Bad("Script running nullified via the ScriptRanEvent.");
                    return;
                }
                script = args2.Script;
                ScriptRanMonitorEventArgs args3 = new ScriptRanMonitorEventArgs();
                args3.Script = script;
                if (OnScriptRanMonitorEvent != null)
                {
                    OnScriptRanMonitorEvent(this, args3);
                }
                entry.Good("Running '<{color.emphasis}>" + TagParser.Escape(fname) + "<{color.base}>'...");
                CommandQueue queue;
                entry.Queue.CommandSystem.ExecuteScript(script, null, out queue);
                if (!queue.Running)
                {
                    entry.Finished = true;
                }
                else
                {
                    EntryFinisher fin = new EntryFinisher() { Entry = entry };
                    queue.Complete += fin.Complete;
                }
                ScriptRanPostEventArgs args4 = new ScriptRanPostEventArgs();
                args4.Script = script;
                args4.Determinations = new List<string>(queue.Determinations);
                if (OnScriptRanPostEvent != null)
                {
                    OnScriptRanPostEvent(this, args4);
                }
                ListTag list = new ListTag(queue.Determinations);
                entry.Queue.SetVariable("run_determinations", list);
            }
            else
            {
                entry.Bad("Cannot run script '<{color.emphasis}>" + TagParser.Escape(fname) + "<{color.base}>': file does not exist!");
                entry.Finished = true;
            }
        }
    }

    /// <summary>
    /// Fires when a a script is going to be ran, cancellable.
    /// </summary>
    public class ScriptRanPreEventArgs: EventArgs
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
    public class ScriptRanEventArgs : EventArgs
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
    /// Fires when a a script is about to be ran, monitor-only.
    /// </summary>
    public class ScriptRanMonitorEventArgs : EventArgs
    {
        /// <summary>
        /// The script that will be run.
        /// Do not edit.
        /// </summary>
        public CommandScript Script;
    }

    /// <summary>
    /// Fires when a a script has been ran, monitor-only.
    /// </summary>
    public class ScriptRanPostEventArgs : EventArgs
    {
        /// <summary>
        /// The script that was ran.
        /// Do not edit.
        /// </summary>
        public CommandScript Script;

        /// <summary>
        /// All determines of the script run.
        /// Do not edit.
        /// </summary>
        public List<string> Determinations;
    }
}
