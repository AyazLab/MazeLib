#region Using directives

using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.ComponentModel;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;


#endregion

namespace MazeMaker
{
    public class Maze
    {
        public Maze()
        {
           
        }

        public bool changed = false;
        
        public List<Wall> cWall = new List<Wall>();
        public List<Floor> cFloor = new List<Floor>();
        public List<CustomObject> cObject = new List<CustomObject>();
        public List<Light> cLight = new List<Light>();
        public List<StaticModel> cStaticModels = new List<StaticModel>();
        public List<DynamicModel> cDynamicModels = new List<DynamicModel>();

        public List<EndRegion> cEndRegions = new List<EndRegion>();

        private string cMazeDirectory;

        public List<Model> cModels = new List<Model>();



        [Category("Models")]
        [Description("Model Files. Place these files to the same directory of with the Maze file or place them in the global models directory")]
        public List<Model> Models
        {
            get { return cModels; }
            set
            {
                cModels = value;
            }
        }

        [Category("Models")]
        [Description("Available Model Number in the List")]
        [ReadOnly(true)]
        public int ModelNumber
        {
            get { return cModels.Count; }
        }

        public List<Texture> cImages = new List<Texture>();
        [Category("Texture")]
        [Description("Texture Image Files. Place these files to the same directory of with the Maze file or place them in the global texture directory")]
        public List<Texture> Images
        {
            get { return cImages; }
            set
            {
                cImages = value; 
            }
        }

        [Category("Texture")]
        [Description("Available Texture Image Number in the List")]
        [ReadOnly(true)]
        public int ImageNumber
        {
            get { return cImages.Count; }
        }

        public StartPos cStart=null;
        //public EndRegion cEnd=null;

        private string desginer = "Anonymous";
        [Category("General")]
        [Description("Designer of the Maze")]
        public string Designer
        {
            get { return desginer; }
            set { desginer = value; }
        }
        private string comments = "";
        [Category("General")]
        [Description("Notes about the Maze")]
        public string Comments
        {
            get { return comments; }
            set { comments = value; }
        }
        private string name = "";
        [Category("General")]
        [Description("Name of the Maze")]
        [ReadOnly(true)]
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        private string filename = "";
        [Category("General")]
        [Description("Name of the Maze")]
        [ReadOnly(true)]
        public string FileName
        {
            get { return filename; }
            set { filename = value; }
        }

        private string timeoutMessage = "";
        [Category("Options")]
        [Description("Message to be shown when timeout occurs")]
        public string TimeoutMessage
        {
            get { return timeoutMessage; }
            set { timeoutMessage = value; }
        }

        private double scale = 17;
        [Category("Options")]
        [Description("Default Coordinate transformation coefficient for all objects")]
        public double Scale
        {
            get 
            { 
                return scale; 
            }
            set
            {
                scale = value;
                foreach (Wall w in cWall)
                    w.Scale = scale;
                foreach (Floor f in cFloor)
                    f.Scale = scale;                
                if (cStart != null)
                    cStart.Scale = scale;
                //if (cEnd != null)
                //   cEnd.Scale = scale;
                foreach (EndRegion en in cEndRegions)
                    en.Scale = scale;
                foreach (Light l in cLight)
                    l.Scale = scale;
                foreach (StaticModel s in cStaticModels)
                    s.Scale = scale;
                foreach (DynamicModel d in cDynamicModels)
                    d.Scale = scale;
            }
        }

        private double timeout = 0;
        [Category("Options")]
        [Description("Timeout value for the Maze walker in seconds. Enter 0 (zero) to disable")]
        public double Timeout
        {
            get 
            { 
                return timeout; 
            }
            set
            { 
                timeout = value;
            }
        }

        private double move = 0.005;
        [Category("Speed")]
        [Description("Walk Speed of the subject in the environment")]
        public double Move
        {
            get
            {
                return move;
            }
            set
            {
                move = value;
            }
        }
        private double view = 0.03;
        [Category("Speed")]
        [Description("View(Turn around) speed of the subject in the environment")]
        public double View
        {
            get
            {
                return view;
            }
            set
            {
                view = value;
            }
        }


        //private Color ambientColor = Color.White;
        //[Category("Global Light")]
        //[Description("Ambient light color of the current light source")]
        //public Color AmbientColor
        //{
        //    get { return ambientColor; }
        //    set { ambientColor = value; }
        //}

        //private float ambientIntensity = 0.1f;
        //[Category("Global Light")]
        //[Description("Ambient light intensity of the current light source. Min=0.0f, Max=1.0f")]
        //public float AmbientIntesity
        //{
        //    get { return ambientIntensity; }
        //    set { ambientIntensity = value; }
        //}

        //private Color diffuseColor = Color.White;
        //[Category("Global Light")]
        //[Description("Diffuse light color of the current light source")]
        //public Color DiffuseColor
        //{
        //    get { return diffuseColor; }
        //    set { diffuseColor = value; }
        //}

        //private float diffuseIntensity = 1.0f;
        //[Category("Global Light")]
        //[Description("Diffuse light intensity of the current light source. Min=0.0f, Max=1.0f")]
        //public float DiffuseIntesity
        //{
        //    get { return diffuseIntensity; }
        //    set { diffuseIntensity = value; }
        //}
        //private int type = 0;
        //[Category("Global Light")]
        //[Description("Type of light. 0->Torch and 1->Regular")]
        //public int Type
        //{
        //    get { return type; }
        //    set { type = value; }
        //}
        //private int attenuation = 0;
        //[Category("Global Light")]
        //[Description("Attenuation of the light. 0->None, 1->Constant, 2->Linear, 3->Quadratic")]
        //public int Attenuation
        //{
        //    get { return attenuation; }
        //    set { attenuation = value; }
        //}

