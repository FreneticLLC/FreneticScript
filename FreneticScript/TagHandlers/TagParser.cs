using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers.Common;
using FreneticScript.CommandSystem;
using FreneticScript.CommandSystem.Arguments;
using FreneticScript.TagHandlers.Objects;

namespace FreneticScript.TagHandlers
{
    /// <summary>
    /// The master class for parsing tags.
    /// </summary>
    public class TagParser
    {
        // <--[definition]
        // @Word specified
        // @Group tag
        // @Description The word 'specified', when used in a tag description, refers to the value input after the tag.
        // EG, you might have the tag <@link tag TextTag.equals[<TextTag>]><{text[<TextTag>].equals[<TextTag>]}><@/link>
        // In this tag, the first <TextTag> is referred to as 'the text', and the second <TextTag> as 'specified text'.
        // Which would look like: <{text[the text].equals[specified text]}>.
        // -->

        /// <summary>
        /// The command system that made this tag system.
        /// </summary>
        public Commands CommandSystem;

        /// <summary>
        /// Escapes any tags inside a string.
        /// </summary>
        /// <param name="input">The string that may have tags.</param>
        /// <returns>An escaped string.</returns>
        public static string Escape(string input)
        {
            if (input == null)
            {
                return "null";
            }
            return input.Replace("<{", "\0TAGSTART").Replace("}>", "\0TAGEND");
        }

        /// <summary>
        /// Reverses any tag escaping inside a string.
        /// </summary>
        /// <param name="input">The string that was escaped.</param>
        /// <returns>An unescaped string that may have tags.</returns>
        public static string Unescape(string input)
        {
            if (input == null)
            {
                return "null";
            }
            return input.Replace("\0TAGSTART", "<{").Replace("\0TAGEND", "}>");
        }

        /// <summary>
        /// All tag handler objects currently registered.
        /// </summary>
        public Dictionary<string, TemplateTagBase> Handlers = new Dictionary<string, TemplateTagBase>();

        /// <summary>
        /// Registers a handler object for later usage by tags.
        /// </summary>
        /// <param name="handler">The handler object to register.</param>
        public void Register(TemplateTagBase handler)
        {
            Handlers.Add(handler.Name, handler);
        }

        /// <summary>
        /// Prepares the tag system.
        /// </summary>
        public void Init(Commands _system)
        {
            CommandSystem = _system;
            Register(new BinaryTagBase());
            Register(new BooleanTagBase());
            Register(new CVarTagBase());
            Register(new EscapeTagBase());
            Register(new IntegerTagBase());
            Register(new ListTagBase());
            Register(new MapTagBase());
            Register(new NumberTagBase());
            Register(new TernaryTagBase());
            Register(new TextColorTags());
            Register(new TextTagBase());
            Register(new UnescapeTagBase());
            Register(new UtilTagBase());
            Register(new VarTagBase());
        }

        static char[] dot = new char[] { '.' };

