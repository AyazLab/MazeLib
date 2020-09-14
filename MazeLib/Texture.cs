#region Using directives

using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.ComponentModel;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.Windows.Forms;
using System.Xml;
#endregion

namespace MazeMaker
{
    //[TypeConverterAttribute(typeof(TextureConverter)), DescriptionAttribute("Texture Support")]
    [Serializable]
    public class Texture
    {
        
        public Texture()
        {
            OpenFileDialog a = new OpenFileDialog();
            a.Filter = "Image File (*.bmp,*.jpg,*.jpeg,*.gif,*.png)|*.bmp;*.jpg;*.jpeg;*.gif;*.png";
            a.FilterIndex = 1;
            a.RestoreDirectory = true;
            a.Multiselect = false;
            String ext,fname;
            bool edited = false;
            if (a.ShowDialog() == DialogResult.OK)
            {

                if (a.FileName.Length > 0)
                {
                    ext = a.FileName.Substring(a.FileName.LastIndexOf(".") + 1).ToLower();    //record extension in case..        
                    Image img = Bitmap.FromFile((string)a.FileName);
                    fname = a.FileName;
                    filePath = a.FileName;
                    if (ext != "jpg" && ext != "jpeg" && ext != "tga" && ext != "bmp" && ext != "png")  //Check if image is bitmap..
                    {
                        edited = true;
                    }
                    if (edited)
                    {
                        //this.Image = new Bitmap(img, newDim,newDim);
                        fname += "_edited.jpg";
                        //this.Image.Save(a.FileName,ImageFormat.Jpeg);
                        img.Save(fname, ImageFormat.Jpeg);
                        MessageBox.Show("Uploaded is converted to the following compatible file:\n" + fname, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    }
                    else
                    {
                        this.Image = img;
                    }

                    int index = fname.LastIndexOf('\\');
                    if (index == -1)
                        this.name = fname;
                    else
                        this.name = fname.Substring(index + 1);
                    //this.index = TextureCounter.GetIndex();
                    this.index = 0;
                }                               
            }
            else
            {             
                throw new Exception("No selection");                
            }
        }

        private bool edited = false;
        public Texture(string dir)
        {
            OpenFileDialog a = new OpenFileDialog();
            a.Filter = "Supported Image Files |*.bmp;*.jpg;*.jpeg;*.gif;*png";
            a.FilterIndex = 1;
            a.RestoreDirectory = true;
            a.Multiselect = false;
            a.InitialDirectory = dir;
            String ext, fname;
            
            if (a.ShowDialog() == DialogResult.OK)
            {

                if (a.FileName.Length > 0)
                {
                    ext = a.FileName.Substring(a.FileName.LastIndexOf(".") + 1).ToLower();    //record extension in case..        
                    Image img = Bitmap.FromFile((string)a.FileName);
                    fname = a.FileName;
                    filePath = a.FileName;
                    if (ext != "jpg" && ext != "jpeg" && ext!="png" && ext != "tga" && ext != "bmp")  //Check if image is bitmap..
                    {
                        edited = true;
                    }
                    if (edited)
                    {
                        //this.Image = new Bitmap(img, newDim,newDim);
                        fname += "_edited.jpg";
                        //this.Image.Save(a.FileName,ImageFormat.Jpeg);
                        img.Save(fname, ImageFormat.Jpeg);
                        MessageBox.Show("Uploaded is converted to the following compatible file:\n" + fname, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.Image = img;
                    }
                    else
                    {
                        this.Image = img;
                    }

                    int index = fname.LastIndexOf('\\');
                    if (index == -1)
                        this.name = fname;
                    else
                        this.name = fname.Substring(index + 1);
                    //this.index = TextureCounter.GetIndex();
                    this.index = 0;
                }
            }
            else
            {
                throw new Exception("No selection");                
            }
        }

        public Texture(string dir, string inpName, int fIndex)
        {
            name = inpName;
            filePath = dir + "\\" + inpName;
            if (dir == "")
                filePath = inpName;
            index = fIndex;
            //if(dir == null)
            //{
            //    Name = "[ none ]";
            //    this.Image = MazeLib.Properties.Resources.delete; ;
            //}
            if (File.Exists(dir + "\\" + inpName))
            {
                try
                {
                    img = Bitmap.FromFile(dir + "\\" + inpName);
                    filePath = dir + "\\" + inpName;
                    //this.img = new Bitmap(AppDomain.CurrentDomain.BaseDirectory + "\\Texture\\" + inpName);            
                    return;
                }
                catch
                { }
            }
            else if (File.Exists(Settings.userLibraryFolder + "\\" + inpName))
            {
                try
                {
                    img = Bitmap.FromFile(Settings.userLibraryFolder + "\\" + inpName);
                    filePath = Settings.userLibraryFolder + "\\" + inpName;
                    //this.img = new Bitmap(AppDomain.CurrentDomain.BaseDirectory + "\\Texture\\" + inpName);            
                    return;
                }
                catch
                { }
            }
            else if (File.Exists(Settings.standardLibraryFolder + "\\" + inpName))
            {
                try
                {
                    img = Bitmap.FromFile(Settings.standardLibraryFolder + "\\" + inpName);
                    filePath = Settings.standardLibraryFolder + "\\" + inpName;
                    //this.img = new Bitmap(AppDomain.CurrentDomain.BaseDirectory + "\\Texture\\" + inpName);            
                    return;
                }
                catch
                { }
            }
            else
            { img = MazeLib.Properties.Resources.delete; }
        }

        string filePath = "";
        [Category("File Information")]
        [Description("File Path of the Texture Image")]
        [DisplayName("File Path")]
        [ReadOnly(true)]
        public string FilePath
        {
            get { return filePath; }
            set { filePath = value; }
        }

        string name = "";
        [Category("File Information")]
        [Description("Name of the Texture Image")]
        [ReadOnly(true)]
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        private Image img=null;
        [Category("Options")]
        [Description("...")]
        [Browsable(false)]
        public Image Image
        {
            get { return img; }
            set { img = value; }
        }

        private int index=0;
        [Category("Options")]
        [Description("...")]
        [Browsable(false)]
        public int Index
        {
            get { return index; }
            set {
                //can go through all planes to change their old index
                index = value; 
            
            }
        }
        
        private bool skybox;
        [Category("Options")]
        [Description("Identify this image as skybox")]
        [Browsable(false)]
        public bool Skybox
        {
            get { return skybox; }
            set
            {
                skybox = value;
                edited = true;
                //int ret = TextureCounter.SetSkyIndex(index);
                //if (ret>0)
                //{
                //    skybox = value;
                //    index = ret;
                //}
                //else
                //{
                //    //MessageBox.Show("A skybox is already set. Please first set it to false before enabling this!", "MazeMaker", MessageBoxButtons.OK, MessageBoxIcon.Question);                   
                //}
            }
        }

        public static int GetMappingIndex(TPoint V1, TPoint V2, TPoint V3, TPoint V4)
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

        public static double GetTileSize(TPoint V1, TPoint V2, TPoint V3, TPoint V4, MPoint M1, MPoint M2, MPoint M3, MPoint M4)
        {
            double h = (Math.Sqrt(Math.Pow((M1.X - M2.X), 2) + Math.Pow((M1.Y - M2.Y), 2) + Math.Pow((M1.Z - M2.Z), 2)) + Math.Sqrt(Math.Pow((M3.X - M4.X), 2) + Math.Pow((M3.Y - M4.Y), 2) + Math.Pow((M3.Z - M4.Z), 2))) / 2;
            double tileSize = h / Math.Max(Math.Max(V1.Y, V2.Y), Math.Max(V3.Y, V4.Y));
            if (tileSize > 0) return tileSize;
            else return 1;
        }
        
        public static double GetAspectRatio(TPoint V1, TPoint V2, TPoint V3, TPoint V4, MPoint M1, MPoint M2, MPoint M3, MPoint M4)
        {
            double l = (Math.Sqrt(Math.Pow((M1.X - M4.X), 2) + Math.Pow((M1.Y - M4.Y), 2) + Math.Pow((M1.Z - M4.Z), 2)) + Math.Sqrt(Math.Pow((M2.X - M3.X), 2) + Math.Pow((M2.Y - M3.Y), 2) + Math.Pow((M2.Z - M3.Z), 2))) / 2;
            double h = (Math.Sqrt(Math.Pow((M1.X - M2.X), 2) + Math.Pow((M1.Y - M2.Y), 2) + Math.Pow((M1.Z - M2.Z), 2)) + Math.Sqrt(Math.Pow((M3.X - M4.X), 2) + Math.Pow((M3.Y - M4.Y), 2) + Math.Pow((M3.Z - M4.Z), 2))) / 2;
            double tileSize1 = l / Math.Max(Math.Max(V1.X, V2.X), Math.Max(V3.X, V4.X));
            double tileSize2 = h / Math.Max(Math.Max(V1.Y, V2.Y), Math.Max(V3.Y, V4.Y));
            double aspectRatio = tileSize1 / tileSize2;
            if (aspectRatio > 0)
                return aspectRatio;
            else return 1;
        }

        public static string GetMode(TPoint V1, TPoint V2, TPoint V3, TPoint V4)
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

        public XmlElement toXMLnode(XmlDocument doc)
        {
            XmlElement texNode = doc.CreateElement(string.Empty, "Image", string.Empty);
            texNode.SetAttribute("id", this.Index.ToString());
            texNode.SetAttribute("file", this.Name);

            return texNode;
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public class RotationConverter:ExpandableObjectConverter
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
            return new StandardValuesCollection (new string[] { "0°", "90°", "180°","270°" });
        }
       
    }

    public class StretchConverter : ExpandableObjectConverter
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
            return new StandardValuesCollection(new string[] { "Stretch","Tile" });
        }
    }

