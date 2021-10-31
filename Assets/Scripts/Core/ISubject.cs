namespace CardsVR
{
    /*
     *      The Subject interface implements the Subject/Observer 
     *      design pattern.  It is generally used by manager classes 
     *      to notify observers when a change in state occurs.  For 
     *      example, the Connection Manager is a subject that will 
     *      notify the Lobby Connection Observer when a change in the 
     *      connection state occurs so that GUI elements can be updated.
     */
    public interface ISubject
    {
        /*
         *      AttachObserver registers an observer class with the subject concrete class.
         *      An example subject concrete class is the ConnectionManager singleton static class.
         *      
         *      Parameters
         *      ----------
         *      observer : IObserver
         *          The observer to be registered with this subject class.
         *      
         *      Returns
         *      -------
         *      None
         */
        public void AttachObserver(IObserver observer);

        /*
         *      DetachObserver unregisters an observer class from the subject concrete class.
         *      An example subject concrete class is the ConnectionManager singleton static class.
         *      
         *      This removes the reference to the observer in the subject concrete class so that 
         *      garbage collection can free memory
         *      
         *      Parameters
         *      ----------
         *      observer : IObserver
         *          The observer to be unregistered from this subject class.
         *      
         *      Returns
         *      -------
         *      None
         */
        public void DetachObserver(IObserver observer);

        /*
         *      This method individually notifies each observer by calling each observer's 
         *      Notify callback function.
         *      
         *      Parameters
         *      ----------
         *      None
         *      
         *      Returns
         *      -------
         *      None
         */
        public void NotifyObservers();
    }
}
