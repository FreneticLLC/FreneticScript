//
// This file is part of FreneticScript, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of the MIT license.
// See README.md or LICENSE.txt in the FreneticScript source root for the contents of the license.
//

using System;
using FreneticScript.TagHandlers.Objects;
using FreneticUtilities.FreneticExtensions;
using FreneticUtilities.FreneticToolkit;

namespace FreneticScript
{
    /// <summary>
    /// Helper for <see cref="CVarFlag"/>.
    /// </summary>
    public static class CVarFlagEnumExtensions
    {
        /// <summary>
        /// Returns whether the mainVal (as a bitflag set) has the required testVal (as a bitflag set).
        /// </summary>
        /// <param name="mainVal">The set of flags present.</param>
        /// <param name="testVal">The set of flags required.</param>
        /// <returns>Whether the flags are present as required.</returns>
        public static bool HasFlagsFS(this CVarFlag mainVal, CVarFlag testVal)
        {
            return (mainVal & testVal) == testVal;
        }
    }

    /// <summary>
    /// The various possible information flags a CVar can hold.
    /// </summary>
    [Flags]
    public enum CVarFlag : int
    {
        /// <summary>
        /// No information.
        /// </summary>
        None = 0,
        /// <summary>
        /// This flag cannot be edited, and exists to represent system information.
        /// </summary>
        ReadOnly = 1,
        /// <summary>
        /// This flag should be treated as text.
        /// </summary>
        Textual = 1 << 1,
        /// <summary>
        /// This tag should be treated as a number.
        /// </summary>
        Numeric = 1 << 2,
        /// <summary>
        /// This flag should be treated as true/false.
        /// </summary>
        Boolean = 1 << 3,
        /// <summary>
        /// This flag won't immediately have an effect when edited.
        /// </summary>
        Delayed = 1 << 4,
        /// <summary>
        /// This flag was made by a user.
        /// </summary>
        UserMade = 1 << 5,
        /// <summary>
        /// This flag can only be modified during load time.
        /// </summary>
        InitOnly = 1 << 6,
        /// <summary>
        /// This flag is on a client, but controlled by the server.
        /// </summary>
        ServerControl = 1 << 7,
        /// <summary>
        /// This flag should not be saved across system restarts.
        /// Useful for scripts to use when tracking temporary data.
        /// </summary>
        DoNotSave = 1 << 8,
    }

    // <--[explanation]
    // @Name CVars
    // @Description
    // CVars are global control variables.
    // TODO: Explain better!
    // -->

    /// <summary>
    /// Represents a name:value pair within a complex system.
    /// </summary>
    public class CVar
    {
        /// <summary>
        /// The name of the CVar.
        /// </summary>
        public string Name;

        /// <summary>
        /// The value of the CVar, as text.
        /// </summary>
        public string Value;

        /// <summary>
        /// The value of the CVar, as a long.
        /// </summary>
        public long ValueL;

        /// <summary>
        /// The value of the CVar, as an int.
        /// </summary>
        public int ValueI;

        /// <summary>
        /// The value of the CVar, as a double.
        /// </summary>
        public double ValueD;

        /// <summary>
        /// The value of the CVar, as a float.
        /// </summary>
        public float ValueF;

        /// <summary>
        /// The value of the CVar, as a boolean.
        /// </summary>
        public bool ValueB;

        /// <summary>
        /// The CVar flags set.
        /// </summary>
        public CVarFlag Flags;

        /// <summary>
        /// The system that generated this CVar.
        /// </summary>
        public CVarSystem System;

        /// <summary>
        /// An implementor can optionally apply a description to a CVar to show in Info() and any implementor-managed code.
        /// </summary>
        public string Description;

        /// <summary>
        /// Any object can be attached to a CVar to mark it for the implementing engine.
        /// </summary>
        public Object Tag;

