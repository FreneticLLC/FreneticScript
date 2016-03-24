using System;
using System.Collections.Generic;
using System.Text;
using FreneticScript.CommandSystem.Arguments;
using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Objects;

namespace FreneticScript.CommandSystem
{
    /// <summary>
    /// All the information for a command being currently run.
    /// </summary>
    public class CommandEntry
    {
        /// <summary>
        /// Creates a CommandEntry from the given input and queue information.
        /// </summary>
        /// <param name="command">The command line text itself.</param>
        /// <param name="_block">The command block that held this entry.</param>
        /// <param name="_owner">The command entry that owns the block that held this entry.</param>
        /// <param name="system">The command system to work from.</param>
        /// <param name="script">The name of the creating script.</param>
        /// <param name="line">The line in the creating script.</param>
        /// <returns>The command system.</returns>
        public static CommandEntry FromInput(string command, List<CommandEntry> _block, CommandEntry _owner, Commands system, string script, int line)
        {
            if (command.StartsWith("//"))
            {
                return null;
            }
            if (command.StartsWith("/"))
            {
                command = command.Substring(1);
            }
            command = command.Replace('\0', ' ');
            List<Argument> args = new List<Argument>();
            int start = 0;
            bool quoted = false;
            for (int i = 0; i < command.Length; i++)
            {
                if (command[i] == '"')
                {
                    quoted = !quoted;
                }
                else if (!quoted && command[i] == ' ' && (i - start > 0))
                {
                    string arg = command.Substring(start, i - start).Trim().Replace("\"", "");
                    if (arg.Length > 0)
                    {
                        args.Add(system.TagSystem.SplitToArgument(arg, quoted));
                    }
                    start = i + 1;
                }
            }
            if (command.Length - start > 0)
            {
                string arg = command.Substring(start, command.Length - start).Trim().Replace("\"", "");
                if (arg.Length > 0)
                {
                    args.Add(system.TagSystem.SplitToArgument(arg, quoted));
                }
            }
            if (args.Count == 0)
            {
                return null;
            }
            int marker = 0;
            string BaseCommand = args[0].ToString();
            if (BaseCommand.StartsWith("+") && BaseCommand.Length > 1)
            {
                marker = 1;
                BaseCommand = BaseCommand.Substring(1);
            }
            else if (BaseCommand.StartsWith("-") && BaseCommand.Length > 1)
            {
                marker = 2;
                BaseCommand = BaseCommand.Substring(1);
            }
            else if (BaseCommand.StartsWith("!") && BaseCommand.Length > 1)
            {
                marker = 3;
                BaseCommand = BaseCommand.Substring(1);
            }
            bool waitfor = false;
            if (BaseCommand.StartsWith("&") && BaseCommand.Length > 1)
            {
                waitfor = true;
                BaseCommand = BaseCommand.Substring(1);
            }
            string BaseCommandLow = BaseCommand.ToLowerInvariant();
            args.RemoveAt(0);
            AbstractCommand cmd;
            if (system.RegisteredCommands.TryGetValue(BaseCommandLow, out cmd))
            {
                return new CommandEntry(command, _block, _owner, cmd, args, BaseCommand, marker, script, line) { WaitFor = waitfor };
            }
            return CreateInvalidOutput(BaseCommand, _block, args, _owner, system, command, marker, waitfor, script, line);
        }

        /// <summary>
        /// Create an entry that represents an error message.
        /// </summary>
        public static CommandEntry CreateErrorOutput(string message, Commands system, string script)
        {
            return new CommandEntry("error \"" + message.Replace('\"', '\'') + "\"", null, null, system.RegisteredCommands["error"],
                new List<Argument>() { new Argument() { Bits = new List<ArgumentBit>() { new TextArgumentBit(message, true) } } }, "error", 0, script, 0);

        }

        /// <summary>
        /// Create an entry that represents invalid output.
        /// </summary>
        public static CommandEntry CreateInvalidOutput(string name, List<CommandEntry> _block, List<Argument> _arguments,
            CommandEntry _owner, Commands system, string line, int marker, bool waitfor, string script, int linen)
        {
            _arguments.Insert(0, system.TagSystem.SplitToArgument(name, false));
            return new CommandEntry(line, _block, _owner, system.DebugInvalidCommand, _arguments, name, marker, script, linen) { WaitFor = waitfor };
                
        }

