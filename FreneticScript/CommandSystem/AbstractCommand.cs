//
// This file is part of FreneticScript, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of the MIT license.
// See README.md or LICENSE.txt in the FreneticScript source root for the contents of the license.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using FreneticScript.CommandSystem.Arguments;
using FreneticScript.ScriptSystems;
using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Objects;

namespace FreneticScript.CommandSystem
{
    /// <summary>The base for a command.</summary>
    public abstract class AbstractCommand
    {
        /// <summary>Represents the "Execute(CommandQueue, CommandEntry)" method for this command.</summary>
        public MethodInfo ExecuteMethod = null;

        /// <summary>Initializes the abstract command.</summary>
        public AbstractCommand()
        {
            ExecuteMethod = GetType().GetMethod("Execute", BindingFlags.Public | BindingFlags.Static);
            Meta = GetType().GetCustomAttribute<CommandMeta>() ?? new CommandMeta();
        }

        /// <summary>The system that owns this command.</summary>
        public ScriptEngine Engine;

        /// <summary>The meta-documentation for this command.</summary>
        public CommandMeta Meta;

#warning Temporary legacy command registration support until all commands have Meta.
        /// <summary>Legacy.</summary>
        public string Name { set { Meta.Name = value; } }
        /// <summary>Legacy.</summary>
        public string Arguments { set { Meta.Arguments = value; } }
        /// <summary>Legacy.</summary>
        public string Description { set { Meta.Description = value; } }
        /// <summary>Legacy.</summary>
        public int MinimumArguments { set { Meta.MinimumArgs = value; } }
        /// <summary>Legacy.</summary>
        public int MaximumArguments { set { Meta.MaximumArgs = value; } }
        /// <summary>Legacy.</summary>
        public bool Asyncable { set { Meta.Asyncable = value; } }
        /// <summary>Legacy.</summary>
        public bool IsFlow { set { Meta.IsFlow = value; } }
        /// <summary>Legacy.</summary>
        public bool IsBreakable { set { Meta.IsBreakable = value; } }
        /// <summary>Legacy.</summary>
        public bool Waitable { set { Meta.Waitable = value; } }
        /// <summary>Legacy.</summary>
        public bool IsDebug { set { Meta.IsDebug = value; } }

        /// <summary>In what way the command saves. Also set <see cref="DefaultSaveName"/> if relevant.</summary>
        public CommandSaveMode SaveMode = CommandSaveMode.NO_SAVE;

        /// <summary>The name of the tag type to save as. By default is set to <see cref="DynamicTag.TYPE"/>.</summary>
        public string SaveType = DynamicTag.TYPE;

        /// <summary>The default save name, if <see cref="SaveMode"/> is set to <see cref="CommandSaveMode.DEFAULT_NAME"/>.</summary>
        public string DefaultSaveName = null;

        /// <summary>The expected object type getters for a command, for validation reasons.</summary>
        public Action<ArgumentValidation>[] ObjectTypes = null;

