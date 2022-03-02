//
// This file is part of FreneticScript, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of the MIT license.
// See README.md or LICENSE.txt in the FreneticScript source root for the contents of the license.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreneticUtilities.FreneticExtensions;
using FreneticScript.CommandSystem;
using FreneticScript.CommandSystem.Arguments;
using FreneticScript.TagHandlers.CommonBases;
using FreneticScript.TagHandlers.HelperBases;

namespace FreneticScript.ScriptSystems
{
    /// <summary>Helper class to parse scripts from text.</summary>
    public static class ScriptParser
    {
        /// <summary>
        /// Separates a string list of command inputs (separated by newlines, semicolons, ...)
        /// and returns a command script object containing all the input commands.
        /// </summary>
        /// <param name="name">The name of the script.</param>
        /// <param name="commands">The command string to parse.</param>
        /// <param name="system">The command system to create the script within.</param>
        /// <param name="currentLine">The current line, if reparsing middle-of-a-file script text.</param>
        /// <param name="mode">The default debug mode, if any.</param>
        /// <returns>A list of command strings.</returns>
        public static CommandScript SeparateCommands(string name, string commands, ScriptEngine system, int currentLine = 1, DebugMode mode = DebugMode.FULL)
        {
            try
            {
                commands = commands.Replace("\r\n", "\n").Replace('\r', '\n');
                List<string> CommandList = new();
                List<int> Lines = new();
                bool quoted = false;
                bool qtype = false;
                int line = currentLine;
                int anons = 0;
                StringBuilder commandConstruct = new(256);
                for (int i = 0; i < commands.Length; i++)
                {
                    // One-Line Comments
                    if (!quoted && commands[i] == '/' &&  commands.IndexEquals(i + 1, '/'))
                    {
                        int x = i;
                        while (x < commands.Length && commands[x] != '\n')
                        {
                            x++;
                        }
                        if (x < commands.Length)
                        {
                            commands = commands[..i] + commands[x..];
                            i--;
                            continue;
                        }
                        else
                        {
                            break;
                        }
                    }
                    // Multi-Line Comments
                    else if (!quoted && commands[i] == '/' && commands.IndexEquals(i + 1, '*'))
                    {
                        int x;
                        for (x = i; x < commands.Length && !(commands[x] == '*' && commands.IndexEquals(x + 1, '/')); x++)
                        {
                            if (commands[x] == '\n')
                            {
                                line++;
                            }
                        }
                        if (x + 1 < commands.Length)
                        {
                            commands = commands[..i] + commands[(x + 1)..];
                        }
                        else
                        {
                            break;
                        }
                    }
                    // Double Quotes
                    else if (commands[i] == '"' && (!quoted || qtype))
                    {
                        qtype = true;
                        quoted = !quoted;
                    }
                    // Single Quotes
                    else if (commands[i] == '\'' && (!quoted || !qtype))
                    {
                        qtype = false;
                        quoted = !quoted;
                    }
                    // Semicolons to end commands
                    else if (!quoted && commands[i] == ';')
                    {
                        if (commandConstruct.Length != 0)
                        {
                            Lines.Add(line);
                            CommandList.Add(commandConstruct.ToString());
                            commandConstruct.Clear();
                        }
                        continue;
                    }
                    // Anonymous functions
                    else if (commands[i] == '<' && commands.IndexEquals(i + 1, '{'))
                    {
                        int funcStart = i + 2;
                        int subFuncs = 0;
                        int funcLine = line;
                        for (i = funcStart; i < commands.Length; i++)
                        {
                            if (commands[i] == '<' && commands.IndexEquals(i + 1, '{'))
                            {
                                subFuncs++;
                            }
                            else if (commands[i] == '>' && commands[i - 1] == '}')
                            {
                                if (subFuncs == 0)
                                {
                                    break;
                                }
                                subFuncs--;
                            }
                            else if (commands[i] == '\n')
                            {
                                line++;
                            }
                        }
                        string funcText = commands.Substring(funcStart, i - funcStart - 1);
                        anons++;
                        commandConstruct.Append("<function[anon|");
                        commandConstruct.Append(EscapeTagBase.Escape(name + "#anon:" + anons)).Append('|');
                        commandConstruct.Append(funcLine).Append('|');
                        commandConstruct.Append(EscapeTagBase.Escape(funcText));
                        commandConstruct.Append("]>");
                        continue;
                    }
                    // Braces open/close
                    else if (((commands[i] == '{' && !commands.IndexEquals(i - 1, '<')) || (commands[i] == '}' && !commands.IndexEquals(i + 1, '>'))) && !quoted)
                    {
                        if (commandConstruct.Length != 0)
                        {
                            Lines.Add(line);
                            CommandList.Add(commandConstruct.ToString());
                            commandConstruct.Clear();
                        }
                        // Add the brace symbol as a command
                        Lines.Add(line);
                        CommandList.Add(commands[i].ToString());
                        continue;
                    }
                    // Newlines
                    else if (commands[i] == '\n')
                    {
                        bool isContinued = false;
                        for (i += 1; i < commands.Length; i++)
                        {
                            if (commands[i] == '|')
                            {
                                isContinued = true;
                                break;
                            }
                            if (commands[i] != ' ' && commands[i] != '\t' && commands[i] != '\n')
                            {
                                i -= 1;
                                break;
                            }
                            if (commands[i] == '\n')
                            {
                                line++;
                            }
                        }
                        if (!isContinued)
                        {
                            if (commandConstruct.Length != 0)
                            {
                                Lines.Add(line);
                                CommandList.Add(commandConstruct.ToString());
                                commandConstruct.Clear();
                            }
                        }
                        line++;
                        continue;
                    }
                    commandConstruct.Append(commands[i]);
                }
                if (commandConstruct.Length != 0)
                {
                    Lines.Add(line);
                    CommandList.Add(commandConstruct.ToString().Trim());
                }
                return new CommandScript(name, CommandScript.TYPE_NAME_FILE, CreateBlock(name, Lines, CommandList, null, system, "", 0, out bool herr).ToArray(), system, 0, mode);
            }
            catch (Exception ex)
            {
                if (ex is ErrorInducedException)
                {
                    system.Context.BadOutput("Error parsing script '" + TextStyle.Separate + name + TextStyle.Base + "': " + ex.Message);
                }
                else
                {
                    system.Context.BadOutput("Exception parsing script '" + name + "': " + ex.ToString());
                }
                return null;
            }
        }

