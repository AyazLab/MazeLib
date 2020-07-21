using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MazeMaker;
using System.Drawing;

namespace MazeLib
{
    public class HeatmapItem
    {
        public enum Type
        {
            Presence, Entrance, Time
        }
        public Type type;

        public double[,] val;
        public double minVal = double.PositiveInfinity;
        public double maxVal = double.NegativeInfinity;

        double minMazeX; // in maze coord
        double maxMazeX;
        double minMazeZ;
        double maxMazeZ;

        double mzXCenter; // in maze coord
        public double offsetMazeX = 0; // default offset
        double mzZCenter;
        public double offsetMazeZ = 0;

        public double res = 2; // default resolution in maze coord / pixel

        public double hmXCenter; // in heatmap coord
        public double hmZCenter;

        public int xOffsetRemainder_Bot; // in heatmap coord
        int xOffsetRemainder_Top;
        public int xPixels = 0;

        public int zOffsetRemainder_Bot;
        int zOffsetRemainder_Top;
        public int zPixels = 0;

        public HeatmapItem()
        {
            type = Type.Presence; // default type
        }

        public HeatmapItem(Type type)
        {
            this.type = type;
        }

        public void UpdateHeatmapPixels(double minX, double maxX, double minZ, double maxZ)
        // called when the heatmap is initially maded
        {
            this.minMazeX = minX;
            this.maxMazeX = maxX;
            this.minMazeZ = minZ;
            this.maxMazeZ = maxZ;

            mzXCenter = (maxX + minX) / 2;
            mzZCenter = (maxZ + minZ) / 2;

            UpdateHeatmapPixels();
        }

        public void UpdateHeatmapPixels()
        // call when the resolution or offset is changed
        {
            hmXCenter = mzXCenter + ((offsetMazeX - mzXCenter) % res);
            hmZCenter = mzZCenter + ((offsetMazeZ - mzZCenter) % res);

            xOffsetRemainder_Bot = (int)Math.Ceiling((hmXCenter - minMazeX) / res); // # of pixels left of offset
            xOffsetRemainder_Top = (int)Math.Ceiling((maxMazeX - hmXCenter) / res); // # of pixels right of offset
            xPixels = xOffsetRemainder_Bot + xOffsetRemainder_Top+1;

            zOffsetRemainder_Bot = (int)Math.Ceiling((hmZCenter - minMazeZ) / res);
            zOffsetRemainder_Top = (int)Math.Ceiling((maxMazeZ - hmZCenter) / res);
            zPixels = zOffsetRemainder_Bot + zOffsetRemainder_Top+1;
        }

        int prevHeatmapX; // for entrance heatmaps
        int prevHeatmapZ;
        public void MakePathHeatmap(List<MPoint> PathPoints, List<long> PathTimes, List<MPoint> teleports)
        // each path has a heatmap, which will be added together such that each maze has a heatmap
        {
            val = new double[xPixels, zPixels];

            for (int i = 0; i < PathPoints.Count; i++)
            {
                // index of offset +- # of pixels away
                Point heatmapCoord = MazeToHeatmapCoord(PathPoints[i].X, PathPoints[i].Z);

                if (!teleports.Contains(PathPoints[i])&&heatmapCoord.X>=0&&heatmapCoord.Y>=0&&heatmapCoord.X<xPixels&&heatmapCoord.Y<zPixels)
                {
                    switch (type)
                    {
                        case Type.Presence:
                            val[heatmapCoord.X, heatmapCoord.Y] = 1;
                            break;

                        case Type.Entrance:
                            if (prevHeatmapX != heatmapCoord.X || prevHeatmapZ != heatmapCoord.Y)
                            {
                                val[heatmapCoord.X, heatmapCoord.Y] += 1;
                            }
                            break;

                        case Type.Time:
                            if (i > 0)
                            {
                                val[heatmapCoord.X, heatmapCoord.Y] += PathTimes[i] - PathTimes[i - 1];
                            }
                            break;
                    }
                }

                prevHeatmapX = heatmapCoord.X;
                prevHeatmapZ = heatmapCoord.Y;
            }
        }

        public Point MazeToHeatmapCoord(double mazeXCoord, double mazeZCoord)
        {
            Point heatmapCoord = new Point();

            heatmapCoord.X = xOffsetRemainder_Bot + (int)Math.Floor((mazeXCoord - hmXCenter) / res);
            heatmapCoord.Y = zOffsetRemainder_Bot + (int)Math.Floor((mazeZCoord - hmZCenter) / res);

            return heatmapCoord;
        }

        public PointF HeatmapToMazeCoord(double heatmapXCoord, double heatmapZCoord)
        {
            PointF mzCoord = new PointF();

            mzCoord.X = (float)((heatmapXCoord- xOffsetRemainder_Bot) * res + hmXCenter);
            mzCoord.Y = (float)((heatmapZCoord-zOffsetRemainder_Bot)*res+ hmZCenter);

            return mzCoord;
        }

