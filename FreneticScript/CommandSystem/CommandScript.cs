using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers;

namespace FreneticScript.CommandSystem
{
    /// <summary>
    /// Represents a series of commands, not currently being processed.
    /// </summary>
    public class CommandScript
    {
        /// <summary>
        /// Separates a string list of command inputs (separated by newlines, semicolons, ...)
        /// and returns a queue object containing all the input commands
        /// </summary>
        /// <param name="name">The name of the script.</param>
        /// <param name="commands">The command string to parse.</param>
        /// <param name="system">The command system to create the script within.</param>
        /// <returns>A list of command strings.</returns>
        public static CommandScript SeparateCommands(string name, string commands, Commands system)
        {
            List<string> CommandList = new List<string>();
            List<int> Lines = new List<int>();
            int start = 0;
            bool quoted = false;
            bool qtype = false;
            int line = 0;
            for (int i = 0; i < commands.Length; i++)
            {
                if (commands[i] == '"' && (!quoted || qtype))
                {
                    qtype = true;
                    quoted = !quoted;
                }
                else if (commands[i] == '\'' && (!quoted || !qtype))
                {
                    qtype = false;
                    quoted = !quoted;
                }
                else if ((commands[i] == '\n') || (!quoted && commands[i] == ';'))
                {
                    if (start < i)
                    {
                        Lines.Add(line);
                        CommandList.Add(commands.Substring(start, i - start).Trim());
                    }
                    start = i + 1;
                    quoted = false;
                }
                if (commands[i] == '\n')
                {
                    line++;
                }
            }
            if (start < commands.Length)
            {
                Lines.Add(line);
                CommandList.Add(commands.Substring(start).Trim());
            }
            bool herr;
            return new CommandScript(name, CreateBlock(name, Lines, CommandList, null, system, "", 0, out herr));
        }

        /// <summary>
        /// Converts a list of command strings to a CommandEntry list, handling any { braced } blocks inside.
        /// </summary>
        /// <param name="name">The name of the script.</param>
        /// <param name="lines">The file line numbers for the corresponding command strings.</param>
        /// <param name="from">The command strings.</param>
        /// <param name="entry">The entry that owns this block.</param>
        /// <param name="system">The command system to create this block inside.</param>
        /// <param name="tabs">How far out tabulation should go.</param>
        /// <param name="had_error">Whether there was a compile error.</param>
        /// <param name="istart">The starting index.</param>
        /// <returns>A list of entries with blocks separated.</returns>
        public static List<CommandEntry> CreateBlock(string name, List<int> lines, List<string> from, CommandEntry entry, Commands system, string tabs, int istart, out bool had_error)
        {
            List<CommandEntry> toret = new List<CommandEntry>();
            List<string> Temp = null;
            List<int> Temp2 = null;
            int blocks = 0;
            for (int i = 0; i < from.Count; i++)
            {
                if (from[i] == "{")
                {
                    blocks++;
                    if (blocks == 1)
                    {
                        Temp = new List<string>();
                        Temp2 = new List<int>();
                    }
                    else
                    {
                        Temp.Add("{");
                        Temp2.Add(lines[i]);
                    }
                }
                else if (from[i] == "}")
                {
                    blocks--;
                    if (blocks == 0)
                    {
                        if (toret.Count == 0)
                        {
                            bool err;
                            List<CommandEntry> block = CreateBlock(name, Temp2, Temp, entry, system, tabs + "    ", istart, out err);
                            if (err)
                            {
                                had_error = true;
                                return block;
                            }
                            toret.AddRange(block);
                            istart += block.Count;
                        }
                        else
                        {
                            bool err;
                            CommandEntry cent = toret[toret.Count - 1];
                            List<CommandEntry> block = CreateBlock(name, Temp2, Temp, cent, system, tabs + "    ", istart, out err);
                            if (err)
                            {
                                had_error = true;
                                return block;
                            }
                            cent.BlockStart = istart;
                            istart += block.Count;
                            cent.BlockEnd = istart - 1;
                            List<CommandEntry> toinj = new List<CommandEntry>(block);
                            if (cent.Command != null)
                            {
                                cent.Command.AdaptBlockFollowers(cent, toinj, block);
                            }
                            istart += (toinj.Count - block.Count);
                            cent.InnerCommandBlock = block;
                            toret.AddRange(toinj);
                        }
                    }
                    else if (blocks < 0)
                    {
                        blocks = 0;
                    }
                    else
                    {
                        Temp.Add("}");
                        Temp2.Add(lines[i]);
                    }
                }
                else if (blocks > 0)
                {
                    Temp.Add(from[i]);
                    Temp2.Add(lines[i]);
                }
                else
                {
                    CommandEntry centry = CommandEntry.FromInput(from[i], system, name, lines[i], tabs);
                    if (centry != null)
                    {
                        istart++;
                        toret.Add(centry);
                    }

                }
            }
            for (int i = 0; i < toret.Count; i++)
            {
                if (toret[i].Command != null)
                {
                    string msg = toret[i].Command.TestForValidity(toret[i]);
                    if (msg != null)
                    {
                        string fullmsg = "FAILED TO COMPILE SCRIPT '" + TagParser.Escape(name) + "': (line " + toret[i].ScriptLine + "): " + msg;
                        system.Output.Bad(fullmsg, DebugMode.FULL);
                        had_error = true;
                        toret.Clear();
                        toret.Add(CommandEntry.CreateErrorOutput(fullmsg, system, name, tabs));
                        return toret;
                    }
                }
            }
            had_error = false;
            return toret;
        }

