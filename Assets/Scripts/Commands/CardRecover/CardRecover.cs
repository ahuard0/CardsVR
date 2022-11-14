using System.IO;
using UnityEngine;

namespace CardsVR.Commands
{
    /*
     *  Broadcasts a change in card state.  This is used to indicate when a card is picked up or put down.  
     *  The master client then knows the state and can update it's model.
     */
    public class CardRecover : ICommandData
    {
        public static readonly byte EventID = 53;

        public byte getEventID { get { return EventID; } }
        public enum CardState { Pile, Finger, Move };

        public int[] CardIDs;
        public int[] Piles;
        public int[] PileIndex;
        public CardState[] States;
        public Vector3[] Positions;
        public Quaternion[] Rotations;
        public int OwnerID;

        /*
         *      Constructor for the CardRecover class.
         *      
         *      Parameters
         *      ----------
         *      cardIDs : int[]
         *          The card IDs being synchronized.
         *      piles : int[]
         *          The card piles:
         *              -3 is "Free"
         *              -2 is Inferior Hand
         *              -1 is Dominant Hand
         *              >= 0 are Table Piles including decks
         *      pileIndex : int
         *          The index of the card within a pile.
         *      cardStates : CardState[]
         *          The card states.
         *      positions : Vector3[]
         *          The card positions.
         *      rotations : Quaternion[]
         *          The card rotations.
         *      ownerID : int
         *          The PlayerID of the card owner.
         */
        public CardRecover(int[] cardIDs, int[] piles, int[] pileIndex, CardState[] cardStates, Vector3[] positions, Quaternion[] rotations, int ownerID)
        {
            this.CardIDs = cardIDs;
            this.Piles = piles;
            this.PileIndex = pileIndex;
            this.States = cardStates;
            this.Positions = positions;
            this.Rotations = rotations;
            this.OwnerID = ownerID;
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
            return "CardID: " + CardIDs.ToString() + ", Piles: " + Piles.ToString() + ", PileIndex: " + PileIndex.ToString() + 
                ", States: " + States.ToString() + ", OwnerID: " + OwnerID.ToString() + ", Positions: " + Positions.ToString() + 
                ", Rotations: " + Rotations.ToString();
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
            int[] StatesInt = new int[States.Length];
            for (int i = 0; i < States.Length; i++)
                StatesInt[i] = (int)States[i];

            object[] data = new object[8];
            data[0] = EventID;
            data[1] = CardIDs;
            data[2] = Piles;
            data[3] = PileIndex;
            data[4] = StatesInt;
            data[5] = Positions;
            data[6] = Rotations;
            data[7] = OwnerID;
            return data;
        }

        /*
         *      Reconstructs a CardRecover class object from an Object array.
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
         *      output : CardRecover
         *          A CardRecover class object populated with data from the object
         *          array.
         */
        public static CardRecover FromObjectArray(object[] data)
        {
            byte ID = (byte)data[0];
            if (ID == CardRecover.EventID)
            {
                int[] CardIDs = (int[])data[1];
                int[] Piles = (int[])data[2];
                int[] PileIndex = (int[])data[3];
                int[] StatesInt = (int[])data[4];
                Vector3[] Positions = (Vector3[])data[5];
                Quaternion[] Rotations = (Quaternion[])data[6];
                int OwnerID = (int)data[7];

                CardState[] States = new CardState[StatesInt.Length];
                for (int i = 0; i < StatesInt.Length; i++)
                    States[i] = (CardState)StatesInt[i];

                return new CardRecover(CardIDs, Piles, PileIndex, States, Positions, Rotations, OwnerID);
            }
            else
            {
                Debug.LogErrorFormat("Event ID {0} does not match", ID);
                return null;
            }
        }
    }
}
