#region Using directives

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Data;
using System.ComponentModel;
using System.Windows.Forms;
using System.Globalization;
using System.IO;
using System.Xml;

#endregion

namespace MazeMaker
{
    [TypeConverterAttribute(typeof(StartPosConverter)), DescriptionAttribute("StartPosition Support")]
    [Serializable]
    public class StartPos : MazeItem
    {
        
        public StartPos(double dScale, string label="", int id = 0, bool generateNewID = false) //0 for new, -1 for temporary, other for set specific Id
        {
            scale = dScale;
            if (generateNewID)
                id = 0;
            this.SetID(id);
            this.Label = label;
            itemType = MazeItemType.Start;
        }

        public StartPos(XmlNode startNode)
        {
            this.SetID(Tools.getIntFromAttribute(startNode, "id", -1));
            this.Label = Tools.getStringFromAttribute(startNode, "label", "");
            this.itemLocked = Tools.getBoolFromAttribute(startNode, "itemLocked", false);
            this.itemVisible = Tools.getBoolFromAttribute(startNode, "itemVisible", true);

            this.MzPoint = Tools.getXYZfromNode(startNode, 0);
            XmlNode viewAngleNode = startNode.SelectSingleNode("ViewAngle");
            this.AngleYaw = Tools.getIntFromAttribute(viewAngleNode, "angle",this.AngleYaw);
            this.AnglePitch = Tools.getIntFromAttribute(viewAngleNode, "angleVert",this.AnglePitch);
            this.RandomAngleYaw = Tools.getBoolFromAttribute(viewAngleNode, "randomAngle",this.RandomAngleYaw);
            this.RandomAnglePitch = Tools.getBoolFromAttribute(viewAngleNode, "randomVertAngle",this.RandomAnglePitch);
            itemType = MazeItemType.Start;
        }

        //private bool selected = false;

        private double scale = 17;
        [Browsable(false)]
        [Category("Options")]
        [Description("Coordinate transformation coefficient")]
        public double Scale
        {
            get { return scale; }
            set {
                //CalculateModifiedScaleMazeCoordinates(value);
                scale = value;
                ConvertFromMazeCoordinates();
                OnPropertyChanged("Scale");
                
            }
        }

        public void CalculateModifiedScaleMazeCoordinates(double newScale)
        {
            if (newScale == 0)
                throw new Exception("Scale can not be zero!");
            if (scale != newScale)
            {
                double coef = (newScale / scale);
                mzPoint.X *= coef;
                mzPoint.Z *= coef;
            }
        }

        public void SilentSetScale(double newScale)
        {
            scale = newScale;
            ConvertFromMazeCoordinates();
            OnPropertyChanged("Scale");
        }

        private Color mazeColorRegular = Color.LightSlateGray;
        [Browsable(false)]
        [Category("Colors")]
        [Description("The Current color in the editor")]
        public Color MazeColorRegular
        {
            get { return mazeColorRegular; }
            set { mazeColorRegular = value; OnPropertyChanged("MazeColor"); }
        }

        private Color mazeColorSelected = Color.DarkBlue;
        [Browsable(false)]
        [Category("Colors")]
        [Description("The Selection Mode color")]
        public Color MazeColorSelected
        {
            get { return mazeColorSelected; }
            set { mazeColorSelected = value; OnPropertyChanged("SelectColor"); }
        }

        private PointF scrPoint = new PointF(0, 0);
        [Category("3.Options")]
        [Description("The location of the start position on screen coordinates")]
        [Browsable(false)]
        public PointF ScrPoint
        {
            get { return scrPoint; }
            set { scrPoint = value; ConvertFromScreenCoordinates(); }
        }

        private MPoint mzPoint = new MPoint(0, 0, 0);
        [Category("2.Maze Coordinates")]
        [Description("The starting position on Maze coordinates")]
        public MPoint MzPoint
        {
            get { return mzPoint; }
            set { mzPoint = value; ConvertFromMazeCoordinates(); OnPropertyChanged("MzPoint"); }
        }

        private int angle_yaw = 0;
        [Category("3.Options")]
        [Description("The initial left-right view angle in XZ plane (yaw) in degrees")]
        public int AngleYaw
        {
            get { return angle_yaw; }
            set
            {
                angle_yaw = value;

                if (angle_yaw == -999)
                    randomAngleYaw = true;
                else
                    randomAngleYaw = false;
                OnPropertyChanged("randomAngleYaw");
            }
        }

