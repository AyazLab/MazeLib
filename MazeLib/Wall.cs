#region Using directives

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Data;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using System.Runtime.Serialization;
using System.Xml;
#endregion

namespace MazeMaker
{
    [Serializable]  
    public class Wall : MazeItem
    {
        public Wall(double dScale, string label="", int id=0, bool generateNewID = false) //0 for new, -1 for temporary, other for set specific Id
        {
            scale = dScale;
            if (generateNewID)
                id = 0;

            this.SetID(id);
            mzPoint1.Y = 1;           
            mzPoint2.Y = -1; 
            mzPoint3.Y = -1;     
            mzPoint4.Y = 1;           
            this.Label = label;
            itemType = MazeItemType.Wall;
            OnPropertyChanged("Initialized");
        }

        public Wall()
        {

        }

        public Wall(XmlNode wallNode)
        {
            this.SetID(Tools.getIntFromAttribute(wallNode, "id", -1));
            this.Label = Tools.getStringFromAttribute(wallNode, "label","");
            XmlNode textureNode = wallNode.SelectSingleNode("Texture");
            this.texID = Tools.getIntFromAttribute(textureNode, "id", -999);
            this.Flip = Tools.getBoolFromAttribute(textureNode, "flip", false);

            this.Color = Tools.getColorFromNode(wallNode);
            this.MzPoint1 = Tools.getXYZfromNode(wallNode, 1);
            this.MzPoint2 = Tools.getXYZfromNode(wallNode, 2);
            this.MzPoint3 = Tools.getXYZfromNode(wallNode, 3);
            this.MzPoint4 = Tools.getXYZfromNode(wallNode, 4);
            this.Vertex1 = Tools.getTexCoordFromNode(wallNode, 1);
            this.Vertex2 = Tools.getTexCoordFromNode(wallNode, 2);
            this.Vertex3 = Tools.getTexCoordFromNode(wallNode, 3);
            this.Vertex4 = Tools.getTexCoordFromNode(wallNode, 4);

            XmlNode appearanceNode = wallNode.SelectSingleNode("Appearance");
            this.Visible = Tools.getBoolFromAttribute(appearanceNode,"visible",true);
            
            // Try to Guess
            tileSize = MazeMaker.Texture.GetTileSize(this.Vertex1, this.Vertex2, this.Vertex3, this.Vertex4, this.MzPoint1, this.MzPoint2, this.MzPoint3, this.MzPoint4);
            String mode = MazeMaker.Texture.GetMode(this.Vertex1, this.Vertex2, this.Vertex3, this.Vertex4);
            mappingIndex = MazeMaker.Texture.GetMappingIndex(this.Vertex1, this.Vertex2, this.Vertex3, this.Vertex4);
            aspectRatio = MazeMaker.Texture.GetAspectRatio(this.Vertex1, this.Vertex2, this.Vertex3, this.Vertex4, this.MzPoint1, this.MzPoint2, this.MzPoint3, this.MzPoint4);

            //Try to Read
            mappingIndex = Tools.getIntFromAttribute(textureNode, "rotation", mappingIndex);
            mode = Tools.getStringFromAttribute(textureNode, "mode", mode);
            tileSize = Tools.getDoubleFromAttribute(textureNode, "tileSize", tileSize);
            aspectRatio = Tools.getDoubleFromAttribute(textureNode, "aspectRatio", aspectRatio);

            this.AssignInitVals(mode, mappingIndex, tileSize, aspectRatio, this.flip);

            itemType = MazeItemType.Wall;
        }

        //private bool selected = false;

        private double scale = 17;
        [Browsable(false)]  
        [Category("4.Appearance")]        
        [Description("Coordinate transformation coefficient")]
        public double Scale
        {
            get { return scale; }
            set {
               // CalculateModifiedScaleMazeCoordinates(value);
                scale = value;
                ConvertFromMazeCoordinates();
                OnPropertyChanged("Scale");
            }
        }

        private bool visible = true;
        [Category("4.Appearance")]
        [Description("Toggles wall visibility in maze. If set to false, wall still will be present but not rendered")]
        public bool Visible
        {
            get { return visible; }
            set { visible = value; OnPropertyChanged("Visible"); }
        }

        public void CalculateModifiedScaleMazeCoordinates(double newScale)
        {
            if (newScale == 0)
                throw new Exception("Scale can not be zero!");
            if (scale != newScale)
            {
                double coef = (newScale / scale);
                mzPoint1.X *= coef;
                mzPoint1.Z *= coef;
                mzPoint2.X *= coef;
                mzPoint2.Z *= coef;
                mzPoint3.X *= coef;
                mzPoint3.Z *= coef;
                mzPoint4.X *= coef;
                mzPoint4.Z *= coef;
            }
        }

        public void SilentSetScale(double newScale)
        {
            scale = newScale;
            ConvertFromMazeCoordinates();
            OnPropertyChanged("Scale");
        }

