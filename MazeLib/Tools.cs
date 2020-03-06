using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Drawing;

namespace MazeMaker
{
    public static class Tools
    {
        public static bool FileExists(string inp)
        {
            return File.Exists(inp);            
        }


        public static string GetShortFileName(string fname)
        {
            int startIndex = fname.LastIndexOf('\\');
            int lastIndex = fname.LastIndexOf('.');
            if (startIndex == -1 || lastIndex == -1) return " ";
            string outStr = fname.Substring(startIndex, lastIndex - startIndex);
            if(outStr.StartsWith("\\"))
                outStr = outStr.Substring(1);
            return outStr;
        }

        public static bool FolderExists(string inp)
        {
            //bool ret=false;
            DirectoryInfo a = new DirectoryInfo(inp);            
            return a.Exists;
        }

        public static bool CreateMissingFolder(string inp)
        {
            DirectoryInfo a = new DirectoryInfo(inp);
            if (a.Exists == false)
            {
                a.Create();
                return true;
            }
            return false;
        }

        public static string getStringFromAttribute(XmlNode node, String attribute, String ifNull = "")
        {
            string output = ifNull;
            if (node == null)
                return output;
            XmlAttribute att = node.Attributes[attribute];
            if (att != null)
                output = att.InnerText;
            return output;
        }

        public static double getDoubleFromAttribute(XmlNode node, String attribute, double ifNull = -1)
        {
            double d = ifNull;
            if (node == null)
                return d;
            XmlAttribute att = node.Attributes[attribute];
            if (att != null)
                Double.TryParse(att.InnerText, out d);
            return d;
        }

        public static int getIntFromAttribute(XmlNode node, String attribute, int ifNull = -1)
        {
            int d = ifNull;
            if (node == null)
                return d;
            XmlAttribute att = node.Attributes[attribute];
            if (att != null)
                Int32.TryParse(att.InnerText, out d);
            return d;
        }

        public static bool getBoolFromAttribute(XmlNode node, String attribute, bool ifNull = false)
        {
            bool d = ifNull;
            if (node == null)
                return d;
            XmlAttribute att = node.Attributes[attribute];
            if (att != null)
                bool.TryParse(att.InnerText, out d);
            return d;
        }

        public static Color getColorFromNode(XmlNode node, string childNodeName="Color")
        {
            Color col = Color.White;
            if (node == null)
                return col;

            XmlNode colorNode = null;
            if (childNodeName.Length > 0)
                colorNode = node.SelectSingleNode(childNodeName);
            else
                colorNode = node;

            if (colorNode == null)
                return col;

            double r, g, b;
            r = getDoubleFromAttribute(colorNode, "r", 1);
            g = getDoubleFromAttribute(colorNode, "g", 1);
            b = getDoubleFromAttribute(colorNode, "b", 1);

            r = Math.Min(r, 255);
            g = Math.Min(g, 255);
            b = Math.Min(b, 255);

            if (r>1&&g>1&&b>1)
                col = Color.FromArgb((int)(r), (int)(g), (int)(b)); 
            else
                col = Color.FromArgb((int)(255 * r), (int)(255 * g), (int)(255 * b));
            return col;
        }

        public static MPoint getXYZfromNode(XmlNode node, int point = 0, string attString = "MzPoint")
        {
            if (node == null)
                return (new MPoint(0, 0, 0));

            if (attString.Length > 0)
            { 
                if (point > 0)
                    attString = attString + point;
            }
            XmlNode pointNode=node.SelectSingleNode(attString);

            if (pointNode == null)
                return (new MPoint(0, 0, 0));

            double mzX=0, mzY=0, mzZ=0;

            if(point>=0)
            { 
                mzX = getDoubleFromAttribute(pointNode, "x", 0);
                mzY = getDoubleFromAttribute(pointNode, "y", 0);
                mzZ = getDoubleFromAttribute(pointNode, "z", 0);
            }
            else if (point == -1) //rotation coordinates
            {
                mzX = getDoubleFromAttribute(pointNode, "rotX", 0);
                mzY = getDoubleFromAttribute(pointNode, "rotY", 0);
                mzZ = getDoubleFromAttribute(pointNode, "rotZ", 0);
            }
            if (point == -2) // for end regions
            {
                mzX = getDoubleFromAttribute(pointNode, "x1", 0);
                mzY = getDoubleFromAttribute(pointNode, "y1", 0);
                mzZ = getDoubleFromAttribute(pointNode, "z1", 0);
            }
            else if (point == -3)
            {
                mzX = getDoubleFromAttribute(pointNode, "x2", 0);
                mzY = getDoubleFromAttribute(pointNode, "y2", 0);
                mzZ = getDoubleFromAttribute(pointNode, "z2", 0);
            }
            return new MPoint(mzX, mzY, mzZ);
        }
        

