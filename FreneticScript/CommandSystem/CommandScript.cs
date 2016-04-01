using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers;
using System.Reflection;
using System.Reflection.Emit;

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
        /// <param name="compile">Whether the script should be compiled.</param>
        /// <returns>A list of command strings.</returns>
        public static CommandScript SeparateCommands(string name, string commands, Commands system, bool compile)
        {
            List<string> CommandList = new List<string>();
            List<int> Lines = new List<int>();
            int start = 0;
            bool quoted = false;
            bool qtype = false;
            int line = 0;
            for (int i = 0; i < commands.Length; i++)
            {
                if (!quoted && commands[i] == '/' && i + 1 < commands.Length && commands[i + 1] == '/')
                {
                    int x = i;
                    while (x < commands.Length && commands[x] != '\n')
                    {
                        x++;
                    }
                    if (x < commands.Length)
                    {
                        commands = commands.Substring(0, i) + commands.Substring(x);
                    }
                    else
                    {
                        break;
                    }
                }
                else if (!quoted && commands[i] == '/' && i + 1 < commands.Length && commands[i + 1] == '*')
                {
                    int x;
                    for (x = i; x < commands.Length && !(commands[x] == '*' && x + 1 < commands.Length && commands[x + 1] == '/'); x++)
                    {
                        if (commands[x] == '\n')
                        {
                            line++;
                        }
                    }
                    if (x + 1 < commands.Length)
                    {
                        commands = commands.Substring(0, i) + commands.Substring(x + 1);
                    }
                    else
                    {
                        break;
                    }
                }
                else if (commands[i] == '"' && (!quoted || qtype))
                {
                    qtype = true;
                    quoted = !quoted;
                }
                else if (commands[i] == '\'' && (!quoted || !qtype))
                {
                    qtype = false;
                    quoted = !quoted;
                }
                else if (!quoted && commands[i] == ';')
                {
                    if (start < i)
                    {
                        Lines.Add(line);
                        CommandList.Add(commands.Substring(start, i - start).Trim());
                    }
                    start = i + 1;
                }
                else if (((commands[i] == '{' && (i == 0 || commands[i - 1] != '<')) || (commands[i] == '}' && (i + 1 >= commands.Length || commands[i + 1] != '>'))) && !quoted)
                {
                    if (start < i)
                    {
                        Lines.Add(line);
                        CommandList.Add(commands.Substring(start, i - start).Trim());
                    }
                    Lines.Add(line);
                    CommandList.Add(commands[i].ToString());
                    start = i + 1;
                    continue;
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
            return new CommandScript(name, CreateBlock(name, Lines, CommandList, null, system, "", 0, out herr), 0, compile);
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
                            int bc = block.Count;
                            if (cent.Command != null)
                            {
                                cent.Command.AdaptBlockFollowers(cent, toinj, block);
                            }
                            istart += (toinj.Count - bc);
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
                return SeparateCommands(filename, system.Output.ReadTextFile(fname), system, false); // TODO: Compile optional
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
        public CommandStackEntry Created;
        
        /// <summary>
        /// Constructs a new command script.
        /// </summary>
        /// <param name="_name">The name of the script.</param>
        /// <param name="_commands">All commands in the script.</param>
        /// <param name="adj">How far to negatively adjust the entries' block positions, if any.</param>
        /// <param name="compile">Whether the script should be compiled.</param>
        public CommandScript(string _name, List<CommandEntry> _commands, int adj = 0, bool compile = false)
        {
            Name = _name.ToLowerFast();
            List<CommandEntry> Commands = _commands;
            Commands = new List<CommandEntry>(_commands);
            for (int i = 0; i < Commands.Count; i++)
            {
                Commands[i] = _commands[i].Duplicate();
                Commands[i].BlockStart -= adj;
                Commands[i].BlockEnd -= adj;
            }
            if (compile)
            {
                Created = new CompiledCommandStackEntry();
            }
            else
            {
                Created = new CommandStackEntry();
            }
            Created.Debug = Debug;
            Created.Variables = new Dictionary<string, TemplateObject>();
            Created.Entries = Commands.ToArray();
            Created.EntryData = new AbstractCommandEntryData[Created.Entries.Length];
            Created.Determinations = new List<TemplateObject>();
            string tname = "__script__" + IDINCR++;
            AssemblyName asmname = new AssemblyName(tname);
            asmname.Name = tname;
            AssemblyBuilder asmbuild = AppDomain.CurrentDomain.DefineDynamicAssembly(asmname, AssemblyBuilderAccess.Run); // TODO: RunAndCollect in .NET 4
            ModuleBuilder modbuild = asmbuild.DefineDynamicModule(tname);
            if (compile)
            {
                CompiledCommandStackEntry ccse = (CompiledCommandStackEntry)Created;
                ccse.EntryCommands = new CompiledCommandRunnable[ccse.Entries.Length];
                for (int i = 0; i < ccse.Entries.Length; i++)
                {
                    TypeBuilder typebuild = modbuild.DefineType(tname + "__" + i, TypeAttributes.Class | TypeAttributes.Public, typeof(CompiledCommandRunnable));
                    MethodBuilder methodbuild = typebuild.DefineMethod("Run", MethodAttributes.Public | MethodAttributes.Virtual, typeof(void), new Type[] { typeof(CommandQueue) });
                    CILAdaptationValues values = new CILAdaptationValues();
                    values.Entry = ccse.Entries[i];
                    values.Script = this;
                    values.ILGen = methodbuild.GetILGenerator();
                    ccse.Entries[i].Command.AdaptToCIL(values);
                    typebuild.DefineMethodOverride(methodbuild, CompiledCommandRunnable.RunMethod);
                    Type t = typebuild.CreateType();
                    ccse.EntryCommands[i] = (CompiledCommandRunnable)Activator.CreateInstance(t);
                    ccse.EntryCommands[i].Entry = ccse.Entries[i];
                }
            }
        }

        static long IDINCR = 0;
        
        /// <summary>
        /// Creates a new queue for this script.
        /// </summary>
        /// <param name="system">The command system to make the queue in.</param>
        /// <returns>The created queue.</returns>
        public CommandQueue ToQueue(Commands system)
        {
            CommandQueue queue = new CommandQueue(this, system);
            if (Created == null)
            {
                throw new Exception("Invalid CREATED object in a CommandScript somehow?!");
            }
            queue.CommandStack.Push(Created.Duplicate());
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
            for (int i = 0; i < Created.Entries.Length; i++)
            {
                if (!Created.Entries[i].CommandLine.Contains('\0'))
                {
                    sb.Append(Created.Entries[i].FullString());
                    if (Created.Entries[i].InnerCommandBlock != null)
                    {
                        i = Created.Entries[i].BlockEnd;
                    }
                }
            }
            return sb.ToString();
        }
    }

    /// <summary>
    /// Abstract class for compiled runnables.
    /// </summary>
    public abstract class CompiledCommandRunnable
    {
        /// <summary>
        /// Reference to the original entry.
        /// </summary>
        public CommandEntry Entry;

        /// <summary>
        /// This class's "Run(queue)" method.
        /// </summary>
        public static MethodInfo RunMethod = typeof(CompiledCommandRunnable).GetMethod("Run", new Type[] { typeof(CommandQueue) });

        /// <summary>
        /// Runs the runnable.
        /// </summary>
        /// <param name="queue">The queue to run on.</param>
        public abstract void Run(CommandQueue queue);
    }
}
