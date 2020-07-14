using System;
using System.Collections.Generic;
using System.Text;
using MazeMaker;
using System.Drawing;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using System.Runtime.Serialization;

namespace MazeLib
{
    public class MazePathItem
    {
        public HeatmapItem presHeatmap = new HeatmapItem(HeatmapItem.Type.Presence);
        public HeatmapItem entrHeatmap = new HeatmapItem(HeatmapItem.Type.Entrance);
        public HeatmapItem timeHeatmap = new HeatmapItem(HeatmapItem.Type.Time);


        public bool selected = false;

        List<MPoint> cPoints = new List<MPoint>();
        [Category("Data")]
        [Description("Path Coordinates")]
        [ReadOnly(true)]
        public List<MPoint> PathPoints
        {
            get { return cPoints; }
        }

        List<MPoint> cViewPoints = new List<MPoint>();
        [Category("Data")]
        [Description("Path Coordinates")]
        [ReadOnly(true)]
        public List<MPoint> PathViewPoints
        {
            get { return cViewPoints; }
        }

        List<long> cTimes = new List<long>();
        [Category("Data")]
        [Description("Path Times")]
        [ReadOnly(true)]
        public List<long> PathTimes
        {
            get { return cTimes; }
        }

        List<EventItem> cEvents = new List<EventItem>();
        [Category("Data")]
        [Description("Events from log")]
        [ReadOnly(true)]
        public List<EventItem> Events
        {
            get { return cEvents; }
            //set {
            //    cEvents = value;
            //}
        }


        private double scale = 17;
        [Category("Options")]
        [Description("Maze scale parameter that should match the used maze file")]
        [ReadOnly(false)]
        [Browsable(false)]
        public double Scale
        {
            get { return scale; }
            set { scale = value; }
        }

        private string subjectID = "";
        [Category("Experimental Info")]
        [Description("Maze scale parameter that should match the used maze file")]
        [DisplayName("3.Subject #")]
        [ReadOnly(false)]
        public string ExpSubjectID
        {
            get { return subjectID; }
            set { subjectID = value; }
        }

        public int groupID = 0;
        private string groupName = "";
        [Category("Experimental Info")]
        [Description("Maze scale parameter that should match the used maze file")]
        [ReadOnly(false)]
        [DisplayName("1.Group")]
        public string ExpGroup
        {
            get { return groupName; }
            set { groupName = value;
                //set groupID
            }
        }

        public int conditionID = 0;
        private string conditionName = "";
        [Category("Experimental Info")]
        [Description("Maze scale parameter that should match the used maze file")]
        [ReadOnly(false)]
        [DisplayName("2.Condition")]
        public string ExpCondition
        {
            get { return conditionName; }
            set
            {
                conditionName = value;
                //set conditionID
            }
        }

        public int sessionNum = 0;
        [Category("Experimental Info")]
        [Description("Maze scale parameter that should match the used maze file")]
        [ReadOnly(false)]
        [DisplayName("4.Session #")]
        public int ExpSession
        {
            get { return sessionNum; }
            set
            {
                sessionNum = value;
                //set groupID
            }
        }

        public int trialNum = 0;
        [Category("Experimental Info")]
        [Description("Maze scale parameter that should match the used maze file")]
        [ReadOnly(false)]
        [DisplayName("5.Trial #")]
        public int ExpTrial
        {
            get { return trialNum; }
            set
            {
                trialNum = value;
                //set groupID
            }
        }

        private Color mazeColorRegular = Color.Transparent;
        
        [Browsable(true)]
        [Category("Color")]
        [Description("Path Display Color")]
        public Color MazeColorRegular
        {
            get { return mazeColorRegular; }
            set { mazeColorRegular = value; }
        }

        private Color autoColorRegular = Color.Red;

        public Color GetAutoColor()
        {
            if (mazeColorRegular == Color.Transparent)

                return autoColorRegular;
            else
                return mazeColorRegular;
        }

        public bool bMazeMatches = false;

        public MazePathItem()
        {
            selected = false;
        }

        public MazePathItem(string mazeName, string walkerName, string dateTime, string melName)
        {
            maze = mazeName;
            walker = walkerName;
            date = dateTime;
            mel = melName;
            selected = false;


            presHeatmap = new HeatmapItem(HeatmapItem.Type.Presence);


        }

