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
        [Browsable(true)]
        public bool ItemVisible
        {
            get { return itemVisible; }
            set { itemVisible = value;
                OnPropertyChanged("itemVisible", true);
            }

        }

        protected bool itemLocked = false;
        [Category("0.Show/Hide")]
        [Description("Shows if Item is rendered and interactable on MazeMaker")]
        [Browsable(true)]
        public bool ItemLocked
        {
            get { return itemLocked; }
            set { itemLocked = value;
                OnPropertyChanged("itemLocked", true);
            }
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
            if (!itemVisible&&!selected)
                return;
        }

        public void SetID(int inp=0,bool copy=false)
        {
            if (copy||inp==-1)
                this.id = inp;
            else
                this.id = NameFactory.SetID(this.GetType().Name.ToString(), inp);
            
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
        static List<int> wallIds = new List<int>();
        static List<int> curvedWallIds = new List<int>();
        static List<int> floorIds = new List<int>();
        static List<int> lightIds = new List<int>();
        static List<int> endRegionIds = new List<int>();
        static List<int> activeRegionIds = new List<int>();
        static List<int> staticModelIds = new List<int>();
        static List<int> dynamicObjectIds = new List<int>();
        static List<int> startPosIds = new List<int>();

        static public int GetNextID(string itemType)
        {
            switch (itemType)
            {
                case "Wall":
                    return (indexWall+1);
                case "CurvedWall":
                    return (indexCurvedWall+1);
                case "Floor":
                    return (indexFloor+1);
                case "Light":
                    return (indexLight+1);
                case "EndRegion":
                    return (indexEnd+1);
                case "ActiveRegion":
                    return (indexActive+1);
                case "StaticModel":
                    return (indexStatic+1);
                case "DynamicObject":
                    return (indexDynamic+1);
                case "StartPos":
                    return (indexStart+1);

            }
            return 0;
        }

        static public int SetID(string itemType, int i=-1)
        {
            int nextId = GetNextID(itemType);
            if (i == 0)
                i = nextId;

            switch (itemType)
            {
                case "Wall":
                    if (wallIds.Contains(i))
                    { 
                        i = nextId;
                    }
                    else
                    {
                        wallIds.Add(i);
                    }
                    if (i >indexWall)
                        indexWall = i ;
                    break;
                case "CurvedWall":
                    if (curvedWallIds.Contains(i))
                    {
                        i = nextId;
                    }
                    else
                    {
                        curvedWallIds.Add(i);
                    }
                    if (i > indexCurvedWall)
                        indexCurvedWall = i;
                    break;
                case "Floor":
                    if (floorIds.Contains(i))
                    {
                        i = nextId;
                    }
                    else
                    {
                        floorIds.Add(i);
                    }
                    if (i > indexFloor)
                        indexFloor = i ;
                    break;
                case "Light":
                    if (lightIds.Contains(i))
                    {
                        i = nextId;
                    }
                    else
                    {
                        lightIds.Add(i);
                    }
                    if (i > indexLight)
                        indexLight = i;
                    break;
                case "EndRegion":
                    if (endRegionIds.Contains(i))
                    {
                        i = nextId;
                    }
                    else
                    {
                        endRegionIds.Add(i);
                    }
                    if (i  > indexEnd)
                        indexEnd = i ;
                    break;
                case "StaticModel":
                    if (staticModelIds.Contains(i))
                    {
                        i = nextId;
                    }
                    else
                    {
                        staticModelIds.Add(i);
                    }
                    if (i > indexStatic)
                        indexStatic = i ;
                    break;
                case "DynamicObject":
                    if (dynamicObjectIds.Contains(i))
                    {
                        i = nextId;
                    }
                    else
                    {
                        dynamicObjectIds.Add(i);
                    }
                    if (i  > indexDynamic)
                        indexDynamic = i ;
                    break;
                case "StartPos":
                    if (startPosIds.Contains(i))
                    {
                        i = nextId;
                    }
                    else
                    {
                        startPosIds.Add(i);
                    }
                    if (i > indexStart)
                        indexStart = i;
                    break;
                case "ActiveRegion":
                    if (activeRegionIds.Contains(i))
                    {
                        i = nextId;
                    }
                    else
                    {
                        activeRegionIds.Add(i);
                    }
                    if (i > indexActive)
                        indexActive = i;
                    break;

            }
            return i;
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

            wallIds.Clear();
            curvedWallIds.Clear();
            floorIds.Clear();
            lightIds.Clear();
            endRegionIds.Clear();
            activeRegionIds.Clear();
            staticModelIds.Clear();
            dynamicObjectIds.Clear();
            startPosIds.Clear();
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