        public bool SaveToFile(string inp)
        { 
            StreamWriter fp = new StreamWriter(inp);

            if (fp == null)
            {
                return false;
            }
            
            SetName(inp);

            FinalCheckBeforeWrite();

            changed = false;

            fp.WriteLine("Maze File 1.07");
            ////Texture List/////////
            fp.WriteLine("-1\t-1");
            //bitmap file names goes here
            //fp.WriteLine("\t1\tmetal.bmp");
            //int index = 1;
            foreach (Texture t in cImages)
            {
                if (t.Name != "")
                {
                    //fp.WriteLine("\t{0}\t{1}", index, t.Name);   
                 
                    //check weather the texture is in the list...

                    bool used = false;
                    if (used == false)
                    {
                        foreach (Wall d in cWall)
                        {
                            if (d.Texture != null && d.Texture.Index == t.Index)
                            {
                                used = true;
                                break;
                            }
                        }
                    }
                    if (used == false)
                    {
                        foreach (Floor d in cFloor)
                        {
                            if (d.TextureFloor != null && d.TextureFloor.Index == t.Index)
                            {
                                used = true;
                                break;
                            }
                            else if (d.TextureCeiling != null && d.TextureCeiling.Index == t.Index)
                            {
                                used = true;
                                break;
                            }
                        }
                    }
                    if (used)
                    {
                        fp.WriteLine("\t{0}\t{1}", t.Index, t.Name.Trim());
                    }
                    else
                    {
                        fp.WriteLine("\t{0}\t{1}", t.Index, t.Name.Trim() + " ");
                    }

                    
                }
                //index++;
            }

            fp.WriteLine("\t-1");


            ////Models List/////////
            fp.WriteLine("-10\t-1");
            //bitmap file names goes here
            //fp.WriteLine("\t1\tmetal.bmp");
            //int index = 1;
            foreach (Model t in cModels)
            {
                if (t.Name != "")
                {
                    //fp.WriteLine("\t{0}\t{1}", index, t.Name);      
              
                    //check weather the model is used or not!
                    bool used = false;

                    foreach (StaticModel s in cStaticModels)
                    {
                        if(s.Model!= null && s.Model.Index == t.Index)
                        {
                            used = true;
                            break;
                        }
                    }
                    if(used==false)
                    {
                        foreach (DynamicModel d in cDynamicModels)
                        {
                            if(d.Model!= null && d.Model.Index == t.Index)
                            {                                used=true;
                                break;
                            }
                        }
                    }
                    if(used)
                    {
                        fp.WriteLine("\t{0}\t{1}", t.Index, t.Name.Trim());
                    }
                    else
                    {
                        fp.WriteLine("\t{0}\t{1}", t.Index, t.Name.Trim() + " ");
                    }

                }
                //index++;
            }

            fp.WriteLine("\t-1");

            ////Objects/////////
            foreach (Floor f in cFloor)
            {
                f.PrintToFile(ref fp);
            }

            foreach (Wall w in cWall)
            {
                w.PrintToFile(ref fp);
            }

            foreach (StaticModel l in cStaticModels)
            {
                l.PrintToFile(ref fp);
            }
            foreach (DynamicModel l in cDynamicModels)
            {
                l.PrintToFile(ref fp);
            }
            if (cStart!=null)
            {
                cStart.PrintToFile(ref fp);
            }

            //if (cEnd != null)
            //{
            //    cEnd.PrintToFile(ref fp);
            //}
            foreach (EndRegion en in cEndRegions)
            {
                en.PrintToFile(ref fp);
            }

            foreach (Light l in cLight)
            {
                l.PrintToFile(ref fp);
            }

            ////Timeout/////////
            if (timeout != 0)
            {                
                fp.WriteLine("-4\t1");
                timeout *= 1000;  //have to write milliseconds, not seconds...
                fp.WriteLine("\t" + timeout.ToString(".##;-.##;0") + "\t" + timeoutMessage);
                timeout /= 1000;
            }

            // Write Designer...
            fp.WriteLine("-5\t1");
            if (desginer.Length == 0)
                fp.WriteLine("\t ");
            else
                fp.WriteLine("\t" + desginer);

            //Write Comments...
            fp.WriteLine("-6\t1");
            if (comments.Length == 0)
                fp.WriteLine("\t ");
            else
                fp.WriteLine("\t" + comments);

            //Write Scale..
            fp.WriteLine("-7\t1");
            fp.WriteLine(scale.ToString(".;-.;0"));

            //Write Move and View Speeds..
            fp.WriteLine("-8\t1");
            fp.WriteLine(move.ToString(".####;-.####;0") + "\t" + view.ToString(".####;-.####;0"));

            fp.Close();
            return true;
        }

        private Texture GetTexture(int id)
        {
            foreach(Texture t in cImages)
            {
                if (t.Index == id)
                    return t;
            }
            return null;
        }

        private Model GetModel(int id)
        {
            foreach (Model t in cModels)
            {
                if (t.Index == id)
                    return t;
            }
            return null;
        }