        public double GetMin()
        {
            minVal = double.PositiveInfinity;
            for (int i = 0; i < xPixels; i++)
            {
                for (int j = 0; j < zPixels; j++)
                {
                    minVal = Math.Min(val[i, j], minVal);
                }
            }

            return minVal;
        }

        public double GetMax()
        {
            maxVal = double.NegativeInfinity;
            for (int i = 0; i < xPixels; i++)
            {
                for (int j = 0; j < zPixels; j++)
                {
                    maxVal = Math.Max(val[i, j], maxVal);
                }
            }

            return maxVal;
        }

        public static HeatmapItem operator +(HeatmapItem hm1) => hm1;

        public static HeatmapItem operator +(HeatmapItem hm1, HeatmapItem hm2)
        {
            if (hm1.xPixels == 0 && hm2.xPixels != 0)
            {
                return hm2;
            }
            if (hm1.xPixels != 0 && hm2.xPixels == 0)
            {
                return hm1;
            }

            if (hm1.type != hm2.type)
            {
                throw new Exception("The heatmap type should be the same.");
            }
            if (hm1.res != hm2.res)
            {
                throw new Exception("The heatmap resolution should be the same.");
            }
            double res = hm1.res;
            if (hm1.offsetMazeX % res != hm2.offsetMazeX % res || hm1.offsetMazeZ % res != hm2.offsetMazeZ % res)
            {
                throw new Exception("The heatmap offset should be the same.");
            }

            Type hmSumType = hm1.type;
            HeatmapItem hmSum = new HeatmapItem(hmSumType)
            {
                offsetMazeX = hm1.offsetMazeX,
                offsetMazeZ = hm1.offsetMazeZ,
                res = res
            };

            // TODO: add heatmap of same resolution, but different sizes
            double hmSumMinX = Math.Min(hm1.minMazeX, hm2.minMazeX);
            double hmSumMaxX = Math.Max(hm1.maxMazeX, hm2.maxMazeX);
            double hmSumMinZ = Math.Min(hm1.minMazeZ, hm2.minMazeZ);
            double hmSumMaxZ = Math.Max(hm1.maxMazeZ, hm2.maxMazeZ);
            hmSum.UpdateHeatmapPixels(hmSumMinX, hmSumMaxX, hmSumMinZ, hmSumMaxZ);

            hmSum.val = new double[hmSum.xPixels, hmSum.zPixels];

            int hm1TopLeftX = 0;
            int hm1TopLeftZ = 0;
            int hm1BotRightX = hm1TopLeftX + hm1.xPixels;
            int hm1BotRightZ = hm1TopLeftZ + hm1.zPixels;

            int hm2TopLeftX = 0;
            int hm2TopLeftZ = 0;
            int hm2BotRightX = hm2TopLeftX + hm2.xPixels;
            int hm2BotRightZ = hm2TopLeftZ + hm2.zPixels;

            for (int i = 0; i < hmSum.xPixels; i++)
            {
                for (int j = 0; j < hmSum.zPixels; j++)
                {
                    hmSum.val[i, j] = 0;

                    if (i >= hm1TopLeftX && i < hm1BotRightX && j >= hm1TopLeftZ && j < hm1BotRightZ)
                    {
                        hmSum.val[i, j] += hm1.val[i, j];
                    }

                    if (i >= hm2TopLeftX && i < hm2BotRightX && j >= hm2TopLeftZ && j < hm2BotRightZ)
                    {
                        hmSum.val[i, j] += hm2.val[i, j];
                    }
                }
            }

            hmSum.GetMin();
            hmSum.GetMax();
            
            return hmSum;
        }

        public static HeatmapItem operator +(HeatmapItem hm1, double val)
        {
            for (int i = 0; i < hm1.xPixels; i++)
            {
                for (int j = 0; j < hm1.zPixels; j++)
                {
                    hm1.val[i, j] += val;
                }
            }

            hm1.minVal += val;
            hm1.maxVal += val;

            return hm1;
        }

        public static HeatmapItem operator -(HeatmapItem hm1, HeatmapItem hm2)
        {
            return hm1 + (hm2 * -1);
        }

        public static HeatmapItem operator -(HeatmapItem hm1, double val)
        {
            return hm1 + (val * -1);
        }

        public static HeatmapItem operator *(HeatmapItem hm1, double val)
        {
            for (int i = 0; i < hm1.xPixels; i++)
            {
                for (int j = 0; j < hm1.zPixels; j++)
                {
                    hm1.val[i, j] *= val;
                }
            }

            hm1.minVal *= val;
            hm1.maxVal *= val;
            
            return hm1;
        }

        public static HeatmapItem operator /(HeatmapItem hm1, double val)
        {
            return hm1 * (1 / val);
        }
    }
}
