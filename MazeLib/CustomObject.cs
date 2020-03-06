using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Data;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
namespace MazeMaker
{
    public class CustomObject
    {

        public enum CustomObjectType { Box, Pyramid, Sphere};

        private CustomObjectType type = CustomObjectType.Box;
        public bool changed = true;
        public bool justCreated = true;
        private BoxProperty bP = new BoxProperty();
        private SphereProperty sP = new SphereProperty();

        private CustomObjectProperty objectProperty = new CustomObjectProperty();
        [Category("Options")]
        [Description("Further Properties")]
        public CustomObjectProperty ObjectProperty
        {
            get { return objectProperty; }
            set { objectProperty = value; }
        }


        [Category("Options")]
        [Description("Pre-defined type of object")]
        public CustomObjectType Type
        {
            get { return type; }
            set 
            { 
                type = value;
                SetAttributes(type);           
            } 
        }

        public CustomObject(double dScale)
        {
            scale = dScale;
        }
        private bool selected = false;

        private double scale = 17;
        [Category("Options")]
        [Description("Coordinate transformation coefficient")]
        public double Scale
        {
            get { return scale; }
            set { scale = value; ConvertFromScreenCoordinates(); }
        }

        private Color pDisplay = Color.White;
        [Category("Colors")]
        [Description("The display color of the object in the Maze")]
        public Color Display
        {
            get { return pDisplay; }
            set { pDisplay = value; }
        }

        private Color colorCur = Color.CornflowerBlue;
        [Category("Colors")]
        [Description("The Current color in the editor")]
        public Color Regular
        {
            get { return colorCur; }
            set { colorCur = value; }
        }

        private Color colorSel = Color.DarkBlue;
        [Category("Colors")]
        [Description("The Selection Mode color")]
        public Color Selected
        {
            get { return colorSel; }
            set { colorSel = value; }
        }

        private RectangleF scrRect = new RectangleF(0, 0, 0, 0);
        [Category("Screen Coordinates")]
        [Description("Location on screen coordinates")]
        public RectangleF Rect
        {
            get { return scrRect; }
            set { scrRect = value; ConvertFromScreenCoordinates(); }
        }

        private MPoint location = new MPoint();
        [Category("Maze Coordinates")]
        [Description("Location in maze coordinates")]
        public MPoint Location
        {
            get { return location; }
            set { location = value; ConvertFromMazeCoordinates(); }
        }
        private MPoint length = new MPoint(1,1,1);
        [Category("Maze Coordinates")]
        [Description("Length in maze coordinates")]
        public MPoint Length
        {
            get { return length; }
            set { length = value; ConvertFromMazeCoordinates(); }
        }

        private int textureIndex = 0;
        [Category("Texture")]
        [Description("Index of texture in the texture list to associate with the object")]
        public int TextureIndex
        {
            get { return textureIndex; }
            set { textureIndex = value; }
        }

        private void ConvertFromScreenCoordinates()
        {
            length.X = scrRect.Width / scale;
            length.Y = 1;
            length.Z = scrRect.Height / scale;

            location.X = (scrRect.Left + scrRect.Width / 2) / scale;
            location.Y = -0.5;
            location.Z = (scrRect.Top + scrRect.Height / 2) / scale;

           // CalculateFromTextureCoordinates();
        }
        private void ConvertFromMazeCoordinates()
        {
            scrRect.X = (int)((location.X - length.X / 2) * scale);
            scrRect.Y = (int)((location.Z - length.Z / 2) * scale);
            scrRect.Width = (int)(length.X * scale);
            scrRect.Height = (int)(length.Z * scale);
        }