        private float screenLength = 0;
        [Category("Location")]
        [Description("The length of the wall in pixels")]
        [ReadOnly(false)]
        [Browsable(false)]
        public float ScreenLength
        {
            get { return screenLength; }
            set { if (value == 0) throw new ArgumentException("Length can not be zero!"); screenLength = value; SetLength(); OnPropertyChanged("Location"); }
        }

        //private int textureIndex = 0;
        //[Category("Texture")]
        //[Description("Index of texture in the texture list to associate")]
        //public int TextureIndex
        //{
        //    get { return textureIndex; }
        //    set { textureIndex = value; }
        //}



        private Color pDisplay = Color.White;
        [Description("The display color of the wall in the Maze")]
        [Category("4.Appearance")]
        public Color Color
        {
            get { return pDisplay; }
            set { pDisplay = value; OnPropertyChanged("Color"); }
        }

        private Color mazeColorRegular = Color.Maroon;
        [Browsable(false)]
        [Category("Colors")]
        [Description("The Current color in the editor")]
        public Color MazeColorRegular
        {
            get { return mazeColorRegular; }
            set { mazeColorRegular = value;  }
        }

        private Color mazeColorSelected = Color.DarkBlue;
        [Browsable(false)]
        [Category("Colors")]
        [Description("The Selection Mode color")]
        public Color MazeColorSelected
        {
            get { return mazeColorSelected; }
            set { mazeColorSelected = value; }
        }

        private PointF scrPoint1 = new PointF(0,0);
        [Browsable(false)]
        [Category("Location")]
        [Description("The location of the 1st point on screen coordinates")]
        public PointF ScrPoint1
        {
            get { return scrPoint1; }
            set { scrPoint1 = value; ConvertFromScreenCoordinates();}
        }
        private PointF scrPoint2 = new PointF(0, 0);
        [Browsable(false)]
        [Category("Location")]
        [Description("The location of the 2nd point on screen coordinates")]
        public PointF ScrPoint2
        {
            get { return scrPoint2; }
            set { scrPoint2 = value; ConvertFromScreenCoordinates(); }
        }

        private MPoint mzPoint1 = new MPoint(0, 0);
        [Category("2.Maze Coordinates")]
        [Description("Vertex 1 on Maze coordinates")]
        public MPoint MzPoint1
        {
            get { return mzPoint1; }
            set { mzPoint1 = value; ConvertFromMazeCoordinates(); OnPropertyChanged("Vertex");}
        }
        private MPoint mzPoint2 = new MPoint(0, 0);
        [Category("2.Maze Coordinates")]
        [Description("Vertex 2 on Maze coordinates")]
        public MPoint MzPoint2
        {
            get { return mzPoint2; }
            set { mzPoint2 = value; ConvertFromMazeCoordinates(); OnPropertyChanged("Vertex");}
        }
        private MPoint mzPoint3 = new MPoint(0, 0);
        [Category("2.Maze Coordinates")]
        [Description("Vertex 3 on Maze coordinates")]
        public MPoint MzPoint3
        {
            get { return mzPoint3; }
            set { mzPoint3 = value; ConvertFromMazeCoordinates(); OnPropertyChanged("Vertex");}
        }
        private MPoint mzPoint4 = new MPoint(0, 0);
        [Category("2.Maze Coordinates")]
        [Description("Vertex 4 on Maze coordinates")]
        public MPoint MzPoint4
        {
            get { return mzPoint4; }
            set { mzPoint4 = value; ConvertFromMazeCoordinates(); OnPropertyChanged("Vertex");}
        }
        
        private TPoint textureVertex1 = new TPoint(1, 1);


        [Category("Texture Coordinates")]
        [Description("Vertex 1 of Texture Coordinates")]
        [Browsable(false)]
        public TPoint Vertex1
        {
            get { return textureVertex1; }
            set { textureVertex1 = value; OnPropertyChanged("Vertex");}
        }

        private TPoint textureVertex2 = new TPoint(1, 0);
        [Category("Texture Coordinates")]
        [Description("Vertex 2 of Texture Coordinates")]
        [Browsable(false)]
        public TPoint Vertex2
        {
            get { return textureVertex2; }
            set { textureVertex2 = value; OnPropertyChanged("Vertex"); }
        }
        private TPoint textureVertex3 = new TPoint(0, 0);
        [Category("Texture Coordinates")]
        [Description("Vertex 3 of Texture Coordinates")]
        [Browsable(false)]
        public TPoint Vertex3
        {
            get { return textureVertex3; }
            set { textureVertex3 = value; OnPropertyChanged("Vertex"); }
        }
        private TPoint textureVertex4 = new TPoint(0, 1);
        [Category("Texture Coordinates")]
        [Description("Vertex 4 of Texture Coordinates")]
        [Browsable(false)]
        public TPoint Vertex4
        {
            get { return textureVertex4; }
            set { textureVertex4 = value; OnPropertyChanged("Vertex"); }
        }

        private int mappingIndex = 3; //default from texture vertex initializations...
        [Category("Texture")]
        [Browsable(false)]
        [Description("Rotation in Degrees")]
        public int MappingIndex
        {
            get { return mappingIndex; }
            set
            {
                if (value < 1 || value > 4)
                {
                    throw new ArgumentException("Please enter a number between 1 and 4");
                }
                mappingIndex = value;
                CalculateFromTextureCoordinates();
                OnPropertyChanged("MappintIndex");

            }
        }

        

