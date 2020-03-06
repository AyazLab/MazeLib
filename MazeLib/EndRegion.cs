#region Using directives

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Data;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Drawing2D;
using System.Xml;

#endregion

namespace MazeMaker
{
    [TypeConverterAttribute(typeof(StartPosMoveConverter)), DescriptionAttribute("StartPosition Support")]
    [Serializable]
    public class EndRegion : MazeItem
    {
        public EndRegion()
        {
            //scale = 17;
            //this.SetID(0, true);
            this.Label = "";
        }

        public EndRegion(double dScale, string label="", int id = 0, bool generateNewID = false) //0 for new, -1 for temporary, other for set specific Id
        {
            this.Scale = dScale;
            if (generateNewID)
                id = 0;
            this.SetID(id);
            this.Label = label;
            itemType = MazeItemType.End;
        }


        public EndRegion(XmlNode endRegNode)
        {
            this.SetID(Tools.getIntFromAttribute(endRegNode, "id", -1));
            this.Label = Tools.getStringFromAttribute(endRegNode, "label", "");

            MPoint temp1 = Tools.getXYZfromNode(endRegNode, -2,"MzCoord");
            MPoint temp2 = Tools.getXYZfromNode(endRegNode, -3, "MzCoord");
            this.MinX = (float)temp1.X;
            this.MaxX = (float)temp2.X;
            this.MinZ = (float)temp1.Z;
            this.MaxZ = (float)temp2.Z;
            this.Height = (float)temp2.Y - (float)temp1.Y;
            this.Offset = (float)(temp2.Y + temp1.Y) / 2;

            XmlNode actionNode = endRegNode.SelectSingleNode("Action");
            this.ReturnValue = Tools.getIntFromAttribute(actionNode, "returnValue");
            this.SuccessMessage = Tools.getStringFromAttribute(actionNode, "successMessage");
            this.pointThreshold = Tools.getIntFromAttribute(actionNode, "pointThreshold");
            this.moveToPosID = Tools.getIntFromAttribute(actionNode, "moveToPos", -999);
            itemType = MazeItemType.End;
        }
        //private bool selected = false;
        private double scale = 17;
        [Category("Options")]
        [Browsable(false)]
        [Description("Coordinate transformation coefficient")]
        public virtual double Scale
        {
            get { return scale; }
            set
            {
                //CalculateModifiedScaleMazeCoordinates(value);
                scale = value;
                ConvertFromMazeCoordinates();
                OnPropertyChanged("Scale");
            }
        }

        public virtual void CalculateModifiedScaleMazeCoordinates(double newScale)
        {
            if (newScale == 0)
                throw new Exception("Scale can not be zero!");
            if (scale != newScale)
            {
                float coef = (float) (newScale / scale);
                minX *= coef;
                maxX *= coef;
                minZ *= coef;
                maxZ *= coef;                
            }
        }

        public virtual void SilentSetScale(double newScale)
        {
            scale = newScale;
            ConvertFromMazeCoordinates();
            OnPropertyChanged("Scale");
        }

        private Color mazeColorRegular = Color.Goldenrod;
        [Browsable(false)]
        [Category("Colors")]
        [Description("The Current color in the editor")]
        public virtual Color MazeColorRegular
        {
            get { return mazeColorRegular; }
            set { mazeColorRegular = value;  }
        }

        private Color mazeColorSelected = Color.Coral;
        [Browsable(false)]
        [Category("Colors")]
        [Description("The Selection Mode color")]
        public virtual Color MazeColorSelected
        {
            get { return mazeColorSelected; }
            set { mazeColorSelected = value;  }
        }
        private RectangleF scrRect = new RectangleF(0, 0, 0, 0);
        [Category("Location")]
        [Browsable(false)]
        [Description("Location on screen coordinates")]
        public virtual RectangleF Rect
        {
            get { return scrRect; }
            set { scrRect = value; ConvertFromScreenCoordinates(); }
        }

