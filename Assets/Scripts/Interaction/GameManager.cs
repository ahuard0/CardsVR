using CardsVR.States;
using Photon.Pun;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace CardsVR.Interaction
{
    public class GameManager : Singleton<GameManager>
    {
        public Stack<int> CardsDeck;
        public Stack<int> CardsHandDominant;
        public Stack<int> CardsPile1;
        public Stack<int> CardsPile2;
        public Stack<int> CardsPile3;
        public Stack<int> CardsPile4;
        public Stack<int> CardsPile5;
        public Stack<int> CardsPile6;

        public GameObject Pile0;
        public GameObject Pile1;
        public GameObject Pile2;
        public GameObject Pile3;
        public GameObject Pile4;
        public GameObject Pile5;
        public GameObject Pile6;

        public PhysicMaterial cardMaterial;
        public float cardThickness = 0.0005f;
        public float pileOffset = -0.025f;

        public enum DominantHandState { Free, Held };
        public DominantHandState StateDominantHand;

        public GameObject debugText;
        public GameObject debugTextPriority;

        private HandsManager HM;

        private void Start()
        {
            HM = HandsManager.Instance;
            Deck Deck = new Deck();

            // Initialize Stacks
            CardsDeck = Deck.cardIDs;
            CardsHandDominant = new Stack<int>();
            CardsPile1 = new Stack<int>();
            CardsPile2 = new Stack<int>();
            CardsPile3 = new Stack<int>();
            CardsPile4 = new Stack<int>();
            CardsPile5 = new Stack<int>();
            CardsPile6 = new Stack<int>();

            if (PhotonNetwork.IsMasterClient)
            {
                // Test
                CardsPile1.Push(CardsDeck.Pop());
                CardsPile1.Push(CardsDeck.Pop());
                CardsPile1.Push(CardsDeck.Pop());

                // Test
                CardsPile2.Push(CardsDeck.Pop());
                CardsPile2.Push(CardsDeck.Pop());
                CardsPile2.Push(CardsDeck.Pop());

                // Test
                CardsPile3.Push(CardsDeck.Pop());
                CardsPile3.Push(CardsDeck.Pop());
                CardsPile3.Push(CardsDeck.Pop());

                // Test
                CardsPile4.Push(CardsDeck.Pop());
                CardsPile4.Push(CardsDeck.Pop());
                CardsPile4.Push(CardsDeck.Pop());

                // Test
                CardsPile5.Push(CardsDeck.Pop());
                CardsPile5.Push(CardsDeck.Pop());
                CardsPile5.Push(CardsDeck.Pop());

                // Test
                CardsPile6.Push(CardsDeck.Pop());
                CardsPile6.Push(CardsDeck.Pop());
                CardsPile6.Push(CardsDeck.Pop());
            }

            SpawnPile(CardsDeck, 0);
            SpawnPile(CardsPile1, 1);
            SpawnPile(CardsPile2, 2);
            SpawnPile(CardsPile3, 3);
            SpawnPile(CardsPile4, 4);
            SpawnPile(CardsPile5, 5);
            SpawnPile(CardsPile6, 6);
        }

        public void Update()
        {
            updateHandState();
        }

        public void addCardToPile(int cardID, int pileID)
        {
            Stack<int> pile = getPileCardStack(pileID);
            pile.Push(cardID);
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
            context.ChangeState(context.pileState);
        }

        public void updateCardParent(GameObject Pile, Stack<int> CardIDs)
        {
            if (CardIDs != null)
            {
                foreach (int CardID in CardIDs)
                {
                    // Get Deck Pile Anchor Location
                    Transform cardAnchor = Pile.transform.Find("CardAnchor");

                    // Set the Parent
                    GameObject card = getCardByTag(CardID);
                    if (card == null)
                        continue;

                    card.transform.parent = cardAnchor;
                }
            }
        }

        public int? getPileNumByCard(int cardID)
        {
            for (int pileID=0; pileID<7; pileID++)
            {
                if (getPileCardStack(pileID).Contains(cardID))
                    return pileID;
            }
            return null;
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
            else if (pile_num == -1)
                try { return HandsManager.Instance.DominantFingerCardAnchor.gameObject; }
                catch (UnassignedReferenceException) { return null; }
                catch (System.Exception e) { throw e; }
            else
            {
                Debug.LogErrorFormat("Unknown pile ID: {0}", pile_num);
                return null;
            }
        }
        public Stack<int> getPileCardStack(int pile_num)
        {
            if (pile_num == 0)
                return CardsDeck;
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
            else if (pile_num == -1)
                return CardsHandDominant;
            else
            {
                Debug.LogErrorFormat("Unknown Pile Number: {0}", pile_num);
                return null;
            }
        }

        public void setPileCardStack(int pileID, Stack<int> cardIDs)
        {
            if (pileID == 0)
                CardsDeck = cardIDs;
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
            else if (pileID == -1)
                CardsHandDominant = cardIDs;
            else
                Debug.LogErrorFormat("Unknown pile ID: {0}", pileID);
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

        public GameObject peekCard(int pile_num)
        {
            Stack<int> cardIDs = getPileCardStack(pile_num);
            if (cardIDs.Count > 0)
            {
                int cardID = cardIDs.Peek();
                return getCardByTag(cardID);
            }
            else
                return null;
        }

        public int peekCardID(int pile_num)
        {
            Stack<int> cardIDs = getPileCardStack(pile_num);
            if (cardIDs.Count > 0)
                return cardIDs.Peek();
            else
                return -1;
        }

        public GameObject grabCardPile2Hand(int pile_num)
        {
            Stack<int> cardIDs = getPileCardStack(pile_num);

            if (cardIDs.Count > 0)
            {
                // Move IDs
                int CardID = cardIDs.Pop();
                CardsHandDominant.Push(CardID);

                // Move CardObject
                GameObject Card = getCardByTag(CardID);
                Card.transform.parent = HM.DominantHandCardAnchor;

                return Card;
            }
            else
            {
                return null;
            }
        }

        public GameObject grabCardHand2Pile(int pile_num)
        {
            if (CardsHandDominant.Count > 0)
            {
                // Move ID & CardObject
                int CardID = CardsHandDominant.Pop();
                GameObject Card = getCardByTag(CardID);
                GameObject CardAnchor = getPileCardAnchor(pile_num);
                Stack<int> CardStack = getPileCardStack(pile_num);

                CardStack.Push(CardID);
                Card.transform.parent = CardAnchor.transform.Find("CardAnchor");

                return Card;
            }
            else
            {
                return null;
            }
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