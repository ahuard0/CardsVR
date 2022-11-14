using System;
using System.Collections.Generic;
using UnityEngine;

namespace CardsVR.Commands
{
    /*
     *  This DeckPosData class is serializable and capable of transmittal over PUN using PhotonNetwork.RaiseEvent.
     *  
     *  This class represents the world position of a deck pile object.
     */
    public class DeckPosData : ICommandData
    {
        public static readonly byte EventID = 51;

        public byte getEventID { get { return EventID; } }

        /*
         *      Constructor for the DeckPosData class.
         *      
         *      Parameters
         *      ----------
         *      playerID : int
         *          The unique ID representing the player (e.g., 1 or 2).  Required.
         *      pileID : int
         *          The pile unique ID representing the card destination.  Required.
         *      cardIDs : Stack<int>
         *          A stack of unique IDs representing the cards.  Required.
         */
        public DeckPosData(int playerID, int pileID, Vector3 position)
        {
            this.PlayerID = playerID;
            this.PileID = pileID;
            this.Position = position;
        }

        /*
         *      The world coordinates of the stack position.
         *      
         *      Accessor
         *      -------
         *      Position : Vector3
         *          Position of the stack in world coordinates.
         */
        public Vector3 Position
        {
            get; set;
        }

        /*
         *      The ID number of the player.
         *      
         *      Accessor
         *      -------
         *      PlayerID : int
         *          Player ID number (e.g., 1 or 2)
         */
        public int PlayerID
        {
            get; set;
        }

        /*
         *      The unique ID of the Pile.
         *      
         *      Accessor
         *      -------
         *      pileID : int
         *          The pile unique ID representing the card destination.
         */
        public int PileID
        {
            get; set;
        }

        /*
         *      Returns the PlayerID, PileID and Position, which represent this class.
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
            return "Pile: " + PileID.ToString() + ", Player: " + PlayerID.ToString() + ", Position: " + Position.ToString();
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
         *          and the remaining elements are the Player ID, Pile ID, and Position.
         */
        public object[] ToObjectArray()
        {
            object[] data = new object[4];
            data[0] = EventID;
            data[1] = PlayerID;
            data[2] = PileID;
            data[3] = Position;
            return data;
        }

        /*
         *      Reconstructs a DeckPosData class object from an Object array.
         *      This static method reverses the action taken by the 
         *      ToObjectArray() method.
         *      
         *      Parameters
         *      ----------
         *      data : object[]
         *          An object array.  The first element is the EventID property, 
         *          and the remaining elements are the Player ID, Pile ID, and Position.
         *      
         *      Returns
         *      -------
         *      output : DeckPosData
         *          A DeckPosData class object populated with data from the object
         *          array.
         */
        public static DeckPosData FromObjectArray(object[] data)
        {
            byte ID = (byte)data[0];
            if (ID == DeckPosData.EventID)
            {
                int PlayerID = (int)data[1];
                int PileID = (int)data[2];
                Vector3 Position = (Vector3)data[3];
                return new DeckPosData(PlayerID, PileID, Position);
            }
            else
            {
                Debug.LogErrorFormat("Event ID {0} does not match", ID);
                return null;
            }
        }
    }
}