        private double tileSize = 1;
        [Category("3.Texture")]
        [Description("Size of tile in maze units")]
        public double TileSize
        {
            get{return tileSize;}
            set
            {
                if(value<=0)
                    throw new ArgumentException("TileSize must be greater than zero");
                tileSize=value;
                
                CalculateFromTextureCoordinates();
                OnPropertyChanged("TileSize");
            }
        }

        private double aspectRatio=1;
        [Category("3.Texture")]
        [Description("Aspect Ratio of Texture")]
        public double AspectRatio
        {
            get{return aspectRatio;}
            set
            {
                if (value<=0)
                    throw new ArgumentException("Aspect Ratio must be greater than zero");
                aspectRatio=value;
                CalculateFromTextureCoordinates();
                OnPropertyChanged("Vertex");
                
            }
        }

        private double mzTextureWidth = 1;
        [Category("3.Texture")]
        [Browsable(false)]
        [Description("Texture width to tile")]
        public double MzTextureWidth
        {
            get { return mzTextureWidth; }
            set 
            {
                textureVertex1.X /= mzTextureWidth;
                textureVertex2.X /= mzTextureWidth;
                textureVertex3.X /= mzTextureWidth;
                textureVertex4.X /= mzTextureWidth;
                mzTextureWidth = value;
                textureVertex1.X *= mzTextureWidth;
                textureVertex2.X *= mzTextureWidth;
                textureVertex3.X *= mzTextureWidth;
                textureVertex4.X *= mzTextureWidth;
                CalculateFromTextureCoordinates();
                OnPropertyChanged("MzTextureWidth");
            }
        }
        private double mzTextureHeight = 1;
        [Category("3.Texture")]
        [Browsable(false)]
        [Description("Texture height to tile")]
        public double MzTextureHeight
        {
            get { return mzTextureHeight; }
            set 
            {
                textureVertex1.Y /= mzTextureHeight;
                textureVertex2.Y /= mzTextureHeight;
                textureVertex3.Y /= mzTextureHeight;
                textureVertex4.Y /= mzTextureHeight;
                mzTextureHeight = value;
                textureVertex1.Y *= mzTextureHeight;
                textureVertex2.Y *= mzTextureHeight;
                textureVertex3.Y *= mzTextureHeight;
                textureVertex4.Y *= mzTextureHeight;               
                CalculateFromTextureCoordinates();
                OnPropertyChanged("MzTextureHeight");
            }
        }

        public int texID = -999;
        private string texture = "";
        [Category("3.Texture")]
        [Description("Select texture to be used with the wall. List can be edited at Texture Collection")]
        [TypeConverter(typeof(ImagePathConverter))]
        public string Texture
        {
            get { return texture; }
            set { texture = value; OnPropertyChanged("Texture"); }

            //set
            //{
            //    texture = value;
                
            //    if (value == null)
            //        texID = -999;
            //    else
            //        texID = texture.Index;
                    
            //    //if (texture.Image != null)
            //    //{
            //    //    double lenX = Math.Sqrt(Math.Pow(mzPoint1.X - mzPoint3.X, 2) + Math.Pow(mzPoint1.Y - mzPoint3.Y, 2) + Math.Pow(mzPoint1.Z - mzPoint3.Z, 2)) * 10;
            //    //    double lenY = Math.Sqrt(Math.Pow(mzPoint1.X - mzPoint2.X, 2) + Math.Pow(mzPoint1.Y - mzPoint2.Y, 2) + Math.Pow(mzPoint1.Z - mzPoint2.Z, 2)) * 10;
            //    //    MzTextureWidth = texture.Image.Width / lenX;
            //    //    MzTextureHeight = texture.Image.Height / lenY;
            //    //}

            //    OnPropertyChanged("Texture");
            //}
        }

        private bool flip = false;
        [Category("3.Texture")]
        public bool Flip
        {
            get
            {
                return flip;
            }
            set
            {
                flip = value;
                CalculateFromTextureCoordinates();
                OnPropertyChanged("Flip");
            }
        }

        //[TypeConverter(typeof(RotationConverter))]
        [TypeConverterAttribute(typeof(RotationConverter)), DescriptionAttribute("Identify the orientation of the texture image to be wrapped to the maze item")]
        [Category("3.Texture")]
       // [Description("Identify the orientation of the texture image to be wrapped to the maze item")]
        public string Rotation{
            get
            {
                if(mappingIndex==3)
                    return "0°";
                else if (mappingIndex == 4)
                    return "90°";
                else if (mappingIndex == 1)
                    return "180°";
                else if (mappingIndex == 2)
                    return "270°";
                else return "Error";
            }
            set
            {
                if (value == "180°")
                    mappingIndex=1;
                if (value == "270°")
                    mappingIndex = 2;
                if (value == "0°")
                    mappingIndex = 3;
                if (value == "90°")
                    mappingIndex = 4;
                CalculateFromTextureCoordinates();
                OnPropertyChanged("Rotation");
            }
        }

