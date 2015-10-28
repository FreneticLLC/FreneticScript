using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Frenetic.TagHandlers;

namespace Frenetic.CommandSystem
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
            int start = 0;
            bool quoted = false;
            for (int i = 0; i < commands.Length; i++)
            {
                if (commands[i] == '"')
                {
                    quoted = !quoted;
                }
                else if ((commands[i] == '\n') || (!quoted && commands[i] == ';'))
                {
                    if (start < i)
                    {
                        CommandList.Add(commands.Substring(start, i - start).Trim());
                    }
                    start = i + 1;
                    quoted = false;
                }
            }
            if (start < commands.Length)
            {
                CommandList.Add(commands.Substring(start).Trim());
            }
            return new CommandScript(name, CreateBlock(CommandList, null, system));
        }

        /// <summary>
        /// Converts a list of command strings to a CommandEntry list, handling any { braced } blocks inside.
        /// </summary>
        /// <param name="from">The command strings.</param>
        /// <param name="entry">The entry that owns this block.</param>
        /// <param name="system">The command system to create this block inside.</param>
        /// <returns>A list of entries with blocks separated.</returns>
        public static List<CommandEntry> CreateBlock(List<string> from, CommandEntry entry, Commands system)
        {
            List<CommandEntry> toret = new List<CommandEntry>();
            List<string> Temp = null;
            int blocks = 0;
            for (int i = 0; i < from.Count; i++)
            {
                if (from[i] == "{")
                {
                    blocks++;
                    if (blocks == 1)
                    {
                        Temp = new List<string>();
                    }
                    else
                    {
                        Temp.Add("{");
                    }
                }
                else if (from[i] == "}")
                {
                    blocks--;
                    if (blocks == 0)
                    {
                        if (toret.Count == 0)
                        {
                            List<CommandEntry> block = CreateBlock(Temp, entry, system);
                            toret.AddRange(block);
                        }
                        else
                        {
                            List<CommandEntry> block = CreateBlock(Temp, toret[toret.Count - 1], system);
                            toret[toret.Count - 1].Block = block;
                        }
                    }
                    else if (blocks < 0)
                    {
                        blocks = 0;
                    }
                    else
                    {
                        Temp.Add("}");
                    }
                }
                else if (blocks > 0)
                {
                    Temp.Add(from[i]);
                }
                else
                {
                    CommandEntry centry = CommandEntry.FromInput(from[i], null, entry, system);
                    if (centry != null)
                    {
                        toret.Add(centry);
                    }
                }
            }
            if (toret.Count == 0 && entry != null)
            {
                return null;
            }
            return toret;
        }

        /// <summary>
        /// Creates a script by file name.
        /// File is /scripts/filename.cfg
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
        /// Removes an entry's ownership over the list of entries, and returns them in a new list of duplicates.
        /// </summary>
        /// <param name="entries">The list of entries.</param>
        /// <param name="baseentry">The entry that is no longer an owner.</param>
        /// <returns>The new entry list.</returns>
        public static List<CommandEntry> DisOwn(List<CommandEntry> entries, CommandEntry baseentry)
        {
            List<CommandEntry> newentries = new List<CommandEntry>();
            for (int i = 0; i < entries.Count; i++)
            {
                newentries.Add(entries[i].Duplicate());
                if (newentries[i].BlockOwner == baseentry)
                {
                    newentries[i].BlockOwner = null;
                }
            }
            return newentries;
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
        public CommandScript(string _name, List<CommandEntry> _commands)
        {
            Name = _name.ToLower();
            Commands = _commands;
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
        /// <returns>The full command string.</returns>
        public string FullString()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < Commands.Count; i++)
            {
                sb.Append(Commands[i].CommandLine);
                if (i + 1 < Commands.Count)
                {
                    sb.Append("; ");
                }
                else
                {
                    sb.Append(";");
                }
            }
            return sb.ToString();
        }
    }
}
