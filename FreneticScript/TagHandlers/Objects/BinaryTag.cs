using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace FreneticScript.TagHandlers.Objects
{
    /// <summary>
    /// Represents binary data.
    /// </summary>
    public class BinaryTag : TemplateObject
    {
        // <--[object]
        // @Type BinaryTag
        // @SubType TextTag
        // @Group Mathematics
        // @Description Represents binary data.
        // -->

        /// <summary>
        /// The binary data this tag represents.
        /// </summary>
        public byte[] Internal;

        /// <summary>
        /// Get a boolean tag relevant to the specified input, erroring on the command system if invalid input is given (Returns false in that case).
        /// Never null!
        /// </summary>
        /// <param name="dat">The TagData used to construct this BooleanTag.</param>
        /// <param name="input">The input text to create a boolean from.</param>
        /// <returns>The boolean tag.</returns>
        public static BinaryTag For(TagData dat, string input)
        {
            string low = input.ToLower();
            try
            {
                return new BinaryTag(StringToBytes(input));
            }
            catch (Exception ex)
            {
                if (ex is ThreadAbortException)
                {
                    throw ex;
                }
                if (!dat.HasFallback)
                {
                    dat.TagSystem.CommandSystem.Output.WriteLine(ex.ToString());
                    dat.Error("Invalid binary data: '" + TagParser.Escape(input) + "'!");
                }
                return null;
            }
        }

        static byte[] StringToBytes(string hex)
        {
            int l = hex.Length >> 1;
            byte[] arr = new byte[l];
            for (int i = 0; i < l; i++)
            {
                arr[i] = (byte)((GetHexVal(hex[i << 1]) << 4) + (GetHexVal(hex[(i << 1) + 1])));
            }
            return arr;
        }

        static int GetHexVal(char val)
        {
            return val - (val < 58 ? 48 : (val < 97 ? 55 : 87));
        }

        /// <summary>
        /// Get a binary tag relevant to the specified input, erroring on the command system if invalid input is given (Returns null in that case).
        /// </summary>
        /// <param name="dat">The TagData used to construct this BooleanTag.</param>
        /// <param name="input">The input to create or get a boolean from.</param>
        /// <returns>The boolean tag.</returns>
        public static BinaryTag For(TagData dat, TemplateObject input)
        {
            return input is BinaryTag ? (BinaryTag)input : For(dat, input.ToString());
        }

        /// <summary>
        /// Constructs a binary tag.
        /// </summary>
        /// <param name="_val">The internal binary data to use.</param>
        public BinaryTag(byte[] _val)
        {
            Internal = _val;
        }

        /// <summary>
        /// Parse any direct tag input values.
        /// </summary>
        /// <param name="data">The input tag data.</param>
        public override TemplateObject Handle(TagData data)
        {
            if (data.Input.Count == 0)
            {
                return this;
            }
            switch (data.Input[0])
            {
                // <--[tag]
                // @Name BooleanTag.byte_at[<IntegerTag>]
                // @Group Binary Attributes
                // @ReturnType IntegerTag
                // @Returns the integer version of the byte at a specific 1-based index.
                // @Example "010203" .byte_at[1] returns "1".
                // -->
                case "byte_at":
                    {
                        int ind = (int)IntegerTag.For(data, data.GetModifier(0)).Internal;
                        if (ind < 1 || ind > Internal.Length)
                        {
                            if (!data.HasFallback)
                            {
                                data.Error("Invalid byte_at tag: " + ind + " is not in the exclusive range of 1 to " + Internal.Length);
                            }
                            return new NullTag();
                        }
                        return new IntegerTag(Internal[ind - 1]).Handle(data.Shrink());
                    }
                // <--[tag]
                // @Name BooleanTag.byte_list
                // @Group Binary Attributes
                // @ReturnType ListTag<IntegerTag>
                // @Returns a list of integer versions of the bytes in this binary tag.
                // @Example "010203" .byte_list returns "1|2|3".
                // -->
                case "byte_list":
                    {
                        List<TemplateObject> objs = new List<TemplateObject>(Internal.Length);
                        for (int i = 0; i < Internal.Length; i++)
                        {
                            objs.Add(new IntegerTag(Internal[i]));
                        }
                        return new ListTag(objs).Handle(data.Shrink());
                    }
                // <--[tag]
                // @Name BooleanTag.to_integer
                // @Group Binary Attributes
                // @ReturnType IntegerTag
                // @Returns the internal data converted to an integer value.
                // @Note currently must be of length: 1, 2, 4, or 8 bytes.
                // @Example "0100000000000000" .to_integer returns "1".
                // -->
                case "to_integer":
                    {
                        switch (Internal.Length)
                        {
                            case 1:
                                return new IntegerTag(Internal[0]).Handle(data.Shrink());
                            case 2:
                                return new IntegerTag(BitConverter.ToInt16(Internal, 0)).Handle(data.Shrink());
                            case 4:
                                return new IntegerTag(BitConverter.ToInt32(Internal, 0)).Handle(data.Shrink());
                            case 8:
                                return new IntegerTag(BitConverter.ToInt64(Internal, 0)).Handle(data.Shrink());
                            default:
                                if (data.HasFallback)
                                {
                                    data.Error("Invalid to_integer binary data length: " + Internal.Length);
                                }
                                return new NullTag();
                        }
                    }
                // <--[tag]
                // @Name BooleanTag.to_number
                // @Group Binary Attributes
                // @ReturnType NumberTag
                // @Returns the internal data converted to an floating-point number value.
                // @Note currently must be of length: 4, or 8 bytes.
                // @Example "000000000000F03F" .to_number returns "1".
                // -->
                case "to_number":
                    {
                        switch (Internal.Length)
                        {
                            case 4:
                                return new NumberTag(BitConverter.ToSingle(Internal, 0)).Handle(data.Shrink());
                            case 8:
                                return new NumberTag(BitConverter.ToDouble(Internal, 0)).Handle(data.Shrink());
                            default:
                                if (data.HasFallback)
                                {
                                    data.Error("Invalid to_number binary data length: " + Internal.Length);
                                }
                                return new NullTag();
                        }
                    }
                default:
                    return new TextTag(ToString()).Handle(data);
            }
        }

        /// <summary>
        /// Returns the a string representation of the binary data internally stored by this boolean tag.
        /// This returns in hexadecimal format.
        /// </summary>
        /// <returns>A string representation of the binary data.</returns>
        public override string ToString()
        {
            if (Internal == null)
            {
                return "";
            }
            // TODO: Efficiency?
            return BitConverter.ToString(Internal).Replace("-", "");
        }
    }
}
