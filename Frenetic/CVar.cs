using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Frenetic
{
    [Flags]
    public enum CVarFlag
    {
        None = 0x0,
        ReadOnly = 0x1,
        Textual = 0x2,
        Numeric = 0x4,
        Boolean = 0x8,
        Delayed = 0x10,
        UserMade = 0x20,
        InitOnly = 0x40,
        ServerControl = 0x80,
    }
    // <--[explanation]
    // @Name CVars
    // @Description
    // CVars are global control variables.
    // TODO: Explain better!
    // -->
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

        public CVar(string newname, string newvalue, CVarFlag newflags, CVarSystem _system)
        {
            system = _system;
            Name = newname;
            Set(newvalue);
            Flags = newflags;
        }

        /// <summary>
        /// Sets the CVar to a new value.
        /// </summary>
        /// <param name="newvalue">The value to set the CVar to</param>
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
            ValueL = FreneticUtilities.StringToLong(newvalue);
            ValueI = (int)ValueL;
            ValueD = FreneticUtilities.StringToDouble(newvalue);
            ValueF = (float)ValueD;
            ValueB = newvalue.ToLower() == "true" || ValueF > 0f;
            system.Modified = true;
        }

        /// <summary>
        /// Sets the CVar to a new value.
        /// </summary>
        /// <param name="newvalue">The value to set the CVar to</param>
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
        }

        /// <summary>
        /// Returns a human-readable list of flags.
        /// </summary>
        /// <returns>The flag list</returns>
        public string FlagInfo()
        {
            if (Flags == CVarFlag.None)
            {
                return "None";
            }
            string Type = null;
            if (Flags.HasFlag(CVarFlag.Boolean))
            {
                Type = "Boolean";
            }
            else if (Flags.HasFlag(CVarFlag.Textual))
            {
                Type = "Textual";
            }
            else if (Flags.HasFlag(CVarFlag.Numeric))
            {
                Type = "Numeric";
            }
            else if (Flags.HasFlag(CVarFlag.UserMade))
            {
                Type = "User-Made";
            }
            if (Flags.HasFlag(CVarFlag.ReadOnly))
            {
                if (Type != null)
                {
                    return "ReadOnly, " + Type;
                }
                else
                {
                    return "ReadOnly";
                }
            }
            else if (Flags.HasFlag(CVarFlag.Delayed))
            {
                if (Type != null)
                {
                    return "Delayed, " + Type;
                }
                else
                {
                    return "Delayed";
                }
            }
            else if (Flags.HasFlag(CVarFlag.ServerControl))
            {
                if (Type != null)
                {
                    return "ServerControlled, " + Type;
                }
                else
                {
                    return "ServerControlled";
                }
            }
            else
            {
                if (Type != null)
                {
                    return Type;
                }
                else
                {
                    return "???UNKNOWN-FLAGS???";
                }
            }
        }

        /// <summary>
        /// Returns a human-readable colored information line from this CVar.
        /// </summary>
        /// <returns>The information</returns>
        public string Info()
        {
            return TextStyle.Color_Simple + "Name: '" + TextStyle.Color_Separate + Name + TextStyle.Color_Simple + "', value: '" +
                TextStyle.Color_Separate + Value + TextStyle.Color_Simple + "', numeric value: " + TextStyle.Color_Separate +
                ValueD + TextStyle.Color_Simple + ", boolean value: " + TextStyle.Color_Separate + (ValueB ? "true" : "false") +
                TextStyle.Color_Simple + ", flags: " + TextStyle.Color_Separate + FlagInfo();
        }
    }
}