        public MazePathItem(string mazeName, string walkerName, string dateTime)
        {
            maze = mazeName;
            walker = walkerName;
            date = dateTime;
            selected = false;
        }


        private string walker = "";
        [Category("Summary")]
        [Description("Participant Name")]
        [ReadOnly(true)]
        public string Walker
        {
            get { return walker; }
            set { walker = value;}
        }

        private string maze = "";
        [Category("File")]
        [Description("Maze File Name")]
        [ReadOnly(true)]
        public string Maze
        {
            get { return maze; }
            set { maze = value; }
        }

        private string date = "";
        [Category("Summary")]
        [Description("Date/Time")]
        [ReadOnly(true)]
        public string Date
        {
            get { return date; }
            set { date = value; }
        }

        private string mel = "";
        [Category("File")]
        [Description("Mel File Name (if applicable)")]
        [ReadOnly(true)]
        public string Mel
        {
            get { return mel; }
            set { mel = value; }
        }

        private float pathLength = 0;
        [Category("Summary")]
        [Description("The length of the path in maze units using x, y and z")]
        [DisplayName("Path Length (3D)")]
        [ReadOnly(true)]
        public float PathLength
        {
            get { return pathLength; }
        }

        private float pathLengthXZ = 0;
        [Category("Summary")]
        [Description("The length of the path in maze units using only x and z as in birds eye view")]
        [DisplayName("Path Length (2D)")]
        [ReadOnly(true)]
        public float PathLengthXZ
        {
            get { return pathLengthXZ; }
        }

        private float pathTime = 0;
        [Category("Summary")]
        [Description("Total path time in seconds")]
        [DisplayName("Path Time")]
        [ReadOnly(true)]
        public float PathTime
        {
            get { return pathTime; }
        }

        private bool bViewVector = false;
        [Category("Options")]
        [Description("Toggle display view vector")]
        [DisplayName("Show View Vector")]
        [ReadOnly(false)]
        public bool ViewVector
        {
            get { return bViewVector; }
            set { bViewVector = value;}
        }

        private string returnValue = "";
        [Category("Summary")]
        [Description("Return Value")]
        [ReadOnly(true)]
        public string ReturnValue
        {
            get { return returnValue; }
            set { returnValue = value; }
        }

        public string logFileName = "";

        public int logIndex;
        public int melIndex = 0;
        [Category("File")]
        [Description("Mel File Log Index (Based on record file)")]
        [ReadOnly(true)]
        [DisplayName("Mel Index")]
        public int MelIndex
        { 
            get { return melIndex; }
            set { melIndex = value; }
        }


        double velocity;
        readonly List<MPoint> teleports = new List<MPoint>();

        double minX;
        double maxX;
        double minZ;
        double maxZ;


        private char[] param = new char[] { '\t' };
        public bool AddLine(string line)
        {
            try
            {
                string[] p = line.Split(param, StringSplitOptions.RemoveEmptyEntries);

                if (p.Length == 8)
                {
                    for (int i = 1; i < p.Length; i++)
                        p[i - 1] = p[i];
                }
                
                long time = long.Parse(p[0]);
                if (time == -1)
                {
                    //end of maze 
                    returnValue = p[1];
                }
                else
                {
                    float temp = 0;
                    if (float.TryParse(p[1], out temp))
                    {
                        MPoint mp = new MPoint();
                        mp.X = temp;
                        mp.Z = float.Parse(p[2]);
                        mp.Y = float.Parse(p[3]);


                        try
                        {
                            velocity = cPoints[cPoints.Count - 1].GetDistance(mp) / (time - cTimes[cTimes.Count - 1]);
                        }
                        catch // cPoints[-1] exception
                        {
                        }

                        if (velocity > 1)
                        {
                            teleports.Add(mp);
                        }

                        minX = Math.Min(mp.X, minX);
                        maxX = Math.Max(mp.X, maxX);
                        minZ = Math.Min(mp.Z, minZ);
                        maxZ = Math.Max(mp.Z, maxZ);

                        presHeatmap.UpdateHeatmapPixels(minX, maxX, minZ, maxZ);
                        entrHeatmap.UpdateHeatmapPixels(minX, maxX, minZ, maxZ);
                        timeHeatmap.UpdateHeatmapPixels(minX, maxX, minZ, maxZ);


                        cPoints.Add(mp);
                        cTimes.Add(time);

                        MPoint mp2 = new MPoint();
                        mp2.X = float.Parse(p[4]);
                        mp2.Z = float.Parse(p[5]);
                        mp2.Y = float.Parse(p[6]);

                        cViewPoints.Add(mp2);

                        //UpdateSummary();
                    }
                    else if(p[1].Contains("Event"))
                    {
                        cEvents.Add(new EventItem(time,int.Parse(p[2]), int.Parse(p[3]), int.Parse(p[4])));
                    }
                }

            }
            catch //(System.Exception ex)
            {
                return false;
            }
            return true;
        }

