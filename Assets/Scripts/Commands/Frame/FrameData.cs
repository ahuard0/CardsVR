using CardsVR.Detection;
using UnityEngine;

namespace CardsVR.Commands
{
    /*
     *  This FrameData class is serializable and capable of transmittal over PUN using PhotonNetwork.RaiseEvent.
     *  
     *  This class represents the camera parameters and detected Aruco tags within a frame.
     */
    public class FrameData : ICommandData
    {
        public static readonly byte EventID = 50;

        public byte getEventID { get { return EventID; } }

        /*
         *      Constructor for the FrameData class.
         *      
         *      Parameters
         *      ----------
         *      CameraID : string
         *          A unique ID representing the camera used.  Required.
         *      TagIDs : Stack<int>
         *          A stack of unique IDs representing the tags.  Required.
         */
        public FrameData(ArucoData ArucoData, int PlayerID, string PlayerNick)
        {
            this.ArucoData = ArucoData;
            this.PlayerID = PlayerID;
            this.PlayerNick = PlayerNick;
        }

        /*
         *      The frame's Aruco Tag Data.
         *      
         *      Accessor
         *      -------
         *      ArucoData : ArucoState
         *          An object containing Aruco Tag information including ID and corner position in the frame.
         */
        public ArucoData ArucoData
        {
            get; set;
        }

        /*
         *      The unique ID of the Player.
         *      
         *      Accessor
         *      -------
         *      PlayerID : int
         *          A unique ID representing the player.
         */
        public int PlayerID
        {
            get; set;
        }

        /*
         *      The PUN nickname of the player.
         *      
         *      Accessor
         *      -------
         *      PlayerNick : string
         *          The Player's string identifier.  Camera players correspond to the VR player except a "B" suffix is added to the end of the player name.
         */
        public string PlayerNick
        {
            get; set;
        }

        /*
         *      Returns the CameraID and TagIDs, which represent this class.
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
            return "TagIDs: " + ArucoData.ToString() + ", PlayerNick: " + PlayerNick + ", PlayerID: " + PlayerID.ToString();
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
         *          and the remaining elements are the ArucoData basic members 
         *          (corners, TagUniqueID, width, height), the PlayerID, and the 
         *          PlayerNick.
         */
        public object[] ToObjectArray()
        {
            float[] corners = ArucoData.corners;
            int[] uniqueIDs = ArucoData.uniqueIDs;
            int width = ArucoData.width;
            int height = ArucoData.height;

            object[] data = new object[7];
            data[0] = EventID;
            data[1] = PlayerNick;
            data[2] = PlayerID;
            data[3] = width;
            data[4] = height;
            data[5] = uniqueIDs;
            data[6] = corners;
            return data;
        }

        /*
         *      Reconstructs a FrameData class object from an Object array.
         *      This static method reverses the action taken by the 
         *      ToObjectArray() method.
         *      
         *      Parameters
         *      ----------
         *      data : object[]
         *          An object array.  The first element is the EventID property, 
         *          and the remaining elements are the ArucoData basic members 
         *          (corners, TagUniqueID, width, height), the PlayerID, and the 
         *          PlayerNick.
         *      
         *      Returns
         *      -------
         *      output : FrameData
         *          A FrameData class object populated with data from the object
         *          array.
         */
        public static FrameData FromObjectArray(object[] data)
        {
            byte ID = (byte)data[0];
            if (ID == FrameData.EventID)
            {
                string PlayerNick = (string)data[1];
                int PlayerID = (int)data[2];
                int width = (int)data[3];
                int height = (int)data[4];
                int[] uniqueIDs = (int[])data[5];
                float[] corners = (float[])data[6];
                ArucoData ArucoData = new ArucoData(corners, uniqueIDs, width, height);
                return new FrameData(ArucoData, PlayerID, PlayerNick);
            }
            else
            {
                Debug.LogErrorFormat("Event ID {0} does not match", ID);
                return null;
            }
        }
    }
}