        public void AssignInitVals(string mode,int mIndex,double tSize,double aspect, bool flipped=false)
        {
            if (mode == "Stretch")
                bStretched = true;
            if (mode == "Tile")
                bStretched = false;
            mappingIndex=mIndex;

            this.flip = flipped;

            if (!bStretched) //discards aspect ratio and tile size if stretched
            {
                aspectRatio = aspect;
                tileSize = tSize;
            }
            CalculateFromTextureCoordinates(); //recalculates texture coordinates based on rotations, aspect, tile size etc.
        }



        private bool bStretched = true;
        [Category("3.Texture")]
        [Browsable(false)]
        [Description("If true, stretch the texture across the wall")]
        public bool Stretch
        {
            get { return bStretched; }
            set { bStretched = value;
                OnPropertyChanged("Stretched");
            }
        }

        public enum ModeType
        {
            Stretch = 0,
            Tile = 1,
        }

        //[TypeConverter(typeof(StretchConverter))]
        //[Category("Texture")]
        //public string Mode
        //{
        //    get
        //    {
        //        if (bStretched)
        //            return "Stretch";
        //        else return "Tile";
        //    }
        //    set
        //    {
        //        if (value == "Stretch")
        //            bStretched=true;
        //        if (value == "Tile")
        //            bStretched = false;
        //        CalculateFromTextureCoordinates();
        //    }
        //}

        [Category("3.Texture")]
        [Description("Identify how texture image is wrapped to the maze item")]
        public ModeType Mode
        {
            get
            {
                if (bStretched)
                    return ModeType.Stretch;
                else return ModeType.Tile;
            }
            set
            {
                if (value == ModeType.Stretch)
                    bStretched = true;
                else if (value == ModeType.Tile)
                    bStretched = false;
                CalculateFromTextureCoordinates();
                OnPropertyChanged("TextureMode");
            }
        }

        public override void Paint(ref Graphics gr)
        {
            Pen p;
            Brush br;

            if (selected == true)
            {
                br = new SolidBrush(mazeColorSelected);
                p = new Pen(br, 11);
                gr.DrawLine(p, scrPoint1, scrPoint2);
                p.Dispose();
                br.Dispose();
            }
            br = new SolidBrush(mazeColorRegular);
            p = new Pen(br, 5);
            gr.DrawLine(p, scrPoint1, scrPoint2);
            p.Dispose();
            br.Dispose();
          
        }
        public virtual bool IfSelected(int x, int y)
        {
            float tolerence = 10;
            PointF curPoint = new Point(x, y);
            float distFromSegment=Tools.LineToPointDistance2D(scrPoint1, scrPoint2, curPoint,true);

            if (Math.Abs(distFromSegment) > tolerence)
            {
                return false;
            }
            else
                return true;

            //PointF vector1 = new PointF(scrPoint1.X- x,scrPoint1.Y- y);
            //PointF vector2 = new PointF(scrPoint2.X - x, scrPoint2.Y - y);

            //double dotproduct = (vector1.X * vector2.X + vector1.Y * vector2.Y);
            //double abs1_sqr = vector1.X * vector1.X + vector1.Y * vector1.Y;
            //double abs2_sqr = vector2.X * vector2.X + vector2.Y * vector2.Y;
            
            //if ( (dotproduct / (Math.Sqrt(abs1_sqr) * Math.Sqrt(abs2_sqr))) < -0.97 )
            //{
            //    return true;
            //}
            //else
            //{
            //    return false;
            //}
        }
        //public virtual bool IfSelected(int x, int y, ref double value)
        //{
        //    PointF vector1 = new PointF(scrPoint1.X - x, scrPoint1.Y - y);
        //    PointF vector2 = new PointF(scrPoint2.X - x, scrPoint2.Y - y);

        //    double dotproduct = (vector1.X * vector2.X + vector1.Y * vector2.Y);
        //    double abs1_sqr = vector1.X * vector1.X + vector1.Y * vector1.Y;
        //    double abs2_sqr = vector2.X * vector2.X + vector2.Y * vector2.Y;

        //    value = (dotproduct / (Math.Sqrt(abs1_sqr) * Math.Sqrt(abs2_sqr)));
        //    if ( value < -0.97)
        //    {
        //        return true;
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}

        public virtual bool CheckClosePoints(int x,int y,ref PointF p)
        {
            if ((scrPoint1.X - x) * (scrPoint1.X - x) + (scrPoint1.Y - y) * (scrPoint1.Y - y)< 180)
            {
                p = scrPoint1;
                return true;
            }
            if ((scrPoint2.X - x) * (scrPoint2.X - x) + (scrPoint2.Y - y) * (scrPoint2.Y - y) < 180)
            {
                p= scrPoint2;
                return true;
            }
            return false;
        }



