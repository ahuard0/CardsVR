using OpenCVForUnity.CoreModule;
using System.Collections.Generic;

namespace CardsVR.Detection
{
    public class Tag
    {
        public TagInfo info;
        public Point corner1;
        public Point corner2;
        public Point corner3;
        public Point corner4;
        public Point centroid;

        public Tag()
        {
            this.info = new TagInfo();
            this.corner1 = new Point(0, 0);
            this.corner2 = new Point(0, 0);
            this.corner3 = new Point(0, 0);
            this.corner4 = new Point(0, 0);
            this.ComputeCentroid();
        }

        public Tag(int tagID, int dictID, Point c1, Point c2, Point c3, Point c4)
        {
            this.info = TagInfo.getTagInfoByDictCodeTagID(dictID, tagID);
            this.corner1 = c1;
            this.corner2 = c2;
            this.corner3 = c3;
            this.corner4 = c4;
            this.ComputeCentroid();
        }

        public Tag(int uniqueID, Point c1, Point c2, Point c3, Point c4)
        {
            this.info = TagInfo.getTagInfo(uniqueID);
            this.corner1 = c1;
            this.corner2 = c2;
            this.corner3 = c3;
            this.corner4 = c4;
            this.ComputeCentroid();
        }

        public Tag(int tagID, int dictID, Point[] c)
        {
            this.info = TagInfo.getTagInfoByDictCodeTagID(dictID, tagID);

            this.corner1 = c[0];
            this.corner2 = c[1];
            this.corner3 = c[2];
            this.corner4 = c[3];
            this.ComputeCentroid();
        }

        public Tag(int uniqueID, Point[] c)
        {
            this.info = TagInfo.getTagInfo(uniqueID);

            this.corner1 = c[0];
            this.corner2 = c[1];
            this.corner3 = c[2];
            this.corner4 = c[3];
            this.ComputeCentroid();
        }

        public Tag(int tagID, int dictID, double x1, double y1, double x2, double y2, double x3, double y3, double x4, double y4)
        {
            this.info = TagInfo.getTagInfoByDictCodeTagID(dictID, tagID);
            this.corner1 = new Point(x1, y1);
            this.corner2 = new Point(x2, y2);
            this.corner3 = new Point(x3, y3);
            this.corner4 = new Point(x4, y4);
            this.ComputeCentroid();
        }

        public Tag(int uniqueID, double x1, double y1, double x2, double y2, double x3, double y3, double x4, double y4)
        {
            this.info = TagInfo.getTagInfo(uniqueID);
            this.corner1 = new Point(x1, y1);
            this.corner2 = new Point(x2, y2);
            this.corner3 = new Point(x3, y3);
            this.corner4 = new Point(x4, y4);
            this.ComputeCentroid();
        }

        public void ComputeCentroid()
        {
            double x_center = (corner1.x + corner2.x + corner3.x + corner4.x) / 4;
            double y_center = (corner1.y + corner2.y + corner3.y + corner4.y) / 4;
            this.centroid = new Point(x_center, y_center);
        }

        public int getDictID()
        {
            return this.info.DictID;
        }

        public int getUniqueID()
        {
            return this.info.UniqueID;
        }

        public int getTagID()
        {
            return this.info.TagID;
        }

        public double getModelX()
        {
            return this.info.ModelX;
        }

        public double getModelY()
        {
            return this.info.ModelY;
        }

        public Point3 getCentroid3()
        {
            this.ComputeCentroid();
            Point centroid2 = this.centroid;
            Point3 centroid3 = new Point3(centroid2.x, centroid2.y, 0);
            return centroid3;
        }

        public Point[] getCornersArray()
        {
            Point[] points = new Point[4];
            points[0] = this.corner1;
            points[1] = this.corner2;
            points[2] = this.corner3;
            points[3] = this.corner4;
            return points;
        }

        public MatOfPoint2f getCorners2()
        {
            Point[] cornersArray = this.getCornersArray();
            return new MatOfPoint2f(cornersArray);
        }

        public MatOfPoint3f getCorners3()
        {
            List<Point3> c_list = new List<Point3>();
            c_list.Add(new Point3(this.corner1.x, this.corner1.y, 0));
            c_list.Add(new Point3(this.corner2.x, this.corner2.y, 0));
            c_list.Add(new Point3(this.corner3.x, this.corner3.y, 0));
            c_list.Add(new Point3(this.corner4.x, this.corner4.y, 0));
            MatOfPoint3f c = new MatOfPoint3f();
            c.fromList(c_list);
            return c;
        }
    }
}
