using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers.Objects;

namespace FreneticScript.TagHandlers.Common
{
    /// <summary>
    /// Handles Ternary calculations.
    /// </summary>
    public class TernaryTagBase : TemplateTagBase
    {
        // <--[tagbase]
        // @Base ternary[<BooleanTag>]
        // @Group Text Comparison
        // @ReturnType TernaryPassTag
        // @Returns the specified pass or fail value.
        // The full tag is formatted as <{ternary[<TextTag>].pass[<TextTag>].fail[<TextTag>]}>
        // Using <@link tag TernaryPassTag.pass[<TextTag>]>Pass<@/link> and <@link tag TernaryFailTag.fail[<TextTag>]>fail<@/link> sub-tags.
        // If the value in ternary[...] is "true", then the contents of .pass[...] will be returned.
        // Otherwise, the contents of .fail[...] will be returned.
        // -->

        /// <summary>
        /// Construct the TernaryTags - for internal use only.
        /// </summary>
        public TernaryTagBase()
        {
            Name = "ternary";
        }

        /// <summary>
        /// Handles the 'ternary' tag.
        /// </summary>
        /// <param name="data">The data to be handled.</param>
        public override TemplateObject Handle(TagData data)
        {
            bool basevalue = data.GetModifier(0).ToLower() == "true";
            data.Shrink();
            if (data.Input.Count == 0)
            {
                data.Error("Invalid ternary tag!");
                return new TextTag("&{NULL}");
            }
            // <--[tag]
            // @Name TernaryPassTag.pass[<TextTag>]
            // @Group Text Comparison
            // @ReturnType TernaryFailTag
            // @Returns a step in the ternary pass/fail tag.
            // Used as a part of the <@link tag Ternary[<TextTag>]>Ternary<@/link> tag.
            // -->
            if (data.Input[0] != "pass")
            {
                data.Error("Invalid ternary tag!");
                return new TextTag("&{NULL}");
            }
            string result = "";
            if (basevalue)
            {
                result = data.GetModifier(0);
            }
            data.Shrink();
            if (data.Input.Count == 0)
            {
                data.Error("Invalid ternary tag!");
                return new TextTag("&{NULL}");
            }
            // <--[tag]
            // @Name TernaryFailTag.fail[<TextTag>]
            // @Group Text Comparison
            // @ReturnType TextTag
            // @Returns a step in the ternary pass/fail tag.
            // Used as a part of the <@link tag Ternary[<TextTag>]>Ternary<@/link> tag.
            // -->
            if (data.Input[0] != "fail")
            {
                data.Error("Invalid ternary tag!");
                return new TextTag("&{NULL}");
            }
            if (!basevalue)
            {
                result = data.GetModifier(0);
            }
            return new TextTag(result).Handle(data.Shrink());
        }
    }
}
