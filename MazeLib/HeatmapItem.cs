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

        double minX; // in maze coord
        double maxX;
        double minZ;
        double maxZ;

        double mazeXCenter; // in maze coord
        public double offsetX = 0; // default offset
        double mazeZCenter;
        public double offsetZ = 0;

        public double res = 5; // default resolution

        public double heatmapXCenter; // in heatmap coord
        public double heatmapZCenter;

        public int xBotRadius; // in heatmap coord
        int xTopRadius;
        public int xPixels = 0;

        public int zBotRadius;
        int zTopRadius;
        public int zPixels = 0;

        public HeatmapItem()
        {
            type = Type.Presence;
        }

        public HeatmapItem(Type type)
        {
            this.type = type;
        }

        public void UpdateHeatmapPixels(double minX, double maxX, double minZ, double maxZ)
        {
            this.minX = minX;
            this.maxX = maxX;
            this.minZ = minZ;
            this.maxZ = maxZ;

            mazeXCenter = (maxX + minX) / 2;
            mazeZCenter = (maxZ + minZ) / 2;

            UpdateHeatmapPixels();
        }

        public void UpdateHeatmapPixels()
        // Updates Heatmap Pixels Based When Resolution or Offset is Changed
        {
            heatmapXCenter = mazeXCenter + ((offsetX - mazeXCenter) % res);
            heatmapZCenter = mazeZCenter + ((offsetZ - mazeZCenter) % res);

            xBotRadius = (int)Math.Ceiling((heatmapXCenter - minX) / res); // # of pixels left of offset
            xTopRadius = (int)Math.Ceiling((maxX - heatmapXCenter) / res); // # of pixels right of offset
            xPixels = xBotRadius + xTopRadius;

            zBotRadius = (int)Math.Ceiling((heatmapZCenter - minZ) / res);
            zTopRadius = (int)Math.Ceiling((maxZ - heatmapZCenter) / res);
            zPixels = zBotRadius + zTopRadius;
        }

        int prevHeatmapX;
        int prevHeatmapZ;
        public void MakePathHeatmap(List<MPoint> PathPoints, List<long> PathTimes, List<MPoint> teleports)
        {
            val = new double[xPixels, zPixels];

            for (int i = 0; i < PathPoints.Count; i++)
            {
                // index of offset +- # of pixels away
                Point heatmapCoord = MazeToHeatmapCoord(PathPoints[i].X, PathPoints[i].Z);

                if (!teleports.Contains(PathPoints[i]))
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
                            try
                            {
                                val[heatmapCoord.X, heatmapCoord.Y] += PathTimes[i] - PathTimes[i - 1];
                            }
                            catch // PathTimes[-1] exception
                            {
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

            heatmapCoord.X = xBotRadius + (int)Math.Floor((mazeXCoord - heatmapXCenter) / res);
            heatmapCoord.Y = zBotRadius + (int)Math.Floor((mazeZCoord - heatmapZCenter) / res);

            return heatmapCoord;
        }

        public double GetMin()
        {
            double minVal = 999999;
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
            double maxVal = -999999;
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
                throw new System.Exception("The heatmap type should be the same.");
            }
            if (hm1.res != hm2.res)
            {
                throw new System.Exception("The heatmap resolution should be the same.");
            }
            double res = hm1.res;
            if (hm1.offsetX % res != hm2.offsetX % res || hm1.offsetZ % res != hm2.offsetZ % res)
            {
                throw new System.Exception("The heatmap offset should be the same.");
            }

            Type hmSumType = hm1.type;
            HeatmapItem hmSum = new HeatmapItem(hmSumType);

            hmSum.offsetX = hm1.offsetX;
            hmSum.offsetZ = hm1.offsetZ;
            hmSum.res = res;

            double hmSumMinX = Math.Min(hm1.minX, hm2.minX);
            double hmSumMaxX = Math.Max(hm1.maxX, hm2.maxX);
            double hmSumMinZ = Math.Min(hm1.minZ, hm2.minZ);
            double hmSumMaxZ = Math.Max(hm1.maxZ, hm2.maxZ);
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

            return hmSum;
        }

        public static HeatmapItem operator -(HeatmapItem hm1, HeatmapItem hm2)
        {
            return hm1 + (hm2 * -1);
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

            return hm1;
        }

        public static HeatmapItem operator /(HeatmapItem hm1, double val)
        {
            for (int i = 0; i < hm1.xPixels; i++)
            {
                for (int j = 0; j < hm1.zPixels; j++)
                {
                    hm1.val[i, j] /= val;
                }
            }

            return hm1;
        }
    }
}
