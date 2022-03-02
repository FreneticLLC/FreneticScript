//
// This file is part of FreneticScript, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of the MIT license.
// See README.md or LICENSE.txt in the FreneticScript source root for the contents of the license.
//

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FreneticScriptConsoleTester
{
    public class SysConsole
    {
        volatile static List<KeyValuePair<string, string>> Waiting = new();

        static Object ConsoleLock;

        static Object WriteLock;

        static Thread ConsoleOutputThread;

        static readonly CancellationTokenSource ConsoleCancelToken = new();

        /// <summary>Closes the SysConsole.</summary>
        public static void ShutDown()
        {
            lock (ConsoleLock)
            {
                lock (WriteLock)
                {
                    ConsoleCancelToken.Cancel();
                    if (Waiting.Count > 0)
                    {
                        foreach (KeyValuePair<string, string> message in Waiting)
                        {
                            WriteInternal(message.Value, message.Key);
                        }
                    }
                }
            }
        }

        /// <summary>Prepares the system console.</summary>
        public static void Init()
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Preparing console...");
            Console.WriteLine("Starting system...");
            ConsoleLock = new Object();
            WriteLock = new Object();
            ConsoleOutputThread = new Thread(new ThreadStart(ConsoleLoop));
            //Program.ThreadsToClose.Add(ConsoleOutputThread);
            ConsoleOutputThread.Start();
            Output(OutputType.INIT, "Console prepared...");
            Output(OutputType.INIT, "Test colors: ^r^7Text Colors: ^0^h^1^^n1 ^!^^n! ^2^^n2 ^@^^n@ ^3^^n3 ^#^^n# ^4^^n4 ^$^^n$ ^5^^n5 ^%^^n% ^6^^n6 ^-^^n- ^7^^n7 ^&^^n& ^8^^n8 ^*^^** ^9^^n9 ^(^^n( ^&^h^0^^n0^h ^)^^n) ^a^^na ^A^^nA\n" +
                            "^r^7Text styles: ^b^^nb is bold,^r ^i^^ni is italic,^r ^u^^nu is underline,^r ^s^^ns is strike-through,^r ^O^^nO is overline,^r ^7^h^0^^nh is highlight,^r^7 ^j^^nj is jello (AKA jiggle),^r " +
                            "^7^h^2^e^0^^ne is emphasis,^r^7 ^t^^nt is transparent,^r ^T^^nT is more transparent,^r ^o^^no is opaque,^r ^R^^nR is random,^r ^p^^np is pseudo-random,^r ^^nk is obfuscated (^kobfu^r),^r " +
                            "^^nS is ^SSuperScript^r, ^^nl is ^lSubScript (AKA Lower-Text)^r, ^h^8^d^^nd is Drop-Shadow,^r^7 ^f^^nf is flip,^r ^^nr is regular text, ^^nq is a ^qquote^q, ^^nn is nothing (escape-symbol),^r " +
                            "and ^^nB is base-colors.");
        }

        static void ConsoleLoop()
        {
            while (true)
            {
                if (ConsoleCancelToken.IsCancellationRequested)
                {
                    return;
                }
                List<KeyValuePair<string, string>> twaiting;
                lock (ConsoleLock)
                {
                    twaiting = Waiting;
                    Waiting = new List<KeyValuePair<string, string>>();
                }
                if (twaiting.Count > 0)
                {
                    // TODO: Log file control! Option to change file name or disable entirely...
                    // Also options to put a value like logs/%yyyy%/%mm%/%dd%.log
                    // TODO: Handle less terribly. Particular multiple-games-running logging
                    // FileHandler.AppendText("console.log", twaiting);
                    lock (WriteLock)
                    {
                        foreach (KeyValuePair<string, string> message in twaiting)
                        {
                            WriteInternal(message.Value, message.Key);
                        }
                    }
                }
                Thread.Sleep(100);
            }
        }

        /// <summary>The console title.</summary>
        public static string Title = "";

        /// <summary>Fixes the title of the system console to how the Client expects it.</summary>
        public static void FixTitle()
        {
            Title = "FreneticScriptConsoleTest / " + Environment.ProcessId.ToString();
            Console.Title = Title;
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        /// <summary>Hides the system console from view.</summary>
        public static void HideConsole()
        {
            // TODO ShowWindow(Program.ConsoleHandle, 0);
        }

        /// <summary>Shows (un-hides) the system console.</summary>
        public static void ShowConsole()
        {
            // TODO ShowWindow(Program.ConsoleHandle, 1);
        }

        /// <summary>Writes a line of colored text to the system console.</summary>
        /// <param name="text">The text to write.</param>
        public static void WriteLine(string text, string bcolor)
        {
            Write(text + "\n", bcolor);
        }

        public static EventHandler<ConsoleWrittenEventArgs> Written;

        /// <summary>Writes some colored text to the system console.</summary>
        /// <param name="text">The text to write.</param>
        private static void Write(string text, string bcolor)
        {
            lock (ConsoleLock)
            {
                Written?.Invoke(null, new ConsoleWrittenEventArgs() { Text = text, BColor = bcolor });
                Waiting.Add(new KeyValuePair<string, string>(bcolor, text));
            }
        }

        /// <summary>Used to identify if an input character is a valid color symbol (generally the character that follows a '^'), for use by RenderColoredText</summary>
        /// <param name="c"><paramref name="c"/>The character to check.</param>
        /// <returns>whether the character is a valid color symbol.</returns>
        public static bool IsColorSymbol(char c)
        {
            return ((c >= '0' && c <= '9') /* 0123456789 */ ||
                    (c >= 'a' && c <= 'b') /* ab */ ||
                    (c >= 'd' && c <= 'f') /* def */ ||
                    (c >= 'h' && c <= 'l') /* hijkl */ ||
                    (c >= 'n' && c <= 'u') /* nopqrstu */ ||
                    (c >= 'R' && c <= 'T') /* RST */ ||
                    (c >= '#' && c <= '&') /* #$%& */ || // 35 - 38
                    (c >= '(' && c <= '*') /* ()* */ || // 40 - 42
                    (c == 'A') ||
                    (c == 'O') ||
                    (c == '-') || // 45
                    (c == '!') || // 33
                    (c == '@') // 64
                    );
        }

        static void WriteInternal(string text, string bcolor)
        {
            text = text.Replace("^B", bcolor);
            Console.SetCursorPosition(0, Console.CursorTop);
            StringBuilder outme = new();
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '^' && i + 1 < text.Length && IsColorSymbol(text[i + 1]))
                {
                    if (outme.Length > 0)
                    {
                        Console.Write(outme);
                        outme.Clear();
                    }
                    i++;
                    switch (text[i])
                    {
                        case '0': Console.ForegroundColor = ConsoleColor.Black; break;
                        case '1': Console.ForegroundColor = ConsoleColor.Red; break;
                        case '2': Console.ForegroundColor = ConsoleColor.Green; break;
                        case '3': Console.ForegroundColor = ConsoleColor.Yellow; break;
                        case '4': Console.ForegroundColor = ConsoleColor.Blue; break;
                        case '5': Console.ForegroundColor = ConsoleColor.Cyan; break;
                        case '6': Console.ForegroundColor = ConsoleColor.Magenta; break;
                        case '7': Console.ForegroundColor = ConsoleColor.White; break;
                        case '8': Console.ForegroundColor = ConsoleColor.Magenta; break;
                        case '9': Console.ForegroundColor = ConsoleColor.Cyan; break;
                        case 'a': Console.ForegroundColor = ConsoleColor.Yellow; break;
                        case ')': Console.ForegroundColor = ConsoleColor.DarkGray; break;
                        case '!': Console.ForegroundColor = ConsoleColor.DarkRed; break;
                        case '@': Console.ForegroundColor = ConsoleColor.DarkGreen; break;
                        case '#': Console.ForegroundColor = ConsoleColor.DarkYellow; break;
                        case '$': Console.ForegroundColor = ConsoleColor.DarkBlue; break;
                        case '%': Console.ForegroundColor = ConsoleColor.DarkCyan; break;
                        case '-': Console.ForegroundColor = ConsoleColor.DarkMagenta; break;
                        case '&': Console.ForegroundColor = ConsoleColor.Gray; break;
                        case '*': Console.ForegroundColor = ConsoleColor.DarkMagenta; break;
                        case '(': Console.ForegroundColor = ConsoleColor.DarkCyan; break;
                        case 'A': Console.ForegroundColor = ConsoleColor.DarkYellow; break;
                        case 'b': break;
                        case 'i': break;
                        case 'u': break;
                        case 's': break;
                        case 'O': break;
                        case 'j': break;
                        case 'e': break;
                        case 't': break;
                        case 'T': break;
                        case 'o': break;
                        case 'R': break;
                        case 'p': break; // TODO: Probably shouldn't be implemented, but... it's possible
                        case 'k': break;
                        case 'S': break;
                        case 'l': break;
                        case 'd': break;
                        case 'f': break;
                        case 'n': break;
                        case 'q': outme.Append('"'); break;
                        case 'r': Console.BackgroundColor = ConsoleColor.Black; break;
                        case 'h': Console.BackgroundColor = Console.ForegroundColor; break;
                        default: outme.Append("INVALID-COLOR-CHAR:" + text[i] + "?"); break;
                    }
                }
                else
                {
                    outme.Append(text[i]);
                }
            }
            if (outme.Length > 0)
            {
                Console.Write(outme);
            }
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(">");// TODO + ConsoleHandler.read);
        }

        public static void Output(string message, Exception ex)
        {
            Output(OutputType.ERROR, message + ": " + ex.ToString() + "\n\n" + Environment.StackTrace);
        }

        public static void Output(Exception ex)
        {
            Output(OutputType.ERROR, ex.ToString() + "\n\n" + Environment.StackTrace);
        }

        public static void OutputCustom(string type, string message, string bcolor = "^r^7")
        {
            WriteLine("^r^7" + DateTimeToString(DateTime.Now) + " [" + bcolor + type + "^r^7] " + bcolor + message, bcolor);
        }

        /// <summary>Returns a string representation of the specified time.</summary>
        /// <returns>The time as a string.</returns>
        public static string DateTimeToString(DateTime dt)
        {
            string utcoffset;
            DateTime UTC = dt.ToUniversalTime();
            if (dt.CompareTo(UTC) < 0)
            {
                TimeSpan span = UTC.Subtract(dt);
                utcoffset = "-" + Pad(((int)Math.Floor(span.TotalHours)).ToString(), '0', 2) + ":" + Pad(span.Minutes.ToString(), '0', 2);
            }
            else
            {
                TimeSpan span = dt.Subtract(UTC);
                utcoffset = "+" + Pad(((int)Math.Floor(span.TotalHours)).ToString(), '0', 2) + ":" + Pad(span.Minutes.ToString(), '0', 2);
            }
            return Pad(dt.Year.ToString(), '0', 4) + "/" + Pad(dt.Month.ToString(), '0', 2) + "/" +
                    Pad(dt.Day.ToString(), '0', 2) + " " + Pad(dt.Hour.ToString(), '0', 2) + ":" +
                    Pad(dt.Minute.ToString(), '0', 2) + ":" + Pad(dt.Second.ToString(), '0', 2) + " UTC" + utcoffset;
        }

        /// <summary>Pads a string to a specified length with a specified input, on a specified side.</summary>
        /// <param name="input">The original string.</param>
        /// <param name="padding">The symbol to pad with.</param>
        /// <param name="length">How far to pad it to.</param>
        /// <param name="left">Whether to pad left (true), or right (false).</param>
        /// <returns>The padded string.</returns>
        public static string Pad(string input, char padding, int length, bool left = true)
        {
            int targetlength = length - input.Length;
            StringBuilder pad = new(targetlength <= 0 ? 1 : targetlength);
            for (int i = 0; i < targetlength; i++)
            {
                pad.Append(padding);
            }
            if (left)
            {
                return pad + input;
            }
            else
            {
                return input + pad;
            }
        }

        public static Func<bool> ShouldOutputDebug = () => true;

        /// <summary>Properly formats system console output.</summary>
        /// <param name="ot">What type of output to use.</param>
        /// <param name="text">The text to output.</param>
        public static void Output(OutputType ot, string text, string bcolor = null)
        {
            if (ot == OutputType.DEBUG && !ShouldOutputDebug())
            {
                return;
            }
            WriteLine("^r^7" + DateTimeToString(DateTime.Now) + " [" + OutputColors[(int)ot] +
                OutputNames[(int)ot] + "^r^7] " + OutputColors[(int)ot] + text, bcolor ?? OutputColors[(int)ot]);
        }

        static readonly string[] OutputColors = new string[]
        {
            "^r^7ERROR:OUTPUTTYPE=NONE?",
            "^r^7",
            "^r^2",
            "^r^3",
            "^r^7^h^0",
            "^r^7",
            "^7^&",
            "^r^2"
        };

        static readonly string[] OutputNames = new string[]
        {
            "NONE",
            "INFO/CLIENT",
            "INIT",
            "WARNING",
            "ERROR",
            "INFO",
            "DEBUG",
            "GOOD"
        };
    }

    public class ConsoleWrittenEventArgs : EventArgs
    {
        public string Text;

        public string BColor;
    }

    /// <summary>All possible console output types.</summary>
    public enum OutputType : int
    {
        /// <summary>Do not use.</summary>
        NONE = 0,
        /// <summary>When the client is sending information to console.</summary>
        CLIENTINFO = 1,
        /// <summary>During the startup sequence.</summary>
        INIT = 2,
        /// <summary>An ignorable error.</summary>
        WARNING = 3,
        /// <summary>A major error.</summary>
        ERROR = 4,
        /// <summary>General information.</summary>
        INFO = 5,
        /// <summary>Disable-able minor debug information.</summary>
        DEBUG = 6,
        /// <summary>Good information, a positive output.</summary>
        GOOD = 7,
        // TODO: More?
    }
}
