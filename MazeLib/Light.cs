#region Using directives

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Data;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;
using System.Xml;

#endregion

namespace MazeMaker
{
    [Serializable]
    public class Light : MazeItem
    {
        

        public Light(double dScale, string label="", int id = 0, bool generateNewID = false) //0 for new, -1 for temporary, other for set specific Id
        {
            this.Scale = dScale;
            if (generateNewID)
                id = 0;
            this.SetID(id);
            this.Label = label;
            itemType = MazeItemType.Light;
        }

        public Light(XmlNode lightNode)
        {
            this.SetID(Tools.getIntFromAttribute(lightNode, "id", -1));
            this.Group = Tools.getStringFromAttribute(lightNode, "group", "");
            this.Label = Tools.getStringFromAttribute(lightNode, "label", "");
            this.itemLocked = Tools.getBoolFromAttribute(lightNode, "itemLocked", false);
            this.itemVisible = Tools.getBoolFromAttribute(lightNode, "itemVisible", true);

            this.MzPoint = Tools.getXYZfromNode(lightNode, 0);
            this.DiffuseColor = Tools.getColorFromNode(lightNode);
            this.AmbientColor = Tools.getColorFromNode(lightNode, "AmbientColor");
            XmlNode appearanceNode = lightNode.SelectSingleNode("Appearance");
            this.Attenuation = Tools.getDoubleFromAttribute(appearanceNode, "attenuation", this.Attenuation);
            this.diffuseIntensity = (float)Tools.getDoubleFromAttribute(appearanceNode, "intensity", this.diffuseIntensity);
            this.ambientIntensity = (float)Tools.getDoubleFromAttribute(appearanceNode, "ambientIntensity", this.ambientIntensity);
            this.Type = (LightTypes)Enum.Parse(typeof(LightTypes),Tools.getStringFromAttribute(appearanceNode, "type"));
            itemType = MazeItemType.Light;
        }

       // private bool selected = false;
        private double scale = 17;

        [Browsable(false)]
        [Category("Options")]
        [Description("Coordinate transformation coefficient")]
        public double Scale
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
            if (centerX != 0 && centerY != 0)
                midPoint = new PointF(centerX, centerY);
            else
                return;

            this.ScrPoint = Tools.RotatePoint(this.ScrPoint, midPoint, degrees);
        }

        private Color mazeColorRegular = Color.Yellow;
        [Browsable(false)]
        [Category("Display Colors")]
        [Description("The Current color in the editor")]
        public Color MazeColorRegular
        {
            get { return mazeColorRegular; }
            set { mazeColorRegular = value; OnPropertyChanged("RegularColor"); }
        }

        private Color mazeColorSelected = Color.DarkBlue;
        [Browsable(false)]
        [Category("Display Colors")]
        [Description("The Selection Mode color")]
        public Color MazeColorSelected
        {
            get { return mazeColorSelected; }
            set { mazeColorSelected = value; OnPropertyChanged("SelectColor"); }
        }

        private PointF scrPoint = new PointF(0, 0);
        [Category("Location")]
        [Browsable(false)]
        [Description("The location of the start position on screen coordinates")]
        public PointF ScrPoint
        {
            get { return scrPoint; }
            set { scrPoint = value; ConvertFromScreenCoordinates(); OnPropertyChanged("Location"); }
        }
        private MPoint mzPoint = new MPoint(0, 0.9, 0);
        [Category("2.Maze Coordinates")]
        [Description("The starting position on Maze coordinates")]
        public MPoint MzPoint
        {
            get { return mzPoint; }
            set { mzPoint = value; ConvertFromMazeCoordinates(); OnPropertyChanged("MazeCoord"); }
        }

        private Color ambientColor = Color.White;
        [Category("Ambient Light")]
        [Description("Ambient light color of the current light source")]
        [Browsable(false)]
        public Color AmbientColor
        {
            get { return ambientColor; }
            set { ambientColor = value; OnPropertyChanged("AmbientColor"); }
        }

        private float ambientIntensity = 0.1f;
        [Category("Ambient Light")]
        [Description("Ambient light intensity of the current light source. Min=0.0f, Max=1.0f")]
        [Browsable(false)]
        public float AmbientIntesity
        {
            get { return ambientIntensity; }
            set { ambientIntensity = value; OnPropertyChanged("AmbientIntensity"); }
        }

