//
// This file is part of FreneticScript, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of the MIT license.
// See README.md or LICENSE.txt in the FreneticScript source root for the contents of the license.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FreneticScript.CommandSystem
{
    /// <summary>
    /// An abstract class for the script engine to interface properly with the context it is running in.
    /// </summary>
    public abstract class ScriptEngineContext
    {
        /// <summary>
        /// Writes a line of text to the console or text window. Used for informational output.
        /// </summary>
        /// <param name="text">The line of text.</param>
        public abstract void WriteLine(string text);

        /// <summary>
        /// Used to output a failure message.
        /// </summary>
        /// <param name="text">The text to output.</param>
        public virtual void BadOutput(string text)
        {
            WriteLine(TextStyle.Outbad + "[Bad] " + text);
        }

        /// <summary>
        /// Used to output a success message.
        /// </summary>
        /// <param name="text">The text to output.</param>
        public virtual void GoodOutput(string text)
        {
            WriteLine(TextStyle.Outgood + "[Good] " + text);
        }

        /// <summary>
        /// Called to properly handle an unknown command.
        /// Only used if <see cref="ShouldErrorOnInvalidCommand"/> returns true.
        /// </summary>
        /// <param name="queue">The queue firing this unknown command.</param>
        /// <param name="basecommand">The command that wasn't recognized.</param>
        /// <param name="arguments">The commands arguments.</param>
        public virtual void UnknownCommand(CommandQueue queue, string basecommand, string[] arguments)
        {
            throw new NotImplementedException("Unknown command: " + basecommand);
        }

        /// <summary>
        /// The CVar System used by this context.
        /// </summary>
        public CVarSystem CVarSys;

        /// <summary>
        /// Used to read a text file, generally a script.
        /// File format is along the lines of "mymap/myscript.frs".
        /// Throw a <see cref="System.IO.FileNotFoundException"/> if file does not exist.
        /// </summary>
        /// <param name="name">The filename to read.</param>
        /// <returns>The read text file.</returns>
        public abstract string ReadTextFile(string name);

        /// <summary>
        /// Used to read a text file, generally generic data for a script.
        /// File format is along the lines of "mymap/mydata.fds".
        /// Throw a <see cref="System.IO.FileNotFoundException"/> if file does not exist.
        /// </summary>
        /// <param name="name">The filename to read.</param>
        /// <returns>The read data file.</returns>
        public abstract byte[] ReadDataFile(string name);

        /// <summary>
        /// Used to write a text file, generally generic data for a script.
        /// File format is along the lines of "mymap/mydata.fds".
        /// </summary>
        /// <param name="name">The filename to write.</param>
        /// <param name="data">The data to write to file.</param>
        public abstract void WriteDataFile(string name, byte[] data);

        /// <summary>
        /// Called to produce a list of valid files starting at a given folder path, with an optional extension rule.
        /// The extension may be null (in which case, assume all file types are valid to be listed).
        /// <para>An example call may be of the form: <code>ListFiles("scripts", "frs", true)</code>.
        /// That call would be expected to return all script files in the scripts folder, and in sub-folders of the scripts folder.</para>
        /// </summary>
        /// <param name="path">The initial folder path.</param>
        /// <param name="extension">A file extension requirement, or null if none.</param>
        /// <param name="deep">Whether to read within subfolders as well.</param>
        /// <returns>A list of file names.</returns>
        public abstract string[] ListFiles(string path, string extension, bool deep);

        /// <summary>
        /// Called when the system is reloaded, to delete any temporary script-related data.
        /// </summary>
        public abstract void Reload();

        /// <summary>
        /// Called when the system has finished reloading.
        /// </summary>
        public virtual void PostReload()
        {
        }

        /// <summary>
        /// Whether the game is still setting up currently. (Used by the InitOnly CVar system).
        /// </summary>
        public bool Initializing = false;

        /// <summary>
        /// Whether the system should error when an invalid command is detected.
        /// </summary>
        public virtual bool ShouldErrorOnInvalidCommand()
        {
            return true;
        }
    }
}