        /// <summary>
        /// Creates a script by file name.
        /// </summary>
        /// <param name="filename">The name of the file to execute.</param>
        /// <param name="system">The command system to get the script for.</param>
        /// <returns>A command script, or null of the file does not exist.</returns>
        public static CommandScript GetByFileName(string filename, Commands system)
        {
            try
            {
                string fname = filename + ".cfg";
                return SeparateCommands(filename, system.Output.ReadTextFile(fname), system);
            }
            catch (System.IO.FileNotFoundException)
            {
                return null;
            }
            catch (Exception ex)
            {
                system.Output.Bad("Generating script for file '" + TagParser.Escape(filename)
                    + "': " + TagParser.Escape(ex.ToString()), DebugMode.NONE);
                return null;
            }
        }
        
        /// <summary>
        /// The name of the script.
        /// </summary>
        public string Name;

        /// <summary>
        /// The default debugmode for queues running this script.
        /// </summary>
        public DebugMode Debug = DebugMode.FULL;

        /// <summary>
        /// All commands in the script.
        /// </summary>
        public List<CommandEntry> Commands;

        /// <summary>
        /// Constructs a new command script.
        /// </summary>
        /// <param name="_name">The name of the script.</param>
        /// <param name="_commands">All commands in the script.</param>
        /// <param name="adj">How far to negatively adjust the entries' block positions, if any.</param>
        public CommandScript(string _name, List<CommandEntry> _commands, int adj = 0)
        {
            Name = _name.ToLowerInvariant();
            Commands = _commands;
            if (adj != 0)
            {
                Commands = new List<CommandEntry>(_commands);
                for (int i = 0; i < Commands.Count; i++)
                {
                    Commands[i] = Commands[i].Duplicate();
                    Commands[i].BlockStart -= adj;
                    Commands[i].BlockEnd -= adj;
                }
            }
        }

        /// <summary>
        /// Returns a duplicate of the script's entry list.
        /// </summary>
        /// <returns>The entry list.</returns>
        public List<CommandEntry> GetEntries()
        {
            List<CommandEntry> entries = new List<CommandEntry>(Commands.Count);
            for (int i = 0; i < Commands.Count; i++)
            {
                entries.Add(Commands[i].Duplicate());
            }
            return entries;
        }

        /// <summary>
        /// Creates a new queue for this script.
        /// </summary>
        /// <param name="system">The command system to make the queue in.</param>
        /// <returns>The created queue.</returns>
        public CommandQueue ToQueue(Commands system)
        {
            CommandQueue queue = new CommandQueue(this, GetEntries(), system);
            queue.Debug = Debug;
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
        /// <param name="tabulation">How much space to include in front of the commands.</param>
        /// <returns>The full command string.</returns>
        public string FullString(string tabulation = "")
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < Commands.Count; i++)
            {
                if (!Commands[i].CommandLine.Contains('\0'))
                {
                    sb.Append(Commands[i].FullString());
                    sb.Append("\n");
                }
            }
            return sb.ToString();
        }
    }
}
