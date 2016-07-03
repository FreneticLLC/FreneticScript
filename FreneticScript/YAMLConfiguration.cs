using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using YamlDotNet.Serialization;
using System.IO;

namespace FreneticScript
{
    /// <summary>
    /// Represents a "YAML Ain't Markup Language" data container.
    /// </summary>
    public class YAMLConfiguration
    {
        /// <summary>
        /// Constructs a YAML configuration from the text of a YAML file.
        /// </summary>
        /// <param name="input">The text to construct from, or an empty string for an empty configuration.</param>
        public YAMLConfiguration(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                Data = new Dictionary<object, object>();
            }
            else
            {
                Deserializer des = new Deserializer();
                Data = des.Deserialize<Dictionary<object, object>>(new StringReader(input));
            }
        }

        /// <summary>
        /// Constructs a YAML configuration from known data.
        /// </summary>
        /// <param name="datas">The data to use.</param>
        public YAMLConfiguration(Dictionary<object, object> datas)
        {
            Data = datas;
        }

        /// <summary>
        /// The internal data of this YAMLConfiguration.
        /// </summary>
        public Dictionary<object, object> Data;

        /// <summary>
        /// Returns whether a specified path is a list.
        /// </summary>
        /// <param name="path">The path to read.</param>
        /// <returns>Whether it is a list.</returns>
        public bool IsList(string path)
        {
            List<object> res = ReadList(path);
            return res != null;
        }

        /// <summary>
        /// Reads a path as a string list.
        /// </summary>
        /// <param name="path">The path to read.</param>
        /// <returns>The string list, or null.</returns>
        public List<string> ReadStringList(string path)
        {
            List<object> data = ReadList(path);
            if (data == null)
            {
                return null;
            }
            List<string> ndata = new List<string>(data.Count);
            for (int i = 0; i < data.Count; i++)
            {
                ndata.Add(data[i] + "");
            }
            return ndata;
        }

        /// <summary>
        /// Reads a path as a list of objects.
        /// </summary>
        /// <param name="path">The path to read.</param>
        /// <returns>The object list, or null.</returns>
        public List<object> ReadList(string path)
        {
            string[] data = path.SplitFast('.');
            int i = 0;
            Dictionary<object, object> obj = Data;
            while (i < data.Length - 1)
            {
                object nobj = obj.ContainsKey(data[i]) ? obj[data[i]] : null;
                if (nobj == null || !(nobj is Dictionary<object, object>))
                {
                    return null;
                }
                obj = (Dictionary<object, object>)nobj;
                i++;
            }
            if (!obj.ContainsKey(data[i]) || !(obj[data[i]] is List<string> || obj[data[i]] is List<object>))
            {
                return null;
            }
            if (obj[data[i]] is List<object>)
            {
                List<object> objs = (List<object>)obj[data[i]];
                return objs;
            }
            return null;
        }

        /// <summary>
        /// Reads a path as a single-precision floating point number.
        /// </summary>
        /// <param name="path">The path to read.</param>
        /// <param name="def">The default value to return if the path is invalid.</param>
        /// <returns>The value.</returns>
        public float ReadFloat(string path, float def)
        {
            return (float)ReadDouble(path, def);
        }

        /// <summary>
        /// Reads a path as a double-precision floating point number.
        /// </summary>
        /// <param name="path">The path to read.</param>
        /// <param name="def">The default value to return if the path is invalid.</param>
        /// <returns>The value.</returns>
        public double ReadDouble(string path, double def)
        {
            string[] data = path.SplitFast('.');
            int i = 0;
            object obj = Data;
            while (i < data.Length - 1)
            {
                // TODO: TryGetValue?
                object nobj = ((Dictionary<object, object>)obj).ContainsKey(data[i]) ? ((Dictionary<object, object>)obj)[data[i]] : null;
                if (nobj == null || !(nobj is Dictionary<object, object>))
                {
                    return def;
                }
                obj = nobj;
                i++;
            }
            if (!((Dictionary<object, object>)obj).ContainsKey(data[i]))
            {
                return def;
            }
            Object iobj = ((Dictionary<object, object>)obj)[data[i]];
            if (iobj is Double)
            {
                return (Double)iobj;
            }
            if (iobj is Single)
            {
                return (Single)iobj;
            }
            if (iobj is Int64)
            {
                return (double)((Int64)iobj);
            }
            if (iobj is Int32)
            {
                return (double)((Int32)iobj);
            }
            double xtemp;
            if (double.TryParse(iobj.ToString(), out xtemp))
            {
                return xtemp;
            }
            return def;
        }

