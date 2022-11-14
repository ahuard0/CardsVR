using System.Collections.Generic;
using UnityEngine;

namespace CardsVR.Interaction
{
    public class HandsManager : Singleton<HandsManager>
    {
        public enum Side { Unknown, Right, Left };
        public Side Dominant;
        public OVRHand HandRight;
        public OVRHand HandLeft;
        public OVRHand DominantHand;
        public OVRSkeleton DominantSkeleton;
        public IList<OVRBone> DominantBones;
        public OVRBone DominantThumbTip;
        public OVRBone DominantIndexTip;
        public OVRHand InferiorHand;
        public OVRSkeleton InferiorSkeleton;
        public IList<OVRBone> InferiorBones;
        public OVRBone InferiorThumbTip;
        public OVRBone InferiorIndexTip;
        public Transform DominantFingerCardAnchor;
        public Transform DominantHandCardAnchor;
        public Transform InferiorFingerCardAnchor;
        public Transform InferiorHandCardAnchor;

        public IList<OVRBone> BonesRight
        {
            get
            {
                if (Dominant == HandsManager.Side.Right)
                {
                    return DominantBones;
                }
                else if (Dominant == HandsManager.Side.Left)
                {
                    return InferiorBones;
                }
                else
                    return null;
            }
        }

        public IList<OVRBone> BonesLeft
        {
            get
            {
                if (Dominant == HandsManager.Side.Right)
                {
                    return InferiorBones;
                }
                else if (Dominant == HandsManager.Side.Left)
                {
                    return DominantBones;
                }
                else
                    return null;
            }
        }

        private void Start()
        {
            Dominant = Side.Unknown;
            DominantBones = new List<OVRBone>();
            InferiorBones = new List<OVRBone>();
        }

        void Update()
        {
            detectHands();
            detectSkeleton();
            detectBones();
            detectFingers();
            detectAnchors();
        }

        public bool IsTrackHighConfidence(Side side)
        {
            if (side == Side.Left)
            {
                if (HandLeft != null)
                    return HandLeft.IsDataHighConfidence;
                else
                    return false;
            }
            else if (side == Side.Right)
            {
                if (HandRight != null)
                    return HandRight.IsDataHighConfidence;
                else
                    return false;
            }
            else
                return false;
        }

        private void detectHands()
        {
            if (HandRight is null)
            {
                object[] foundOVRHandObjects = FindObjectsOfType(typeof(OVRHand));
                foreach (object obj in foundOVRHandObjects)
                {
                    OVRHand hand = (OVRHand)obj;
                    if (hand.name.Contains("Right") || hand.name.Contains("_R"))
                    {
                        HandRight = hand;
                    }
                }
            }

            if (HandLeft is null)
            {
                object[] foundOVRHandObjects = FindObjectsOfType(typeof(OVRHand));
                foreach (object obj in foundOVRHandObjects)
                {
                    OVRHand hand = (OVRHand)obj;
                    if (hand.name.Contains("Left") || hand.name.Contains("_L"))
                    {
                        HandLeft = hand;
                    }
                }
            }

            if (DominantHand is null)
            {
                if (HandRight.IsDominantHand)
                {
                    DominantHand = HandRight;
                    InferiorHand = HandLeft;
                    Dominant = Side.Right;
                }
                else if (HandLeft.IsDominantHand)
                {
                    DominantHand = HandLeft;
                    InferiorHand = HandRight;
                    Dominant = Side.Left;
                }
                else
                {
                    DominantHand = HandRight;
                    InferiorHand = HandLeft;
                    Dominant = Side.Right;
                }
            }
        }

        private void detectSkeleton()
        {
            if (DominantSkeleton is null)
            {
                if (DominantHand != null)
                {
                    object[] foundOVRSkeletonObjects = FindObjectsOfType(typeof(OVRSkeleton));
                    foreach (object obj in foundOVRSkeletonObjects)
                    {
                        OVRSkeleton skeleton = (OVRSkeleton)obj;
                        if (skeleton.name.Contains("Right") || skeleton.name.Contains("_R"))
                        {
                            if (Dominant == Side.Right)
                            {
                                DominantSkeleton = skeleton;
                            }
                            else if (Dominant == Side.Left)
                            {
                                InferiorSkeleton = skeleton;
                            }
                            else
                                throw new System.Exception("Dominant Side not set.");
                        } 
                        else if (skeleton.name.Contains("Left") || skeleton.name.Contains("_L"))
                        {
                            if (Dominant == Side.Left)
                            {
                                DominantSkeleton = skeleton;
                            }
                            else if (Dominant == Side.Right)
                            {
                                InferiorSkeleton = skeleton;
                            }
                            else
                                throw new System.Exception("Dominant Side not set.");
                        }
                    }
                }
            }
        }

