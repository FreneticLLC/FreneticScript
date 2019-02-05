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
        public static ScriptEngine Engine;

        public static Object Locker = new Object();

        static void Main(string[] args)
        {
            SysConsole.Init();
            Engine = new ScriptEngine() { Context = new ConsoleTesterContext() };
            Engine.Init();
            // Register things here!
            Engine.PostInit();
            CVarSystem cs = new CVarSystem(Engine.Context);
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    Thread.Sleep(50);
                    lock (Locker)
                    {
                        Engine.Tick(0.05);
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
                    Engine.ExecuteCommands(cmd, null);
                }
            }
        }
    }
}
