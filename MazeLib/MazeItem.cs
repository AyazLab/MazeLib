using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Drawing;

namespace MazeMaker
{
    public enum MazeItemType
    {
        None,Wall, CurvedWall, Floor, End, Light, Start, Static, Dynamic, CustomObject, ActiveRegion
    }

    public interface ICloneable<T>
    {
        T Clone();
    }

    
    [Serializable]  
    public class MazeItem : ICloneable<MazeItem>
    {
        public MazeItemType itemType = MazeItemType.None;
        //[field: NonSerialized]
        public bool changed=true;
        public bool treeChanged;
        public bool justCreated = true;

        protected bool selected=false;
        public bool IsSelected()
        {
            return selected;
        }

        public void Select(bool inp=true)
        {
            selected = inp;
        }

        public virtual void Move(float mzXdir,float mzZdir)
        {

        }

        public virtual void Rescale(double factor)
        {
            //Rescales X,Z coordinates (but not height)
        }

        public virtual void RescaleXYZ(double scaleX,double scaleY, double scaleZ)
        {

        }

        public MazeItem Clone()
        {

            return new MazeItem();
        }

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName, bool updateTree=false)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            changed = true;
            treeChanged = updateTree;
            
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool itemVisible = true;
        [Category("0.Show/Hide")]
        [Description("Shows if Item is rendered and interactable on MazeMaker")]
        [Browsable(false)]
        public bool ItemVisible
        {
            get { return itemVisible; }
            set { itemVisible = value; }
        }


        private int id = 0;
        [Category("1.Item")]
        [Description("Unique identifier of this maze item")]
        [ReadOnly(true)]
        public string ID
        {
            get { return this.GetType().Name.ToString() + id.ToString(); }
            set { }
        }

        public virtual void Rotate(float degrees,float centerX=0,float centerY=0)
        {

        }

        public virtual void Paint(ref Graphics gr)
        {

        }

        public void SetID(int inp=0)
        {
            if (inp > 0) //assign ID, but increment next valid IDs in case
            {
                this.id = inp;

                NameFactory.SetNextID(this.GetType().Name.ToString(), inp);
            }
            else if (inp == -1) //key for invalid IDs
            {
                this.id = -1;
                //if (incrementIDs)
                //{
                //    this.id = NameFactory.GetNextID(this.GetType().Name.ToString());
                //}
            }
            else //assign new value
                this.id=NameFactory.GetNextID(this.GetType().Name.ToString());
        }

        public int GetID()
        {
            return id;
        }