        private void detectBones()
        {
            if (DominantSkeleton != null)
            {
                if (DominantSkeleton.GetCurrentNumBones() > 0)
                {
                    if (DominantSkeleton.GetCurrentNumBones() != DominantBones.Count)
                        DominantBones = DominantSkeleton.Bones;
                } 
                else
                {
                    DominantBones.Clear();
                }
            }

            if (InferiorSkeleton != null)
            {
                if (InferiorSkeleton.GetCurrentNumBones() > 0)
                {
                    if (InferiorSkeleton.GetCurrentNumBones() != InferiorBones.Count)
                        InferiorBones = InferiorSkeleton.Bones;
                }
                else
                {
                    InferiorBones.Clear();
                }
            }
        }

        private void detectFingers()
        {
            if (DominantBones.Count > 0)
            {
                if (DominantIndexTip == null || DominantThumbTip == null)
                {
                    foreach (OVRBone bone in DominantBones)
                    {
                        if (bone.Id == OVRSkeleton.BoneId.Hand_ThumbTip)
                        {
                            DominantThumbTip = bone;
                        }
                        else if (bone.Id == OVRSkeleton.BoneId.Hand_IndexTip)
                        {
                            DominantIndexTip = bone;
                        }
                    }
                }
            }
            else
            {
                DominantIndexTip = null;
                DominantThumbTip = null;
            }

            if (InferiorBones.Count > 0)
            {
                if (InferiorIndexTip == null || InferiorThumbTip == null)
                {
                    foreach (OVRBone bone in InferiorBones)
                    {
                        if (bone.Id == OVRSkeleton.BoneId.Hand_ThumbTip)
                        {
                            InferiorThumbTip = bone;
                        }
                        else if (bone.Id == OVRSkeleton.BoneId.Hand_IndexTip)
                        {
                            InferiorIndexTip = bone;
                        }
                    }
                }
            }
            else
            {
                InferiorIndexTip = null;
                InferiorThumbTip = null;
            }
        }

        private void detectAnchors()
        {
            if (DominantHand != null)
            {
                if (DominantHandCardAnchor == null)
                {
                    Transform parent = DominantHand.gameObject.transform.parent;
                    Transform DHCA = parent.Find("DominantHandCardAnchor");
                    if (DHCA == null)
                    {
                        GameObject go = new GameObject();
                        go.transform.parent = parent;
                        DominantHandCardAnchor = go.transform;
                        DominantHandCardAnchor.gameObject.name = "DominantHandCardAnchor";
                    }
                    else
                        DominantHandCardAnchor = DHCA;
                }

                if (DominantFingerCardAnchor == null)
                {
                    Transform parent = DominantHand.gameObject.transform.parent;
                    Transform DFCA = parent.Find("DominantFingerCardAnchor");
                    if (DFCA == null)
                    {
                        GameObject go = new GameObject();
                        go.transform.parent = parent;
                        DominantFingerCardAnchor = go.transform;
                        DominantFingerCardAnchor.gameObject.name = "DominantFingerCardAnchor";
                        DominantFingerCardAnchor.gameObject.AddComponent(typeof(AlignToIndexFinger));
                    }
                    else
                        DominantFingerCardAnchor = DFCA;
                }
                
                
            }

            if (InferiorHand != null)
            {
                if (InferiorHandCardAnchor == null)
                {
                    Transform parent = InferiorHand.gameObject.transform.parent;
                    Transform IHCA = parent.Find("InferiorHandCardAnchor");
                    if (IHCA == null)
                    {
                        GameObject go = new GameObject();
                        go.transform.parent = parent;
                        InferiorHandCardAnchor = go.transform;
                        InferiorHandCardAnchor.gameObject.name = "InferiorHandCardAnchor";
                    }
                    else
                        InferiorHandCardAnchor = IHCA;
                }

                if (InferiorFingerCardAnchor == null)
                {
                    Transform parent = InferiorHand.gameObject.transform.parent;
                    Transform IFCA = parent.Find("DominantFingerCardAnchor");
                    if (IFCA == null)
                    {
                        GameObject go = new GameObject();
                        go.transform.parent = parent;
                        InferiorFingerCardAnchor = go.transform;
                        InferiorFingerCardAnchor.gameObject.name = "InferiorFingerCardAnchor";
                    }
                    else
                        InferiorFingerCardAnchor = IFCA;
                }


            }

        }

    }
}