        public virtual bool InRegion(int x1, int y1, int x2, int y2) //currently requires entire area to encompass selection
        {
            if(x1==x2&&y1==y2)
            {
                //double value = 0;
                return this.IfSelected(x1, y1);
                
            }

            int iconTolerence = 0;
            x1 = x1 - iconTolerence; //top
            x2 = x2 + iconTolerence; //bottom
            y1 = y1 - iconTolerence; //left
            y2 = y2 + iconTolerence; //right
            
            float midX = scrPoint1.X + (scrPoint2.X - scrPoint1.X) / 2;
            float midY = scrPoint1.Y + (scrPoint2.Y - scrPoint1.Y) / 2;

            int regCount = 0;

            if (x1 < midX && x2 > midX && y1 < midY && y2 > midY) //must entirely enclose two points + center
            {
                regCount++;
            }
            if (x1 < scrPoint1.X && x2 > scrPoint1.X && y1 < scrPoint1.Y && y2 > scrPoint1.Y) //must entirely enclose two points + center
            {
                regCount++;
            }
            if (x1 < scrPoint2.X && x2 > scrPoint2.X && y1 < scrPoint2.Y && y2 > scrPoint2.Y) //must entirely enclose two points + center
            {
                regCount++;
            }

            return regCount >= 2;
        }

        private void ConvertFromScreenCoordinates()
        {
            mzPoint1.X = scrPoint1.X / scale;
           // mzPoint1.Y = 1;
            mzPoint1.Z = scrPoint1.Y / scale;

            mzPoint2.X = scrPoint1.X / scale;
           // mzPoint2.Y = -1;
            mzPoint2.Z = scrPoint1.Y / scale;

            mzPoint3.X = scrPoint2.X / scale;
           // mzPoint3.Y = -1;
            mzPoint3.Z = scrPoint2.Y / scale;

            mzPoint4.X = scrPoint2.X / scale;
           // mzPoint4.Y = 1;
            mzPoint4.Z = scrPoint2.Y / scale;

            ReMeasure();
            CalculateFromTextureCoordinates();
        }
        public void ConvertFromMazeCoordinates()
        {
            scrPoint1.X = (float)((mzPoint1.X + MzPoint2.X) / 2.0 * scale);
            scrPoint1.Y = (float)((mzPoint1.Z + mzPoint2.Z) / 2.0 * scale);

            scrPoint2.X = (float)((mzPoint3.X + MzPoint4.X) / 2.0 * scale);
            scrPoint2.Y = (float)((mzPoint3.Z + mzPoint4.Z) / 2.0 * scale);

            if (scrPoint1 == ScrPoint2)
            {
                //Fix for old maze versions...
                MPoint temp;
                temp = MzPoint4;
                mzPoint4 = mzPoint2;
                mzPoint2 = temp;

                TPoint temp2;
                temp2 = Vertex4;
                Vertex4 = Vertex2;
                Vertex2 = temp2;

                scrPoint1.X = (float)((mzPoint1.X + MzPoint2.X) / 2.0 * scale);
                scrPoint1.Y = (float)((mzPoint1.Z + mzPoint2.Z) / 2.0 * scale);

                scrPoint2.X = (float)((mzPoint3.X + MzPoint4.X) / 2.0 * scale);
                scrPoint2.Y = (float)((mzPoint3.Z + mzPoint4.Z) / 2.0 * scale);
                if (scrPoint1 == scrPoint2)
                {
                    scrPoint2.X += 1;
                    scrPoint2.Y += 1;
                }
                ReMeasure();
            }
        }
      

