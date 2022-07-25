using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Data;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;
using System.Xml;

namespace MazeMaker
{
    [Serializable]
    public class StaticModel : MazeItem
    {
        public StaticModel()
        {

        }
        public StaticModel(double dScale, string label="", int id = 0, bool generateNewID = false) //0 for new, -1 for temporary, other for set specific Id
        {
            scale = dScale;
            if (generateNewID)
                id = 0;
            this.SetID(id);
            this.Label = label;
            itemType = MazeItemType.Static;
        }

        public StaticModel(XmlNode sModelNode)
        {
            this.SetID(Tools.getIntFromAttribute(sModelNode, "id", -1));
            this.Group = Tools.getStringFromAttribute(sModelNode, "group", "");
            this.Label = Tools.getStringFromAttribute(sModelNode, "label", "");
            this.itemLocked = Tools.getBoolFromAttribute(sModelNode, "itemLocked", false);
            this.itemVisible = Tools.getBoolFromAttribute(sModelNode, "itemVisible", true);

            this.MzPoint = Tools.getXYZfromNode(sModelNode, 0);
            XmlNode modelNode = sModelNode.SelectSingleNode("Model");
            XmlNode physicsNode = sModelNode.SelectSingleNode("Physics");

            this.modelID = Tools.getIntFromAttribute(modelNode, "id", -999);
            this.ModelScale = Tools.getDoubleFromAttribute(modelNode, "scale", 1);
            this.MzPointRot = Tools.getXYZfromNode(sModelNode, -1, "Model");

            this.Collision = Tools.getBoolFromAttribute(physicsNode,"collision", this.Collision);
            this.Mass = Tools.getDoubleFromAttribute(physicsNode, "mass", this.Mass);
            this.Kinematic = Tools.getBoolFromAttribute(physicsNode, "kinematic", this.Kinematic);
            itemType = MazeItemType.Static;
        }

      //  protected bool selected = false;