        public static TPoint getTexCoordFromNode(XmlNode node, int point = 0, bool isCeiling = false)
        {
            if (node == null)
                return (new TPoint(0, 0));
            XmlNode pointNode;
            if (point <= 0)
                pointNode = node.SelectSingleNode("MzPoint");
            else
                pointNode = node.SelectSingleNode("MzPoint" + point);

            if (pointNode == null)
                return (new TPoint(0, 0));


            double texX = 0, texY = 0;

            switch (point)
            {
                case 1:
                    texX = 0; texY = 0;
                    break;
                case 2:
                    texX = 0; texY = 1;
                    break;
                case 3:
                    texX = 1; texY = 1;
                    break;
                case 4:
                    texX = 1; texY = 0;
                    break;
            }

            if (!isCeiling)
            {
                texX = getDoubleFromAttribute(pointNode, "texX", texX);
                texY = getDoubleFromAttribute(pointNode, "texY", texY);
            }
            else
            {
                texX = getDoubleFromAttribute(pointNode, "texX_Ceiling", texX);
                texY = getDoubleFromAttribute(pointNode, "texY_Ceiling", texY);
            }


            return new TPoint((float)texX, (float)texY);
        }

        public static RectangleF GetMinRectangle(PointF point1,PointF point2)
        {
            return new RectangleF(Math.Min(point1.X, point2.X), Math.Min(point1.Y, point2.Y), Math.Abs(point1.X - point2.X), Math.Abs(point2.Y - point1.Y));
        }

        public static PointF RotatePoint(PointF pointToRotate, PointF centerPoint, double angleInDegrees)
        {
            double angleInRadians = angleInDegrees * (Math.PI / 180);
            double cosTheta = Math.Cos(angleInRadians);
            double sinTheta = Math.Sin(angleInRadians);
            return new PointF
            {
                X =
                    (float)
                    (cosTheta * (pointToRotate.X - centerPoint.X) -
                    sinTheta * (pointToRotate.Y - centerPoint.Y) + centerPoint.X),
                Y =
                    (float)
                    (sinTheta * (pointToRotate.X - centerPoint.X) +
                    cosTheta * (pointToRotate.Y - centerPoint.Y) + centerPoint.Y)
            };
        }

        //Compute the dot product AB . AC
        public static float DotProduct(PointF pointA, PointF pointB, PointF pointC)
        {
            PointF AB = new PointF();
            PointF BC = new PointF();
            AB.X = pointB.X - pointA.X;
            AB.Y = pointB.Y - pointA.Y;
            BC.X = pointC.X - pointB.X;
            BC.Y = pointC.Y - pointB.Y;
            float dot = AB.X * BC.X + AB.Y * BC.Y;

            return dot;
        }

        //Compute the cross product AB x AC
        public static float CrossProduct(PointF pointA, PointF pointB, PointF pointC)
        {
            PointF AB = new PointF();
            PointF AC = new PointF();
            AB.X = pointB.X - pointA.X;
            AB.Y = pointB.Y - pointA.Y;
            AC.X = pointC.X - pointA.X;
            AC.Y = pointC.Y - pointA.Y;
            float cross = AB.X * AC.Y - AB.Y * AC.X;

            return cross;
        }

        //Compute the distance from A to B
        public static float Distance(PointF pointA, PointF pointB)
        {
            float d1 = pointA.X - pointB.X;
            float d2 = pointA.Y - pointB.Y;

            return (float) Math.Sqrt(d1 * d1 + d2 * d2);
        }

