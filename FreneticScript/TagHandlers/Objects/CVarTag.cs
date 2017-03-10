using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreneticScript.TagHandlers.Objects
{
    class CVarTag : TemplateObject
    {
        // <--[object]
        // @Type CVarTag
        // @SubType TextTag
        // @Group Variables
        // @Description Represents a global control variable.
        // -->
        
        public static CVarTag For(TagData data, string input)
        {
            CVar tcv = data.TagSystem.CommandSystem.Output.CVarSys.Get(input);
            if (tcv == null)
            {
                data.Error("Invalid CVar specified!");
                return null;
            }
            return new CVarTag(tcv);
        }

        public CVar Internal;
        
        public static CVarTag For(TemplateObject input, TagData data)
        {
            return input as CVarTag ?? For(data, input.ToString());
        }

        /// <summary>
        /// Constructs a CVar tag.
        /// </summary>
        public CVarTag(CVar _cvar)
        {
            Internal = _cvar;
        }

        /// <summary>
        /// The CVarTag type.
        /// </summary>
        public const string TYPE = "cvartag";

#pragma warning disable 1591

        [TagMeta(TagType = TYPE, Name = "duplicate", Group = "Tag System", ReturnType = TYPE, Returns = "A perfect duplicate of this object.")]
        public static CVarTag Tag_Duplicate(CVarTag obj, TagData data)
        {
            return new CVarTag(obj.Internal);
        }

        [TagMeta(TagType = TYPE, Name = "type", Group = "Tag System", ReturnType = TagTypeTag.TYPE, Returns = "The type of this object (CVarTag).")]
        public static TagTypeTag Tag_Type(CVarTag obj, TagData data)
        {
            return new TagTypeTag(data.TagSystem.Type_Cvar);
        }

        [TagMeta(TagType = TYPE, Name = "value_boolean", Group = "Variables", ReturnType = BooleanTag.TYPE,
            Returns = "Whether the CVar is marked 'true'.")]
        public static BooleanTag Tag_Value_Boolean(CVarTag obj, NumberTag modifier)
        {
            return new BooleanTag(obj.Internal.ValueB);
        }

        [TagMeta(TagType = TYPE, Name = "value_integer", Group = "Variables", ReturnType = IntegerTag.TYPE,
            Returns = "The integer number value of the CVar.")]
        public static IntegerTag Tag_Value_Integer(CVarTag obj, NumberTag modifier)
        {
            return new IntegerTag(obj.Internal.ValueL);
        }

        [TagMeta(TagType = TYPE, Name = "value_text", Group = "Variables", ReturnType = TextTag.TYPE,
            Returns = "The value of the CVar as plain text.")]
        public static TextTag Tag_Value_Text(CVarTag obj, NumberTag modifier)
        {
            return new TextTag(obj.Internal.Value);
        }

        [TagMeta(TagType = TYPE, Name = "Value_Number", Group = "Variables", ReturnType = NumberTag.TYPE,
            Returns = "The decimal number value of the CVar.")]
        public static NumberTag Tag_Value_Number(CVarTag obj, NumberTag modifier)
        {
            return new NumberTag(obj.Internal.ValueD);
        }

        [TagMeta(TagType = TYPE, Name = "name", Group = "Variables", ReturnType = TextTag.TYPE,
            Returns = "The name of the CVar.")]
        public static TextTag Tag_Name(CVarTag obj, NumberTag modifier)
        {
            return new TextTag(obj.Internal.Name);
        }

        [TagMeta(TagType = TYPE, Name = "info", Group = "Variables", ReturnType = TextTag.TYPE,
            Returns = "The full <@link command cvarinfo>cvarinfo<@/link> output of the CVar.")]
        public static TextTag Tag_Info(CVarTag obj, NumberTag modifier)
        {
            return new TextTag(obj.Internal.Info());
        }

        [TagMeta(TagType = TYPE, Name = "server_controlled", Group = "Variables", ReturnType = BooleanTag.TYPE,
            Returns = "Whether the CVar is server controlled only.")]
        public static BooleanTag Tag_Server_Controlled(CVarTag obj, NumberTag modifier)
        {
            return new BooleanTag(obj.Internal.Flags.HasFlag(CVarFlag.ServerControl));
        }

        [TagMeta(TagType = TYPE, Name = "read_only", Group = "Variables", ReturnType = BooleanTag.TYPE,
            Returns = "Whether the CVar is read only.")]
        public static BooleanTag Tag_Read_Only(CVarTag obj, NumberTag modifier)
        {
            return new BooleanTag(obj.Internal.Flags.HasFlag(CVarFlag.ReadOnly));
        }

        [TagMeta(TagType = TYPE, Name = "is_boolean", Group = "Variables", ReturnType = BooleanTag.TYPE,
            Returns = "Whether the CVar is treated as a boolean by the system.")]
        public static BooleanTag Tag_Is_Boolean(CVarTag obj, NumberTag modifier)
        {
            return new BooleanTag(obj.Internal.Flags.HasFlag(CVarFlag.Boolean));
        }

        [TagMeta(TagType = TYPE, Name = "is_number", Group = "Variables", ReturnType = BooleanTag.TYPE,
            Returns = "Whether the CVar is treated as a decimal number by the system.")]
        public static BooleanTag Tag_Is_Number(CVarTag obj, NumberTag modifier)
        {
            return new BooleanTag(obj.Internal.Flags.HasFlag(CVarFlag.Numeric));
        }

        [TagMeta(TagType = TYPE, Name = "is_text", Group = "Variables", ReturnType = BooleanTag.TYPE,
            Returns = "Whether the CVar is treated as text by the system.")]
        public static BooleanTag Tag_Is_Text(CVarTag obj, NumberTag modifier)
        {
            return new BooleanTag(obj.Internal.Flags.HasFlag(CVarFlag.Textual));
        }

        [TagMeta(TagType = TYPE, Name = "delayed", Group = "Variables", ReturnType = BooleanTag.TYPE,
            Returns = "Whether the CVar has a delayed value read (requires a reload or restart).")]
        public static BooleanTag Tag_Delayed(CVarTag obj, NumberTag modifier)
        {
            return new BooleanTag(obj.Internal.Flags.HasFlag(CVarFlag.Delayed));
        }

        [TagMeta(TagType = TYPE, Name = "init_only", Group = "Variables", ReturnType = BooleanTag.TYPE,
            Returns = "Whether the CVar is only able to be set before the system has initialized.")]
        public static BooleanTag Tag_Init_Only(CVarTag obj, NumberTag modifier)
        {
            return new BooleanTag(obj.Internal.Flags.HasFlag(CVarFlag.InitOnly));
        }

        [TagMeta(TagType = TYPE, Name = "user_made", Group = "Variables", ReturnType = BooleanTag.TYPE,
            Returns = "Whether the CVar is was made by a command instead of being for internal-use.")]
        public static BooleanTag Tag_User_Made(CVarTag obj, NumberTag modifier)
        {
            return new BooleanTag(obj.Internal.Flags.HasFlag(CVarFlag.UserMade));
        }

#pragma warning restore 1591

        public override string ToString()
        {
            return Internal.Name;
        }
    }
}
