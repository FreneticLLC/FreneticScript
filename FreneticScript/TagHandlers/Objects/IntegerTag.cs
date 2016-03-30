using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FreneticScript.TagHandlers.Objects
{
    /// <summary>
    /// Represents a number as a usable tag.
    /// </summary>
    public class IntegerTag : TemplateObject
    {
        // <--[object]
        // @Type IntegerTag
        // @SubType NumberTag
        // @Group Mathematics
        // @Description Represents an integer.
        // @Other note that the number is internally stored as a 64-bit signed integer (a 'long').
        // -->

        /// <summary>
        /// The integer this IntegerTag represents.
        /// </summary>
        public long Internal;

        /// <summary>
        /// Get an integer tag relevant to the specified input, erroring on the command system if invalid input is given (Returns 0 in that case).
        /// Never null!
        /// </summary>
        /// <param name="dat">The TagData used to construct this IntegerTag.</param>
        /// <param name="input">The input text to create a integer from.</param>
        /// <returns>The integer tag.</returns>
        public static IntegerTag For(TagData dat, string input)
        {
            long tval;
            if (long.TryParse(input, out tval))
            {
                return new IntegerTag(tval);
            }
            if (!dat.HasFallback)
            {
                dat.Error("Invalid integer: '" + TagParser.Escape(input) + "'!");
            }
            return new IntegerTag(0);
        }

        /// <summary>
        /// Get an integer tag relevant to the specified input, erroring on the command system if invalid input is given (Returns 0 in that case).
        /// Never null!
        /// </summary>
        /// <param name="dat">The TagData used to construct this IntegerTag.</param>
        /// <param name="input">The input text to create a integer from.</param>
        /// <returns>The integer tag.</returns>
        public static IntegerTag For(TagData dat, TemplateObject input)
        {
            return input is IntegerTag ? (IntegerTag)input : For(dat, input.ToString());
        }

        /// <summary>
        /// Tries to return a valid integer, or null.
        /// </summary>
        /// <param name="input">The input that is potentially an integer.</param>
        /// <returns>An integer, or null.</returns>
        public static IntegerTag TryFor(string input)
        {
            long tval;
            if (long.TryParse(input, out tval))
            {
                return new IntegerTag(tval);
            }
            return null;
        }

        /// <summary>
        /// Tries to return a valid integer, or null.
        /// </summary>
        /// <param name="input">The input that is potentially an integer.</param>
        /// <returns>An integer, or null.</returns>
        public static IntegerTag TryFor(TemplateObject input)
        {
            if (input == null)
            {
                return null;
            }
            if (input is IntegerTag)
            {
                return (IntegerTag)input;
            }
            return TryFor(input.ToString());
        }

        /// <summary>
        /// Constructs an integer tag.
        /// </summary>
        /// <param name="_val">The internal integer to use.</param>
        public IntegerTag(long _val)
        {
            Internal = _val;
        }

        static Dictionary<string, Func<TagData, IntegerTag, TemplateObject>> Handlers = new Dictionary<string, Func<TagData, IntegerTag, TemplateObject>>();

        static void RegisterTag(string name, Func<TagData, IntegerTag, TemplateObject> method)
        {
            Handlers.Add(name.ToLowerFast(), method);
        }

        static IntegerTag()
        {
            // Documented in NumberTag.
            RegisterTag("is_greater_than", (data, obj) => new BooleanTag(obj.Internal > For(data, data.GetModifierObject(0)).Internal).Handle(data.Shrink()));
            // Documented in NumberTag.
            RegisterTag("is_greater_than_or_equal_to", (data, obj) => new BooleanTag(obj.Internal >= For(data, data.GetModifierObject(0)).Internal).Handle(data.Shrink()));
            // Documented in NumberTag.
            RegisterTag("is_less_than", (data, obj) => new BooleanTag(obj.Internal < For(data, data.GetModifierObject(0)).Internal).Handle(data.Shrink()));
            // Documented in NumberTag.
            RegisterTag("is_less_than_or_equal_to", (data, obj) => new BooleanTag(obj.Internal <= For(data, data.GetModifierObject(0)).Internal).Handle(data.Shrink()));
            // Documented in TextTag.
            RegisterTag("equals", (data, obj) => new BooleanTag(obj.Internal == For(data, data.GetModifierObject(0)).Internal).Handle(data.Shrink()));
            // <--[tag]
            // @Name IntegerTag.add_int[<IntegerTag>]
            // @Group Mathematics
            // @ReturnType IntegerTag
            // @Returns the number plus the specified number.
            // @Other Commonly shortened to "+".
            // @Example "1" .add_int[1] returns "2".
            // -->
            RegisterTag("add_int", (data, obj) => new IntegerTag(obj.Internal + For(data, data.GetModifierObject(0)).Internal).Handle(data.Shrink()));
            // <--[tag]
            // @Name IntegerTag.subtract_int[<IntegerTag>]
            // @Group Mathematics
            // @ReturnType IntegerTag
            // @Returns the number minus the specified number.
            // @Other Commonly shortened to "-".
            // @Example "1" .subtract_int[1] returns "0".
            // -->
            RegisterTag("subtract_int", (data, obj) => new IntegerTag(obj.Internal - For(data, data.GetModifierObject(0)).Internal).Handle(data.Shrink()));
            // <--[tag]
            // @Name IntegerTag.multiply_int[<IntegerTag>]
            // @Group Mathematics
            // @ReturnType IntegerTag
            // @Returns the number multiplied by the specified number.
            // @Other Commonly shortened to "*".
            // @Example "2" .multiply_int[2] returns "4".
            // -->
            RegisterTag("multiply_int", (data, obj) => new IntegerTag(obj.Internal * For(data, data.GetModifierObject(0)).Internal).Handle(data.Shrink()));
            // <--[tag]
            // @Name IntegerTag.divide_int[<IntegerTag>]
            // @Group Mathematics
            // @ReturnType IntegerTag
            // @Returns the number divided by the specified number.
            // @Other Commonly shortened to "/".
            // @Example "4" .divide_int[2] returns "2".
            // -->
            RegisterTag("divide_int", (data, obj) => new IntegerTag(obj.Internal / For(data, data.GetModifierObject(0)).Internal).Handle(data.Shrink()));
            // <--[tag]
            // @Name IntegerTag.modulo_int[<IntegerTag>]
            // @Group Mathematics
            // @ReturnType IntegerTag
            // @Returns the number modulo the specified number.
            // @Other Commonly shortened to "%".
            // @Example "10" .modulo_int[3] returns "1".
            // -->
            RegisterTag("modulo_int", (data, obj) => new IntegerTag(obj.Internal % For(data, data.GetModifierObject(0)).Internal).Handle(data.Shrink()));
            // <--[tag]
            // @Name IntegerTag.absolute_value_int
            // @Group Mathematics
            // @ReturnType IntegerTag
            // @Returns the absolute value of the number.
            // @Example "-1" .absolute_value_int returns "1".
            // -->
            RegisterTag("absolute_value_int", (data, obj) => new IntegerTag(Math.Abs(obj.Internal)).Handle(data.Shrink()));
            // <--[tag]
            // @Name NumberTag.maximum_int[<NumberTag>]
            // @Group Mathematics
            // @ReturnType IntegerTag
            // @Returns whichever is greater: the number or the specified number.
            // @Example "10" .maximum_int[12] returns "12".
            // -->
            RegisterTag("maximum_int", (data, obj) => new IntegerTag(Math.Max(obj.Internal, For(data, data.GetModifierObject(0)).Internal)).Handle(data.Shrink()));
            // <--[tag]
            // @Name IntegerTag.minimum_int[<NumberTag>]
            // @Group Mathematics
            // @ReturnType IntegerTag
            // @Returns whichever is lower: the number or the specified number.
            // @Example "10" .minimum_int[12] returns "10".
            // -->
            RegisterTag("minimum_int", (data, obj) => new IntegerTag(Math.Min(obj.Internal, For(data, data.GetModifierObject(0)).Internal)).Handle(data.Shrink()));
            // Documented in NumberTag.
            RegisterTag("sign", (data, obj) => new IntegerTag(Math.Sign(obj.Internal)).Handle(data.Shrink()));
            // <--[tag]
            // @Name IntegerTag.to_binary
            // @Group Mathematics
            // @ReturnType BinaryTag
            // @Returns a binary representation of this integer.
            // @Example "1" .to_binary returns "0100000000000000".
            // -->
            RegisterTag("to_binary", (data, obj) => new BinaryTag(BitConverter.GetBytes(obj.Internal)).Handle(data.Shrink()));
            // Documented in TextTag.
            RegisterTag("to_integer", (data, obj) => obj.Handle(data.Shrink()));
            // Documented in TextTag.
            RegisterTag("to_number", (data, obj) => new NumberTag(obj.Internal).Handle(data.Shrink()));
            // Documented in TextTag.
            RegisterTag("duplicate", (data, obj) => new IntegerTag(obj.Internal).Handle(data.Shrink()));
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
            Func<TagData, IntegerTag, TemplateObject> handler;
            if (Handlers.TryGetValue(data[0], out handler))
            {
                return handler.Invoke(data, this);
            }
            return new NumberTag(Internal).Handle(data); // TODO: is this best? Perhaps a BigDecimal type of tag?
        }

        /// <summary>
        /// Returns the a string representation of the integer internally stored by this integer tag. EG, could return "0", or "1", or...
        /// </summary>
        /// <returns>A string representation of the integer.</returns>
        public override string ToString()
        {
            return Internal.ToString();
        }

        /// <summary>
        /// Sets a value on the object.
        /// </summary>
        /// <param name="names">The name of the value.</param>
        /// <param name="val">The value to set it to.</param>
        public override void Set(string[] names, TemplateObject val)
        {
            if (names == null || names.Length == 0)
            {
                Internal = TryFor(val).Internal;
                return;
            }
            base.Set(names, val);
        }

        /// <summary>
        /// Adds a value to a value on the object.
        /// </summary>
        /// <param name="names">The name of the value.</param>
        /// <param name="val">The value to add.</param>
        public override void Add(string[] names, TemplateObject val)
        {
            if (names == null || names.Length == 0)
            {
                Internal += TryFor(val).Internal;
                return;
            }
            base.Add(names, val);
        }

        /// <summary>
        /// Subtracts a value from a value on the object.
        /// </summary>
        /// <param name="names">The name of the value.</param>
        /// <param name="val">The value to subtract.</param>
        public override void Subtract(string[] names, TemplateObject val)
        {
            if (names == null || names.Length == 0)
            {
                Internal -= TryFor(val).Internal;
                return;
            }
            base.Subtract(names, val);
        }

        /// <summary>
        /// Multiplies a value by a value on the object.
        /// </summary>
        /// <param name="names">The name of the value.</param>
        /// <param name="val">The value to multiply.</param>
        public override void Multiply(string[] names, TemplateObject val)
        {
            if (names == null || names.Length == 0)
            {
                Internal *= TryFor(val).Internal;
                return;
            }
            base.Multiply(names, val);
        }

        /// <summary>
        /// Divides a value from a value on the object.
        /// </summary>
        /// <param name="names">The name of the value.</param>
        /// <param name="val">The value to divide.</param>
        public override void Divide(string[] names, TemplateObject val)
        {
            if (names == null || names.Length == 0)
            {
                Internal /= TryFor(val).Internal;
                return;
            }
            base.Divide(names, val);
        }
    }
}
