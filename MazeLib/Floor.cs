#region Using directives

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Data;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Xml;
#endregion

namespace MazeMaker
{
    [Serializable]
    public class Floor : MazeItem
    {
        public Floor(double dScale, string label="", int id = 0, bool generateNewID = false) //0 for new, -1 for temporary, other for set specific Id
        {
            scale = dScale;
            if (generateNewID)
                id = 0;
            this.SetID(id);
            mzPoint1.Y = -1;
            mzPoint2.Y = -1;
            mzPoint3.Y = -1;
            mzPoint4.Y = -1;
            this.Label = label;
            itemType = MazeItemType.Floor;
        }

        public Floor(XmlNode floorNode)
        {
            this.SetID(Tools.getIntFromAttribute(floorNode, "id", -1));
            this.Label = Tools.getStringFromAttribute(floorNode, "label", "");
            XmlNode floorTextureNode = floorNode.SelectSingleNode("FloorTexture");
            this.floorTexID = Tools.getIntFromAttribute(floorTextureNode, "id", -999);
            //this.floorFlip = Tools.getBoolFromAttribute(floorTextureNode, "flip", false);
            XmlNode ceilingTextureNode = floorNode.SelectSingleNode("CeilingTexture");
            this.ceilingTexID = Tools.getIntFromAttribute(ceilingTextureNode, "id", -999);
            //this.Flip = Tools.getBoolFromAttribute(ceilingTextureNode, "flip", false);

            this.FloorColor = Tools.getColorFromNode(floorNode,"FloorColor");
            this.CeilingColor = Tools.getColorFromNode(floorNode, "CeilingColor");
            this.MzPoint1 = Tools.getXYZfromNode(floorNode, 1);
            this.MzPoint2 = Tools.getXYZfromNode(floorNode, 2);
            this.MzPoint3 = Tools.getXYZfromNode(floorNode, 3);
            this.MzPoint4 = Tools.getXYZfromNode(floorNode, 4);
            this.FloorVertex1 = Tools.getTexCoordFromNode(floorNode, 1);
            this.FloorVertex2 = Tools.getTexCoordFromNode(floorNode, 2);
            this.FloorVertex3 = Tools.getTexCoordFromNode(floorNode, 3);
            this.FloorVertex4 = Tools.getTexCoordFromNode(floorNode, 4);

            this.CeilingVertex1 = Tools.getTexCoordFromNode(floorNode, 1,true);
            this.CeilingVertex2 = Tools.getTexCoordFromNode(floorNode, 2, true);
            this.CeilingVertex3 = Tools.getTexCoordFromNode(floorNode, 3, true);
            this.CeilingVertex4 = Tools.getTexCoordFromNode(floorNode, 4, true);

            XmlNode appearanceNode = floorNode.SelectSingleNode("Appearance");
            this.Visible = Tools.getBoolFromAttribute(appearanceNode, "visible", true);
            this.Ceiling = Tools.getBoolFromAttribute(appearanceNode, "hasCeiling", this.Ceiling);
            this.CeilingHeight = Tools.getDoubleFromAttribute(appearanceNode, "ceilingHeight", this.CeilingHeight);

            int mappingIndex = Texture.GetMappingIndex(this.FloorVertex1, this.FloorVertex2, this.FloorVertex3, this.FloorVertex4);
            String mode = Texture.GetMode(this.FloorVertex1, this.FloorVertex2, this.FloorVertex3, this.FloorVertex4);
            double tileSize = Texture.GetTileSize(this.FloorVertex1, this.FloorVertex2, this.FloorVertex3, this.FloorVertex4, this.MzPoint1, this.MzPoint2, this.MzPoint3, this.MzPoint4);
            double aspectRatio = Texture.GetAspectRatio(this.FloorVertex1, this.FloorVertex2, this.FloorVertex3, this.FloorVertex4, this.MzPoint1, this.MzPoint2, this.MzPoint3, this.MzPoint4);

            mappingIndex = Tools.getIntFromAttribute(floorTextureNode, "rotation", mappingIndex);
            mode = Tools.getStringFromAttribute(floorTextureNode, "mode", mode);
            tileSize = Tools.getDoubleFromAttribute(floorTextureNode, "tileSize", tileSize);
            aspectRatio = Tools.getDoubleFromAttribute(floorTextureNode, "aspectRatio", aspectRatio);

            this.AssignInitVals(mode, mappingIndex, tileSize, aspectRatio, true);

            mappingIndex = Texture.GetMappingIndex(this.CeilingVertex1, this.CeilingVertex2, this.CeilingVertex3, this.CeilingVertex4);
            mode = Texture.GetMode(this.CeilingVertex1, this.CeilingVertex2, this.CeilingVertex3, this.CeilingVertex4);
            tileSize = Texture.GetTileSize(this.CeilingVertex1, this.CeilingVertex2, this.CeilingVertex3, this.CeilingVertex4, this.MzPoint1, this.MzPoint2, this.MzPoint3, this.MzPoint4);
            aspectRatio = Texture.GetAspectRatio(this.CeilingVertex1, this.CeilingVertex2, this.CeilingVertex3, this.CeilingVertex4, this.MzPoint1, this.MzPoint2, this.MzPoint3, this.MzPoint4);


            mappingIndex = Tools.getIntFromAttribute(ceilingTextureNode, "rotation", mappingIndex);
            mode = Tools.getStringFromAttribute(ceilingTextureNode, "mode", mode);
            tileSize = Tools.getDoubleFromAttribute(ceilingTextureNode, "tileSize", tileSize);
            aspectRatio = Tools.getDoubleFromAttribute(ceilingTextureNode, "aspectRatio", aspectRatio);

            this.AssignInitVals(mode, mappingIndex, tileSize, aspectRatio, false);

            itemType = MazeItemType.Floor;
        }

        public void AssignInitVals(string mode, int mIndex, double tSize, double aspect,bool floor)
        {
            if (floor)
            {
                if (mode == "Stretch")
                    bStretchedFloor = true;
                if (mode == "Tile")
                    bStretchedFloor = false;
                mappingIndexCeiling = mIndex;

                if (!bStretchedFloor) //discards aspect ratio and tile size if stretched
                {
                    aspectRatioFloor = aspect;
                    tileSizeFloor = tSize;
                }
            }
            else
            {
                if (mode == "Stretch")
                    bStretchedCeiling = true;
                if (mode == "Tile")
                    bStretchedCeiling = false;
                mappingIndexFloor = mIndex;

                if (!bStretchedCeiling) //discards aspect ratio and tile size if stretched
                {
                    aspectRatioCeiling = aspect;
                    tileSizeCeiling = tSize;
                }
            }
            CalculateFromTextureCoordinates(); //recalculates texture coordinates based on rotations, aspect, tile size etc.
        }

