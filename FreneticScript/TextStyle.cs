//
// This file is part of FreneticScript, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of the MIT license.
// See README.md or LICENSE.txt in the FreneticScript source root for the contents of the license.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers.HelperBases;

namespace FreneticScript
{
    /// <summary>
    /// Holds all text styles to be used... replace these with your engine's color tag system.
    /// The FreneticScript core uses these where needed, particularly in the <see cref="TextColorTagBase"/> class.
    /// </summary>
    public static class TextStyle
    {
        /// <summary>Default style, default (SysConsole) value: ^r^7</summary>
        public static string Default = "^r^7";
        /// <summary>Reset style, default (SysConsole) value: ^r</summary>
        public static string Reset = "^r";
        /// <summary>Bold style, default (SysConsole) value: ^b</summary>
        public static string Bold = "^b";
        /// <summary>Italic style, default (SysConsole) value: ^i</summary>
        public static string Italic = "^i";
        /// <summary>Transparent style, default (SysConsole) value: ^t</summary>
        public static string Transparent = "^t";
        /// <summary>Opaque style, default (SysConsole) value: ^o</summary>
        public static string Opaque = "^o";
        /// <summary>White color: ^7</summary>
        public static string White = "^7";
        /// <summary>Minor style, default (SysConsole) value: ^r^)</summary>
        public static string Minor = "^r^)";
        /// <summary>Simple style, default (SysConsole) value: ^r^7</summary>
        public static string Simple = "^r^7";
        /// <summary>Standout style, default (SysConsole) value: ^r^0^h^5</summary>
        public static string Standout = "^r^0^h^5";
        /// <summary>Readable style, default (SysConsole) value: ^r^7^e^0^b</summary>
        public static string Readable = "^r^7^e^0^b";
        /// <summary>Chat style, default (SysConsole) value: ^r^2^d</summary>
        public static string Chat = "^r^2^d";
        /// <summary>Error style, default (SysConsole) value: ^r^0^h^3</summary>
        public static string Error = "^r^0^h^3";
        /// <summary>Warning style, default (SysConsole) value: ^r^0^h^1</summary>
        public static string Warning = "^r^0^h^1";
        /// <summary>Commandhelp style, default (SysConsole) value: ^r^0^h^1</summary>
        public static string Commandhelp = "^r^0^h^1";
        /// <summary>Separate style, default (SysConsole) value: ^r^5</summary>
        public static string Separate = "^r^5";
        /// <summary>Good output style, default (SysConsole) value: ^r^2</summary>
        public static string Outgood = "^r^2";
        /// <summary>Bad output style, default (SysConsole) value: ^r^1</summary>
        public static string Outbad = "^r^1";
        /// <summary>Important information style, default (SysConsole) value: ^r^3</summary>
        public static string Importantinfo = "^r^3";
        /// <summary>Base coloring style, default (SysConsole) value: ^B</summary>
        public static string Base = "^B";
    }
}
