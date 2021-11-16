//
// This file is part of FreneticScript, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of the MIT license.
// See README.md or LICENSE.txt in the FreneticScript source root for the contents of the license.
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreneticUtilities.FreneticToolkit;
using FreneticScript;
using FreneticScript.CommandSystem;
using FreneticScript.TagHandlers;

namespace FreneticScriptConsoleTester
{
    public class ConsoleTesterContext : ScriptEngineContext
    {
        public class ConsoleTesterEventHelper : FreneticEventHelper
        {
            public override void ScheduleSync(Action act)
            {
                Program.SyncTasks.Enqueue(act);
            }

            public override void ScheduleSync(Action act, double delay)
            {
                if (delay <= 0)
                {
                    ScheduleSync(act);
                    return;
                }
                Task.Factory.StartNew(() =>
                {
                    Task.Delay((int)(delay * 1000)).Wait();
                    ScheduleSync(act);
                });
            }

            public override void StartAsync(Action act)
            {
                Task.Factory.StartNew(act);
            }
        }

        public ConsoleTesterEventHelper Helper = new ConsoleTesterEventHelper();

        public override FreneticEventHelper GetEventHelper()
        {
            return Helper;
        }

        public override void BadOutput(string text)
        {
            SysConsole.Output(OutputType.WARNING, TagHandler.Unescape(text));
        }

        public override void GoodOutput(string text)
        {
            SysConsole.Output(OutputType.GOOD, TagHandler.Unescape(text));
        }

        public override byte[] ReadDataFile(string name)
        {
            throw new NotImplementedException();
        }

        string Clean(string inp)
        {
            return inp.Replace("..", "/").Replace('\\', '/');
        }

        public override string ReadTextFile(string name)
        {
            return File.ReadAllText(Clean(name));
        }

        public override void Reload()
        {
        }

        public override void WriteDataFile(string name, byte[] data)
        {
            throw new NotImplementedException();
        }

        public override void WriteLine(string text)
        {
            SysConsole.Output(OutputType.INFO, TagHandler.Unescape(text));
        }

        public override string[] ListFiles(string path, string extension, bool deep)
        {
            path = Clean(path);
            if (!Directory.Exists(path))
            {
                return new string[0];
            }
            string[] results = Directory.GetFiles(path, extension == null ? "*.*" : "*." + extension , deep ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
            for (int i = 0; i < results.Length; i++)
            {
                results[i] = results[i].Replace('\\', '/').Replace(Environment.CurrentDirectory.Replace('\\', '/'), "");
            }
            return results;
        }
    }
}
