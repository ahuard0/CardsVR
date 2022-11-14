using CardsVR.Interaction;
using CardsVR.Networking;
using CardsVR.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CardsVR.Commands
{
    /*
     *   TableHeightHandsClient follows the Command design pattern as a receiver class.
     *   
     *   Receiver classes are implemented by the Execute() methods of the corresponding Command classes.
     *   There is no Receiver interface.
     *   
     *   As a static class, the methods may be called independently of Command classes, such as during
     *   unit testing.
     */
    public class TableHeightHandsClient : MonoBehaviour
    {
        [Header("Parameters")]
        [SerializeField]
        private float tableSyncInterval = 1f;
        [SerializeField]
        private float ThresholdStdDevFingerY;
        [SerializeField]
        private float MinStdDevFingerDist;

        [Header("Game Objects")]
        [SerializeField]
        private GameObject Tabletop;
        private float initialHeight;
        [SerializeField]
        private GameObject Table;
        [SerializeField]
        private GameObject[] Chairs;

        [Header("Data")]
        public float AbsoluteTableHeight;
        public float AbsTableHeightPlayer1;
        public float AbsTableHeightPlayer2;
        public float AbsTableHeightPlayer3;
        public float AbsTableHeightPlayer4;

        /*
         *      Unity MonoBehavior callback used to perform actions on startup.
         *      
         *      This method starts the coroutine that synchronizes the table height
         *      periodically, broadcasting the player's table height across all
         *      clients.
         *      
         *      Parameters
         *      ----------
         *      None
         *      
         *      Returns
         *      -------
         *      None
         */

        private void Start()
        {
            initialHeight = Tabletop.transform.position.y;
            StartCoroutine("SyncTable");
        }

        private void CalibrateTableHeight()
        {
            if (HandsManager.Instance.IsTrackHighConfidence(HandsManager.Side.Left) && 
                HandsManager.Instance.IsTrackHighConfidence(HandsManager.Side.Right))  // Both Left and Right Hands are Tracked
            {
                IList<OVRBone> BonesLeft = HandsManager.Instance.BonesLeft;
                IList<OVRBone> BonesRight = HandsManager.Instance.BonesRight;

                if (BonesRight == null || BonesLeft == null)  // integrity check
                    return;

                List<float> FingerTipsLeft = GetFingerTipHeights(BonesLeft);
                List<float> FingerTipsRight = GetFingerTipHeights(BonesRight);

                float StdDevLeft = (float)MathTools.StdDev(FingerTipsLeft);
                float StdDevRight = (float)MathTools.StdDev(FingerTipsRight);
                if (StdDevLeft < ThresholdStdDevFingerY && StdDevRight < ThresholdStdDevFingerY)  // process only if fingers of each hand are stable
                {

                    float AvgLeft = (float)MathTools.Avg(FingerTipsLeft);
                    float AvgRight = (float)MathTools.Avg(FingerTipsRight);

                    if (Mathf.Abs(AvgLeft - AvgRight) < ThresholdStdDevFingerY)  // hands are in the same plane
                    {
                        List<float> FingerTipDistancesLeft = GetFingerTipDistances(BonesLeft);
                        List<float> FingerTipDistancesRight = GetFingerTipDistances(BonesRight);
                        float AvgDistLeft = MathTools.Avg(FingerTipDistancesLeft);
                        float AvgDistRight = MathTools.Avg(FingerTipDistancesRight);

                        if (AvgDistLeft > MinStdDevFingerDist && AvgDistRight > MinStdDevFingerDist)  // fingers are webbed
                        {
                            Debug.LogFormat("Calibrate Table Height.  AvgLeft: {0}, AvgRight: {1}, StdDevLeft: {2}, StdDevRight: {3}, AvgDistLeft: {4}, AvgDistRight: {5}", AvgLeft, AvgRight, StdDevLeft, StdDevRight, AvgDistLeft, AvgDistRight);
                            float AvgFingerTip = (AvgLeft + AvgRight) / 2f;
                            this.AbsoluteTableHeight = AvgFingerTip;

                            AdjustTableHeight(AvgFingerTip);
                        }
                    }
                }

            }
        }

        private List<float> GetFingerTipHeights(IList<OVRBone> bones)
        {
            List<float> FingerTips = new List<float>();

            foreach (OVRBone bone in bones)  // process bones
            {
                if (bone.Id == OVRSkeleton.BoneId.Hand_ThumbTip ||
                    bone.Id == OVRSkeleton.BoneId.Hand_IndexTip ||
                    bone.Id == OVRSkeleton.BoneId.Hand_MiddleTip ||
                    bone.Id == OVRSkeleton.BoneId.Hand_RingTip ||
                    bone.Id == OVRSkeleton.BoneId.Hand_PinkyTip)  // only return finger tips
                {
                    FingerTips.Add(bone.Transform.position.y);
                }
            }

            return FingerTips;
        }

        private List<float> GetFingerTipDistances(IList<OVRBone> bones)
        {
            List<float> FingerTipDistances = new List<float>();

            OVRBone Index = null;
            OVRBone Middle = null;
            OVRBone Ring = null;
            OVRBone Pinky = null;

            foreach (OVRBone bone in bones)  // process bones
            {
                if (bone.Id == OVRSkeleton.BoneId.Hand_IndexTip)
                    Index = bone;
                else if (bone.Id == OVRSkeleton.BoneId.Hand_MiddleTip)
                    Middle = bone;
                else if (bone.Id == OVRSkeleton.BoneId.Hand_RingTip)
                    Ring = bone;
                else if (bone.Id == OVRSkeleton.BoneId.Hand_PinkyTip)
                    Pinky = bone;
            }

            if (Index != null && Middle != null)
                FingerTipDistances.Add(Mathf.Abs(Vector3.Distance(Index.Transform.position, Middle.Transform.position)));

            if (Middle != null && Ring != null)
                FingerTipDistances.Add(Mathf.Abs(Vector3.Distance(Middle.Transform.position, Ring.Transform.position)));

            if (Ring != null && Pinky != null)
                FingerTipDistances.Add(Mathf.Abs(Vector3.Distance(Ring.Transform.position, Pinky.Transform.position)));

            return FingerTipDistances;
        }

        private void AdjustTableHeight(float height)
        {
            // Scale Table
            float Scale_Table = AbsoluteTableHeight / initialHeight;
            Vector3 scale = Table.transform.localScale;
            scale.y = Scale_Table;
            Table.transform.localScale = scale;

            // Scale Tabletop
            Vector3 position = Tabletop.transform.position;
            position.y = height;
            Tabletop.transform.position = position;

            // Scale Chairs
            foreach (GameObject chair in Chairs)
            {
                scale = chair.transform.localScale;
                scale.y = Scale_Table;
                chair.transform.localScale = scale;
            }

            // Recenter OVR Pose
            //Debug.Log("Recenter OVR Pose...");
            //OVRManager.display.RecenterPose();
        }

        /*
         *      A coroutine used to sync the table model data across clients with a time 
         *      delay between sync operations.
         *      
         *      Parameters
         *      ----------
         *      None
         *      
         *      Returns
         *      -------
         *      None
         */
        private IEnumerator SyncTable()
        {
            while (true)
            {
                CalibrateTableHeight();
                //BroadcastTableData();
                yield return new WaitForSecondsRealtime(tableSyncInterval);
            }
        }

        /*
         *      Sync the table height model data across clients.
         *      
         *      Parameters
         *      ----------
         *      None
         *      
         *      Returns
         *      -------
         *      None
         */
        private void BroadcastTableData()
        {
            int PlayerID = PlayerManager.Instance.PlayerNum;
            TableHeightData data = new TableHeightData(PlayerID, AbsoluteTableHeight);
            SendData command = new SendData(data: data, SendReliable: false, ReceiveLocally: true);
            Invoker.Instance.SetCommand(command);
            Invoker.Instance.ExecuteCommand(true);  // record command history
        }

        public void ReceiveTableData(TableHeightData data)
        {
            if (data.PlayerID == 1)
                AbsTableHeightPlayer1 = data.AbsoluteTableHeight;
            else if (data.PlayerID == 2)
                AbsTableHeightPlayer2 = data.AbsoluteTableHeight;
            else if (data.PlayerID == 3)
                AbsTableHeightPlayer3 = data.AbsoluteTableHeight;
            else if (data.PlayerID == 4)
                AbsTableHeightPlayer4 = data.AbsoluteTableHeight;
        }
    }
}
