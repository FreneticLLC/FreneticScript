using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FreneticScript.TagHandlers.Objects
{
    /// <summary>
    /// Represents a true or false as a usable tag.
    /// </summary>
    public class BooleanTag : TemplateObject
    {
        // <--[object]
        // @Type BooleanTag
        // @SubType TextTag
        // @Group Mathematics
        // @Description Represents a 'true' or 'false'.
        // -->

        /// <summary>
        /// The boolean this tag represents.
        /// </summary>
        public bool Internal;

        /// <summary>
        /// Get a boolean tag relevant to the specified input, erroring on the command system if invalid input is given (Returns false in that case).
        /// Never null!
        /// </summary>
        /// <param name="dat">The TagData used to construct this BooleanTag.</param>
        /// <param name="input">The input text to create a boolean from.</param>
        /// <returns>The boolean tag.</returns>
        public static BooleanTag For(TagData dat, string input)
        {
            string low = input.ToLowerFast();
            if (low == "true")
            {
                return new BooleanTag(true);
            }
            if (low == "false")
            {
                return new BooleanTag(false);
            }
            if (!dat.HasFallback)
            {
                dat.Error("Invalid boolean: '" + TagParser.Escape(input) + "'!");
            }
            return new BooleanTag(false);
        }

        /// <summary>
        /// Get a boolean tag relevant to the specified input, erroring on the command system if invalid input is given (Returns false in that case).
        /// Never null!
        /// </summary>
        /// <param name="dat">The TagData used to construct this BooleanTag.</param>
        /// <param name="input">The input to create or get a boolean from.</param>
        /// <returns>The boolean tag.</returns>
        public static BooleanTag For(TagData dat, TemplateObject input)
        {
            return input is BooleanTag ? (BooleanTag)input : For(dat, input.ToString());
        }

        /// <summary>
        /// Tries to return a valid boolean, or null.
        /// </summary>
        /// <param name="input">The input that is potentially a boolean.</param>
        /// <returns>A boolean, or null.</returns>
        public static BooleanTag TryFor(string input)
        {
            string low = input.ToLowerFast();
            if (low == "true")
            {
                return new BooleanTag(true);
            }
            if (low == "false")
            {
                return new BooleanTag(false);
            }
            return null;
        }

        /// <summary>
        /// Tries to return a valid boolean, or null.
        /// </summary>
        /// <param name="input">The input that is potentially a boolean.</param>
        /// <returns>A boolean, or null.</returns>
        public static BooleanTag TryFor(TemplateObject input)
        {
            if (input == null)
            {
                return null;
            }
            if (input is BooleanTag)
            {
                return (BooleanTag)input;
            }
            return TryFor(input.ToString());
        }

        /// <summary>
        /// Constructs a boolean tag.
        /// </summary>
        /// <param name="_val">The internal boolean to use.</param>
        public BooleanTag(bool _val)
        {
            Internal = _val;
        }

        /// <summary>
        /// Parse any direct tag input values.
        /// </summary>
        /// <param name="data">The input tag data.</param>
        public override TemplateObject Handle(TagData data)
        {
            if (data.Remaining == 0)
            {
                return this;
            }
            switch (data[0])
            {
                // <--[tag]
                // @Name BooleanTag.not
                // @Group Boolean Logic
                // @ReturnType BooleanTag
                // @Returns the opposite of the tag - true and false are flipped.
                // @Example "true" .not returns "false".
                // @Example "false" .not returns "true".
                // -->
                case "not":
                    return new BooleanTag(!Internal).Handle(data.Shrink());
                // <--[tag]
                // @Name BooleanTag.and[<BooleanTag>]
                // @Group Boolean Logic
                // @ReturnType BooleanTag
                // @Returns whether the boolean and the specified text are both true.
                // @Example "true" .and[true] returns "true".
                // @Example "true" .and[false] returns "false".
                // @Example "false" .and[true] returns "false".
                // -->
                case "and":
                    return new BooleanTag(Internal && For(data, data.GetModifierObject(0)).Internal).Handle(data.Shrink());
                // <--[tag]
                // @Name BooleanTag.or[<BooleanTag>]
                // @Group Boolean Logic
                // @ReturnType BooleanTag
                // @Returns whether the boolean or the specified text are true.
                // @Example "true" .or[true] returns "true".
                // @Example "true" .or[false] returns "true".
                // @Example "false" .or[false] returns "false".
                // -->
                case "or":
                    return new BooleanTag(Internal | For(data, data.GetModifierObject(0)).Internal).Handle(data.Shrink());
                // <--[tag]
                // @Name BooleanTag.xor[<BooleanTag>]
                // @Group Boolean Logic
                // @ReturnType BooleanTag
                // @Returns whether the boolean exclusive-or the specified text are true. Meaning, exactly one of the two must be true, and the other false.
                // @Example "true" .xor[true] returns "false".
                // @Example "true" .xor[fa,se] returns "true".
                // -->
                case "xor":
                    return new BooleanTag(Internal != For(data, data.GetModifierObject(0)).Internal).Handle(data.Shrink());
                default:
                    return new TextTag(ToString()).Handle(data);
            }
        }

        /// <summary>
        /// Returns the a string representation of the boolean internally stored by this boolean tag. IE, this returns "true" or "false".
        /// </summary>
        /// <returns>A string representation of the boolean.</returns>
        public override string ToString()
        {
            return Internal ? "true" : "false";
        }
    }
}
