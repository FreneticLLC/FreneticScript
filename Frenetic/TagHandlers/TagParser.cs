using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Frenetic.TagHandlers.Common;
using Frenetic.CommandSystem;

namespace Frenetic.TagHandlers
{
    public class TagParser
    {
        // <--[definition]
        // @Word specified
        // @Set tag
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
        /// <param name="input">The string that may have tags</param>
        /// <returns>An escaped string</returns>
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
        /// <param name="input">The string that was escaped</param>
        /// <returns>An unescaped string that may have tags</returns>
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
        public Dictionary<string, TemplateTags> Handlers = new Dictionary<string, TemplateTags>();

        /// <summary>
        /// Registers a handler object for later usage by tags.
        /// </summary>
        /// <param name="handler">The handler object to register.</param>
        public void Register(TemplateTags handler)
        {
            Handlers.Add(handler.Name, handler);
        }

        /// <summary>
        /// Prepares the tag system.
        /// </summary>
        public void Init(Commands _system)
        {
            CommandSystem = _system;
            Register(new ColorTags());
            Register(new CVarTags());
            Register(new EscapeTags());
            Register(new ListTags());
            Register(new TernaryTags());
            Register(new TextTags());
            Register(new UnescapeTags());
            Register(new UtilTags());
            Register(new VarTags());
            // TODO: CVars, ...
        }

        static char[] dot = new char[] { '.' };

        /// <summary>
        /// Reads and parses all tags inside a string.
        /// </summary>
        /// <param name="base_color">The base color for tags to use</param>
        /// <param name="vars">Any variables in this tag's context</param>
        /// <param name="input">The tagged string</param>
        /// <returns>The string with tags parsed</returns>
        public string ParseTags(string input, string base_color, Dictionary<string, string> vars, DebugMode mode)
        {
            if (input.IndexOf("<{") < 0)
            {
                return Unescape(input);
            }
            int len = input.Length;
            int blocks = 0;
            int brackets = 0;
            StringBuilder blockbuilder = new StringBuilder();
            StringBuilder final = new StringBuilder(len);
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
                        string value = blockbuilder.ToString();
                        List<string> split = value.Split(dot).ToList();
                        TagData data = new TagData(this, split, base_color, vars, mode);
                        TemplateTags handler;
                        bool handled = Handlers.TryGetValue(data.Input[0], out handler);
                        if (handled)
                        {
                            string res = handler.Handle(data);
                            final.Append(res);
                            if (mode <= DebugMode.FULL)
                            {
                                CommandSystem.Output.Good("Filled tag " + TextStyle.Color_Separate +
                                    Escape("<{" + value + "}>") + TextStyle.Color_Outgood + " with \"" + TextStyle.Color_Separate + res
                                    + TextStyle.Color_Outgood + "\".", mode);
                            }
                        }
                        else
                        {
                            if (mode <= DebugMode.MINIMAL)
                            {
                                CommandSystem.Output.Bad("Failed to fill tag tag " + TextStyle.Color_Separate +
                                    Escape("<{" + value + "}>") + TextStyle.Color_Outbad + "!", mode);
                            }
                            final.Append("{UNKNOWN_TAG:" + data.Input[0] + "}");
                        }
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
                    final.Append(input[i]);
                }
            }
            return Unescape(final.ToString());
        }
    }
}
