using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using CardsVR.Utility;

namespace CardsVR.Detection
{
    public class RegisteredTag : Tag
    {

        //public Vector3 worldCoords;
        public Vector3 modelCoords;
        public GameObject model;

        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;

        private Matrix4x4 transform;
        public Matrix4x4 transformation
        {
            get { return transform; }
            set
            {
                this.transform = value;

                // Get Tag Model->Camera->World Transform
                position = transform.ExtractPosition();
                rotation = transform.ExtractRotation();
                scale = transform.ExtractScale();
            }
        }

        public RegisteredTag() { }
        public RegisteredTag(Tag tag)
        {
            int id = tag.getUniqueID();
            this.info = TagInfo.getTagInfo(id);
            this.corner1 = tag.corner1;
            this.corner2 = tag.corner2;
            this.corner3 = tag.corner3;
            this.corner4 = tag.corner4;
            this.centroid = tag.centroid;


            // Get Card->Tag Center Translation Vector in Model Coords
            double tagCenterX = info.ModelX;
            double tagCenterY = info.ModelY;
            Vector3 tagCenter = new Vector3((float)tagCenterY / 0.3f, (float)tagCenterX / 0.3f, 0);

            // Correct Tag Position for Scaling
            if (id % 28 == 26 || id % 28 == 27)
            {
                this.modelCoords = new Vector3(3.333f / 2f, 3.333f / 2f, 0) - tagCenter;
            }
            else
            {
                this.modelCoords = new Vector3(0.5f, 0.5f, 0) - tagCenter;
            }

        }

    }

}