        private int angle_pitch = 0;
        [Category("3.Options")]
        [Description("The initial up-down view angle in YZ plane (pitch) in degrees")]
        public int AnglePitch
        {
            get { return angle_pitch; }
            set
            { angle_pitch = value;

                if (angle_pitch == -999)
                    randomAnglePitch = true;
                else
                    randomAnglePitch = false;
                OnPropertyChanged("AnglePitch");
            }
        }

        private bool isDefaultStartPos = false;
        [Category("3.Options")]
        [Description("The default start point for the maze")]
        [DisplayName("Active Starting Point")]
        public bool IsDefaultStartPos
        {
            get { return isDefaultStartPos; }
            set
            {
                //foreach (StartPos sPos i)
                isDefaultStartPos = value;
                OnPropertyChanged("DefaultStartPos",true);
            }
        }

        private bool randomAngleYaw = false;
        [Category("3.Options")]
        [Description("If true, a random initial view left-right angle used instead of the current angle value")]
        public bool RandomAngleYaw
        {
            get { return randomAngleYaw; }
            set
            {
                randomAngleYaw = value;
                if (randomAngleYaw)
                {
                    angle_yaw = -999;
                }
                else
                    angle_yaw = 0;
                OnPropertyChanged("randomAngleYaw");
            }
        }

        private bool randomAnglePitch = false;
        [Category("3.Options")]
        [Description("If true, a random initial view up-down angle used instead of the current angle value")]
        public bool RandomAnglePitch
        {
            get { return randomAnglePitch; }
            set { randomAnglePitch = value;
                if (randomAnglePitch)
                {
                    angle_pitch = -999;
                }
                else
                { 
                    angle_pitch = 0;

                }
                OnPropertyChanged("RandomAnglePitch");
                }
        }

        private void ConvertFromScreenCoordinates()
        {
            mzPoint.X = scrPoint.X  / scale;
            //mzPoint.Y = 0;
            mzPoint.Z = scrPoint.Y / scale;
        }


        public void SetElevation(double newElevation, bool addToCurrent = false)
        {

            if (addToCurrent)
            {
                mzPoint.Y = mzPoint.Y + newElevation;

            }
            else
            {

                mzPoint.Y = newElevation;
            }

        }
        private void ConvertFromMazeCoordinates()
        {
            scrPoint.X =(float) (mzPoint.X * scale);
            scrPoint.Y =(float) (mzPoint.Z * scale);
        }
        public override void Paint(ref Graphics gr)
        {
            if (!itemVisible&&!selected)
                return;

            Brush br;
            Pen p;

            Color colorDefault = Color.OrangeRed;

            if (selected == true)
            {
                br = new SolidBrush(mazeColorSelected);
                gr.FillEllipse(br, scrPoint.X - 11, scrPoint.Y - 11, 22, 22);
                
                p = new Pen(mazeColorSelected,8);
                gr.DrawLine(p, scrPoint, new PointF((float)(Math.Cos(angle_yaw * Math.PI / 180) * 17) + scrPoint.X, -(float)Math.Sin(angle_yaw * Math.PI / 180) * 17 + scrPoint.Y));
                p.Dispose();
                br.Dispose();
            }

            if (isDefaultStartPos == true)
            {
                
                br = new SolidBrush(colorDefault);

                if(selected)
                    gr.FillEllipse(br, scrPoint.X - 9, scrPoint.Y - 9, 18, 18);
                else
                    gr.FillEllipse(br, scrPoint.X - 10, scrPoint.Y - 10, 20, 20);

                p = new Pen(colorDefault, 8);
                gr.DrawLine(p, scrPoint, new PointF((float)(Math.Cos(angle_yaw * Math.PI / 180) * 17) + scrPoint.X, -(float)Math.Sin(angle_yaw * Math.PI / 180) * 17 + scrPoint.Y));
                p.Dispose();
                br.Dispose();
            }


            br = new SolidBrush(mazeColorRegular);
            gr.FillEllipse(br, scrPoint.X - 8, scrPoint.Y - 8, 16, 16);
            gr.DrawEllipse(Pens.Black, scrPoint.X - 8, scrPoint.Y - 8, 16, 16);
            p = new Pen(Color.Black, 5);
            gr.DrawLine(p, scrPoint, new PointF((float)(Math.Cos(angle_yaw * Math.PI / 180) * 14) + scrPoint.X, -(float)Math.Sin(angle_yaw * Math.PI / 180) * 14 + scrPoint.Y));
            p.Dispose();
            p = new Pen(mazeColorRegular, 4);
            gr.DrawLine(p, scrPoint, new PointF((float)(Math.Cos(angle_yaw * Math.PI / 180) * 13) + scrPoint.X, -(float)Math.Sin(angle_yaw * Math.PI / 180) * 13 + scrPoint.Y));            

            br.Dispose();
            p.Dispose();
        }

