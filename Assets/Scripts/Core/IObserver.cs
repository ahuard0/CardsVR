namespace CardsVR
{
    /*
     *      The Observer interface implements the Subject/Observer 
     *      design pattern.  It is generally used by manager classes 
     *      to notify observers when a change in state occurs.  For 
     *      example, the Connection Manager is a subject that will 
     *      notify the Lobby Connection Observer when a change in the 
     *      connection state occurs so that GUI elements can be updated.
     */
    public interface IObserver
    {
        /*
         *      Notify the Observer when a change in the subject state occurs.  
         *      The subject calls this function explicitly whenever it wishes 
         *      to notify observers to take action.
         *      
         *      Parameters
         *      ----------
         *      None
         *      
         *      Returns
         *      -------
         *      None
         */
        public void Notify();
    }
}