        private float height = 2;
        [Category("3.Y Parameters")]        
        [Description("Height of the end region on Y axis")]
        public virtual float Height
        {
            get { return height; }
            set { height = value; OnPropertyChanged("Height"); }
        }

        private float offset = 0;
        [Category("3.Y Parameters")]
        [Description("Center of the end region on Y axis")]
        public virtual float Offset
        {
            get { return offset; }
            set { offset = value; OnPropertyChanged("Offset"); }
        }

        private int returnValue = 0;
        [Category("4.Action")]
        [DisplayName("Return value")]
        [Description("Return value to indicate this end region after maze ends")]
        public virtual int ReturnValue
        {
            get { return returnValue; }
            set { returnValue = value; OnPropertyChanged("Return"); }
        }

        private float minX = 0;
        [Category("2.Maze Coordinates")]
        [Description("Minimum X value of the end region")]
        public virtual float MinX
        {
            get { return minX; }
            set { minX = value; ConvertFromMazeCoordinates(); OnPropertyChanged("MinX"); }
        }
        private float maxX = 0;
        [Category("2.Maze Coordinates")]
        [Description("Maximum X value of the end region")]
        public virtual float MaxX
        {
            get { return maxX; }
            set { maxX = value; ConvertFromMazeCoordinates(); OnPropertyChanged("MaxX"); }
        }
        private float minZ = 0;
        [Category("2.Maze Coordinates")]
        [Description("Minimum Z value of the end region")]
        public virtual float MinZ
        {
            get { return minZ; }
            set { minZ = value; ConvertFromMazeCoordinates(); OnPropertyChanged("MinZ"); }
        }
        private float maxZ = 0;
        [Category("2.Maze Coordinates")]
        [Description("Minimum Z value of the end region")]
        public virtual float MaxZ
        {
            get { return maxZ; }
            set { maxZ = value; ConvertFromMazeCoordinates(); OnPropertyChanged("MaxZ"); }
        }

        public int moveToPosID = -999;
        private StartPos moveToPos = null;
        [Category("4.Action")]
        [Description("What happens when EndRegion is reached")]
        [DisplayName("Action")]
        [TypeConverter(typeof(StartPosMoveConverter))]
        public virtual StartPos MoveToPos
        {
            get { return moveToPos; }
            set
            {
                moveToPos = value;
                if (moveToPos == null)
                    moveToPosID = -999;
                else
                    moveToPosID = moveToPos.GetID();
            }
        }

        private string successMessage = "";
        [Category("4.Action")]
        [Description("Message to be shown when EndRegion is reached")]
        [DisplayName("Message Text")]
        public virtual string SuccessMessage
        {
            get { return successMessage; }
            set { successMessage = value; OnPropertyChanged("EndRegion"); }
        }

        private int pointThreshold = 0;
        [Category("4.Action")]
        [Description("MazePoints required to activate")]
        [DisplayName("Point Threshold")]
        public virtual int PointThreshold
        {
            get { return pointThreshold; }
            set { pointThreshold = value; OnPropertyChanged("PointThreshold"); }
        }

        public virtual bool InRegion(int x1, int y1, int x2, int y2) //currently requires entire area to encompase selection
        {
            int iconTolerence = 0;
            x1 = x1 - iconTolerence; //top
            x2 = x2 + iconTolerence; //bottom
            y1 = y1 - iconTolerence; //left
            y2 = y2 + iconTolerence; //right

            float midX = scrRect.Left + (scrRect.Right - scrRect.Left) / 2;
            float midY = scrRect.Top + (scrRect.Bottom - scrRect.Top) / 2;

            if (x1==x2&&y1==y2) //single point select
            { 

                if (x1 > scrRect.Left && x1 < scrRect.Right && y1 > scrRect.Top && y1 < scrRect.Bottom) //is the point in the region
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                
                if (x1 < midX && x2 > midX && y1 < midY && y2 > midY) //must entirely enclose two points + center
                {
                    int regCount = 0;
                    if (x1 < scrRect.Left && y2 > scrRect.Bottom)
                        regCount++;
                    if (x1 < scrRect.Left && y1 < scrRect.Top)
                        regCount++;
                    if (x2 > scrRect.Right && y1 < scrRect.Top)
                        regCount++;
                    if (x2 > scrRect.Right && y2 > scrRect.Bottom)
                        regCount++;
                    return regCount>=2;
                }
                else
                {
                    return false;
                }
            }
        }