        private string label = "";
        [Category("1.Item")]
        [Description("Descriptive label of the Item")]
        public string Label
        {
            get { return label; }
            set
            {
                label = value;
                OnPropertyChanged("Label", true);
            }
        }
    }

    public static class NameFactory
    {
        static public int GetNextID(string itemType)
        {
            switch (itemType)
            {
                case "Wall":
                    return (++indexWall);
                case "CurvedWall":
                    return (++indexCurvedWall);
                case "Floor":
                    return (++indexFloor);
                case "Light":
                    return (++indexLight);
                case "EndRegion":
                    return (++indexEnd);
                case "ActiveRegion":
                    return (++indexActive);
                case "StaticModel":
                    return (++indexStatic);
                case "DynamicObject":
                    return (++indexDynamic);
                case "StartPos":
                    return (++indexStart);

            }
            return 0;
        }

        static public void SetNextID(string itemType, int i)
        {
            switch (itemType)
            {
                case "Wall":
                    if(i >indexWall)
                    indexWall = i ;
                    break;
                case "CurvedWall":
                    if (i > indexCurvedWall)
                        indexCurvedWall = i;
                    break;
                case "Floor":
                    if (i > indexFloor)
                    indexFloor = i ;
                    break;
                case "Light":
                    if (i > indexLight)
                    indexLight = i;
                    break;
                case "EndRegion":
                    if (i  > indexEnd)
                    indexEnd = i ;
                    break;
                case "StaticModel":
                    if (i > indexStatic)
                    indexStatic = i ;
                    break;
                case "DynamicModel":
                    if (i  > indexDynamic)
                    indexDynamic = i ;
                    break;
                case "StartPos":
                    if (i > indexStart)
                        indexStart = i;
                    break;
                case "ActiveRegion":
                    if (i > indexStart)
                        indexStart = i;
                    break;

            }
        }

        static int indexLight =0;
        //public static string GetNextNameLight()
        //{
        //    return "Light" + (++indexLight).ToString();
        //}

        static int indexWall = 0;
        //public static string GetNextNameWall()
        //{
        //    return "Wall" + (++indexWall).ToString();
        //}

        static int indexCurvedWall = 0;

        static int indexFloor = 0;
        //public static string GetNextNameFloor()
        //{
        //    return "Floor" + (++indexFloor).ToString();
        //}
        static int indexActive = 0;


        static int indexEnd = 0;
        //public static string GetNextNameEnd()
        //{
        //    return "End" + (++indexEnd).ToString();
        //}

        static int indexStatic = 0;
        //public static string GetNextNameStatic()
        //{
        //    return "Static" + (++indexStatic).ToString();
        //}

        static int indexDynamic = 0;
        //public static string GetNextNameDynamic()
        //{
        //    return "Dynamic" + (++indexDynamic).ToString();
        //}

        static int indexStart = 0;
        //public static string GetNextNameStartPos()
        //{
        //    return "Start Position";
        //}


        public static void Reset()
        {
            indexDynamic = 0;
            indexStatic = 0;
            indexWall = 0;
            indexCurvedWall=0;
            indexFloor = 0;
            indexEnd = 0;
            indexLight = 0;
            indexActive = 0;
            indexStart = 0;
        }
    }

    public class ImagePathConverter : StringConverter
    {
        public static Dictionary<string, string> Paths = new Dictionary<string, string>();
        List<string> textures = new List<string>();
        int memoryReset = 2;

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context) { return true; }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            if (memoryReset == 0)
            {
                textures = new List<string>();
                memoryReset += 3;
            }
            memoryReset--;

            if (!textures.Contains("[Import Item]"))
            {
                textures.Add("[Import Item]");
                textures.Add("[Manage Items]");
                textures.Add("----------------------------------------");
            }

            foreach (string texture in Paths.Keys)
            {
                if (!textures.Contains(texture))
                {
                    textures.Add(texture);
                }
            }

            return new StandardValuesCollection(textures);
        }
    }

    public class AudioPathConverter : StringConverter
    {
        public static Dictionary<string, string> Paths = new Dictionary<string, string>();
        List<string> audios = new List<string>();
        int memoryReset = 2;

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context) { return true; }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            if (memoryReset == 0)
            {
                audios = new List<string>();
                memoryReset += 3;
            }
            memoryReset--;

            if (!audios.Contains("[Import Item]"))
            {
                audios.Add("[Import Item]");
                audios.Add("[Manage Items]");
                audios.Add("----------------------------------------");
            }

            foreach (string audio in Paths.Keys)
            {
                if (!audios.Contains(audio))
                {
                    audios.Add(audio);
                }
            }

            return new StandardValuesCollection(audios);
        }
    }

    public class ModelPathConverter : StringConverter
    {
        public static Dictionary<string, string> Paths = new Dictionary<string, string>();
        List<string> models = new List<string>();
        int memoryReset = 2;

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context) { return true; }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            if (memoryReset == 0)
            {
                models = new List<string>();
                memoryReset += 3;
            }
            memoryReset--;

            if (!models.Contains("[Import Item]"))
            {
                models.Add("[Import Item]");
                models.Add("[Manage Items]");
                models.Add("----------------------------------------");
            }

            foreach (string model in Paths.Keys)
            {
                if (!models.Contains(model))
                {
                    models.Add(model);
                }
            }

            return new StandardValuesCollection(models);
        }
    }
}
