using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers.Common;
using FreneticScript.CommandSystem;
using FreneticScript.CommandSystem.Arguments;
using FreneticScript.TagHandlers.Objects;
using System.Reflection;
using System.Reflection.Emit;

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
        /// All tag types currently registered.
        /// </summary>
        public Dictionary<string, TagType> Types = new Dictionary<string, TagType>();

        /// <summary>
        /// Registers a handler object for later usage by tags.
        /// </summary>
        /// <param name="handler">The handler object to register.</param>
        public void Register(TemplateTagBase handler)
        {
            Handlers.Add(handler.Name, handler);
        }

        /// <summary>
        /// Registers a type object for later usage by tags.
        /// </summary>
        /// <param name="type">The type object to register.</param>
        public void Register(TagType type)
        {
            Types.Add(type.TypeName, type);
        }

        /// <summary>
        /// Local variable tag base.
        /// </summary>
        public LvarTagBase LVar;

        /// <summary>
        /// Prepares the tag system.
        /// </summary>
        public void Init(Commands _system)
        {
            CommandSystem = _system;
            // Bases
            Register(new BinaryTagBase());
            Register(new BooleanTagBase());
            Register(new CVarTagBase());
            Register(new EscapeTagBase());
            Register(new IntegerTagBase());
            Register(new ListTagBase());
            Register(LVar = new LvarTagBase());
            Register(new MapTagBase());
            Register(new NumberTagBase());
            Register(new SystemTagBase());
            Register(new TagTypeBase());
            Register(new TernaryTagBase());
            Register(new TextColorTagBase());
            Register(new TextTagBase());
            Register(new TimeTagBase());
            Register(new UnescapeTagBase());
            Register(new VarTagBase());
            // Object types
            Register(Type_Binary = new TagType()
            {
                TypeName = "binarytag",
                SubTypeName = TextTag.TYPE,
                TypeGetter = BinaryTag.For,
                GetNextTypeDown = (obj) => new TextTag(obj.ToString()),
                SubHandlers = null
            });
            Register(Type_Boolean = new TagType()
            {
                TypeName = "booleantag",
                SubTypeName = TextTag.TYPE,
                TypeGetter = BooleanTag.For,
                GetNextTypeDown = (obj) => new TextTag(obj.ToString()),
                SubHandlers = null
            });
            Register(Type_Cvar = new TagType()
            {
                TypeName = "cvartag",
                SubTypeName = TextTag.TYPE,
                TypeGetter = CVarTag.For,
                GetNextTypeDown = (obj) => new TextTag(obj.ToString()),
                SubHandlers = CVarTag.Handlers
            });
            Register(Type_Dynamic = new TagType()
            {
                TypeName = "dynamictag",
                SubTypeName = TextTag.TYPE,
                TypeGetter = DynamicTag.For,
                GetNextTypeDown = (obj) => new TextTag(obj.ToString()),
                SubHandlers = DynamicTag.Handlers
            });
            Register(Type_Integer = new TagType()
            {
                TypeName = IntegerTag.TYPE,
                SubTypeName = "numbertag",
                TypeGetter = IntegerTag.For,
                GetNextTypeDown = (obj) => new NumberTag(((IntegerTag)obj).Internal),
                SubHandlers = IntegerTag.Handlers,
                RawType = typeof(IntegerTag)
            });
            Register(Type_List = new TagType()
            {
                TypeName = "listtag",
                SubTypeName = TextTag.TYPE,
                TypeGetter = ListTag.For,
                GetNextTypeDown = (obj) => new TextTag(obj.ToString()),
                SubHandlers = null
            });
            Register(Type_Map = new TagType()
            {
                TypeName = "maptag",
                SubTypeName = TextTag.TYPE,
                TypeGetter = MapTag.For,
                GetNextTypeDown = (obj) => new TextTag(obj.ToString()),
                SubHandlers = null
            });
            Register(Type_Null = new TagType()
            {
                TypeName = "nulltag",
                SubTypeName = TextTag.TYPE,
                TypeGetter = (data, obj) => new NullTag(),
                GetNextTypeDown = (obj) => new TextTag(obj.ToString()),
                SubHandlers = NullTag.Handlers
            });
            Register(Type_Number = new TagType()
            {
                TypeName = "numbertag",
                SubTypeName = TextTag.TYPE,
                TypeGetter = NumberTag.For,
                GetNextTypeDown = (obj) => new TextTag(obj.ToString()),
                SubHandlers = null
            });
            Register(Type_System = new TagType()
            {
                TypeName = SystemTagBase.SystemTag.TYPE,
                SubTypeName = TextTag.TYPE,
                TypeGetter = SystemTagBase.SystemTag.For,
                GetNextTypeDown = (obj) => new TextTag(obj.ToString()),
                SubHandlers = SystemTagBase.SystemTag.Handlers,
                RawType = typeof(SystemTagBase.SystemTag)
            });
            Register(Type_TagType = new TagType()
            {
                TypeName = "tagtypetag",
                SubTypeName = TextTag.TYPE,
                TypeGetter = TagTypeTag.For,
                GetNextTypeDown = (obj) => new TextTag(obj.ToString()),
                SubHandlers = TagTypeTag.Handlers
            });
            Register(Type_TernayPass = new TagType()
            {
                TypeName = "ternarypasstag",
                SubTypeName = TextTag.TYPE,
                TypeGetter = TernaryTagBase.TernaryPassTag.For,
                GetNextTypeDown = (obj) => new TextTag(obj.ToString()),
                SubHandlers = null
            });
            Register(Type_Text = new TagType()
            {
                TypeName = TextTag.TYPE,
                SubTypeName = null,
                TypeGetter = (data, obj) => new TextTag(obj.ToString()),
                SubHandlers = TextTag.Handlers,
                RawType = typeof(TextTag)
            });
            Register(Type_Time = new TagType()
            {
                TypeName = TimeTag.TYPE,
                SubTypeName = TextTag.TYPE,
                TypeGetter = TimeTag.For,
                GetNextTypeDown = (obj) => new TextTag(obj.ToString()),
                SubHandlers = TimeTag.Handlers,
                RawType = typeof(TimeTag)
            });
        }
        /// <summary>
        /// Set up the tag engine after all input has be registered.
        /// </summary>
        public void PostInit()
        {
            foreach (TemplateTagBase tagbase in Handlers.Values)
            {
                if (tagbase.ResultTypeString != null)
                {
                    tagbase.ResultType = Types[tagbase.ResultTypeString];
                }
            }
            foreach (TagType type in Types.Values)
            {
                if (type.SubTypeName == null)
                {
                    type.SubType = null;
                }
                else
                {
                    type.SubType = Types[type.SubTypeName];
                }
                type.TagHelpers = new Dictionary<string, TagHelpInfo>();
                if (type.RawType != null)
                {
                    foreach (MethodInfo method in type.RawType.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public))
                    {
                        TagMeta tm = method.GetCustomAttribute<TagMeta>();
                        if (tm != null)
                        {
                            tm.Ready(this);
                            TagHelpInfo thi = new TagHelpInfo(method);
                            thi.Meta.ReturnTypeResult = Types[thi.Meta.ReturnType];
                            if (thi.Meta.ReturnTypeResult == null)
                            {
                                CommandSystem.Output.BadOutput("Bad tag declaration (returns '" + thi.Meta.ReturnType + "'): " + type.TypeName + "." + thi.Meta.Name);
                            }
                            type.TagHelpers.Add(tm.Name, thi);
                        }
                        else if (method.Name == "CreateFor")
                        {
                            ParameterInfo[] prms = method.GetParameters();
                            if (prms.Length == 2 && prms[0].ParameterType == typeof(TagData) && prms[1].ParameterType == typeof(TemplateObject))
                            {
                                type.CreatorMethod = method;
                            }
                        }
                    }
                }
                // TODO: this line: else { error(); }
                Dictionary<string, TagSubHandler> orig = type.SubHandlers;
                type.SubHandlers = new Dictionary<string, TagSubHandler>();
                if (orig != null)
                {
                    foreach (KeyValuePair<string, TagSubHandler> point in orig)
                    {
                        TagSubHandler hand = point.Value.Duplicate();
                        if (hand.ReturnTypeString != null && !Types.ContainsKey(hand.ReturnTypeString))
                        {
                            CommandSystem.Output.BadOutput("Unrecognized type string: " + hand.ReturnTypeString);
                        }
                        else
                        {
                            hand.ReturnType = hand.ReturnTypeString == null ? null : Types[hand.ReturnTypeString];
                            type.SubHandlers.Add(point.Key, hand);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// The BinaryTag type.
        /// </summary>
        public TagType Type_Binary;

        /// <summary>
        /// The BooleanTag type.
        /// </summary>
        public TagType Type_Boolean;

        /// <summary>
        /// The CVar tag type.
        /// </summary>
        public TagType Type_Cvar;

        /// <summary>
        /// The DynamicTag type.
        /// </summary>
        public TagType Type_Dynamic;

        /// <summary>
        /// The IntegerTag type.
        /// </summary>
        public TagType Type_Integer;

        /// <summary>
        /// The ListTag type.
        /// </summary>
        public TagType Type_List;

        /// <summary>
        /// The MapTag type.
        /// </summary>
        public TagType Type_Map;

        /// <summary>
        /// The NullTag type.
        /// </summary>
        public TagType Type_Null;

        /// <summary>
        /// The NumberTag type.
        /// </summary>
        public TagType Type_Number;

        /// <summary>
        /// The SystemTag type.
        /// </summary>
        public TagType Type_System;

        /// <summary>
        /// The TagTypeTag type.
        /// </summary>
        public TagType Type_TagType;

        /// <summary>
        /// The TernaryPassTag type.
        /// </summary>
        public TagType Type_TernayPass;

        /// <summary>
        /// The TextTag type.
        /// </summary>
        public TagType Type_Text;

        /// <summary>
        /// The TimeTag type.
        /// </summary>
        public TagType Type_Time;

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
                        for (int fb = 0; fb < value.Length; fb++)
                        {
                            if (value[fb] == '[')
                            {
                                brack++;
                            }
                            if (value[fb] == ']')
                            {
                                brack--;
                            }
                            if (brack == 0 && value[fb] == '|' && fb > 0 && value[fb - 1] == '|')
                            {
                                fallback = value.Substring(fb + 1);
                                value = value.Substring(0, fb);
                                break;
                            }
                        }
                        string[] split = value.SplitFast('.');
                        for (int s = 0; s < split.Length; s++)
                        {
                            split[s] = split[s].Replace("&dot", ".").Replace("&amp", "&");
                        }
                        List<TagBit> bits = new List<TagBit>();
                        for (int x = 0; x < split.Length; x++)
                        {
                            TagBit bit = new TagBit();
                            if (split[x].Length > 1 && split[x].Contains('[') && split[x][split[x].Length - 1] == ']')
                            {
                                int index = split[x].IndexOf('[');
                                bit.Variable = SplitToArgument(split[x].Substring(index + 1, split[x].Length - (index + 2)), wasquoted);
                                split[x] = split[x].Substring(0, index).ToLowerFast();
                                if (split[x].Length == 0)
                                {
                                    if (x == 0)
                                    {
                                        split[x] = "var";
                                    }
                                    else
                                    {
                                        split[x] = "get";
                                    }
                                }
                            }
                            else
                            {
                                split[x] = split[x].ToLowerFast();
                                bit.Variable = new Argument();
                            }
                            bit.Key = split[x];
                            bits.Add(bit);
                        }
                        TagArgumentBit tab = new TagArgumentBit(CommandSystem, bits.ToArray());
                        if (tab.Bits.Length > 0)
                        {
                            TemplateTagBase start;
                            if (!CommandSystem.TagSystem.Handlers.TryGetValue(tab.Bits[0].Key.ToLowerFast(), out start))
                            {
                                throw new ErrorInducedException("Invalid tag base '" + tab.Bits[0].Key.ToLowerFast() + "'!");
                            }
                            tab.Start = start;
                        }
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
        /// <param name="cse">The relevant command stack entry, if any.</param>
        /// <returns>The string with tags parsed.</returns>
        public TemplateObject ParseTags(TagArgumentBit bits, string base_color, Dictionary<string, ObjectHolder> vars, DebugMode mode, Action<string> error, CommandStackEntry cse)
        {
            if (bits.Bits.Length == 0)
            {
                return new TextTag("");
            }
            TagData data = new TagData(this, bits.Bits, base_color, vars, mode, error, bits.Fallback, cse) { Start = bits.Start };
            TemplateObject res = bits.GetResultHelper(data);
            if (mode <= DebugMode.FULL)
            {
                CommandSystem.Output.GoodOutput("Filled tag " + TextStyle.Color_Separate +
                    bits.ToString() + TextStyle.Color_Outgood + " with \"" + TextStyle.Color_Separate + res.ToString()
                    + TextStyle.Color_Outgood + "\".");
            }
            return res;
        }

        /// <summary>
        /// Reads and parses all tags inside a string. Note: Avoid this where possible. Use SplitToArgument instead!
        /// </summary>
        /// <param name="base_color">The base color for tags to use.</param>
        /// <param name="vars">Any variables in this tag's context.</param>
        /// <param name="input">The tagged string.</param>
        /// <param name="mode">What debugmode to use.</param>
        /// <param name="error">What to invoke if there's an error.</param>
        /// <param name="wasquoted">Whether the input had "quotes".</param>
        /// <returns>The string with tags parsed.</returns>
        public string ParseTagsFromText(string input, string base_color, Dictionary<string, ObjectHolder> vars, DebugMode mode, Action<string> error, bool wasquoted)
        {
            // TODO: Unescape need?
            return Unescape((SplitToArgument(input, wasquoted).Parse(base_color, vars, mode, error, null) ?? new NullTag()).ToString());
        }
    }
}
