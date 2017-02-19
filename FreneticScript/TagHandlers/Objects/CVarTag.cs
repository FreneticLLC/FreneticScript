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
        /// All tag handlers for this tag type.
        /// </summary>
        public static Dictionary<string, TagSubHandler> Handlers = new Dictionary<string, TagSubHandler>();

        // TODO: !!!
        /*
        static CVarTag()
        {
            // Documented in TextTag.
            Handlers.Add("duplicate", new TagSubHandler() { Handle = (data, obj) => new CVarTag(((CVarTag)obj).Internal), ReturnTypeString = "cvartag" });
            // Documented in TextTag.
            Handlers.Add("type", new TagSubHandler() { Handle = (data, obj) => new TagTypeTag(data.TagSystem.Type_Cvar), ReturnTypeString = "tagtypetag" });
            // <--[tag]
            // @Name CVarTag.value_boolean
            // @Group Variables
            // @ReturnType BooleanTag
            // @Returns whether the CVar is marked 'true'.
            // -->
            Handlers.Add("value_boolean", new TagSubHandler() { Handle = (data, obj) => new BooleanTag(((CVarTag)obj).Internal.ValueB).Handle(data.Shrink()), ReturnTypeString = "booleantag" });
            // <--[tag]
            // @Name CVarTag.value_integer
            // @Group Variables
            // @ReturnType IntegerTag
            // @Returns the integer number value of the CVar.
            // -->
            Handlers.Add("value_integer", new TagSubHandler() { Handle = (data, obj) => new IntegerTag(((CVarTag)obj).Internal.ValueL).Handle(data.Shrink()), ReturnTypeString = "integertag" });
            // <--[tag]
            // @Name CVarTag.value_text
            // @Group Variables
            // @ReturnType TextTag
            // @Returns the value of the CVar, as plain text.
            // -->
            Handlers.Add("value_text", new TagSubHandler() { Handle = (data, obj) => new TextTag(((CVarTag)obj).Internal.Value).Handle(data.Shrink()), ReturnTypeString = "texttag" });
            // <--[tag]
            // @Name CVarTag.value_number
            // @Group Variables
            // @ReturnType TextTag
            // @Returns the decimal number value of the CVar.
            // -->
            Handlers.Add("value_number", new TagSubHandler() { Handle = (data, obj) => new NumberTag(((CVarTag) obj).Internal.ValueD).Handle(data.Shrink()), ReturnTypeString = "numbertag" });
            // <--[tag]
            // @Name CVarTag.name
            // @Group Variables
            // @ReturnType TextTag
            // @Returns the the name of the CVar.
            // -->
            Handlers.Add("name", new TagSubHandler() { Handle = (data, obj) => new TextTag(((CVarTag)obj).Internal.Name).Handle(data.Shrink()), ReturnTypeString = "texttag" });
            // <--[tag]
            // @Name CVarTag.info
            // @Group Variables
            // @ReturnType TextTag
            // @Returns the full <@link command cvarinfo>cvarinfo<@/link> output of the CVar.
            // -->
            Handlers.Add("info", new TagSubHandler() { Handle = (data, obj) => new TextTag(((CVarTag)obj).Internal.Info()).Handle(data.Shrink()), ReturnTypeString = "texttag" });
            // <--[tag]
            // @Name CVarTag.server_controlled
            // @Group Variables
            // @Mode Client
            // @ReturnType BooleanTag
            // @Returns whether the CVar is server controlled only.
            // -->
            Handlers.Add("server_controlled", new TagSubHandler() { Handle = (data, obj) => new BooleanTag(((CVarTag)obj).Internal.Flags.HasFlag(CVarFlag.ServerControl)).Handle(data.Shrink()), ReturnTypeString = "booleantag" });
            // <--[tag]
            // @Name CVarTag.read_only
            // @Group Variables
            // @ReturnType BooleanTag
            // @Returns whether the CVar is read only.
            // -->
            Handlers.Add("read_only", new TagSubHandler() { Handle = (data, obj) => new BooleanTag(((CVarTag)obj).Internal.Flags.HasFlag(CVarFlag.ReadOnly)).Handle(data.Shrink()), ReturnTypeString = "booleantag" });
            // <--[tag]
            // @Name CVarTag.is_boolean
            // @Group Variables
            // @ReturnType BooleanTag
            // @Returns whether the CVar is treated as a boolean by the system.
            // -->
            Handlers.Add("is_boolean", new TagSubHandler() { Handle = (data, obj) => new BooleanTag(((CVarTag)obj).Internal.Flags.HasFlag(CVarFlag.Boolean)).Handle(data.Shrink()), ReturnTypeString = "booleantag" });
            // <--[tag]
            // @Name CVarTag.delayed
            // @Group Variables
            // @ReturnType BooleanTag
            // @Returns whether the CVar has a delayed value read (requires a reload or restart).
            // -->
            Handlers.Add("delayed", new TagSubHandler() { Handle = (data, obj) => new BooleanTag(((CVarTag)obj).Internal.Flags.HasFlag(CVarFlag.Delayed)).Handle(data.Shrink()), ReturnTypeString = "booleantag" });
            // <--[tag]
            // @Name CVarTag.init_only
            // @Group Variables
            // @ReturnType BooleanTag
            // @Returns whether the CVar is only settable before the system has initialized.
            // -->
            Handlers.Add("init_only", new TagSubHandler() { Handle = (data, obj) => new BooleanTag(((CVarTag)obj).Internal.Flags.HasFlag(CVarFlag.InitOnly)).Handle(data.Shrink()), ReturnTypeString = "booleantag" });
            // <--[tag]
            // @Name CVarTag.is_number
            // @Group Variables
            // @ReturnType BooleanTag
            // @Returns whether the CVar is treated as a number by the system.
            // -->
            Handlers.Add("is_number", new TagSubHandler() { Handle = (data, obj) => new BooleanTag(((CVarTag)obj).Internal.Flags.HasFlag(CVarFlag.Numeric)).Handle(data.Shrink()), ReturnTypeString = "booleantag" });
            // <--[tag]
            // @Name CVarTag.is_text
            // @Group Variables
            // @ReturnType BooleanTag
            // @Returns whether the CVar is treated as text by the system.
            // -->
            Handlers.Add("is_text", new TagSubHandler() { Handle = (data, obj) => new BooleanTag(((CVarTag)obj).Internal.Flags.HasFlag(CVarFlag.Textual)).Handle(data.Shrink()), ReturnTypeString = "booleantag" });
            // <--[tag]
            // @Name CVarTag.user_made
            // @Group Variables
            // @ReturnType BooleanTag
            // @Returns whether the CVar was made by a command instead of being internal-use.
            // -->
            Handlers.Add("user_made", new TagSubHandler() { Handle = (data, obj) => new BooleanTag(((CVarTag)obj).Internal.Flags.HasFlag(CVarFlag.UserMade)).Handle(data.Shrink()), ReturnTypeString = "booleantag" });
        }*/
        
        public override string ToString()
        {
            return Internal.Name;
        }
    }
}