        /// <summary>Converts a list of command strings to a CommandEntry list, handling any { braced } blocks inside.</summary>
        /// <param name="name">The name of the script.</param>
        /// <param name="lines">The file line numbers for the corresponding command strings.</param>
        /// <param name="from">The command strings.</param>
        /// <param name="entry">The entry that owns this block.</param>
        /// <param name="system">The command system to create this block inside.</param>
        /// <param name="tabs">How far out tabulation should go.</param>
        /// <param name="had_error">Whether there was a compile error.</param>
        /// <param name="indexStart">The starting index.</param>
        /// <returns>A list of entries with blocks separated.</returns>
        public static List<CommandEntry> CreateBlock(string name, List<int> lines, List<string> from, CommandEntry entry, ScriptEngine system, string tabs, int indexStart, out bool had_error)
        {
            List<CommandEntry> outEntryList = new();
            List<string> fromArgsHelper = null;
            List<int> lineArgsHelper = null;
            int blocks = 0;
            for (int i = 0; i < from.Count; i++)
            {
                if (from[i] == "{")
                {
                    blocks++;
                    if (blocks == 1)
                    {
                        fromArgsHelper = new List<string>();
                        lineArgsHelper = new List<int>();
                    }
                    else
                    {
                        fromArgsHelper.Add("{");
                        lineArgsHelper.Add(lines[i]);
                    }
                }
                else if (from[i] == "}")
                {
                    blocks--;
                    if (blocks == 0)
                    {
                        if (outEntryList.Count == 0)
                        {
                            List<CommandEntry> block = CreateBlock(name, lineArgsHelper, fromArgsHelper, entry, system, tabs + "    ", indexStart, out bool err);
                            if (err)
                            {
                                had_error = true;
                                return block;
                            }
                            outEntryList.AddRange(block);
                            indexStart += block.Count;
                        }
                        else
                        {
                            CommandEntry cent = outEntryList[^1];
                            List<CommandEntry> block = CreateBlock(name, lineArgsHelper, fromArgsHelper, cent, system, tabs + "    ", indexStart, out bool err);
                            if (err)
                            {
                                had_error = true;
                                return block;
                            }
                            cent.BlockStart = indexStart;
                            indexStart += block.Count;
                            cent.BlockEnd = indexStart - 1;
                            List<CommandEntry> blockToInject = new(block);
                            int bc = block.Count;
                            if (cent.Command != null)
                            {
                                cent.Command.AdaptBlockFollowers(cent, blockToInject, block);
                            }
                            indexStart += (blockToInject.Count - bc);
                            cent.InnerCommandBlock = block.ToArray();
                            outEntryList.AddRange(blockToInject);
                        }
                    }
                    else if (blocks < 0)
                    {
                        blocks = 0;
                    }
                    else
                    {
                        fromArgsHelper.Add("}");
                        lineArgsHelper.Add(lines[i]);
                    }
                }
                else if (blocks > 0)
                {
                    fromArgsHelper.Add(from[i]);
                    lineArgsHelper.Add(lines[i]);
                }
                else
                {
                    CommandEntry centry = EntryFromInput(from[i], system, name, lines[i], tabs);
                    if (centry != null)
                    {
                        indexStart++;
                        outEntryList.Add(centry);
                    }
                }
            }
            for (int i = 0; i < outEntryList.Count; i++)
            {
                if (outEntryList[i].Command != null)
                {
                    string msg = outEntryList[i].Command.TestForValidity(outEntryList[i]);
                    if (msg != null)
                    {
                        string fullmsg = $"FAILED TO COMPILE SCRIPT '{name}': (line {outEntryList[i].ScriptLine}): {msg}";
                        system.Context.BadOutput(fullmsg);
                        had_error = true;
                        outEntryList.Clear();
                        // TODO: Maybe throw an exception?
                        outEntryList.Add(CreateErrorOutputEntry(fullmsg, system, name, tabs));
                        return outEntryList;
                    }
                }
            }
            had_error = false;
            return outEntryList;
        }



