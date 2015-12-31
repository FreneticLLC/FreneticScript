using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Frenetic.TagHandlers.Objects;

namespace Frenetic.TagHandlers.Common
{
    /// <summary>
    /// Returns CVar information.
    /// </summary>
    public class CVarTagBase : TemplateTagBase
    {
        // <--[tagbase]
        // @Base cvar[<TextTag>]
        // @Group Variables
        // @ReturnType CVarTag
        // @Returns the specified global control variable.
        // <@link explanation cvars>What are CVars?<@/link>
        // -->

        /// <summary>
        /// Construct the CVarTags - for internal use only.
        /// </summary>
        public CVarTagBase()
        {
            Name = "cvar";
        }

        /// <summary>
        /// Handles a 'cvar' tag.
        /// </summary>
        /// <param name="data">The data to be handled.</param>
        public override TemplateObject Handle(TagData data)
        {
            string modif = data.GetModifier(0).ToLower();
            CVar cvar = data.TagSystem.CommandSystem.Output.CVarSys.Get(modif);
            if (cvar != null)
            {
                data.Shrink();
                if (data.Input.Count == 0)
                {
                    return new TextTag(cvar.Value);
                }
                if (data.Input.Count > 0 && data.Input[0] == "exists")
                {
                    return new BooleanTag(false).Handle(data.Shrink());
                }
                // TODO: Separate CVar object?
                switch (data.Input[0])
                {
                    // <--[tag]
                    // @Name CVarTag.exists
                    // @Group Variables
                    // @ReturnType BooleanTag
                    // @Returns whether the specified CVar exists.
                    // Specifically for the tag <@link tag cvar[<TextTag>]><{cvar[<TextTag>]}><@/link>.
                    // -->
                    case "exists":
                        return new BooleanTag(true).Handle(data.Shrink());
                    // <--[tag]
                    // @Name CVarTag.value
                    // @Group Variables
                    // @ReturnType TextTag
                    // @Returns the value of the CVar, as plain text.
                    // -->
                    case "value":
                        return new TextTag(cvar.Value).Handle(data.Shrink());
                    // <--[tag]
                    // @Name CVarTag.value_boolean
                    // @Group Variables
                    // @ReturnType BooleanTag
                    // @Returns whether the CVar is marked 'true'.
                    // -->
                    case "value_boolean":
                        return new BooleanTag(cvar.ValueB).Handle(data.Shrink());
                    // <--[tag]
                    // @Name CVarTag.value_decimal
                    // @Group Variables
                    // @ReturnType NumberTag
                    // @Returns the decimal number value of the CVar.
                    // -->
                    case "value_decimal":
                        return new NumberTag(cvar.ValueD).Handle(data.Shrink());
                    // <--[tag]
                    // @Name CVarTag.value_number
                    // @Group Variables
                    // @ReturnType NumberTag
                    // @Returns the integer number value of the CVar.
                    // @TODO make an integer-typed tag?
                    // -->
                    case "value_number":
                        return new NumberTag(cvar.ValueL).Handle(data.Shrink());
                    // <--[tag]
                    // @Name CVarTag.name
                    // @Group Variables
                    // @ReturnType TextTag
                    // @Returns the the name of the CVar.
                    // -->
                    case "name":
                        return new TextTag(cvar.Name).Handle(data.Shrink());
                    // <--[tag]
                    // @Name CVarTag.info
                    // @Group Variables
                    // @ReturnType TextTag
                    // @Returns the full <@link command cvarinfo>cvarinfo<@/link> output of the CVar.
                    // -->
                    case "info":
                        return new TextTag(cvar.Info()).Handle(data.Shrink());
                    // <--[tag]
                    // @Name CVarTag.server_controlled
                    // @Group Variables
                    // @Mode Client
                    // @ReturnType BooleanTag
                    // @Returns whether the CVar is server controlled only.
                    // -->
                    case "server_controlled":
                        return new BooleanTag(cvar.Flags.HasFlag(CVarFlag.ServerControl)).Handle(data.Shrink());
                    // <--[tag]
                    // @Name CVarTag.read_only
                    // @Group Variables
                    // @ReturnType BooleanTag
                    // @Returns whether the CVar is read only.
                    // -->
                    case "read_only":
                        return new BooleanTag(cvar.Flags.HasFlag(CVarFlag.ReadOnly)).Handle(data.Shrink());
                    // <--[tag]
                    // @Name CVarTag.is_boolean
                    // @Group Variables
                    // @ReturnType BooleanTag
                    // @Returns whether the CVar is treated as a boolean by the system.
                    // -->
                    case "is_boolean":
                        return new BooleanTag(cvar.Flags.HasFlag(CVarFlag.Boolean)).Handle(data.Shrink());
                    // <--[tag]
                    // @Name CVarTag.delayed
                    // @Group Variables
                    // @ReturnType BooleanTag
                    // @Returns whether the CVar has a delayed value read (requires a reload or restart).
                    // -->
                    case "delayed":
                        return new BooleanTag(cvar.Flags.HasFlag(CVarFlag.Delayed)).Handle(data.Shrink());
                    // <--[tag]
                    // @Name CVarTag.init_only
                    // @Group Variables
                    // @ReturnType BooleanTag
                    // @Returns whether the CVar is only settable before the system has initialized.
                    // -->
                    case "init_only":
                        return new BooleanTag(cvar.Flags.HasFlag(CVarFlag.InitOnly)).Handle(data.Shrink());
                    // <--[tag]
                    // @Name CVarTag.is_number
                    // @Group Variables
                    // @ReturnType BooleanTag
                    // @Returns whether the CVar is treated as a number by the system.
                    // -->
                    case "numeric":
                        return new BooleanTag(cvar.Flags.HasFlag(CVarFlag.Numeric)).Handle(data.Shrink());
                    // <--[tag]
                    // @Name CVarTag.is_text
                    // @Group Variables
                    // @ReturnType BooleanTag
                    // @Returns whether the CVar is treated as text by the system.
                    // -->
                    case "is_text":
                        return new BooleanTag(cvar.Flags.HasFlag(CVarFlag.Textual)).Handle(data.Shrink());
                    // <--[tag]
                    // @Name CVarTag.user_made
                    // @Group Variables
                    // @ReturnType BooleanTag
                    // @Returns whether the CVar was made by a command instead of being internal-use.
                    // -->
                    case "user_made":
                        return new BooleanTag(cvar.Flags.HasFlag(CVarFlag.UserMade)).Handle(data.Shrink());
                    default:
                        return new TextTag(cvar.Value).Handle(data);
                }
            }
            if (data.Input.Count > 0 && data.Input[0] == "exists")
            {
                return new BooleanTag(false).Handle(data.Shrink());
            }
            else
            {
                // TODO: ??
                return new TextTag("").Handle(data);
            }
        }
    }
}