        public bool ReadOldFormat(string inp)
        {
            StreamReader fp = new StreamReader(inp);
            if (fp == null)
            {
                return false;
            }
            string buf = "";

            SetName(inp);
            int cmd = 0;
            int tab = 0;
            int tab2 = 0;
            
            MPoint tempPoint;// = new MPoint();
            MPoint tempPoint2;// = new MPoint();
            MPoint tempPoint3;// = new MPoint();
            MPoint tempPoint4;// = new MPoint();
            PointF tem = new PointF();
            PointF tem2 = new PointF();
            PointF tem3 = new PointF();
            PointF tem4 = new PointF();
            Color col= new Color();
            int texture=0;
            Floor tFloor;
            Wall tWall;
            Texture tTex;
            EndRegion tEn;
          try
          {
                while (true)
                {
                    buf = fp.ReadLine();
                    cmd = Int32.Parse(buf);
                    switch (cmd)
                    {
                        case 0:     //plane
                            buf = fp.ReadLine();
                            ReadColorLine(ref buf, ref texture, ref col);                            
                            buf = fp.ReadLine();
                            tempPoint = new MPoint();
                            ReadALine(ref buf, ref tem, ref tempPoint);
                            buf = fp.ReadLine();
                            tempPoint2 = new MPoint();
                            ReadALine(ref buf, ref tem2, ref tempPoint2);
                            buf = fp.ReadLine();
                            tempPoint3 = new MPoint();
                            ReadALine(ref buf, ref tem3, ref tempPoint3);
                            buf = fp.ReadLine();
                            tempPoint4 = new MPoint();
                            ReadALine(ref buf, ref tem4, ref tempPoint4);
                            if (AreEqual(tempPoint.Y, tempPoint2.Y) && AreEqual(tempPoint2.Y,tempPoint3.Y) && AreEqual(tempPoint3.Y,tempPoint4.Y))
                            {
                                //floor
                                if (tempPoint.Y < 0)
                                {
                                    //new floor
                                    tFloor = new Floor(scale);
                                    //tFloor.TextureIndex = texture;
                                    tFloor.TextureFloor = GetTexture(texture);
                                    tFloor.FloorColor = col;
                                    tFloor.MzPoint1 = tempPoint;
                                    tFloor.MzPoint2 = tempPoint2;
                                    tFloor.MzPoint3 = tempPoint3;
                                    tFloor.MzPoint4 = tempPoint4;
                                    tFloor.FloorVertex1 = new TPoint(tem.X, tem.Y);
                                    tFloor.FloorVertex2 = new TPoint(tem2.X, tem2.Y);
                                    tFloor.FloorVertex3 = new TPoint(tem3.X, tem3.Y);
                                    tFloor.FloorVertex4 = new TPoint(tem4.X, tem4.Y);
                                    tFloor.Ceiling = false;
                                    cFloor.Add(tFloor);
                                }
                                else
                                {
                                    //ceiling - search for its floor
                                    foreach (Floor f in cFloor)
                                    {                                        
                                        if (AreEqual(f.MzPoint1.X, tempPoint.X) && AreEqual(f.MzPoint1.Z, tempPoint.Z) && AreEqual(f.MzPoint2.X, tempPoint2.X) && AreEqual(f.MzPoint2.Z, tempPoint2.Z))
                                        {
                                            //f.TextureIndex2 = texture;
                                            f.TextureCeiling = GetTexture(texture);
                                            f.Ceiling = true;
                                            f.CeilingColor = col;
                                            f.CeilingVertex1 = new TPoint(tem.X, tem.Y);
                                            f.CeilingVertex2 = new TPoint(tem2.X, tem2.Y);
                                            f.CeilingVertex3 = new TPoint(tem3.X, tem3.Y);
                                            f.CeilingVertex4 = new TPoint(tem4.X, tem4.Y);
                                            break;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                //wall                                
                                tWall = new Wall(scale);
                                //tWall.TextureIndex = texture;                                
                                tWall.Texture = GetTexture(texture);
                                tWall.Color = col;
                                tWall.MzPoint1 = tempPoint;
                                tWall.MzPoint2 = tempPoint2;
                                tWall.MzPoint3 = tempPoint3;
                                tWall.MzPoint4 = tempPoint4;
                                tWall.Vertex1 = new TPoint(tem.X, tem.Y);
                                tWall.Vertex2 = new TPoint(tem2.X, tem2.Y);
                                tWall.Vertex3 = new TPoint(tem3.X, tem3.Y);
                                tWall.Vertex4 = new TPoint(tem4.X, tem4.Y);
                                
                                cWall.Add(tWall);
                            }
                            break;
                        case 1:     //triangle
                            buf = fp.ReadLine();
                            buf = fp.ReadLine();
                            buf = fp.ReadLine();
                            buf = fp.ReadLine();
                            break;
                        case -1:    //texture
                            tab = 0;
                            cmd = 0;
                            while (cmd != -1)
                            {
                                fp.Read();
                                buf = fp.ReadLine();
                                tab = buf.IndexOf('\t');
                                if (tab != -1)
                                {
                                    cmd = int.Parse(buf.Substring(0, tab));                                    
                                    tTex = new Texture(cMazeDirectory, buf.Substring(tab+1),cmd);
                                    cImages.Add(tTex);
                                }
                                else
                                    break;
                            }
                            break;
                        case -2:    //start pos
                            buf = fp.ReadLine();
                            cStart = new StartPos(scale);
                            tab = buf.IndexOf('\t');
                            tempPoint = new MPoint();
                            tempPoint.X = double.Parse(buf.Substring(0, tab));
                            tab2 = buf.IndexOf('\t', tab + 1);
                            tempPoint.Y = double.Parse(buf.Substring(tab + 1, tab2 - tab));
                            tempPoint.Z = double.Parse(buf.Substring(tab2 + 1));
                            cStart.MzPoint = tempPoint;                            
                            break;
                        case -3:    //end region
                            buf = fp.ReadLine();
                            tEn = new EndRegion(scale);
                            tab = buf.IndexOf('\t');
                            tEn.MinX = float.Parse(buf.Substring(0, tab));
                            tab2 = buf.IndexOf('\t', tab + 1);
                            tEn.MaxX = float.Parse(buf.Substring(tab + 1, tab2 - tab));
                            tab = buf.IndexOf('\t', tab2 + 1);
                            tEn.MinZ = float.Parse(buf.Substring(tab2 + 1, tab - tab2));
                            tEn.MaxZ = float.Parse(buf.Substring(tab + 1));
                            cEndRegions.Add(tEn);
                            //old way
                            //buf = fp.ReadLine();
                            //cEnd = new EndRegion(scale);
                            //tab = buf.IndexOf('\t');
                            //cEnd.MinX = double.Parse(buf.Substring(0, tab));
                            //tab2 = buf.IndexOf('\t', tab + 1);
                            //cEnd.MaxX = double.Parse(buf.Substring(tab + 1, tab2 - tab));
                            //tab = buf.IndexOf('\t', tab2 + 1);
                            //cEnd.MinZ = double.Parse(buf.Substring(tab2 + 1, tab - tab2));
                            //cEnd.MaxZ = double.Parse(buf.Substring(tab + 1));
                            break;
                        case -4:    //timeout
                            buf = fp.ReadLine();
                            timeout = double.Parse(buf)/1000;
                            break;
                        case -5:    //designer
                            fp.Read();
                            buf = fp.ReadLine();
                            //buf.CopyTo(0,desginer,0,buf.Length);
                            desginer = buf;
                            break;
                        case -6:    //comment
                            fp.Read();
                            buf = fp.ReadLine();
                            //buf.CopyTo(0,comments,0,buf.Length);
                            comments = buf;
                            break;
                        case -7:
                            buf = fp.ReadLine();
                            this.Scale = double.Parse(buf);
                            break;
                    }
                }
            }
        
            catch//(Exception ex)
            {
                //MessageBox.Show(ex.Message);
            }
                fp.Close();
            return true;
        }
        public bool ReadCurrentFormat(ref StreamReader fp)
        {

            string buf = "";
            
            int cmd = 0;
            int tab = 0;
            int tab2 = 0;

            MPoint tempPoint;// = new MPoint();
            MPoint tempPoint2;// = new MPoint();
            MPoint tempPoint3;// = new MPoint();
            MPoint tempPoint4;// = new MPoint();
            PointF tem = new PointF();
            PointF tem2 = new PointF();
            PointF tem3 = new PointF();
            PointF tem4 = new PointF();
            Color col = new Color();
            int texture = 0;
            Floor tFloor;
            Wall tWall;
            Texture tTex;
            EndRegion tEn;
            Model mMod;
            string[] parsed;
            int flag1, flag2;
            while ((buf = fp.ReadLine()) != null)
            {                               
                try
                {
                    tab = buf.IndexOf('\t');
                    if(tab==-1)
                        cmd = Int32.Parse(buf);
                    else
                        cmd = Int32.Parse(buf.Substring(0,tab));
                    tab2 = Int32.Parse(buf.Substring(tab + 1));
                    switch (cmd)
                    {
                        case 0:     //plane
                            buf = fp.ReadLine();
                            ReadColorLine(ref buf, ref texture, ref col);
                            buf = fp.ReadLine();
                            tempPoint = new MPoint();
                            ReadALine(ref buf, ref tem, ref tempPoint);
                            buf = fp.ReadLine();
                            tempPoint2 = new MPoint();
                            ReadALine(ref buf, ref tem2, ref tempPoint2);
                            buf = fp.ReadLine();
                            tempPoint3 = new MPoint();
                            ReadALine(ref buf, ref tem3, ref tempPoint3);
                            buf = fp.ReadLine();
                            tempPoint4 = new MPoint();
                            ReadALine(ref buf, ref tem4, ref tempPoint4);
                            if (tab2 > 5)
                            {
                                buf = fp.ReadLine();
                                string[] parts= buf.Split(new char[] {'\t'},StringSplitOptions.RemoveEmptyEntries);
                                flag1 = int.Parse(parts[0]);
                                flag2 = int.Parse(parts[1]);
                            }
                            else
                            {
                                if (AreEqual(tempPoint.Y, tempPoint2.Y) && AreEqual(tempPoint2.Y, tempPoint3.Y) && AreEqual(tempPoint3.Y, tempPoint4.Y))
                                    flag1 = 1;
                                else
                                    flag1 = 0;

                                if (tempPoint.Y < 0)
                                    flag2 = 0;
                                else
                                    flag2 = 1;
                            }
                            //if (AreEqual(tempPoint.Y, tempPoint2.Y) && AreEqual(tempPoint2.Y, tempPoint3.Y) && AreEqual(tempPoint3.Y, tempPoint4.Y))
                            if(flag1==1)
                            {
                                //floor
                                //if (tempPoint.Y < 0)
                                if(flag2==0)
                                {
                                    //new floor
                                    tFloor = new Floor(scale);
                                    //tFloor.TextureIndex = texture;
                                    tFloor.TextureFloor = GetTexture(texture);
                                    tFloor.FloorColor = col;
                                    tFloor.MzPoint1 = tempPoint;
                                    tFloor.MzPoint2 = tempPoint2;
                                    tFloor.MzPoint3 = tempPoint3;
                                    tFloor.MzPoint4 = tempPoint4;
                                    tFloor.FloorVertex1 = new TPoint(tem.X, tem.Y);
                                    tFloor.FloorVertex2 = new TPoint(tem2.X, tem2.Y);
                                    tFloor.FloorVertex3 = new TPoint(tem3.X, tem3.Y);
                                    tFloor.FloorVertex4 = new TPoint(tem4.X, tem4.Y);
                                    int mappingIndex = GetMappingIndex(tFloor.FloorVertex1, tFloor.FloorVertex2, tFloor.FloorVertex3, tFloor.FloorVertex4);
                                    String mode = GetMode(tFloor.FloorVertex1, tFloor.FloorVertex2, tFloor.FloorVertex3, tFloor.FloorVertex4);
                                    double tileSize=GetTileSize(tFloor.FloorVertex1, tFloor.FloorVertex2, tFloor.FloorVertex3, tFloor.FloorVertex4,tFloor.MzPoint1,tFloor.MzPoint2,tFloor.MzPoint3,tFloor.MzPoint4);
                                    double aspectRatio=GetAspectRatio(tFloor.FloorVertex1, tFloor.FloorVertex2, tFloor.FloorVertex3, tFloor.FloorVertex4,tFloor.MzPoint1,tFloor.MzPoint2,tFloor.MzPoint3,tFloor.MzPoint4);
                                    tFloor.AssignInitVals(mode, mappingIndex, tileSize,aspectRatio, true);
                                    tFloor.Ceiling = false;
                                    cFloor.Add(tFloor);
                                }
                                else
                                {
                                    //ceiling - search for its floor
                                    foreach (Floor f in cFloor)
                                    {
                                        if (AreEqual(f.MzPoint1.X, tempPoint.X) && AreEqual(f.MzPoint1.Z, tempPoint.Z) && AreEqual(f.MzPoint2.X, tempPoint2.X) && AreEqual(f.MzPoint2.Z, tempPoint2.Z))
                                        {
                                            //f.TextureIndex2 = texture;
                                            f.TextureCeiling = GetTexture(texture);
                                            f.Ceiling = true;
                                            f.CeilingColor = col;
                                            f.CeilingVertex1 = new TPoint(tem.X, tem.Y);
                                            f.CeilingVertex2 = new TPoint(tem2.X, tem2.Y);
                                            f.CeilingVertex3 = new TPoint(tem3.X, tem3.Y);
                                            f.CeilingVertex4 = new TPoint(tem4.X, tem4.Y);
                                            int mappingIndex = GetMappingIndex(f.CeilingVertex1, f.CeilingVertex2, f.CeilingVertex3, f.CeilingVertex4);
                                            String mode = GetMode(f.CeilingVertex1, f.CeilingVertex2, f.CeilingVertex3, f.CeilingVertex4);
                                            double tileSize = GetTileSize(f.CeilingVertex1, f.CeilingVertex2, f.CeilingVertex3, f.CeilingVertex4,f.MzPoint1,f.MzPoint2,f.MzPoint3,f.MzPoint4);
                                            double aspectRatio = GetAspectRatio(f.CeilingVertex1, f.CeilingVertex2, f.CeilingVertex3, f.CeilingVertex4, f.MzPoint1, f.MzPoint2, f.MzPoint3, f.MzPoint4);
                                            f.AssignInitVals(mode, mappingIndex, tileSize, aspectRatio, false);
                                            break;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                //wall                                
                                tWall = new Wall(scale);
                                //tWall.TextureIndex = texture;
                                tWall.Texture = GetTexture(texture);
                                tWall.Color = col;
                                tWall.MzPoint1 = tempPoint;
                                tWall.MzPoint2 = tempPoint2;
                                tWall.MzPoint3 = tempPoint3;
                                tWall.MzPoint4 = tempPoint4;
                                tWall.Vertex1 = new TPoint(tem.X, tem.Y);
                                tWall.Vertex2 = new TPoint(tem2.X, tem2.Y);
                                tWall.Vertex3 = new TPoint(tem3.X, tem3.Y);
                                tWall.Vertex4 = new TPoint(tem4.X, tem4.Y);
                                
                                double tileSize = GetTileSize(tWall.Vertex1, tWall.Vertex2, tWall.Vertex3, tWall.Vertex4, tWall.MzPoint1, tWall.MzPoint2, tWall.MzPoint3, tWall.MzPoint4);
                                String mode = GetMode(tWall.Vertex1, tWall.Vertex2, tWall.Vertex3, tWall.Vertex4);
                                int mappingIndex = GetMappingIndex(tWall.Vertex1, tWall.Vertex2, tWall.Vertex3, tWall.Vertex4);
                                double aspectRatio = GetAspectRatio(tWall.Vertex1, tWall.Vertex2, tWall.Vertex3, tWall.Vertex4, tWall.MzPoint1, tWall.MzPoint2, tWall.MzPoint3, tWall.MzPoint4);
                                tWall.AssignInitVals(mode, mappingIndex, tileSize,aspectRatio);
                                
                                cWall.Add(tWall);
                            }
                            break;
                        case 1:     //triangle
                            buf = fp.ReadLine();
                            buf = fp.ReadLine();
                            buf = fp.ReadLine();
                            buf = fp.ReadLine();
                            break;
                        case -1:    //texture
                            tab = 0;
                            cmd = 0;
                            while (cmd != -1)
                            {
                                fp.Read();
                                buf = fp.ReadLine();
                                tab = buf.IndexOf('\t');
                                if (tab != -1)
                                {
                                    cmd = int.Parse(buf.Substring(0, tab));
                                    tTex = new Texture(cMazeDirectory, buf.Substring(tab + 1), cmd);
                                    cImages.Add(tTex);
                                }
                                else
                                    break;
                            }
                            break;
                        case -2:    //start pos
                            buf = fp.ReadLine();
                            cStart = new StartPos(scale);
                            parsed = buf.Split('\t');
                            tempPoint = new MPoint();
                            tempPoint.X = double.Parse(parsed[0]);
                            tempPoint.Y = double.Parse(parsed[1]);
                            tempPoint.Z = double.Parse(parsed[2]);
                            cStart.MzPoint = tempPoint;
                            cStart.Angle = int.Parse(parsed[3]);
                            break;
                        case -3:    //end region
                            buf = fp.ReadLine();
                            tEn = new EndRegion(scale);
                            parsed = buf.Split('\t');
                            tEn.MinX = float.Parse(parsed[0]);
                            tEn.MaxX = float.Parse(parsed[1]);
                            tEn.MinZ = float.Parse(parsed[2]);
                            tEn.MaxZ = float.Parse(parsed[3]);
                            tEn.SuccessMessage = parsed[4];
                            if(tab2>1)
                            {
                                buf = fp.ReadLine();
                                parsed = buf.Split('\t');
                                tEn.Height = float.Parse(parsed[0]);
                                tEn.Offset = float.Parse(parsed[1]);
                                tEn.ReturnValue = int.Parse(parsed[2]);
                                //tEn.mode= int.Parse(parsed[3]);
                            }
                            cEndRegions.Add(tEn);
                            
                            //old way
                            //buf = fp.ReadLine();
                            //cEnd = new EndRegion(scale);
                            //parsed = buf.Split('\t');
                            //cEnd.MinX = double.Parse(parsed[0]);
                            //cEnd.MaxX = double.Parse(parsed[1]);
                            //cEnd.MinZ = double.Parse(parsed[2]);
                            //cEnd.MaxZ = double.Parse(parsed[3]);
                            //cEnd.SuccessMessage = parsed[4];
                            break;
                        case -4:    //timeout
                            buf = fp.ReadLine();
                            int p = buf.IndexOf('\t', 1);

                            timeout = double.Parse(buf.Substring(0, p)) / 1000;
                            timeoutMessage = buf.Substring(p + 1);

                            break;
                        case -5:    //designer
                            fp.Read();
                            buf = fp.ReadLine();
                            //buf.CopyTo(0,desginer,0,buf.Length);
                            desginer = buf;
                            break;
                        case -6:    //comment
                            fp.Read();
                            buf = fp.ReadLine();
                            //buf.CopyTo(0,comments,0,buf.Length);
                            comments = buf;
                            break;
                        case -7:    //scale
                            buf = fp.ReadLine();
                            this.Scale = double.Parse(buf);
                            break;
                        case -8:    //move and view speed..
                            buf = fp.ReadLine();
                            tab = buf.IndexOf('\t');
                            this.move = double.Parse(buf.Substring(0,tab));
                            this.view = double.Parse(buf.Substring(tab + 1));
                            break;
                        case -9: //light source..
                            buf = fp.ReadLine();
                            parsed = buf.Split('\t');
                            Light item = new Light(scale);
                            tempPoint = new MPoint();
                            tempPoint.X = double.Parse(parsed[0]);
                            tempPoint.Y = double.Parse(parsed[1]);
                            tempPoint.Z = double.Parse(parsed[2]);

                            if (parsed[3].Contains("0"))
                                item.Type = Light.LightTypes.Torch;

                            item.MzPoint = tempPoint;

                            buf = fp.ReadLine();
                            parsed = buf.Split('\t');

                            item.AmbientColor= Color.FromArgb((int)(255 * double.Parse(parsed[0])), (int)(255 * double.Parse(parsed[1])), (int)(255 * double.Parse(parsed[2])));
                            item.AmbientIntesity = float.Parse(parsed[3]);

                            buf = fp.ReadLine();
                            parsed = buf.Split('\t');
                            item.DiffuseColor=Color.FromArgb((int)(255 * double.Parse(parsed[0])), (int)(255 * double.Parse(parsed[1])), (int)(255 * double.Parse(parsed[2])));
                            item.DiffuseIntesity= float.Parse(parsed[3]);
                            cLight.Add(item);

                            buf = fp.ReadLine();
                            item.Attenuation = double.Parse(buf);

                            break;
                        case -10: //model list
                            tab = 0;
                            cmd = 0;
                            while (cmd != -1)
                            {
                                fp.Read();
                                buf = fp.ReadLine();
                                tab = buf.IndexOf('\t');
                                if (tab != -1)
                                {
                                    cmd = int.Parse(buf.Substring(0, tab));
                                    mMod = new Model(cMazeDirectory, buf.Substring(tab + 1), cmd);
                                    cModels.Add(mMod);
                                }
                                else
                                    break;
                            }
                            break;
                        case 10:  //static model
                            buf = fp.ReadLine();
                            parsed = buf.Split('\t');
                            StaticModel sm = new StaticModel(scale);
                            sm.Model = GetModel(int.Parse(parsed[0]));
                            tempPoint = new MPoint();
                            tempPoint.X = double.Parse(parsed[1]);
                            tempPoint.Y = double.Parse(parsed[2]);
                            tempPoint.Z = double.Parse(parsed[3]);
                            sm.MzPoint = tempPoint;

                            buf = fp.ReadLine();
                            parsed = buf.Split('\t');

                            sm.ModelScale = double.Parse(parsed[0]);
                            tempPoint = new MPoint();
                            tempPoint.X = double.Parse(parsed[1]);
                            tempPoint.Y = double.Parse(parsed[2]);
                            tempPoint.Z = double.Parse(parsed[3]);
                            sm.MzPointRot = tempPoint;

                            buf = fp.ReadLine();
                            parsed = buf.Split('\t');
                            if (parsed[0] == "1")
                                sm.Collision = true;
                            else
                                sm.Collision = false;
                            cStaticModels.Add(sm);
                            break;
                        case 11:  //dynamic model
                            buf = fp.ReadLine();
                            parsed = buf.Split('\t');
                            DynamicModel dm = new DynamicModel(scale);
                            dm.Model = GetModel(int.Parse(parsed[0]));
                            tempPoint = new MPoint();
                            tempPoint.X = double.Parse(parsed[1]);
                            tempPoint.Y = double.Parse(parsed[2]);
                            tempPoint.Z = double.Parse(parsed[3]);
                            dm.MzPoint = tempPoint;

                            buf = fp.ReadLine();
                            parsed = buf.Split('\t');

                            dm.ModelScale = double.Parse(parsed[0]);
                            tempPoint = new MPoint();
                            tempPoint.X = double.Parse(parsed[1]);
                            tempPoint.Y = double.Parse(parsed[2]);
                            tempPoint.Z = double.Parse(parsed[3]);
                            dm.MzPointRot = tempPoint;

                            buf = fp.ReadLine();
                            parsed = buf.Split('\t');
                            if (parsed[0] == "1")
                                dm.Collision = true;
                            else
                                dm.Collision = false;
                            //dm.TriggerType = (DynamicModel.TriggerTypes) int.Parse( parsed[1] );
                            dm.TriggerType = parsed[1];
                            dm.ActiveRadius = double.Parse(parsed[2]);
                            dm.TriggerAction = parsed[3];

                            buf = fp.ReadLine();
                            parsed = buf.Split('\t');
                            dm.EndScale = double.Parse(parsed[0]);
                            tempPoint = new MPoint();
                            tempPoint.X = double.Parse(parsed[1]);
                            tempPoint.Y = double.Parse(parsed[2]);
                            tempPoint.Z = double.Parse(parsed[3]);
                            dm.MzEndPoint = tempPoint - dm.MzPoint;

                            buf = fp.ReadLine();
                            parsed = buf.Split('\t');
                            dm.ActionTime = double.Parse(parsed[0]);
                            tempPoint = new MPoint();
                            tempPoint.X = double.Parse(parsed[1]);
                            tempPoint.Y = double.Parse(parsed[2]);
                            tempPoint.Z = double.Parse(parsed[3]);
                            dm.MzEndRot = tempPoint;

                            buf = fp.ReadLine();
                            parsed = buf.Split('\t');
                            dm.SwitchToModel = GetModel(int.Parse(parsed[0]));
                            dm.HighlightStyle = (DynamicModel.HighLightTypes) int.Parse(parsed[1]);

                            cDynamicModels.Add(dm);
                            break;
                        default:
                            tab2 = Int32.Parse(buf.Substring(tab + 1));
                            for(int i=0;i<tab2;i++)
                                buf=fp.ReadLine();
                            break;
                    }
                }
                catch
                {
                    if (MessageBox.Show("Error in Maze file\n\nDetails\nType: " + cmd + "\nValue: " + buf, "Warning", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Warning) == DialogResult.Abort)
                        break;
                }
            }   
            fp.Close();
            return true;
        }

        public bool ReadFromFile(string inp)
        {
            StreamReader fp = new StreamReader(inp);
            if (fp == null)
            {
                return false;
            }

            cMazeDirectory = inp.Substring(0, inp.LastIndexOf('\\'));
            
            string buf = "";
            bool oldformat = false;
            buf=fp.ReadLine();
            try
            {
                if (buf.Substring(0, 9).CompareTo("Maze File") != 0)
                {
                    oldformat=true;
                }
            }
            catch
            {
                oldformat = true;
            }
            //if (Double.Parse(buf.Substring(15))>1.0)
            //{
            //    //Problem
            //}

            if (oldformat)
            {
                //check if it is old type
                fp.Close();
                fp = new StreamReader(inp);
                char[] init = new char[5];
                fp.Read(init, 0, 5);
                if (init[0] != '-' || init[1] != '1' || init[2] != '\r' || init[3] != '\n' || init[4] != '\t')
                {
                    fp.Close();
                    return false;
                }
                fp.Close();
                return ReadOldFormat(inp);
            }
            SetName(inp);
            return ReadCurrentFormat(ref fp);

        }

        public void ReadColorLine(ref string buf, ref int texture, ref Color a)
        {
            int tab, tab2;
            tab = buf.IndexOf('\t');
            texture = int.Parse(buf.Substring(0, tab));
            tab2 = buf.IndexOf('\t', tab + 1);
            int r, g, b;
            r = (int) (double.Parse(buf.Substring(tab + 1, tab2 - tab)) * 255);
            tab = buf.IndexOf('\t', tab2 + 1);
            g = (int) (double.Parse(buf.Substring(tab2 + 1, tab - tab2))*255);
            b = (int) (double.Parse(buf.Substring(tab + 1))*255);
            a = Color.FromArgb(r, g, b);            
        }
        public void ReadALine(ref string line, ref PointF tem, ref MPoint tempPoint)
        {
            int tab, tab2;
            tab = line.IndexOf('\t');
            tem.X = float.Parse(line.Substring(0, tab));
            tab2 = line.IndexOf('\t', tab + 1);
            tem.Y = float.Parse(line.Substring(tab + 1, tab2 - tab));
            tab = line.IndexOf('\t', tab2 + 1);
            tempPoint.X = double.Parse(line.Substring(tab2 + 1, tab - tab2));
            tab2 = line.IndexOf('\t', tab + 1);
            tempPoint.Y = double.Parse(line.Substring(tab + 1, tab2 - tab));
            tempPoint.Z = double.Parse(line.Substring(tab2 + 1));
        }

        public bool AreEqual(double inp1, double inp2)
        {
            if (Math.Abs(inp1 - inp2) < 0.001)
                return true;
            else
                return false;
        }

        public bool CheckForClosePoint(int x, int y,ref Point ret)
        {
            foreach (Wall w in cWall)
            {
                if (w.CheckClosePoints(x, y,ref ret)==true)
                {
                    return true;
                }
            }
            return false;
        }


        public bool ModifyAllScreenCoordinates(double scale)
        {
            Point p = new Point();
            Rectangle r = new Rectangle();
            foreach (Wall w in cWall)
            {
                
                p.X = (int)Math.Round((w.ScrPoint1.X * scale));
                p.Y = (int)Math.Round((w.ScrPoint1.Y * scale));
                w.ScrPoint1 = p;
                p.X = (int)Math.Round((w.ScrPoint2.X * scale));
                p.Y = (int)Math.Round((w.ScrPoint2.Y * scale));
                w.ScrPoint2 = p;
            }
            foreach (Floor f in cFloor)
            {
                r.X = (int)Math.Round((f.Rect.Left * scale));
                r.Y = (int)Math.Round((f.Rect.Top * scale));
                r.Width = (int)Math.Round(( (f.Rect.Width+f.Rect.X) * scale));
                r.Width -= r.X;
                r.Height = (int)Math.Round(( (f.Rect.Height+f.Rect.Y) * scale));
                r.Height -= r.Y;
                f.Rect = r;
            }
            foreach(Light h in cLight)
            {
                p.X = (int)Math.Round((h.ScrPoint.X * scale));
                p.Y = (int)Math.Round((h.ScrPoint.Y * scale));
                h.ScrPoint = p;
            }
            foreach (StaticModel h in cStaticModels)
            {
                p.X = (int)Math.Round((h.ScrPoint.X * scale));
                p.Y = (int)Math.Round((h.ScrPoint.Y * scale));
                h.ScrPoint = p;
            }
            foreach (DynamicModel h in cDynamicModels)
            {
                p.X = (int)Math.Round((h.ScrPoint.X * scale));
                p.Y = (int)Math.Round((h.ScrPoint.Y * scale));
                h.ScrPoint = p;
            }
            if (cStart != null)
            {
                p.X = (int)(cStart.ScrPoint.X * scale);
                p.Y = (int)(cStart.ScrPoint.Y * scale);
                cStart.ScrPoint = p;
            }
            foreach (EndRegion en in cEndRegions)
            {
                    r.X = (int)Math.Round((en.Rect.Left * scale));
                    r.Y = (int)Math.Round((en.Rect.Top * scale));
                    r.Width = (int)Math.Round(((en.Rect.Width + en.Rect.X) * scale));
                    r.Width -= r.X;
                    r.Height = (int)Math.Round(((en.Rect.Height + en.Rect.Y) * scale));
                    r.Height -= r.Y;
                    en.Rect = r;
               
            }
            //if (cEnd != null)
            //{
            //    r.X = (int)Math.Round((cEnd.Rect.Left * scale));
            //    r.Y = (int)Math.Round((cEnd.Rect.Top * scale));
            //    r.Width = (int)Math.Round(((cEnd.Rect.Width + cEnd.Rect.X) * scale));
            //    r.Width -= r.X;
            //    r.Height = (int)Math.Round(((cEnd.Rect.Height + cEnd.Rect.Y) * scale));
            //    r.Height -= r.Y;
            //    cEnd.Rect = r;
            //}
            return true;
        }

        public bool AutoFixPlacement()
        {
            Point p = new Point();
            int minX=10000, minY=10000;
            Rectangle r = new Rectangle();
            foreach (Wall w in cWall)
            {                
                minX = Math.Min(minX, w.ScrPoint1.X);
                minY = Math.Min(minY, w.ScrPoint1.Y);
                minX = Math.Min(minX, w.ScrPoint2.X);
                minY = Math.Min(minY, w.ScrPoint2.Y);
            }
            foreach (Floor f in cFloor)
            {
                minX = Math.Min(minX, f.Rect.X);
                minY = Math.Min(minY, f.Rect.Y);
            }
            foreach (Light l in cLight)
            {
                minX = Math.Min(minX, l.ScrPoint.X);
                minY = Math.Min(minY, l.ScrPoint.Y);
            }
            foreach (StaticModel f in cStaticModels)
            {
                minX = Math.Min(minX, f.ScrPoint.X);
                minY = Math.Min(minY, f.ScrPoint.Y);
            }
            foreach (DynamicModel f in cDynamicModels)
            {
                minX = Math.Min(minX, f.ScrPoint.X);
                minY = Math.Min(minY, f.ScrPoint.Y);
            }
            if (cStart != null)
            {
                minX = Math.Min(minX, cStart.ScrPoint.X);
                minY = Math.Min(minY, cStart.ScrPoint.Y);
            }
            foreach (EndRegion en in cEndRegions)
            {
                minX = Math.Min(minX, en.Rect.X);
                minY = Math.Min(minY, en.Rect.Y);
            }
            //if (cEnd != null)
            //{
            //    minX = Math.Min(minX, cEnd.Rect.X);
            //    minY = Math.Min(minY, cEnd.Rect.Y);
            //}

            minX -= 10;
            minY -= 10;
            foreach (Wall w in cWall)
            {

                p.X = w.ScrPoint1.X - minX;
                p.Y = w.ScrPoint1.Y - minY;
                w.ScrPoint1 = p;
                p.X = w.ScrPoint2.X - minX;
                p.Y = w.ScrPoint2.Y - minY;
                w.ScrPoint2 = p;
            }
            foreach (Floor f in cFloor)
            {
                r.X = f.Rect.Left - minX;
                r.Y = f.Rect.Top - minY;
                r.Width = f.Rect.Width;
                r.Height = f.Rect.Height;
                f.Rect = r;
            }
            foreach (StaticModel s in cStaticModels)
            {
                p.X = s.ScrPoint.X - minX;
                p.Y = s.ScrPoint.Y - minY;
                s.ScrPoint = p;
            }
            foreach (DynamicModel d in cDynamicModels)
            {
                p.X = d.ScrPoint.X - minX;
                p.Y = d.ScrPoint.Y - minY;
                d.ScrPoint = p;
            }
            foreach (Light l in cLight)
            {
                p.X = l.ScrPoint.X - minX;
                p.Y = l.ScrPoint.Y - minY;
                l.ScrPoint = p;
            }
            if (cStart != null)
            {
                p.X = cStart.ScrPoint.X - minX;
                p.Y = cStart.ScrPoint.Y - minY;
                cStart.ScrPoint = p;
            }
            foreach (EndRegion en in cEndRegions)
            {
                r.X = en.Rect.Left - minX;
                r.Y = en.Rect.Top - minY;
                r.Width = en.Rect.Width;
                r.Height = en.Rect.Height;
                en.Rect = r;
            }
            //if (cEnd != null)
            //{
            //    r.X = cEnd.Rect.Left - minX;
            //    r.Y = cEnd.Rect.Top - minY;
            //    r.Width = cEnd.Rect.Width;
            //    r.Height = cEnd.Rect.Height;
            //    cEnd.Rect = r;
            //}

            return true;

        }

        public void FinalCheckBeforeWrite()
        {
            CheckForRemovedTextures();
            CheckForRemovedModels();
        }

        //public void FinalCheckBeforeWrite()
        //{
        //    int corrections = 0;
        //    //check that all texture indices are available range...
        //    foreach (Wall w in cWall)
        //    {
        //        if (w.TextureIndex > cImages.Count)
        //        {
        //            w.TextureIndex = 0;
        //            corrections++;
        //        }
        //    }
        //    foreach (Floor f in cFloor)
        //    {
        //        if (f.TextureIndex> cImages.Count)
        //        {
        //            f.TextureIndex = 0;
        //            corrections++;
        //        }
        //        if (f.TextureIndex2 > cImages.Count)
        //        {
        //            f.TextureIndex2 = 0;
        //            corrections++;
        //        }
        //    }
        //    if(corrections>1)
        //        MessageBox.Show("There were " + corrections.ToString() + " corrections made in the design!..");
        //    else if(corrections==1)
        //        MessageBox.Show("There was " + corrections.ToString() + " correction made in the design!..");            
        //}

        public void SetName(String inp)
        {
            filename = inp;
            int i1=0,i2 = 0;
            i1 = inp.LastIndexOf('\\');
            i2 = inp.LastIndexOf('.');
            name = inp.Substring(i1+1, i2 - i1-1);
        }

        public string GetName()
        {
            return filename;
        }
        //For access members...
        static public Maze mzP;
        static public List<Texture> GetImages()
        {
            return mzP.cImages;
        }

        static public List<Model> GetModels()
        {
            return mzP.cModels;
        }
        private int GetMappingIndex(TPoint V1,TPoint V2,TPoint V3,TPoint V4)
        {
            if (V3.X == 0 && V3.Y == 0)
                return 3;
            else if (V2.X == 0 && V2.Y == 0)
                return 2;
            else if (V1.X == 0 && V1.Y == 0)
                return 1;
            else if (V4.X == 0 && V4.Y == 0)
                return 4;
            else return 3;
        }
        private double GetTileSize(TPoint V1,TPoint V2,TPoint V3,TPoint V4,MPoint M1,MPoint M2,MPoint M3,MPoint M4)
        {
            double h = (Math.Sqrt(Math.Pow((M1.X - M2.X), 2) + Math.Pow((M1.Y - M2.Y), 2) + Math.Pow((M1.Z - M2.Z), 2)) + Math.Sqrt(Math.Pow((M3.X - M4.X), 2) + Math.Pow((M3.Y - M4.Y), 2) + Math.Pow((M3.Z - M4.Z), 2))) / 2;
            double tileSize = h/ Math.Max(Math.Max(V1.Y, V2.Y), Math.Max(V3.Y, V4.Y));
            if (tileSize > 0) return tileSize;
            else return 1;
        }
        private double GetAspectRatio(TPoint V1, TPoint V2, TPoint V3, TPoint V4, MPoint M1, MPoint M2, MPoint M3, MPoint M4)
        {
            double l = (Math.Sqrt(Math.Pow((M1.X - M4.X), 2) + Math.Pow((M1.Y - M4.Y), 2) + Math.Pow((M1.Z - M4.Z), 2)) + Math.Sqrt(Math.Pow((M2.X - M3.X), 2) + Math.Pow((M2.Y - M3.Y), 2) + Math.Pow((M2.Z - M3.Z), 2))) / 2;
            double h = (Math.Sqrt(Math.Pow((M1.X - M2.X), 2) + Math.Pow((M1.Y - M2.Y), 2) + Math.Pow((M1.Z - M2.Z), 2)) + Math.Sqrt(Math.Pow((M3.X - M4.X), 2) + Math.Pow((M3.Y - M4.Y), 2) + Math.Pow((M3.Z - M4.Z), 2))) / 2;
            double tileSize1 = l / Math.Max(Math.Max(V1.X, V2.X), Math.Max(V3.X, V4.X));
            double tileSize2=h / Math.Max(Math.Max(V1.Y, V2.Y), Math.Max(V3.Y, V4.Y));
            double aspectRatio=tileSize1/tileSize2;
            if (aspectRatio > 0)
                return aspectRatio;
            else return 1;
        }
        private string GetMode(TPoint V1, TPoint V2, TPoint V3, TPoint V4)
        {
            if (V3.X == 1 && V3.Y == 1)
                return "Stretch";
            else if (V2.X == 1 && V2.Y == 1)
                return "Stretch";
            else if (V1.X == 1 && V1.Y == 1)
                return "Stretch";
            else if (V4.X == 1 && V4.Y == 1)
                return "Stretch";
            else return "Tile";
        }

        public void CheckForRemovedTextures()
        {
            foreach (Wall w in cWall)
            {
                if(cImages.Contains(w.Texture)==false)                    
                {
                    w.Texture = null;
                }
            }

            foreach (Floor w in cFloor)
            {
                if (cImages.Contains(w.TextureFloor) == false)
                {
                    w.TextureFloor = null;
                }
                if (cImages.Contains(w.TextureCeiling) == false)
                {
                    w.TextureCeiling = null;
                }
            }
        }

        public void CheckForRemovedModels()
        {
            foreach (StaticModel s in cStaticModels)
            {
                if( cModels.Contains(s.Model)==false)
                {
                    s.Model = null;
                }
            }
            foreach (DynamicModel s in cDynamicModels)
            {
                if (cModels.Contains(s.Model) == false)
                {
                    s.Model = null;
                }
                if(cModels.Contains(s.SwitchToModel)==false)
                {
                    s.SwitchToModel = null;
                }
            }
        }



//necessary to find bar...
        public double minY, maxY;
        public double minX, maxX;

        public void SetMinMaxValues()
        {
            minY = 0;
            minX = 0;
            maxX = 0;
            minX = 0;


            foreach (Floor f in cFloor)
            {
                if (f.Rect.Left < minX)
                    minX = f.Rect.Left;
                else if (f.Rect.Right > maxX)
                    maxX = f.Rect.Right;

                if (f.Rect.Top < minY)
                    minY = f.Rect.Top;
                else if (f.Rect.Bottom > maxY)
                    maxY = f.Rect.Bottom;
            }

            foreach (Wall l in cWall)
            {
                if (l.ScrPoint1.X < minX)
                    minX = l.ScrPoint1.X;
                else if (l.ScrPoint1.X > maxX)
                    maxX = l.ScrPoint1.X;

                if (l.ScrPoint2.X < minX)
                    minX = l.ScrPoint2.X;
                else if (l.ScrPoint2.X > maxX)
                    maxX = l.ScrPoint2.X;

                if (l.ScrPoint1.Y < minY)
                    minY = l.ScrPoint1.Y;
                else if (l.ScrPoint1.Y > maxY)
                    maxY = l.ScrPoint1.Y;

                if (l.ScrPoint2.Y < minY)
                    minY = l.ScrPoint2.Y;
                else if (l.ScrPoint2.Y > maxY)
                    maxY = l.ScrPoint2.Y;
            }

            foreach (Light li in cLight)
            {
                if (li.ScrPoint.X < minX)
                    minX = li.ScrPoint.X;
                else if (li.ScrPoint.X > maxX)
                    maxX = li.ScrPoint.X;
                
                if (li.ScrPoint.Y < minY)
                    minY = li.ScrPoint.Y;
                else if (li.ScrPoint.Y > maxY)
                    maxY = li.ScrPoint.Y;                
            }

            foreach (StaticModel li in cStaticModels)
            {
                if (li.ScrPoint.X < minX)
                    minX = li.ScrPoint.X;
                else if (li.ScrPoint.X > maxX)
                    maxX = li.ScrPoint.X;

                if (li.ScrPoint.Y < minY)
                    minY = li.ScrPoint.Y;
                else if (li.ScrPoint.Y > maxY)
                    maxY = li.ScrPoint.Y;
            }

            foreach (DynamicModel li in cDynamicModels)
            {
                if (li.ScrPoint.X < minX)
                    minX = li.ScrPoint.X;
                else if (li.ScrPoint.X > maxX)
                    maxX = li.ScrPoint.X;

                if (li.ScrPoint.Y < minY)
                    minY = li.ScrPoint.Y;
                else if (li.ScrPoint.Y > maxY)
                    maxY = li.ScrPoint.Y;
            }

            //check start pos
            if (this.cStart.ScrPoint.X < minX)
                minX = this.cStart.ScrPoint.X;
            else if (this.cStart.ScrPoint.X > maxX)
                maxX = this.cStart.ScrPoint.X;

            if (this.cStart.ScrPoint.Y < minY)
                minY = this.cStart.ScrPoint.Y;
            else if (this.cStart.ScrPoint.Y > maxY)
                maxY = this.cStart.ScrPoint.Y;

            //check end region
            
            //maxX += 40;
            //maxY += 40;
        }




    }
}
