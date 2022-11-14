using UnityEngine;

namespace CardsVR.Commands
{
    /*
     *  Requests a Broadcast in card state from a remote user. 
     */
    public class RequestCardData : ICommandData
    {
        public static readonly byte EventID = 54;

        public byte getEventID { get { return EventID; } }

        public int OwnerID;
        public int RequesterID;

        /*
         *      Constructor for the RequestCardData class.
         *      
         *      Parameters
         *      ----------
         *      OwnerID : int
         *          The PlayerID of the card owner.
         *      RequesterID : int
         *          The PlayerID requesting the data.
         */
        public RequestCardData(int OwnerID, int RequesterID)
        {
            this.OwnerID = OwnerID;
            this.RequesterID = RequesterID;
        }

        /*
         *      Returns a string representing this class.
         *      
         *      Parameters
         *      ----------
         *      None
         *      
         *      Returns
         *      -------
         *      output : string
         *          A visual representation of the contents of this class.  Used for debugging or inspection.
         */
        public override string ToString()
        {
            return "OwnerID: " + OwnerID.ToString() + ", RequesterID: " + RequesterID.ToString();
        }

        /*
         *      Converts this class into an object array of its properties.
         *      
         *      Parameters
         *      ----------
         *      None
         *      
         *      Returns
         *      -------
         *      output : object[]
         *          An object array.  The first element is the EventID property, 
         *          and the remaining elements are the properties of this class.
         */
        public object[] ToObjectArray()
        {
            object[] data = new object[3];
            data[0] = EventID;
            data[1] = OwnerID;
            data[2] = RequesterID;
            return data;
        }

        /*
         *      Reconstructs a RequestCardData class object from an Object array.
         *      This static method reverses the action taken by the 
         *      ToObjectArray() method.
         *      
         *      Parameters
         *      ----------
         *      data : object[]
         *          An object array.  The first element is the EventID property, 
         *          and the remaining elements are the properties of this class.
         *      
         *      Returns
         *      -------
         *      output : RequestCardData
         *          A RequestCardData class object populated with data from the object
         *          array.
         */
        public static RequestCardData FromObjectArray(object[] data)
        {
            byte ID = (byte)data[0];
            if (ID == RequestCardData.EventID)
            {
                int OwnerID = (int)data[1];
                int RequesterID = (int)data[2];

                return new RequestCardData(OwnerID, RequesterID);
            }
            else
            {
                Debug.LogErrorFormat("Event ID {0} does not match", ID);
                return null;
            }
        }
    }
}