        private void CalculateFromTextureCoordinates()
        {
            Vertex1.X = 1;
            Vertex1.Y = 1;
            Vertex2.X = 1;
            Vertex2.Y = 0;
            Vertex3.X = 0;
            Vertex3.Y = 0;
            Vertex4.X = 0;
            Vertex4.Y = 1;


            TPoint[] buf = new TPoint[4];
            buf[0] = new TPoint(textureVertex1);
            buf[1] = new TPoint(textureVertex2);
            buf[2] = new TPoint(textureVertex3);
            buf[3] = new TPoint(textureVertex4);
            int index = 2;
            if (flip == false)
            {
                switch (mappingIndex)
                {
                    case 1:
                        textureVertex1 = buf[index];
                        textureVertex2 = buf[(index + 1) % 4];
                        textureVertex3 = buf[(index + 2) % 4];
                        textureVertex4 = buf[(index + 3) % 4];
                        break;
                    case 2:
                        textureVertex2 = buf[index];
                        textureVertex3 = buf[(index + 1) % 4];
                        textureVertex4 = buf[(index + 2) % 4];
                        textureVertex1 = buf[(index + 3) % 4];
                        break;
                    case 3: //270
                        textureVertex3 = buf[index];
                        textureVertex4 = buf[(index + 1) % 4];
                        textureVertex1 = buf[(index + 2) % 4];
                        textureVertex2 = buf[(index + 3) % 4];
                        break;
                    case 4: //90
                        textureVertex4 = buf[index];
                        textureVertex1 = buf[(index + 1) % 4];
                        textureVertex2 = buf[(index + 2) % 4];
                        textureVertex3 = buf[(index + 3) % 4];
                        break;
                }
            }
            else
            {
                switch (mappingIndex)
                {
                    case 1:
                        textureVertex1 = buf[(index + 3) % 4];
                        textureVertex2 = buf[(index + 2) % 4];
                        textureVertex3 = buf[(index + 1) % 4];
                        textureVertex4 = buf[index];
                        break;
                    case 2:
                        textureVertex2 = buf[(index + 3) % 4];
                        textureVertex3 = buf[(index + 2) % 4];
                        textureVertex4 = buf[(index + 1) % 4];
                        textureVertex1 = buf[index];
                        break;
                    case 3:
                        textureVertex3 = buf[(index + 3) % 4];
                        textureVertex4 = buf[(index + 2) % 4];
                        textureVertex1 = buf[(index + 1) % 4];
                        textureVertex2 = buf[index];
                        break;
                    case 4:
                        textureVertex4 = buf[(index + 3) % 4];
                        textureVertex1 = buf[(index + 2) % 4];
                        textureVertex2 = buf[(index + 1) % 4];
                        textureVertex3 = buf[index];
                        break;
                }
            }

            if (!bStretched)
            {
                double l = (Math.Sqrt(Math.Pow((MzPoint1.X - MzPoint4.X), 2) + Math.Pow((MzPoint1.Y - MzPoint4.Y), 2) + Math.Pow((MzPoint1.Z - MzPoint4.Z), 2)) + Math.Sqrt(Math.Pow((MzPoint2.X - MzPoint3.X), 2) + Math.Pow((MzPoint2.Y - MzPoint3.Y), 2) + Math.Pow((MzPoint2.Z - MzPoint3.Z), 2))) / 2;
                double h = (Math.Sqrt(Math.Pow((MzPoint1.X - MzPoint2.X), 2) + Math.Pow((MzPoint1.Y - MzPoint2.Y), 2) + Math.Pow((MzPoint1.Z - MzPoint2.Z), 2)) + Math.Sqrt(Math.Pow((MzPoint3.X - MzPoint4.X), 2) + Math.Pow((MzPoint3.Y - MzPoint4.Y), 2) + Math.Pow((MzPoint3.Z - MzPoint4.Z), 2))) / 2;
                textureVertex1.X = textureVertex1.X * l / (tileSize * aspectRatio);
                textureVertex1.Y = textureVertex1.Y * h / tileSize;
                textureVertex2.X = textureVertex2.X * l / (tileSize * aspectRatio);
                textureVertex2.Y = textureVertex2.Y * h / tileSize;
                textureVertex3.X = textureVertex3.X * l / (tileSize * aspectRatio);
                textureVertex3.Y = textureVertex3.Y * h / tileSize;
                textureVertex4.X = textureVertex4.X * l / (tileSize * aspectRatio);
                textureVertex4.Y = textureVertex4.Y * h / tileSize;
            }

        }

        private void ReMeasure()
        {
            screenLength = (float) Math.Sqrt(Math.Pow(scrPoint1.X - scrPoint2.X, 2) + Math.Pow(scrPoint1.Y - scrPoint2.Y, 2));
        }

        public override void Move(float mzXdir, float mzZdir)
        {
            mzPoint1.X += mzXdir;
            mzPoint1.Z += mzZdir;
            mzPoint2.X += mzXdir;
            mzPoint2.Z += mzZdir;
            mzPoint3.X += mzXdir;
            mzPoint3.Z += mzZdir;
            mzPoint4.X += mzXdir;
            mzPoint4.Z += mzZdir;

            ConvertFromMazeCoordinates();
        }

        public override void Rescale(double factor)
        {
            mzPoint1.X *= factor;
            mzPoint1.Z *= factor;
            mzPoint2.X *= factor;
            mzPoint2.Z *= factor;
            mzPoint3.X *= factor;
            mzPoint3.Z *= factor;
            mzPoint4.X *= factor;
            mzPoint4.Z *= factor;

            ConvertFromMazeCoordinates();
        }

        public override void RescaleXYZ(double scaleX,double scaleY,double scaleZ)
        {
            mzPoint1.X *= scaleX;
            mzPoint1.Y *= scaleY;
            mzPoint1.Z *= scaleZ;
            mzPoint2.X *= scaleX;
            mzPoint2.Y *= scaleY;
            mzPoint2.Z *= scaleZ;
            mzPoint3.X *= scaleX;
            mzPoint3.Y *= scaleY;
            mzPoint3.Z *= scaleZ;
            mzPoint4.X *= scaleX;
            mzPoint4.Y *= scaleY;
            mzPoint4.Z *= scaleZ;

            ConvertFromMazeCoordinates();
        }
        
        private void SetLength()
        {
            double prevLength = (float) Math.Sqrt(Math.Pow(scrPoint1.X - scrPoint2.X, 2) + Math.Pow(scrPoint1.Y - scrPoint2.Y, 2));
            double factor = screenLength / prevLength;
            scrPoint2.X = scrPoint1.X + (int)((scrPoint2.X - scrPoint1.X) * factor);
            scrPoint2.Y = scrPoint1.Y + (int)((scrPoint2.Y - scrPoint1.Y) * factor);

            ConvertFromScreenCoordinates();
        }

