//
// This file is created by Frenetic LLC.
// This code is Copyright (C) 2016-2017 Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreneticScript;
using FreneticScript.CommandSystem;
using FreneticScript.TagHandlers;

namespace FreneticScriptConsoleTester
{
    public class ConsoleTesterContext : ScriptEngineContext
    {
        public override void BadOutput(string text)
        {
            SysConsole.Output(OutputType.WARNING, TagParser.Unescape(text));
        }

        public override void GoodOutput(string text)
        {
            SysConsole.Output(OutputType.GOOD, TagParser.Unescape(text));
        }

        public override byte[] ReadDataFile(string name)
        {
            throw new NotImplementedException();
        }

        string Clean(string inp)
        {
            return Environment.CurrentDirectory + "/data/" + inp.Replace("..", "/").Replace('\\', '/');
        }

        public override string ReadTextFile(string name)
        {
            return System.IO.File.ReadAllText(Clean(name));
        }

        public override void Reload()
        {
        }

        public override bool ShouldErrorOnInvalidCommand()
        {
            return true;
        }

        public override void UnknownCommand(CommandQueue queue, string basecommand, string[] arguments)
        {
            throw new NotImplementedException();
        }

        public override void WriteDataFile(string name, byte[] data)
        {
            throw new NotImplementedException();
        }

        public override void WriteLine(string text)
        {
            SysConsole.Output(OutputType.INFO, TagParser.Unescape(text));
        }
    }
}