        //private bool selected = false;
        private double scale = 17;
        [Category("3.Appearance")]
        [Browsable(false)]
        [Description("Coordinate transformation coefficient")]
        public double Scale
        {
            get { return scale; }
            set
            {
                //CalculateModifiedScaleMazeCoordinates(value);
                scale = value;
                ConvertFromMazeCoordinates();
                OnPropertyChanged("Appearance");
            }
        }

        private bool visible = true;
        [Category("3.Appearance")]
        [Description("Toggles wall visibility in maze. If set to false, floor still will be present but not rendered")]
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

        //private int textureIndex = 0;
        //[Category("Texture")]
        //[Description("Index of texture in the texture list to associate with floor")]
        //public int TextureIndex
        //{
        //    get { return textureIndex; }
        //    set { textureIndex = value; }
        //}

        //private int textureIndex2 = 0;
        //[Category("Texture")]
        //[Description("Index of texture in the texture list to associate with ceiling")]
        //public int TextureIndex2
        //{
        //    get { return textureIndex2; }
        //    set { textureIndex2 = value; }
        //}

        private int mappingIndexFloor = 1; //default from texture vertex initializations...
        [Category("Texture")]
        [Description("Vertex number (1-4) of floor to associate with the lower left corner of the texture image. Texture image can be rotated by changing this parameter")]
        [Browsable(false)]
        public int MappingIndexFloor
        {
            get { return mappingIndexFloor; }
            set
            {
                if (value < 1 || value > 4)
                {
                    throw new ArgumentException("Please enter a number between 1 and 4");
                }
                TPoint[] buf = new TPoint[4];
                buf[0] = floorVertex1;
                buf[1] = floorVertex2;
                buf[2] = floorVertex3;
                buf[3] = floorVertex4;
                int index = mappingIndexFloor - 1;
                mappingIndexFloor = value;
                switch (mappingIndexFloor)
                {
                    case 1:
                        floorVertex1 = buf[index];
                        floorVertex2 = buf[(index + 1) % 4];
                        floorVertex3 = buf[(index + 2) % 4];
                        floorVertex4 = buf[(index + 3) % 4];
                        break;
                    case 2:
                        floorVertex2 = buf[index];
                        floorVertex3 = buf[(index + 1) % 4];
                        floorVertex4 = buf[(index + 2) % 4];
                        floorVertex1 = buf[(index + 3) % 4];
                        break;
                    case 3:
                        floorVertex3 = buf[index];
                        floorVertex4 = buf[(index + 1) % 4];
                        floorVertex1 = buf[(index + 2) % 4];
                        floorVertex2 = buf[(index + 3) % 4];
                        break;
                    case 4:
                        floorVertex4 = buf[index];
                        floorVertex1 = buf[(index + 1) % 4];
                        floorVertex2 = buf[(index + 2) % 4];
                        floorVertex3 = buf[(index + 3) % 4];
                        break;
                }
                OnPropertyChanged("Index");
            }
        }

        private int mappingIndexCeiling = 1; //default from texture vertex initializations...
        [Category("Texture")]
        [Browsable(false)]
        [Description("Vertex number (1-4) of ceiling to associate with the lower left corner of the texture image. Texture image can be rotated by changing this parameter")]
        public int MappingIndexCeiling
        {
            get { return mappingIndexCeiling; }
            set
            {
                if (value < 1 || value > 4)
                {
                    throw new ArgumentException("Please enter a number between 1 and 4");
                }
                TPoint[] buf = new TPoint[4];
                buf[0] = ceilingVertex1;
                buf[1] = ceilingVertex2;
                buf[2] = ceilingVertex3;
                buf[3] = ceilingVertex4;
                int index = mappingIndexCeiling - 1;
                mappingIndexCeiling = value;
                switch (mappingIndexCeiling)
                {
                    case 1:
                        ceilingVertex1 = buf[index];
                        ceilingVertex2 = buf[(index + 1) % 4];
                        ceilingVertex3 = buf[(index + 2) % 4];
                        ceilingVertex4 = buf[(index + 3) % 4];
                        break;
                    case 2:
                        ceilingVertex2 = buf[index];
                        ceilingVertex3 = buf[(index + 1) % 4];
                        ceilingVertex4 = buf[(index + 2) % 4];
                        ceilingVertex1 = buf[(index + 3) % 4];
                        break;
                    case 3:
                        ceilingVertex3 = buf[index];
                        ceilingVertex4 = buf[(index + 1) % 4];
                        ceilingVertex1 = buf[(index + 2) % 4];
                        ceilingVertex2 = buf[(index + 3) % 4];
                        break;
                    case 4:
                        ceilingVertex4 = buf[index];
                        ceilingVertex1 = buf[(index + 1) % 4];
                        ceilingVertex2 = buf[(index + 2) % 4];
                        ceilingVertex3 = buf[(index + 3) % 4];
                        break;
                }
                OnPropertyChanged("VertexNum");
            }
        }

        [TypeConverter(typeof(RotationConverter))]
        [Category("5.Ceiling Properties")]
        [DisplayName("Texture Rotation")]
        [Description("Identify the orientation of the texture image to be wrapped to the maze item")]
        public string TextureRotation1
        {
            get
            {
                if (mappingIndexFloor == 3)
                    return "0°";
                else if (mappingIndexFloor == 4)
                    return "90°";
                else if (mappingIndexFloor == 1)
                    return "180°";
                else if (mappingIndexFloor == 2)
                    return "270°";
                else return "Error";
            }
            set
            {
                if (value == "180°")
                    mappingIndexFloor = 1;
                if (value == "270°")
                    mappingIndexFloor = 2;
                if (value == "0°")
                    mappingIndexFloor = 3;
                if (value == "90°")
                    mappingIndexFloor = 4;
                CalculateFromTextureCoordinates();
                OnPropertyChanged("Rotation");
            }
        }

