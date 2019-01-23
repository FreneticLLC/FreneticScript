//
// This file is part of FreneticScript, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of the MIT license.
// See README.md or LICENSE.txt in the FreneticScript source root for the contents of the license.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript;
using FreneticScript.CommandSystem;
using System.Threading;
using System.Threading.Tasks;

namespace FreneticScriptConsoleTester
{
    class Program
    {
        public static Commands CommandSystem;

        public static Object Locker = new Object();

        static void Main(string[] args)
        {
            SysConsole.Init();
            CommandSystem = new Commands() { Context = new ConsoleTesterContext() };
            CommandSystem.Init();
            // Register things here!
            CommandSystem.PostInit();
            CVarSystem cs = new CVarSystem(CommandSystem.Context);
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    Thread.Sleep(50);
                    lock (Locker)
                    {
                        CommandSystem.Tick(0.05);
                    }
                }
            });
            while (true)
            {
                string cmd = Console.ReadLine();
                if (cmd == "quit")
                {
                    Environment.Exit(0);
                    return;
                }
                lock (Locker)
                {
                    CommandSystem.ExecuteCommands(cmd, null);
                }
            }
        }
    }
}