        /// <summary>Tests if the CommandEntry is valid for this command at pre-process time.</summary>
        /// <param name="entry">The entry to test</param>
        /// <returns>An error message (with tags), or null for none.</returns>
        public virtual string TestForValidity(CommandEntry entry)
        {
            if (entry.Arguments.Length < Meta.MinimumArgs)
            {
                return $"Not enough arguments. Expected at least: {TextStyle.SeparateVal(Meta.MinimumArgs)}. Usage: {TextStyle.SeparateVal(Meta.Arguments)}, found only: {TextStyle.SeparateVal(entry.AllOriginalArguments())}";
            }
            if (Meta.MaximumArgs != -1 && entry.Arguments.Length > Meta.MaximumArgs)
            {
                return $"Too many arguments. Expected no more than: {TextStyle.SeparateVal(Meta.MaximumArgs)}. Usage: {TextStyle.SeparateVal(Meta.Arguments)}, found: {TextStyle.SeparateVal(entry.AllOriginalArguments())}";
            }
            if (ObjectTypes != null)
            {
                ArgumentValidation validator = new()
                {
                    Entry = entry
                };
                for (int i = 0; i < entry.Arguments.Length; i++)
                {
                    if (entry.Arguments[i].Bits.Length == 1
                        && entry.Arguments[i].Bits[0] is TextArgumentBit tab
                        && i < ObjectTypes.Length)
                    {
                        if (ObjectTypes[i] == null)
                        {
                            continue;
                        }
                        validator.ObjectValue = tab.InputValue;
                        ObjectTypes[i].Invoke(validator);
                        if (validator.ErrorResult != null)
                        {
                            return $"Invalid argument '{TextStyle.SeparateVal(entry.Arguments[i].ToString())}' for command '{TextStyle.SeparateVal(entry.Command.Meta.Name)}': " + validator.ErrorResult;
                        }
                        if (validator.ObjectValue == null)
                        {
                            return $"Invalid argument '{TextStyle.SeparateVal(entry.Arguments[i].ToString())}', translates to internal NULL for this command's input expectation "
                                + $"(Command is {TextStyle.SeparateVal(entry.Command.Meta.Name)}). (Dev note: expectation is {TextStyle.SeparateVal(ObjectTypes[i].Method.Name)})";
                        }
                        ((TextArgumentBit)entry.Arguments[i].Bits[0]).InputValue = validator.ObjectValue;
                    }
                }
            }
            return null;
        }

        /// <summary>Gets the follower (callback) entry for an entry.</summary>
        /// <param name="entry">The entry.</param>
        public static CommandEntry GetFollower(CommandEntry entry)
        {
            return new CommandEntry("CALLBACK:" + entry.Name, entry.BlockStart, entry.BlockEnd, entry.Command, new Argument[] { new Argument() { Bits = Array.Empty<ArgumentBit>() } },
                entry.Name, CommandPrefix.CALLBACK, entry.ScriptName, entry.ScriptLine, entry.FairTabulation + "    ", entry.System);
        }

        /// <summary>Adapts a command entry to CIL.</summary>
        /// <param name="values">The adaptation-relevant values.</param>
        /// <param name="entry">The relevant entry ID.</param>
        public virtual void AdaptToCIL(CILAdaptationValues values, int entry)
        {
            values.CallExecute(entry, this);
        }

        /// <summary>Prepares to adapt a command entry to CIL.</summary>
        /// <param name="values">The adaptation-relevant values.</param>
        /// <param name="entry">The relevant entry ID.</param>
        public virtual void PreAdaptToCIL(CILAdaptationValues values, int entry)
        {
            if (SaveMode != CommandSaveMode.NO_SAVE)
            {
                CommandEntry cent = values.CommandAt(entry);
                TagType saveTagType = cent.System.TagTypes.TypeForName(SaveType) ?? throw new ErrorInducedException($"Command '{TextStyle.SeparateVal(Meta.Name)}' specifies a non-existent save tag type '{TextStyle.SeparateVal(SaveType)}'.");
                PreAdaptSaveMode(values, entry, true, saveTagType, SaveMode != CommandSaveMode.WHEN_NAME_SPECIFIED, DefaultSaveName);
            }
        }

