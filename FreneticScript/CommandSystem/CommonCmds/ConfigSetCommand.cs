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

namespace FreneticScript.CommandSystem.CommonCmds
{
    /// <summary>The ConfigSet command: sets a value in a config.</summary>
    [CommandMeta(
        Name = "configset",
        Arguments = "<config name> <key> <value>",
        Short = "Sets a value in a config.",
        Updated = "2022/03/02",
        Group = "Common",
        MinimumArgs = 3, MaximumArgs = 3,
        Description = "The first argument is a config name - must correspond to a config provided in the engine context.\n"
                    + "The second argument is the setting key within the config.\n"
                    + "The third argument is the new value of the key. Must be of a correct type for the setting key.\n",
        Examples = new[]{ "// This example presumes config 'test' has key 'TestString', and sets it.\n"
                        + "configset test TestString \"this is some text!\";"}
        )]
    public class ConfigSetCommand : AbstractCommand
    {
        /// <summary>Constructs the ConfigSet command.</summary>
        public ConfigSetCommand()
        {
            ObjectTypes = new Action<ArgumentValidation>[]
            {
                TextTag.Validator,
                TextTag.Validator,
                null
            };
        }

        /// <summary>Converts a script object type to a specific raw type (if possible) for the ConfigSet command.</summary>
        public static object ConvertForType(Type fieldType, TemplateObject input, CommandQueue queue)
        {
            if (fieldType == typeof(string))
            {
                return input.ToString();
            }
            else if (fieldType == typeof(bool))
            {
                return BooleanTag.TryFor(input)?.Internal;
            }
            else if (fieldType == typeof(long))
            {
                return IntegerTag.TryFor(input)?.Internal;
            }
            else if (fieldType == typeof(int))
            {
                IntegerTag integer = IntegerTag.TryFor(input);
                if (integer is not null)
                {
                    return (int)integer.Internal;
                }
            }
            else if (fieldType == typeof(short))
            {
                IntegerTag integer = IntegerTag.TryFor(input);
                if (integer is not null)
                {
                    return (short)integer.Internal;
                }
            }
            else if (fieldType == typeof(byte))
            {
                IntegerTag integer = IntegerTag.TryFor(input);
                if (integer is not null)
                {
                    return (byte)integer.Internal;
                }
            }
            else if (fieldType == typeof(double))
            {
                return NumberTag.TryFor(input)?.Internal;
            }
            else if (fieldType == typeof(float))
            {
                NumberTag number = NumberTag.TryFor(input);
                if (number is not null)
                {
                    return (float)number.Internal;
                }
            }
            else
            {
                queue.HandleError($"Cannot convert script objects to config type {TextStyle.SeparateVal(fieldType.Name)}");
            }
            return null;
        }

        /// <summary>Executes the command.</summary>
        /// <param name="queue">The command queue involved.</param>
        /// <param name="entry">Entry to be executed.</param>
        public static void Execute(CommandQueue queue, CommandEntry entry)
        {
            string configName = entry.GetArgument(queue, 0);
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
            TemplateObject newValue = entry.GetArgumentObject(queue, 2);
            object rawValue = ConvertForType(field.Field.FieldType, newValue, queue);
            field.SetValue(section, rawValue);
            field.OnChanged?.Invoke();
            if (queue.ShouldShowGood())
            {
                queue.GoodOutput($"For config '{TextStyle.SeparateVal(configName)}', set '{TextStyle.SeparateVal(configKey)}' to '{TextStyle.SeparateVal(rawValue)}'");
            }
        }
    }
}