        [TypeConverter(typeof(RotationConverter))]
        [Category("4.Floor Properties")]
        [DisplayName("Texture Rotation")]
        [Description("Identify the orientation of the texture image to be wrapped to the maze item")]
        public string TextureRotation2
        {
            get
            {
                if (mappingIndexCeiling == 3)
                    return "0°";
                else if (mappingIndexCeiling == 4)
                    return "90°";
                else if (mappingIndexCeiling == 1)
                    return "180°";
                else if (mappingIndexCeiling == 2)
                    return "270°";
                else return "Error";
            }
            set
            {
                if (value == "180°")
                    mappingIndexCeiling = 1;
                if (value == "270°")
                    mappingIndexCeiling = 2;
                if (value == "0°")
                    mappingIndexCeiling = 3;
                if (value == "90°")
                    mappingIndexCeiling = 4;
                CalculateFromTextureCoordinates();
                OnPropertyChanged("Rotation2");
            }
        }

        private bool ceiling= true;
        [Category("3.Appearance")]
        [Description("Create a ceiling over this floor")]
        public bool Ceiling
        {
            get { return ceiling; }
            set
            {
                ceiling = value;
                OnPropertyChanged("Ceiling");
            }
        }

        private Color pDisplay = Color.White;
        [Category("4.Floor Properties")]
        [Description("The display color of the floor in the Maze")]
        public Color FloorColor
        {
            get { return pDisplay; }
            set { pDisplay = value; OnPropertyChanged("FloorColor"); }
        }

        private Color pDisplay2 = Color.White;
        [Category("5.Ceiling Properties")]
        [Description("The display color of the ceiling in the Maze")]
        public Color CeilingColor
        {
            get { return pDisplay2; }
            set { pDisplay2 = value; OnPropertyChanged("CeilingColor"); }
        }

        private Color mazeColorRegular = Color.CornflowerBlue;
        [Browsable(false)]
        [Category("Colors")]
        [Description("The Current color in the editor")]
        public Color MazeColorRegular
        {
            get { return mazeColorRegular; }
            set { mazeColorRegular = value; }
        }

        private Color mazeColorSelected = Color.DodgerBlue;
        [Browsable(false)]
        [Category("Colors")]
        [Description("The Selection Mode color")]
        public Color MazeColorSelected
        {
            get { return mazeColorSelected; }
            set { mazeColorSelected = value;  }
        }

        private RectangleF scrRect = new RectangleF(0,0,0,0);
        [Category("Location")]
        [Browsable(false)]
        [Description("Location on screen coordinates")]
        public RectangleF Rect
        {
            get { return scrRect; }
            set { scrRect = value; ConvertFromScreenCoordinates(); }
        }

        private MPoint mzPoint1 = new MPoint(0, 0);
        [Category("2.Maze Coordinates")]
        [Description("Corner 1 on Maze coordinates")]//[Description("Vertex 1 on Maze coordinates")]
        public MPoint MzPoint1
        {
            get { return mzPoint1; }
            set {
                if (mzPoint1 == value)
                    return;
                mzPoint1 = value;
                mzPoint4.X = value.X;
                mzPoint2.Z = value.Z;
                ConvertFromMazeCoordinates(); OnPropertyChanged("Vertex1"); }
        }
        private MPoint mzPoint2 = new MPoint(0, 0);
        [Category("2.Maze Coordinates")]
        [Browsable(true)]
        [Description("Vertex 2 on Maze coordinates")]
        public MPoint MzPoint2
        {
            get { return mzPoint2; }
            set { 
                if (mzPoint2 == value)
                    return;
                mzPoint2 = value;
                mzPoint3.X = value.X;
                mzPoint1.Z = value.Z;
                ConvertFromMazeCoordinates(); OnPropertyChanged("Vertex2"); }
        }
        private MPoint mzPoint3 = new MPoint(0, 0);
        [Category("2.Maze Coordinates")]
        [Description("Corner 2 on Maze coordinates")]//[Description("Vertex 3 on Maze coordinates")]
        public MPoint MzPoint3
        {
            get { return mzPoint3; }
            set {
                if (mzPoint3 == value)
                    return;
                mzPoint3 = value;
                mzPoint2.X = value.X;
                mzPoint4.Z = value.Z;
                ConvertFromMazeCoordinates(); OnPropertyChanged("Vertex3"); }
        }
        private MPoint mzPoint4 = new MPoint(0, 0);
        [Category("2.Maze Coordinates")]
        [Browsable(true)]
        [Description("Vertex 4 on Maze coordinates")]
        public MPoint MzPoint4
        {
            get { return mzPoint4; }
            set {
                if (mzPoint4 == value)
                    return;
                mzPoint4 = value;
                mzPoint1.X = value.X;
                mzPoint3.Z = value.Z;
                ConvertFromMazeCoordinates(); OnPropertyChanged("Vertex4"); }
        }
        private TPoint floorVertex1 = new TPoint(1, 1);
        [Category("Texture Coordinates")]
        [Description("Vertex 1 of Texture Coordinates")]
        [Browsable(false)]
        public TPoint FloorVertex1
        {
            get { return floorVertex1; }
            set { floorVertex1 = value; ConvertFromMazeCoordinates(); OnPropertyChanged("TexFloorVertex1"); }
        }
        private TPoint floorVertex2 = new TPoint(1, 0);
        [Category("Texture Coordinates")]
        [Description("Vertex 2 of Texture Coordinates")]
        [Browsable(false)]
        public TPoint FloorVertex2
        {
            get { return floorVertex2; }
            set { floorVertex2 = value; ConvertFromMazeCoordinates(); OnPropertyChanged("TexFloorVertex2"); }
        }
        private TPoint floorVertex3 = new TPoint(0, 0);
        [Category("Texture Coordinates")]
        [Description("Vertex 3 of Texture Coordinates")]
        [Browsable(false)]
        public TPoint FloorVertex3
        {
            get { return floorVertex3; }
            set { floorVertex3 = value; ConvertFromMazeCoordinates(); OnPropertyChanged("TexFloorVertex3"); }
        }
        private TPoint floorVertex4 = new TPoint(0, 1);
        [Category("Texture Coordinates")]
        [Description("Vertex 4 of Texture Coordinates")]
        [Browsable(false)]
        public TPoint FloorVertex4
        {
            get { return floorVertex4; }
            set { floorVertex4 = value; ConvertFromMazeCoordinates(); OnPropertyChanged("TexFloorVertex4"); }
        }

