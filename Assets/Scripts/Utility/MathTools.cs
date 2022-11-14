using System.Collections.Generic;
using UnityEngine;

namespace CardsVR.Utility
{
    public static class MathTools
    {
        public static float Avg(IEnumerable<float> values)
        {
            float sum = 0;
            int count = 0;
            foreach (float val in values)
            {
                sum += val;
                count += 1;
            }
                
            return sum / count;
        }

        public static float StdDev(IEnumerable<float> values)
        {
            float avg = Avg(values);

            float squ_sum = 0;
            int count = 0;
            foreach (float val in values)
            {
                squ_sum += Mathf.Pow(val - avg, 2f);
                count += 1;
            }
            float variance = squ_sum / count;

            return Mathf.Sqrt(variance);
        }
    }

}