        public override void Move(float mzXdir, float mzZdir)
        {
            MzPoint.X += mzXdir;
            MzPoint.Z += mzZdir;


            ConvertFromMazeCoordinates();
        }

        public override void Rescale(double factor)
        {
            MzPoint.X *= factor;
            MzPoint.Z *= factor;


            ConvertFromMazeCoordinates();
        }


        public override void RescaleXYZ(double scaleX, double scaleY, double scaleZ)
        {
            MzPoint.X *= scaleX;
            MzPoint.Y *= scaleY;
            MzPoint.Z *= scaleZ;


            ConvertFromMazeCoordinates();
        }

        public override void Rotate(float degrees, float centerX = 0, float centerY = 0)
        {
            PointF midPoint;

            this.AngleYaw -= (int)degrees;

            if (centerX != 0 && centerY != 0)
                midPoint = new PointF(centerX, centerY);
            else
                return;


            this.ScrPoint = Tools.RotatePoint(this.ScrPoint, midPoint, degrees);
        }

        public bool IfSelected(int x, int y)
        {
            
            if ((x - scrPoint.X) * (x - scrPoint.X) + (y - scrPoint.Y) * (y - scrPoint.Y) < 100)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool InRegion(int x1, int y1, int x2, int y2)
        {
            if (!itemVisible)
                return false;

            int iconTolerence = 15;
            x1 = x1 - iconTolerence;
            x2 = x2 + iconTolerence;
            y1 = y1 - iconTolerence;
            y2 = y2 + iconTolerence;

            if (scrPoint.X>x1&&scrPoint.X<x2&&scrPoint.Y>y1&&scrPoint.Y<y2)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public string PrintToTreeItem()
        {
            String str = this.ID;
            if (!this.itemVisible)
                str += "👁";
            if (this.itemLocked)
                str += "🔒";

            if (!string.IsNullOrWhiteSpace(this.Label))
                str += "[" + this.Label + "]";
            if (this.isDefaultStartPos)
                return str + " (Active)";
            else
                return str;
        }
        public virtual bool PrintToFile(ref StreamWriter fp)
        {
            try
            {
                fp.WriteLine("-2\t1\t"+ this.GetID() + "\t" + this.Label);
                //fp.WriteLine(mzPoint.X.ToString(".##;-.##;0") + "\t" + mzPoint.Y.ToString(".##;-.##;0") + "\t" + mzPoint.Z.ToString(".##;-.##;0") + "\t" + angle + "\t" + (randomAngle?"1":"0") + "\t0");
                fp.WriteLine(mzPoint.X.ToString(".##;-.##;0") + "\t" + mzPoint.Y.ToString(".##;-.##;0") + "\t" + mzPoint.Z.ToString(".##;-.##;0") + "\t" + angle_yaw + "\t" + angle_pitch + "\t0");
                return true;
            }
            catch
            {
                MessageBox.Show("Couldn't write StartPos...","MazeMaker", MessageBoxButtons.OK , MessageBoxIcon.Error);
                return false;
            }
        }

        public XmlElement toXMLnode(XmlDocument doc)
        {
            XmlElement startPosNode = doc.CreateElement(string.Empty, "StartPosition", string.Empty);
            startPosNode.SetAttribute("label", this.Label);
            startPosNode.SetAttribute("id", this.GetID().ToString());
            startPosNode.SetAttribute("itemLocked", this.itemLocked.ToString());
            startPosNode.SetAttribute("itemVisible", this.itemVisible.ToString());

            XmlElement mzPointNode = doc.CreateElement(string.Empty, "MzPoint", string.Empty);
            startPosNode.AppendChild(mzPointNode);
            mzPointNode.SetAttribute("x", this.MzPoint.X.ToString());
            mzPointNode.SetAttribute("y", this.MzPoint.Y.ToString());
            mzPointNode.SetAttribute("z", this.MzPoint.Z.ToString());

            XmlElement viewAngleNode = doc.CreateElement(string.Empty, "ViewAngle", string.Empty);
            startPosNode.AppendChild(viewAngleNode);
            viewAngleNode.SetAttribute("angle", this.AngleYaw.ToString());
            viewAngleNode.SetAttribute("vertAngle", this.AnglePitch.ToString());
            viewAngleNode.SetAttribute("randomAngle", this.RandomAngleYaw.ToString());
            viewAngleNode.SetAttribute("randomVertAngle", this.RandomAnglePitch.ToString());

            //XmlElement optionsNode = doc.CreateElement(string.Empty, "Options", string.Empty);
            //startPosNode.AppendChild(optionsNode);
            //optionsNode.SetAttribute("isDefaultStartingPoint", this.IsDefaultStartPos.ToString());

            return startPosNode;
        }

        new public StartPos Clone()
        {
            return Copy(true, 0);
        }


        public StartPos Copy(bool clone=false,int offsetX=0, int offsetY=0)
        {
            StartPos temp = new StartPos(this.scale, this.Label, -1);

            temp.itemLocked = this.itemLocked;
            temp.itemVisible = this.itemVisible;

            temp.AnglePitch = this.AnglePitch;
            temp.AnglePitch = this.AngleYaw;
           // temp.ID = this.ID;
            temp.justCreated = this.justCreated;
            temp.MzPoint = this.MzPoint;
            temp.RandomAnglePitch = this.RandomAnglePitch;
            temp.RandomAngleYaw = this.RandomAngleYaw;
            temp.MazeColorRegular = this.MazeColorRegular;
            temp.ScrPoint = new PointF(this.ScrPoint.X + offsetX, this.ScrPoint.Y + offsetY);
            temp.isDefaultStartPos = this.isDefaultStartPos;

            if (clone)
            {
                temp.SetID(this.GetID(),true);
            }
            else
            {
                temp.justCreated = true;
                temp.SetID();
                this.isDefaultStartPos = false;
            }
            return temp;
        }
    }

    public class StartPosConverter : ExpandableObjectConverter
    {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            //true means show a combobox
            return true;
        }
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            //true will limit to list. false will show the list, but allow free-form entry
            return true;
        }