        protected double scale = 17;
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
                //mzPoint.X *= coef;
                //mzPoint.Z *= coef;
            }
        }

        public void SilentSetScale(double newScale)
        {
            scale = newScale;
            ConvertFromMazeCoordinates();
            OnPropertyChanged("Scale");
        }

        private Color mazeColorRegular = Color.BlanchedAlmond;
        [Browsable(false)]
        [Category("Colors")]
        [Description("The Current color in the editor")]
        public Color MazeColorRegular
        {
            get { return mazeColorRegular; }
            set { mazeColorRegular = value;
                OnPropertyChanged("Colors");
            }
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
        [Category("Location")]
        [Browsable(false)]
        [Description("The center location of the model on screen coordinates")]
        public PointF ScrPoint
        {
            get { return scrPoint; }
            set { scrPoint = value; ConvertFromScreenCoordinates(); OnPropertyChanged("Location"); }
        }
        private MPoint mzPoint = new MPoint(0, 0);
        [Category("2.Maze Coordinates")]
        [Description("The center location on Maze coordinates")]
        public MPoint MzPoint
        {
            get { return mzPoint; }
            set { mzPoint = value; ConvertFromMazeCoordinates(); OnPropertyChanged("MazeCoord"); }
        }

        private MPoint mzPointRot = new MPoint(0, 0, 0);
        [Category("3.Model")]
        [DisplayName("Rotation")]
        [Description("The rotation in degrees on each coordinates")]
        public MPoint MzPointRot
        {
            get { return mzPointRot; }
            set { mzPointRot = value; OnPropertyChanged("Rotation"); }
        }

        public int modelID = -999;
        private string model = "";
        [Category("3.Model")]
        [Description("Select model to be used with this object. List can be edited from Maze 'models' properties")]
        [TypeConverter(typeof(ModelPathConverter))]
        public string Model
        {
            get { return model; }
            set { model = value; OnPropertyChanged("Model"); }
        }

        private double modelscale = 1;
        [Category("3.Model")]
        [Description("The model scale")]
        public double ModelScale
        {
            get { return modelscale; }
            set { modelscale = value; OnPropertyChanged("ModelScale"); }
        }

        private bool enableCollision= true;
        [Category("4.Physics")]
        [Description("Enable collision detection for the model")]
        public bool Collision
        {
            get { return enableCollision; }
            set { enableCollision = value; OnPropertyChanged("Physics"); }
        }

        private bool kinematic = false;
        [Category("4.Physics")]
        [Description("Enable kinematic interactivity for the model")]
        public bool Kinematic
        {
            get { return kinematic; }
            set 
            {
                kinematic = value;
                if (kinematic)
                {
                    enableCollision = true;
                    if (mass <= 0)
                        mass = 1;
                }
                OnPropertyChanged("Kinematic");
            }
        }

        private double mass = 1;
        [Category("4.Physics")]
        [Description("Mass of the model")]
        public double Mass
        {
            get { return mass; }
            set
            {
                mass = value;
                if (mass <= 0)
                {
                    kinematic = false;
                    mass = 0;
                }
                OnPropertyChanged("Mass");
            }
        }



        protected void ConvertFromScreenCoordinates()
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
        public void ConvertFromMazeCoordinates()
        {
            scrPoint.X = (float)(mzPoint.X * scale);
            scrPoint.Y = (float)(mzPoint.Z * scale);
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
            gr.FillEllipse(br, scrPoint.X - 8, scrPoint.Y - 8, 16, 16);
            gr.DrawEllipse(Pens.Black, scrPoint.X - 8, scrPoint.Y - 8, 16, 16);
            //p = new Pen(Color.Black, 5);
            //gr.DrawLine(p, scrPoint, new PointF((float)(Math.Cos(10 * Math.PI / 180) * 14) + scrPoint.X, -(float)Math.Sin(angle * Math.PI / 180) * 14 + scrPoint.Y));
            //p.Dispose();
            //p = new Pen(colorCur, 4);
            // gr.DrawLine(p, scrPoint, new PointF((float)(Math.Cos(angle * Math.PI / 180) * 13) + scrPoint.X, -(float)Math.Sin(angle * Math.PI / 180) * 13 + scrPoint.Y));
            if(model!=null)
            {
                Font ft = new Font(FontFamily.GenericSerif, 7);
                gr.DrawString("S", ft, Brushes.BlueViolet, scrPoint.X - 5, scrPoint.Y - 5);
                ft.Dispose();
            }
            br.Dispose();
            //p.Dispose();
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

            this.MzPointRot.X += degrees;

            if (centerX != 0 && centerY != 0)
                midPoint = new PointF(centerX, centerY);
            else
                return;


            this.ScrPoint = Tools.RotatePoint(this.ScrPoint, midPoint, degrees);
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
        public string PrintToTreeItem()
        {
            string str = this.ID;
            if (!this.itemVisible)
                str += "👁";
            if (this.itemLocked)
                str += "🔒";

            if (string.IsNullOrWhiteSpace(this.Label)==false)
                str += " [" + this.Label + "]";

            if(this.Model != null)
            {
                str += "(" + this.Model + ")";
            }

            return str;
        }
        public bool PrintToFile(ref StreamWriter fp, Dictionary<string, string> cModels)
        {
            try
            {
                fp.WriteLine("10\t3\t" + this.GetID().ToString() + "\t" + this.Label);
                //fp.WriteLine( ((model==null)?"-1":model.Index.ToString()) + "\t" + mzPoint.X.ToString(".##;-.##;0") + "\t" + mzPoint.Y.ToString(".##;-.##;0") + "\t" + mzPoint.Z.ToString(".##;-.##;0"));
                string modelID = "0";
                if (cModels.ContainsKey(model))
                    modelID = cModels[model];
                fp.WriteLine(modelID + "\t" + mzPoint.X.ToString(".##;-.##;0") + "\t" + mzPoint.Y.ToString(".##;-.##;0") + "\t" + mzPoint.Z.ToString(".##;-.##;0"));
                fp.WriteLine(modelscale.ToString(".##;-.##;0") + "\t" + mzPointRot.X.ToString(".##;-.##;0") + "\t" + mzPointRot.Y.ToString(".##;-.##;0") + "\t" + mzPointRot.Z.ToString(".##;-.##;0"));
                fp.WriteLine((enableCollision ? "1" : "0") + "\t" + (kinematic ? "1" : "0") + "\t" + mass);
                return true;
            }
            catch
            {
                MessageBox.Show("Couldn't write StaticModel...", "MazeMaker", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public virtual XmlElement toXMLnode(XmlDocument doc, Dictionary<string, string> cModels)
        {
            XmlElement staticModelNode = doc.CreateElement(string.Empty, "StaticModel", string.Empty);
            staticModelNode.SetAttribute("group", this.Group);
            staticModelNode.SetAttribute("label", this.Label);
            staticModelNode.SetAttribute("id", this.GetID().ToString());
            staticModelNode.SetAttribute("itemLocked", this.itemLocked.ToString());
            staticModelNode.SetAttribute("itemVisible", this.itemVisible.ToString());

            XmlElement mzPointnode = doc.CreateElement(string.Empty, "MzPoint", string.Empty);
            staticModelNode.AppendChild(mzPointnode);
            mzPointnode.SetAttribute("x", this.MzPoint.X.ToString());
            mzPointnode.SetAttribute("y", this.MzPoint.Y.ToString());
            mzPointnode.SetAttribute("z", this.MzPoint.Z.ToString());

            XmlElement modelNode = doc.CreateElement(string.Empty, "Model", string.Empty);
            staticModelNode.AppendChild(modelNode);
            //if(this.Model!=null)
            //{ 
            //    modelNode.SetAttribute("id", this.Model.Index.ToString());
            //}
            
            string modelID = "";
            if (cModels.ContainsKey(Model))
                modelID = cModels[Model];
            modelNode.SetAttribute("id", modelID);

            modelNode.SetAttribute("scale", this.ModelScale.ToString());
            modelNode.SetAttribute("rotX", this.MzPointRot.X.ToString());
            modelNode.SetAttribute("rotY", this.MzPointRot.Y.ToString());
            modelNode.SetAttribute("rotZ", this.MzPointRot.Z.ToString());

            XmlElement physicsNode = doc.CreateElement(string.Empty, "Physics", string.Empty);
            staticModelNode.AppendChild(physicsNode);
            physicsNode.SetAttribute("collision", this.Collision.ToString());
            physicsNode.SetAttribute("kinematic", this.Kinematic.ToString());
            physicsNode.SetAttribute("mass", this.Mass.ToString());

            return staticModelNode;
        }

        new public StaticModel Clone()
        {
            return Copy(true, 0);
        }

        public StaticModel Copy(bool clone,int offsetX=0, int offsetY=0)
        {
            StaticModel temp = new StaticModel(this.scale, this.Label, -1);

            temp.itemLocked = this.itemLocked;
            temp.itemVisible = this.itemVisible;

            //temp.ID = this.ID;
            temp.Collision = this.Collision;
            temp.Kinematic = this.Kinematic;
            temp.Mass = this.Mass;
            temp.Model = this.Model;
            temp.ModelScale = this.ModelScale;
            temp.MzPoint = new MPoint(this.mzPoint);
            temp.MzPointRot = new MPoint(this.MzPointRot);
            temp.MazeColorRegular = this.MazeColorRegular;
            temp.ScrPoint = new PointF(this.ScrPoint.X + offsetX, this.ScrPoint.Y + offsetY);

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
