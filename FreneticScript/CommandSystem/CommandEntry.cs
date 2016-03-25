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
        /// <param name="system">The command system to work from.</param>
        /// <param name="script">The name of the creating script.</param>
        /// <param name="line">The line in the creating script.</param>
        /// <param name="tabs">What tabulation to use when outputting this entry.</param>
        /// <returns>The command system.</returns>
        public static CommandEntry FromInput(string command, Commands system, string script, int line, string tabs)
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
            bool qtype = false;
            for (int i = 0; i < command.Length; i++)
            {
                if (command[i] == '"' && (!quoted || qtype))
                {
                    qtype = true;
                    quoted = !quoted;
                }
                else if (command[i] == '\'' && (!quoted || !qtype))
                {
                    qtype = false;
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
            string BaseCommandLow = BaseCommand.ToLowerFast();
            args.RemoveAt(0);
            AbstractCommand cmd;
            if (system.RegisteredCommands.TryGetValue(BaseCommandLow, out cmd))
            {
                return new CommandEntry(command, 0, 0, cmd, args, BaseCommand, marker, script, line, tabs) { WaitFor = waitfor };
            }
            return CreateInvalidOutput(BaseCommand, args, system, command, marker, waitfor, script, line, tabs);
        }

        /// <summary>
        /// Create an entry that represents an error message.
        /// </summary>
        public static CommandEntry CreateErrorOutput(string message, Commands system, string script, string tabs)
        {
            return new CommandEntry("error \"" + message.Replace('\"', '\'') + "\"", 0, 0, system.RegisteredCommands["error"],
                new List<Argument>() { new Argument() { Bits = new List<ArgumentBit>() { new TextArgumentBit(message, true) } } }, "error", 0, script, 0, tabs);

        }

        /// <summary>
        /// Create an entry that represents invalid output.
        /// </summary>
        public static CommandEntry CreateInvalidOutput(string name, List<Argument> _arguments,
            Commands system, string line, int marker, bool waitfor, string script, int linen, string tabs)
        {
            _arguments.Insert(0, system.TagSystem.SplitToArgument(name, false));
            return new CommandEntry(line, 0, 0, system.DebugInvalidCommand, _arguments, name, marker, script, linen, tabs) { WaitFor = waitfor };
                
        }

        /// <summary>
        /// The original command input.
        /// </summary>
        public string CommandLine;

        /// <summary>
        /// A list of all commands that were inside this command originally.
        /// </summary>
        public List<CommandEntry> InnerCommandBlock = null;

        /// <summary>
        /// The start of this command's braced block.
        /// </summary>
        public int BlockStart;

        /// <summary>
        /// The end of this command's braced block.
        /// </summary>
        public int BlockEnd;
        
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
        public CommandEntry(string _commandline, int bstart, int bend,
            AbstractCommand _command, List<Argument> _arguments, string _name, int _marker, string _script, int _line, string fairtabs)
        {
            BlockStart = bstart;
            BlockEnd = bend;
            CommandLine = _commandline;
            Command = _command;
            Arguments = _arguments;
            Name = _name;
            Marker = _marker;
            ScriptName = _script;
            ScriptLine = _line;
            FairTabulation = fairtabs;
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
        /// <returns>The full command string.</returns>
        public string FullString()
        {
            if (InnerCommandBlock == null)
            {
                return FairTabulation + CommandLine + ";\n";
            }
            else
            {
                string b = FairTabulation + CommandLine + "\n"
                    + FairTabulation + "{\n";
                foreach (CommandEntry entr in InnerCommandBlock)
                {
                    b += entr.FullString();
                }
                b += FairTabulation + "}\n";
                return b;
            }
        }

        /// <summary>
        /// Space to include in front of this tab when outputting it as text.
        /// </summary>
        public string FairTabulation = "";

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
            EMsg = "ERROR in script '" + TagParser.Escape(ScriptName) + "' on line " + (ScriptLine + 1) + ": " + EMsg;
            Queue.HandleError(this, EMsg);
            throw new ErrorInducedException();
        }

        /// <summary>
        /// Returns a duplicate of this command entry.
        /// </summary>
        /// <returns>The duplicate entry.</returns>
        public CommandEntry Duplicate()
        {
            return (CommandEntry)MemberwiseClone();
        }
    }
}