        /// <summary>
        /// Reads a path as a 32-bit integer number.
        /// </summary>
        /// <param name="path">The path to read.</param>
        /// <param name="def">The default value to return if the path is invalid.</param>
        /// <returns>The value.</returns>
        public int ReadInt(string path, int def)
        {
            return (int)ReadLong(path, def);
        }

        /// <summary>
        /// Reads a path as a 64-bit integer number.
        /// </summary>
        /// <param name="path">The path to read.</param>
        /// <param name="def">The default value to return if the path is invalid.</param>
        /// <returns>The value.</returns>
        public long ReadLong(string path, long def)
        {
            string[] data = path.SplitFast('.');
            int i = 0;
            object obj = Data;
            while (i < data.Length - 1)
            {
                // TODO: TryGetValue?
                object nobj = ((Dictionary<object, object>)obj).ContainsKey(data[i]) ? ((Dictionary<object, object>)obj)[data[i]] : null;
                if (nobj == null || !(nobj is Dictionary<object, object>))
                {
                    return def;
                }
                obj = nobj;
                i++;
            }
            if (!((Dictionary<object, object>)obj).ContainsKey(data[i]))
            {
                return def;
            }
            Object iobj = ((Dictionary<object, object>)obj)[data[i]];
            if (iobj is Int64)
            {
                return (Int64)iobj;
            }
            if (iobj is Int32)
            {
                return (Int32)iobj;
            }
            if (iobj is Double)
            {
                return (long)((Double)iobj);
            }
            if (iobj is Single)
            {
                return (long)((Single)iobj);
            }
            long xtemp;
            if (long.TryParse(iobj.ToString(), out xtemp))
            {
                return xtemp;
            }
            return def;
        }

        /// <summary>
        /// Reads a path as a string.
        /// </summary>
        /// <param name="path">The path to read.</param>
        /// <param name="def">The default value to return if the path is invalid.</param>
        /// <returns>The value.</returns>
        public string ReadString(string path, string def)
        {
            object obj = Read(path, def);
            if (obj == null)
            {
                return def;
            }
            return obj.ToString();
        }

        /// <summary>
        /// Reads a path as an object.
        /// </summary>
        /// <param name="path">The path to read.</param>
        /// <param name="def">The default value to return if the path is invalid.</param>
        /// <returns>The value.</returns>
        public object Read(string path, object def)
        {
            string[] data = path.SplitFast('.');
            int i = 0;
            object obj = Data;
            while (i < data.Length - 1)
            {
                // TODO: TryGetValue?
                object nobj = ((Dictionary<object, object>)obj).ContainsKey(data[i]) ? ((Dictionary<object, object>)obj)[data[i]] : null;
                if (nobj == null || !(nobj is Dictionary<object, object>))
                {
                    return def;
                }
                obj = nobj;
                i++;
            }
            if (!((Dictionary<object, object>)obj).ContainsKey(data[i]))
            {
                return def;
            }
            return ((Dictionary<object, object>)obj)[data[i]];
        }

        /// <summary>
        /// Returns whether the key at the path exists.
        /// </summary>
        /// <param name="path">The base path, can be null.</param>
        /// <param name="key">The key to use.</param>
        /// <returns>Whether the key exists.</returns>
        public bool HasKey(string path, string key)
        {
            return GetKeys(path).Contains(key);
        }