        /// <summary>Pre-Adapt helper for save targets.</summary>
        /// <param name="values">The adaptation-relevant values.</param>
        /// <param name="entry">The relevant entry ID.</param>
        /// <param name="canPreExist">Whether the variable is allowed to already exist.</param>
        /// <param name="tagType">The required type.</param>
        /// <param name="required">Whether a save name *must* be specified by the user.</param>
        /// <param name="defaultName">The default name (or null if none).</param>
        public void PreAdaptSaveMode(CILAdaptationValues values, int entry, bool canPreExist, TagType tagType, bool required, string defaultName = null)
        {
            CommandEntry cent = values.CommandAt(entry);
            string saveName = cent.GetSaveNameNoParse(defaultName);
            if (saveName == null)
            {
                if (!required)
                {
                    return;
                }
                throw new ErrorInducedException($"Command '{Meta.Name}' requires a save name, but none was given.");
            }
            int preVarLoc = values.LocalVariableLocation(saveName, out TagReturnType preVarType);
            if (preVarLoc >= 0)
            {
                if (!canPreExist)
                {
                    throw new ErrorInducedException($"Already have a save target var (labeled '{TextStyle.SeparateVal(saveName)}')?!");
                }
                if (preVarType.Type != tagType)
                {
                    throw new ErrorInducedException($"Already have a save target var (labeled '{TextStyle.SeparateVal(saveName)}', with type "
                        + $"'{TextStyle.SeparateVal(preVarType.Type.TypeName)}') of wrong type (expected '{TextStyle.SeparateVal(tagType.TypeName)}').");
                }
                cent.SaveLoc = preVarLoc;
            }
            else
            {
                cent.SaveLoc = values.AddVariable(saveName, tagType);
            }
        }

        /// <summary>
        /// Gets the save variable location (used in Adapt, after using <see cref="PreAdaptSaveMode(CILAdaptationValues, int, bool, TagType, bool, string)"/> in PreAdapt).
        /// </summary>
        /// <param name="values">The adaptation-relevant values.</param>
        /// <param name="entry">The relevant entry ID.</param>
        /// <param name="defaultName">The default name (or null if none).</param>
        public static int GetSaveLoc(CILAdaptationValues values, int entry, string defaultName = null)
        {
            CommandEntry cent = values.CommandAt(entry);
            string sn = values.Entry.Entries[cent.BlockStart - 1].GetSaveNameNoParse(defaultName);
            return cent.VarLoc(sn);
        }

        /// <summary>Adjust list of commands that are formed by an inner block.</summary>
        /// <param name="entry">The producing entry.</param>
        /// <param name="input">The block of commands.</param>
        /// <param name="fblock">The final block to add to the entry.</param>
        public virtual void AdaptBlockFollowers(CommandEntry entry, List<CommandEntry> input, List<CommandEntry> fblock)
        {
            input.Add(GetFollower(entry));
        }

        /// <summary>Displays the usage information on a command to the console.</summary>
        /// <param name="queue">The associated queue.</param>
        /// <param name="entry">The CommandEntry data to show usage help to.</param>
        /// <param name="doError">Whether to end with an error.</param>
        /// <param name="cmd">The command to show help for - if unspecified, will get from the entry.</param>
        public static void ShowUsage(CommandQueue queue, CommandEntry entry, bool doError = true, AbstractCommand cmd = null)
        {
            cmd ??= entry.Command;
            if (entry.ShouldShowGood(queue))
            {
                entry.InfoOutput(queue, TextStyle.Separate + cmd.Meta.Name + TextStyle.Base + ": " + cmd.Meta.Description);
                entry.InfoOutput(queue, TextStyle.Commandhelp + "Usage: /" + cmd.Meta.Name + " " + cmd.Meta.Arguments);
                if (cmd.Meta.IsDebug)
                {
                    entry.InfoOutput(queue, "Note: This command is intended for debugging purposes.");
                }
            }
            if (doError)
            {
                queue.HandleError(entry, "Invalid arguments or not enough arguments!");
            }
        }
    }

    /// <summary>Helper class for argument validation.</summary>
    public class ArgumentValidation
    {
        /// <summary>The argument value to validate or replace.</summary>
        public TemplateObject ObjectValue;

        /// <summary>The command entry being validated.</summary>
        public CommandEntry Entry;

        /// <summary>An error result, if any.</summary>
        public string ErrorResult = null;

        /// <summary>An error action helper method, for when an error action is needed.</summary>
        /// <param name="s">The error message.</param>
        public void ErrorAction(string s)
        {
            ErrorResult = s;
        }
    }
}