        public virtual string PrintToTreeItem()
        {
            string str = this.ID;

            if (string.IsNullOrWhiteSpace(this.Label) == false)
                    str += " [" + this.Label + "]";

            //if (this.Texture != null)
            if (this.Texture != "")
            {
                //str += "(" + this.Texture.Name + ")";
                str += "(" + this.Texture + ")";
            }

            return str;
        }

        public virtual bool PrintToFile(ref StreamWriter fp, Dictionary<string, string> cImages)
        {
            try
            {
                fp.WriteLine("0\t6\t" + this.GetID().ToString() + "\t" + this.Label);
                double r, g, b;
                r = this.Color.R / 255.0;
                g = this.Color.G / 255.0;
                b = this.Color.B / 255.0;

                //int textureIndex = 0;
                //if (texture != null) textureIndex = texture.Index;
                //fp.WriteLine( textureIndex.ToString() + "\t" + r.ToString(".#;.#;0") + "\t" + g.ToString(".#;.#;0") + "\t" + b.ToString(".#;.#;0"));
                string imageID = "0";
                if (cImages.ContainsKey(texture))
                    imageID = cImages[texture];
                fp.WriteLine(imageID + "\t" + r.ToString(".#;.#;0") + "\t" + g.ToString(".#;.#;0") + "\t" + b.ToString(".#;.#;0"));

                fp.WriteLine(this.textureVertex1.X.ToString(".##;-.##;0") + "\t" + this.textureVertex1.Y.ToString(".##;-.##;0") + "\t" + this.mzPoint1.X.ToString(".##;-.##;0") + "\t" + this.mzPoint1.Y.ToString(".##;-.##;0") + "\t" + this.mzPoint1.Z.ToString(".##;-.##;0"));
                fp.WriteLine(this.textureVertex2.X.ToString(".##;-.##;0") + "\t" + this.textureVertex2.Y.ToString(".##;-.##;0") + "\t" + this.mzPoint2.X.ToString(".##;-.##;0") + "\t" + this.mzPoint2.Y.ToString(".##;-.##;0") + "\t" + this.mzPoint2.Z.ToString(".##;-.##;0"));
                fp.WriteLine(this.textureVertex3.X.ToString(".##;-.##;0") + "\t" + this.textureVertex3.Y.ToString(".##;-.##;0") + "\t" + this.mzPoint3.X.ToString(".##;-.##;0") + "\t" + this.mzPoint3.Y.ToString(".##;-.##;0") + "\t" + this.mzPoint3.Z.ToString(".##;-.##;0"));
                fp.WriteLine(this.textureVertex4.X.ToString(".##;-.##;0") + "\t" + this.textureVertex4.Y.ToString(".##;-.##;0") + "\t" + this.mzPoint4.X.ToString(".##;-.##;0") + "\t" + this.mzPoint4.Y.ToString(".##;-.##;0") + "\t" + this.mzPoint4.Z.ToString(".##;-.##;0"));
                fp.WriteLine("0\t0\t" + (visible ? "0" : "-1") +"\t" + (flip?"1":"0") + "\t0");
                return true;
            }
            catch
            {
                MessageBox.Show("Couldn't write Wall...", "MazeMaker", MessageBoxButtons.OK , MessageBoxIcon.Error);
                return false;
            }
        }