        /// <summary>
        /// The original command input.
        /// </summary>
        public string CommandLine;

        /// <summary>
        /// If the command has a block of { braced } commands, this will contain that block.
        /// </summary>
        public List<CommandEntry> Block;

        /// <summary>
        /// What command entry object owns this entry, if any.
        /// </summary>
        public CommandEntry BlockOwner = null;

        /// <summary>
        /// Whether the &amp;waitable command entry is finished.
        /// </summary>
        public bool Finished = false;

        /// <summary>
        /// Whether the &amp;waitable command entry should be waited for.
        /// </summary>
        public bool WaitFor = false;

        /// <summary>
        /// The name of the creating script.
        /// </summary>
        public string ScriptName;

        /// <summary>
        /// The line number in the creating script.
        /// </summary>
        public int ScriptLine;

        /// <summary>
        /// Full constructor, recommended.
        /// </summary>
        public CommandEntry(string _commandline, List<CommandEntry> _block, CommandEntry _owner,
            AbstractCommand _command, List<Argument> _arguments, string _name, int _marker, string _script, int _line)
        {
            CommandLine = _commandline;
            Block = _block;
            BlockOwner = _owner;
            Command = _command;
            Arguments = _arguments;
            Name = _name;
            Marker = _marker;
            ScriptName = _script;
            ScriptLine = _line;
            if (Command == null)
            {
                throw new Exception("Invalid Command (null!)");
            }
        }

        /// <summary>
        /// Use at own risk.
        /// </summary>
        public CommandEntry()
        {
        }
        
        /// <summary>
        /// Gets the full command string that represents this command.
        /// </summary>
        /// <param name="tabulation">How much space to include in front of the commands.</param>
        /// <returns>The full command string.</returns>
        public string FullString(string tabulation = "")
        {
            if (Block == null)
            {
                return tabulation + CommandLine + ";";
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(tabulation + CommandLine + "\n");
                sb.Append("{\n");
                foreach (CommandEntry entry in Block)
                {
                    sb.Append(entry.FullString(tabulation + "\t") + "\n");
                }
                sb.Append("}\n");
                return sb.ToString();
            }
        }

        /// <summary>
        /// The command name input by the user.
        /// </summary>
        public string Name = "";

        /// <summary>
        /// The command that should execute this input.
        /// </summary>
        public AbstractCommand Command;

        /// <summary>
        /// The arguments input by the user.
        /// </summary>
        public List<Argument> Arguments;

        /// <summary>
        /// The command queue this command is running inside.
        /// </summary>
        public CommandQueue Queue = null;

        /// <summary>
        /// The object to use for any console / debug output.
        /// </summary>
        public Outputter Output = null;

        /// <summary>
        /// An object set by the command, if any.
        /// </summary>
        public AbstractCommandEntryData Data;

        /// <summary>
        /// What marker was used. 0 = none, 1 = +, 2 = -, 3 = !
        /// </summary>
        public int Marker = 0;

        /// <summary>
        /// Gets an argument at a specified place, handling any tags.
        /// </summary>
        /// <param name="place">The argument place number.</param>
        /// <returns>The parsed argument.</returns>
        public TemplateObject GetArgumentObject(int place)
        {
            if (place >= Arguments.Count || place < 0)
            {
                throw new ArgumentOutOfRangeException("place", "Value must be greater than 0 and less than command input argument count");
            }
            if (Queue.ParseTags)
            {
                return Arguments[place].Parse(TextStyle.Color_Simple, Queue.Variables, Queue.Debug, Error);
            }
            else
            {
                return new TextTag(Arguments[place].ToString());
            }
        }

        /// <summary>
        /// Gets an argument at a specified place, handling any tags - returning a string.
        /// </summary>
        /// <param name="place">The argument place number.</param>
        /// <returns>The parsed argument as a string.</returns>
        public string GetArgument(int place)
        {
            if (place >= Arguments.Count || place < 0)
            {
                throw new ArgumentOutOfRangeException("place", "Value must be greater than 0 and less than command input argument count");
            }
            if (Queue.ParseTags)
            {
                return Arguments[place].Parse(TextStyle.Color_Simple, Queue.Variables, Queue.Debug, Error).ToString();
            }
            else
            {
                return Arguments[place].ToString();
            }
        }