        /// <summary>
        /// Splits text into an Argument, for preparsing.
        /// </summary>
        /// <param name="input">The original text.</param>
        /// <param name="wasquoted">Whether the argument was input with "quotes".</param>
        /// <returns>The parsed Argument.</returns>
        public Argument SplitToArgument(string input, bool wasquoted)
        {
            if (input.Length == 0)
            {
                return new Argument();
            }
            if (input.IndexOf("<{") < 0)
            {
                Argument a = new Argument();
                a.WasQuoted = wasquoted;
                a.Bits.Add(new TextArgumentBit(input, wasquoted) { CommandSystem = CommandSystem });
                return a;
            }
            Argument arg = new Argument();
            arg.WasQuoted = wasquoted;
            int len = input.Length;
            int blocks = 0;
            int brackets = 0;
            StringBuilder blockbuilder = new StringBuilder();
            StringBuilder tbuilder = new StringBuilder();
            for (int i = 0; i < len; i++)
            {
                if (i + 1 < len && input[i] == '<' && input[i + 1] == '{')
                {
                    blocks++;
                    if (blocks == 1)
                    {
                        i++;
                        continue;
                    }
                }
                else if (i + 1 < len && input[i] == '}' && input[i + 1] == '>')
                {
                    blocks--;
                    if (blocks == 0)
                    {
                        if (tbuilder.Length > 0)
                        {
                            arg.Bits.Add(new TextArgumentBit(tbuilder.ToString(), wasquoted) { CommandSystem = CommandSystem });
                            tbuilder = new StringBuilder();
                        }
                        string value = blockbuilder.ToString();
                        string fallback = null;
                        int brack = 0;
                        for (int fb = 1; fb < value.Length; fb++)
                        {
                            if (value[fb] == '[')
                            {
                                brack++;
                            }
                            if (value[fb] == ']')
                            {
                                brack--;
                            }
                            if (brack == 0 && value[fb] == '|' && value[fb - 1] == '|')
                            {
                                fallback = value.Substring(fb + 1);
                                value = value.Substring(0, fb);
                                break;
                            }
                        }
                        List<string> split = value.Split(dot).ToList();
                        for (int s = 0; s < split.Count; s++)
                        {
                            split[s] = split[s].Replace("&dot", ".").Replace("&amp", "&");
                        }
                        List<TagBit> bits = new List<TagBit>();
                        for (int x = 0; x < split.Count; x++)
                        {
                            TagBit bit = new TagBit();
                            if (split[x].Length > 1 && split[x].Contains('[') && split[x][split[x].Length - 1] == ']')
                            {
                                int index = split[x].IndexOf('[');
                                bit.Variable = SplitToArgument(split[x].Substring(index + 1, split[x].Length - (index + 2)), wasquoted);
                                split[x] = split[x].Substring(0, index).ToLowerInvariant();
                            }
                            else
                            {
                                split[x] = split[x].ToLowerInvariant();
                                bit.Variable = new Argument();
                            }
                            bit.Key = split[x];
                            bits.Add(bit);
                        }
                        TagArgumentBit tab = new TagArgumentBit(CommandSystem, bits);
                        tab.Fallback = fallback == null ? null : SplitToArgument(fallback, false);
                        arg.Bits.Add(tab);
                        blockbuilder = new StringBuilder();
                        i++;
                        continue;
                    }
                }
                else if (blocks == 1 && input[i] == '[')
                {
                    brackets++;
                }
                else if (blocks == 1 && input[i] == ']')
                {
                    brackets--;
                }
                if (blocks > 0)
                {
                    switch (input[i])
                    {
                        case '.':
                            if (blocks > 1 || brackets > 0)
                            {
                                blockbuilder.Append("&dot");
                            }
                            else
                            {
                                blockbuilder.Append(".");
                            }
                            break;
                        case '&':
                            blockbuilder.Append("&amp");
                            break;
                        default:
                            blockbuilder.Append(input[i]);
                            break;
                    }
                }
                else
                {
                    tbuilder.Append(input[i]);
                }
            }
            if (tbuilder.Length > 0)
            {
                arg.Bits.Add(new TextArgumentBit(tbuilder.ToString(), wasquoted) { CommandSystem = CommandSystem });
            }
            return arg;
        }

        /// <summary>
        /// Reads and parses all tags inside a list of tag bits.
        /// </summary>
        /// <param name="base_color">The base color for tags to use.</param>
        /// <param name="vars">Any variables in this tag's context.</param>
        /// <param name="bits">The tag data.</param>
        /// <param name="mode">What debugmode to use.</param>
        /// <param name="error">What to invoke if there's an error.</param>
        /// <param name="starter">A preparsed start location.</param>
        /// <returns>The string with tags parsed.</returns>
        public TemplateObject ParseTags(TagArgumentBit bits, string base_color, Dictionary<string, TemplateObject> vars, DebugMode mode, Action<string> error, TemplateTagBase starter)
        {
            if (bits.Bits.Count == 0)
            {
                return new TextTag("");
            }
            TagData data = new TagData(this, bits.Bits, base_color, vars, mode, error, bits.Fallback);
            TemplateTagBase handler = starter;
            try
            {
                if (handler != null || Handlers.TryGetValue(data[0], out handler))
                {
                    TemplateObject res = handler.Handle(data) ?? new TextTag("");
                    if (mode <= DebugMode.FULL)
                    {
                        CommandSystem.Output.Good("Filled tag " + TextStyle.Color_Separate +
                            Escape(bits.ToString()) + TextStyle.Color_Outgood + " with \"" + TextStyle.Color_Separate + Escape(res.ToString())
                            + TextStyle.Color_Outgood + "\".", mode);
                    }
                    return res;
                }
                else if (data.HasFallback)
                {
                    return data.Fallback.Parse(base_color, vars, mode, error);
                }
                else
                {
                    if (mode <= DebugMode.MINIMAL)
                    {
                        error("Failed to fill tag tag " + Escape(bits.ToString()) + ": nonexistant tag handler!");
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                if (ex is ErrorInducedException)
                {
                    throw ex;
                }
                error("Failed to fill tag tag " + Escape(bits.ToString()) + ": " + ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// Reads and parses all tags inside a string.
        /// </summary>
        /// <param name="base_color">The base color for tags to use.</param>
        /// <param name="vars">Any variables in this tag's context.</param>
        /// <param name="input">The tagged string.</param>
        /// <param name="mode">What debugmode to use.</param>
        /// <param name="error">What to invoke if there's an error.</param>
        /// <param name="wasquoted">Whether the input had "quotes".</param>
        /// <returns>The string with tags parsed.</returns>
        public string ParseTagsFromText(string input, string base_color, Dictionary<string, TemplateObject> vars, DebugMode mode, Action<string> error, bool wasquoted)
        {
            return Unescape((SplitToArgument(input, wasquoted).Parse(base_color, vars, mode, error) ?? new TextTag("&{NULL}")).ToString());
        }
    }
}