        public override
        System.ComponentModel.TypeConverter.StandardValuesCollection
        GetStandardValues(ITypeDescriptorContext context)
        {
            List<StartPos> theArray = new List<StartPos>();
            //theArray.Add(new Texture(null,null,0));
            theArray.Add(null);
            List<StartPos> list = MazeMaker.Maze.GetStartPositions();
            foreach (StartPos sPos in list)
            {
                theArray.Add(sPos);
            }

            return new StandardValuesCollection(theArray.ToArray());
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, System.Type destinationType)
        {
            if (destinationType == typeof(StartPos))
                return true;
            return base.CanConvertTo(context, destinationType);
        }
        public override bool CanConvertFrom(ITypeDescriptorContext context, System.Type sourceType)
        {
            if (sourceType == typeof(String))
                return true;

            return base.CanConvertFrom(context, sourceType);
        }
        public override object ConvertTo(ITypeDescriptorContext context,
                               CultureInfo culture,
                               object value,
                               System.Type destinationType)
        {
            if (destinationType == typeof(System.String) &&
                 value is StartPos)
            {
                StartPos sPos = (StartPos)value;
                String cmpString = (sPos.ID);
                if (sPos.Label.Length > 0)
                    cmpString += "[" + sPos.Label + "]";
                return cmpString;
            }
            else if (value == null)
                return "Random Start Position";
            return base.ConvertTo(context, culture, value, destinationType);
        }
        public override object ConvertFrom(ITypeDescriptorContext context,
                              CultureInfo culture, object value)
        {
            if (value is string)
            {
                try
                {
                    //if (value == "[ none ]")
                    if (value == null || (string)value == "")
                        return null;
                    List<StartPos> cList = Maze.GetStartPositions();
                    foreach (StartPos sPos in cList)
                    {
                        String cmpString = (sPos.ID);
                        if (sPos.Label.Length > 0)
                            cmpString += "[" + sPos.Label + "]";
                        if (cmpString.CompareTo(value) == 0)
                            return sPos;
                    }
                    
                    return null;
                }
                catch
                {
                    throw new ArgumentException("Can not convert '" + (string)value + "' to type Start Position");
                }
                return value;
            }
            return base.ConvertFrom(context, culture, value);
        }

    }

