using System;
using FreneticScript.TagHandlers.Objects;

namespace FreneticScript
{
    /// <summary>
    /// Adds HasFlag to the CVarFlag enum, for .NET 3.5 usage of a .NET 4.0 trick.
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        /// Returns whether the Flag set has a specific flag.
        /// </summary>
        /// <param name="tenum">The flag set.</param>
        /// <param name="val">The specific flag.</param>
        /// <returns>Whether it is had.</returns>
        public static bool HasFlag(this CVarFlag tenum, CVarFlag val)
        {
            return (tenum & val) != 0;
        }
    }

    /// <summary>
    /// The various possible information flags a CVar can hold.
    /// </summary>
    [Flags]
    public enum CVarFlag
    {
        /// <summary>
        /// No information.
        /// </summary>
        None = 0x0000,
        /// <summary>
        /// This flag cannot be edited, and exists to represent system information.
        /// </summary>
        ReadOnly = 0x0001,
        /// <summary>
        /// This flag should be treated as text.
        /// </summary>
        Textual = 0x0002,
        /// <summary>
        /// This tag should be treated as a number.
        /// </summary>
        Numeric = 0x0004,
        /// <summary>
        /// This flag should be treated as true/false.
        /// </summary>
        Boolean = 0x0008,
        /// <summary>
        /// This flag won't immediately have an effect when edited.
        /// </summary>
        Delayed = 0x0010,
        /// <summary>
        /// This flag was made by a user.
        /// </summary>
        UserMade = 0x0020,
        /// <summary>
        /// This flag can only be modified during load time.
        /// </summary>
        InitOnly = 0x0040,
        /// <summary>
        /// This flag is on a client, but controlled by the server.
        /// </summary>
        ServerControl = 0x0080,
        /// <summary>
        /// This flag should not be saved across system restarts.
        /// Useful for scripts to use when tracking temporary data.
        /// </summary>
        DoNotSave = 0x0100,
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
        CVarSystem system;

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
            system = _system;
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
            if (Flags.HasFlag(CVarFlag.ReadOnly))
            {
                return;
            }
            if (Flags.HasFlag(CVarFlag.InitOnly) && !system.Output.Initializing)
            {
                return;
            }
            if (Flags.HasFlag(CVarFlag.ServerControl) && !force)
            {
                return;
            }
            Value = newvalue;
            ValueL = FreneticScriptUtilities.StringToLong(newvalue);
            ValueI = (int)ValueL;
            ValueD = FreneticScriptUtilities.StringToDouble(newvalue);
            ValueF = (float)ValueD;
            ValueB = newvalue.ToLowerFast() == "true" || ValueF > 0f;
            if (!force)
            {
                system.Modified = true;
            }
            OnChanged?.Invoke(this, null);
        }

        /// <summary>
        /// Sets the CVar to a new value.
        /// </summary>
        /// <param name="value">The value to set the CVar to.</param>
        public void Set(bool value)
        {
            if (Flags.HasFlag(CVarFlag.ReadOnly))
            {
                return;
            }
            if (Flags.HasFlag(CVarFlag.InitOnly) && !system.Output.Initializing)
            {
                return;
            }
            if (Flags.HasFlag(CVarFlag.ServerControl))
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
            system.Modified = true;
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
            foreach (CVarFlag flag in Enum.GetValues(typeof(CVarFlag)))
            {
                if (flag != CVarFlag.None && Flags.HasFlag(flag))
                {
                    list.Internal.Add(new TextTag(flag.ToString()));
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
            return TextStyle.Color_Simple + "Name: '" + TextStyle.Color_Separate + Name + TextStyle.Color_Simple + "', value: '" +
                TextStyle.Color_Separate + Value + TextStyle.Color_Simple + "', numeric value: " + TextStyle.Color_Separate +
                ValueD + TextStyle.Color_Simple + ", boolean value: " + TextStyle.Color_Separate + (ValueB ? "true" : "false") +
                TextStyle.Color_Simple + ", flags: " + TextStyle.Color_Separate + FlagInfo() + TextStyle.Color_Simple
                + ", description: '" + TextStyle.Color_Separate + Description + TextStyle.Color_Simple + "'";
        }
    }
}
