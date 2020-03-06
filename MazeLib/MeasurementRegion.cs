using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using MazeMaker;
using System.ComponentModel;

namespace MazeLib
{
    public class MeasurementRegion
    {

        public MeasurementRegion(string inp,string filename, Guid g)
        {
            name = inp;
            regGuid = g;

            this.filename = filename;
        }

        public MeasurementRegion(string inp, string filename="new")
        {
            name = inp;
            regGuid = System.Guid.NewGuid();

            this.filename = filename;
        }


        public Guid regGuid;
        public string filename;

        private float scale = 17;
        [Category("Misc")]
        [Description("")]
        [ReadOnly(false)]
        [Browsable(false)]
        public double Scale
        {
            get { return scale; }
            set
            {
                
                scale = (float) value;
                ConvertFromMazeToScreen();
                //ConvertFromMazeCoordinates();
            }
        }

        private double ymin = -1;
        [Category("2. Properties")]
        [Description("Vertical Axis minimum, do not include below this range")]
        public double Ymin
        {
            get { return ymin; }
            set { ymin = value; }
        }

        private double ymax = 1;
        [Category("2. Properties")]
        [Description("Vertical Axis maximum, do not include above this range")]
        public double Ymax
        {
            get { return ymax; }
            set { ymax = value; }
        }

        private string name= "";
        [Category("1. Info")]
        [Description("Identifier of this region")]
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        private string regionGroup = "Group 1";
        [Category("1. Info")]
        [Description("Grouping of this region during export and analysis")]
        public string RegionGroup
        {
            get { return regionGroup; }
            set { regionGroup = value; }
        }

        private bool editing = true;
        [ReadOnly(true)]
        [Browsable(false)]
        public bool Editing
        {
            get { return editing; }
            set { editing = value; }
        }

        public enum RegionSection
        {
            Inside=0,
            Outside=1,
            //DistanceToCenter=2
        }

        private RegionSection roiSection = RegionSection.Inside;
        [Category("2. Properties")]
        [Description("Identifies region of interest; whether it is inside the polygon borders or outside (inverted)")]
        public RegionSection ROI
        {
            get { return roiSection; }
            set { roiSection = value; }
        }

        // add screen coordinates, needs to be converted to maze coordinates
        public int AddPoint(float x, float y)
        {
            verticesScr.Add(new PointF(x, y));
            verticesMz.Add(new PointF(x/scale, y/ scale));
            return 1;
        }

        //list of all points...
        private List<PointF> verticesScr = new List<PointF>();
        private List<PointF> verticesMz = new List<PointF>();
        [Category("2. Properties")]
        [Description("X and Z coordinates of selected polygon vertices in maze coordinates")]
        public List<PointF> Vertices
        {
            get { return verticesMz; }
            set { 
                verticesMz = value;
                ConvertFromMazeToScreen();
            }
        }

        
        public void ConvertFromMazeToScreen()
        {
            verticesScr.Clear();            
            for (int i = 0; i < verticesMz.Count; i++)
            {
                verticesScr.Add(new PointF(verticesMz[i].X * scale, verticesMz[i].Y * scale));
            }           
        }

        public bool IsPointIn(MPoint inp)
        {
            if (roiSection == RegionSection.Inside)
                return IsPointIn_(inp);
            return !IsPointIn_(inp);
        }
        private bool IsPointIn_(MPoint inp)
        {
            int i, j;

            // First check if current point is within Y axis limits...
            if (inp.Y < Ymin || inp.Y > Ymax)
                return false;

            // Finally, apply point in polygon algorithm in 2d, xz plane...
            bool ret = false;
            for (i = 0, j = verticesMz.Count - 1; i < verticesMz.Count; j = i++)
            {
                if (((verticesMz[i].Y > inp.Z) != (verticesMz[j].Y > inp.Z)) &&
                 (inp.X < (verticesMz[j].X - verticesMz[i].X) * (inp.Z - verticesMz[i].Y) / (verticesMz[j].Y - verticesMz[i].Y) + verticesMz[i].X))
                    ret = !ret;
            }
            return ret;
        }

