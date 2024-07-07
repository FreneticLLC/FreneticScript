//
// This file is part of FreneticScript, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of the MIT license.
// See README.md or LICENSE.txt in the FreneticScript source root for the contents of the license.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.CommandSystem;
using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Objects;
using FreneticUtilities.FreneticDataSyntax;
using FreneticUtilities.FreneticExtensions;

namespace FreneticScript.CommandSystem.CommonCmds;

/// <summary>The ConfigToggle command: toggles a boolean value in a config.</summary>
[CommandMeta(
    Name = "configtoggle",
    Arguments = "<config name> <key>",
    Short = "Sets a value in a config.",
    Updated = "2022/03/02",
    Group = "Common",
    MinimumArgs = 2, MaximumArgs = 2,
    Description = "The first argument is a config name - must correspond to a config provided in the engine context.\n"
                + "The second argument is the setting key within the config. Must be a boolean type.\n",
    Examples = [ "// This example presumes config 'test' has key 'TestBoolean', and toggles it.\n"
                    + "configtoggle test TestBoolean;"]
    )]
public class ConfigToggleCommand : AbstractCommand
{
    /// <summary>Constructs the ConfigToggle command.</summary>
    public ConfigToggleCommand()
    {
        ObjectTypes =
        [
            TextTag.Validator,
            TextTag.Validator
        ];
    }

    /// <summary>Executes the command.</summary>
    /// <param name="queue">The command queue involved.</param>
    /// <param name="entry">Entry to be executed.</param>
    public static void Execute(CommandQueue queue, CommandEntry entry)
    {
        string configName = entry.GetArgument(queue, 0).ToLowerFast();
        AutoConfiguration config = queue.Engine.Context.GetConfig(configName);
        if (config is null)
        {
            queue.HandleError(entry, $"Invalid config name '{TextStyle.SeparateVal(configName)}' - are you sure you typed it correctly?");
            return;
        }
        string configKey = entry.GetArgument(queue, 1);
        AutoConfiguration.Internal.SingleFieldData field = config.TryGetFieldInternalData(configKey, out AutoConfiguration section, true);
        if (field is null)
        {
            queue.HandleError(entry, $"Invalid config setting key '{TextStyle.SeparateVal(configKey)}' - are you sure you typed it correctly?");
            return;
        }
        if (field.Field.FieldType != typeof(bool))
        {
            queue.HandleError(entry, $"Invalid config setting key '{TextStyle.SeparateVal(configKey)}' - not a bool. Cannot be toggled.");
            return;
        }
        bool currentValue = (bool)field.GetValue(section);
        field.SetValue(section, !currentValue);
        field.OnChanged?.Invoke();
        queue.Engine.Context.SignalDidChangeConfig(configName);
        if (queue.ShouldShowGood())
        {
            entry.GoodOutput(queue, $"For config '{TextStyle.SeparateVal(configName)}', toggled '{TextStyle.SeparateVal(configKey)}' to '{TextStyle.SeparateVal(!currentValue)}'");
        }
    }
}