    public class StartPosMoveConverter : ExpandableObjectConverter
    {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            //true means show a combobox
            return true;
        }
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            //true will limit to list. false will show the list, but allow free-form entry
            return true;
        }

        public override
        System.ComponentModel.TypeConverter.StandardValuesCollection
        GetStandardValues(ITypeDescriptorContext context)
        {
            List<StartPos> theArray = new List<StartPos>();
            //theArray.Add(new Texture(null,null,0));
            theArray.Add(null);
            List<StartPos> list = MazeMaker.Maze.GetStartPositions();
            foreach (StartPos sPos in list)
            {
                theArray.Add(sPos);
            }

            return new StandardValuesCollection(theArray.ToArray());
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, System.Type destinationType)
        {
            if (destinationType == typeof(StartPos))
                return true;
            return base.CanConvertTo(context, destinationType);
        }
        public override bool CanConvertFrom(ITypeDescriptorContext context, System.Type sourceType)
        {
            if (sourceType == typeof(String))
                return true;

            return base.CanConvertFrom(context, sourceType);
        }
        public override object ConvertTo(ITypeDescriptorContext context,
                               CultureInfo culture,
                               object value,
                               System.Type destinationType)
        {
            if (destinationType == typeof(System.String) &&
                 value is StartPos)
            {
                StartPos sPos = (StartPos)value;
                String cmpString ="Move to "+ (sPos.ID);
                if (sPos.Label.Length > 0)
                    cmpString += "[" + sPos.Label + "]";
                return cmpString;
            }
            else if (value == null)
                return "Exit Maze";
            return base.ConvertTo(context, culture, value, destinationType);
        }
        public override object ConvertFrom(ITypeDescriptorContext context,
                              CultureInfo culture, object value)
        {
            if (value is string)
            {
                try
                {
                    //if (value == "[ none ]")
                    if (value == null || (string)value == "")
                        return null;
                    List<StartPos> cList = Maze.GetStartPositions();
                    foreach (StartPos sPos in cList)
                    {
                        String cmpString = "Move to "+(sPos.ID);
                        if (sPos.Label.Length > 0)
                            cmpString += "[" + sPos.Label + "]";
                        if (cmpString.CompareTo(value) == 0)
                            return sPos;
                    }

                    return null;
                }
                catch
                {
                    throw new ArgumentException("Can not convert '" + (string)value + "' to type Start Position");
                }
                return value;
            }
            return base.ConvertFrom(context, culture, value);
        }

    }

    public class StartPosMoveWNoneConverter : ExpandableObjectConverter
    {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            //true means show a combobox
            return true;
        }
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            //true will limit to list. false will show the list, but allow free-form entry
            return true;
        }

        public override
        System.ComponentModel.TypeConverter.StandardValuesCollection
        GetStandardValues(ITypeDescriptorContext context)
        {
            List<StartPos> theArray = new List<StartPos>();
            //theArray.Add(new Texture(null,null,0));
            theArray.Add(null);
            List<StartPos> list = MazeMaker.Maze.GetStartPositions();
            foreach (StartPos sPos in list)
            {
                theArray.Add(sPos);
            }

            return new StandardValuesCollection(theArray.ToArray());
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, System.Type destinationType)
        {
            if (destinationType == typeof(StartPos))
                return true;
            return base.CanConvertTo(context, destinationType);
        }
        public override bool CanConvertFrom(ITypeDescriptorContext context, System.Type sourceType)
        {
            if (sourceType == typeof(String))
                return true;

            return base.CanConvertFrom(context, sourceType);
        }
        public override object ConvertTo(ITypeDescriptorContext context,
                               CultureInfo culture,
                               object value,
                               System.Type destinationType)
        {
            if (destinationType == typeof(System.String) &&
                 value is StartPos)
            {
                StartPos sPos = (StartPos)value;
                String cmpString = "Move to " + (sPos.ID);
                if (sPos.Label.Length > 0)
                    cmpString += "[" + sPos.Label + "]";
                return cmpString;
            }
            else if (value == null)
                return "None";
            return base.ConvertTo(context, culture, value, destinationType);
        }
        public override object ConvertFrom(ITypeDescriptorContext context,
                              CultureInfo culture, object value)
        {
            if (value is string)
            {
                try
                {
                    //if (value == "[ none ]")
                    if (value == null || (string)value == "")
                        return null;
                    List<StartPos> cList = Maze.GetStartPositions();
                    foreach (StartPos sPos in cList)
                    {
                        String cmpString = "Move to " + (sPos.ID);
                        if (sPos.Label.Length > 0)
                            cmpString += "[" + sPos.Label + "]";
                        if (cmpString.CompareTo(value) == 0)
                            return sPos;
                    }

                    return null;
                }
                catch
                {
                    throw new ArgumentException("Can not convert '" + (string)value + "' to type Start Position");
                }
                return value;
            }
            return base.ConvertFrom(context, culture, value);
        }

    }

}