        /// <summary>
        /// Construct a CVar.
        /// </summary>
        /// <param name="newname">The name of  the CVar.</param>
        /// <param name="newvalue">The value to set the CVar to.</param>
        /// <param name="newflags">The flags the CVar should be locked into.</param>
        /// <param name="_system">The CVarSystem to create this CVar within.</param>
        public CVar(string newname, string newvalue, CVarFlag newflags, CVarSystem _system)
        {
            System = _system;
            Name = newname;
            Set(newvalue);
            Flags = newflags;
        }

        /// <summary>
        /// This event is called when the CVar is changed.
        /// </summary>
        public EventHandler OnChanged;

        /// <summary>
        /// Sets the CVar to a new value.
        /// A force change will not trigger a system 'modified' save.
        /// </summary>
        /// <param name="newvalue">The value to set the CVar to.</param>
        /// <param name="force">Whether to force the edit (EG, a server has demanded the change).</param>
        public void Set(string newvalue, bool force = false)
        {
            if (newvalue == Value)
            {
                return;
            }
            if (Flags.HasFlagsFS(CVarFlag.ReadOnly))
            {
                return;
            }
            if (Flags.HasFlagsFS(CVarFlag.InitOnly) && !System.Output.Initializing)
            {
                return;
            }
            if (Flags.HasFlagsFS(CVarFlag.ServerControl) && !force)
            {
                return;
            }
            Value = newvalue;
            ValueL = StringConversionHelper.StringToLong(newvalue);
            ValueI = (int)ValueL;
            ValueD = StringConversionHelper.StringToDouble(newvalue);
            ValueF = (float)ValueD;
            ValueB = newvalue.ToLowerFast() == "true" || ValueF > 0f;
            if (!force)
            {
                System.Modified = true;
            }
            OnChanged?.Invoke(this, null);
        }

        /// <summary>
        /// Sets the CVar to a new value.
        /// </summary>
        /// <param name="value">The value to set the CVar to.</param>
        public void Set(bool value)
        {
            if (Flags.HasFlagsFS(CVarFlag.ReadOnly))
            {
                return;
            }
            if (Flags.HasFlagsFS(CVarFlag.InitOnly) && !System.Output.Initializing)
            {
                return;
            }
            if (Flags.HasFlagsFS(CVarFlag.ServerControl))
            {
                return;
            }
            ValueB = value;
            if (value)
            {
                Value = "true";
                ValueI = 1;
                ValueF = 1;
                ValueL = 1;
                ValueD = 1;
            }
            else
            {
                Value = "false";
                ValueI = 0;
                ValueF = 0;
                ValueL = 0;
                ValueD = 0;
            }
            System.Modified = true;
            OnChanged?.Invoke(this, null);
        }

        /// <summary>
        /// Returns a human-readable list of flags.
        /// </summary>
        /// <returns>The flag list.</returns>
        public string FlagInfo()
        {
            if (Flags == CVarFlag.None)
            {
                return "None";
            }
            ListTag list = new ListTag();
            foreach (CVarFlag flag in EnumHelper<CVarFlag>.Values)
            {
                if (flag != CVarFlag.None && Flags.HasFlagsFS(flag))
                {
                    list.Internal.Add(new TextTag(EnumHelper<CVarFlag>.GetName(flag)));
                }
            }
            return list.Formatted();
        }

        /// <summary>
        /// Returns a human-readable colored information line from this CVar.
        /// </summary>
        /// <returns>The information.</returns>
        public string Info()
        {
            return TextStyle.Simple + "Name: '" + TextStyle.Separate + Name + TextStyle.Simple + "', value: '" +
                TextStyle.Separate + Value + TextStyle.Simple + "', numeric value: " + TextStyle.Separate +
                ValueD + TextStyle.Simple + ", boolean value: " + TextStyle.Separate + (ValueB ? "true" : "false") +
                TextStyle.Simple + ", flags: " + TextStyle.Separate + FlagInfo() + TextStyle.Simple
                + ", description: '" + TextStyle.Separate + Description + TextStyle.Simple + "'";
        }
    }
}