        public bool InRegion(int x1, int y1, int x2, int y2) //currently requires entire area to encompase selection
        {
            int iconTolerence = 15;
            x1 = x1 - iconTolerence; //top
            x2 = x2 + iconTolerence; //bottom
            y1 = y1 - iconTolerence; //left
            y2 = y2 + iconTolerence; //right

            float midX = scrRect.Left + (scrRect.Right - scrRect.Left) / 2;
            float midY = scrRect.Top + (scrRect.Bottom - scrRect.Top) / 2;

            if (x1 == x2 && y1 == y2) //single point select
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
                    return regCount >= 2;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool IfSelected(int x, int y)
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
        public void Select(bool inp)
        {
            selected = inp;
        }
        public bool IsSelected()
        {
            return selected;
        }
        public void Paint(ref Graphics gr)
        {
            Brush br;
            if (selected == false)
            {
                //br = new SolidBrush(colorCur);
                br = new HatchBrush(HatchStyle.Divot, Color.DimGray, colorCur);
            }
            else
            {
                //br = new SolidBrush(colorSel);
                br = new HatchBrush(HatchStyle.DashedVertical, Color.DodgerBlue, colorSel);
            }

            gr.FillRectangle(br, scrRect);
            br.Dispose();
            if (selected)
            {
                Pen a = new Pen(Brushes.Black, 3);
                //gr.DrawRectangle(a, scrRect);
                gr.DrawRectangle(a, scrRect.Left, scrRect.Top, scrRect.Width, scrRect.Height);
                a.Dispose();
            }
            else
            {
                //gr.DrawRectangle(Pens.Black, scrRect);
                gr.DrawRectangle(Pens.Black, scrRect.Left, scrRect.Top, scrRect.Width, scrRect.Height);
            }


        }

        public bool PrintToFile(ref StreamWriter fp)
        {
            try
            {
                switch (type)
                {
                    case CustomObjectType.Box:
                        BoxPrintToFile(ref fp);
                        break;
                    case CustomObjectType.Pyramid:
                        PyramidPrintToFile(ref fp);
                        break;
                    case CustomObjectType.Sphere:
                        SpherePrintToFile(ref fp);
                        break;
                }
                return true;
            }
            catch
            {
                MessageBox.Show("Couldn't write Object...", "MazeMaker", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }           
            
        }


        public bool BoxPrintToFile(ref StreamWriter fp)
        {
            fp.WriteLine("3\t5");
            double r, g, b;
            r = Display.R / 255.0;
            g = Display.G / 255.0;
            b = Display.B / 255.0;
            fp.WriteLine(textureIndex.ToString() + "\t" + r.ToString(".#;.#;0") + "\t" + g.ToString(".#;.#;0") + "\t" + b.ToString(".#;.#;0"));

            /*
            fp.WriteLine(this.FloorVertex1.X.ToString(".##;-.##;0") + "\t" + this.FloorVertex1.Y.ToString(".##;-.##;0") + "\t" + this.mzPoint1.X.ToString(".##;-.##;0") + "\t" + this.mzPoint1.Y.ToString(".##;-.##;0") + "\t" + this.mzPoint1.Z.ToString(".##;-.##;0"));
            fp.WriteLine(this.FloorVertex2.X.ToString(".##;-.##;0") + "\t" + this.FloorVertex2.Y.ToString(".##;-.##;0") + "\t" + this.mzPoint2.X.ToString(".##;-.##;0") + "\t" + this.mzPoint2.Y.ToString(".##;-.##;0") + "\t" + this.mzPoint2.Z.ToString(".##;-.##;0"));
            fp.WriteLine(this.FloorVertex3.X.ToString(".##;-.##;0") + "\t" + this.FloorVertex3.Y.ToString(".##;-.##;0") + "\t" + this.mzPoint3.X.ToString(".##;-.##;0") + "\t" + this.mzPoint3.Y.ToString(".##;-.##;0") + "\t" + this.mzPoint3.Z.ToString(".##;-.##;0"));
            fp.WriteLine(this.FloorVertex4.X.ToString(".##;-.##;0") + "\t" + this.FloorVertex4.Y.ToString(".##;-.##;0") + "\t" + this.mzPoint4.X.ToString(".##;-.##;0") + "\t" + this.mzPoint4.Y.ToString(".##;-.##;0") + "\t" + this.mzPoint4.Z.ToString(".##;-.##;0"));
            */
            return true;
        }
 

        public  bool PyramidPrintToFile(ref StreamWriter fp)
        {
            fp.WriteLine("3\t5");
            double r, g, b;
            r = Display.R / 255.0;
            g = Display.G / 255.0;
            b = Display.B / 255.0;
            fp.WriteLine(textureIndex.ToString() + "\t" + r.ToString(".#;.#;0") + "\t" + g.ToString(".#;.#;0") + "\t" + b.ToString(".#;.#;0"));

            return true;
        }
 


        public  bool SpherePrintToFile(ref StreamWriter fp)
        {
            fp.WriteLine("3\t5");
            double r, g, b;
            r = Display.R / 255.0;
            g = Display.G / 255.0;
            b = Display.B / 255.0;
            fp.WriteLine(textureIndex.ToString() + "\t" + r.ToString(".#;.#;0") + "\t" + g.ToString(".#;.#;0") + "\t" + b.ToString(".#;.#;0"));

            return true;
        }

        private void SetAttributes(CustomObjectType inp)
        {
            switch (type)
            {
                case CustomObjectType.Box:

                    break;
                case CustomObjectType.Pyramid:
                    break;
                case CustomObjectType.Sphere:
                    
                    break;
            } 
        }


        public class CustomObjectProperty
        {
            private double height = 2;
            [Category("Maze Options")]
            [Description("Height of the object")]
            public double Height
            {
                get { return height; }
                set { height = value; }
            }
        }

        public class BoxProperty : CustomObjectProperty
        {
            private double t = 0;
            [Category("Maze Options")]
            [Description("ee")]
            public double T
            {
                get { return t; }
                set { t = value; }
            }
        }
        public class SphereProperty : CustomObjectProperty
        {
            private double radius = 0;
            [Category("Maze Options")]
            [Description("bb")]
            public double Radius
            {
                get { return radius; }
                set { radius = value; }
            }
        }

        public CustomObject Clone()
        {
            return Copy(true, 0);
        }

        public CustomObject Copy(bool clone=false, int offset=0)
        {
            CustomObject temp = new CustomObject(this.scale);
            temp.Length = this.Length;
            temp.Location = this.Location;
            temp.ObjectProperty = this.ObjectProperty;
            temp.Rect = new RectangleF(this.Rect.X+offset, this.Rect.Y+offset, this.Rect.Width, this.Rect.Height);
            temp.Regular = this.Regular;
            temp.TextureIndex = this.TextureIndex;
            temp.Type = this.Type;
            temp.justCreated = this.justCreated;
            if(clone)
            { //   temp.CloneID(this.GetInternalID());
            }
            else
                temp.justCreated = true;
            //if (clone)
            

            return temp;
        }

   }

    

}
