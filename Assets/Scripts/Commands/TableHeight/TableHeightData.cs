using System;
using System.Collections.Generic;
using UnityEngine;

namespace CardsVR.Commands
{
    /*
     *  This TableHeightData class is serializable and capable of transmittal over PUN using PhotonNetwork.RaiseEvent.
     *  
     *  This class represents the position and rotation orientation of an object.
     */
    public class TableHeightData : ICommandData
    {
        public static readonly byte EventID = 48;

        public byte getEventID { get { return EventID; } }

        /*
         *      Constructor for the TableHeightData class.
         *      
         *      Parameters
         *      ----------
         *      PlayerID : int
         *          The player's unique ID.  Required.
         *      AbsoluteTableHeight : float
         *          The Y-Axis height of the Tabletop GameObject from the scene origin in world coordinates.  Required.
         */
        public TableHeightData(int PlayerID, float AbsoluteTableHeight)
        {
            this.PlayerID = PlayerID;
            this.AbsoluteTableHeight = AbsoluteTableHeight;
        }

        /*
         *      The unique ID assigned to the player.
         *      
         *      Accessor
         *      -------
         *      PlayerID : int
         *          A unique ID representing the player.  It is a positive number from 1 to 4.
         */
        public int PlayerID
        {
            get; set;
        }

        /*
         *      The height of the player's tabletop.
         *      
         *      Accessor
         *      -------
         *      AbsoluteTableHeight : float
         *          The Y-Axis height of the Tabletop GameObject.
         */
        public float AbsoluteTableHeight
        {
            get; set;
        }

        /*
         *      Returns the PlayerID and AbsoluteTableHeight, which represent this class.
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
            return "PlayerID: " + PlayerID.ToString() + ", AbsoluteTableHeight: " + AbsoluteTableHeight.ToString();
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
         *          and the remaining elements are the PlayerID and AbsoluteTableHeight numbers.
         */
        public object[] ToObjectArray()
        {
            object[] data = new object[3];
            data[0] = EventID;
            data[1] = PlayerID;
            data[2] = AbsoluteTableHeight;
            return data;
        }

        /*
         *      Reconstructs a TableHeightData class object from an Object array.
         *      This static method reverses the action taken by the 
         *      ToObjectArray() method.
         *      
         *      Parameters
         *      ----------
         *      data : object[]
         *          An object array.  The first element is the EventID property, 
         *          and the remaining elements are the PlayerID and AbsoluteTableHeight numbers.
         *      
         *      Returns
         *      -------
         *      output : TableHeightData
         *          A TableHeightData class object populated with data from the object
         *          array.
         */
        public static TableHeightData FromObjectArray(object[] data)
        {
            byte ID = (byte)data[0];
            if (ID == TableHeightData.EventID)
            {
                int PlayerID = (int)data[1];
                int AbsoluteTableHeight = (int)data[2];
                return new TableHeightData(PlayerID, AbsoluteTableHeight);
            }
            else
            {
                Debug.LogErrorFormat("Event ID {0} does not match", ID);
                return null;
            }
        }
    }
}
