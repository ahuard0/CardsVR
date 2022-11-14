using System;
using System.Collections.Generic;
using UnityEngine;
using CardsVR.Utility;

namespace CardsVR.Detection
{
    public class Card
    {

        public enum Ranks
        {
            Ace, Two, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Jack, Queen, King
        }
        public enum Suits
        {
            Clubs, Diamonds, Hearts, Spades
        }
        public enum Sides
        {
            Front, Back
        }

        public int id;
        public Vector3 poseEuler
        {
            get
            {
                return rotation.eulerAngles;
            }
            set
            {
                rotation = Quaternion.Euler(value);
            }
        }
        public Quaternion rotation;
        public Vector3 tagPosition;
        public Vector3 modelTranslation;
        public Matrix4x4 transformation;

        public Vector3 centroid;
        public Vector3 poseNormal;
        public RegisteredTag tag;


        public class CardLabel
        {
            public Ranks Rank;
            public Suits Suit;
            public Sides Side;

            CardLabel(string rank, string suit, string side)
            {
                Rank = rank switch
                {
                    "A" => Ranks.Ace,
                    "2" => Ranks.Two,
                    "3" => Ranks.Three,
                    "4" => Ranks.Four,
                    "5" => Ranks.Five,
                    "6" => Ranks.Six,
                    "7" => Ranks.Seven,
                    "8" => Ranks.Eight,
                    "9" => Ranks.Nine,
                    "10" => Ranks.Ten,
                    "J" => Ranks.Jack,
                    "Q" => Ranks.Queen,
                    "K" => Ranks.King,
                    _ => throw new System.Exception("Unknown Rank"),
                };
                Suit = suit switch
                {
                    "C" => Suits.Clubs,
                    "D" => Suits.Diamonds,
                    "H" => Suits.Hearts,
                    "S" => Suits.Spades,
                    _ => throw new System.Exception("Unknown Suit"),
                };
                Side = side switch
                {
                    "F" => Sides.Front,
                    "B" => Sides.Back,
                    _ => throw new System.Exception("Unknown Side"),
                };
            }
        }

        public Card(RegisteredTag tag)
        {
            TagInfo info = tag.info;
            id = info.CardID;
            this.tag = tag;
            //rotation = tag.rotation;
            //tagPosition = tag.position;
            //modelTranslation = tag.modelCoords;
            //transformation = tag.transformation;
        }

        // The Card will be represented by a median consensus based on pose and position.
        public Card(int cardID, Dictionary<int, Queue<RegisteredTag>> tagDictionary)
        {
            id = cardID;
            if (tagDictionary.Count == 0) { return; }

            // Step 1:  Calculate the number of candidates
            int num_candidates = 0;
            foreach (var tagHistory in tagDictionary.Values)
            {
                num_candidates += tagHistory.Count;
            }

            // Step 2:  Iterate Over the Tags and Compute the Card Position and Pose
            Vector3[] candidatePositions = new Vector3[num_candidates];
            Vector3[] candidatePoses = new Vector3[num_candidates];
            RegisteredTag[] candidateTags = new RegisteredTag[num_candidates];
            int index = 0;
            foreach (var tagHistory in tagDictionary.Values)
            {
                RegisteredTag[] tags = tagHistory.ToArray();
                foreach (RegisteredTag tag in tags)
                {

                    if (tag == null) { continue; }

                    // Compute Card Position (Centroid Vector) and Pose (Normal Vector)
                    Vector3 scaledModelCoords = tag.modelCoords;
                    scaledModelCoords.x *= 2;
                    scaledModelCoords.y *= 2;

                    Vector3 CardCentroidWorld = tag.transformation.MultiplyPoint3x4(scaledModelCoords); // Position

                    Quaternion PoseRotation = tag.transformation.ExtractRotation();
                    Vector3 ModelPoseNormal = new Vector3(0, 0, 1);
                    Vector3 WorldPoseNormal = PoseRotation * ModelPoseNormal; // Pose

                    //if (RegisterCards._DebugCardCandidates)
                    //{
                    //    Debug.DrawLine(CardCentroidWorld, CardCentroidWorld + -15*WorldPoseNormal, Color.magenta, 1f);

                    //    Debug.Log(CardCentroidWorld);
                    //    RegisterCards.DrawCardFromTag(tag);

                    //    Vector3 farPt = CardCentroidWorld;
                    //    Vector3 nearPt = new Vector3(0, 0, 0);
                    //    Debug.DrawLine(nearPt, farPt, Color.cyan, 1f);
                    //}

                    // Store Vectors and Tag Information
                    candidatePositions[index] = CardCentroidWorld;
                    candidatePoses[index] = WorldPoseNormal;
                    candidateTags[index] = tag;
                    index++;

                }
            }

            // Step 3:  Optimize Cumulative Position and Pose Vector Separation to candidates
            double[] distPosition = new double[num_candidates];
            double[] distPose = new double[num_candidates];
            double[] loss = new double[num_candidates];
            for (int i = 0; i < num_candidates; i++)
            {
                for (int j = 0; j < num_candidates; j++)
                {
                    distPosition[i] += Math.Abs((candidatePositions[i] - candidatePositions[j]).magnitude);
                    distPose[i] += Math.Abs((candidatePoses[i] - candidatePoses[j]).magnitude);
                }
                loss[i] = distPosition[i] + distPose[i]; // Loss Function
            }
            int indexMin = Utility.Toolbox.IndexOfMin(loss); // Optimum Index
            RegisteredTag tagSelected = candidateTags[indexMin];
            Vector3 positionSelected = candidatePositions[indexMin];  // Optimum Position
            Vector3 poseSelected = candidatePoses[indexMin];  // Optimum Pose



            // Store Card Parameters
            //rotation = tagSelected.rotation;
            //tagPosition = tagSelected.position;
            //modelTranslation = tagSelected.modelCoords;
            //transformation = tagSelected.transformation;
            tag = tagSelected;
            centroid = positionSelected;   // Optimum Position
            poseNormal = poseSelected;  // Optimum Pose

        }

    }
}
