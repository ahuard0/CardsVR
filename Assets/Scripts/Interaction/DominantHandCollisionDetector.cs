using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace CardsVR.Interaction
{
    public class DominantHandCollisionDetector : Singleton<DominantHandCollisionDetector>, ISubject
    {
        private HandsManager HM;
        private ArrayList _observers = new ArrayList();
        private List<RaycastHit> hits = new List<RaycastHit>();
        private List<int> piles = new List<int>();
        public string RayTraceLayer = "Cards";

        public int pileHit;

        private List<RaycastHit> hit_candidates = new List<RaycastHit>();

        private void Start()
        {
            HM = HandsManager.Instance;
        }

        private void Update()
        {
            if (HandsManager.Instance.DominantThumbTip == null && HandsManager.Instance.DominantIndexTip == null)  // Test for HandTracking
                return;

            hits.Clear();
            piles.Clear();
            hit_candidates.Clear();
            hit_candidates = PerformRayTrace();

            foreach (RaycastHit hit in hit_candidates)
            {
                string obj_name = hit.transform.name; // Object Name Intersected

                if (obj_name.Contains("Pile") && obj_name.Contains("Collider"))  // Test if a valid pile naming convention (e.g., Pile2Collider)
                {
                    int pile_num = int.Parse(obj_name[4].ToString());  // Get Pile Number
                    piles.Add(pile_num);  // Update detector view
                    hits.Add(hit);  // Update detector view
                }
            }

            if (hits.Count > 0 && piles.Count > 0)  // Notify observers if hit was observed.
            {
                int[] pileArray = piles.ToArray();
                pileHit = pileArray[0];  // Only broadcast the first detection

                NotifyObservers();
            }
        }

        private List<RaycastHit> PerformRayTrace()
        {
            RaycastHit[] hits1;
            RaycastHit[] hits2;
            LayerMask mask = LayerMask.GetMask(RayTraceLayer);

            // Ray Tracing Distance: Index-to-Thumb
            Vector3 pos_thumb = HM.DominantThumbTip.Transform.position;
            Vector3 pos_index = HM.DominantIndexTip.Transform.position;
            float dist_hand_ref = Vector3.Distance(pos_thumb, pos_index);

            // Calculate Ray Tracing End Points
            Vector3 pos_primary = pos_index;
            Vector3 pos_ref = new Vector3(pos_primary.x, pos_primary.y - dist_hand_ref, pos_primary.z); // project vector to floor length of index-to-thumb
            float distance = Vector3.Distance(pos_ref, pos_primary);

            // Peform Ray Tracing on all objects in the Cards layer
            hits1 = Physics.RaycastAll(pos_index, -1 * (pos_index - pos_thumb), distance, mask, QueryTriggerInteraction.Ignore);
            hits2 = Physics.RaycastAll(pos_primary, -1 * (pos_primary - pos_ref), distance, mask, QueryTriggerInteraction.Ignore);

            // Combine Ray Tracing Techniques : First Detection Returns
            List<RaycastHit> hitList = new List<RaycastHit>();
            hitList.AddRange(hits1);
            hitList.AddRange(hits2);

            return hitList;
        }

        public void AttachObserver(IObserver observer)
        {
            _observers.Add(observer);
        }

        public void DetachObserver(IObserver observer)
        {
            _observers.Remove(observer);
        }

        public void NotifyObservers()
        {
            try
            {
                foreach (IObserver observer in _observers)
                    observer.Notify();
            }
            catch (System.InvalidOperationException) { }  // Occurs when cards are transferred and observers added or detached while the subject is inside the foreach loop.  This try/catch block acts as a break for the loop whe this event occurs.
        }
    }

}

