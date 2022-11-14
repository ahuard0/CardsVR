using System;
using System.Collections.Generic;
using UnityEngine;
using CardsVR;

namespace CardsVR.Detection
{
    public class RegisterCards : MonoBehaviour
    {

        public GameObject CardTemplate;
        public GameObject PlayerTags;
        private Dictionary<int, Card> Cards;

        private static Dictionary<int, Queue<RegisteredTag>> TagsHistory;
        private static Dictionary<int, RegisteredTag> TagsFrame;
        //private static Dictionary<int, int> LastSeenFramesTagIDs;

        private static int _TagHistory;

        public int TagHistory = 10;
        public bool DrawCards = false;
        public bool CalculatePoseConsensus = true;


        public bool DebugCardCandidates;
        public static bool _DebugCardCandidates;

        void Start()
        {
            // Initialize
            Cards = new Dictionary<int, Card>();

            //LastSeenFramesTagIDs = new Dictionary<int, int>();
            TagsHistory = new Dictionary<int, Queue<RegisteredTag>>();
            TagsFrame = new Dictionary<int, RegisteredTag>();
            _TagHistory = TagHistory;
            _DebugCardCandidates = DebugCardCandidates;

            // Get Prefab Card Template for Display
            //CardTemplate = Utility.FindInActiveObjectByName("CardTemplate");
            CardTemplate.SetActive(false);
            if (CardTemplate == null)
            {
                throw new Exception("Card template game object not found.");
            }

            // Get GameObject Parent Holder for Tags and Cards
            //PlayerTags = GameObject.Find("PlayerTags");
            if (PlayerTags == null)
            {
                throw new Exception("Player Tags game object not found.");
            }
        }

        private void LateUpdate()
        {
            Cards.Clear();
            TagsFrame.Clear();
        }

        void Update()
        {
            // Initialize
            DisableAllCardsInScene();
            _TagHistory = TagHistory; // Maximum Number of Tag Records to Keep

            // Register Cards
            RegisterTagsFrameHistory();


            // Gather Tags by Card ID
            List<int> detectedCardIDs = new List<int>();
            Dictionary<int, Dictionary<int, Queue<RegisteredTag>>> CardTagQueue = new Dictionary<int, Dictionary<int, Queue<RegisteredTag>>>();
            foreach (var tagQueue in TagsHistory)
            {
                RegisteredTag tagPeek = tagQueue.Value.Peek();
                if (tagPeek == null) { continue; }
                var cardID = tagPeek.info.CardID;
                if (CardTagQueue.ContainsKey(cardID))
                {
                    CardTagQueue[cardID].Add(tagQueue.Key, tagQueue.Value);
                }
                else
                {
                    var queue = new Dictionary<int, Queue<RegisteredTag>>
                {
                    { tagQueue.Key, tagQueue.Value }
                };
                    CardTagQueue.Add(cardID, queue);
                }
                detectedCardIDs.Add(cardID);
            }

            // Remove Undetected Cards from the Tag Queue
            var cardIDs = CardTagQueue.Keys;
            foreach (int cardID in cardIDs)
            {
                if (!detectedCardIDs.Contains(cardID))
                {
                    CardTagQueue.Remove(cardID);
                }
            }


            // Process Tag History->Cards
            // Cards are formed using a history of tags.  Multiple tags represent one card.
            foreach (var item in CardTagQueue)
            {
                int cardID = item.Key;
                var cardTagHistory = item.Value;

                int tagHistoryCount = 0;
                int tagCount = 0;
                foreach (var history in cardTagHistory)
                {
                    tagCount++;
                    foreach (var tag in history.Value)
                    {
                        tagHistoryCount++;
                    }
                }
                float avgHistoryPerTag = tagHistoryCount / tagCount;
                if (avgHistoryPerTag < _TagHistory * 0.9) // allow for some tag dropouts
                {
                    continue;
                }


                Card card = new Card(cardID, cardTagHistory);
                int id = card.id;
                if (Cards.ContainsKey(id))
                {
                    Cards[id] = card;
                }
                else
                {
                    Cards.Add(card.id, card);
                }
            }

            if (DrawCards)
            {
                foreach (var c in Cards)
                {
                    if (c.Value.tag == null) { continue; }
                    DrawCard(c.Value);
                }
            }

        }


