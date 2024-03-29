﻿#region Using directives

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
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
    public class CurvedWall : Wall
    {
        public CurvedWall(double dScale, string label="", int id=0, bool generateNewID = false) //0 for new, -1 for temporary, other for set specific Id
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
            
            OnPropertyChanged("Initialized");
            itemType = MazeItemType.CurvedWall;
        }

        public CurvedWall(XmlNode wallNode)
        {
            this.SetID(Tools.getIntFromAttribute(wallNode, "id", -1));
            this.Group = Tools.getStringFromAttribute(wallNode, "group", "");
            this.Label = Tools.getStringFromAttribute(wallNode, "label","");

            this.itemLocked = Tools.getBoolFromAttribute(wallNode, "itemLocked", false);
            this.itemVisible = Tools.getBoolFromAttribute(wallNode, "itemVisible", true);

            XmlNode textureNode = wallNode.SelectSingleNode("Texture");
            this.texID = Tools.getIntFromAttribute(textureNode, "id", -999);
            this.Flip = Tools.getBoolFromAttribute(textureNode, "flip", false);

            this.Color = Tools.getColorFromNode(wallNode);

            XmlNode centerPointNode = wallNode.SelectSingleNode("MzPointCenter");
            
            this.angleBegin = Tools.getDoubleFromAttribute(centerPointNode, "angleStart", 0);
            this.angleEnd = Tools.getDoubleFromAttribute(centerPointNode, "angleEnd", 0);
            this.mzPointCenter = Tools.getXYZfromNode(wallNode, 0, "MzPointCenter");


            this.mzPoint1 = Tools.getXYZfromNode(wallNode, 1);
            this.mzPoint2 = Tools.getXYZfromNode(wallNode, 2);
            this.mzPoint3 = Tools.getXYZfromNode(wallNode, 3);
            this.mzPoint4 = Tools.getXYZfromNode(wallNode, 4);
            this.circleRadius = (float)Tools.getDoubleFromAttribute(centerPointNode, "radius", 0);

            ConvertFromMazeCoordinates();
            
            this.Vertex1 = Tools.getTexCoordFromNode(wallNode, 1);
            this.Vertex2 = Tools.getTexCoordFromNode(wallNode, 2);
            this.Vertex3 = Tools.getTexCoordFromNode(wallNode, 3);
            this.Vertex4 = Tools.getTexCoordFromNode(wallNode, 4);

            XmlNode appearanceNode = wallNode.SelectSingleNode("Appearance");
            this.Visible = Tools.getBoolFromAttribute(appearanceNode,"visible",true);
            
            tileSize = MazeMaker.Texture.GetTileSize(this.Vertex1, this.Vertex2, this.Vertex3, this.Vertex4, this.MzPoint1, this.MzPoint2, this.MzPoint3, this.MzPoint4);
            String mode = MazeMaker.Texture.GetMode(this.Vertex1, this.Vertex2, this.Vertex3, this.Vertex4);
            mappingIndex = MazeMaker.Texture.GetMappingIndex(this.Vertex1, this.Vertex2, this.Vertex3, this.Vertex4);
            aspectRatio = MazeMaker.Texture.GetAspectRatio(this.Vertex1, this.Vertex2, this.Vertex3, this.Vertex4, this.MzPoint1, this.MzPoint2, this.MzPoint3, this.MzPoint4);

            //Try to +
            mappingIndex = Tools.getIntFromAttribute(textureNode, "rotation", mappingIndex);
            mode = Tools.getStringFromAttribute(textureNode, "mode", mode);
            tileSize = Tools.getDoubleFromAttribute(textureNode, "tileSize", tileSize);
            aspectRatio = Tools.getDoubleFromAttribute(textureNode, "aspectRatio", aspectRatio);

            this.AssignInitVals(mode, mappingIndex, tileSize, aspectRatio, this.flip);

            itemType = MazeItemType.CurvedWall;
        }

        //private bool selected = false;
        

        private double scale = 17;
        [Browsable(false)]  
        [Category("4.Appearance")]        
        [Description("Coordinate transformation coefficient")]
        public new double Scale
        {
            
            get { return scale; }
            set {
                //CalculateModifiedScaleMazeCoordinates(value);
                scale = value;
                ConvertFromMazeCoordinates();
                OnPropertyChanged("Scale");
            }
        }

        private bool visible = true;
        [Category("4.Appearance")]
        [Description("Toggles wall visibility in maze. If set to false, wall still will be present but not rendered")]
        public new bool Visible
        {
            get { return visible; }
            set { visible = value; OnPropertyChanged("Visible"); }
        }

        double angularResolution = 1.0f; //one plane per degree
        [Category("4.Appearance")]
        [Description("Number of degrees in each plane generated by the arc. Reduce to increase performance")]
        public double AngularResolution
        {
            get { return angularResolution; }
            set { angularResolution = value;
                if (angularResolution < 0.5)
                    angularResolution = 0.5;
                    OnPropertyChanged("angularResolution"); }
        }

        //public new void CalculateModifiedScaleMazeCoordinates(double newScale)
        //{
        //    if (newScale == 0)
        //        throw new Exception("Scale can not be zero!");
        //    if (scale != newScale)
        //    {
        //        double coef = (newScale / scale);
        //        mzPoint1.X *= coef;
        //        mzPoint1.Z *= coef;
        //        mzPoint2.X *= coef;
        //        mzPoint2.Z *= coef;
        //        mzPoint3.X *= coef;
        //        mzPoint3.Z *= coef;
        //        mzPoint4.X *= coef;
        //        mzPoint4.Z *= coef;
        //    }
        //}

        //public void SilentSetScale(double newScale)
        //{
        //    scale = newScale;
        //    ConvertFromMazeCoordinates();
        //    OnPropertyChanged("Scale");
        //}

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
        public new Color Color
        {
            get { return pDisplay; }
            set { pDisplay = value; OnPropertyChanged("Color"); }
        }

        private Color mazeColorRegular = Color.Maroon;
        [Browsable(false)]
        [Category("Colors")]
        [Description("The Current color in the editor")]
        public new Color MazeColorRegular
        {
            get { return mazeColorRegular; }
            set { mazeColorRegular = value;  }
        }

        private Color mazeColorSelected = Color.DarkBlue;
        [Browsable(false)]
        [Category("Colors")]
        [Description("The Selection Mode color")]
        public new Color MazeColorSelected
        {
            get { return mazeColorSelected; }
            set { mazeColorSelected = value;  }
        }
        private PointF[] scrPoints = new PointF[3];
        private PointF scrPoint1 = new PointF(0,0);
        [Browsable(false)]
        [Category("Location")]
        [Description("The location of the 1st point on screen coordinates")]
        public new  PointF ScrPoint1
        {
            get { return scrPoint1; }
            set { scrPoint1 = value; ConvertFromScreenCoordinates();}
        }
        private PointF scrPoint2 = new PointF(0, 0);
        [Browsable(false)]
        [Category("Location")]
        [Description("The location of the 2nd point on screen coordinates")]
        public new PointF ScrPoint2
        {
            get { return scrPoint2; }
            set { scrPoint2 = value; ConvertFromScreenCoordinates(); }
        }

        private PointF scrPointMid = new PointF(0, 0);
        [Browsable(false)]
        [Category("Location")]
        [Description("The location of the mid point on screen coordinates")]
        public PointF ScrPointMid
        {
            get { return scrPointMid; }
            set { scrPointMid = value; ConvertFromScreenCoordinates(); }
        }

        private double angleBegin = 0;
        [Category("2.Maze Coordinates")]
        [Browsable(true)]
        [Description("Starting Angle of the Arc")]
        public double AngleBegin
        {
            get { return angleBegin; }
            set
            {
                double deltaAngle = angleBegin - value;

                angleBegin = value;

                //Move Point 1,2 based on new angle
                mzPoint1.SetPointF(Tools.RotatePoint(mzPoint1.GetPointF(), mzPointCenter.GetPointF(), -deltaAngle));
                mzPoint2.SetPointF(Tools.RotatePoint(mzPoint2.GetPointF(), mzPointCenter.GetPointF(), -deltaAngle));

                ConvertFromMazeCoordinates();
                OnPropertyChanged("AngleEndChanged");
            }
        }

        private double angleEnd = 360;
        [Category("2.Maze Coordinates")]
        [Browsable(true)]
        [Description("Ending Angle of the Arc")]
        public double AngleEnd
        {
            get { return angleEnd; }
            set
            {
                double deltaAngle = angleEnd - value;

                angleEnd = value;

                //Move Point 1,2 based on new angle
                mzPoint3.SetPointF(Tools.RotatePoint(mzPoint3.GetPointF(), mzPointCenter.GetPointF(), -deltaAngle));
                mzPoint4.SetPointF(Tools.RotatePoint(mzPoint4.GetPointF(), mzPointCenter.GetPointF(), -deltaAngle));

                //Move Point 3,4 based on new angle

                ConvertFromMazeCoordinates();
                OnPropertyChanged("AngleEndChanged");
            }
        }

        public double scrRadius = 0;
        private double circleRadius = 0;
        [Category("2.Maze Coordinates")]
        [Browsable(true)]
        [Description("Radius of the Arc")]
        public double CircleRadius
        {
            get { return circleRadius; }
            set
            {
                if (circleRadius < 0.5)
                    circleRadius = value;
                if(value>0.5)
                {
                    double radiusFactor = value / circleRadius;
                    mzPoint1.SetPointF(Tools.RotatePoint(mzPoint1.GetPointF(), mzPointCenter.GetPointF(), -angleBegin)); //unrotate
                    mzPoint2.SetPointF(Tools.RotatePoint(mzPoint2.GetPointF(), mzPointCenter.GetPointF(), -angleBegin));
                    mzPoint3.SetPointF(Tools.RotatePoint(mzPoint3.GetPointF(), mzPointCenter.GetPointF(), -angleEnd));
                    mzPoint4.SetPointF(Tools.RotatePoint(mzPoint4.GetPointF(), mzPointCenter.GetPointF(), -angleEnd));

                    mzPoint1.X = (mzPoint1.X - mzPointCenter.X) * radiusFactor + mzPointCenter.X;
                    mzPoint1.Y = (mzPoint1.Y - mzPointCenter.Y) * radiusFactor + mzPointCenter.Y;
                    mzPoint2.X = (mzPoint2.X - mzPointCenter.X) * radiusFactor + mzPointCenter.X;
                    mzPoint2.Y = (mzPoint2.Y - mzPointCenter.Y) * radiusFactor + mzPointCenter.Y;
                    mzPoint3.X = (mzPoint3.X - mzPointCenter.X) * radiusFactor + mzPointCenter.X;
                    mzPoint3.Y = (mzPoint3.Y - mzPointCenter.Y) * radiusFactor + mzPointCenter.Y;
                    mzPoint4.X = (mzPoint4.X - mzPointCenter.X) * radiusFactor + mzPointCenter.X;
                    mzPoint4.Y = (mzPoint4.Y - mzPointCenter.Y) * radiusFactor + mzPointCenter.Y;

                    mzPoint1.SetPointF(Tools.RotatePoint(mzPoint1.GetPointF(), mzPointCenter.GetPointF(), angleBegin)); //rerotate
                    mzPoint2.SetPointF(Tools.RotatePoint(mzPoint2.GetPointF(), mzPointCenter.GetPointF(), angleBegin));
                    mzPoint3.SetPointF(Tools.RotatePoint(mzPoint3.GetPointF(), mzPointCenter.GetPointF(), angleEnd));
                    mzPoint4.SetPointF(Tools.RotatePoint(mzPoint4.GetPointF(), mzPointCenter.GetPointF(), angleEnd));
                    circleRadius = value;
                    ConvertFromMazeCoordinates();
                    OnPropertyChanged("RadiusChanged");
                }
            }
        }

        private MPoint mzPoint1 = new MPoint(0, 0);
        [Category("2.Maze Coordinates")]
        [Description("Start Vertex 1 on Maze coordinates")]
        public new MPoint MzPoint1
        {
            get { return mzPoint1; }
            set { mzPoint1 = value; ConvertFromMazeCoordinates(); OnPropertyChanged("Vertex");}
        }
        private MPoint mzPoint2 = new MPoint(0, 0);
        [Category("2.Maze Coordinates")]
        [Description("Start Vertex 2 on Maze coordinates")]
        public new MPoint MzPoint2
        {
            get { return mzPoint2; }
            set { mzPoint2 = value; ConvertFromMazeCoordinates(); OnPropertyChanged("Vertex");}
        }
        private MPoint mzPoint3 = new MPoint(0, 0);
        [Category("2.Maze Coordinates")]
        [Description("End Vertex 1 on Maze coordinates")]
        public new MPoint MzPoint3
        {
            get { return mzPoint3; }
            set { mzPoint3 = value; ConvertFromMazeCoordinates(); OnPropertyChanged("Vertex");}
        }
        private MPoint mzPoint4 = new MPoint(0, 0);
        [Category("2.Maze Coordinates")]
        [Description("End Vertex 2 on Maze coordinates")]
        public  new MPoint MzPoint4
        {
            get { return mzPoint4; }
            set { mzPoint4 = value; ConvertFromMazeCoordinates(); OnPropertyChanged("Vertex");}
        }

        private MPoint mzPointCenter = new MPoint(0, 0);
        [Category("2.Maze Coordinates")]
        [Description("Center of the Arc in Maze coordinates")]
        public MPoint MzPointCenter
        {
            get { return mzPointCenter; }
            set {
                double deltaX = mzPointCenter.X - value.X;
                double deltaZ = mzPointCenter.Z - value.Z;
                mzPoint1.X -= deltaX;
                mzPoint1.Z -= deltaZ;
                mzPoint2.X -= deltaX;
                mzPoint2.Z -= deltaZ;
                mzPoint3.X -= deltaX;
                mzPoint3.Z -= deltaZ;
                mzPoint4.X -= deltaX;
                mzPoint4.Z -= deltaZ;
                mzPointCenter = value; ConvertFromMazeCoordinates(); OnPropertyChanged("Vertex"); }
        }


        private TPoint textureVertex1 = new TPoint(1, 1);
        [Category("Texture Coordinates")]
        [Description("Vertex 1 of Texture Coordinates")]
        [Browsable(false)]
        public new TPoint Vertex1
        {
            get { return textureVertex1; }
            set { textureVertex1 = value; OnPropertyChanged("Vertex");}
        }

        private TPoint textureVertex2 = new TPoint(1, 0);
        [Category("Texture Coordinates")]
        [Description("Vertex 2 of Texture Coordinates")]
        [Browsable(false)]
        public new TPoint Vertex2
        {
            get { return textureVertex2; }
            set { textureVertex2 = value; OnPropertyChanged("Vertex"); }
        }
        private TPoint textureVertex3 = new TPoint(0, 0);
        [Category("Texture Coordinates")]
        [Description("Vertex 3 of Texture Coordinates")]
        [Browsable(false)]
        public new TPoint Vertex3
        {
            get { return textureVertex3; }
            set { textureVertex3 = value; OnPropertyChanged("Vertex"); }
        }
        private TPoint textureVertex4 = new TPoint(0, 1);
        [Category("Texture Coordinates")]
        [Description("Vertex 4 of Texture Coordinates")]
        [Browsable(false)]
        public new TPoint Vertex4
        {
            get { return textureVertex4; }
            set { textureVertex4 = value; OnPropertyChanged("Vertex"); }
        }

        private int mappingIndex = 3; //default from texture vertex initializations...
        [Category("Texture")]
        [Browsable(false)]
        [Description("Rotation in Degrees")]
        public new int MappingIndex
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
        public new double TileSize
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
        public new double AspectRatio
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
        public new double MzTextureWidth
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
        public new double MzTextureHeight
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

        public new int texID = -999;
        private string texture = "";
        [Category("3.Texture")]
        [Description("Select texture to be used with the wall. List can be edited at Texture Collection")]
        [TypeConverter(typeof(ImagePathConverter))]
        public new string Texture
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
        public new bool Flip
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
        public new string Rotation{
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

        public new void AssignInitVals(string mode,int mIndex,double tSize,double aspect, bool flipped=false)
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
        public new  bool Stretch
        {
            get { return bStretched; }
            set { bStretched = value;
                OnPropertyChanged("Stretched");
            }
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
        public new  ModeType Mode
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
            this.Paint(ref gr, 1);
        }

        public new void Paint(ref Graphics gr, float opacity = 1)
        {

            if (!itemVisible&&!selected)
                return;

            Pen p;
            Brush br;


            if(scrRadius>0)
            {

                RectangleF rect = new RectangleF((float)(scrPointMid.X - scrRadius), (float)(scrPointMid.Y - scrRadius), (float)(scrRadius * 2), (float)(scrRadius * 2));

                if (selected == true)
                {
                    br = new SolidBrush(Color.FromArgb((int)(255*opacity),mazeColorSelected));
                    p = new Pen(br, 11);
                    Color aClr = Color.FromArgb((int)(255 * opacity), p.Color);
                    p.Color = aClr;
                    //gr.DrawLine(p, scrPoint1, scrPoint3);
                    //gr.DrawLine(p, scrPoint3, scrPoint2);
                    //gr.DrawCurve(p, scrPoints);
                    gr.DrawArc(p, rect, (float)angleBegin, (float)(angleEnd - angleBegin));
                    p.Dispose();
                    br.Dispose();
                }
                else
                {
                    //Rectangle rectI = new Rectangle(new Point((int)(circleCenter.X - circleRadius), (int)(circleCenter.Y - circleRadius)), new Size((int)(circleRadius * 2), (int)(circleRadius * 2)));
                    br = new SolidBrush(Color.FromArgb((int)(255 * opacity), mazeColorSelected));
                    p = new Pen(br, 5);
                    Color aClr = Color.FromArgb((int)(255 * opacity), p.Color);
                    p.Color = aClr;
                    //gr.DrawRectangle(Pens.Aquamarine, rectI);
                    gr.DrawArc(p, rect, (float)angleBegin, (float)(angleEnd - angleBegin));

                    //gr.DrawCurve(p, scrPoints);
                    //gr.DrawLine(p, scrPoint1, scrPoint3);
                    //gr.DrawLine(p, scrPoint3, scrPoint2);
                    p.Dispose();
                    br.Dispose();
                }
                

            

            }
          
        }
        public override bool IfSelected(int x, int y)
        {
            float tolerence = 10;

            PointF curPoint = new PointF(x, y);
            float dist = Tools.Distance(curPoint, scrPointMid);

            if (Math.Abs(dist - scrRadius) > tolerence) //check if radii match up
            {
                return false;

            }

            float curAngle=(180-Tools.GetAngleDegree(scrPointMid, curPoint))%360;
            adjustAngles();
            if (angleEnd > angleBegin)
            {
                if (curAngle < angleBegin)
                    curAngle += 360;
                if (curAngle > angleBegin && curAngle < angleEnd)
                    return true;
            }
            else if (angleBegin > angleEnd)
            {
                if (curAngle < angleEnd)
                    curAngle += 360;
                if(curAngle < angleBegin && curAngle > angleEnd)
                    return true;
            }
            else
                return false;
            return false;
        }

        private void adjustAngles()
        {
            double deltaAngle = angleEnd - angleBegin;
            angleBegin = angleBegin % 360;
            angleEnd = angleBegin + deltaAngle;

            if(angleEnd<0)
            {
                angleBegin += 360;
                angleEnd += 360;
            }
        }


        public override bool CheckClosePoints(int x,int y,ref PointF p)
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



        public override bool InRegion(int x1, int y1, int x2, int y2) //currently requires entire area to encompass selection
        {
            if(x1==x2&&y1==y2)
            {
                
                return this.IfSelected(x1, y1);
                
            }

            int iconTolerence = 0;
            x1 = x1 - iconTolerence; //top
            x2 = x2 + iconTolerence; //bottom
            y1 = y1 - iconTolerence; //left
            y2 = y2 + iconTolerence; //right


            int regCount = 0;
            int numCheck = 8;
            float numPlanes = (float)(angleEnd - angleBegin) / numCheck;
            PointF tempPointRot = new PointF(scrPoint1.X,scrPoint1.Y) ;
            for(int i=0;i<=numCheck; i++)
            {
                tempPointRot=(Tools.RotatePoint(scrPoint1, scrPointMid, numPlanes*i));
                if (x1 < tempPointRot.X && x2 > tempPointRot.X && y1 < tempPointRot.Y && y2 > tempPointRot.Y) //must entirely enclose two points + center
                {
                    regCount++;
                }
            }
            


            return regCount > numCheck/2;
        }

        public void EndPointFromFirstAndCenterAndCursor(bool invert = false)
        {
            scrRadius = Tools.Distance(scrPoint1, scrPointMid);

            if (scrRadius <= 0 || double.IsNaN(scrRadius))
            {
                angleBegin = 0;
                angleEnd = 0;
            }
            else
            {
                angleBegin = 180 - Tools.GetAngleDegree(scrPointMid, scrPoint1);
                angleEnd = 180 - Tools.GetAngleDegree(scrPointMid, scrPoint2);

                if (angleBegin >= angleEnd)
                    angleEnd += 360;

                scrPoint2 = Tools.RotatePoint(scrPoint1, scrPointMid, (angleEnd - angleBegin));
                if (invert)
                {
                    if (angleEnd > angleBegin)
                        angleBegin += 360;
                }

            }
        }

        public bool bCtrlFlag=false;
        private void ConvertFromScreenCoordinates()
        {
            mzPoint1.X = scrPoint1.X / scale;
           // mzPoint1.Y = 1;
            mzPoint1.Z = scrPoint1.Y / scale;

            mzPoint2.X = scrPoint1.X / scale;
           // mzPoint2.Y = -1;
            mzPoint2.Z = scrPoint1.Y / scale;

            //Tools.FindCircle(scrPoint1, scrPoint2, scrPointMid, out circleCenter, out circleRadius);

            mzPointCenter.X = scrPointMid.X / scale;
            // mzPoint3.Y = -1;
            mzPointCenter.Z = scrPointMid.Y / scale;



            mzPoint3.X = scrPoint2.X / scale;
            // mzPoint1.Y = 1;
            mzPoint3.Z = scrPoint2.Y / scale;

            mzPoint4.X = scrPoint2.X / scale;
            // mzPoint2.Y = -1;
            mzPoint4.Z = scrPoint2.Y / scale;

            scrRadius = Tools.Distance(scrPoint1, scrPointMid);
            circleRadius = scrRadius / scale;

            ReMeasure();
            CalculateFromTextureCoordinates();
        }
        public void ConvertFromMazeCoordinates()
        {
            scrPoint1.X = (float)((mzPoint1.X + MzPoint2.X) / 2.0 * scale);
            scrPoint1.Y = (float)((mzPoint1.Z + mzPoint2.Z) / 2.0 * scale);

            scrPoint2.X = (float)((mzPoint3.X + MzPoint4.X) / 2.0 * scale);
            scrPoint2.Y = (float)((mzPoint3.Z + mzPoint4.Z) / 2.0 * scale);

            scrPointMid.X = (float)((mzPointCenter.X) * scale);
            scrPointMid.Y = (float)((mzPointCenter.Z) * scale);

            scrRadius=Tools.Distance(scrPoint1,scrPointMid);
            circleRadius = scrRadius / scale;

            adjustAngles();

            //if (scrPoint1 == ScrPoint2)
            //{
            //    //Fix for old maze versions...
            //    MPoint temp;
            //    temp = MzPoint4;
            //    mzPoint4 = mzPoint2;
            //    mzPoint2 = temp;

            //    TPoint temp2;
            //    temp2 = Vertex4;
            //    Vertex4 = Vertex2;
            //    Vertex2 = temp2;

            //    scrPoint1.X = (float)((mzPoint1.X + MzPoint2.X) / 2.0 * scale);
            //    scrPoint1.Y = (float)((mzPoint1.Z + mzPoint2.Z) / 2.0 * scale);

            //    scrPoint2.X = (float)((mzPoint3.X + MzPoint4.X) / 2.0 * scale);
            //    scrPoint2.Y = (float)((mzPoint3.Z + mzPoint4.Z) / 2.0 * scale);
            //    if (scrPoint1 == scrPoint2)
            //    {
            //        scrPoint2.X += 1;
            //        scrPoint2.Y += 1;
            //    }
            //    ReMeasure();
            //}
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
            buf[0] = textureVertex1;
            buf[1] = textureVertex2;
            buf[2] = textureVertex3;
            buf[3] = textureVertex4;
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
                    case 3:
                        textureVertex3 = buf[index];
                        textureVertex4 = buf[(index + 1) % 4];
                        textureVertex1 = buf[(index + 2) % 4];
                        textureVertex2 = buf[(index + 3) % 4];
                        break;
                    case 4:
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
                double deltaAngle = Math.Abs(angleEnd - angleBegin);

                double l = circleRadius*2 * Math.PI * deltaAngle / 360;
                //double l = (Math.Sqrt(Math.Pow((MzPoint1.X - MzPoint4.X), 2) + Math.Pow((MzPoint1.Y - MzPoint4.Y), 2) + Math.Pow((MzPoint1.Z - MzPoint4.Z), 2)) + Math.Sqrt(Math.Pow((MzPoint2.X - MzPoint3.X), 2) + Math.Pow((MzPoint2.Y - MzPoint3.Y), 2) + Math.Pow((MzPoint2.Z - MzPoint3.Z), 2))) / 2;
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

        public override void Move(float mzXdir,float mzZdir)
        {
            mzPoint1.X += mzXdir;
            mzPoint1.Z += mzZdir;
            mzPoint2.X += mzXdir;
            mzPoint2.Z += mzZdir;
            mzPoint3.X += mzXdir;
            mzPoint3.Z += mzZdir;
            mzPoint4.X += mzXdir;
            mzPoint4.Z += mzZdir;

            mzPointCenter.X += mzXdir;
            mzPointCenter.Z += mzZdir;

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

            mzPointCenter.X *= factor;
            mzPointCenter.Z *= factor;

            ConvertFromMazeCoordinates();
        }

        public override void RescaleXYZ(double scaleX, double scaleY, double scaleZ)
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

            mzPointCenter.X *= scaleX;
            mzPointCenter.Y *= scaleY;
            mzPointCenter.Z *= scaleZ;

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

        public override string PrintToTreeItem()
        {
            string str = this.ID;
            if (!this.itemVisible)
                str += "👁";
            if (this.itemLocked)
                str += "🔒";

            if (string.IsNullOrWhiteSpace(this.Label) == false)
                    str += " [" + this.Label + "]";

            if (Texture != "")
            {
                str += "(" + Texture + ")";
            }

            return str;
        }

        public bool PrintToFile(ref StreamWriter fp)
        {
            try
            {
                MessageBox.Show("Not supported in classic maze", "MazeMaker", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return true;
            }
            catch
            {
                MessageBox.Show("Couldn't write Wall...", "MazeMaker", MessageBoxButtons.OK , MessageBoxIcon.Error);
                return false;
            }
        }

        
        private int numVBOplanes=0;
        public string buildVBO()
        {
            string vboText = "";
            float numPlanes = (float) Math.Abs((double)(angleEnd - angleBegin) / angularResolution);

            float curAngle=0;

            MPoint curPoint1 = new MPoint(mzPoint1);
            MPoint curPoint2 = new MPoint(mzPoint2);

            MPoint finalPoint1 = new MPoint(mzPoint4);
            MPoint finalPoint2 = new MPoint(mzPoint3);

            PointF mzCenterPoint = mzPointCenter.GetPointF();

            vboText += curPoint1.X.ToString() + "," + curPoint1.Y.ToString() + "," + curPoint1.Z.ToString() + ",";
            vboText += textureVertex1.X.ToString() + "," + textureVertex1.Y.ToString()+",";
            vboText += curPoint2.X.ToString() + "," + curPoint2.Y.ToString() + "," + curPoint2.Z.ToString() + ",";
            vboText += textureVertex2.X.ToString() + "," + textureVertex2.Y.ToString() + ",";
            numVBOplanes=1;

            for (int i = 0; i < numPlanes; i++)
            {

                curPoint1.Y = mzPoint1.Y + (finalPoint1.Y - mzPoint1.Y) * i / numPlanes; //interpolate y coordinates
                curPoint2.Y = mzPoint2.Y + (finalPoint2.Y - mzPoint2.Y) * i / numPlanes;

                curAngle += (float)angularResolution;
                curPoint1.SetPointF(Tools.RotatePoint(mzPoint1.GetPointF(), mzCenterPoint, curAngle));
                curPoint2.SetPointF(Tools.RotatePoint(mzPoint2.GetPointF(), mzCenterPoint, curAngle));

                if (curAngle < angleEnd - angleBegin)
                {

                    numVBOplanes++;
                    vboText += curPoint1.X.ToString() + "," + curPoint1.Y.ToString() + "," + curPoint1.Z.ToString() + ",";
                    vboText += (textureVertex1.X+(i+1)/ (numPlanes+1) * (textureVertex3.X- textureVertex1.X)).ToString() + "," + (textureVertex1.Y + (i + 1) / (numPlanes + 1) * (textureVertex4.Y - textureVertex1.Y)).ToString() + ",";
                    vboText += curPoint2.X.ToString() + "," + curPoint2.Y.ToString() + "," + curPoint2.Z.ToString() + ",";
                    vboText += (textureVertex2.X+ (i+1) / (numPlanes+1) * (textureVertex4.X - textureVertex2.X)).ToString() + "," + (textureVertex2.Y + (i + 1) / (numPlanes + 1) * (textureVertex3.Y - textureVertex2.Y)).ToString() + ",";
                }
            }

            numVBOplanes++;

            curPoint1.Y = finalPoint1.Y; //interpolate y coordinates
            curPoint2.Y = finalPoint2.Y;

            //curPoint1.SetPointF(Tools.RotatePoint(MzPoint1.GetPointF(), mzCenterPoint, angleEnd-angleStart));
            //curPoint2.SetPointF(Tools.RotatePoint(MzPoint2.GetPointF(), mzCenterPoint, angleEnd - angleStart));
            curPoint1 = finalPoint1;
            curPoint2 = finalPoint2;

            vboText += curPoint1.X.ToString() + "," + curPoint1.Y.ToString() + "," + curPoint1.Z.ToString() + ",";
            vboText += (textureVertex3.X).ToString() + "," + textureVertex1.Y.ToString() + ",";
            vboText += curPoint2.X.ToString() + "," + curPoint2.Y.ToString() + "," + curPoint2.Z.ToString() + ",";
            vboText += (textureVertex4.X).ToString() + "," + textureVertex2.Y.ToString() + ",";

            vboText += curPoint1.X.ToString() + "," + curPoint1.Y.ToString() + "," + curPoint1.Z.ToString() + ",";
            vboText += (textureVertex3.X).ToString() + "," + textureVertex1.Y.ToString() + ",";
            vboText += curPoint2.X.ToString() + "," + curPoint2.Y.ToString() + "," + curPoint2.Z.ToString() + ",";
            vboText += (textureVertex4.X).ToString() + "," + textureVertex2.Y.ToString() + ",";

            return vboText;
        }

        public string buildVBO_Index()
        {
            string indexText = "";

            //double angularResolution = 10.0f; //one plane per degree

            //float numPlanes = (float)Math.Abs((double)(angleEnd - angleBegin) / angularResolution);
            double curAngle = 0;
            

            for (int i=0;i<numVBOplanes*2-2;i+=2)
            {
                curAngle += angularResolution;
                if (indexText.Length != 0)
                    indexText += ",";
                indexText += i.ToString() + "," + (i + 1).ToString() + "," + (i + 2).ToString() + "," + (i + 2).ToString() + "," + (i + 1).ToString() + "," + (i + 3).ToString();
        
            }

            return indexText;
        }

        public override XmlElement toXMLnode(XmlDocument doc, Dictionary<string, string> cImages)
        {
            XmlElement wallNode = doc.CreateElement(string.Empty, "CurvedWall", string.Empty);
            wallNode.SetAttribute("group", this.Group);
            wallNode.SetAttribute("label", this.Label);
            wallNode.SetAttribute("id", this.GetID().ToString());
            wallNode.SetAttribute("itemLocked", this.itemLocked.ToString());
            wallNode.SetAttribute("itemVisible", this.itemVisible.ToString());

            XmlElement geometryNode = doc.CreateElement(string.Empty, "Geometry", string.Empty);
            wallNode.AppendChild(geometryNode);
            geometryNode.InnerText = buildVBO();

            XmlElement indiciesNode = doc.CreateElement(string.Empty, "Indicies", string.Empty);
            wallNode.AppendChild(indiciesNode);
            indiciesNode.InnerText = buildVBO_Index();

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

            XmlElement mzPointCenterNode = doc.CreateElement(string.Empty, "MzPointCenter", string.Empty);
            wallNode.AppendChild(mzPointCenterNode);
            mzPointCenterNode.SetAttribute("x", this.MzPointCenter.X.ToString());
            mzPointCenterNode.SetAttribute("z", this.MzPointCenter.Z.ToString());
            mzPointCenterNode.SetAttribute("radius", this.CircleRadius.ToString());
            mzPointCenterNode.SetAttribute("angleStart", this.AngleBegin.ToString());
            mzPointCenterNode.SetAttribute("angleEnd", this.AngleEnd.ToString());

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

        new public CurvedWall Clone()
        {
            return Copy(true,0);
        }

        public override PointF CalcMidPoint()
        {
            return new PointF((float)(this.ScrPoint1.X + this.ScrPoint2.X) / 2, (float)(this.ScrPoint1.Y + this.ScrPoint2.Y) / 2);
        }

        public override void Rotate(float degrees,float centerX=0,float centerY=0)
        {
            PointF midPoint;
            if(centerX!=0&&centerY!=0)
                midPoint=new PointF(centerX, centerY);
            else
                midPoint= new PointF(ScrPointMid.X, ScrPointMid.Y);


            this.ScrPoint1 = Tools.RotatePoint(this.ScrPoint1, midPoint, degrees);
            this.ScrPoint2 = Tools.RotatePoint(this.ScrPoint2, midPoint, degrees);
            this.ScrPointMid = Tools.RotatePoint(this.ScrPointMid, midPoint, degrees);

            this.angleBegin += degrees;
            this.angleEnd += degrees;

        }

        public new CurvedWall Copy(bool clone, int offsetX=0, int offsetY=0)
        {
            CurvedWall temp = new CurvedWall(this.scale, this.Label, -1);

            temp.itemLocked = this.itemLocked;
            temp.itemVisible = this.itemVisible;

            temp.Vertex1 = new TPoint(this.Vertex1);
            temp.Vertex2 = new TPoint(this.Vertex2);
            temp.Vertex3 = new TPoint(this.Vertex3);
            temp.Vertex4 = new TPoint(this.Vertex4);

            
            temp.AspectRatio = this.AspectRatio;
            temp.Color = this.Color;
            temp.Flip = this.Flip;
            temp.MappingIndex = this.MappingIndex;
            temp.Mode = this.Mode;
            temp.MzPoint1= new MPoint(this.MzPoint1);
            temp.MzPoint2 = new MPoint(this.MzPoint2);
            temp.MzPoint3 = new MPoint(this.MzPoint3);
            temp.MzPoint4 = new MPoint(this.MzPoint4);
            temp.MzPointCenter = new MPoint(this.MzPointCenter);
            temp.angleBegin = this.AngleBegin;
            temp.angleEnd = this.AngleEnd;
            temp.circleRadius = this.circleRadius;
            temp.MzTextureHeight = this.MzTextureHeight;
            temp.MzTextureWidth = this.MzTextureWidth;
            temp.MazeColorRegular = this.MazeColorRegular;
            temp.Rotation = this.Rotation;
            temp.Stretch = this.Stretch;
            temp.Texture = this.Texture;
            temp.TileSize = this.tileSize;
            temp.Visible = this.Visible;
            temp.justCreated = this.justCreated;
            temp.ScrPoint1 = new PointF(this.ScrPoint1.X + offsetX, this.ScrPoint1.Y + offsetY);
            temp.ScrPoint2 = new PointF(this.ScrPoint2.X + offsetX, this.ScrPoint2.Y + offsetY);
            temp.ScrPointMid = new PointF(this.ScrPointMid.X + offsetX, this.ScrPointMid.Y + offsetY);

            if (clone)
            {
                temp.SetID(this.GetID(),true);
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
