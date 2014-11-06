using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Frenetic.CommandSystem.QueueCmds
{
    class ElseCommand: AbstractCommand
    {
        // TODO: META
        public ElseCommand()
        {
            Name = "else";
            Arguments = "[if true/false]";
            Description = "Executes the following block of commands only if the previous if failed, "
            + "and optionally if additional requirements are met.";
            IsFlow = true;
        }

        public override void Execute(CommandEntry entry)
        {
            IfCommandData data = new IfCommandData();
            data.Result = 0;
            entry.Data = data;
            CommandEntry IfEntry = null;
            CommandEntry Holder = entry.Queue.LastCommand;
            while (IfEntry == null && Holder != null)
            {
                if (Holder.BlockOwner == entry.BlockOwner)
                {
                    if (Holder.Command.Name == "if" || Holder.Command.Name == "else")
                    {
                        IfEntry = Holder;
                    }
                    break;
                }
                Holder = Holder.BlockOwner;
            }
            if (IfEntry == null)
            {
                entry.Bad("Else invalid: IF command did not preceed!");
                return;
            }
            if (((IfCommandData)IfEntry.Data).Result == 1)
            {
                entry.Good("Else continuing, IF passed.");
                return;
            }
            if (entry.Arguments.Count >= 1)
            {
                string ifbit = entry.GetArgument(0);
                if (ifbit.ToLower() != "if")
                {
                    ShowUsage(entry);
                    return;
                }
                else
                {
                    List<string> parsedargs = new List<string>(entry.Arguments.Count);
                    for (int i = 1; i < entry.Arguments.Count; i++)
                    {
                        parsedargs.Add(entry.GetArgument(i));
                    }
                    bool success = IfCommand.TryIf(parsedargs);
                    if (entry.Block != null)
                    {
                        if (success)
                        {
                            entry.Good("Else if is true, executing...");
                            data.Result = 1;
                            entry.Queue.AddCommandsNow(entry.Block);
                        }
                        else
                        {
                            entry.Good("Else If is false, doing nothing!");
                        }
                    }
                }
            }
            else
            {
                if (entry.Block != null)
                {
                    entry.Good("Else is valid, executing...");
                    data.Result = 1;
                    entry.Queue.AddCommandsNow(entry.Block);
                }
                else
                {
                    entry.Bad("Else invalid: No block follows!");
                }
            }
        }
    }
}