        //Compute the distance from AB to C
        //if isSegment is true, AB is a segment, not a line.
        public static float LineToPointDistance2D(PointF pointA, PointF pointB, PointF pointC,
            bool isSegment)
        {
            float dist = CrossProduct(pointA, pointB, pointC) / Distance(pointA, pointB);
            if (isSegment)
            {
                float dot1 = DotProduct(pointA, pointB, pointC);
                if (dot1 > 0)
                    return Distance(pointB, pointC);

                float dot2 = DotProduct(pointB, pointA, pointC);
                if (dot2 > 0)
                    return Distance(pointA, pointC);
            }
            return (float)Math.Abs(dist);
        }

        public static float GetAngleDegree(PointF center, PointF target)
        {
            double n = - (Math.Atan2(center.Y - target.Y, center.X - target.X)) * 180 / Math.PI;
            return (float)n % 360;
        }

        // Find a circle through the three points.
        public static void FindCircle(PointF a, PointF b, PointF c,
            out PointF center, out float radius)
        {
            // Get the perpendicular bisector of (x1, y1) and (x2, y2).
            float x1 = (b.X + a.X) / 2;
            float y1 = (b.Y + a.Y) / 2;
            float dy1 = b.X - a.X;
            float dx1 = -(b.Y - a.Y);

            // Get the perpendicular bisector of (x2, y2) and (x3, y3).
            float x2 = (c.X + b.X) / 2;
            float y2 = (c.Y + b.Y) / 2;
            float dy2 = c.X - b.X;
            float dx2 = -(c.Y - b.Y);

            // See where the lines intersect.
            bool lines_intersect, segments_intersect;
            PointF intersection, close1, close2;
            FindIntersection(
                new PointF(x1, y1), new PointF(x1 + dx1, y1 + dy1),
                new PointF(x2, y2), new PointF(x2 + dx2, y2 + dy2),
                out lines_intersect, out segments_intersect,
                out intersection, out close1, out close2);
            if (!lines_intersect)
            {
                //MessageBox.Show("The points are colinear");
                center = new PointF(0, 0);
                radius = 0;
            }
            else
            {
                center = intersection;
                float dx = center.X - a.X;
                float dy = center.Y - a.Y;
                radius = (float)Math.Sqrt(dx * dx + dy * dy);
            }
        }

        // Find the point of intersection between
        // the lines p1 --> p2 and p3 --> p4.
        public static void FindIntersection(
            PointF p1, PointF p2, PointF p3, PointF p4,
            out bool lines_intersect, out bool segments_intersect,
            out PointF intersection,
            out PointF close_p1, out PointF close_p2)
        {
            // Get the segments' parameters.
            float dx12 = p2.X - p1.X;
            float dy12 = p2.Y - p1.Y;
            float dx34 = p4.X - p3.X;
            float dy34 = p4.Y - p3.Y;

            // Solve for t1 and t2
            float denominator = (dy12 * dx34 - dx12 * dy34);

            float t1 =
                ((p1.X - p3.X) * dy34 + (p3.Y - p1.Y) * dx34)
                    / denominator;
            if (float.IsInfinity(t1))
            {
                // The lines are parallel (or close enough to it).
                lines_intersect = false;
                segments_intersect = false;
                intersection = new PointF(float.NaN, float.NaN);
                close_p1 = new PointF(float.NaN, float.NaN);
                close_p2 = new PointF(float.NaN, float.NaN);
                return;
            }
            lines_intersect = true;

            float t2 =
                ((p3.X - p1.X) * dy12 + (p1.Y - p3.Y) * dx12)
                    / -denominator;

            // Find the point of intersection.
            intersection = new PointF(p1.X + dx12 * t1, p1.Y + dy12 * t1);

            // The segments intersect if t1 and t2 are between 0 and 1.
            segments_intersect =
                ((t1 >= 0) && (t1 <= 1) &&
                 (t2 >= 0) && (t2 <= 1));

            // Find the closest points on the segments.
            if (t1 < 0)
            {
                t1 = 0;
            }
            else if (t1 > 1)
            {
                t1 = 1;
            }

            if (t2 < 0)
            {
                t2 = 0;
            }
            else if (t2 > 1)
            {
                t2 = 1;
            }

            close_p1 = new PointF(p1.X + dx12 * t1, p1.Y + dy12 * t1);
            close_p2 = new PointF(p3.X + dx34 * t2, p3.Y + dy34 * t2);
        }

    }
}
