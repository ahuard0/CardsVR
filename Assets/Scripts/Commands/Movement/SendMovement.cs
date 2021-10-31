namespace CardsVR.Commands
{
    /*
     *      A Command that interfaces with a Movement Sender to handle movements sent over PUN.
     *      
     *      A MovementSender is a "receiver" type class following the Command design pattern.
     *      
     *      A Movement is a custom class that is fully serializable and can transfer arbitrary data
     *      in accordance with the Movement class structure.
     */
    public class SendMovement: Command
    {
        public Movement movement;

        /*
         *      Constructor for the SendMovement class.
         *      
         *      Parameters
         *      ----------
         *      movement : Movement
         *          The Movement object to be sent over the PUN network to all connected clients within a Room.
         */
        public SendMovement(Movement movement)
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
            MovementSender.Broadcast(movement);  // method defined by the MovementSender (receiver class) in a Command design pattern
        }
    }
}