        //private bool isPoint=false; //if true this is a point, false means region
        public MPoint centeroid = new MPoint(0,0,0);

        public void InitializeDistanceCalculations()
        {
            if (verticesScr.Count < 1)
                return;
            if (name.StartsWith("Point"))
            {
                //isPoint = true;
                centeroid.X = verticesMz[0].X;
                centeroid.Y = 0;
                centeroid.Z = verticesMz[0].Y;
            }
            else
            {
                //isPoint = false;
                centeroid.X = 0;
                for (int i = 0; i < verticesMz.Count; i++)
                    centeroid.X += verticesMz[i].X;
                centeroid.X /= verticesMz.Count;

                centeroid.Y = 0;

                centeroid.Z = 0;
                for (int i = 0; i < verticesMz.Count; i++)
                    centeroid.Z += verticesMz[i].Y;
                centeroid.Z /= verticesMz.Count;
            }
        }

        public double Distance(MPoint inp)
        {
            return Math.Sqrt(Math.Pow((centeroid.X - inp.X), 2) + Math.Pow((centeroid.Z - inp.Z), 2));
        }

        public bool IsPointIn(float scrX, float scrY)
        {
            if (roiSection == RegionSection.Inside)
                return IsPointIn_(scrX,scrY);
            return !IsPointIn_(scrX,scrY);
        }
        // Check if input screen coordinates are within the region
        private bool IsPointIn_(float scrX, float scrY)
        {
            int i, j;
            bool ret = false;
            for (i = 0, j = verticesScr.Count - 1; i < verticesScr.Count; j = i++)
            {
                if (((verticesScr[i].Y > scrY) != (verticesScr[j].Y > scrY)) &&
                 (scrX < (verticesScr[j].X - verticesScr[i].X) * (scrY - verticesScr[i].Y) / (verticesScr[j].Y - verticesScr[i].Y) + verticesScr[i].X))
                    ret = !ret;
            }
            return ret;
        }

        public void Paint(ref Graphics gr,bool fillRegion=false)
        {
            try
            {
                if (verticesScr.Count == 0)
                    return;
                
                Pen pDash,pBack,p2,pPoint;
                pDash = new Pen(Color.Red, 3);
                pBack = new Pen(Color.LightGray, 5);
                pDash.DashPattern= new float[] { 3.0F, 1.0F, 3.0F, 1.0F };
                pPoint = new Pen(Color.Red, 6);
                p2 = new Pen(Color.Yellow, 6);

                if (verticesScr.Count == 1) //Point
                {
                    //point
                    if(!editing)
                    {
                        gr.DrawEllipse(pPoint, verticesScr[0].X - 4, verticesScr[0].Y - 4, 8, 8);
                        
                    }
                    else
                    {
                        gr.DrawEllipse(p2, verticesScr[0].X - 6, verticesScr[0].Y - 6, 12, 12);
                    }
                }
                else if(verticesScr.Count >= 1) //region
                {
                    if (editing)
                    {
                        gr.DrawEllipse(pBack, verticesScr[0].X - 7, verticesScr[0].Y - 7, 12, 12);
                        gr.DrawEllipse(pDash, verticesScr[0].X - 7, verticesScr[0].Y - 7, 12, 12);
                        gr.DrawEllipse(p2, verticesScr[verticesScr.Count - 1].X - 7, verticesScr[verticesScr.Count - 1].Y - 7, 14, 14);                    
                    }
                    if(verticesScr.Count>2)
                    {
                        //region
                        gr.DrawClosedCurve(pBack, verticesScr.ToArray(), 0, System.Drawing.Drawing2D.FillMode.Winding);
                        gr.DrawClosedCurve(pDash, verticesScr.ToArray(), 0, System.Drawing.Drawing2D.FillMode.Winding);
                    }
                }

                if (fillRegion && verticesScr.Count > 2)
                {
                    SolidBrush b = new SolidBrush(Color.Red);

                    gr.FillPolygon(b, verticesScr.ToArray());
                    b.Dispose();
                }
                pDash.Dispose();
                pBack.Dispose();
                p2.Dispose();


            }
            catch //(System.Exception ex)
            {
                //int x = 1;
            }
            
           
        }

    }
}
