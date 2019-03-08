//
// This file is part of FreneticScript, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of the MIT license.
// See README.md or LICENSE.txt in the FreneticScript source root for the contents of the license.
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
using FreneticScript.TagHandlers.CommonBases;
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
        /// <param name="scriptName">The name of the script file to execute.</param>
        /// <param name="system">The command system to get the script for.</param>
        /// <param name="status">A status output.</param>
        /// <returns>A command script, or null of the file does not exist.</returns>
        public static CommandScript GetByFileName(string scriptName, ScriptEngine system, out string status)
        {
            try
            {
                string fileName = system.ScriptsFolder + "/" + scriptName + "." + system.FileExtension;
                CommandScript script = ScriptParser.SeparateCommands(scriptName, system.Context.ReadTextFile(fileName), system);
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
                FreneticScriptUtilities.CheckException(ex);
                system.Context.BadOutput("Generating script for file '" + scriptName + "': " + ex.ToString());
                status = "Internal Exception";
                return null;
            }
        }

        /// <summary>
        /// The name of the script.
        /// </summary>
        public string Name;

        /// <summary>
        /// Standard type name: Function.
        /// </summary>
        public const string TYPE_NAME_FUNCTION = "Function";

        /// <summary>
        /// Standard type name: File.
        /// </summary>
        public const string TYPE_NAME_FILE = "File";

        /// <summary>
        /// Standard type name: Event.
        /// </summary>
        public const string TYPE_NAME_EVENT = "Event";

        /// <summary>
        /// Standard type name: Anonymous.
        /// </summary>
        public const string TYPE_NAME_ANONYMOUS = "Anonymous";

        /// <summary>
        /// The backing command system.
        /// </summary>
        public ScriptEngine System;

        /// <summary>
        /// The name of the type of script. Function, File, Event, and Anonymous are common.
        /// </summary>
        public string TypeName;

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
        /// <param name="_typeName">The name of the script type.</param>
        /// <param name="_commands">All commands in the script.</param>
        /// <param name="engine">The backing script engine.</param>
        /// <param name="adj">How far to negatively adjust the entries' block positions, if any.</param>
        /// <param name="mode">What debug mode to use for this script.</param>
        public CommandScript(string _name, string _typeName, CommandEntry[] _commands, ScriptEngine engine, int adj = 0, DebugMode mode = DebugMode.FULL)
        {
            Debug = mode;
            Name = _name.ToLowerFast();
            TypeName = _typeName;
            CommandArray = new CommandEntry[_commands.Length];
            System = engine;
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
        public CommandQueue ToQueue(ScriptEngine system)
        {
            CommandQueue queue = new CommandQueue(this, system);
            queue.RunningStack.Push(Compiled.ReferenceCompiledRunnable.Duplicate());
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