        /// <summary>
        /// Gets a list of all keys at the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>The list of all keys.</returns>
        public List<string> GetKeys(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                List<string> atemp = new List<string>();
                foreach (object xtobj in Data.Keys)
                {
                    atemp.Add(xtobj + "");
                }
                return atemp;
            }
            string[] data = path.SplitFast('.');
            int i = 0;
            object obj = Data;
            while (i < data.Length - 1)
            {
                object nobj = ((Dictionary<object, object>)obj).ContainsKey(data[i]) ? ((Dictionary<object, object>)obj)[data[i]] : null;
                if (nobj == null || !(nobj is Dictionary<object, object>))
                {
                    return new List<string>();
                }
                obj = nobj;
                i++;
            }
            if (!((Dictionary<object, object>)obj).ContainsKey(data[i]))
            {
                return new List<string>();
            }
            object tobj = ((Dictionary<object, object>)obj)[data[i]];
            if (tobj is Dictionary<object, object>)
            {
                Dictionary<object, object>.KeyCollection objs = ((Dictionary<object, object>)tobj).Keys;
                List<string> toret = new List<string>();
                foreach (object o in objs)
                {
                    toret.Add(o + "");
                }
                return toret;
            }
            if (!(tobj is Dictionary<string, object>))
            {
                return new List<string>();
            }
            List<string> temp = new List<string>();
            foreach (object xtobj in ((Dictionary<object, object>)tobj).Keys)
            {
                temp.Add(xtobj + "");
            }
            return temp;
        }
        
        /// <summary>
        /// Gets a section of this YAML configuration.
        /// </summary>
        /// <param name="path">The base path to use.</param>
        /// <returns>The section.</returns>
        public YAMLConfiguration GetConfigurationSection(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return new YAMLConfiguration(Data);
            }
            string[] data = path.SplitFast('.');
            int i = 0;
            object obj = Data;
            while (i < data.Length - 1)
            {
                // TODO: TryGetValue?
                object nobj = ((Dictionary<object, object>)obj).ContainsKey(data[i]) ? ((Dictionary<object, object>)obj)[data[i]] : null;
                if (nobj == null || !(nobj is Dictionary<object, object>))
                {
                    return null;
                }
                obj = nobj;
                i++;
            }
            if (!((Dictionary<object, object>)obj).ContainsKey(data[i]))
            {
                return null;
            }
            object tobj = ((Dictionary<object, object>)obj)[data[i]];
            if (!(tobj is Dictionary<object, object>))
            {
                return null;
            }
            return new YAMLConfiguration((Dictionary<object, object>)tobj);
        }

        /// <summary>
        /// Sets an object at a path if the path does not exist yet.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="val">The object.</param>
        public void Default(string path, object val)
        {
            string[] data = path.SplitFast('.');
            int i = 0;
            object obj = Data;
            while (i < data.Length - 1)
            {
                object nobj = ((Dictionary<object, object>)obj).ContainsKey(data[i]) ? ((Dictionary<object, object>)obj)[data[i]] : null;
                if (nobj == null || !(nobj is Dictionary<object, object>))
                {
                    nobj = new Dictionary<object, object>();
                    ((Dictionary<object, object>)obj)[data[i]] = nobj;
                }
                obj = nobj;
                i++;
            }
            if (!((Dictionary<object, object>)obj).ContainsKey(data[i]))
            {
                ((Dictionary<object, object>)obj)[data[i]] = val;
                if (Changed != null)
                {
                    Changed.Fire(new EventArgs());
                }
            }
        }

        /// <summary>
        /// Force-sets an object at a path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="val">The object.</param>
        public void Set(string path, object val)
        {
            string[] data = path.SplitFast('.');
            int i = 0;
            object obj = Data;
            while (i < data.Length - 1)
            {
                object nobj = ((Dictionary<object, object>)obj).ContainsKey(data[i]) ? ((Dictionary<object, object>)obj)[data[i]] : null;
                if (nobj == null || !(nobj is Dictionary<object, object>))
                {
                    nobj = new Dictionary<object, object>();
                    ((Dictionary<object, object>)obj)[data[i]] = nobj;
                }
                obj = nobj;
                i++;
            }
            if (val == null)
            {
                ((Dictionary<object, object>)obj).Remove(data[i]);
            }
            else
            {
                ((Dictionary<object, object>)obj)[data[i]] = val;
            }
            if (Changed != null)
            {
                Changed.Fire(new EventArgs());
            }
        }

        /// <summary>
        /// Fired when the YAMLConfiguration changes.
        /// </summary>
        public FreneticScriptEventHandler<EventArgs> Changed;

        /// <summary>
        /// Saves the entire YAML configuration to a file-ready string.
        /// </summary>
        /// <returns>The string.</returns>
        public string SaveToString()
        {
            Serializer ser = new Serializer(SerializationOptions.EmitDefaults);
            StringWriter sw = new StringWriter();
            ser.Serialize(sw, Data);
            return sw.ToString();
        }
    }
}
