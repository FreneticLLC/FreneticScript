//
// This file is part of FreneticScript, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of the MIT license.
// See README.md or LICENSE.txt in the FreneticScript source root for the contents of the license.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreneticUtilities.FreneticExtensions;
using FreneticScript.CommandSystem;
using FreneticScript.CommandSystem.Arguments;
using FreneticScript.TagHandlers;

namespace FreneticScript.ScriptSystems;

/// <summary>Helper class for parsing arguments.</summary>
public static class ArgumentParser
{
    /// <summary>Splits text into an Argument, for preparsing.</summary>
    /// <param name="system">The relevant command system.</param>
    /// <param name="input">The original text.</param>
    /// <param name="wasquoted">Whether the argument was input with "quotes".</param>
    /// <returns>The parsed Argument.</returns>
    public static Argument SplitToArgument(ScriptEngine system, string input, bool wasquoted)
    {
        if (input.Length == 0)
        {
            return new Argument() { Bits = Array.Empty<ArgumentBit>() };
        }
        int firstOpen = input.IndexOf('<');
        if (firstOpen < 0 || input.IndexOf('>') < firstOpen)
        {
            Argument a = new() { WasQuoted = wasquoted };
            a.Bits = new ArgumentBit[] { new TextArgumentBit(input, wasquoted, wasquoted, system) };
            return a;
        }
        Argument arg = new() { WasQuoted = wasquoted };
        int len = input.Length;
        int blocks = 0;
        int brackets = 0;
        StringBuilder blockbuilder = new();
        StringBuilder tbuilder = new();
        List<ArgumentBit> bitos = new();
        for (int i = 0; i < len; i++)
        {
            if (input[i] == '<')
            {
                blocks++;
                if (blocks == 1)
                {
                    continue;
                }
            }
            else if (input[i] == '>')
            {
                blocks--;
                if (blocks == 0)
                {
                    if (tbuilder.Length > 0)
                    {
                        bitos.Add(new TextArgumentBit(tbuilder.ToString(), wasquoted, true, system));
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
                        // TODO: Scrap old fallback engine, in favor of null tricks.
                        if (brack == 0 && value[fb] == '|' && fb > 0 && value[fb - 1] == '|')
                        {
                            fallback = value[(fb + 1)..];
                            value = value[..(fb - 1)];
                            break;
                        }
                    }
                    string[] split = value.SplitFast('.');
                    for (int s = 0; s < split.Length; s++)
                    {
                        split[s] = split[s].Replace("&dot", ".").Replace("&amp", "&");
                    }
                    List<TagBit> bits = new();
                    for (int x = 0; x < split.Length; x++)
                    {
                        TagBit bit = new();
                        if (split[x].Length > 1 && split[x].Contains('[') && split[x][^1] == ']')
                        {
                            int index = split[x].IndexOf('[');
                            bit.Variable = SplitToArgument(system, split[x].Substring(index + 1, split[x].Length - (index + 2)), wasquoted);
                            split[x] = split[x][..index].ToLowerFast();
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
                    TagArgumentBit tab = new(system, bits.ToArray());
                    if (tab.Bits.Length > 0)
                    {
                        if (system.TagSystem.Handlers.TryGetValue(tab.Bits[0].Key.ToLowerFast(), out TemplateTagBase start))
                        {
                            tab.Start = start;
                        }
                        else
                        {
                            tab.Start = null;
                        }
                    }
                    tab.Fallback = fallback == null ? null : SplitToArgument(system, fallback, false);
                    bitos.Add(tab);
                    blockbuilder = new StringBuilder();
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
                            blockbuilder.Append('.');
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
            bitos.Add(new TextArgumentBit(tbuilder.ToString(), wasquoted, true, system));
        }
        arg.Bits = bitos.ToArray();
        return arg;
    }
}
