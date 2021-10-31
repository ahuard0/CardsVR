using System;
using System.Collections.Generic;
using UnityEngine;

namespace CardsVR.Utility
{
    public static class Toolbox
    {
        public static GameObject FindInActiveObjectByName(string name)
        {
            Transform[] objs = Resources.FindObjectsOfTypeAll<Transform>() as Transform[];
            for (int i = 0; i < objs.Length; i++)
            {
                if (objs[i].hideFlags == HideFlags.None)
                {
                    if (objs[i].name == name)
                    {
                        return objs[i].gameObject;
                    }
                }
            }
            return null;
        }
        public static List<GameObject> FindInActiveObjectsByPattern(string pattern)
        {
            Transform[] objs = Resources.FindObjectsOfTypeAll<Transform>() as Transform[];
            List<GameObject> GOs = new List<GameObject>();
            for (int i = 0; i < objs.Length; i++)
            {
                if (objs[i].hideFlags == HideFlags.None)
                {
                    if (System.Text.RegularExpressions.Regex.IsMatch(objs[i].name, pattern))
                    {
                        GOs.Add(objs[i].gameObject);
                    }
                }
            }
            return GOs;
        }

        // Returns last index of the value that is the minimum.
        public static int IndexOfMin(IEnumerable<float> source)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            float minValue = float.MaxValue;
            int minIndex = -1;
            int index = -1;

            foreach (int num in source)
            {
                index++;

                if (num <= minValue)
                {
                    minValue = num;
                    minIndex = index;
                }
            }

            if (index == -1)
                throw new InvalidOperationException("Sequence was empty");

            return minIndex;
        }

        public static int IndexOfMin(IEnumerable<double> source)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            double minValue = double.MaxValue;
            int minIndex = -1;
            int index = -1;

            foreach (int num in source)
            {
                index++;

                if (num <= minValue)
                {
                    minValue = num;
                    minIndex = index;
                }
            }

            if (index == -1)
                throw new InvalidOperationException("Sequence was empty");

            return minIndex;
        }

        public static float Sum(this IEnumerable<float> source)
        {
            if (source == null) throw new ArgumentNullException("source");
            double sum = 0;
            foreach (float v in source) sum += v;
            return (float)sum;
        }

    }
}