        /// <summary>
        /// Gets all arguments piled together into a string.
        /// </summary>
        /// <param name="index">The index to start at.</param>
        /// <returns>The combined string.</returns>
        public string AllArguments(int index = 0)
        {
            StringBuilder result = new StringBuilder(CommandLine.Length);
            for (int i = index; i < Arguments.Count; i++)
            {
                result.Append(GetArgument(i));
                if (i + 1 < Arguments.Count)
                {
                    result.Append(" ");
                }
            }
            return result.ToString();
        }

        /// <summary>
        /// Gets all arguments (without parsing) piled together into a string.
        /// </summary>
        /// <param name="index">The index to start at.</param>
        /// <returns>The combined string.</returns>
        public string AllOriginalArguments(int index = 0)
        {
            StringBuilder result = new StringBuilder(CommandLine.Length);
            for (int i = index; i < Arguments.Count; i++)
            {
                result.Append(Arguments[i]);
                if (i + 1 < Arguments.Count)
                {
                    result.Append(" ");
                }
            }
            return result.ToString();
        }

        /// <summary>
        /// Used to output requested information.
        /// </summary>
        /// <param name="text">The text to output, with tags included.</param>
        public void Info(string text)
        {
            Output.Good(text, DebugMode.MINIMAL);
            if (Queue.Outputsystem != null)
            {
                Queue.Outputsystem.Invoke(text, MessageType.INFO);
            }
        }

        /// <summary>
        /// Used to output a success message.
        /// </summary>
        /// <param name="text">The text to output, with tags included.</param>
        public void Good(string text)
        {
            if (Queue.Debug == DebugMode.FULL)
            {
                Output.Good(text, DebugMode.MINIMAL);
                if (Queue.Outputsystem != null)
                {
                    Queue.Outputsystem.Invoke(text, MessageType.GOOD);
                }
            }
        }

        /// <summary>
        /// Returns whether commands should output 'good' results.
        /// </summary>
        /// <returns>Whether commands should output 'good' results.</returns>
        public bool ShouldShowGood()
        {
            return Queue.Debug == DebugMode.FULL;
        }

        /// <summary>
        /// Used to output a failure message. This is considered a 'warning' and will not induce an error.
        /// </summary>
        /// <param name="text">The text to output, with tags included.</param>
        public void Bad(string text)
        {
            text = "WARNING in script '" + TagParser.Escape(ScriptName) + "' on line " + (ScriptLine + 1) + ": " + text;
            if (Queue.Debug <= DebugMode.MINIMAL)
            {
                Output.Bad(text, DebugMode.MINIMAL);
                if (Queue.Outputsystem != null)
                {
                    Queue.Outputsystem.Invoke(text, MessageType.BAD);
                }
            }
        }

        /// <summary>
        /// Used to indicate an error has occured, and have the system react accordingly.
        /// It is recommended you "return;" immediately after invoking this - this is not needed, as an exception is thrown, but instead used to keep code clear that the method stops there.
        /// </summary>
        /// <param name="EMsg">The error message.</param>
        public void Error(string EMsg)
        {
            EMsg = "ERROR in script '" + TagParser.Escape(ScriptName) + "' on line " + (ScriptLine + 1) + ": " + TagParser.Escape(EMsg);
            Queue.HandleError(this, EMsg);
            throw new ErrorInducedException();
        }

        /// <summary>
        /// Returns a duplicate of this command entry.
        /// </summary>
        /// <param name="NewOwner">The new owner of the command entry.</param>
        /// <returns>The duplicate entry.</returns>
        public CommandEntry Duplicate(CommandEntry NewOwner = null)
        {
            CommandEntry entry = new CommandEntry();
            entry.Arguments = new List<Argument>(Arguments);
            if (Block == null)
            {
                entry.Block = null;
            }
            else
            {
                entry.Block = new List<CommandEntry>();
                for (int i = 0; i < Block.Count; i++)
                {
                    entry.Block.Add(Block[i].Duplicate(entry));
                }
            }
            entry.BlockOwner = NewOwner;
            entry.Command = Command;
            entry.CommandLine = CommandLine;
            entry.Name = Name;
            entry.Output = Output;
            entry.Queue = Queue;
            entry.ScriptName = ScriptName;
            entry.ScriptLine = ScriptLine;
            if (Data != null)
            {
                entry.Data = Data.Duplicate();
            }
            else
            {
                entry.Data = null;
            }
            entry.Marker = Marker;
            return entry;
        }
    }
}
