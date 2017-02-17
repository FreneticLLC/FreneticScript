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
        /// <param name="tabs">How much tabulation tihs command had.</param>
        /// <returns>The command system.</returns>
        public static CommandEntry FromInput(string command, Commands system, string script, int line, string tabs)
        {
            try
            {
                if (command.StartsWith("/"))
                {
                    command = command.Substring(1);
                }
                command = command.Replace('\0', ' ');
                List<Argument> args = new List<Argument>();
                int start = 0;
                bool quoted = false;
                bool qtype = false;
                bool thisArgQuoted = false;
                for (int i = 0; i < command.Length; i++)
                {
                    if (command[i] == '"' && (!quoted || qtype))
                    {
                        qtype = true;
                        quoted = !quoted;
                        thisArgQuoted = true;
                    }
                    else if (command[i] == '\'' && (!quoted || !qtype))
                    {
                        qtype = false;
                        quoted = !quoted;
                        thisArgQuoted = true;
                    }
                    else if (command[i] == '\n' && !quoted)
                    {
                        command = (i + 1 < command.Length) ? command.Substring(0, i) + command.Substring(i + 1) : command.Substring(0, i);
                    }
                    else if (!quoted && command[i] == ' ')
                    {
                        if (i - start > 0)
                        {
                            string arg = command.Substring(start, i - start).Trim().Replace('\'', '"').Replace("\"", "");
                            args.Add(system.TagSystem.SplitToArgument(arg, thisArgQuoted));
                            start = i + 1;
                            thisArgQuoted = false;
                        }
                        else
                        {
                            start = i + 1;
                        }
                    }
                }
                if (command.Length - start > 0)
                {
                    string arg = command.Substring(start, command.Length - start).Trim().Replace('\'', '"').Replace("\"", "");
                    args.Add(system.TagSystem.SplitToArgument(arg, thisArgQuoted));
                }
                if (args.Count == 0)
                {
                    return null;
                }
                Dictionary<string, Argument> nameds = new Dictionary<string, Argument>();
                if (args.Count >= 3 && !args[1].WasQuoted)
                {
                    string a1 = args[1].ToString();
                    if (a1 == "=" || a1 == "+=" || a1 == "-=" || a1 == "*=" || a1 == "/=")
                    {
                        return new CommandEntry(command, 0, 0, system.DebugVarSetCommand, args, system.DebugVarSetCommand.Name, 0, script, line, tabs, system);
                    }
                    else if (a1 == "^=")
                    {
                        Argument varname = args[0];
                        args.RemoveRange(0, 2);
                        nameds["\0varname"] = varname;
                    }
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
                for (int i = 0; i < args.Count - 1; i++)
                {
                    if (!args[i].WasQuoted && args[i].ToString().StartsWith("--"))
                    {
                        nameds[args[i].ToString().Substring(2).ToLowerFast()] = args[i + 1];
                        args.RemoveRange(i, 2);
                        i -= 2;
                    }
                }
                AbstractCommand cmd;
                CommandEntry entry;
                if (system.RegisteredCommands.TryGetValue(BaseCommandLow, out cmd))
                {
                    entry = new CommandEntry(command, 0, 0, cmd, args, BaseCommand, marker, script, line, tabs, nameds, system) { WaitFor = waitfor };
                }
                else
                {
                    entry = CreateInvalidOutput(BaseCommand, args, system, command, marker, waitfor, script, line, tabs, nameds, system);
                }
                return entry;
            }
            catch (Exception ex)
            {
                if (ex is ErrorInducedException)
                {
                    throw new ErrorInducedException("Error on script line: " + line + "(" + command + "): " + ex.Message);
                }
                throw new ErrorInducedException("Internal exception occured on script line: " + line + "(" + command + ")", ex);
            }
        }

        /// <summary>
        /// Create an entry that represents an error message.
        /// </summary>
        public static CommandEntry CreateErrorOutput(string message, Commands system, string script, string tabs)
        {
            return new CommandEntry("error \"" + TagParser.Escape(message.Replace('\"', '\'')) + "\"", 0, 0, system.RegisteredCommands["error"],
                new List<Argument>() { new Argument() { Bits = new List<ArgumentBit>() { new TextArgumentBit(message, true) } } }, "error", 0, script, 0, tabs, system);

        }

        /// <summary>
        /// Create an entry that represents invalid output.
        /// </summary>
        public static CommandEntry CreateInvalidOutput(string name, List<Argument> _arguments,
            Commands system, string line, int marker, bool waitfor, string script, int linen, string tabs, Dictionary<string, Argument> nameds, Commands sys)
        {
            if (sys.Output.ShouldErrorOnInvalidCommand())
            {
                throw new ErrorInducedException("Unknown command '" + name + "'!");
            }
            _arguments.Insert(0, system.TagSystem.SplitToArgument(name, false));
            return new CommandEntry(line, 0, 0, system.DebugInvalidCommand, _arguments, name, marker, script, linen, tabs, nameds, sys) { WaitFor = waitfor };
                
        }

        /// <summary>
        /// The system controlling this CommandEntry.
        /// </summary>
        public Commands System;

        /// <summary>
        /// The index of this entry in its block.
        /// </summary>
        public int OwnIndex;

        /// <summary>
        /// The original command input.
        /// </summary>
        public string CommandLine;

        /// <summary>
        /// A list of all commands that were inside this command originally.
        /// </summary>
        public List<CommandEntry> InnerCommandBlock = null;

        /// <summary>
        /// All 'named' arguments on this command entry.
        /// </summary>
        public Dictionary<string, Argument> NamedArguments;

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
        /// The inner command block as its own script, generated by commands execute methods where needed.
        /// </summary>
        public CommandScript BlockScript = null;
        
        /// <summary>
        /// Full constructor, recommended.
        /// </summary>
        public CommandEntry(string _commandline, int bstart, int bend, AbstractCommand _command, List<Argument> _arguments,
            string _name, int _marker, string _script, int _line, string fairtabs, Commands sys)
            : this(_commandline, bstart, bend, _command, _arguments, _name, _marker, _script, _line, fairtabs, new Dictionary<string, Argument>(), sys)
        {
        }

        /// <summary>
        /// Full constructor, recommended.
        /// </summary>
        public CommandEntry(string _commandline, int bstart, int bend, AbstractCommand _command, List<Argument> _arguments,
            string _name, int _marker, string _script, int _line, string fairtabs, Dictionary<string, Argument> nameds, Commands sys)
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
            NamedArguments = nameds;
            System = sys;
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
        /// What marker was used. 0 = none, 1 = +, 2 = -, 3 = !
        /// </summary>
        public int Marker = 0;

        /// <summary>
        /// Gets the save name for this entry, without parsing it.
        /// </summary>
        /// <param name="defaultval">The default value.</param>
        /// <param name="id">The ID of the saver.</param>
        /// <returns>The save name.</returns>
        public string GetSaveNameNoParse(string defaultval, string id = "save")
        {
            Argument arg;
            if (NamedArguments.TryGetValue(id, out arg))
            {
                return arg.ToString().ToLowerFast();
            }
            return defaultval;
        }

        /// <summary>
        /// Gets a named argument with a specified name, handling any tags.
        /// </summary>
        /// <param name="queue">The command queue involved.</param>
        /// <param name="name">The argument name.</param>
        /// <returns>The parsed argument.</returns>
        public TemplateObject GetNamedArgumentObject(CommandQueue queue, string name)
        {
            Argument arg;
            if (NamedArguments.TryGetValue(name, out arg))
            {
                CommandStackEntry cse = queue.CurrentEntry;
                return arg.Parse(TextStyle.Color_Simple, cse.Debug, (o) => queue.HandleError(this, o), cse);
            }
            return null;
        }

        /// <summary>
        /// Gets an argument at a specified place, handling any tags.
        /// </summary>
        /// <param name="queue">The command queue involved.</param>
        /// <param name="place">The argument place number.</param>
        /// <returns>The parsed argument.</returns>
        public TemplateObject GetArgumentObject(CommandQueue queue, int place)
        {
            if (queue.ParseTags != TagParseMode.OFF) // TODO: Compile parse tags option
            {
                CommandStackEntry cse = queue.CurrentEntry;
                return Arguments[place].Parse(TextStyle.Color_Simple, cse.Debug, (o) => queue.HandleError(this, o), cse);
            }
            else
            {
                return new TextTag(Arguments[place].ToString());
            }
        }

        /// <summary>
        /// Gets an argument at a specified place, handling any tags - returning a string.
        /// </summary>
        /// <param name="queue">The command queue involved.</param>
        /// <param name="place">The argument place number.</param>
        /// <returns>The parsed argument as a string.</returns>
        public string GetArgument(CommandQueue queue, int place)
        {
            if (queue.ParseTags != TagParseMode.OFF)
            {
                CommandStackEntry cse = queue.CurrentEntry;
                return Arguments[place].Parse(TextStyle.Color_Simple, cse.Debug, (o) => queue.HandleError(this, o), cse).ToString();
            }
            else
            {
                return Arguments[place].ToString();
            }
        }

        /// <summary>
        /// Gets all arguments piled together into a string.
        /// </summary>
        /// <param name="queue">The command queue involved.</param>
        /// <param name="index">The index to start at.</param>
        /// <returns>The combined string.</returns>
        public string AllArguments(CommandQueue queue, int index = 0)
        {
            StringBuilder result = new StringBuilder(CommandLine.Length);
            for (int i = index; i < Arguments.Count; i++)
            {
                result.Append(GetArgument(queue, i));
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
        /// <param name="queue">The command queue involved.</param>
        /// <param name="text">The text to output.</param>
        public void InfoOutput(CommandQueue queue, string text)
        {
            queue.CommandSystem.Output.WriteLine(text);
            if (queue.Outputsystem != null)
            {
                queue.Outputsystem.Invoke(text, MessageType.INFO);
            }
        }

        /// <summary>
        /// Used to output a success message.
        /// </summary>
        /// <param name="queue">The command queue involved.</param>
        /// <param name="text">The text to output.</param>
        public void GoodOutput(CommandQueue queue, string text)
        {
            if (queue.CurrentEntry.Debug == DebugMode.FULL)
            {
                queue.CommandSystem.Output.GoodOutput(text);
                if (queue.Outputsystem != null)
                {
                    queue.Outputsystem.Invoke(text, MessageType.GOOD);
                }
            }
        }

        /// <summary>
        /// Gets the data associated with this entry in the queue.
        /// </summary>
        /// <param name="queue">The queue holding the data.</param>
        /// <param name="x">The data to set to.</param>
        /// <returns>The entry data.</returns>
        public void SetData(CommandQueue queue, AbstractCommandEntryData x)
        {
            queue.CurrentEntry.EntryData[OwnIndex] = x;
        }

        /// <summary>
        /// Gets the data associated with this entry in the queue.
        /// </summary>
        /// <param name="queue">The queue holding the data.</param>
        /// <returns>The entry data.</returns>
        public AbstractCommandEntryData GetData(CommandQueue queue)
        {
            return queue.CurrentEntry.EntryData[OwnIndex];
        }

        /// <summary>
        /// Returns whether commands should output 'good' results.
        /// </summary>
        /// <returns>Whether commands should output 'good' results.</returns>
        public bool ShouldShowGood(CommandQueue queue)
        {
            return queue.CurrentEntry.Debug == DebugMode.FULL;
        }

        /// <summary>
        /// Used to output a failure message. This is considered a 'warning' and will not induce an error.
        /// </summary>
        /// <param name="queue">The command queue involved.</param>
        /// <param name="text">The text to output.</param>
        public void BadOutput(CommandQueue queue, string text)
        {
            if (queue.CurrentEntry.Debug <= DebugMode.MINIMAL)
            {
                text = "WARNING in script '" + ScriptName + "' on line " + (ScriptLine + 1) + ": " + text;
                queue.CommandSystem.Output.BadOutput(text);
                if (queue.Outputsystem != null)
                {
                    queue.Outputsystem.Invoke(text, MessageType.BAD);
                }
            }
        }

#warning Remove these Bad/Good/Info old versions

        /// <summary>
        /// TODO: Remove this!
        /// </summary>
        /// <param name="queue">The command queue involved.</param>
        /// <param name="text">The text to output, with tags included.</param>
        public void Bad(CommandQueue queue, string text)
        {
            BadOutput(queue, text);
        }

        /// <summary>
        /// TODO: Remove this!
        /// </summary>
        /// <param name="queue">The command queue involved.</param>
        /// <param name="text">The text to output, with tags included.</param>
        public void Good(CommandQueue queue, string text)
        {
            GoodOutput(queue, text);
        }

        /// <summary>
        /// TODO: Remove this!
        /// </summary>
        /// <param name="queue">The command queue involved.</param>
        /// <param name="text">The text to output, with tags included.</param>
        public void Info(CommandQueue queue, string text)
        {
             InfoOutput(queue, text);
        }

        /// <summary>
        /// Perfectly duplicates the command entry.
        /// </summary>
        /// <returns>The duplicate entry.</returns>
        public CommandEntry Duplicate()
        {
            return (CommandEntry)MemberwiseClone();
        }

        /// <summary>
        /// A stack of all CIL Variables, for compilation reasons.
        /// </summary>
        public CILVariables[] CILVars;

        /// <summary>
        /// Gets the location of a variable from its name.
        /// </summary>
        /// <param name="name">The name of the variable.</param>
        /// <returns>The location of the variable.</returns>
        public int VarLoc(string name)
        {
            for (int i = 0; i < CILVars.Length; i++)
            {
                for (int x = 0; x < CILVars[i].LVariables.Count; x++)
                {
                    if (CILVars[i].LVariables[x].Item2 == name)
                    {
                        return CILVars[i].LVariables[x].Item1;
                    }
                }
            }
            return -1;
        }
    }
}
