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

namespace MazeMaker
{
    //[TypeConverterAttribute(typeof(ModelConverter)), DescriptionAttribute("Model Support")]
    [Serializable]
    public class Model
    {
        public Model()
        {
            OpenFileDialog a = new OpenFileDialog();
            a.Filter = "Supported Model Files |*.obj";
            a.FilterIndex = 1;
            a.RestoreDirectory = true;
            if (a.ShowDialog() == DialogResult.OK)
            {                
                this.name = a.FileName;
                filePath = name;
                int index = a.FileName.LastIndexOf('\\');
                if (index == -1)
                    this.name = a.FileName;
                else
                    this.name = a.FileName.Substring(index + 1);  
                //this.index = ModelCounter.GetIndex();
                this.index = 0;

                string preview_str = a.FileName.Replace(".obj", "_preview.jpg");
                if(Tools.FileExists(preview_str))
                {
                    this.img = Image.FromFile(preview_str);
                }
            }
            else
            {
                //no selection..
                throw new Exception("No selection"); 
            }
        }

        public Model(String dir)
        {
            OpenFileDialog a = new OpenFileDialog();
            a.Filter = "Supported Model Files |*.obj";
            a.FilterIndex = 1;
            a.RestoreDirectory = true;
            a.InitialDirectory = dir;
            if (a.ShowDialog() == DialogResult.OK)
            {
                this.name = a.FileName;
                filePath = name;
                int index = a.FileName.LastIndexOf('\\');
                if (index == -1)
                    this.name = a.FileName;
                else
                    this.name = a.FileName.Substring(index + 1);
                //this.index = ModelCounter.GetIndex();
                this.index = 0;

                string preview_str = a.FileName.Replace(".obj", "_preview.jpg");
                if (Tools.FileExists(preview_str))
                {
                    this.img = Image.FromFile(preview_str);
                }
            }
            else
            {
                //no selection..
                throw new Exception("No selection"); 
            }
        }

        public Model(string dir, string inpName, int fIndex)
        {
            name = inpName;
            filePath = dir + "\\" + inpName;
            index = fIndex;

            string preview_name = inpName.Replace(".obj", "_preview.jpg");

            try
            {
                img = Bitmap.FromFile(dir + "\\" + preview_name);
                filePath = dir + "\\" + preview_name;
            }
            catch
            {


                try
                {
                    img = Bitmap.FromFile(Settings.userLibraryFolder + "\\" + preview_name);
                    filePath = Settings.userLibraryFolder + "\\" + preview_name;
                }
                catch// (System.Exception ex)
                {

                    try
                    {
                        img = Bitmap.FromFile(Settings.standardLibraryFolder + "\\" + preview_name);
                        filePath = Settings.standardLibraryFolder + "\\" + preview_name;
                    }
                    catch// (System.Exception ex)
                    {

                    }

                }

            }

        }

        public string name = "";
        [Category("Options")]
        [Description("Name of the Static Model")]
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public string filePath = "";
        [Browsable(false)]
        public string FilePath
        {
            get { return filePath; }
            set { filePath = value; }
        }

        private Image img = null;
        [Category("Options")]
        [Description("Preview image of the Model")]
        public Image Image
        {
            get { return img; }
            set { img = value; }
        }

        //private String file = null;
        //[Category("Options")]
        //[Description("...")]
        //public String File
        //{
        //    get { return file; }
        //    set { file = value; }
        //}

        private int index;
        [Category("Options")]
        [Description("...")]
        public int Index
        {
            get { return index; }
            set
            {
                //can go through all planes to change their old index
                index = value;

            }
        }

        public XmlElement toXMLnode(XmlDocument doc)
        {
            XmlElement texNode = doc.CreateElement(string.Empty, "Model", string.Empty);
            texNode.SetAttribute("id", this.Index.ToString());
            texNode.SetAttribute("file", this.Name);

            return texNode;
        }
    }

    //public class ModelConverter : ExpandableObjectConverter
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

    //    public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
    //    {
    //        List<Model> theArray = new List<Model>();
    //        //theArray.Add(new Texture(null,null,0));
    //        theArray.Add(null);
    //        List<Model> list = MazeMaker.Maze.GetModels();
    //        foreach (Model t in list)
    //        {
    //            theArray.Add(t);
    //        }
    //        return new StandardValuesCollection(theArray.ToArray());
    //        //return new StandardValuesCollection(MazeMaker.Maze.GetModels().ToArray());
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

    //    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, System.Type destinationType)
    //    {
    //        if (destinationType == typeof(string) && value is Model)
    //        {
    //            Model so = (Model)value;
    //            return so.name;
    //        }
    //        return base.ConvertTo(context, culture, value, destinationType);
    //    }

    //    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
    //    {
    //        if (value is string)
    //        {
    //            try
    //            {
    //                if (value == null || (string)value == "")
    //                    return null;
    //                List<Model> cList = Maze.GetModels();
    //                foreach (Model t in cList)
    //                {
    //                    if (t.Name.CompareTo(value) == 0)
    //                        return t;
    //                }
    //            }
    //            catch
    //            {
    //                throw new ArgumentException("Can not convert '" + (string)value + "' to type StaticModel");
    //            }
    //            return value;
    //        }
    //        return base.ConvertFrom(context, culture, value);
    //    }
    //}

    //public class ModelCounter
    //{
    //    static int curIndex = 99;
    //    static public int GetIndex()
    //    {
    //        curIndex++;
    //        foreach (Model t in Maze.mzP.cModels)
    //        {
    //            if (t.Index >= curIndex)
    //            {
    //                curIndex = t.Index + 1;
    //            }
    //        }
    //        return curIndex;
    //    }
    //}

    public class HighLightConverter : ExpandableObjectConverter
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
            return new StandardValuesCollection(new string[] { "Bounce", "Shake", "Rotate", "Glow Red", "Glow Green", "Glow Blue", "Particle Color 1", "Particle Color 2", "Particle Color 3", "Falling Particle Color 1", "Falling Particle Color 2", "Falling Particle Color 3" });
        }

    }
    public class EventActionConverter : ExpandableObjectConverter
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
            return new StandardValuesCollection(new string[] { "No Action", "Move/Rotate", "Change Model", "Destroy Model" });
        }

    }
    public class TriggerTypeConverter : ExpandableObjectConverter
    {
        public static string[] options = new string[] { "None", "Proximity", "Time", "Interact", "Proximity and Time", "Proximity and Interact", "Interact and Time", "Proximity, Interact, and Time" };

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
            return new StandardValuesCollection(options);
        }

    }

    public class HighlightTypeConverter : ExpandableObjectConverter
    {
        public static string[] options = new string[] { "Skip", "Proximity", "Time", "Proximity and Time" };

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
            return new StandardValuesCollection(options);
        }

    }
}
