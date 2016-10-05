using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.CommandSystem.Arguments;
using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Objects;
using System.Reflection;
using System.Reflection.Emit;
using FreneticScript.CommandSystem.QueueCmds;
using FreneticScript.TagHandlers.Common;

namespace FreneticScript.CommandSystem
{
    /// <summary>
    /// Represents a series of commands, not currently being processed.
    /// </summary>
    public class CommandScript
    {
        /// <summary>
        /// Separates a string list of command inputs (separated by newlines, semicolons, ...)
        /// and returns a command script object containing all the input commands.
        /// </summary>
        /// <param name="name">The name of the script.</param>
        /// <param name="commands">The command string to parse.</param>
        /// <param name="system">The command system to create the script within.</param>
        /// <param name="compile">Whether the script should be compiled.</param>
        /// <returns>A list of command strings.</returns>
        public static CommandScript SeparateCommands(string name, string commands, Commands system, bool compile)
        {
            try
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
            catch (Exception ex)
            {
                if (ex is ErrorInducedException)
                {
                    system.Output.BadOutput("Error parsing script: " + ex.Message);
                }
                else
                {
                    system.Output.BadOutput("Exception parsing script: " + ex.ToString());
                }
                return null;
            }
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
                        string fullmsg = "FAILED TO COMPILE SCRIPT '" + name + "': (line " + toret[i].ScriptLine + "): " + msg;
                        system.Output.BadOutput(fullmsg);
                        had_error = true;
                        toret.Clear();
                        // TODO: Maybe throw an exception?
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
                return SeparateCommands(filename, system.Output.ReadTextFile(fname), system, true); // TODO: Compile optional
            }
            catch (System.IO.FileNotFoundException)
            {
                return null;
            }
            catch (Exception ex)
            {
                if (ex is System.Threading.ThreadAbortException)
                {
                    throw ex;
                }
                system.Output.BadOutput("Generating script for file '" + filename + "': " + ex.ToString());
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
                Commands[i].OwnIndex = i;
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
            Created.Variables = new Dictionary<string, ObjectHolder>();
            Created.Entries = Commands.ToArray();
            Created.EntryData = new AbstractCommandEntryData[Created.Entries.Length];
            if (compile)
            {
                string tname = "__script__" + IDINCR++;
                AssemblyName asmname = new AssemblyName(tname);
                asmname.Name = tname;
                AssemblyBuilder asmbuild = AppDomain.CurrentDomain.DefineDynamicAssembly(asmname,
#if NET_4_5
                    AssemblyBuilderAccess.RunAndCollect
#else
                    AssemblyBuilderAccess.Run/*AndSave*/
#endif
                    );
                ModuleBuilder modbuild = asmbuild.DefineDynamicModule(tname/*, "testmod.dll", true*/);
                CompiledCommandStackEntry ccse = (CompiledCommandStackEntry)Created;
                ccse.AdaptedILPoints = new Label[ccse.Entries.Length + 1];
                TypeBuilder typebuild_c = modbuild.DefineType(tname + "__CENTRAL", TypeAttributes.Class | TypeAttributes.Public, typeof(CompiledCommandRunnable));
                MethodBuilder methodbuild_c = typebuild_c.DefineMethod("Run", MethodAttributes.Public | MethodAttributes.Virtual, typeof(void), new Type[] { typeof(CommandQueue), typeof(IntHolder), typeof(int) });
                ILGenerator ilgen = methodbuild_c.GetILGenerator();
                CILAdaptationValues values = new CILAdaptationValues();
                values.Entry = ccse;
                values.Script = this;
                values.ILGen = ilgen;
                values.Method = methodbuild_c;
                for (int i = 0; i < ccse.AdaptedILPoints.Length; i++)
                {
                    ccse.AdaptedILPoints[i] = ilgen.DefineLabel();
                }
                for (int i = 0; i < ccse.Entries.Length; i++)
                {
                    ccse.Entries[i].Command.PreAdaptToCIL(values, i);
                }
                ccse.LocalVariables = new ObjectHolder[values.LVariables.Count];
                ccse.LocalVarNames = new string[values.LVariables.Count];
                Dictionary<string, TagType> types = new Dictionary<string, TagType>();
                for (int i = 0; i < ccse.LocalVariables.Length; i++)
                {
                    ccse.LocalVarNames[i] = values.LVariables[i].Key;
                    ccse.LocalVariables[i] = new ObjectHolder();
                    ccse.Variables[ccse.LocalVarNames[i]] = ccse.LocalVariables[i];
                    types[values.LVariables[i].Key] = values.LVariables[i].Value;
                }
                ilgen.Emit(OpCodes.Ldarg_3);
                ilgen.Emit(OpCodes.Switch, ccse.AdaptedILPoints);
                for (int i = 0; i < ccse.Entries.Length; i++)
                {
                    for (int a = 0; a < ccse.Entries[i].Arguments.Count; a++)
                    {
                        Argument arg = ccse.Entries[i].Arguments[a];
                        if (arg.Bits.Count > 0 && arg.Bits[0] is TagArgumentBit)
                        {
                            TagArgumentBit tab = ((TagArgumentBit)arg.Bits[0]);
                            TagType returnable = tab.Start.ResultType;
                            if (tab.Start is VarTagBase)
                            {
                                string vn = tab.Bits[0].Variable.ToString().ToLowerFast();
                                for (int x = 0; x < ccse.LocalVarNames.Length; x++)
                                {
                                    if (ccse.LocalVarNames[x] == vn)
                                    {
                                        tab.Start = ccse.Entries[i].Command.CommandSystem.TagSystem.LVar;
                                        tab.Bits[0].Key = "\0lvar";
                                        tab.Bits[0].Handler = null;
                                        tab.Bits[0].OVar = tab.Bits[0].Variable;
                                        tab.Bits[0].Variable = new Argument() { WasQuoted = false, Bits = new List<ArgumentBit>() { new TextArgumentBit( x.ToString(), false) } };
                                        types.TryGetValue(vn, out returnable);
                                        break;
                                    }
                                }
                            }
                            if (returnable != null)
                            {
                                for (int x = 1; x < tab.Bits.Length; x++)
                                {
                                    string key = tab.Bits[x].Key;
                                    if (returnable.TagHelpers.ContainsKey(key))
                                    {
                                        TagType temptype = returnable;
                                        TagHelpInfo tsh = null;
                                        while (tsh == null && temptype != null)
                                        {
                                            tsh = temptype.TagHelpers[key];
                                            temptype = temptype.SubType;
                                        }
                                        tab.Bits[x].TagHandler = tsh;
                                        if (tsh == null || tsh.Meta.ReturnTypeResult == null)
                                        {
                                            break;
                                        }
                                        returnable = tsh.Meta.ReturnTypeResult;
                                    }
                                    else
                                    {
                                        if (!returnable.SubHandlers.ContainsKey(key))
                                        {
                                            throw new ErrorInducedException("Error in command line " + ccse.Entries[i].ScriptLine + ": (" + ccse.Entries[i].CommandLine
                                                + "): Invalid tag sub-handler " + key + " for tag " + tab.ToString());
                                        }
                                        TagType temptype = returnable;
                                        TagSubHandler tsh = null;
                                        while (tsh == null && temptype != null)
                                        {
                                            tsh = temptype.SubHandlers[key];
                                            temptype = temptype.SubType;
                                        }
                                        tab.Bits[x].Handler = tsh;
                                        if (tsh == null || tsh.ReturnType == null)
                                        {
                                            break;
                                        }
                                        returnable = tsh.ReturnType;
                                    }
                                }
                            }
                        }
                    }
                    ilgen.MarkLabel(ccse.AdaptedILPoints[i]);
                    ccse.Entries[i].Command.AdaptToCIL(values, i);
                }
                ilgen.MarkLabel(ccse.AdaptedILPoints[ccse.AdaptedILPoints.Length - 1]);
                ilgen.Emit(OpCodes.Ret);
                typebuild_c.DefineMethodOverride(methodbuild_c, CompiledCommandRunnable.RunMethod);
                Type t_c = typebuild_c.CreateType();
                ccse.MainCompiledRunnable = (CompiledCommandRunnable)Activator.CreateInstance(t_c);
                ccse.MainCompiledRunnable.CSEntry = ccse;
                //asmbuild.Save("test.dll");
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
        /// The command stack entry that forms this runnable.
        /// </summary>
        public CommandStackEntry CSEntry;
        
        /// <summary>
        /// This class's "Run(queue)" method.
        /// </summary>
        public static MethodInfo RunMethod = typeof(CompiledCommandRunnable).GetMethod("Run", new Type[] { typeof(CommandQueue), typeof(IntHolder), typeof(int) });

        /// <summary>
        /// Runs the runnable.
        /// </summary>
        /// <param name="queue">The queue to run on.</param>
        /// <param name="counter">The current command index.</param>
        /// <param name="fent">The first entry (the entry to start calculating at).</param>
        public abstract void Run(CommandQueue queue, IntHolder counter, int fent);
    }

    /// <summary>
    /// Holds a 32-bit integer.
    /// </summary>
    public class IntHolder
    {
        /// <summary>
        /// The actual integer.
        /// </summary>
        public int Internal;
    }
}
