using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreneticScript;
using FreneticScript.CommandSystem;

namespace FreneticScriptConsoleTester
{
    public class ConsoleTestOutputter : Outputter
    {
        public override void BadOutput(string text)
        {
            SysConsole.Output(OutputType.WARNING, "[BAD] " + text);
        }

        public override void GoodOutput(string text)
        {
            SysConsole.Output(OutputType.INFO, "[GOOD] " + text);
        }

        public override byte[] ReadDataFile(string name)
        {
            throw new NotImplementedException();
        }

        public override string ReadTextFile(string name)
        {
            throw new NotImplementedException();
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
            SysConsole.Output(OutputType.INFO, "[INFO] " + text);
        }
    }
}