        private TPoint ceilingVertex1 = new TPoint(1, 1);
        [Category("Texture Coordinates")]
        [Description("Vertex 1 of Texture Coordinates")]
        [Browsable(false)]
        public TPoint CeilingVertex1
        {
            get { return ceilingVertex1; }
            set { ceilingVertex1 = value; ConvertFromMazeCoordinates(); OnPropertyChanged("TexCeilingVertex1"); }
        }
        private TPoint ceilingVertex2 = new TPoint(1, 0);
        [Category("Texture Coordinates")]
        [Description("Vertex 2 of Texture Coordinates")]
        [Browsable(false)]
        public TPoint CeilingVertex2
        {
            get { return ceilingVertex2; }
            set { ceilingVertex2 = value; ConvertFromMazeCoordinates(); OnPropertyChanged("TexCeilingVertex2"); }
        }
        private TPoint ceilingVertex3 = new TPoint(0, 0);
        [Category("Texture Coordinates")]
        [Description("Vertex 3 of Texture Coordinates")]
        [Browsable(false)]
        public TPoint CeilingVertex3
        {
            get { return ceilingVertex3; }
            set { ceilingVertex3 = value; ConvertFromMazeCoordinates(); OnPropertyChanged("TexCeilingVertex3"); }
        }
        private TPoint ceilingVertex4 = new TPoint(0, 1);
        [Category("Texture Coordinates")]
        [Description("Vertex 4 of Texture Coordinates")]
        [Browsable(false)]
        public TPoint CeilingVertex4
        {
            get { return ceilingVertex4; }
            set { ceilingVertex4 = value; ConvertFromMazeCoordinates(); OnPropertyChanged("TexCeilingVertex4"); }
        }
        private double aspectRatioCeiling = 1;
        [Category("5.Ceiling Properties")]
        [Description("Aspect Ratio of Ceiling Texture")]
        [DisplayName("Aspect Ratio")]
        public double AspectRatioCeiling
        {
            get { return aspectRatioCeiling; }
            set
            {
                if (value <= 0)
                    throw new ArgumentException("Aspect Ratio must be greater than zero");
                aspectRatioCeiling = value;
                CalculateFromTextureCoordinates();
                OnPropertyChanged("AspectRatio1");
            }
        }
        private double aspectRatioFloor = 1;
        [Category("4.Floor Properties")]
        [Description("Aspect Ratio of Floor Texture")]
        [DisplayName("Aspect Ratio")]
        public double AspectRatioFloor
        {
            get { return aspectRatioFloor; }
            set
            {
                if (value <= 0)
                    throw new ArgumentException("Aspect Ratio must be greater than zero");
                aspectRatioFloor = value;
                CalculateFromTextureCoordinates();
                OnPropertyChanged("AspectRatio2");
            }
        }
        private double tileSizeCeiling = 1;
        [Category("5.Ceiling Properties")]
        [Description("Size of tile in maze units")]
        [DisplayName("Tile Size")]
        public double TileSizeCeiling
        {
            get { return tileSizeCeiling; }
            set
            {
                if (value <= 0)
                    throw new ArgumentException("TileSize must be greater than zero");
                tileSizeCeiling = value;

                CalculateFromTextureCoordinates();
                OnPropertyChanged("TileSize1");
            }
        }

        private double tileSizeFloor = 1;
        [Category("4.Floor Properties")]
        [DisplayName("Tile Size")]
        [Description("Size of tile in maze units")]
        public double TileSizeFloor
        {
            get { return tileSizeFloor; }
            set
            {
                if (value <= 0)
                    throw new ArgumentException("TileSize must be greater than zero");
                tileSizeFloor = value;

                CalculateFromTextureCoordinates();
                OnPropertyChanged("TileSize2");
            }
        }

        private bool bStretchedCeiling = true;
        [Category("5.Ceiling Properties")]
        [Description("If true, stretch the texture across the ceiling")]
        [Browsable(false)]
        public bool StretchCeiling
        {
            get { return bStretchedCeiling; }
            set
            {
                bStretchedCeiling = value;
                OnPropertyChanged("StretchCeiling");
            }
        }

        private bool bStretchedFloor = true;
        [Category("4.Floor Properties")]
        [Description("If true, stretch the texture across the floor")]
        [Browsable(false)]
        public bool StretchFloor
        {
            get { return bStretchedFloor; }
            set
            {
                bStretchedFloor = value;
                OnPropertyChanged("Stretch Floor");
            }
        }

        public enum ModeType
        {
            Stretch = 0,
            Tile = 1,
        }
       
        [Category("5.Ceiling Properties")]
        [Description("Identify how texture image is wrapped to the maze item")]
        [DisplayName("Mode")]
        public ModeType ModeCeiling
        {
            get
            {
                if (bStretchedCeiling)
                    return ModeType.Stretch;
                else return ModeType.Tile;
            }
            set
            {
                if (value == ModeType.Stretch)
                    bStretchedCeiling = true;
                else if (value == ModeType.Tile)
                    bStretchedCeiling = false;
                CalculateFromTextureCoordinates();
                OnPropertyChanged("CeilingTexMode");
            }
        }
        
        [Category("4.Floor Properties")]
        [Description("Identify how texture image is wrapped to the maze item")]
        [DisplayName("Mode")]
        public ModeType ModeFloor
        {
            get
            {
                if (bStretchedFloor)
                    return ModeType.Stretch;
                else return ModeType.Tile;
            }
            set
            {
                if (value == ModeType.Stretch)
                    bStretchedFloor = true;
                else if (value == ModeType.Tile)
                    bStretchedFloor = false;
                CalculateFromTextureCoordinates();
                OnPropertyChanged("FloorTexMode");
            }
        }

        public int floorTexID = -999;
        private string floorTexture = "";
        [Category("4.Floor Properties")]
        [Description("Select texture to be used on the floor. List can be edited at Texture Collection")]
        [TypeConverter(typeof(ImagePathConverter))]
        public string FloorTexture
        {
            get { return floorTexture; }
            set { floorTexture = value; OnPropertyChanged("TextureFloor"); }

            //set
            //{
            //    floorTexture = value;
            
            //    if (floorTexture == null)
            //        floorTexID = -999;
            //    else
            //        floorTexID = floorTexture.Index;
                
            //    OnPropertyChanged("TextureFloor");
            //}
        }

