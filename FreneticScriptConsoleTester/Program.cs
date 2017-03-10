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
            CommandSystem = new Commands() { Output = new ConsoleTestOutputter() };
            CommandSystem.Init();
            // Register things here!
            CommandSystem.PostInit();
            CVarSystem cs = new CVarSystem(CommandSystem.Output);
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
