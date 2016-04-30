using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Objects;

namespace FreneticScript.CommandSystem.CommonCmds
{
    class YamlCommand: AbstractCommand
    {
        // TODO: Meta!

        public YamlCommand()
        {
            Name = "yaml";
            Arguments = "'new'/'write'/'read'/'save'/'load' <id> [key]/[filename] [value]";
            Description = "Adjust a YAML file.";
            Asyncable = true;
            MinimumArguments = 2;
            MaximumArguments = 4;
            ObjectTypes = new List<Func<TemplateObject, TemplateObject>>()
            {
                verify,
                TextTag.For,
                TextTag.For,
                TemplateObject.Basic_For
            };
        }

        TemplateObject verify(TemplateObject input)
        {
            string low = input.ToString().ToLowerFast();
            if (low == "new" || low == "write" || low == "read" || low == "save" || low == "load")
            {
                return new TextTag(low);
            }
            return null;
        }

        public Dictionary<string, YAMLConfiguration> configs = new Dictionary<string, YAMLConfiguration>();
        
        public override void Execute(CommandQueue queue, CommandEntry entry)
        {
            string arg0 = entry.GetArgument(queue, 0);
            string id = entry.GetArgument(queue, 1).ToLowerFast();
            YAMLConfiguration config;
            if (arg0 == "write")
            {
                if (entry.Arguments.Count < 4)
                {
                    ShowUsage(queue, entry);
                    return;
                }
                string key = entry.GetArgument(queue, 2);
                TemplateObject toWrite = entry.GetArgumentObject(queue, 3);
                if (!configs.TryGetValue(id, out config))
                {
                    queue.HandleError(entry, "A YAML Configuration by that id doesn't exist!");
                    return;
                }
                if (toWrite is IntegerTag)
                {
                    config.Set(key, ((IntegerTag)toWrite).Internal);
                }
                else if (toWrite is NumberTag)
                {
                    config.Set(key, ((NumberTag)toWrite).Internal);
                }
                else if (toWrite is ListTag)
                {
                    List<string> strs = new List<string>(((ListTag)toWrite).ListEntries.Capacity);
                    foreach (TemplateObject obj in ((ListTag)toWrite).ListEntries)
                    {
                        strs.Add(obj.ToString());
                    }
                    config.Set(key, strs);
                }
                // TODO: MapTag?
                else
                {
                    config.Set(key, toWrite.ToString());
                }
                if (entry.ShouldShowGood(queue))
                {
                    entry.Good(queue, "Wrote successfully!");
                }
            }
            else if (arg0 == "read")
            {
                if (entry.Arguments.Count < 3)
                {
                    ShowUsage(queue, entry);
                    return;
                }
                string key = entry.GetArgument(queue, 2);
                if (!configs.TryGetValue(id, out config))
                {
                    queue.HandleError(entry, "A YAML Configuration by that id doesn't exist!");
                    return;
                }
                object obj = config.Read(key, null);
                if (obj == null)
                {
                    queue.HandleError(entry, "That key does not exist!");
                    return;
                }
                TemplateObject res;
                if (obj is Int64 || obj is Int32)
                {
                    res = new IntegerTag((long)obj);
                }
                else if (obj is Double || obj is Single)
                {
                    res = new NumberTag((double)obj);
                }
                else if (obj is List<string>)
                {
                    res = new ListTag((List<string>)obj);
                }
                else if (obj is List<object>)
                {
                    List<string> strs = new List<string>();
                    foreach (object o in (List<object>)obj)
                    {
                        strs.Add(o.ToString());
                    }
                    res = new ListTag(strs);
                }
                // TODO: Map/Dictionary?
                else
                {
                    res = new TextTag(obj.ToString());
                }
                queue.SetVariable("yaml_read", res);
                if (entry.ShouldShowGood(queue))
                {
                    entry.Good(queue, "Read successfully!");
                }
            }
            else if (arg0 == "save")
            {
                if (entry.Arguments.Count < 3)
                {
                    ShowUsage(queue, entry);
                    return;
                }
                string fname = entry.GetArgument(queue, 2).ToLowerFast();
                if (!configs.TryGetValue(id, out config))
                {
                    queue.HandleError(entry, "A YAML Configuration by that id doesn't exist!");
                    return;
                }
                try
                {
                    string saved = config.SaveToString();
                    byte[] data = FreneticScriptUtilities.Enc.GetBytes(saved);
                    queue.CommandSystem.Output.WriteDataFile(fname, data);
                    if (entry.ShouldShowGood(queue))
                    {
                        entry.Good(queue, "Saved configuration!");
                    }
                }
                catch (System.IO.FileNotFoundException ex)
                {
                    queue.HandleError(entry, "Unable to save YAML file: " + ex.Message);
                    return;
                }
            }
            else if (arg0 == "new")
            {
                if (configs.ContainsKey(id))
                {
                    queue.HandleError(entry, "A YAML Configuration by that id already exists!");
                    return;
                }
                config = new YAMLConfiguration("");
                configs.Add(id, config);
                if (entry.ShouldShowGood(queue))
                {
                    entry.Good(queue, "Prepared new configuration!");
                }
            }
            else // Load
            {
                if (entry.Arguments.Count < 3)
                {
                    ShowUsage(queue, entry);
                    return;
                }
                string fname = entry.GetArgument(queue, 2).ToLowerFast();
                if (configs.ContainsKey(id))
                {
                    queue.HandleError(entry, "A YAML Configuration by that id already exists!");
                    return;
                }
                try
                {
                    byte[] dat = queue.CommandSystem.Output.ReadDataFile(fname);
                    string str = FreneticScriptUtilities.Enc.GetString(dat);
                    config = new YAMLConfiguration(str);
                    configs.Add(id, config);
                    if (entry.ShouldShowGood(queue))
                    {
                        entry.Good(queue, "Loaded new configuration!");
                    }
                }
                catch (System.IO.FileNotFoundException ex)
                {
                    queue.HandleError(entry, "Unable to load YAML file: " + ex.Message);
                    return;
                }
            }
        }
    }
}
