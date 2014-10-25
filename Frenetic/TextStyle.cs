using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Frenetic
{
    /// <summary>
    /// Holds all text styles to be used... replace these with your engine's color tag system.
    /// The Frenetic core uses these where needed, particularly in the ColorTags class.
    /// </summary>
    public class TextStyle
    {
        public static string Default = "^r^7";
        public static string Reset = "^r";
        public static string Bold = "^b";
        public static string Italic = "^i";
        public static string Transparent = "^t";
        public static string Opaque = "^o";
        public static string White = "^7";
        public static string Color_Simple = "^r^7";
        public static string Color_Standout = "^r^0^h^5";
        public static string Color_Readable = "^r^7^e^0^b";
        public static string Color_Chat = "^r^2^d";
        public static string Color_Error = "^r^0^h^3";
        public static string Color_Warning = "^r^0^h^1";
        public static string Color_Commandhelp = "^r^0^h^1";
        public static string Color_Separate = "^r^5";
        public static string Color_Outgood = "^r^2";
        public static string Color_Outbad = "^r^1";
        public static string Color_Importantinfo = "^r^3";
    }
}