        public int ceilingTexID = -999;
        private string ceilingTexture = "";
        [Category("5.Ceiling Properties")]
        [Description("Select texture to be used with the ceiling. List can be edited at Texture Collection")]
        [TypeConverter(typeof(ImagePathConverter))]
        public string CeilingTexture
        {
            get { return ceilingTexture; }
            set { ceilingTexture = value; OnPropertyChanged("TextureCeiling"); }

            //set
            //{
            //    ceilingTexture = value;
            
            //    if (ceilingTexture == null)
            //        ceilingTexID = -999;
            //    else
            //        ceilingTexID = ceilingTexture.Index;
                
            //    OnPropertyChanged("TextureCeiling");
            //}
        }

        private double ceilingHeight = 2.0;
        [Category("5.Ceiling Properties")]
        [Description("Height of the ceiling plane from the floor plane")]
        public double CeilingHeight
        {
            get { return ceilingHeight; }
            set { ceilingHeight = value; OnPropertyChanged("CeilingHeight"); }
        }

        public override void Paint(ref Graphics gr)
        {
            Brush br;
            if (selected == false)
            {
                //br = new SolidBrush(colorCur);
                Color foreColor = Color.FromArgb(180,mazeColorRegular);
                
                br = new HatchBrush(HatchStyle.DiagonalBrick, foreColor, mazeColorRegular);
            }
            else
            {
                //br = new SolidBrush(colorSel);
                Color foreColor = Color.FromArgb(90, mazeColorSelected);
                br = new HatchBrush(HatchStyle.Cross, foreColor, mazeColorSelected);
            }

            gr.FillRectangle(br, scrRect);
            br.Dispose();
            if (selected)
            {
                Pen a = new Pen(Brushes.Black, 3);
                //gr.DrawRectangle(a, scrRect);
                gr.DrawRectangle(a, scrRect.Left,scrRect.Top,scrRect.Width,scrRect.Height);
                a.Dispose();
            }
            else
            {
                //gr.DrawRectangle(Pens.Black,scrRect);
                gr.DrawRectangle(Pens.Black, scrRect.Left, scrRect.Top, scrRect.Width, scrRect.Height);
            }
            

        }

