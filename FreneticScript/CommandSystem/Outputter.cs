using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FreneticScript.CommandSystem
{
    /// <summary>
    /// An abstract class, implementations should provide methods that output to a console or equivalent.
    /// </summary>
    public abstract class Outputter
    {
        /// <summary>
        /// Writes a line of text to the console.
        /// </summary>
        /// <param name="text">The line of text.</param>
        public abstract void WriteLine(string text);

        /// <summary>
        /// Used to output a failure message.
        /// </summary>
        /// <param name="tagged_text">The text to output, with tags included.</param>
        /// <param name="mode">What debug mode is currently in use.</param>
        public abstract void Bad(string tagged_text, DebugMode mode);

        /// <summary>
        /// Used to output a success message.
        /// </summary>
        /// <param name="tagged_text">The text to output, with tags included.</param>
        /// <param name="mode">What debug mode is currently in use.</param>
        public abstract void Good(string tagged_text, DebugMode mode);

        /// <summary>
        /// Used to properly handle an unknown command.
        /// </summary>
        /// <param name="queue">The queue firing this unknown command.</param>
        /// <param name="basecommand">The command that wasn't recognized.</param>
        /// <param name="arguments">The commands arguments.</param>
        public abstract void UnknownCommand(CommandQueue queue, string basecommand, string[] arguments);

        /// <summary>
        /// The CVar System used by this command engine.
        /// </summary>
        public CVarSystem CVarSys;

        /// <summary>
        /// Used to read a text file, generally a script.
        /// File format is along the lines of "mymap/myscript.cfg".
        /// Throw a System.IO.FileNotFoundException if file does not exist.
        /// </summary>
        /// <param name="name">The filename to read.</param>
        /// <returns>The read text file.</returns>
        public abstract string ReadTextFile(string name);

        /// <summary>
        /// Used to read a text file, generally generic data for a script.
        /// File format is along the lines of "mymap/mydata.yml".
        /// Throw a System.IO.FileNotFoundException if file does not exist.
        /// </summary>
        /// <param name="name">The filename to read.</param>
        /// <returns>The read data file.</returns>
        public abstract byte[] ReadDataFile(string name);

        /// <summary>
        /// Used to write a text file, generally generic data for a script.
        /// File format is along the lines of "mymap/mydata.yml".
        /// </summary>
        /// <param name="name">The filename to write.</param>
        /// <param name="data">The data to write to file.</param>
        public abstract void WriteDataFile(string name, byte[] data);

        /// <summary>
        /// Used when the system is reloaded, to delete any temporary script-related data.
        /// </summary>
        public abstract void Reload();

        /// <summary>
        /// Whether the game is still setting up currently. (Used by the InitOnly CVar system).
        /// </summary>
        public bool Initializing = false;

        /// <summary>
        /// Whether the system should error when an invalid command is detected.
        /// </summary>
        public abstract bool ShouldErrorOnInvalidCommand();
    }
}