        public virtual bool IfSelected(int x, int y)
        {
            if (x > scrRect.Left && x < scrRect.Right && y > scrRect.Top && y < scrRect.Bottom)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override void Move(float mzXdir, float mzZdir)
        {
            minX += mzXdir;
            maxX += mzXdir;
            minZ += mzZdir;
            maxZ += mzZdir;

            ConvertFromMazeCoordinates();
        }

        public override void Rescale(double factor)
        {
            minX *= (float)factor;
            maxX *= (float)factor;
            minZ *= (float)factor;
            maxZ *= (float)factor;

            ConvertFromMazeCoordinates();
        }


        public override void RescaleXYZ(double scaleX, double scaleY, double scaleZ)
        {
            minX *= (float)scaleX;
            maxX *= (float)scaleX;
            height *=(float) scaleY;
            offset *= (float)scaleY;
            minZ *= (float)scaleZ;
            maxZ *= (float)scaleZ;

            ConvertFromMazeCoordinates();
        }


        public PointF CalcMidPoint()
        {
            return new PointF((float)((this.scrRect.Width / 2) + this.scrRect.Location.X), (float)((this.scrRect.Height / 2) + this.scrRect.Location.Y));
        }

        public override void Rotate(float degrees, float centerX = 0, float centerY = 0)
        {
            PointF midPoint;
            if (centerX != 0 && centerY != 0)
                midPoint = new PointF(centerX, centerY);
            else
                midPoint = CalcMidPoint();

            PointF corner1 = this.scrRect.Location;
            PointF corner2 = new PointF(this.scrRect.Right, this.scrRect.Bottom);

            corner1 = Tools.RotatePoint(corner1, midPoint, degrees);
            corner2 = Tools.RotatePoint(corner2, midPoint, degrees);

            this.scrRect.Location = new PointF(Math.Min(corner1.X, corner2.X), Math.Min(corner1.Y, corner2.Y));
            this.scrRect.Width = Math.Abs(corner1.X - corner2.X);
            this.scrRect.Height = Math.Abs(corner1.Y - corner2.Y);
            ConvertFromScreenCoordinates();
        }

        private void ConvertFromScreenCoordinates()
        {
            minX = (float)(scrRect.Left / scale);
            maxX = (float)(scrRect.Right / scale);
            minZ = (float)(scrRect.Top / scale);
            maxZ = (float)(scrRect.Bottom / scale);
        }
        private void ConvertFromMazeCoordinates()
        {
            scrRect.X = (float)(minX * scale);
            scrRect.Y= (float)(minZ * scale);
            scrRect.Width = (float) Math.Abs((maxX * scale) - scrRect.Left);
            scrRect.Height = (float) Math.Abs(((maxZ * scale) - scrRect.Top));
        }
        public override void Paint(ref Graphics gr)
        {
            Brush br;
            Pen p;
            if (selected == false)
            {
                //br = new SolidBrush(colorCur);
                br = new HatchBrush(HatchStyle.DashedDownwardDiagonal, Color.Brown, mazeColorRegular);



            }
            else
            {
                //br = new SolidBrush(colorSel);
                br = new HatchBrush(HatchStyle.ForwardDiagonal, Color.DarkKhaki, mazeColorSelected);

                if(this.moveToPos!=null)
                {
                    p = new Pen(Color.Aquamarine, 4);
                    gr.DrawLine(p, this.scrRect.Location.X+this.scrRect.Width/2,this.scrRect.Location.Y+this.scrRect.Height/2, this.moveToPos.ScrPoint.X, this.moveToPos.ScrPoint.Y);
                }
            }

            p = new Pen(Color.Black, 1);
            gr.FillRectangle(br, scrRect);
            gr.DrawRectangle(p, Rectangle.Round(scrRect));
            br.Dispose();

            
            //gr.DrawRectangle(new Pen(new HatchBrush(HatchStyle.Weave,Color.Blue)), scrRect);
        }

        public string PrintToTreeItem()
        {
            if (string.IsNullOrWhiteSpace(this.Label))
                return this.ID;
            else
                return this.ID + "[" + this.Label + "]";
        }

        public virtual bool PrintToFile(ref StreamWriter fp)
        {
            try
            {
                fp.WriteLine("-3\t2\t" + this.GetID().ToString() + "\t" + this.Label);
                fp.WriteLine(minX.ToString(".##;-.##;0") + "\t" + maxX.ToString(".##;-.##;0") + "\t" + minZ.ToString(".##;-.##;0") + "\t" + maxZ.ToString(".##;-.##;0") + "\t" + successMessage);
                fp.WriteLine(height.ToString(".##;-.##;0") + "\t" + offset.ToString(".##;-.##;0") + "\t" + returnValue + "\t0");
                return true;
            }
            catch
            {
                MessageBox.Show("Couldn't write EndRegion...", "MazeMaker", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public virtual XmlElement toXMLnode(XmlDocument doc)
        {
            XmlElement endRegNode = doc.CreateElement(string.Empty, "EndRegion", string.Empty);
            endRegNode.SetAttribute("label", this.Label);
            endRegNode.SetAttribute("id", this.GetID().ToString());

            XmlElement mzCoordNode = doc.CreateElement(string.Empty, "MzCoord", string.Empty);
            endRegNode.AppendChild(mzCoordNode);
            mzCoordNode.SetAttribute("x1", this.minX.ToString());
            mzCoordNode.SetAttribute("x2", this.maxX.ToString());
            mzCoordNode.SetAttribute("z1", this.minZ.ToString());
            mzCoordNode.SetAttribute("z2", this.maxZ.ToString());
            mzCoordNode.SetAttribute("y1", (this.Offset - this.Height / 2).ToString());
            mzCoordNode.SetAttribute("y2", (this.Offset+this.Height/2).ToString());

            XmlElement actionNode = doc.CreateElement(string.Empty, "Action", string.Empty);
            endRegNode.AppendChild(actionNode);
            if(this.MoveToPos!=null)
                actionNode.SetAttribute("moveToPos", this.MoveToPos.GetID().ToString());
            actionNode.SetAttribute("returnValue", this.ReturnValue.ToString());
            actionNode.SetAttribute("pointThreshold", this.PointThreshold.ToString());
            actionNode.SetAttribute("successMessage", this.SuccessMessage);


            return endRegNode;
        }

        new public virtual EndRegion Clone()
        {
            return Copy(true, 0);
        }

        public virtual EndRegion Copy(bool clone, int offsetX=0, int offsetY=0)
        {
            EndRegion temp = new EndRegion(this.scale, this.Label, -1);
            //temp.ID = this.ID;
            temp.Height = this.Height;
            temp.MaxX = this.MaxX;
            temp.MaxZ = this.MaxZ;
            temp.MinX = this.MinX;
            temp.MinZ = this.MinZ;
            temp.Offset = this.Offset;
            temp.MazeColorRegular = this.MazeColorRegular;
            temp.ReturnValue = this.ReturnValue;
            temp.SuccessMessage = this.SuccessMessage;
            temp.justCreated = this.justCreated;
            temp.moveToPos = this.moveToPos;
            temp.pointThreshold = this.pointThreshold;


            temp.Rect = new RectangleF(this.Rect.X + offsetX, this.Rect.Y + offsetY, this.Rect.Width, this.Rect.Height);
            if (clone)
            {
                temp.SetID(this.GetID());
            }
            else
            {
                temp.justCreated = true;
                temp.SetID();
            }


            return temp;
        }

    }
}
