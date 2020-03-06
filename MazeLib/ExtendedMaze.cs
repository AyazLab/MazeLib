using System;
using System.Collections.Generic;
using System.Text;
using MazeMaker;

namespace MazeLib
{
    public class ExtendedMaze : Maze
    {
        public ExtendedMaze(int index)
        {
            Index = index;
        }

        public int Index;

        //necessary to find bar...
        public double minY, maxY;
        public double minX, maxX;
        public double midStartX, midStartY;


        public void SetMinMaxValues()
        {
            minX = double.MaxValue;
            minY = double.MaxValue;
            maxX = double.MinValue;
            maxY = double.MinValue;

            midStartX = 0;
            midStartY = 0;

            foreach (Floor f in cFloor)
            {
                //if (f.Rect.Left < minX)
                //    minX = f.Rect.Left;
                //else if (f.Rect.Right > maxX)
                //    maxX = f.Rect.Right;

                //if (f.Rect.Top < minY)
                //    minY = f.Rect.Top;
                //else if (f.Rect.Bottom > maxY)
                //    maxY = f.Rect.Bottom;

                minX = Math.Min(minX, f.Rect.Left);
                minY = Math.Min(minY, f.Rect.Top);

                maxX = Math.Max(maxX, f.Rect.Right);
                maxY = Math.Max(maxY, f.Rect.Bottom);
            }

            foreach (Wall l in cWall)
            {
                minX = Math.Min(minX, l.ScrPoint1.X);
                minX = Math.Min(minX, l.ScrPoint2.X);
                minY = Math.Min(minY, l.ScrPoint1.Y);
                minY = Math.Min(minY, l.ScrPoint2.Y);

                maxX = Math.Max(maxX, l.ScrPoint1.X);
                maxX = Math.Max(maxX, l.ScrPoint2.X);
                maxY = Math.Max(maxY, l.ScrPoint1.Y);
                maxY = Math.Max(maxY, l.ScrPoint2.Y);
                /*
                if (l.ScrPoint1.X < minX)
                    minX = l.ScrPoint1.X;
                else if (l.ScrPoint1.X > maxX)
                    maxX = l.ScrPoint1.X;

                if (l.ScrPoint2.X < minX)
                    minX = l.ScrPoint2.X;
                else if (l.ScrPoint2.X > maxX)
                    maxX = l.ScrPoint2.X;

                if (l.ScrPoint1.Y < minY)
                    minY = l.ScrPoint1.Y;
                else if (l.ScrPoint1.Y > maxY)
                    maxY = l.ScrPoint1.Y;

                if (l.ScrPoint2.Y < minY)
                    minY = l.ScrPoint2.Y;
                else if (l.ScrPoint2.Y > maxY)
                    maxY = l.ScrPoint2.Y;*/
            }

            foreach (CurvedWall l in cCurveWall)
            {
                minX = Math.Min(minX, l.ScrPoint1.X);
                minX = Math.Min(minX, l.ScrPoint2.X);
                minY = Math.Min(minY, l.ScrPoint1.Y);
                minY = Math.Min(minY, l.ScrPoint2.Y);

                maxX = Math.Max(maxX, l.ScrPoint1.X);
                maxX = Math.Max(maxX, l.ScrPoint2.X);
                maxY = Math.Max(maxY, l.ScrPoint1.Y);
                maxY = Math.Max(maxY, l.ScrPoint2.Y);

                maxX = Math.Max(maxX, l.ScrPointMid.X);
                maxX = Math.Max(maxX, l.ScrPointMid.X);
                maxY = Math.Max(maxY, l.ScrPointMid.Y);
                maxY = Math.Max(maxY, l.ScrPointMid.Y);
                /*
                if (l.ScrPoint1.X < minX)
                    minX = l.ScrPoint1.X;
                else if (l.ScrPoint1.X > maxX)
                    maxX = l.ScrPoint1.X;

                if (l.ScrPoint2.X < minX)
                    minX = l.ScrPoint2.X;
                else if (l.ScrPoint2.X > maxX)
                    maxX = l.ScrPoint2.X;

                if (l.ScrPoint1.Y < minY)
                    minY = l.ScrPoint1.Y;
                else if (l.ScrPoint1.Y > maxY)
                    maxY = l.ScrPoint1.Y;

                if (l.ScrPoint2.Y < minY)
                    minY = l.ScrPoint2.Y;
                else if (l.ScrPoint2.Y > maxY)
                    maxY = l.ScrPoint2.Y;*/
            }

            foreach (Light li in cLight)
            {
                minX = Math.Min(minX, li.ScrPoint.X);
                minY = Math.Min(minY, li.ScrPoint.Y);

                maxX = Math.Max(maxX, li.ScrPoint.X);
                maxY = Math.Max(maxY, li.ScrPoint.Y);
            }

            foreach (StaticModel li in cStaticModels)
            {
                minX = Math.Min(minX, li.ScrPoint.X);
                minY = Math.Min(minY, li.ScrPoint.Y);

                maxX = Math.Max(maxX, li.ScrPoint.X);
                maxY = Math.Max(maxY, li.ScrPoint.Y);
            }

            foreach (DynamicObject li in cDynamicObjects)
            {
                minX = Math.Min(minX, li.ScrPoint.X);
                minY = Math.Min(minY, li.ScrPoint.Y);

                maxX = Math.Max(maxX, li.ScrPoint.X);
                maxY = Math.Max(maxY, li.ScrPoint.Y);
            }

            foreach (StartPos sPos in cStart)
            {
                //check start pos
                minX = Math.Min(minX, sPos.ScrPoint.X);
                minY = Math.Min(minY, sPos.ScrPoint.Y);

                maxX = Math.Max(maxX, sPos.ScrPoint.X);
                maxY = Math.Max(maxY, sPos.ScrPoint.Y);

                midStartX = midStartX + sPos.ScrPoint.X;
                midStartY = midStartY + sPos.ScrPoint.Y;
            }

            midStartX = midStartX / cStart.Count-minX;
            midStartY = midStartY / cStart.Count-minY;

            int MaxMazeImagDim = 20000;
            if (maxX - minX > MaxMazeImagDim)
            { 
                maxX = minX + MaxMazeImagDim; 
            }
            if (maxY - minY > MaxMazeImagDim)
            { 
                maxY = minY + MaxMazeImagDim;
            }



            //check end region

            //maxX += 40;
            //maxY += 40;
        }
    }
}