        public virtual XmlElement toXMLnode(XmlDocument doc, Dictionary<string, string> cImages)
        {
            XmlElement wallNode = doc.CreateElement(string.Empty, "Wall", string.Empty);
            wallNode.SetAttribute("label", this.Label);
            wallNode.SetAttribute("id", this.GetID().ToString());

            XmlElement mzPoint1node = doc.CreateElement(string.Empty, "MzPoint1", string.Empty);
            wallNode.AppendChild(mzPoint1node);
            mzPoint1node.SetAttribute("x", this.MzPoint1.X.ToString());
            mzPoint1node.SetAttribute("y", this.MzPoint1.Y.ToString());
            mzPoint1node.SetAttribute("z", this.MzPoint1.Z.ToString());
            mzPoint1node.SetAttribute("texX", this.textureVertex1.X.ToString());
            mzPoint1node.SetAttribute("texY", this.textureVertex1.Y.ToString());

            XmlElement mzPoint2node = doc.CreateElement(string.Empty, "MzPoint2", string.Empty);
            wallNode.AppendChild(mzPoint2node);
            mzPoint2node.SetAttribute("x", this.MzPoint2.X.ToString());
            mzPoint2node.SetAttribute("y", this.MzPoint2.Y.ToString());
            mzPoint2node.SetAttribute("z", this.MzPoint2.Z.ToString());
            mzPoint2node.SetAttribute("texX", this.textureVertex2.X.ToString());
            mzPoint2node.SetAttribute("texY", this.textureVertex2.Y.ToString());

            XmlElement mzPoint3node = doc.CreateElement(string.Empty, "MzPoint3", string.Empty);
            wallNode.AppendChild(mzPoint3node);
            mzPoint3node.SetAttribute("x", this.MzPoint3.X.ToString());
            mzPoint3node.SetAttribute("y", this.MzPoint3.Y.ToString());
            mzPoint3node.SetAttribute("z", this.MzPoint3.Z.ToString());
            mzPoint3node.SetAttribute("texX", this.textureVertex3.X.ToString());
            mzPoint3node.SetAttribute("texY", this.textureVertex3.Y.ToString());

            XmlElement mzPoint4node = doc.CreateElement(string.Empty, "MzPoint4", string.Empty);
            wallNode.AppendChild(mzPoint4node);
            mzPoint4node.SetAttribute("x", this.MzPoint4.X.ToString());
            mzPoint4node.SetAttribute("y", this.MzPoint4.Y.ToString());
            mzPoint4node.SetAttribute("z", this.MzPoint4.Z.ToString());
            mzPoint4node.SetAttribute("texX", this.textureVertex4.X.ToString());
            mzPoint4node.SetAttribute("texY", this.textureVertex4.Y.ToString());

            if (this.Texture != null)
            {
                XmlElement textureNode = doc.CreateElement(string.Empty, "Texture", string.Empty);
                wallNode.AppendChild(textureNode);
                string imageID = "";
                if (cImages.ContainsKey(Texture))
                    imageID = cImages[Texture];
                textureNode.SetAttribute("id", imageID);
                textureNode.SetAttribute("aspectRatio", this.AspectRatio.ToString());
                textureNode.SetAttribute("flip", this.Flip.ToString());
                textureNode.SetAttribute("mode", this.Mode.ToString());
                textureNode.SetAttribute("rotation", this.mappingIndex.ToString());
                textureNode.SetAttribute("tileSize", this.TileSize.ToString());
            }

            XmlElement colorNode = doc.CreateElement(string.Empty, "Color", string.Empty);
            wallNode.AppendChild(colorNode);
            colorNode.SetAttribute("r", ((float)this.Color.R / 255).ToString());
            colorNode.SetAttribute("g", ((float)this.Color.G / 255).ToString());
            colorNode.SetAttribute("b", ((float)this.Color.B / 255).ToString());

            XmlElement appearanceNode = doc.CreateElement(string.Empty, "Appearance", string.Empty);
            wallNode.AppendChild(appearanceNode);
            appearanceNode.SetAttribute("visible", this.Visible.ToString());

            return wallNode;
        }

        new public Wall Clone()
        {
            return Copy(true,0);
        }

        public virtual PointF CalcMidPoint()
        {
            return new PointF((float)(this.ScrPoint1.X + this.ScrPoint2.X) / 2, (float)(this.ScrPoint1.Y + this.ScrPoint2.Y) / 2);
        }

        public override void Rotate(float degrees,float centerX=0,float centerY=0)
        {
            PointF midPoint;
            if(centerX!=0&&centerY!=0)
                midPoint=new PointF(centerX,centerY);
            else
                midPoint= CalcMidPoint();


            this.ScrPoint1 = Tools.RotatePoint(this.ScrPoint1, midPoint, degrees);
            this.ScrPoint2 = Tools.RotatePoint(this.ScrPoint2, midPoint, degrees);        
        }

        public virtual Wall Copy(bool clone, int offsetX=0, int offsetY=0)
        {
            Wall temp = new Wall(this.scale, this.Label, -1);
            temp.Vertex1 = new TPoint(this.Vertex1);
            temp.Vertex2 = new TPoint(this.Vertex2);
            temp.Vertex3 = new TPoint(this.Vertex3);
            temp.Vertex4 = new TPoint(this.Vertex4);

            
            temp.aspectRatio = this.aspectRatio;
            temp.Color = this.Color;
            temp.flip = this.flip;
            temp.mappingIndex = this.mappingIndex;
            
            temp.MzPoint1= new MPoint(this.MzPoint1);
            temp.MzPoint2 = new MPoint(this.MzPoint2);
            temp.MzPoint3 = new MPoint(this.MzPoint3);
            temp.MzPoint4 = new MPoint(this.MzPoint4);
            temp.mzTextureHeight = this.mzTextureHeight;
            temp.mzTextureWidth = this.mzTextureWidth;
            temp.MazeColorRegular = this.MazeColorRegular;
            
            temp.bStretched = this.bStretched;
            temp.Texture = this.Texture;
            temp.tileSize = this.tileSize;
            temp.Mode = this.Mode;
            temp.Rotation = this.Rotation;
            temp.Visible = this.Visible;
            temp.justCreated = this.justCreated;
            temp.ScrPoint1 = new PointF(this.ScrPoint1.X + offsetX, this.ScrPoint1.Y + offsetY);
            temp.ScrPoint2 = new PointF(this.ScrPoint2.X + offsetX, this.ScrPoint2.Y + offsetY);

            if (clone)
            {
                temp.SetID(this.GetID(),true);
            }
            else
            {
                temp.justCreated = true;
                temp.SetID();
            }

            return temp;

        }

        public void UpdateAfterLoading()
        {
            ConvertFromMazeCoordinates();
        }

    }
}