        public void UpdateSummary()
        {
            if( cTimes.Count >2)
            {
                pathTime = ((float)(cTimes[cTimes.Count - 1] - cTimes[0])) / 1000;
            }
            else if(cTimes.Count==1)
            {
                pathTime = 0;
            }
            else
            {
                pathTime = 0;
            }

            pathLength = 0;
            double dist = 0;
            double distxz = 0;
            for(int i=1; i< cPoints.Count;i++)
            {
                dist += cPoints[i].GetDistance(cPoints[i - 1]);
                distxz += Math.Sqrt( Math.Pow(cPoints[i].Z - cPoints[i - 1].Z, 2) + Math.Pow(cPoints[i].X - cPoints[i - 1].X, 2) );
            }
            pathLength = (float)(dist);
            pathLengthXZ = (float)distxz;
        }

        public void Paint(ref Graphics gr,Color drawColor, bool bShowingAll)
        {
            Pen p,p2;
            //Brush br;
            autoColorRegular = drawColor;
            //br = new SolidBrush(Color.BlueViolet);
            p = new Pen(drawColor, 4);
            p2 = new Pen(Color.LightYellow, 3);

            if (cViewPoints.Count != cPoints.Count)
                bViewVector = false;
            //gr.DrawLine(p, 0, 0, 50, 50);
            for (int i = 1; i < cPoints.Count;i++)
            {
                gr.DrawLine(p, (float)(cPoints[i-1].X * scale), (float)(cPoints[i-1].Z * scale), (float)(cPoints[i].X * scale),(float) (cPoints[i].Z * scale));
                if(bViewVector)// && bShowingAll==false)
                {
                    gr.DrawLine(p2, (float)(cViewPoints[i - 1].X * scale), (float)(cViewPoints[i - 1].Z * scale), (float)(cViewPoints[i].X * scale), (float)(cViewPoints[i].Z * scale));
                }
            }

            p.Dispose();
            p2.Dispose();
            //br.Dispose();

        }


        public void SetHeatmapOffset(double offsetX, double offsetZ)
        {
            presHeatmap.offsetX = offsetX;
            presHeatmap.offsetZ = offsetZ;

            entrHeatmap.offsetX = offsetX;
            entrHeatmap.offsetZ = offsetZ;
            
            timeHeatmap.offsetX = offsetX;
            timeHeatmap.offsetZ = offsetZ;
           
            UpdateHeatmapPixels();
        }

        public void SetHeatmapRes(double res)
        {
            presHeatmap.res = res;
            entrHeatmap.res = res;
            timeHeatmap.res = res;

            UpdateHeatmapPixels();
        }

        public void UpdateHeatmapPixels()
        // Updates Heatmap Pixels Based When Resolution or Offset is Changed
        {
            presHeatmap.UpdateHeatmapPixels();
            entrHeatmap.UpdateHeatmapPixels();
            timeHeatmap.UpdateHeatmapPixels();
        }

        public void MakePathHeatmap()
        {
            presHeatmap.MakePathHeatmap(PathPoints, PathTimes, teleports);
            entrHeatmap.MakePathHeatmap(PathPoints, PathTimes, teleports);
            timeHeatmap.MakePathHeatmap(PathPoints, PathTimes, teleports);
        }
    }
}
