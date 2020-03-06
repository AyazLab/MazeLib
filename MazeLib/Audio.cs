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
    [TypeConverterAttribute(typeof(AudioConverter)), DescriptionAttribute("Audio Support")]
    [Serializable]
    public class Audio
    {
        public Audio()
        {
            OpenFileDialog a = new OpenFileDialog();
            a.Filter = "Supported Audio Files |*.wav;*.mp3";
            a.FilterIndex = 1;
            a.RestoreDirectory = true;
            if (a.ShowDialog() == DialogResult.OK)
            {                
                this.name = a.FileName;               
                int index = a.FileName.LastIndexOf('\\');
                if (index == -1)
                    this.name = a.FileName;
                else
                    this.name = a.FileName.Substring(index + 1);  
                this.index = AudioCounter.GetIndex();

                //string preview_str = a.FileName.Replace(".obj", "_preview.jpg");
                //if(Tools.FileExists(preview_str))
                //{
                //    this.img = Image.FromFile(preview_str);
                //}
            }
            else
            {
                //no selection..
                throw new Exception("No selection"); 
            }

              
        }
        public Audio(String dir)
        {
            OpenFileDialog a = new OpenFileDialog();
            a.Filter = "Supported Audio Files |*.wav;*.mp3";
            a.FilterIndex = 1;
            a.RestoreDirectory = true;
            a.InitialDirectory = dir;
            if (a.ShowDialog() == DialogResult.OK)
            {
                this.name = a.FileName;
                int index = a.FileName.LastIndexOf('\\');
                if (index == -1)
                    this.name = a.FileName;
                else
                    this.name = a.FileName.Substring(index + 1);
                this.index = AudioCounter.GetIndex();

                //string preview_str = a.FileName.Replace(".obj", "_preview.jpg");
                //if (Tools.FileExists(preview_str))
                //{
                //    this.img = Image.FromFile(preview_str);
                //}
            }
            else
            {
                //no selection..
                throw new Exception("No selection"); 
            }


        }
        public Audio(string dir, string inpName, int fIndex)
        {
            name = inpName;
            index = fIndex;

            /*
            string preview_name = inpName.Replace(".obj", "_preview.jpg");

            try
            {
               // img = Bitmap.FromFile(dir + "\\" + preview_name);                             
            }
            catch
            {


                try
                {
                    img = Bitmap.FromFile(Settings.userLibraryFolder + "\\" + preview_name);
                }
                catch// (System.Exception ex)
                {

                    try
                    {
                        img = Bitmap.FromFile(Settings.standardLibraryFolder + "\\" + preview_name);
                    }
                    catch// (System.Exception ex)
                    {

                    }

                }

            }
             * */

        }

        public string name = "";
        [Category("Options")]
        [Description("Name of the Audio Item")]
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        //private Image img = null;
        //[Category("Options")]
        //[Description("Preview image of the Model")]
        //public Image Image
        //{
        //    get { return img; }
        //    set { img = value; }
        //}

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
            XmlElement texNode = doc.CreateElement(string.Empty, "Sound", string.Empty);
            texNode.SetAttribute("id", this.Index.ToString());
            texNode.SetAttribute("file", this.Name);

            return texNode;
        }
    }

    public class AudioConverter : ExpandableObjectConverter
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
            List<Audio> theArray = new List<Audio>();
            //theArray.Add(new Texture(null,null,0));
            theArray.Add(null);
            List<Audio> list = MazeMaker.Maze.GetAudios();
            foreach (Audio t in list)
            {
                theArray.Add(t);
            }
            return new StandardValuesCollection(theArray.ToArray());
            //return new StandardValuesCollection(MazeMaker.Maze.GetModels().ToArray());
            //return new StandardValuesCollection(new string[] { "entry1", "entry2", "entry3" });
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, System.Type destinationType)
        {
            if (destinationType == typeof(Texture))
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
                 value is Audio)
            {
                Audio so = (Audio)value;
                return so.name;
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
        public override object ConvertFrom(ITypeDescriptorContext context,
                              CultureInfo culture, object value)
        {
            if (value is string)
            {
                try
                {
                    if (value == null || (string)value == "")
                        return null;
                    List<Audio> cList = Maze.GetAudios();
                    foreach (Audio t in cList)
                    {
                        if (t.Name.CompareTo(value) == 0)
                            return t;
                    }
                }
                catch
                {
                    throw new ArgumentException("Can not convert '" + (string)value + "' to type Audio Item");
                }
                return value;
            }
            return base.ConvertFrom(context, culture, value);
        }

    }


    public class AudioCounter
    {
        static int curIndex = 99;
        static public int GetIndex()
        {
            curIndex++;
            foreach (Audio t in Maze.mzP.cAudio)
            {
                if (t.Index >= curIndex)
                {
                    curIndex = t.Index + 1;
                }
            }
            return curIndex;
        }
    }




}
