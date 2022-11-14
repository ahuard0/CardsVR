using System.IO;
using UnityEngine;

namespace CardsVR.Commands
{
    /*
     *  Used to broadcast a change in card ownership.
     */
    public class CardControlData : ICommandData
    {
        public static readonly byte EventID = 57;

        public byte getEventID { get { return EventID; } }

        /*
         *      Constructor for the CardControlData class.
         *      
         *      Parameters
         *      ----------
         *      cardID : int
         *          The unique ID of the card.
         *      PlayerID : int
         *          The unique ID of the player requesting ownership.
         */
        public CardControlData(int[] cardID, int playerID)
        {
            this.CardID = cardID;
            this.PlayerID = playerID;
        }
        public CardControlData(int cardID, int playerID)
        {
            int[] cardIDarr = new int[1];
            cardIDarr[0] = cardID;
            this.CardID = cardIDarr;
            this.PlayerID = playerID;
        }

        /*
         *      The unique ID assigned to this card, which is used to for lookup in the Game Manager.
         *      
         *      Accessor
         *      -------
         *      CardID : int
         *          The unique card number.
         */
        public int[] CardID
        {
            get; set;
        }

        /*
         *      The player's unique ID.
         *      
         *      Accessor
         *      -------
         *      PlayerID : int
         *          The ID of the player initiating the change.
         */
        public int PlayerID
        {
            get; set;
        }

        /*
         *      Returns the Name and Pile properties, which represent this class.
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
            return "CardID: " + CardID.ToString() + ", PlayerID: " + PlayerID.ToString();
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
         *          and the remaining elements are the Name, Card, and Pile numbers.
         */
        public object[] ToObjectArray()
        {
            object[] data = new object[3];
            data[0] = EventID;
            data[1] = CardID;
            data[2] = PlayerID;
            return data;
        }

        /*
         *      Reconstructs a CardControlData class object from an Object array.
         *      This static method reverses the action taken by the 
         *      ToObjectArray() method.
         *      
         *      Parameters
         *      ----------
         *      data : object[]
         *          An object array.  The first element is the EventID property, 
         *          and the remaining elements are the CardID and the PlayerID.
         *      
         *      Returns
         *      -------
         *      output : CardControlData
         *          A CardControlData class object populated with data from the object
         *          array.
         */
        public static CardControlData FromObjectArray(object[] data)
        {
            byte ID = (byte)data[0];
            if (ID == CardControlData.EventID)
            {
                int[] CardID = (int[])data[1];
                int PlayerID = (int)data[2];
                return new CardControlData(CardID, PlayerID);
            }
            else
            {
                Debug.LogErrorFormat("Event ID {0} does not match", ID);
                return null;
            }
        }
    }
}
