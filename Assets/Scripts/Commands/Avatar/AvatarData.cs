using UnityEngine;

namespace CardsVR.Commands
{
    /*
     *  This AvatarData class is serializable and capable of transmittal over PUN using PhotonNetwork.RaiseEvent.
     *  
     *  This class represents the information necessary for clients to create a Remote Avatar of the local player.
     */
    public class AvatarData : ICommandData
    {
        public static readonly byte EventID = 46;

        public byte getEventID { get { return EventID; } }

        /*
         *      Constructor for the AvatarData class.
         *      
         *      Parameters
         *      ----------
         *      FrameData : byte[]
         *          The avatar frame data returned by the SDK.  Required.
         *      FrameDataSize : uint
         *          The number of bytes represented by FrameData.  Required.
         */
        public AvatarData(byte[] FrameData, uint FrameDataSize, byte PlayerID)
        {
            this.FrameData = FrameData;
            this.FrameDataSize = FrameDataSize;
            this.PlayerID = PlayerID;
        }

        /*
         *      A byte sequence representing an avatar frame as returned by the Oculus Avatar SDK.
         *      
         *      Accessor
         *      -------
         *      FrameData : byte[]
         *          Byte sequence representing an avatar frame.
         */
        public byte[] FrameData
        {
            get; set;
        }

        /*
         *      The size of the byte sequence representing an avatar frame.
         *      
         *      Accessor
         *      -------
         *      FrameDataSize : uint
         *          The number of bytes in the sequence.
         */
        public uint FrameDataSize
        {
            get; set;
        }

        /*
         *      The player's number, which denotes their position at the card table.
         *      
         *      Accessor
         *      -------
         *      PlayerID : byte
         *          The player number (1-4).
         */
        public byte PlayerID
        {
            get; set;
        }

        /*
         *      Returns the FrameDataSize and the recorded length of FrameData, which represent this class.
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
            return "PlayerID: " + PlayerID.ToString() + ", FrameDataSize: " + FrameDataSize.ToString() + ", Length of FrameData: " + FrameData.Length.ToString();
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
         *          and the remaining elements are the PlayerID, FrameDataSize and FrameData.
         */
        public object[] ToObjectArray()
        {
            object[] data = new object[4];
            data[0] = EventID;
            data[1] = PlayerID;
            data[2] = (int)FrameDataSize;
            data[3] = FrameData;
            return data;
        }

        /*
         *      Reconstructs a AvatarData class object from an Object array.
         *      This static method reverses the action taken by the 
         *      ToObjectArray() method.
         *      
         *      Parameters
         *      ----------
         *      data : object[]
         *          An object array.  The first element is the EventID property, 
         *          and the remaining elements are the PlayerID, FrameDataSize and FrameData.
         *      
         *      Returns
         *      -------
         *      output : AvatarData
         *          A AvatarData class object populated with data from the object
         *          array.
         */
        public static AvatarData FromObjectArray(object[] data)
        {
            byte ID = (byte)data[0];
            if (ID == AvatarData.EventID)
            {
                byte PlayerID = (byte)data[1];
                int frameDataSize = (int)data[2];
                byte[] frameData = (byte[])data[3];
                return new AvatarData(frameData, (uint)frameDataSize, PlayerID);
            }
            else
            {
                Debug.LogErrorFormat("Event ID {0} does not match", ID);
                return null;
            }
        }
    }
}
