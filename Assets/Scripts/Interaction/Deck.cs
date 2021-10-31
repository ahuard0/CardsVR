using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CardsVR.Interaction
{
    public class Deck
    {
        #region Model Parameters
        public Stack<int> cardIDs;
        private static bool isInitialized = false;
        #endregion

        #region Static Game Objects

        public static GameObject Diamond_2;
        public static GameObject Diamond_3;
        public static GameObject Diamond_4;
        public static GameObject Diamond_5;
        public static GameObject Diamond_6;
        public static GameObject Diamond_7;
        public static GameObject Diamond_8;
        public static GameObject Diamond_9;
        public static GameObject Diamond_10;
        public static GameObject Diamond_J;
        public static GameObject Diamond_Q;
        public static GameObject Diamond_K;
        public static GameObject Diamond_A;

        public static GameObject Heart_2;
        public static GameObject Heart_3;
        public static GameObject Heart_4;
        public static GameObject Heart_5;
        public static GameObject Heart_6;
        public static GameObject Heart_7;
        public static GameObject Heart_8;
        public static GameObject Heart_9;
        public static GameObject Heart_10;
        public static GameObject Heart_J;
        public static GameObject Heart_Q;
        public static GameObject Heart_K;
        public static GameObject Heart_A;

        public static GameObject Club_2;
        public static GameObject Club_3;
        public static GameObject Club_4;
        public static GameObject Club_5;
        public static GameObject Club_6;
        public static GameObject Club_7;
        public static GameObject Club_8;
        public static GameObject Club_9;
        public static GameObject Club_10;
        public static GameObject Club_J;
        public static GameObject Club_Q;
        public static GameObject Club_K;
        public static GameObject Club_A;

        public static GameObject Spade_2;
        public static GameObject Spade_3;
        public static GameObject Spade_4;
        public static GameObject Spade_5;
        public static GameObject Spade_6;
        public static GameObject Spade_7;
        public static GameObject Spade_8;
        public static GameObject Spade_9;
        public static GameObject Spade_10;
        public static GameObject Spade_J;
        public static GameObject Spade_Q;
        public static GameObject Spade_K;
        public static GameObject Spade_A;

        #endregion

        #region public instance methods
        public Deck()
        {
            cardIDs = generateDeck(52);
        }

        public Stack<int> generateDeck(int length)
        {
            List<int> IDs = new List<int>();
            for (int i = 0; i < length; i++)
            {
                IDs.Add(i);
            }

            System.Random r = new System.Random();
            List<int> randomIDs = IDs.OrderBy(x => r.Next()).ToList();

            Stack<int> cards = new Stack<int>((IEnumerable<int>)randomIDs);
            return cards;
        }
        #endregion

        #region static methods
        private static void loadResources()
        {
            if (!isInitialized)
            {
                Diamond_2 = Resources.Load("Diamond_2") as GameObject;
                Diamond_3 = Resources.Load("Diamond_3") as GameObject;
                Diamond_4 = Resources.Load("Diamond_4") as GameObject;
                Diamond_5 = Resources.Load("Diamond_5") as GameObject;
                Diamond_6 = Resources.Load("Diamond_6") as GameObject;
                Diamond_7 = Resources.Load("Diamond_7") as GameObject;
                Diamond_8 = Resources.Load("Diamond_8") as GameObject;
                Diamond_9 = Resources.Load("Diamond_9") as GameObject;
                Diamond_10 = Resources.Load("Diamond_10") as GameObject;
                Diamond_J = Resources.Load("Diamond_J") as GameObject;
                Diamond_Q = Resources.Load("Diamond_Q") as GameObject;
                Diamond_K = Resources.Load("Diamond_K") as GameObject;
                Diamond_A = Resources.Load("Diamond_A") as GameObject;

                Heart_2 = Resources.Load("Heart_2") as GameObject;
                Heart_3 = Resources.Load("Heart_3") as GameObject;
                Heart_4 = Resources.Load("Heart_4") as GameObject;
                Heart_5 = Resources.Load("Heart_5") as GameObject;
                Heart_6 = Resources.Load("Heart_6") as GameObject;
                Heart_7 = Resources.Load("Heart_7") as GameObject;
                Heart_8 = Resources.Load("Heart_8") as GameObject;
                Heart_9 = Resources.Load("Heart_9") as GameObject;
                Heart_10 = Resources.Load("Heart_10") as GameObject;
                Heart_J = Resources.Load("Heart_J") as GameObject;
                Heart_Q = Resources.Load("Heart_Q") as GameObject;
                Heart_K = Resources.Load("Heart_K") as GameObject;
                Heart_A = Resources.Load("Heart_A") as GameObject;

                Club_2 = Resources.Load("Club_2") as GameObject;
                Club_3 = Resources.Load("Club_3") as GameObject;
                Club_4 = Resources.Load("Club_4") as GameObject;
                Club_5 = Resources.Load("Club_5") as GameObject;
                Club_6 = Resources.Load("Club_6") as GameObject;
                Club_7 = Resources.Load("Club_7") as GameObject;
                Club_8 = Resources.Load("Club_8") as GameObject;
                Club_9 = Resources.Load("Club_9") as GameObject;
                Club_10 = Resources.Load("Club_10") as GameObject;
                Club_J = Resources.Load("Club_J") as GameObject;
                Club_Q = Resources.Load("Club_Q") as GameObject;
                Club_K = Resources.Load("Club_K") as GameObject;
                Club_A = Resources.Load("Club_A") as GameObject;

                Spade_2 = Resources.Load("Spade_2") as GameObject;
                Spade_3 = Resources.Load("Spade_3") as GameObject;
                Spade_4 = Resources.Load("Spade_4") as GameObject;
                Spade_5 = Resources.Load("Spade_5") as GameObject;
                Spade_6 = Resources.Load("Spade_6") as GameObject;
                Spade_7 = Resources.Load("Spade_7") as GameObject;
                Spade_8 = Resources.Load("Spade_8") as GameObject;
                Spade_9 = Resources.Load("Spade_9") as GameObject;
                Spade_10 = Resources.Load("Spade_10") as GameObject;
                Spade_J = Resources.Load("Spade_J") as GameObject;
                Spade_Q = Resources.Load("Spade_Q") as GameObject;
                Spade_K = Resources.Load("Spade_K") as GameObject;
                Spade_A = Resources.Load("Spade_A") as GameObject;

                isInitialized = true;
            }
        }

        public static GameObject getCardByNumber(int num)
        {
            loadResources();
            switch (num)
            {
                case 0: return Club_2;
                case 1: return Club_3;
                case 2: return Club_4;
                case 3: return Club_5;
                case 4: return Club_6;
                case 5: return Club_7;
                case 6: return Club_8;
                case 7: return Club_9;
                case 8: return Club_10;
                case 9: return Club_J;
                case 10: return Club_Q;
                case 11: return Club_K;
                case 12: return Club_A;
                case 13: return Diamond_2;
                case 14: return Diamond_3;
                case 15: return Diamond_4;
                case 16: return Diamond_5;
                case 17: return Diamond_6;
                case 18: return Diamond_7;
                case 19: return Diamond_8;
                case 20: return Diamond_9;
                case 21: return Diamond_10;
                case 22: return Diamond_J;
                case 23: return Diamond_Q;
                case 24: return Diamond_K;
                case 25: return Diamond_A;
                case 26: return Heart_2;
                case 27: return Heart_3;
                case 28: return Heart_4;
                case 29: return Heart_5;
                case 30: return Heart_6;
                case 31: return Heart_7;
                case 32: return Heart_8;
                case 33: return Heart_9;
                case 34: return Heart_10;
                case 35: return Heart_J;
                case 36: return Heart_Q;
                case 37: return Heart_K;
                case 38: return Heart_A;
                case 39: return Spade_2;
                case 40: return Spade_3;
                case 41: return Spade_4;
                case 42: return Spade_5;
                case 43: return Spade_6;
                case 44: return Spade_7;
                case 45: return Spade_8;
                case 46: return Spade_9;
                case 47: return Spade_10;
                case 48: return Spade_J;
                case 49: return Spade_Q;
                case 50: return Spade_K;
                case 51: return Spade_A;
                default: throw (new Exception("Unknown Card"));
            }
        }

        public static string getCardNameByNumber(int num)
        {
            switch (num)
            {
                case 0: return "Club_2";
                case 1: return "Club_3";
                case 2: return "Club_4";
                case 3: return "Club_5";
                case 4: return "Club_6";
                case 5: return "Club_7";
                case 6: return "Club_8";
                case 7: return "Club_9";
                case 8: return "Club_10";
                case 9: return "Club_J";
                case 10: return "Club_Q";
                case 11: return "Club_K";
                case 12: return "Club_A";
                case 13: return "Diamond_2";
                case 14: return "Diamond_3";
                case 15: return "Diamond_4";
                case 16: return "Diamond_5";
                case 17: return "Diamond_6";
                case 18: return "Diamond_7";
                case 19: return "Diamond_8";
                case 20: return "Diamond_9";
                case 21: return "Diamond_10";
                case 22: return "Diamond_J";
                case 23: return "Diamond_Q";
                case 24: return "Diamond_K";
                case 25: return "Diamond_A";
                case 26: return "Heart_2";
                case 27: return "Heart_3";
                case 28: return "Heart_4";
                case 29: return "Heart_5";
                case 30: return "Heart_6";
                case 31: return "Heart_7";
                case 32: return "Heart_8";
                case 33: return "Heart_9";
                case 34: return "Heart_10";
                case 35: return "Heart_J";
                case 36: return "Heart_Q";
                case 37: return "Heart_K";
                case 38: return "Heart_A";
                case 39: return "Spade_2";
                case 40: return "Spade_3";
                case 41: return "Spade_4";
                case 42: return "Spade_5";
                case 43: return "Spade_6";
                case 44: return "Spade_7";
                case 45: return "Spade_8";
                case 46: return "Spade_9";
                case 47: return "Spade_10";
                case 48: return "Spade_J";
                case 49: return "Spade_Q";
                case 50: return "Spade_K";
                case 51: return "Spade_A";
                default: throw (new Exception("Unknown Card"));
            }
        }
        #endregion
    }

}
