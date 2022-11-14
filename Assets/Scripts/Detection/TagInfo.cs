using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace CardsVR.Detection
{
    public class TagInfo
    {
        public int UniqueID;
        public int TagID;
        public int CardID;
        public int DictID;
        public double ModelX;
        public double ModelY;
        public string CardRank;
        public string CardSuit;
        public string CardSide;

        public TagInfo() { }
        public TagInfo(int UniqueID, int TagID, int CardID, string CardRank, string CardSuit, string CardSide, int DictID, double ModelX, double ModelY)
        {
            this.UniqueID = UniqueID;
            this.TagID = TagID;
            this.CardID = CardID;
            this.DictID = DictID;
            this.ModelX = ModelX;
            this.ModelY = ModelY;
            this.CardRank = CardRank;
            this.CardSuit = CardSuit;
            this.CardSide = CardSide;
        }

        private static Dictionary<int, TagInfo> dict;
        public static Dictionary<int, TagInfo> dictionary
        {
            get
            {
                if (dict == null)
                {
                    dict = importCSV();
                }
                return dict;
            }
        }

        private static Dictionary<int, TagInfo> importCSV()
        {
            string filename_csv = "card_tag_centers_inches";
            TextAsset text_csv = Resources.Load<TextAsset>(filename_csv);
            return importCSV(text_csv);
        }

        private static Dictionary<int, TagInfo> importCSV(TextAsset txt)
        {
            Dictionary<int, TagInfo> newDict = new Dictionary<int, TagInfo>();

            // Load Tag->Card Center Vector CSV File
            string[] lines = txt.text.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string line in lines)
            {
                string[] fields = line.Replace(", ", ",").Split(',');

                int UniqueID = Convert.ToInt32(fields[0]);
                int CardID = Convert.ToInt32(fields[1]);
                string CardRank = fields[2];
                string CardSuit = fields[3];
                string CardSide = fields[4];
                int DictID = Convert.ToInt32(fields[5]);
                int TagID = Convert.ToInt32(fields[6]);
                double ModelX = Convert.ToDouble(fields[7]);
                double ModelY = Convert.ToDouble(fields[8]);

                newDict.Add(UniqueID, new TagInfo(UniqueID, TagID, CardID, CardRank, CardSuit, CardSide, DictID, ModelX, ModelY));
            }

            return newDict;
        }

        public static TagInfo getTagInfo(int uniqueID)
        {
            return dictionary[uniqueID];
        }

        public static TagInfo getTagInfoByDictCodeTagID(int dictCode, int tagID)
        {
            for (int i = 0; i < dictionary.Count; i++)
            {
                if (dictionary[i].DictID == dictCode && dictionary[i].TagID == tagID)
                {
                    return dictionary[i];
                }
            }
            return new TagInfo();
        }

        public static int getDictCodeFromArucoDict(int ArucoDict)
        {
            if (ArucoDict == OpenCVForUnity.ArucoModule.Aruco.DICT_4X4_1000)
            {
                return 0;
            }
            else if (ArucoDict == OpenCVForUnity.ArucoModule.Aruco.DICT_5X5_1000)
            {
                return 1;
            }
            else if (ArucoDict == OpenCVForUnity.ArucoModule.Aruco.DICT_6X6_1000)
            {
                return 2;
            }
            else
            {
                throw new Exception("Aruco Dictionary not mapped to Dictionary Code");
            }
        }

        public static TagInfo getTagInfoFromArucoDictTagID(int tagID, int ArucoDict)
        {
            return getTagInfoByDictCodeTagID(getDictCodeFromArucoDict(ArucoDict), tagID);
        }

        public static int getUniqueIDFromArucoDictTagID(int tagID, int ArucoDict)
        {
            return getTagInfoFromArucoDictTagID(tagID, ArucoDict).UniqueID;
        }
    }
}