    //public class LightConverter : ExpandableObjectConverter
    //{
    //    public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
    //    {
    //        //true means show a combobox
    //        return true;
    //    }

    //    public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
    //    {
    //        //true will limit to list. false will show the list, but allow free-form entry
    //        return true;
    //    }

    //    public override
    //    System.ComponentModel.TypeConverter.StandardValuesCollection
    //    GetStandardValues(ITypeDescriptorContext context)
    //    {
    //        return new StandardValuesCollection(new string[] { "Torch", "Bulb" });
    //    }

    //}

    //public class TextureConverter : ExpandableObjectConverter
    //{
    //    public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
    //    {
    //        //true means show a combobox
    //        return true;
    //    }
    //    public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
    //    {
    //        //true will limit to list. false will show the list, but allow free-form entry
    //        return true;
    //    }

    //    public override
    //    System.ComponentModel.TypeConverter.StandardValuesCollection
    //    GetStandardValues(ITypeDescriptorContext context)
    //    {
    //        List<Texture> theArray = new List<Texture>();
    //        //theArray.Add(new Texture(null,null,0));
    //        theArray.Add(null);
    //        List<Texture> list = MazeMaker.Maze.GetImages();
    //        foreach (Texture t in list)
    //        {
    //            theArray.Add(t);
    //        }
    //        //return new StandardValuesCollection(MazeMaker.Maze.GetImages().ToArray());
    //        return new StandardValuesCollection(theArray.ToArray());
    //        //return new StandardValuesCollection(new string[] { "entry1", "entry2", "entry3" });
    //    }