        private Color diffuseColor = Color.White;
        [Category("3.Appearance")]
        [Description("Diffuse light color of the current light source")]
        [DisplayName("Color")]
        public Color DiffuseColor
        {
            get { return diffuseColor; }
            set { diffuseColor = value; OnPropertyChanged("DiffuseColor"); }
        }

        private float diffuseIntensity = 1.0f;
        [Category("3.Appearance")]
        [Description("Diffuse light intensity of the current light source. Min=0.0f, Max=1.0f")]
        [DisplayName("Intensity")]
        public float DiffuseIntesity
        {
            get { return diffuseIntensity; }
            set { diffuseIntensity = value; OnPropertyChanged("DiffuseIntensity"); }
        }

        public enum LightTypes
        {
            Ambulatory = 0,
            Stationary = 1,
        }


        private int type = 1;  //0->Torch and 1->Regular
//        [TypeConverter(typeof(LightConverter))]
         [Category("3.Appearance")]
        [Description("Type of light. Ambulatory type light navigates with the user within the Maze. Stationary is a regular point light source that does not move.")]
        public LightTypes Type
        {
            get {
                return (LightTypes)type; 
            }
            set {
                type = (int)value;
                OnPropertyChanged("Type");
            }
        }

        private double attenuation = 0.080;
        [Category("3.Appearance")]
        [Description("Attenuation amount of the light. ")]
        public double Attenuation
        {
            get { return attenuation; }
            set { attenuation = value; OnPropertyChanged("Attenuation"); }
        }

        private void ConvertFromScreenCoordinates()
        {
            mzPoint.X = scrPoint.X / scale;
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
            scrPoint.X = (float)(mzPoint.X * scale);
            scrPoint.Y = (float)(mzPoint.Z * scale);
        }

