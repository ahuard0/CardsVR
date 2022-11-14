using UnityEngine;

namespace CardsVR.Commands
{
    /*
     *  This HandData class is serializable and capable of transmittal 
     *  over PUN using PhotonNetwork.RaiseEvent.
     *  
     *  This class represents the information necessary for clients to 
     *  create a Remote Avatar of the local player.
     */
    public class HandData : ICommandData
    {
        public static readonly byte EventID = 47;

        public enum HandSide { Left, Right };

        public byte getEventID { get { return EventID; } }

        /*
         *      Constructor for the HandData class.
         *      
         *      Parameters
         *      ----------
         *      PlayerID : byte
         *          The playerID number, which is an integer from 1-4.
         *      Side : HandSide
         *          The hand side, either left or right.
         *      BasePosition : Vector3
         *          Ttranslation of the hand relative to the global origin.
         *      BaseRotation : Quaternion
         *          Rotation of the hand relative to the global origin.
         */
        public HandData(byte PlayerID, HandSide Side, Vector3 BasePosition, Quaternion BaseRotation)
        {
            this.Side = Side;
            this.PlayerID = PlayerID;
            this.BasePosition = BasePosition;
            this.BaseRotation = BaseRotation;
        }
        public HandData(int PlayerID, HandSide Side, Vector3 BasePosition, Quaternion BaseRotation)
        {
            this.Side = Side;
            this.PlayerID = (byte)PlayerID;
            this.BasePosition = BasePosition;
            this.BaseRotation = BaseRotation;
        }

        /*
         *      An enum property representing the hand side, either left or right.
         *      
         *      Accessor
         *      -------
         *      Side : enum HandSide
         *          Denotes either the left or right hand.
         */
        public HandSide Side { get; set; }

        /*
         *      The base position vector.
         *      
         *      Accessor
         *      -------
         *      Position : Vector3
         *          A vector representing the base position in global cartesian coordinates.
         */
        public Vector3 BasePosition { get; set; }

        /*
         *      The base rotation quaternion.
         *      
         *      Accessor
         *      -------
         *      Quaternion : Vector3
         *          A quaternion representing the base rotation in the global reference axis.
         */
        public Quaternion BaseRotation { get; set; }

        /*
         *      The player's number, which denotes their position at the card table.
         *      
         *      Accessor
         *      -------
         *      PlayerID : byte
         *          The player number (1-4).
         */
        public byte PlayerID { get; set; }

        /*
         *      Returns the string equivalent of the PlayerID, Side, Position, 
         *      and Rotation, which represent this class.
         *      
         *      Parameters
         *      ----------
         *      None
         *      
         *      Returns
         *      -------
         *      output : string
         *          A visual representation of the contents of this class.  
         *          Used for debugging or inspection.
         */
        public override string ToString()
        {
            return "PlayerID: " + PlayerID.ToString() + ", Side: " + Side.ToString() + ", BasePosition: " + BasePosition.ToString() + ", BaseRotation: " + BaseRotation.ToString();
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
         *          and the remaining elements are the PlayerID, Side, BasePosition,
         *          and BaseRotation.
         */
        public object[] ToObjectArray()
        {
            object[] data = new object[5];
            data[0] = EventID;
            data[1] = PlayerID;
            data[2] = Side;
            data[3] = BasePosition;
            data[4] = BaseRotation;
            return data;
        }

        /*
         *      Reconstructs a HandData class object from an Object array.
         *      This static method reverses the action taken by the 
         *      ToObjectArray() method.
         *      
         *      Parameters
         *      ----------
         *      data : object[]
         *          An object array.  The first element is the EventID property, 
         *          and the remaining elements are the PlayerID, Side, BasePosition,
         *          and BaseRotation.
         *      
         *      Returns
         *      -------
         *      output : HandData
         *          A HandData class object populated with data from the object
         *          array.
         */
        public static HandData FromObjectArray(object[] data)
        {
            byte ID = (byte)data[0];
            if (ID == HandData.EventID)
            {
                byte PlayerID = (byte)data[1];
                HandSide Side = (HandSide)data[2];
                Vector3 BasePosition = (Vector3)data[3];
                Quaternion BaseRotation = (Quaternion)data[4];
                return new HandData(PlayerID, Side, BasePosition, BaseRotation);
            }
            else
            {
                Debug.LogErrorFormat("Event ID {0} does not match", ID);
                return null;
            }
        }
    }
}