    //    public override bool CanConvertTo(ITypeDescriptorContext context, System.Type destinationType)
    //    {
    //        if (destinationType == typeof(Texture))
    //            return true;
    //        return base.CanConvertTo(context, destinationType);
    //    }

    //    public override bool CanConvertFrom(ITypeDescriptorContext context, System.Type sourceType)
    //    {
    //        if (sourceType == typeof(String))
    //            return true;

    //        return base.CanConvertFrom(context, sourceType);
    //    }

    //    public override object ConvertTo(ITypeDescriptorContext context,
    //                           CultureInfo culture,
    //                           object value,
    //                           System.Type destinationType)
    //    {
    //        if (destinationType == typeof(System.String) &&
    //             value is Texture)
    //        {
    //            Texture so = (Texture)value;
    //            return so.name;
    //        }
    //        return base.ConvertTo(context, culture, value, destinationType);
    //    }

    //    public override object ConvertFrom(ITypeDescriptorContext context,
    //                          CultureInfo culture, object value)
    //    {
    //        if (value is string)
    //        {
    //            try
    //            {
    //                //if (value == "[ none ]")
    //                if (value == null || (string)value == "")
    //                    return null;
    //                List<Texture> cList = Maze.GetImages();
    //                foreach (Texture t in cList)
    //                {
    //                    if (t.Name.CompareTo(value) == 0)
    //                        return t;
    //                }
    //            }
    //            catch
    //            {
    //                throw new ArgumentException("Can not convert '" + (string)value + "' to type Texture");
    //            }
    //            return value;
    //        }
    //        return base.ConvertFrom(context, culture, value);
    //    }
    //}

    //public class TextureCounter
    //{
    //    static private int curIndex = 99;
    //    static public int GetIndex()
    //    {
    //        curIndex++;
    //        foreach( Texture t in Maze.mzP.cImages)
    //        {
    //            if (t.Index >= curIndex)
    //            {
    //                curIndex = t.Index + 1;
    //            }
    //        }
    //        return curIndex;
    //    }
    //    /*
    //    static private int skyIndex = 0;
    //    static public int SetSkyIndex(int inp)
    //    {
    //        if (skyIndex == 0)
    //        {
    //            skyIndex = inp;
    //            return 900;
    //        }
    //        else if (inp == 900)
    //        {
    //            int a = skyIndex;
    //            skyIndex = 0;
    //            return a;
    //        }
    //        else
    //        {
    //            return 0;
    //        }
    //    } */

    //    static public void Reset()
    //    {
    //        curIndex = 99;
    //        //skyIndex = 0;
    //    }
    //}
}