        /// <summary>
        /// Creates a CommandEntry from the given input and queue information.
        /// May return null.
        /// </summary>
        /// <param name="command">The command line text itself.</param>
        /// <param name="system">The command system to work from.</param>
        /// <param name="script">The name of the creating script.</param>
        /// <param name="line">The line in the creating script.</param>
        /// <param name="tabs">How much tabulation tihs command had.</param>
        /// <returns>The command system.</returns>
        public static CommandEntry EntryFromInput(string command, ScriptEngine system, string script, int line, string tabs)
        {
            try
            {
                if (command.Trim().Length == 0)
                {
                    return null;
                }
                if (command.StartsWith("/"))
                {
                    command = command[1..];
                }
                command = command.Replace('\0', ' ');
                List<Argument> args = new();
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
                        command = (i + 1 < command.Length) ? command[..i] + command[(i + 1)..] : command[..i];
                    }
                    else if (!quoted && command[i] == ' ')
                    {
                        if (i - start > 0)
                        {
                            string arg = command[start..i].Trim().Replace('\'', '"').Replace("\"", "");
                            args.Add(ArgumentParser.SplitToArgument(system, arg, thisArgQuoted));
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
                    string arg = command[start..].Trim().Replace('\'', '"').Replace("\"", "");
                    args.Add(ArgumentParser.SplitToArgument(system, arg, thisArgQuoted));
                }
                if (args.Count == 0)
                {
                    return null;
                }
                Dictionary<string, Argument> nameds = new();
                if (args.Count >= 3 && !args[1].WasQuoted)
                {
                    string a1 = args[1].ToString();
                    if (a1 == "=" || a1 == "+=" || a1 == "-=" || a1 == "*=" || a1 == "/=")
                    {
                        return new CommandEntry(command, 0, 0, system.DebugVarSetCommand, args.ToArray(), system.DebugVarSetCommand.Name, CommandPrefix.NONE, script, line, tabs, system);
                    }
                    else if (a1 == "^=")
                    {
                        Argument varname = args[0];
                        args.RemoveRange(0, 2);
                        nameds[CommandEntry.SAVE_NAME_ARG_ID] = new Argument() { Bits = new ArgumentBit[] { new TextArgumentBit("\0" + varname.ToString().ToLowerFast(), true, true, system) } };
                    }
                    else if (args.Count >= 4 && args[0].ToString() == "var" && args[2].ToString() == "^=")
                    {
                        Argument varname = args[1];
                        args.RemoveRange(0, 3);
                        nameds[CommandEntry.SAVE_NAME_ARG_ID] = new Argument() { Bits = new ArgumentBit[] { new TextArgumentBit(varname.ToString().ToLowerFast(), true, true, system) } };
                    }
                }
                string BaseCommand = args[0].ToString();
                CommandPrefix prefix = CommandPrefix.NONE;
                if (BaseCommand.Length > 1)
                {
                    prefix = CommandPrefixHelpers.ForCharacter(BaseCommand[0]);
                    if (prefix != CommandPrefix.NONE)
                    {
                        BaseCommand = BaseCommand[1..];
                    }
                }
                string BaseCommandLow = BaseCommand.ToLowerFast();
                args.RemoveAt(0);
                for (int i = 0; i < args.Count - 1; i++)
                {
                    if (!args[i].WasQuoted && args[i].ToString().StartsWith("--"))
                    {
                        nameds[args[i].ToString()[2..].ToLowerFast()] = args[i + 1];
                        args.RemoveRange(i, 2);
                        i -= 2;
                    }
                }
                CommandEntry entry;
                if (system.RegisteredCommands.TryGetValue(BaseCommandLow, out AbstractCommand foundCommandObject))
                {
                    if (prefix == CommandPrefix.WAIT && !foundCommandObject.Waitable)
                    {
                        throw new ErrorInducedException("Cannot wait ('&') on command '" + foundCommandObject.Name + "'.");
                    }
                    entry = new CommandEntry(command, 0, 0, foundCommandObject, args.ToArray(), BaseCommand, prefix, script, line, tabs, nameds, system);
                }
                else
                {
                    entry = CreateInvalidOutputEntry(BaseCommand, args, system, command, prefix, script, line, tabs, nameds, system);
                }
                return entry;
            }
            catch (Exception ex)
            {
                if (ex is ErrorInducedException)
                {
                    throw new ErrorInducedException("Error on script line: " + TextStyle.Separate + line + TextStyle.Base + " (" + TextStyle.Separate + command + TextStyle.Base + "): " + ex.Message);
                }
                throw new ErrorInducedException("Internal exception occured on script line: " + line + " (" + command + "): " + ex.ToString());
            }
        }

        /// <summary>Create an entry that represents an error message.</summary>
        public static CommandEntry CreateErrorOutputEntry(string message, ScriptEngine system, string script, string tabs)
        {
            return new CommandEntry("error \"Script run rejected: " + message.Replace('\"', '\'') + "\"", 0, 0, system.TheErrorCommand,
                new Argument[] { new Argument() { Bits = new ArgumentBit[] { new TextArgumentBit(message, true, true, system) } } }, "error", CommandPrefix.NONE, script, 0, tabs, system);

        }

        /// <summary>Create an entry that represents invalid output.</summary>
        public static CommandEntry CreateInvalidOutputEntry(string name, List<Argument> _arguments,
            ScriptEngine system, string line, CommandPrefix prefix, string script, int linen, string tabs, Dictionary<string, Argument> nameds, ScriptEngine sys)
        {
            if (sys.Context.ShouldErrorOnInvalidCommand())
            {
                throw new ErrorInducedException("Unknown command '" + name + "'!");
            }
            _arguments.Insert(0, ArgumentParser.SplitToArgument(system, name, false));
            return new CommandEntry(line, 0, 0, system.DebugInvalidCommand, _arguments.ToArray(), name, prefix, script, linen, tabs, nameds, sys);

        }
    }
}
