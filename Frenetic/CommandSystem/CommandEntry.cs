using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Frenetic.CommandSystem.CommonCmds;

namespace Frenetic.CommandSystem
{
    /// <summary>
    /// All the information for a command being currently run.
    /// </summary>
    public class CommandEntry
    {
        /// <summary>
        /// Creates a CommandEntry from the given input and queue information.
        /// </summary>
        /// <param name="_command">The command line text itself</param>
        /// <param name="_block">The command block that held this entry</param>
        /// <param name="_owner">The command entry that owns the block that held this entry</param>
        /// <param name="system">The command system to work from</param>
        /// <returns>The command system</returns>
        public static CommandEntry FromInput(string command, List<CommandEntry> _block, CommandEntry _owner, Commands system)
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
            List<string> args = new List<string>();
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
                        args.Add(arg);
                    }
                    start = i + 1;
                }
            }
            if (command.Length - start > 0)
            {
                string arg = command.Substring(start, command.Length - start).Trim().Replace("\"", "");
                if (arg.Length > 0)
                {
                    args.Add(arg);
                }
            }
            if (args.Count == 0)
            {
                return null;
            }
            int marker = 0;
            string BaseCommand = args[0];
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
            string BaseCommandLow = BaseCommand.ToLower();
            args.RemoveAt(0);
            AbstractCommand cmd;
            if (system.RegisteredCommands.TryGetValue(BaseCommandLow, out cmd))
            {
                return new CommandEntry(command, _block, _owner, cmd, args, BaseCommand, marker);
            }
            return CreateInvalidOutput(BaseCommand, _block, args, _owner, system, command, marker);
        }

        /// <summary>
        /// Create an entry that represents invalid output.
        /// </summary>
        public static CommandEntry CreateInvalidOutput(string name, List<CommandEntry> _block,
            List<string> _arguments, CommandEntry _owner, Commands system, string line, int marker)
        {
            _arguments.Insert(0, name);
            return new CommandEntry(line, _block, _owner, system.DebugInvalidCommand, _arguments, name, marker);
                
        }

        /// <summary>
        /// The command itself.
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
        /// Full constructor, recommended.
        /// </summary>
        public CommandEntry(string _commandline, List<CommandEntry> _block, CommandEntry _owner,
            AbstractCommand _command, List<string> _arguments, string _name, int _marker)
        {
            CommandLine = _commandline;
            Block = _block;
            BlockOwner = _owner;
            Command = _command;
            Arguments = _arguments;
            Name = _name;
            Marker = _marker;
        }

        /// <summary>
        /// Use at own risk.
        /// </summary>
        public CommandEntry()
        {
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
        public List<string> Arguments;

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
        /// <param name="place">The argument place number</param>
        /// <returns>The parsed argument</returns>
        public string GetArgument(int place)
        {
            if (place >= Arguments.Count || place < 0)
            {
                throw new ArgumentOutOfRangeException("Value must be greater than 0 and less than command input argument count");
            }
            if (Queue.ParseTags)
            {
                return Queue.CommandSystem.TagSystem.ParseTags(Arguments[place], TextStyle.Color_Simple, Queue.Variables, Queue.Debug);
            }
            else
            {
                return Arguments[place];
            }
        }

        /// <summary>
        /// Gets all arguments piled together into a string.
        /// </summary>
        /// <param name="index">The index to start at</param>
        /// <returns>The combined string</returns>
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
        /// <param name="index">The index to start at</param>
        /// <returns>The combined string</returns>
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
        /// <param name="tagged_text">The text to output, with tags included</param>
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
        /// <param name="tagged_text">The text to output, with tags included</param>
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
        /// Used to output a failure message.
        /// </summary>
        /// <param name="tagged_text">The text to output, with tags included</param>
        public void Bad(string text)
        {
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
        /// Returns a duplicate of this command entry.
        /// </summary>
        /// <param name="NewOwner">The new owner of the command entry</param>
        /// <returns>The duplicate entry</returns>
        public CommandEntry Duplicate(CommandEntry NewOwner = null)
        {
            CommandEntry entry = new CommandEntry();
            entry.Arguments = new List<string>(Arguments);
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
