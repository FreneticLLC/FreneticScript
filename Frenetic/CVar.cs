using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Frenetic
{
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
        /// Construct a CVar.
        /// </summary>
        /// <param name="newname">The name of  the CVar</param>
        /// <param name="newvalue">The value to set the CVar to</param>
        /// <param name="newflags">The flags the CVar should be locked into</param>
        /// <param name="_system">The CVarSystem to create this CVar within</param>
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
        /// <param name="newvalue">The value to set the CVar to</param>
        /// <param name="force">Whether to force the edit (EG, a server has demanded the change)</param>
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
            if (!force)
            {
                system.Modified = true;
            }
            if (OnChanged != null)
            {
                OnChanged(this, null);
            }
        }

        /// <summary>
        /// Sets the CVar to a new value.
        /// </summary>
        /// <param name="value">The value to set the CVar to</param>
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
            if (OnChanged != null)
            {
                OnChanged(this, null);
            }
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
