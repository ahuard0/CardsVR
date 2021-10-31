namespace CardsVR.Commands
{
    /*
     *   MovementReceiver follows the Command design pattern as a receiver class.
     *   
     *   Receiver classes are implemented by the Execute() methods of the corresponding Command classes.
     *   There is no Receiver interface.
     *   
     *   MovementReceiver is called by the Execute method of the ReceiveMovement class.
     *   
     *   As a static class, the methods may be called independently of Command classes, such as during
     *   unit testing.
     */
    public static class MovementReceiver
    {
        public static IMovementClient client;  // client is registered in MovementClient

        /*
         *      Performs a callback to the Move() callback registered by MovementClient.
         *      Move() is a required method of the corresponding interface IMovementClient.
         *      
         *      The callback is necessary to access MonoBehavior functions such as Find() and other
         *      components associated with the Movement Client.  This also serves as a way to
         *      make the receiver component reusable.
         *      
         *      Parameters
         *      ----------
         *      movement : Movement
         *          Represents the movement of a GameObject object. This method is a callback 
         *          defined in MovementClient.
         *      
         *      Returns
         *      -------
         *      None
         */
        public static void Move(Movement movement)
        {
            if (client == null)
                return;
            client.Move(movement);
        }
    }
}