        public double getY()
        {
            return mzPoint.Y;
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

            if (scrPoint.X > x1 && scrPoint.X < x2 && scrPoint.Y > y1 && scrPoint.Y < y2)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        //public bool IsSelected()
        //{
        //    return selected;
        //}
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

        public override void Paint(ref Graphics gr)
        {
            this.Paint(ref gr, 1);
        }

        public void Paint(ref Graphics gr, float opacity = 1)
        {

            if (!itemVisible&&!selected)
                return;

            Brush br;
            //Pen p;
            if (selected == true)
            {
                br = new SolidBrush(Color.FromArgb((int)(255*opacity),mazeColorSelected));
                gr.FillEllipse(br, scrPoint.X - 11, scrPoint.Y - 11, 22, 22);

                //p = new Pen(Color.Beige, 8);
                //gr.DrawLine(p, scrPoint, new PointF((float)(Math.Cos(angle * Math.PI / 180) * 17) + scrPoint.X, -(float)Math.Sin(angle * Math.PI / 180) * 17 + scrPoint.Y));
                //p.Dispose();
                br.Dispose();
            }
            br = new SolidBrush(Color.FromArgb((int)(255 * opacity), mazeColorRegular));
            Pen p = new Pen(Color.FromArgb((int)(255*opacity),Color.Black), 5);
            gr.FillEllipse(br, scrPoint.X - 8, scrPoint.Y - 8, 16, 16);
            gr.DrawEllipse(p, scrPoint.X - 8, scrPoint.Y - 8, 16, 16);
            //p = new Pen(Color.Black, 5);
            //gr.DrawLine(p, scrPoint, new PointF((float)(Math.Cos(10 * Math.PI / 180) * 14) + scrPoint.X, -(float)Math.Sin(angle * Math.PI / 180) * 14 + scrPoint.Y));
            //p.Dispose();
            //p = new Pen(colorCur, 4);
            // gr.DrawLine(p, scrPoint, new PointF((float)(Math.Cos(angle * Math.PI / 180) * 13) + scrPoint.X, -(float)Math.Sin(angle * Math.PI / 180) * 13 + scrPoint.Y));

            br.Dispose();
            p.Dispose();
        }
        public string PrintToTreeItem()
        {
            string str = this.ID;
            if (!this.itemVisible)
                str += "??";
            if (this.itemLocked)
                str += "??";
            if (string.IsNullOrWhiteSpace(this.Label))
                return str;
            else
                return str + "[" + this.Label + "]";
        }
        public bool PrintToFile(ref StreamWriter fp)
        {
            try
            {
                fp.WriteLine("-9\t4\t" + this.GetID().ToString() + "\t" + this.Label);
                if(type==1)
                    fp.WriteLine(mzPoint.X.ToString(".##;-.##;0") + "\t" + mzPoint.Y.ToString(".##;-.##;0") + "\t" + mzPoint.Z.ToString(".##;-.##;0") + "\t1" );
                else                
                    fp.WriteLine("0\t0\t0\t0");
                fp.WriteLine( (ambientColor.R/255.0).ToString(".##;-.##;0") + "\t" + (ambientColor.G/255.0).ToString(".##;-.##;0") + "\t" + (ambientColor.B/255.0).ToString(".##;-.##;0") + "\t" + ambientIntensity.ToString(".##;-.##;0"));
                fp.WriteLine((diffuseColor.R/255.0).ToString(".##;-.##;0") + "\t" + (diffuseColor.G/255.0).ToString(".##;-.##;0") + "\t" + (diffuseColor.B/255.0).ToString(".##;-.##;0") + "\t" + diffuseIntensity.ToString(".##;-.##;0"));
                fp.WriteLine(attenuation.ToString());
                return true;
            }
            catch
            {
                MessageBox.Show("Couldn't write Light...", "MazeMaker", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public XmlElement toXMLnode(XmlDocument doc)
        {
            XmlElement lightNode = doc.CreateElement(string.Empty, "Light", string.Empty);
            lightNode.SetAttribute("group", this.Group);
            lightNode.SetAttribute("label", this.Label);
            lightNode.SetAttribute("id", this.GetID().ToString());
            lightNode.SetAttribute("itemLocked", this.itemLocked.ToString());
            lightNode.SetAttribute("itemVisible", this.itemVisible.ToString());

            XmlElement mzPointnode = doc.CreateElement(string.Empty, "MzPoint", string.Empty);
            lightNode.AppendChild(mzPointnode);
            mzPointnode.SetAttribute("x", this.MzPoint.X.ToString());
            mzPointnode.SetAttribute("y", this.MzPoint.Y.ToString());
            mzPointnode.SetAttribute("z", this.MzPoint.Z.ToString());

            //XmlElement ambientColorNode = doc.CreateElement(string.Empty, "AmbientColor", string.Empty);
            //lightNode.AppendChild(ambientColorNode);
            //ambientColorNode.SetAttribute("r", this.AmbientColor.R.ToString());
            //ambientColorNode.SetAttribute("g", this.AmbientColor.G.ToString());
            //ambientColorNode.SetAttribute("b", this.AmbientColor.B.ToString());

            XmlElement colorNode = doc.CreateElement(string.Empty, "Color", string.Empty);
            lightNode.AppendChild(colorNode);
            colorNode.SetAttribute("r", ((float)this.DiffuseColor.R / 255).ToString());
            colorNode.SetAttribute("g", ((float)this.DiffuseColor.G / 255).ToString());
            colorNode.SetAttribute("b", ((float)this.DiffuseColor.B / 255).ToString());

            XmlElement appearanceNode = doc.CreateElement(string.Empty, "Appearance", string.Empty);
            lightNode.AppendChild(appearanceNode);
            appearanceNode.SetAttribute("attenuation", this.Attenuation.ToString());
            appearanceNode.SetAttribute("intensity", this.DiffuseIntesity.ToString());
            appearanceNode.SetAttribute("type", this.Type.ToString());

            return lightNode;
        }

        new public Light Clone()
        {
            return Copy(true, 0);
        }

        public Light Copy(bool clone=false, int offsetX=0, int offsetY=0)
        {
            Light temp = new Light(this.scale, this.Label, -1);

            temp.itemLocked = this.itemLocked;
            temp.itemVisible = this.itemVisible;
            //temp.ID = this.ID;
            temp.AmbientColor = this.AmbientColor;
            temp.AmbientIntesity = this.AmbientIntesity;
            temp.Attenuation = this.Attenuation;
            temp.DiffuseColor = this.DiffuseColor;
            temp.DiffuseIntesity = this.DiffuseIntesity;
            temp.MzPoint = new MPoint(this.MzPoint);
            temp.MazeColorRegular = this.MazeColorRegular;
            temp.ScrPoint = new PointF(this.ScrPoint.X + offsetX, this.ScrPoint.Y + offsetY);
            temp.Type = this.Type;
            temp.justCreated = this.justCreated;

            if (clone)
            {
                temp.SetID(this.GetID(), true);
                temp.Group = this.Group;
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
