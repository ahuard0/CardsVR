using CardsVR.Networking;
using CardsVR.States;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace CardsVR.Interaction
{
    public class GameManager : Singleton<GameManager>
    {
        public Stack<int> CardsDeck1;
        public Stack<int> CardsDeck2;
        public Stack<int> CardsHandDominant;
        public Stack<int> CardsHandInferior;
        public Stack<int> CardsPile1;
        public Stack<int> CardsPile2;
        public Stack<int> CardsPile3;
        public Stack<int> CardsPile4;
        public Stack<int> CardsPile5;
        public Stack<int> CardsPile6;
        public Stack<int> CardsFreeStack;

        public List<int> Cards = new List<int>();
        public Dictionary<int, int> CardOwner = new Dictionary<int, int>();

        public GameObject Pile0;
        public GameObject Pile1;
        public GameObject Pile2;
        public GameObject Pile3;
        public GameObject Pile4;
        public GameObject Pile5;
        public GameObject Pile6;
        public GameObject Pile7;
        public GameObject FreeCardsAnchor;

        public PhysicMaterial cardMaterial;
        public float cardThickness = 0.0005f;
        public float pileOffset = -0.025f;

        public enum DominantHandState { Free, Held };
        private DominantHandState _dominantHandState;
        public DominantHandState StateDominantHand
        {
            get
            {
                updateHandState();
                return _dominantHandState;
            }
            set { _dominantHandState = value; }
        }

        public GameObject debugText;
        public GameObject debugTextPriority;

        public Stack<int> CardsDeck
        {
            get
            {
                int PlayerID = PlayerManager.Instance.PlayerNum;
                if (PlayerID == 1)
                    return CardsDeck1;
                else if (PlayerID == 2)
                    return CardsDeck2;
                else
                {
                    Debug.LogError("Tried to access a deck without being assigned a player number first.");
                    return null;
                }
                    
            }
            set
            {
                int PlayerID = PlayerManager.Instance.PlayerNum;
                if (PlayerID == 1)
                    CardsDeck1 = value;
                else if (PlayerID == 2)
                    CardsDeck2 = value;
                else
                    Debug.LogError("Tried to assign a deck without being assigned a player number first.");
            }
        }

        private void Start()
        {
            // Initialize Decks
            CardsDeck1 = new Stack<int>();
            CardsDeck2 = new Stack<int>();

            // Initialize Stacks
            CardsHandDominant = new Stack<int>();
            CardsHandInferior = new Stack<int>();
            CardsPile1 = new Stack<int>();
            CardsPile2 = new Stack<int>();
            CardsPile3 = new Stack<int>();
            CardsPile4 = new Stack<int>();
            CardsPile5 = new Stack<int>();
            CardsPile6 = new Stack<int>();
            CardsFreeStack = new Stack<int>();

            StartCoroutine("DealCards");
        }

        public void Update()
        {
            updateHandState();
        }

        /*
         *      A coroutine used to deal the cards to their respective piles.
         *      This function is used for testing purposes.
         *      
         *      Parameters
         *      ----------
         *      None
         *      
         *      Returns
         *      -------
         *      None
         */
        private IEnumerator DealCards()
        {
            while (PlayerManager.Instance.PlayerNum == 0)  // wait for the player ID number to be assigned.
                yield return new WaitForSecondsRealtime(0.5f);

            Deck Deck = new Deck();
            Stack<int> CardIDs = Deck.cardIDs;
            int N_Cards = CardIDs.Count;

            if (PlayerManager.Instance.PlayerNum == 1)
            {
                // Deal Decks
                for (int i = 0; i < N_Cards / 2; i++)
                    CardsDeck1.Push(CardIDs.Pop());
                for (int i = 0; i < N_Cards / 2; i++)
                    CardsDeck2.Push(CardIDs.Pop());
            }
            else
            {
                // Deal Decks
                for (int i = 0; i < N_Cards; i++)
                    CardsFreeStack.Push(CardIDs.Pop());
            }

            // Initialize Ownership
            for (int i = 0; i < N_Cards; i++)
            {
                CardOwner[i] = 1;
                Cards.Add(i);
            }

            //if (CardsDeck.Count > 0 && PlayerManager.Instance.PlayerNum == 1)
            //{
            //    // Deal to Pile 1
            //    CardsPile1.Push(CardsDeck.Pop());
            //    CardsPile1.Push(CardsDeck.Pop());
            //    CardsPile1.Push(CardsDeck.Pop());

            //    // Deal to Pile 3
            //    CardsPile3.Push(CardsDeck.Pop());
            //    CardsPile3.Push(CardsDeck.Pop());
            //    CardsPile3.Push(CardsDeck.Pop());

            //    // Deal to Pile 4
            //    CardsPile4.Push(CardsDeck.Pop());
            //    CardsPile4.Push(CardsDeck.Pop());
            //    CardsPile4.Push(CardsDeck.Pop());

            //    // Deal to Pile 6
            //    CardsPile6.Push(CardsDeck.Pop());
            //    CardsPile6.Push(CardsDeck.Pop());
            //    CardsPile6.Push(CardsDeck.Pop());
            //}

            SpawnPile(CardsDeck1, 0);
            SpawnPile(CardsPile1, 1);
            SpawnPile(CardsPile3, 3);
            SpawnPile(CardsPile4, 4);
            SpawnPile(CardsPile6, 6);
            SpawnPile(CardsDeck2, 7);
            SpawnPile(CardsFreeStack, -3);
        }

        public bool isCardOwned(int cardID, int playerID)
        {
            if (CardOwner[cardID] == playerID)
                return true;
            else
                return false;
        }

        public bool isCardUnowned(int cardID)
        {
            if (CardOwner[cardID] == 0)
                return true;
            else
                return false;
        }

        public void setCardOwned(int cardID, int playerID)
        {
            CardOwner[cardID] = playerID;
        }

        public List<int> CardsOwnerNotSet()
        {
            return CardsOwned(0);  // OwnerID : 0 -> Owner Not Set
        }

        public List<int> CardsOwnedButUnownedBy(int playerID)
        {
            List<int> CardsOwned = new List<int>();
            foreach (int CardID in Cards)
                if (!isCardOwned(CardID, playerID) && !isCardUnowned(CardID))
                    CardsOwned.Add(CardID);
            return CardsOwned;
        }

        public List<int> CardsOwned(int playerID)
        {
            List<int> CardsOwned = new List<int>();
            foreach (int CardID in Cards)
                if (isCardOwned(CardID, playerID))
                    CardsOwned.Add(CardID);
            return CardsOwned;
        }

        public void updateHandState()
        {
            if (getNumCardsHand() > 0)
            {
                StateDominantHand = DominantHandState.Held;
            }
            else
            {
                StateDominantHand = DominantHandState.Free;
            }
        }

        public GameObject getCardByTag(int cardID)
        {
            GameObject cardModel = Deck.getCardByNumber(cardID);
            string name = cardModel.name;

            GameObject[] cards = GameObject.FindGameObjectsWithTag("PlayerCards");
            foreach(GameObject card in cards)
            {
                if (card.name.Contains(name))
                    return card;
            }
            return null;
        }

        public void SpawnPile(Stack<int> CardsPile, int PileID)
        {
            if (CardsPile.Count == 0)
                return;

            foreach (int cardID in CardsPile)
                SpawnCard(cardID, PileID);  // Instantiate Card Game Objects
        }

        public void SpawnCard(int cardID, int pileID)
        {
            // Get Card Model
            GameObject cardTemplate = Deck.getCardByNumber(cardID);

            // Get Deck Pile Anchor Location
            Transform cardAnchor = getPileCardAnchor(pileID).transform;

            // Attach to Card Anchor
            //GameObject card = Instantiate(cardTemplate, new Vector3(0, 0, 0), Quaternion.identity);
            GameObject card = Instantiate<GameObject>(cardTemplate, cardAnchor);
            MeshRenderer mr = card.GetComponent<MeshRenderer>();
            float meshThickness = mr.bounds.size.y;

            // Set Card Tag
            card.tag = "PlayerCards";

            // Position the Card Face Down in a Stack
            card.transform.localPosition = new Vector3(0, 0, 0); // Stack by Card Height
                                                                 //card.transform.localPosition = new Vector3(0, (cards.Count - i - 1) * (cardThickness), 0); // Stack by Card Height
            card.transform.localScale = new Vector3(1, cardThickness / meshThickness, 1); // Scale the Card Thickness
            card.transform.localRotation = Quaternion.Euler(180, 180, 0); // Rotate Face Down
            card.layer = LayerMask.NameToLayer("Cards"); // Set Layer Mask for Ray Tracing

            // Setup Kinematic Mode to Disable Gravity and Physics
            Rigidbody rb = card.GetComponent<Rigidbody>();
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
            rb.isKinematic = true;
            rb.drag = 1;
            rb.angularDrag = 1;

            // Setup Collider
            MeshCollider mc = card.GetComponent<MeshCollider>();
            mc.convex = true;
            mc.material = cardMaterial;
            mc.cookingOptions = MeshColliderCookingOptions.None;
            mc.isTrigger = true;

            // Setup State Context
            CardStateContext context = card.AddComponent<CardStateContext>();
            context.PileNum = pileID;
            context.CardID = cardID;
            if (pileID == -3)
                context.ChangeState(context.moveState);  // Free Card
            else if (pileID == -1 || pileID == -2)
                context.ChangeState(context.fingerState);  // Held Card
            else
                context.ChangeState(context.pileState);  // Pile Card
        }

        public int? getPileNumByCard(int cardID)
        {
            for (int pileID=-3; pileID<8; pileID++)
            {
                try
                {
                    if (getPileCardStack(pileID).Contains(cardID))
                        return pileID;
                }
                catch
                {
                    Debug.LogErrorFormat("Error caught: Pile {0} and Card {1}", pileID, cardID);
                }
                
            }
            return null;
        }

        public int? getPileIndexByCard(int cardID)
        {
            int? pile_num = getPileNumByCard(cardID);
            if (pile_num == null)  // card not in pile
                return null;

            Stack<int> pileStack = getPileCardStack((int)pile_num);
            int[] pileArray = pileStack.ToArray();
            for (int i=0; i<pileArray.Length; i++)
            {
                if (pileArray[i] == cardID)  // card found, return index
                    return i;
            }

            return null;  // index not found
        }

        public GameObject getPileCardCollider(int pile_num)
        {
            if (pile_num < 0)  // Only return piles on the table top
                return null;

            GameObject CardAnchor = getPileCardAnchor(pile_num);

            return CardAnchor.transform.parent.Find("Pile" + pile_num + "Collider").gameObject;
        }

        public GameObject getPileCardAnchor(int pile_num)
        {
            if (pile_num == 0)
                return Pile0;
            else if (pile_num == 1)
                return Pile1;
            else if (pile_num == 2)
                return Pile2;
            else if (pile_num == 3)
                return Pile3;
            else if (pile_num == 4)
                return Pile4;
            else if (pile_num == 5)
                return Pile5;
            else if (pile_num == 6)
                return Pile6;
            else if (pile_num == 7)
                return Pile7;
            else if (pile_num == -3)
                return FreeCardsAnchor;
            else if (pile_num == -1)
                try { return HandsManager.Instance.DominantFingerCardAnchor.gameObject; }
                catch (UnassignedReferenceException) { return null; }
                catch (System.Exception e) { throw e; }
            else if (pile_num == -2)
                try { return HandsManager.Instance.InferiorFingerCardAnchor.gameObject; }
                catch (UnassignedReferenceException) { return null; }
                catch (System.Exception e) { throw e; }
            else
            {
                Debug.LogErrorFormat("Unknown pile ID: {0}", pile_num);
                return null;
            }
        }
        
        public GameObject getPile(int pile_num)
        {
            return getPileCardAnchor(pile_num).transform.parent.gameObject;
        }

        public Stack<int> getPileCardStack(int pile_num)
        {
            if (pile_num == 0)
                return CardsDeck1;
            else if (pile_num == 1)
                return CardsPile1;
            else if (pile_num == 2)
                return CardsPile2;
            else if (pile_num == 3)
                return CardsPile3;
            else if (pile_num == 4)
                return CardsPile4;
            else if (pile_num == 5)
                return CardsPile5;
            else if (pile_num == 6)
                return CardsPile6;
            else if (pile_num == 7)
                return CardsDeck2;
            else if (pile_num == -1)
                return CardsHandDominant;
            else if (pile_num == -2)
                return CardsHandInferior;
            else if (pile_num == -3)
                return CardsFreeStack;
            else
            {
                Debug.LogErrorFormat("Unknown Pile Number: {0}", pile_num);
                return null;
            }
        }

        public void setPileCardStack(int pileID, Stack<int> cardIDs)
        {
            if (pileID == 0)
                CardsDeck1 = cardIDs;
            else if (pileID == 1)
                CardsPile1 = cardIDs;
            else if (pileID == 2)
                CardsPile2 = cardIDs;
            else if (pileID == 3)
                CardsPile3 = cardIDs;
            else if (pileID == 4)
                CardsPile4 = cardIDs;
            else if (pileID == 5)
                CardsPile5 = cardIDs;
            else if (pileID == 6)
                CardsPile6 = cardIDs;
            else if (pileID == 7)
                CardsDeck2 = cardIDs;
            else if (pileID == -1)
                CardsHandDominant = cardIDs;
            else if (pileID == -2)
                CardsHandInferior = cardIDs;
            else if (pileID == -3)
                CardsFreeStack = cardIDs;
            else
                Debug.LogErrorFormat("Unknown pile ID: {0}", pileID);
        }

        public void addCardToPile(int pileID, int cardID)
        {
            getPileCardStack(pileID).Push(cardID);
        }

        public void removeCardFromPileStack(int pileID, int cardID)
        {
            Stack<int> Pile = getPileCardStack(pileID);
            int[] newArray = Pile.ToArray();
            Stack<int> newStack = new Stack<int>();
            for (int j = 0; j < newArray.Length; j++)
            {
                if (newArray[j] != cardID)
                    newStack.Push(newArray[j]);
            }
            setPileCardStack(pileID, newStack);
        }

        public int? getStackPosition(int cardID, int pile_num)
        {
            Stack<int> pile_stack = getPileCardStack(pile_num);
            Stack<int>.Enumerator pile_enum = pile_stack.GetEnumerator();
            int? ndx = -1;
            while(pile_enum.MoveNext())
            {
                ndx++;
                if (pile_enum.Current.Equals(cardID))
                    break;
            }
            if (ndx < 0)
                return null;
            else
                return ndx;
        }

        public int getNumCards(int pile_num)
        {
            return getPileCardStack(pile_num).Count;
        }

        public int getNumCardsHand()
        {
            return CardsHandDominant.Count;
        }

        public int peekCardID(int pile_num)
        {
            Stack<int> cardIDs = getPileCardStack(pile_num);
            if (cardIDs.Count > 0)
                return cardIDs.Peek();
            else
                return -1;
        }

        public void transferCardHand2Pile(int pile_num)
        {
            GameManager.Instance.getPileCardStack(pile_num).Push(GameManager.Instance.CardsHandDominant.Pop());
        }
        
        public void transferCardPile2Hand(int pile_num)
        {
            GameManager.Instance.CardsHandDominant.Push(GameManager.Instance.getPileCardStack(pile_num).Pop());
        }

        public void setDebugText(string msg)
        {
            TextMeshPro text = debugText.GetComponent<TextMeshPro>();
            text.SetText(msg);
        }

        public void setDebugPriorityText(string msg)
        {
            TextMeshPro text = debugTextPriority.GetComponent<TextMeshPro>();
            text.SetText(msg);
        }

    }
}