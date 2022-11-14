using System.Collections.Generic;

namespace CardsVR.Detection
{
    [System.Serializable]
    public class ArucoData
    {
        public float[] corners;
        public int[] uniqueIDs;
        public int width;
        public int height;


        public ArucoData() { }
        public ArucoData(float[] corners, int[] uniqueIDs, int width, int height)
        {
            this.corners = corners;
            this.uniqueIDs = uniqueIDs;
            this.width = width;
            this.height = height;
        }

        public ArucoData(ArucoData state)
        {
            this.corners = state.corners;
            this.uniqueIDs = state.uniqueIDs;
            this.width = state.width;
            this.height = state.height;
        }

        public void Reset()
        {
            this.corners = null;
            this.uniqueIDs = null;
            this.width = 0;
            this.height = 0;
        }

        public int Count
        {
            get
            {
                if (uniqueIDs == null)
                    return 0;
                else
                    return uniqueIDs.Length; 
            }
        }

        public override string ToString()
        {
            return "Tag Count: " + uniqueIDs.Length.ToString();
        }


        private List<Tag> _tags;
        public List<Tag> Tags
        {
            get
            {
                _tags = new List<Tag>();
                if (uniqueIDs != null)
                {
                    for (int i = 0; i < uniqueIDs.Length; i++)
                    {
                        double x1 = width - corners[i * 8 + 0];
                        double y1 = corners[i * 8 + 1];
                        double x2 = width - corners[i * 8 + 2];
                        double y2 = corners[i * 8 + 3];
                        double x3 = width - corners[i * 8 + 4];
                        double y3 = corners[i * 8 + 5];
                        double x4 = width - corners[i * 8 + 6];
                        double y4 = corners[i * 8 + 7];
                        _tags.Add(new Tag(uniqueIDs[i], x1, y1, x2, y2, x3, y3, x4, y4));
                    }
                }
                return _tags;
            }
        }

    }
}
