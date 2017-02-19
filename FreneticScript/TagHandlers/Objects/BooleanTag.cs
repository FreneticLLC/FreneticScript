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
        public static BooleanTag For(TemplateObject input, TagData dat)
        {
            return input as BooleanTag ?? For(dat, input.ToString());
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
        /// The BooleanTag type.
        /// </summary>
        public const string TYPE = "booleantag";

        /// <summary>
        /// Creates a BooleanTag for the given input data.
        /// </summary>
        /// <param name="dat">The tag data.</param>
        /// <param name="input">The text input.</param>
        /// <returns>A valid boolean tag.</returns>
        public static BooleanTag CreateFor(TemplateObject input, TagData dat)
        {
            BooleanTag conv = input as BooleanTag;
            if (conv != null)
            {
                return conv;
            }
            DynamicTag dynamic = input as DynamicTag;
            if (dynamic != null)
            {
                return CreateFor(dynamic.Internal, dat);
            }
            return For(dat, input.ToString());
        }

#pragma warning disable 1591

        [TagMeta(TagType = TYPE, Name = "not", Group = "Boolean Logic", ReturnType = TYPE, Returns = "The opposite of the tag - true and false are flipped.",
            Examples = new string[] { "'true' .not returns 'false'.", "'false' .not returns 'true'." })]
        public static TemplateObject Tag_Not(TemplateObject obj, TagData data)
        {
            return new BooleanTag(!(obj as BooleanTag).Internal);
        }

        [TagMeta(TagType = TYPE, Name = "and", Group = "Boolean Logic", ReturnType = TYPE, Returns = "Whether the boolean and the specified text are both true.",
            Examples = new string[] { "'true' .and[true] returns 'true'.", "'true' .and[false] returns 'false'.", "'false' .and[true] returns 'false'." })]
        public static TemplateObject Tag_And(TemplateObject obj, TagData data)
        {
            return new BooleanTag((obj as BooleanTag).Internal && For(data.GetModifierObject(0), data).Internal);
        }

        [TagMeta(TagType = TYPE, Name = "or", Group = "Boolean Logic", ReturnType = TYPE, Returns = "Whether the boolean or the specified text are true.",
            Examples = new string[] { "'true' .or[true] returns 'true'.", "'true' .or[false] returns 'true'.", "'false' .or[false] returns 'false'." })]
        public static TemplateObject Tag_Or(TemplateObject obj, TagData data)
        {
            return new BooleanTag((obj as BooleanTag).Internal || For(data.GetModifierObject(0), data).Internal);
        }

        [TagMeta(TagType = TYPE, Name = "xor", Group = "Boolean Logic", ReturnType = TYPE, Returns = "Whether the boolean exclusive-or the specified text are true. Meaning, exactly one of the two must be true, and the other false.",
            Examples = new string[] { "'true' .xor[true] returns 'false'.", "'true' .xor[false] returns 'true.'" })]
        public static TemplateObject Tag_Xor(TemplateObject obj, TagData data)
        {
            return new BooleanTag((obj as BooleanTag).Internal != For(data.GetModifierObject(0), data).Internal);
        }

        [TagMeta(TagType = TYPE, Name = "duplicate", Group = "Tag System", ReturnType = TYPE, Returns = "A perfect duplicate of this object.",
            Examples = new string[] { "'true' .duplicate returns 'true'." })]
        public static TemplateObject Tag_Duplicate(TemplateObject obj, TagData data)
        {
            return new BooleanTag((obj as BooleanTag).Internal);
        }

        [TagMeta(TagType = TYPE, Name = "type", Group = "Tag System", ReturnType = TagTypeTag.TYPE, Returns = "The type of the tag (BooleanTag).",
            Examples = new string[] { "'true' .type returns 'booleantag'." })]
        public static TemplateObject Tag_Type(TemplateObject obj, TagData data)
        {
            return new TagTypeTag(data.TagSystem.Type_Boolean);
        }

#pragma warning restore 1591
        
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
