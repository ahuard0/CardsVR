using System.IO;
using UnityEngine;

namespace CardsVR.Commands
{
    /*
     *  This Movement class is serializable and capable of transmittal over PUN using PhotonNetwork.RaiseEvent.
     *  
     *  This class represents the position and rotation orientation of an object.
     */
    public class CardToPile : ICommandData
    {
        public static readonly byte EventID = 44;

        public byte getEventID { get { return EventID; } }

        /*
         *      Constructor for the Movement class.
         *      
         *      Parameters
         *      ----------
         *      cardID : int
         *          The unique ID of the card.  Required.
         *      pile : int
         *          The unique ID of the pile destination.  Required.
         *      name : string (optional)
         *          The name of the game object representing the card
         */
        public CardToPile(int cardID, int pile, string name)
        {
            this.CardID = cardID;
            this.Pile = pile;
            this.Name = name;
        }

        /*
         *      The unique ID assigned to this card, which is used to for lookup in the Game Manager.
         *      
         *      Accessor
         *      -------
         *      CardID : int
         *          The unique card number.
         */
        public int CardID
        {
            get; set;
        }

        /*
         *      The unique ID of the Pile the GameObject will be moved to.
         *      
         *      Accessor
         *      -------
         *      Pile : int
         *          The unique pile number.
         */
        public int Pile
        {
            get; set;
        }

        /*
         *      GameObject name.  When present, identifies the GameObject.
         *      
         *      Accessor
         *      -------
         *      Name : string
         *          Name of the GameObject being moved.
         */
        public string Name
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
            return "CardID: " + CardID.ToString() + ", Pile: " + Pile.ToString() + ", Name: " + Name;
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
            object[] data = new object[4];
            data[0] = EventID;
            data[1] = CardID;
            data[2] = Pile;
            data[3] = Name;
            return data;
        }

        /*
         *      Reconstructs a CardToPile class object from an Object array.
         *      This static method reverses the action taken by the 
         *      ToObjectArray() method.
         *      
         *      Parameters
         *      ----------
         *      data : object[]
         *          An object array.  The first element is the EventID property, 
         *          and the remaining elements are the Name, Card, and Pile numbers.
         *      
         *      Returns
         *      -------
         *      output : CardToPile
         *          A CardToPile class object populated with data from the object
         *          array.
         */
        public static CardToPile FromObjectArray(object[] data)
        {
            byte ID = (byte)data[0];
            if (ID == CardToPile.EventID)
            {
                int Card = (int)data[1];
                int Pile = (int)data[2];
                string Name = (string)data[3];
                return new CardToPile(Card, Pile, Name);
            }
            else
            {
                Debug.LogErrorFormat("Event ID {0} does not match", ID);
                return null;
            }
        }
    }
}
