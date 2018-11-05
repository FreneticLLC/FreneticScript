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
using FreneticScript.CommandSystem.Arguments;
using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Objects;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Reflection.Emit;
using FreneticScript.CommandSystem.QueueCmds;
using FreneticScript.TagHandlers.Common;
using FreneticScript.ScriptSystems;
using System.Threading;
using FreneticUtilities.FreneticExtensions;

namespace FreneticScript.CommandSystem
{
    /// <summary>
    /// Represents a series of commands, not currently being processed.
    /// </summary>
    public class CommandScript
    {
        /// <summary>
        /// Creates a script by file name.
        /// </summary>
        /// <param name="filename">The name of the file to execute.</param>
        /// <param name="system">The command system to get the script for.</param>
        /// <param name="status">A status output.</param>
        /// <returns>A command script, or null of the file does not exist.</returns>
        public static CommandScript GetByFileName(string filename, Commands system, out string status)
        {
            try
            {
                string fname = filename + ".cfg";
                CommandScript script = ScriptParser.SeparateCommands(filename, system.Context.ReadTextFile(fname), system);
                if (script == null)
                {
                    status = "Failed To Parse";
                }
                else
                {
                    status = "Valid";
                }
                return script;
            }
            catch (System.IO.FileNotFoundException)
            {
                status = "File Not Found";
                return null;
            }
            catch (Exception ex)
            {
                if (ex is ThreadAbortException)
                {
                    throw;
                }
                system.Context.BadOutput("Generating script for file '" + filename + "': " + ex.ToString());
                status = "Internal Exception";
                return null;
            }
        }

        /// <summary>
        /// The name of the script.
        /// </summary>
        public string Name;

        /// <summary>
        /// Whether this script is an anonymous script.
        /// </summary>
        public bool IsAnonymous;

        /// <summary>
        /// The full original construction string, if the script is anonymous (use <see cref="IsAnonymous"/>).
        /// </summary>
        public string AnonymousString;
        
        /// <summary>
        /// The default debugmode for this script (until set otherwise by the debug command).
        /// </summary>
        public DebugMode Debug = DebugMode.FULL;

        /// <summary>
        /// A compiled command structure set created by this script.
        /// </summary>
        public CompiledCommandStackEntry Compiled;

        /// <summary>
        /// An array of command entries on this script.
        /// </summary>
        public CommandEntry[] CommandArray;

        /// <summary>
        /// Constructs a new command script.
        /// </summary>
        /// <param name="_name">The name of the script.</param>
        /// <param name="_commands">All commands in the script.</param>
        /// <param name="adj">How far to negatively adjust the entries' block positions, if any.</param>
        /// <param name="mode">What debug mode to use for this script.</param>
        public CommandScript(string _name, List<CommandEntry> _commands, int adj = 0, DebugMode mode = DebugMode.FULL)
        {
            Debug = mode;
            Name = _name.ToLowerFast();
            CommandArray = _commands.ToArray();
            for (int i = 0; i < CommandArray.Length; i++)
            {
                CommandArray[i] = _commands[i].Duplicate();
                CommandArray[i].OwnIndex = i;
                CommandArray[i].BlockStart -= adj;
                CommandArray[i].BlockEnd -= adj;
            }
            Compiled = ScriptCompiler.Compile(this);
        }
        
        /// <summary>
        /// Creates a new queue for this script.
        /// </summary>
        /// <param name="system">The command system to make the queue in.</param>
        /// <returns>The created queue.</returns>
        public CommandQueue ToQueue(Commands system)
        {
            CommandQueue queue = new CommandQueue(this, system);
            queue.CommandStack.Push(Compiled.Duplicate());
            return queue;
        }

        /// <summary>
        /// Returns the name of the script.
        /// </summary>
        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// Gets the full command string that represents this script.
        /// </summary>
        /// <returns>The full command string.</returns>
        public string FullString()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < CommandArray.Length; i++)
            {
                if (!CommandArray[i].CommandLine.Contains('\0'))
                {
                    sb.Append(CommandArray[i].FullString());
                    if (CommandArray[i].InnerCommandBlock != null)
                    {
                        i = CommandArray[i].BlockEnd;
                    }
                }
            }
            return sb.ToString();
        }
    }
}
