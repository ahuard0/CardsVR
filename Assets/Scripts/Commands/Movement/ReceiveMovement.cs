namespace CardsVR.Commands
{
    /*
     *      A Command that interfaces with a Movement Receiver to handle received movements over PUN.
     *      
     *      A MovementReceiver is a "receiver" type class following the Command design pattern.
     *      
     *      A Movement is a custom class that is fully serializable and can transfer arbitrary data
     *      in accordance with the Movement class structure.
     */
    public class ReceiveMovement: Command
    {
        public Movement movement;

        /*
         *      Constructor for the ReceiveMovement class.
         *      
         *      Parameters
         *      ----------
         *      movement : Movement
         *          The Movement object received within the OnEvent PUN callback in MovementClient.
         */
        public ReceiveMovement(Movement movement)
        {
            this.movement = movement;
        }

        /*
         *      Executes the command.  This is a required interface method for all commands.
         *      
         *      Parameters
         *      ----------
         *      None
         *      
         *      Returns
         *      -------
         *      None
         */
        public override void Execute()
        {
            MovementReceiver.Move(movement);  // method defined by the MovementReceiver (receiver class) in a Command design pattern
        }
    }
}
