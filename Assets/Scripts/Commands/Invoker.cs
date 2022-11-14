using System.Collections.Generic;
using UnityEngine;

namespace CardsVR.Commands
{
    /* 
     *  The Invoker allows recording and recall of commands executed
     *  through the invoker.  The Invoker also allows commands to be
     *  played back using Unity's FixedUpdate method as a timer.
     *  
     *  This Invoker is attached to a GameObject.  A reference to this
     *  Invoker component is then passed to the controller (Client)
     *  GameObject.
     */
    public class Invoker : Singleton<Invoker>
    {
        private Command _command;
        private float _replayTime = 0f;
        private TupleList<float, Command> _recordedCommands = new TupleList<float, Command>();  // temporary variable used for playback of commands

        [HideInInspector]
        public bool isReplaying = false;

        /*
         *      Sets the operating command of the invoker, but does not execute it.  Use ExecuteCommand()
         *      to execute the command.
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
        public void SetCommand(Command command)
        {
            _command = command;
        }

        /*
         *      Executes the current command.  Use SetCommand() to set the current operating command.
         *      
         *      Optionally, this method saves the command to the CommandRecorder static class.
         *      
         *      Parameters
         *      ----------
         *      record : bool
         *          Flag used to save executed commands in the CommandRecorder static class.
         *      
         *      Returns
         *      -------
         *      None
         */
        public void ExecuteCommand(bool record)
        {
            if (record)
                StoreCommand();
            _command.Execute();
        }

        /*
         *      Stores the current command.  Use SetCommand() to set the current operating command.
         *      
         *      This method saves the command to the CommandRecorder static class.
         *      
         *      Parameters
         *      ----------
         *      None
         *      
         *      Returns
         *      -------
         *      None
         */
        public void StoreCommand()
        {
            CommandRecorder.Instance.Record(_command);
        }

        /*
         *      Replay commands saved by the CommandRecorder in the same order and with the same 
         *      timing that originated the commands.
         *      
         *      Parameters
         *      ----------
         *      None
         *      
         *      Returns
         *      -------
         *      None
         */
        public void Replay()
        {
            if (CommandRecorder.Instance.RecordedCommands.Count <= 0)  // list of commands is empty, do not replay
                return;
            _recordedCommands = CommandRecorder.Instance.RecordedCommands;  // copy the recorded commands for playback, which modifies the list until all items are expended
            _replayTime = 0f;
            isReplaying = true;
        }

        /*
         *      A utility function used within FixedUpdate() to determine when the current time (a) matches the specified record time (b).
         *      
         *      Timing matches within a specified tolerance, which is usually set to Time.deltaTime within a Unity FixedUpdate() loop.
         *      
         *      Parameters
         *      ----------
         *      a : float
         *          The first parameter to compare for equality.
         *      b: float
         *          The second parameter to compare for equality.
         *      tolerance : float
         *          The specified tolerance (Epsilon) for equality comparison.  This is usually set to Time.deltaTime within a Unity FixedUpdate() loop.
         *      
         *      Returns
         *      -------
         *      output : bool
         *          Whether the two parameters, a and b, are equal to within specified tolerance.
         */
        private bool Approximately(float a, float b, float tolerance)
        {
            return (Mathf.Abs(a - b) <= tolerance);
        }

        /*
         *      A MonoBehavior interface used by Unity that executes with a fixed time interval, Time.deltaTime.
         *      
         *      This method is used for replaying recorded commands with the same time interval between those commands as when recorded.
         *      
         *      Parameters
         *      ----------
         *      None
         *      
         *      Returns
         *      -------
         *      None
         */
        private void FixedUpdate()
        {
            if (isReplaying)
            {
                if (_recordedCommands.Count > 0)
                {
                    _replayTime += Time.deltaTime;  // increment by FixedUpdate time interval
                    if (Approximately(_replayTime, _recordedCommands.Keys[0], Time.deltaTime))  // when timing of events synchronize, execute and remove command from the list
                    {
                        _recordedCommands.Values[0].Execute();
                        _recordedCommands.RemoveAt(0);
                    }
                }
                else
                {
                    isReplaying = false;  // reset and end playback when the list is empty
                }
            }
        }
    }
}
