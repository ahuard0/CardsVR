using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using CardsVR.Utility;

namespace CardsVR.Commands
{
    /*
     *      The Command Recorder logs the commands executed by the Invoker for later Serialization to a 
     *      file if desired.
     *      
     *      The command recorder is a Singleton because commands may be recorded across multiple scenes.
     *      The Singleton design pattern preserves this object across scenes.
     */
    public class CommandRecorder: Singleton<CommandRecorder>
    {
        public TupleList<float, Command> RecordedCommands = new TupleList<float, Command>();
        private DateTime startTime = DateTime.MinValue;

        /*
         *      Records a command.  The recorded command is added to a SortedList data structure, which
         *      may be serialized (saved) to a binary file or replayed with the same order and timing
         *      as when the commands were recorded.
         *      
         *      Parameters
         *      ----------
         *      command : Command
         *          The command, which may be an extended Command object (e.g., SendMessage : Command)  
         *      
         *      Returns
         *      -------
         *      None
         */
        public void Record(Command command)
        {
            if (startTime == DateTime.MinValue)
                startTime = DateTime.UtcNow;

            TimeSpan span = DateTime.UtcNow - startTime;
            float timestamp = (float)span.TotalSeconds;

            RecordedCommands.Add(timestamp, command);
        }

        /*
         *      Save the recorded commands to a binary data file.
         *      
         *      Parameters
         *      ----------
         *      savepath : string
         *          The path to save the binary command data file (e.g., commands.dat).
         *      
         *      Returns
         *      -------
         *      None
         */
        public void Save(string savepath)
        {
            using (FileStream fs = new FileStream(savepath, FileMode.Create, FileAccess.Write))
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(fs, RecordedCommands);
            }
        }

        /*
         *      Load the recorded commands from a binary data file.
         *      
         *      Parameters
         *      ----------
         *      loadpath : string
         *          The path to a binary command data file (e.g., commands.dat).
         *      
         *      Returns
         *      -------
         *      None
         */
        public void Load(string loadpath)
        {
            using (FileStream fs = new FileStream(loadpath, FileMode.Open, FileAccess.Read))
            {
                BinaryFormatter bf = new BinaryFormatter();
                RecordedCommands = (TupleList<float, Command>)bf.Deserialize(fs);
            }
        }

        /*
         *      Clear and reset the Command Recorder.
         *      
         *      Parameters
         *      ----------
         *      None
         *      
         *      Returns
         *      -------
         *      None
         */
        public void Clear()
        {
            startTime = DateTime.MinValue;
            RecordedCommands.Clear();
        }

        /*
         *      Returns the number of commands stored.
         *      
         *      Parameters
         *      ----------
         *      None
         *      
         *      Returns
         *      -------
         *      Count : int
         *          The number of stored commands.
         */
        public int Count()
        {
            return RecordedCommands.Count;
        }
    }
}
