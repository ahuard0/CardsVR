using System;
using System.Collections.Generic;
using UnityEngine;

namespace CardsVR.Commands
{
    /*
     *  This PileData class is serializable and capable of transmittal over PUN using PhotonNetwork.RaiseEvent.
     *  
     *  This class represents the position and rotation orientation of an object.
     */
    public class PileData : ICommandData
    {
        public static readonly byte EventID = 45;

        public byte getEventID { get { return EventID; } }

        /*
         *      Constructor for the PileData class.
         *      
         *      Parameters
         *      ----------
         *      pileID : int
         *          The pile unique ID representing the card destination.  Required.
         *      cardIDs : Stack<int>
         *          A stack of unique IDs representing the cards.  Required.
         */
        public PileData(int pileID, Stack<int> cardIDs)
        {
            this.PileID = pileID;
            this.CardIDs = cardIDs;
        }

        /*
         *      The unique IDs assigned to the cards, which are used to for lookup in the Game Manager.
         *      
         *      Accessor
         *      -------
         *      cardIDs : Stack<int>
         *          A stack of unique IDs representing the cards.
         */
        public Stack<int> CardIDs
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
         *      Returns the PileID and CardIDs, which represent this class.
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
            return "Pile: " + PileID.ToString() + ", CardID: " + CardIDs.ToString();
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
         *          and the remaining elements are the Pile ID and Card ID numbers.
         */
        public object[] ToObjectArray()
        {
            object[] data = new object[3];
            data[0] = EventID;
            data[1] = PileID;
            data[2] = CardIDs.ToArray();
            return data;
        }

        /*
         *      Reconstructs a PileData class object from an Object array.
         *      This static method reverses the action taken by the 
         *      ToObjectArray() method.
         *      
         *      Parameters
         *      ----------
         *      data : object[]
         *          An object array.  The first element is the EventID property, 
         *          and the remaining elements are the Pile ID and Card ID numbers.
         *      
         *      Returns
         *      -------
         *      output : PileData
         *          A PileData class object populated with data from the object
         *          array.
         */
        public static PileData FromObjectArray(object[] data)
        {
            byte ID = (byte)data[0];
            if (ID == PileData.EventID)
            {
                int PileID = (int)data[1];
                int[] cards_rev = (int[])data[2];  // comes in reverse order
                int[] cards = new int[cards_rev.Length];
                for (int i=0; i<cards_rev.Length; i++)  // reverse the order back to original
                    cards[i] = cards_rev[cards_rev.Length - 1 - i];
                Stack<int> CardIDs = new Stack<int>(cards);  // load array into Stack<int>
                return new PileData(PileID, CardIDs);
            }
            else
            {
                Debug.LogErrorFormat("Event ID {0} does not match", ID);
                return null;
            }
        }
    }
}