        public bool InRegion(int x1, int y1, int x2, int y2) //currently requires entire area to encompase selection
        {
            int iconTolerence = 0;
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
            CeilingHeight *= scaleY;

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

        private void ConvertFromScreenCoordinates()
        {
            mzPoint1.X = scrRect.Left / scale;
            //mzPoint1.Y = -1;
            mzPoint1.Z = scrRect.Top / scale;

            mzPoint2.X = scrRect.Right / scale;
            //mzPoint2.Y = -1;
            mzPoint2.Z = scrRect.Top / scale;

            mzPoint3.X = scrRect.Right / scale;
            //mzPoint3.Y = -1;
            mzPoint3.Z = scrRect.Bottom / scale;

            mzPoint4.X = scrRect.Left / scale;
            //mzPoint4.Y = -1;
            mzPoint4.Z = scrRect.Bottom / scale;
            CalculateFromTextureCoordinates();
        }
        public void ConvertFromMazeCoordinates()
        {
            mzPoint2.Z = mzPoint1.Z;
            mzPoint2.X = mzPoint3.X;
            mzPoint4.X = mzPoint1.X;
            mzPoint4.Z = mzPoint3.Z;

            scrRect.X = (float)((mzPoint1.X) * scale);
            scrRect.Y = (float)((mzPoint1.Z) * scale);
            scrRect.Width = (float)Math.Abs(((MzPoint3.X) * scale) - scrRect.X);
            scrRect.Height = (float)Math.Abs(((MzPoint4.Z) * scale) - scrRect.Y);
        }
        private void CalculateFromTextureCoordinates()
        {
            double l = (Math.Sqrt(Math.Pow((MzPoint1.X - MzPoint4.X), 2) + Math.Pow((MzPoint1.Y - MzPoint4.Y), 2) + Math.Pow((MzPoint1.Z - MzPoint4.Z), 2)) + Math.Sqrt(Math.Pow((MzPoint2.X - MzPoint3.X), 2) + Math.Pow((MzPoint2.Y - MzPoint3.Y), 2) + Math.Pow((MzPoint2.Z - MzPoint3.Z), 2))) / 2;
            double h = (Math.Sqrt(Math.Pow((MzPoint1.X - MzPoint2.X), 2) + Math.Pow((MzPoint1.Y - MzPoint2.Y), 2) + Math.Pow((MzPoint1.Z - MzPoint2.Z), 2)) + Math.Sqrt(Math.Pow((MzPoint3.X - MzPoint4.X), 2) + Math.Pow((MzPoint3.Y - MzPoint4.Y), 2) + Math.Pow((MzPoint3.Z - MzPoint4.Z), 2))) / 2;
                
            if (!bStretchedCeiling)
            {
                ceilingVertex1.X = l / (tileSizeCeiling * aspectRatioCeiling);
                ceilingVertex1.Y = h / tileSizeCeiling;
                ceilingVertex2.X = l / (tileSizeCeiling * aspectRatioCeiling);
                ceilingVertex2.Y = 0;
                ceilingVertex3.X = 0;
                ceilingVertex3.Y = 0;
                ceilingVertex4.X = 0;
                ceilingVertex4.Y = h / tileSizeCeiling;
            }
            else
            {
                ceilingVertex1.X = 1;
                ceilingVertex1.Y = 1;
                ceilingVertex2.X = 1;
                ceilingVertex2.Y = 0;
                ceilingVertex3.X = 0;
                ceilingVertex3.Y = 0;
                ceilingVertex4.X = 0;
                ceilingVertex4.Y = 1;
            }
            if (!bStretchedFloor)
            {
                floorVertex1.X = l / (tileSizeFloor * aspectRatioFloor);
                floorVertex1.Y = h / tileSizeFloor;
                floorVertex2.X = l / (tileSizeFloor * aspectRatioFloor);
                floorVertex2.Y = 0;
                floorVertex3.X = 0;
                floorVertex3.Y = 0;
                floorVertex4.X = 0;
                floorVertex4.Y = h / tileSizeFloor;
            }
            else
            {
                floorVertex1.X = 1;
                floorVertex1.Y = 1;
                floorVertex2.X = 1;
                floorVertex2.Y = 0;
                floorVertex3.X = 0;
                floorVertex3.Y = 0;
                floorVertex4.X = 0;
                floorVertex4.Y = 1;
            }
            TPoint[] buf = new TPoint[4];
            buf[0] = ceilingVertex1;
            buf[1] = ceilingVertex2;
            buf[2] = ceilingVertex3;
            buf[3] = ceilingVertex4;
            int index = 2;
            switch (mappingIndexFloor)
            {
                case 1:
                    ceilingVertex1 = buf[index];
                    ceilingVertex2 = buf[(index + 1) % 4];
                    ceilingVertex3 = buf[(index + 2) % 4];
                    ceilingVertex4 = buf[(index + 3) % 4];
                    break;
                case 2:
                    ceilingVertex2 = buf[index];
                    ceilingVertex3 = buf[(index + 1) % 4];
                    ceilingVertex4 = buf[(index + 2) % 4];
                    ceilingVertex1 = buf[(index + 3) % 4];
                    break;
                case 3:
                    ceilingVertex3 = buf[index];
                    ceilingVertex4 = buf[(index + 1) % 4];
                    ceilingVertex1 = buf[(index + 2) % 4];
                    ceilingVertex2 = buf[(index + 3) % 4];
                    break;
                case 4:
                    ceilingVertex4 = buf[index];
                    ceilingVertex1 = buf[(index + 1) % 4];
                    ceilingVertex2 = buf[(index + 2) % 4];
                    ceilingVertex3 = buf[(index + 3) % 4];
                    break;
            }
            buf[0] = floorVertex1;
            buf[1] = floorVertex2;
            buf[2] = floorVertex3;
            buf[3] = floorVertex4;
            
            switch (mappingIndexCeiling)
            {
                case 1:
                    floorVertex1 = buf[index];
                    floorVertex2 = buf[(index + 1) % 4];
                    floorVertex3 = buf[(index + 2) % 4];
                    floorVertex4 = buf[(index + 3) % 4];
                    break;
                case 2:
                    floorVertex2 = buf[index];
                    floorVertex3 = buf[(index + 1) % 4];
                    floorVertex4 = buf[(index + 2) % 4];
                    floorVertex1 = buf[(index + 3) % 4];
                    break;
                case 3:
                    floorVertex3 = buf[index];
                    floorVertex4 = buf[(index + 1) % 4];
                    floorVertex1 = buf[(index + 2) % 4];
                    floorVertex2 = buf[(index + 3) % 4];
                    break;
                case 4:
                    floorVertex4 = buf[index];
                    floorVertex1 = buf[(index + 1) % 4];
                    floorVertex2 = buf[(index + 2) % 4];
                    floorVertex3 = buf[(index + 3) % 4];
                    break;
            }
        }
        public string PrintToTreeItem()
        {
            string str = this.ID;

            if (string.IsNullOrWhiteSpace(this.Label) == false)
                str += " [" + this.Label + "]";

            if (ceiling)
                str += "(Ceiling:Y)";
            else
                str += "(Ceiling:N)";
            if (FloorTexture != "")
            {
                str += "(" + FloorTexture + ")";
            }

            return str;
        }
        public bool PrintToFile(ref StreamWriter fp, Dictionary<string, string> cImages)
        {
            try
            {
                fp.WriteLine("0\t6\t" + this.GetID().ToString() + "\t" + this.Label);
                double r, g, b;
                r = FloorColor.R / 255.0;
                g = FloorColor.G / 255.0;
                b = FloorColor.B / 255.0;

                //int textureIndex = 0;
                //if (floorTexture != null) textureIndex = floorTexture.Index;
                //fp.WriteLine( textureIndex.ToString() + "\t" + r.ToString(".#;.#;0") + "\t" + g.ToString(".#;.#;0") + "\t" + b.ToString(".#;.#;0"));
                string imageID = "0";
                if (cImages.ContainsKey(floorTexture))
                    imageID = cImages[floorTexture];
                fp.WriteLine(imageID + "\t" + r.ToString(".#;.#;0") + "\t" + g.ToString(".#;.#;0") + "\t" + b.ToString(".#;.#;0"));

                fp.WriteLine(this.FloorVertex1.X.ToString(".##;-.##;0") + "\t" + this.FloorVertex1.Y.ToString(".##;-.##;0") + "\t" + this.mzPoint1.X.ToString(".##;-.##;0") + "\t" + this.mzPoint1.Y.ToString(".##;-.##;0") + "\t" + this.mzPoint1.Z.ToString(".##;-.##;0"));
                fp.WriteLine(this.FloorVertex2.X.ToString(".##;-.##;0") + "\t" + this.FloorVertex2.Y.ToString(".##;-.##;0") + "\t" + this.mzPoint2.X.ToString(".##;-.##;0") + "\t" + this.mzPoint2.Y.ToString(".##;-.##;0") + "\t" + this.mzPoint2.Z.ToString(".##;-.##;0"));
                fp.WriteLine(this.FloorVertex3.X.ToString(".##;-.##;0") + "\t" + this.FloorVertex3.Y.ToString(".##;-.##;0") + "\t" + this.mzPoint3.X.ToString(".##;-.##;0") + "\t" + this.mzPoint3.Y.ToString(".##;-.##;0") + "\t" + this.mzPoint3.Z.ToString(".##;-.##;0"));
                fp.WriteLine(this.FloorVertex4.X.ToString(".##;-.##;0") + "\t" + this.FloorVertex4.Y.ToString(".##;-.##;0") + "\t" + this.mzPoint4.X.ToString(".##;-.##;0") + "\t" + this.mzPoint4.Y.ToString(".##;-.##;0") + "\t" + this.mzPoint4.Z.ToString(".##;-.##;0"));
                //fp.WriteLine("1\t0\t0\t0\t0");
                fp.WriteLine("1\t0\t" + (visible ? "0" : "-1") + "\t0\t0");
                if (Ceiling)
                {
                    fp.WriteLine("0\t6");
                    r = CeilingColor.R / 255.0;
                    g = CeilingColor.G / 255.0;
                    b = CeilingColor.B / 255.0;
                    //textureIndex = 0;
                    //if (ceilingTexture != null) textureIndex = ceilingTexture.Index;
                    //fp.WriteLine( textureIndex.ToString() + "\t" + r.ToString(".#;.#;0") + "\t" + g.ToString(".#;.#;0") + "\t" + b.ToString(".#;.#;0"));
                    imageID = "0";
                    if (cImages.ContainsKey(ceilingTexture))
                        imageID = cImages[ceilingTexture];
                    fp.WriteLine(imageID + "\t" + r.ToString(".#;.#;0") + "\t" + g.ToString(".#;.#;0") + "\t" + b.ToString(".#;.#;0"));
                    //r = -1 * this.mzPoint1.Y;
                    r = this.mzPoint1.Y + ceilingHeight;
                    fp.WriteLine(this.ceilingVertex1.X.ToString(".##;-.##;0") + "\t" + this.ceilingVertex1.Y.ToString(".##;-.##;0") + "\t" + this.mzPoint1.X.ToString(".##;-.##;0") + "\t" + r.ToString(".##;-.##;0") + "\t" + this.mzPoint1.Z.ToString(".##;-.##;0"));
                    //r = -1 * this.mzPoint2.Y;
                    r = this.mzPoint2.Y + ceilingHeight;
                    fp.WriteLine(this.ceilingVertex2.X.ToString(".##;-.##;0") + "\t" + this.ceilingVertex2.Y.ToString(".##;-.##;0") + "\t" + this.mzPoint2.X.ToString(".##;-.##;0") + "\t" + r.ToString(".##;-.##;0") + "\t" + this.mzPoint2.Z.ToString(".##;-.##;0"));
                    //r = -1 * this.mzPoint3.Y;
                    r = this.mzPoint3.Y + ceilingHeight;
                    fp.WriteLine(this.ceilingVertex3.X.ToString(".##;-.##;0") + "\t" + this.ceilingVertex3.Y.ToString(".##;-.##;0") + "\t" + this.mzPoint3.X.ToString(".##;-.##;0") + "\t" + r.ToString(".##;-.##;0") + "\t" + this.mzPoint3.Z.ToString(".##;-.##;0"));
                    //r = -1 * this.mzPoint4.Y;
                    r = this.mzPoint4.Y + ceilingHeight;
                    fp.WriteLine(this.ceilingVertex4.X.ToString(".##;-.##;0") + "\t" + this.ceilingVertex4.Y.ToString(".##;-.##;0") + "\t" + this.mzPoint4.X.ToString(".##;-.##;0") + "\t" + r.ToString(".##;-.##;0") + "\t" + this.mzPoint4.Z.ToString(".##;-.##;0"));
                    //fp.WriteLine("1\t1\t0\t0\t0");
                    fp.WriteLine("1\t1\t" + (visible ? "0" : "-1") + "\t0\t0");
                }
                return true;
            }
            catch
            {
                MessageBox.Show("Couldn't write Floor...","MazeMaker", MessageBoxButtons.OK , MessageBoxIcon.Error);
                return false;
            }
        }

        public XmlElement toXMLnode(XmlDocument doc, Dictionary<string, string> cImages)
        {
            XmlElement floorNode = doc.CreateElement(string.Empty, "Floor", string.Empty);
            floorNode.SetAttribute("label", this.Label);
            floorNode.SetAttribute("id", this.GetID().ToString());

            XmlElement mzPoint1node = doc.CreateElement(string.Empty, "MzPoint1", string.Empty);
            floorNode.AppendChild(mzPoint1node);
            mzPoint1node.SetAttribute("x", this.MzPoint1.X.ToString());
            mzPoint1node.SetAttribute("y", this.MzPoint1.Y.ToString());
            mzPoint1node.SetAttribute("z", this.MzPoint1.Z.ToString());
            mzPoint1node.SetAttribute("texX", this.FloorVertex1.X.ToString());
            mzPoint1node.SetAttribute("texY", this.FloorVertex1.Y.ToString());
            mzPoint1node.SetAttribute("texX_Ceiling", this.CeilingVertex1.X.ToString());
            mzPoint1node.SetAttribute("texY_Ceiling", this.CeilingVertex1.Y.ToString());

            XmlElement mzPoint2node = doc.CreateElement(string.Empty, "MzPoint2", string.Empty);
            floorNode.AppendChild(mzPoint2node);
            mzPoint2node.SetAttribute("x", this.MzPoint2.X.ToString());
            mzPoint2node.SetAttribute("y", this.MzPoint2.Y.ToString());
            mzPoint2node.SetAttribute("z", this.MzPoint2.Z.ToString());
            mzPoint2node.SetAttribute("texX", this.FloorVertex2.X.ToString());
            mzPoint2node.SetAttribute("texY", this.FloorVertex2.Y.ToString());
            mzPoint2node.SetAttribute("texX_Ceiling", this.CeilingVertex2.X.ToString());
            mzPoint2node.SetAttribute("texY_Ceiling", this.CeilingVertex2.Y.ToString());

            XmlElement mzPoint3node = doc.CreateElement(string.Empty, "MzPoint3", string.Empty);
            floorNode.AppendChild(mzPoint3node);
            mzPoint3node.SetAttribute("x", this.MzPoint3.X.ToString());
            mzPoint3node.SetAttribute("y", this.MzPoint3.Y.ToString());
            mzPoint3node.SetAttribute("z", this.MzPoint3.Z.ToString());
            mzPoint3node.SetAttribute("texX", this.FloorVertex3.X.ToString());
            mzPoint3node.SetAttribute("texY", this.FloorVertex3.Y.ToString());
            mzPoint3node.SetAttribute("texX_Ceiling", this.CeilingVertex3.X.ToString());
            mzPoint3node.SetAttribute("texY_Ceiling", this.CeilingVertex3.Y.ToString());

            XmlElement mzPoint4node = doc.CreateElement(string.Empty, "MzPoint4", string.Empty);
            floorNode.AppendChild(mzPoint4node);
            mzPoint4node.SetAttribute("x", this.MzPoint4.X.ToString());
            mzPoint4node.SetAttribute("y", this.MzPoint4.Y.ToString());
            mzPoint4node.SetAttribute("z", this.MzPoint4.Z.ToString());
            mzPoint4node.SetAttribute("texX", this.FloorVertex4.X.ToString());
            mzPoint4node.SetAttribute("texY", this.FloorVertex4.Y.ToString());
            mzPoint4node.SetAttribute("texX_Ceiling", this.CeilingVertex4.X.ToString());
            mzPoint4node.SetAttribute("texY_Ceiling", this.CeilingVertex4.Y.ToString());

            XmlElement floorColorNode = doc.CreateElement(string.Empty, "FloorColor", string.Empty);
            floorNode.AppendChild(floorColorNode);
            floorColorNode.SetAttribute("r", ((float)this.FloorColor.R / 255).ToString());
            floorColorNode.SetAttribute("g", ((float)this.FloorColor.G / 255).ToString());
            floorColorNode.SetAttribute("b", ((float)this.FloorColor.B / 255).ToString());


            //if (this.FloorTexture != null)
            //{
            //    XmlElement floorTextureNode = doc.CreateElement(string.Empty, "FloorTexture", string.Empty);
            //    floorNode.AppendChild(floorTextureNode);
            //    floorTextureNode.SetAttribute("id", this.FloorTexture.Index.ToString());
            //    floorTextureNode.SetAttribute("aspectRatio", this.AspectRatioFloor.ToString());
            //    floorTextureNode.SetAttribute("mode", this.ModeFloor.ToString());
            //    floorTextureNode.SetAttribute("rotation", this.mappingIndexCeiling.ToString());
            //    floorTextureNode.SetAttribute("tileSize", this.TileSizeFloor.ToString());
            //}
            if (cImages.ContainsKey(FloorTexture))
            {
                XmlElement floorTextureNode = doc.CreateElement(string.Empty, "FloorTexture", string.Empty);
                floorNode.AppendChild(floorTextureNode);
                floorTextureNode.SetAttribute("id", cImages[FloorTexture]);
                floorTextureNode.SetAttribute("aspectRatio", AspectRatioFloor.ToString());
                floorTextureNode.SetAttribute("mode", ModeFloor.ToString());
                floorTextureNode.SetAttribute("rotation", mappingIndexCeiling.ToString());
                floorTextureNode.SetAttribute("tileSize", TileSizeFloor.ToString());
            }

            XmlElement ceilingColorNode = doc.CreateElement(string.Empty, "CeilingColor", string.Empty);
            floorNode.AppendChild(ceilingColorNode);
            ceilingColorNode.SetAttribute("r", ((float)this.CeilingColor.R / 255).ToString());
            ceilingColorNode.SetAttribute("g", ((float)this.CeilingColor.G / 255).ToString());
            ceilingColorNode.SetAttribute("b", ((float)this.CeilingColor.B / 255).ToString());

            if (cImages.ContainsKey(CeilingTexture))
            {
                XmlElement ceilingTextureNode = doc.CreateElement(string.Empty, "CeilingTexture", string.Empty);
                floorNode.AppendChild(ceilingTextureNode);
                ceilingTextureNode.SetAttribute("id", cImages[CeilingTexture]);
                ceilingTextureNode.SetAttribute("aspectRatio", AspectRatioCeiling.ToString());
                ceilingTextureNode.SetAttribute("mode", ModeCeiling.ToString());
                ceilingTextureNode.SetAttribute("rotation", mappingIndexFloor.ToString());
                ceilingTextureNode.SetAttribute("tileSize", TileSizeCeiling.ToString());
            }

            XmlElement appearanceNode = doc.CreateElement(string.Empty, "Appearance", string.Empty);
            floorNode.AppendChild(appearanceNode);
            appearanceNode.SetAttribute("hasCeiling", this.Ceiling.ToString());
            appearanceNode.SetAttribute("ceilingHeight", this.CeilingHeight.ToString());
            appearanceNode.SetAttribute("visible", this.Visible.ToString());

            return floorNode;
        }

        new public Floor Clone()
        {
            return Copy(true, 0);
        }

        public Floor Copy(bool clone=false, int offsetX=0, int offsetY=0)
        {
            Floor temp = new Floor(this.scale, this.Label, -1);
            //temp.ID = this.ID;
            temp.AspectRatioCeiling = this.AspectRatioCeiling;
            temp.AspectRatioFloor = this.AspectRatioFloor;
            temp.Ceiling = this.Ceiling;
            temp.CeilingColor = this.CeilingColor;
            temp.CeilingHeight = this.CeilingHeight;
            temp.CeilingVertex1 = new TPoint(this.CeilingVertex1);
            temp.CeilingVertex2 = new TPoint(this.CeilingVertex2);
            temp.CeilingVertex3 = new TPoint(this.CeilingVertex3);
            temp.CeilingVertex4 = new TPoint(this.CeilingVertex4);
            temp.FloorColor = this.FloorColor;
            temp.FloorVertex1 = new TPoint(this.FloorVertex1);
            temp.FloorVertex2 = new TPoint(this.FloorVertex2);
            temp.FloorVertex3 = new TPoint(this.FloorVertex3);
            temp.FloorVertex4 = new TPoint(this.FloorVertex4);
            temp.MappingIndexFloor = this.MappingIndexFloor;
            temp.MappingIndexCeiling = this.MappingIndexCeiling;
            temp.ModeCeiling = this.ModeCeiling;
            temp.ModeFloor = this.ModeFloor;

            temp.MzPoint1 = new MPoint(this.MzPoint1);
            temp.MzPoint2 = new MPoint(this.MzPoint2);
            temp.MzPoint3 = new MPoint(this.MzPoint3);
            temp.MzPoint4 = new MPoint(this.MzPoint4);

            temp.MazeColorRegular = this.MazeColorRegular;
            temp.StretchCeiling = this.StretchCeiling;
            temp.StretchFloor = this.StretchFloor;
            temp.CeilingTexture = this.ceilingTexture;
            temp.FloorTexture = this.FloorTexture;
            temp.TextureRotation1 = this.TextureRotation1;
            temp.TextureRotation2 = this.TextureRotation2;
            temp.TileSizeCeiling = this.TileSizeCeiling;
            temp.TileSizeFloor = this.TileSizeFloor;

            temp.Visible = this.Visible;
            temp.justCreated = this.justCreated;

            temp.Rect = new RectangleF(this.Rect.X + offsetX, this.Rect.Y + offsetY, this.Rect.Width, this.Rect.Height);
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