        // Add Frame Tags to History Queue and Discard Expired Tags
        public static void RegisterTagsFrameHistory()
        {
            // Manage Existing Tag History Queues
            int[] IDs = new int[TagsHistory.Count];
            int i = 0;
            foreach (var tagUniqueID in TagsHistory.Keys)
            {
                IDs[i] = tagUniqueID;
                i++;
            }
            for (int j = 0; j < IDs.Length; j++)
            {
                int tagUniqueID = IDs[j];

                if (TagsFrame.ContainsKey(tagUniqueID))
                {
                    Queue<RegisteredTag> tagQueue = TagsHistory[tagUniqueID];
                    RegisteredTag tag = TagsFrame[tagUniqueID];
                    while (tagQueue.Count >= _TagHistory)
                    {
                        tagQueue.Dequeue(); // discard one
                    }
                    tagQueue.Enqueue(tag); // add one
                    TagsHistory[tagUniqueID] = tagQueue;
                }
                else
                {
                    Queue<RegisteredTag> tagQueue = TagsHistory[tagUniqueID];
                    while (tagQueue.Count >= _TagHistory)
                    {
                        tagQueue.Dequeue(); // discard one
                    }
                    tagQueue.Enqueue(null); // add one
                    TagsHistory[tagUniqueID] = tagQueue;
                }
            }

            // Add New Tags to Tag History
            foreach (var item in TagsFrame)
            {
                int tagUniqueID = item.Key;
                if (!TagsHistory.ContainsKey(tagUniqueID))
                {
                    Queue<RegisteredTag> tagQueue = new Queue<RegisteredTag>();
                    RegisteredTag tag = item.Value;
                    tagQueue.Enqueue(tag); // add one
                    TagsHistory.Add(tagUniqueID, tagQueue);
                }

            }

            // Remove Cards with No Tags (All Null)
            IDs = new int[TagsHistory.Count];
            i = 0;
            foreach (var tagUniqueID in TagsHistory.Keys)
            {
                IDs[i] = tagUniqueID;
                i++;
            }
            for (int j = 0; j < IDs.Length; j++)
            {
                int tagUniqueID = IDs[j];
                Queue<RegisteredTag> tagQueue = TagsHistory[tagUniqueID];
                bool isNull = true;
                foreach (RegisteredTag tag in tagQueue)
                {
                    if (tag != null)
                    {
                        isNull = false;
                    }
                }
                if (isNull)
                {
                    TagsHistory.Remove(tagUniqueID);
                }
            }

        }

        // Add Tag to the Current Frame -> Will be queued into tag history
        public static void RegisterTagFrame(RegisteredTag tag)
        {
            int id = tag.getUniqueID();
            if (TagsFrame.ContainsKey(id))
            {
                TagsFrame[id] = tag;
            }
            else
            {
                TagsFrame.Add(id, tag);
            }
        }

        public void DrawCard(Card card)
        {

            if (card.tag == null)
            {
                Debug.Log("test");
            }


            Vector3 centroid;
            Vector3 poseNormal;
            Quaternion pose;
            centroid = card.centroid;
            poseNormal = card.poseNormal;
            pose = card.tag.rotation;
            //Vector3 scale = card.tag.scale;
            //Quaternion pose = Quaternion.FromToRotation(new Vector3(0, 0, 1), poseNormal);



            int id = card.id;
            GameObject CardObj = Utility.Toolbox.FindInActiveObjectByName("Card" + id);
            if (CardObj == null)
            {
                GameObject CardModel = Instantiate<GameObject>(CardTemplate, new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 0), PlayerTags.transform);
                //Transform square = CardModel.transform.Find("Square");
                //square.Translate(card.tag.modelCoords); // Model at Origin -> Shift for Card Alignment
                CardObj = Instantiate<GameObject>(CardModel, centroid, pose, PlayerTags.transform);
                CardObj.name = "Card" + id;
                Destroy(CardModel);
            }
            else
            {
                CardObj.transform.position = centroid;
                CardObj.transform.rotation = pose;
            }
            CardObj.transform.localScale = new Vector3(2.0f, 2.0f, 1.0f);
            CardObj.SetActive(true);

            Vector3 farPt = centroid;
            Vector3 nearPt = new Vector3(0, 0, 0);
            Debug.DrawLine(nearPt, farPt, Color.blue, 1f);
            Debug.DrawLine(centroid, centroid + -15 * poseNormal, Color.green, 1f);
        }

        // Disable All Cards
        public static void DisableAllCardsInScene()
        {
            List<GameObject> cards = Utility.Toolbox.FindInActiveObjectsByPattern("Card");
            foreach (GameObject card in cards)
            {
                card.SetActive(false);
            }
        }

    }

}

