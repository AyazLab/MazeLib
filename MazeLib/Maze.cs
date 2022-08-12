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
using System.Xml;
using System.IO.Compression;

#endregion

namespace MazeMaker
{
    public class Maze
    {
        public Maze()
        {
            NameFactory.Reset();
            mazeGroups.Add("",new MazeItemGroup("(None)"));
        }

        public struct MazeItemGroup
        {
            public MazeItemGroup(string groupName)
            {
                this.isVisible = true;
                this.isLocked = false;
                if (groupName == "")
                    groupName = "(None)";
                this.groupName = groupName;
                groupItems = new List<MazeItem>();
                
                isChecked = true;
            }
            public string groupName;
            public List<MazeItem> groupItems;
            public bool isVisible;
            public bool isLocked;
            public bool isChecked;
            
        };

        public Dictionary<string,MazeItemGroup> mazeGroups=new Dictionary<string, MazeItemGroup>();

        public bool changed = false;
        
        public List<Wall> cWall = new List<Wall>();
        public List<CurvedWall> cCurveWall = new List<CurvedWall>();
        public List<Floor> cFloor = new List<Floor>();
        public List<CustomObject> cObject = new List<CustomObject>();
        public List<Light> cLight = new List<Light>();
        public List<StaticModel> cStaticModels = new List<StaticModel>();
        public List<DynamicObject> cDynamicObjects = new List<DynamicObject>();
        public List<StartPos> cStart = new List<StartPos>();

        public List<EndRegion> cEndRegions = new List<EndRegion>();
        public List<ActiveRegion> cActRegions = new List<ActiveRegion>();

        private string cMazeDirectory;

        int imageIDCounter = 100;
        public Dictionary<string, string> cImages = new Dictionary<string, string>();
        [Category("2.Collections")]
        [Description("Texture Image Files. Place these files to the same directory of with the Maze file or place them in the user library directory")]
        [Browsable(false)]
        public Dictionary<string, string> Image
        {
            get { return cImages; }
            set { cImages = value; }
        }

        //[Category("2.Collections")]
        //[Description("Available Texture Image Number in the List")]
        //[ReadOnly(true)]
        //public int ImageCount
        //{
        //    get { return cImages.Count; }
        //}

        int audioIDCounter = 100;
        public Dictionary<string, string> cAudio = new Dictionary<string, string>();
        [Category("2.Collections")]
        [Description("Audio Files. Place these files to the same directory of with the Maze file or place them in the user library directory")]
        [Browsable(false)]
        public Dictionary<string, string> Audio
        {
            get { return cAudio; }
            set { cAudio = value; }
        }

        //[Category("2.Collections")]
        //[Description("Available Audio Number in the List")]
        //[ReadOnly(true)]
        //public int AudioCount
        //{
        //    get { return cAudio.Count; }
        //}

        int modelIDCounter = 100;
        public Dictionary<string, string> cModels = new Dictionary<string, string>();
        [Category("2.Collections")]
        [Description("Model Files. Place these files to the same directory of with the Maze file or place them in the global models directory")]
        [Browsable(false)]
        public Dictionary<string, string> Model
        {
            get { return ModelPathConverter.Paths; }
            set { ModelPathConverter.Paths = value; }
        }
        
        //[Category("2.Collections")]
        //[Description("Available Model Number in the List")]
        //[ReadOnly(true)]
        //public int ModelCount
        //{
        //    get { return ModelPathConverter.Paths.Count; }
        //}

        //public List<StartPos> cStart = new List<StartPos>();
        //public EndRegion cEnd=null;

        private string desginer = "Anonymous";
        [Category("1.General")]
        [Description("Designer of the Maze")]
        public string Designer
        {
            get { return desginer; }
            set { desginer = value; }
        }
        private string comments = "";
        [Category("1.General")]
        [Description("Notes about the Maze")]
        public string Comments
        {
            get { return comments; }
            set { comments = value; }
        }
        private string name = "";
        [Category("1.General")]
        [Description("Name of the Maze")]
        [ReadOnly(true)]
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        private string filename = "";
        [Category("1.General")]
        [Description("Name of the Maze")]
        [ReadOnly(true)]
        public string FileName
        {
            get { return filename; }
            set { filename = value; }
        }

        private string timeoutMessageText = "";
        [Category("4.Timeout")]
        [Description("Message to be shown when timeout occurs")]
        [DisplayName("Message Text")]
        public string TimeoutMessageText
        {
            get { return timeoutMessageText; }
            set { timeoutMessageText = value; }
        }

        private string pointOutMessageText = "";
        [Category("5.Point Options")]
        [Description("Message to be shown when MazePoint Count reached")]
        [DisplayName("Point Message Text")]
        public string PointOutMessageText
        {
            get { return pointOutMessageText; }
            set { pointOutMessageText = value; }
        }

        private int pointOutThreshold =0;
        [Category("5.Point Options")]
        [Description("Exit Maze when Point Treshold Reached")]
        [DisplayName("Point Exit Threshold")]
        public int PointOutThreshold
        {
            get { return pointOutThreshold; }
            set { pointOutThreshold = value; }
        }

        public enum ThresholdComparator
        {
            GreaterThan, GreaterThanEqual, EqualTo, LessThan, LessThanEqual, NotEqual
        }

        private ThresholdComparator pointOutThresholdOperator = ThresholdComparator.GreaterThanEqual;
        [Category("5.Point Options")]
        [Description("Comparison applied to point threshold. Ex requires exactly 10 points, more than 10 points, or less than 10 points")]
        [DisplayName("Point Exit Threshold Operator")]
        public ThresholdComparator PointOutThresholdOperator
        {
            get { return pointOutThresholdOperator; }
            set { pointOutThresholdOperator = value; }
        }

        private int defaultStartPosID = -999;
        public StartPos defaultStartPos = null;
        [Category("3.Start Settings")]
        [Description("Select Initial Starting Position when loading maze")]
        [DisplayName("Default Start Position")]
        [TypeConverter(typeof(StartPosConverter))]
        public StartPos DefaultStartPos
        {
            get { return defaultStartPos; }
            set {
                defaultStartPos = value;
                foreach (StartPos sPos in cStart) { sPos.IsDefaultStartPos=false; }
                if (value != null)
                {
                    defaultStartPos.IsDefaultStartPos = true;
                    defaultStartPosID = defaultStartPos.GetID();
                }
                else
                    defaultStartPosID = -999;
            }
        }
        

        private bool startMessageEnable = false;
        [Category("3.Start Settings")]
        [Description("Enable or disable start message to be shown when user enters maze")]
        [DisplayName("Enable Start Message")]
        public bool StartMessageEnable
        {
            get { return startMessageEnable; }
            set { startMessageEnable = value; }
        }
        private string startMessageText = "";
        [Category("3.Start Settings")]
        [Description("Message to be shown when user enters maze")]
        [DisplayName("Message Text")]
        
        public string StartMessageText
        {
            get { return startMessageText; }
            set {
                startMessageText = value;
                if (String.IsNullOrWhiteSpace(startMessageText) == false)
                    StartMessageEnable = true;
            }
        }
        private Color ambientColor = Color.White;
        [Category("7.Ambient Light")]
        [Description("Ambient light color in the entire maze")]
        public Color AmbientColor
        {
            get { return ambientColor; }
            set { ambientColor = value; }
        }

        private float ambientIntensity = 0.6f;
        [Category("7.Ambient Light")]
        [Description("Ambient light intensity in the entire maze. Min=0.0f, Max=1.0f")]
        public float AmbientIntesity
        {
            get { return ambientIntensity; }
            set { ambientIntensity = value; }
        }

        private int skyTextureID = -999;
        private string skyTexture = "";
        [Category("6.Skybox")]
        [Description("Select texture to be used for skybox. List can be edited at Texture Collection")]
        [TypeConverter(typeof(ImagePathConverter))]
        public string SkyBoxTexture
        {
            get { return skyTexture; }
            set { skyTexture = value; }
            //set
            //{
            //    if (skyTexture == null)
            //    {
            //        skyTextureID = -999;
            //        foreach (Texture t in cImages)
            //        {
            //            t.Skybox = false;
            //        }
            //    }
            //    else
            //    {
            //        foreach (Texture t in cImages)
            //        {
            //            t.Skybox = false;
            //        }
            //        skyTextureID = skyTexture.Index;
            //        skyTexture.Skybox = true;
            //    }
            //    skyTexture = value;
            //}
        }

        private double scale = 17;
        [Category("1.General")]
        [Description("Zoom functionality (internal)")]
        [Browsable(false)]
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
                {
                    if(w.Scale!= scale)
                        w.Scale = scale;
                }
                foreach (CurvedWall w in cCurveWall)
                {
                    if (w.Scale != scale)
                        w.Scale = scale;
                }
                foreach (Floor f in cFloor)
                {
                    if(f.Scale != scale)
                        f.Scale = scale;
                }
                foreach (StartPos s in cStart)
                {
                    if(s.Scale!=scale)
                        s.Scale = scale;
                }
                foreach (EndRegion en in cEndRegions)
                {
                    if(en.Scale!=scale)
                        en.Scale = scale;
                }
                foreach (ActiveRegion en in cActRegions)
                {
                    if (en.Scale != scale)
                        en.Scale = scale;
                }
                foreach (Light l in cLight)
                {
                    if(l.Scale!=scale)
                        l.Scale = scale;
                }
                foreach (StaticModel s in cStaticModels)
                {
                    if(s.Scale != scale)
                        s.Scale = scale;
                }
                foreach (DynamicObject d in cDynamicObjects)
                {
                    if(d.Scale != scale)
                        d.Scale = scale;
                }

            }
        }

        //used only when loading files...
        public void SilentSetScale(double newScale)
        {
            scale = newScale;
            foreach (Wall w in cWall)
            {
                if (w.Scale != scale)
                    w.SilentSetScale(scale);
            }
            foreach (CurvedWall w in cCurveWall)
            {
                if (w.Scale != scale)
                    w.SilentSetScale(scale);
            }
            foreach (Floor f in cFloor)
            {
                if (f.Scale != scale)
                    f.SilentSetScale(scale);
            }
            foreach(StartPos s in cStart)
            {
                if (s.Scale != scale)
                    s.SilentSetScale(scale);
            }
            foreach (EndRegion en in cEndRegions)
            {
                if (en.Scale != scale)
                    en.SilentSetScale(scale);
            }
            foreach (Light l in cLight)
            {
                if (l.Scale != scale)
                    l.SilentSetScale(scale);
            }
            foreach (StaticModel s in cStaticModels)
            {
                if (s.Scale != scale)
                    s.SilentSetScale(scale);
            }
            foreach (DynamicObject d in cDynamicObjects)
            {
                if (d.Scale != scale)
                    d.SilentSetScale(scale);
            }
        }
        private bool timeoutEnable = false;
        [Category("4.Timeout")]
        [Description("Enable or disable timeout feature")]
        [DisplayName("Enable")]
        public bool TimeoutEnable
        {
            get
            {
                return timeoutEnable;
            }
            set
            {
                timeoutEnable = value;
                if (timeoutEnable == false)
                {
                    timeout = 0;
                }
            }
        }


        private double timeout = 0;
        [Category("4.Timeout")]
        [Description("Timeout value for the Maze walker in seconds. Enter 0 (zero) to disable")]
        public double TimeoutValue
        {
            get 
            { 
                return timeout; 
            }
            set
            { 
                timeout = value;
                if (timeout <= 0)
                {
                    timeoutEnable = false;
                }
                else
                {
                    timeoutEnable = true;
                }
            }
        }

        private double moveSpeed = 3.0;
        [Category("8.Speed")]
        [Description("Walk Speed of the subject in the environment in Mz/s")]
        [DisplayName("Move Speed")]
        public double MoveSpeed
        {
            get
            {
                return moveSpeed;
            }
            set
            {
                moveSpeed = value;
            }
        }
        private double turnSpeed = 45;
        [Category("8.Speed")]
        [Description("View(Turn around) speed of the subject in the environment in deg/s")]
        [DisplayName("Turn Speed")]
        public double TurnSpeed
        {
            get
            {
                return turnSpeed;
            }
            set
            {
                turnSpeed = value;
            }
        }

        private int viewPerspectiveSetting = 0;
        [Category("9.Perspective Settings")]
        [TypeConverter(typeof(ViewPerspectiveConverter))]
        [Description("Perspective of the MazeWalker Avatar")]
        [DisplayName("Camera Mode")]
        public String ViewPerspective
        {
            get
            {
                if (viewPerspectiveSetting == 0)
                    return "First-Person";
                else if (viewPerspectiveSetting == 1)
                    return "Top-Down";
                else if (viewPerspectiveSetting == 2)
                    return "3/4 View or Fixed";
                else return "Error";
            }
            set
            {
                if (value == "First-Person")
                    viewPerspectiveSetting = 0;
                else if (value == "Top-Down")
                    viewPerspectiveSetting = 1;
                else if (value == "3/4 View or Fixed")
                    viewPerspectiveSetting = 2;
                else
                    viewPerspectiveSetting = -1;
            }
        }

        private int topDownOrientation = 1;
        [Category("9.Perspective Settings")]
        [TypeConverter(typeof(OrientationConverter))]
        [Description("Perspective of the MazeWalker Avatar")]
        [DisplayName("TopDown Orientation")]
        public String TopDownOrientation
        {
            get
            {
                if (topDownOrientation == 0)
                    return "Free";
                else if (topDownOrientation == 1)
                    return "North";
                else if (topDownOrientation == 2)
                    return "South";
                else if (topDownOrientation == 3)
                    return "East";
                else if (topDownOrientation == 4)
                    return "West";
                else return "Error";
            }
            set
            {
                if (value == "Free")
                    topDownOrientation = 0;
                else if (value == "North")
                    topDownOrientation = 1;
                else if (value == "South")
                    topDownOrientation = 2;
                else if (value == "East")
                    topDownOrientation = 3;
                else if (value == "West")
                    topDownOrientation = 4;
                else
                    topDownOrientation = -1;
            }
        }

        //private bool useMouseToOrient = false;
        //[Category("9.Perspective Settings")]
        //[Description("Use the Mouse to control rotation in Top-Down or fixed")]
        //[DisplayName("Use Mouse to Orient")]
        //public bool UseMouseToOrient
        //{
        //    get
        //    {
        //        return useMouseToOrient;
        //    }
        //    set
        //    {
        //        useMouseToOrient = value;
        //    }
        //}

        private int avatarModelID = -999;
        private string avatarModel = "";
        [Category("X.Avatar Options")]
        [Description("Model used for MazeWalker Avatar when not in First Person Mode")]
        [DisplayName("Avatar Model")]
        [TypeConverter(typeof(ModelPathConverter))]
        public string AvatarModel
        {
            get { return avatarModel; }
            set { avatarModel = value; }
        }

        private double avatarScale = 1;
        [Category("X.Avatar Options")]
        [Description("Scale of Avatar Model displayed when not in First Person Mode")]
        [DisplayName("Avatar Scale")]
        public double AvatarScale
        {
            get { return avatarScale; }
            set { avatarScale = value; }
        }

        private MPoint avatarInitRot = new MPoint(0, 0,0);
        [Category("X.Avatar Options")]
        [DisplayName("Avatar Initial Rotation")]
        [Description("Rotation of Avatar Model displayed when not in First Person Mode")]
        public MPoint AvatarInitRot
        {
            get { return avatarInitRot; }
            set { avatarInitRot = value; }
        }


        private double avatarHeight = 0;
        [Category("9.Perspective Settings")]
        [Description("Camera Height from Avatar when in First Person Mode")]
        [DisplayName("Avatar Height")]
        public double AvatarHeight
        {
            get
            {
                return avatarHeight;
            }
            set
            {
                avatarHeight = value;
            }
        }

        private double cameraHeight = 15;
        [Category("9.Perspective Settings")]
        [Description("Camera Height from Avatar when in Free, Top-Down, or 3/4 Mode")]
        [DisplayName("Camera Height")]
        public double CameraHeight
        {
            get
            {
                return cameraHeight;
            }
            set
            {
                cameraHeight = value;
            }
        }

        private bool useFixedXLocation = false;
        [Category("9.Perspective Settings")]
        [Description("Enable Fixed X Camera Enabled")]
        [DisplayName("Fix Camera X")]
        public bool FixedXLocationEnabled
        {
            get
            {
                return useFixedXLocation;
            }
            set
            {
                useFixedXLocation = value;
            }
        }

        private bool useFixedZLocation = false;
        [Category("9.Perspective Settings")]
        [Description("Enable Fixed Z Camera Enabled")]
        [DisplayName("Fix Camera Z ")]
        public bool FixedZLocationEnabled
        {
            get
            {
                return useFixedZLocation;
            }
            set
            {
                useFixedZLocation = value;
            }
        }

        private double fixedX = 20;
        [Category("9.Perspective Settings")]
        [Description("Camera X Location in MazeCoordinates when in Free, Top-Down, or 3/4 Mode and Fixed XZ is selected")]
        [DisplayName("Fixed X Location")]
        public double FixedX
        {
            get
            {
                return fixedX;
            }
            set
            {
                fixedX = value;
            }
        }

        private double fixedZ = 20;
        [Category("9.Perspective Settings")]
        [Description("Camera Z Location in MazeCoordinates when in Free, Top-Down, or 3/4 Mode and Fixed XZ is selected")]
        [DisplayName("Fixed Z Location")]
        public double FixedZ
        {
            get
            {
                return fixedZ;
            }
            set
            {
                fixedZ = value;
            }
        }

        private double fieldOfView = 45;
        [Category("9.Perspective Settings")]
        [Description("Camera Field of View")]
        [DisplayName("Field Of View")]
        public double FieldOfView
        {
            get
            {
                return fieldOfView;
            }
            set
            {
                fieldOfView = value;
            }
        }

        private bool useXRayDisplay = false;
        [Category("9.Perspective Settings")]
        [Description("Draws Walls using Lines instead")]
        [DisplayName("X-Ray Rendering")]
        public bool UseXRayDisplay
        {
            get
            {
                return useXRayDisplay;
            }
            set
            {
                useXRayDisplay = value;
            }
        }

        public Dictionary<string,string> usedImageAssets;
        public Dictionary<string, string> usedModelAssets;
        public Dictionary<string, string> usedAudioAssets;

        public void CheckUsed()
        {
            usedImageAssets = ImagePathConverter.Paths;
            usedModelAssets = ModelPathConverter.Paths;
            usedAudioAssets = AudioPathConverter.Paths;

            List<string> imagesToRemove = new List<string>();

            foreach (string image in usedImageAssets.Keys)
            {
                if (image != "") // true
                {

                    //check weather the texture is in the list...

                    bool used = false;

                    if(skyTexture == image)
                    {
                        used = true;
                    }

                    if (used == false)
                    {
                        foreach (Wall wall in cWall)
                        {
                            if (wall.Texture == image)
                            {
                                used = true;
                                break;
                            }
                        }
                    }

                    if (used == false)
                    {
                        foreach (CurvedWall curveWall in cCurveWall)
                        {
                            if (curveWall.Texture == image)
                            {

                                used = true;
                                break;
                            }

                        }
                    }

                    if (used == false)
                    {
                        foreach (Floor floor in cFloor)
                        {
                            if (floor.FloorTexture == image)
                            {

                                used = true;
                                break;
                            }
                            else if (floor.CeilingTexture == image)
                            {
                                used = true;
                                break;
                            }
                        }
                    }

                    //string filePath = MakeRelativePath(inp, ImagePathConverter.Paths[image]);
                    if (!used)
                    {
                        imagesToRemove.Add(image);
                        
                    }
                   
                }
            }

            foreach (string image in imagesToRemove)
                usedImageAssets.Remove(image);

            List<string> modelsToRemove = new List<string>();
            foreach (string model in usedModelAssets.Keys)
            {
                if (model != "") // true
                {

                    bool used = false;

                    if (avatarModel == model)
                    {
                        used = true;
                    }

                    foreach (StaticModel staticModel in cStaticModels)
                    {
                        //if (s.Model!= null && s.Model.Index == model.Index)
                        if (staticModel.Model == model)
                        {

                            used = true;
                            break;
                        }
                    }

                    if (used == false)
                    {
                        foreach (DynamicObject dynamicObject in cDynamicObjects)
                        {
                            //if (d.Model != null && d.Model.Index == model.Index)
                            if (dynamicObject.Model == model)
                            {

                                used = true;
                                break;
                            }
                            //else if (dynamicObject.SwitchToModel != null && dynamicObject.SwitchToModel.Index == model.Index)
                            else if (dynamicObject.SwitchToModel == model)
                            {

                                used = true;
                                break;
                            }
                        }
                    }


                    if (!used)
                    {
                        modelsToRemove.Add(model);
                        
                    }

                }
 
            }

            foreach (string model in modelsToRemove)
                usedModelAssets.Remove(model);


            List<string> audioToRemove = new List<string>();

            foreach (string audio in usedAudioAssets.Keys)
            {
                if (audio != "")
                {

                    //check weather the texture is in the list...

                    bool used = false;
                    if (used == false)
                    {
                        foreach (DynamicObject dynamicObject in cDynamicObjects)
                        {
                            if (dynamicObject.Phase1HighlightAudio == audio)
                            {

                                used = true;
                                break;
                            }
                            if (dynamicObject.Phase2EventAudio == audio)
                            {
                               
                                used = true;
                                break;
                            }
                        }
                    }

                    if (used == false)
                    {
                        foreach (ActiveRegion activeRegion in cActRegions)
                        {
                            if (activeRegion.Phase1HighlightAudio == audio)
                            {

                                used = true;
                                break;
                            }
                            if (activeRegion.Phase2EventAudio == audio)
                            {

                                used = true;
                                break;
                            }
                        }
                    }


                    if (!used)
                    {
                        audioToRemove.Add(audio);
                    }
                    

                }

            }
            foreach (string audio in audioToRemove)
                usedAudioAssets.Remove(audio);
        }

        public bool SaveToMazeXML(string inp)
        {
            CheckUsed();

            XmlDocument doc = new XmlDocument();

            SetName(inp);

            //FinalCheckBeforeWrite();

            changed = false;

            XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            XmlElement root = doc.DocumentElement;
            doc.InsertBefore(xmlDeclaration, root);

            XmlElement mazeXMLnode = doc.CreateElement(string.Empty, "MazeFile", string.Empty);
            doc.AppendChild(mazeXMLnode);
            mazeXMLnode.SetAttribute("version", "2.0");
            mazeXMLnode.SetAttribute("url", "http://www.mazesuite.com");

            XmlElement infoNode = doc.CreateElement(string.Empty, "Info", string.Empty);
            mazeXMLnode.AppendChild(infoNode);

            XmlElement authorNode = doc.CreateElement(string.Empty, "Author", string.Empty);
            infoNode.AppendChild(authorNode);
                    

            authorNode.SetAttribute("name", this.Designer);
            authorNode.SetAttribute("comments", this.Comments);

            XmlElement globalNode = doc.CreateElement(string.Empty, "Global", string.Empty);
            mazeXMLnode.AppendChild(globalNode);

                
            XmlElement avatarNode = doc.CreateElement(string.Empty, "Avatar", string.Empty);
            globalNode.AppendChild(avatarNode);
            avatarNode.SetAttribute("scale", this.AvatarScale.ToString());
            //if (this.AvatarModel != null)
            //    avatarNode.SetAttribute("id", this.AvatarModel.Index.ToString());
            if (AvatarModel != "")
            {
                cModels[AvatarModel] = modelIDCounter.ToString();
                modelIDCounter++;
                avatarNode.SetAttribute("id", cModels[AvatarModel]);
            }
            avatarNode.SetAttribute("rotX", this.AvatarInitRot.X.ToString());
            avatarNode.SetAttribute("rotY", this.AvatarInitRot.Y.ToString());
            avatarNode.SetAttribute("rotZ", this.AvatarInitRot.Z.ToString());

            XmlElement generalNode = doc.CreateElement(string.Empty, "General", string.Empty);
            globalNode.AppendChild(generalNode);
            //generalNode.SetAttribute("scale", this.Scale.ToString());

            XmlElement speedNode = doc.CreateElement(string.Empty, "Speed", string.Empty);
            globalNode.AppendChild(speedNode);
            speedNode.SetAttribute("moveSpeed", this.moveSpeed.ToString());
            speedNode.SetAttribute("turnSpeed", this.turnSpeed.ToString());

            XmlElement ambientLightNode = doc.CreateElement(string.Empty, "AmbientLight", string.Empty);
            globalNode.AppendChild(ambientLightNode);
            ambientLightNode.SetAttribute("r", ((float)this.AmbientColor.R/255).ToString());
            ambientLightNode.SetAttribute("g", ((float)this.AmbientColor.G / 255).ToString());
            ambientLightNode.SetAttribute("b", ((float)this.AmbientColor.B / 255).ToString());
            ambientLightNode.SetAttribute("intensity", this.AmbientIntesity.ToString());

            XmlElement startMessageNode = doc.CreateElement(string.Empty, "StartMessage", string.Empty);
            globalNode.AppendChild(startMessageNode);
            startMessageNode.SetAttribute("enabled", this.StartMessageEnable.ToString());
            startMessageNode.SetAttribute("message", this.StartMessageText);

            XmlElement defaultStartPositionNode = doc.CreateElement(string.Empty, "DefaultStartPosition", string.Empty);
            globalNode.AppendChild(defaultStartPositionNode);
            if(this.DefaultStartPos!=null)
                defaultStartPositionNode.SetAttribute("id", this.DefaultStartPos.GetID().ToString());

            XmlElement timeoutNode = doc.CreateElement(string.Empty, "Timeout", string.Empty);
            globalNode.AppendChild(timeoutNode);
            timeoutNode.SetAttribute("enabled", this.TimeoutEnable.ToString());
            timeoutNode.SetAttribute("message", this.TimeoutMessageText);
            timeoutNode.SetAttribute("timeoutValue", this.TimeoutValue.ToString());

            XmlElement pointOptionsNode = doc.CreateElement(string.Empty, "PointOptions", string.Empty);
            globalNode.AppendChild(pointOptionsNode);
            pointOptionsNode.SetAttribute("exitThreshold", this.PointOutThreshold.ToString());
            pointOptionsNode.SetAttribute("exitThresholdOperator", this.PointOutThresholdOperator.ToString());
            pointOptionsNode.SetAttribute("messageText", this.PointOutMessageText);

            //if (this.skyTexture != null)
            //{
            //    XmlElement skyboxNode = doc.CreateElement(string.Empty, "Skybox", string.Empty);
            //    globalNode.AppendChild(skyboxNode);
            //    skyboxNode.SetAttribute("id", this.skyTexture.Index.ToString());
            //}
            if (skyTexture != "")
            {
                cImages[skyTexture] = imageIDCounter.ToString();
                imageIDCounter++;

                XmlElement skyboxNode = doc.CreateElement(string.Empty, "Skybox", string.Empty);
                globalNode.AppendChild(skyboxNode);
                skyboxNode.SetAttribute("id", cImages[skyTexture]);
            }

            XmlElement perspectiveSettingsNode = doc.CreateElement(string.Empty, "PerspectiveSettings", string.Empty);
            globalNode.AppendChild(perspectiveSettingsNode);
            perspectiveSettingsNode.SetAttribute("avatarHeight", this.AvatarHeight.ToString());
            perspectiveSettingsNode.SetAttribute("cameraHeight", this.CameraHeight.ToString());
            perspectiveSettingsNode.SetAttribute("cameraMode", this.ViewPerspective.ToString());
            perspectiveSettingsNode.SetAttribute("fieldOfView", this.FieldOfView.ToString());
            perspectiveSettingsNode.SetAttribute("fixCameraX", this.FixedXLocationEnabled.ToString());
            perspectiveSettingsNode.SetAttribute("fixedCameraX", this.FixedX.ToString());
            perspectiveSettingsNode.SetAttribute("fixCameraZ", this.FixedZLocationEnabled.ToString());
            perspectiveSettingsNode.SetAttribute("fixedCameraZ", this.FixedZ.ToString());
            perspectiveSettingsNode.SetAttribute("topDownOrientation", this.TopDownOrientation.ToString());
            //perspectiveSettingsNode.SetAttribute("useMouseToOrient", this.UseMouseToOrient.ToString());
            perspectiveSettingsNode.SetAttribute("xRayRendering", this.UseXRayDisplay.ToString());

            XmlElement imageLibraryNode = doc.CreateElement(string.Empty, "ImageLibrary", string.Empty);
            mazeXMLnode.AppendChild(imageLibraryNode);

            XmlElement imageLibraryItem;
            foreach (string image in usedImageAssets.Keys)
            {
                //imageLibraryItem = image.toXMLnode(doc);
                imageLibraryItem = doc.CreateElement(string.Empty, "Image", string.Empty);

                if (!cImages.ContainsKey(image))
                {
                    cImages[image] = imageIDCounter.ToString();
                    imageIDCounter++;
                }
                imageLibraryItem.SetAttribute("id", cImages[image]);

                string filePath = usedImageAssets[image];
                if (filePath[1] == ':')
                {
                    filePath = MakeRelativePath(inp, filePath);
                }
                imageLibraryItem.SetAttribute("file", filePath);

                imageLibraryNode.AppendChild(imageLibraryItem);
            }

            XmlElement modelLibraryNode = doc.CreateElement(string.Empty, "ModelLibrary", string.Empty);
            mazeXMLnode.AppendChild(modelLibraryNode);

            XmlElement modelLibraryItem;
            foreach (string model in usedModelAssets.Keys)
            {
                //modelNode = m.toXMLnode(doc);
                modelLibraryItem = doc.CreateElement(string.Empty, "Model", string.Empty);

                if (!cModels.ContainsKey(model))
                {
                    cModels[model] = modelIDCounter.ToString();
                    modelIDCounter++;
                }
                modelLibraryItem.SetAttribute("id", cModels[model]);

                string filePath = usedModelAssets[model];
                if (filePath[1] == ':')
                {
                    filePath = MakeRelativePath(inp, filePath);
                }
                modelLibraryItem.SetAttribute("file", filePath);

                modelLibraryNode.AppendChild(modelLibraryItem);
            }

            XmlElement audioLibraryNode = doc.CreateElement(string.Empty, "AudioLibrary", string.Empty);
            mazeXMLnode.AppendChild(audioLibraryNode);

            XmlElement audioLibraryItem;
            //foreach (Audio a in cAudio)
            foreach (string audio in usedAudioAssets.Keys)
            {
                //audioLibraryItem = a.toXMLnode(doc);
                audioLibraryItem = doc.CreateElement(string.Empty, "Sound", string.Empty);

                if (!cAudio.ContainsKey(audio))
                {
                    cAudio[audio] = audioIDCounter.ToString();
                    audioIDCounter++;
                }
                audioLibraryItem.SetAttribute("id", cAudio[audio]);

                string filePath = usedAudioAssets[audio];
                if (filePath[1] == ':')
                {
                    filePath = MakeRelativePath(inp, filePath);
                }
                audioLibraryItem.SetAttribute("file", filePath);

                audioLibraryNode.AppendChild(audioLibraryItem);
            }

            XmlElement mazeItemsNode = doc.CreateElement(string.Empty, "MazeItems", string.Empty);
            mazeXMLnode.AppendChild(mazeItemsNode);

            XmlElement wallsNode = doc.CreateElement(string.Empty, "Walls", string.Empty);
            mazeItemsNode.AppendChild(wallsNode);

            XmlElement wallNode;
            foreach (Wall w in cWall)
            {
                wallNode = w.toXMLnode(doc, cImages);
                wallsNode.AppendChild(wallNode);
            }

            XmlElement curvedWallNode;
            foreach (CurvedWall w in cCurveWall)
            {
                curvedWallNode = w.toXMLnode(doc, cImages);
                wallsNode.AppendChild(curvedWallNode);
            }

            XmlElement floorsNode = doc.CreateElement(string.Empty, "Floors", string.Empty);
            mazeItemsNode.AppendChild(floorsNode);

            XmlElement floorNode;
            foreach (Floor f in cFloor)
            {
                floorNode = f.toXMLnode(doc, cImages);
                floorsNode.AppendChild(floorNode);
            }

            XmlElement staticModelsNode = doc.CreateElement(string.Empty, "StaticModels", string.Empty);
            mazeItemsNode.AppendChild(staticModelsNode);

            XmlElement staticModelNode;
            foreach (StaticModel s in cStaticModels)
            {
                staticModelNode = s.toXMLnode(doc, cModels);
                staticModelsNode.AppendChild(staticModelNode);
            }

            XmlElement dynamicObjectsNode = doc.CreateElement(string.Empty, "DynamicObjects", string.Empty);
            mazeItemsNode.AppendChild(dynamicObjectsNode);

            XmlElement dynamicObjectNode;
            foreach (DynamicObject d in cDynamicObjects)
            {
                dynamicObjectNode = d.toXMLnode(doc, cAudio, cModels);
                dynamicObjectsNode.AppendChild(dynamicObjectNode);
            }

            XmlElement startPositionsNode = doc.CreateElement(string.Empty, "StartPositions", string.Empty);
            mazeItemsNode.AppendChild(startPositionsNode);

            XmlElement startPositionNode;
            foreach (StartPos s in cStart)
            {
                startPositionNode = s.toXMLnode(doc);
                startPositionsNode.AppendChild(startPositionNode);
            }

            XmlElement endRegionsNode = doc.CreateElement(string.Empty, "EndRegions", string.Empty);
            mazeItemsNode.AppendChild(endRegionsNode);

            XmlElement endRegionNode;
            foreach (EndRegion en in cEndRegions)
            {
                endRegionNode = en.toXMLnode(doc);
                endRegionsNode.AppendChild(endRegionNode);
            }

            XmlElement lightsNode = doc.CreateElement(string.Empty, "Lights", string.Empty);
            mazeItemsNode.AppendChild(lightsNode);

            XmlElement lightNode;
            foreach (Light l in cLight)
            {
                lightNode = l.toXMLnode(doc);
                lightsNode.AppendChild(lightNode);
            }

            XmlElement activeRegionsNode = doc.CreateElement(string.Empty, "ActiveRegions", string.Empty);
            mazeItemsNode.AppendChild(activeRegionsNode);

            XmlElement activeRegionNode;
            foreach (ActiveRegion ar in cActRegions)
            {
                activeRegionNode = ar.toXMLnode(doc, cAudio);
                activeRegionsNode.AppendChild(activeRegionNode);
            }


            doc.Save(inp);

            return true;
        }

        string MakeRelativePath(string fromPath, string toPath)
        {
            try
            {
                Uri fromUri = new Uri(fromPath);
                Uri toUri = new Uri(toPath);

                if (fromUri.Scheme != toUri.Scheme) { return toPath; }

                String relativePath = Uri.UnescapeDataString(fromUri.MakeRelativeUri(toUri).ToString());

                if (toUri.Scheme.Equals("file", StringComparison.InvariantCultureIgnoreCase))
                    return relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

                return relativePath;
            }
            catch { return toPath; }
        }

        public bool SaveToClassicFile(string inp)
        { 
            StreamWriter fp = new StreamWriter(inp);

            if (fp == null)
            {
                return false;
            }
            
            SetName(inp);

            //FinalCheckBeforeWrite();

            changed = false;

            fp.WriteLine("Maze File 1.2");
            ////Texture List/////////
            fp.WriteLine("-1\t-1");
            //bitmap file names goes here
            //fp.WriteLine("\t1\tmetal.bmp");
            //int index = 1;
            //foreach (Texture image in cImages)
            foreach (string image in ImagePathConverter.Paths.Keys)
            {
                //if (image.Name != "")
                if (image != "") // true
                {
                    //fp.WriteLine("\t{0}\t{1}", index, t.Name);   
                 
                    //check weather the texture is in the list...

                    bool used = false;
                    if (used == false)
                    {
                        foreach (Wall wall in cWall)
                        {
                            if (wall.Texture == image)
                            {
                                if (!cImages.ContainsKey(image))
                                {
                                    cImages[image] = imageIDCounter.ToString();
                                    imageIDCounter++;
                                }

                                used = true;
                                break;
                            }
                        }
                    }

                    if (used == false)
                    {
                        foreach (Floor floor in cFloor)
                        {
                            if (floor.FloorTexture == image)
                            {
                                if (!cImages.ContainsKey(image))
                                {
                                    cImages[image] = imageIDCounter.ToString();
                                    imageIDCounter++;
                                }

                                used = true;
                                break;
                            }
                            else if (floor.CeilingTexture == image)
                            {
                                if (!cImages.ContainsKey(image))
                                {
                                    cImages[image] = imageIDCounter.ToString();
                                    imageIDCounter++;
                                }

                                used = true;
                                break;
                            }
                        }
                    }

                    string filePath = MakeRelativePath(inp, ImagePathConverter.Paths[image]);
                    if (used)
                    {
                        //fp.WriteLine("\t{0}\t{1}", image.Index, image.Name.Trim());
                        fp.WriteLine("\t{0}\t{1}", cImages[image], filePath);
                    }
                    else
                    {
                        //fp.WriteLine("\t{0}\t{1}", image.Index, image.Name.Trim() + " ");
                        fp.WriteLine("\t{0}\t{1}", "99", filePath + " ");
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

            //foreach (Model t in cModels)
            foreach (string model in ModelPathConverter.Paths.Keys)
            {
                //if (model.Name != "")
                if (model != "") // true
                {
                    //fp.WriteLine("\t{0}\t{1}", index, t.Name);      

                    //check weather the model is used or not!
                    bool used = false;

                    foreach (StaticModel staticModel in cStaticModels)
                    {
                        //if (s.Model!= null && s.Model.Index == model.Index)
                        if (staticModel.Model == model)
                        {
                            if (!cModels.ContainsKey(model))
                            {
                                cModels[model] = modelIDCounter.ToString();
                                modelIDCounter++;
                            }

                            used = true;
                            break;
                        }
                    }

                    if (used == false)
                    {
                        foreach (DynamicObject dynamicObject in cDynamicObjects)
                        {
                            //if (d.Model != null && d.Model.Index == model.Index)
                            if (dynamicObject.Model == model)
                            {
                                if (!cModels.ContainsKey(model))
                                {
                                    cModels[model] = modelIDCounter.ToString();
                                    modelIDCounter++;
                                }

                                used = true;
                                break;
                            }
                            //else if (dynamicObject.SwitchToModel != null && dynamicObject.SwitchToModel.Index == model.Index)
                            else if (dynamicObject.SwitchToModel == model)
                            {
                                if (!cModels.ContainsKey(model))
                                {
                                    cModels[model] = modelIDCounter.ToString();
                                    modelIDCounter++;
                                }

                                used = true;
                                break;
                            }
                        }
                    }

                    string filePath = MakeRelativePath(inp, ModelPathConverter.Paths[model]);
                    if (used)
                    {
                        //fp.WriteLine("\t{0}\t{1}", model.Index, model.Name.Trim());
                        fp.WriteLine("\t{0}\t{1}", cModels[model], filePath);
                    }
                    else
                    {
                        //fp.WriteLine("\t{0}\t{1}", model.Index, model.Name.Trim() + " ");
                        fp.WriteLine("\t{0}\t{1}", "99", filePath + " ");
                    }

                }
                //index++;
            }

            fp.WriteLine("\t-1");

            ////Audio List//////
            fp.WriteLine("-11\t-1");
            //foreach (Audio audio in cAudio)
            foreach (string audio in AudioPathConverter.Paths.Keys)
            {
                //if (audio.Name != "")
                if (audio != "")
                {
                    //fp.WriteLine("\t{0}\t{1}", index, t.Name);   

                    //check weather the texture is in the list...

                    bool used = false;
                    if (used == false)
                    {
                        foreach (DynamicObject dynamicObject in cDynamicObjects)
                        {
                            //if (dynamicObject.Phase1HighlightAudio != null && dynamicObject.Phase1HighlightAudio.Index == audio.Index)
                            if (dynamicObject.Phase1HighlightAudio == audio)
                            {
                                if (!cAudio.ContainsKey(audio))
                                {
                                    cAudio[audio] = audioIDCounter.ToString();
                                    audioIDCounter++;
                                }

                                used = true;
                                break;
                            }
                            //if (dynamicObject.Phase2EventAudio != null && dynamicObject.Phase2EventAudio.Index == audio.Index)
                            if (dynamicObject.Phase2EventAudio == audio)
                            {
                                if (!cAudio.ContainsKey(audio))
                                {
                                    cAudio[audio] = audioIDCounter.ToString();
                                    audioIDCounter++;
                                }

                                used = true;
                                break;
                            }
                        }
                    }

                    string filePath = MakeRelativePath(inp, AudioPathConverter.Paths[audio]);
                    if (used)
                    {
                        //fp.WriteLine("\t{0}\t{1}", audio.Index, audio.Name.Trim());
                        fp.WriteLine("\t{0}\t{1}", cAudio[audio], filePath);
                    }
                    else
                    {
                        //fp.WriteLine("\t{0}\t{1}", audio.Index, audio.Name.Trim() + " ");
                        fp.WriteLine("\t{0}\t{1}", "99", filePath + " ");
                    }


                }
                //index++;
            }

            fp.WriteLine("\t-1");


            ////Objects/////////
            foreach (Floor f in cFloor)
            {
                f.PrintToFile(ref fp, cImages);
            }

            foreach (Wall w in cWall)
            {
                w.PrintToFile(ref fp, cImages);
            }

            foreach (StaticModel l in cStaticModels)
            {
                l.PrintToFile(ref fp, cModels);
            }
            foreach (DynamicObject l in cDynamicObjects)
            {
                l.PrintToFile(ref fp, cAudio, cModels);
            }
            foreach (StartPos s in cStart)
            {
                s.PrintToFile(ref fp);
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
            //if (timeout != 0) //Write it anyways
            {                
                fp.WriteLine("-4\t1");
                timeout *= 1000;  //have to write milliseconds, not seconds...
                fp.WriteLine("\t" + timeout.ToString(".##;-.##;0") + "\t" + timeoutMessageText);
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
            //fp.WriteLine(scale.ToString(".;-.;0"));
            fp.WriteLine(scale.ToString());

            //Write Move and View Speeds..
            fp.WriteLine("-8\t1");
            fp.WriteLine("0.005\t0.03"); // Don't export real speeds

            //Ambient light
            fp.WriteLine("-20\t1");
            fp.WriteLine((ambientColor.R / 255.0).ToString(".##;-.##;0") + "\t" + (ambientColor.G / 255.0).ToString(".##;-.##;0") + "\t" + (ambientColor.B / 255.0).ToString(".##;-.##;0") + "\t" + ambientIntensity.ToString(".##;-.##;0"));

            //start message
            fp.WriteLine("-21\t1");
            fp.WriteLine( (startMessageEnable ? "1" : "0") + "\t" + startMessageText);

            //skybox texture
            fp.WriteLine("-22\t1");
            //int textureIndex = 0;
            //if (skyTexture != null) textureIndex = skyTexture.Index;
            //fp.WriteLine(textureIndex);
            string imageID = "0";
            if (cImages.ContainsKey(skyTexture))
                imageID = cImages[skyTexture];
            fp.WriteLine(imageID);

            fp.Close();
            return true;
        }

        //private Texture GetTexture(int id)
        //{
        //    if (id == -999) //marked bad
        //        return null;

        //    foreach(Texture t in cImages)
        //    {
        //        if (t.Index == id)
        //            return t;
        //    }
        //    return null;
        //}

        //private Model GetModel(int id)
        //{
        //    if (id == -999)
        //        return null;
        //    foreach (Model t in modelLibraryItems)
        //    {
        //        if (t.Index == id)
        //            return t;
        //    }
        //    return null;
        //}

        //private Audio GetAudio(int id)
        //{
        //    if (id == -999)
        //        return null;

        //    foreach (Audio t in cAudio)
        //    {
        //        if (t.Index == id)
        //            return t;
        //    }
        //    return null;
        //}

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
            Color col = new Color();
            int texture = 0;
            Floor tFloor;
            Wall tWall;
            Texture tTex;
            EndRegion tEn;
            StartPos sPos;
            try
            {
                while (true)
                {
                    buf = fp.ReadLine();
                    cmd = Int32.Parse(buf);
                    switch (cmd)
                    {
                        case 0: // plane
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
                            if (AreEqual(tempPoint.Y, tempPoint2.Y) && AreEqual(tempPoint2.Y, tempPoint3.Y) && AreEqual(tempPoint3.Y, tempPoint4.Y))
                            {
                                //floor
                                if (tempPoint.Y < 0)
                                {
                                    //new floor
                                    tFloor = new Floor(scale, "");
                                    //tFloor.floorTexID = texture;
                                    //tFloor.FloorTexture = GetTexture(texture);
                                    if (cImages.ContainsKey(texture.ToString()))
                                        tFloor.FloorTexture = cImages[texture.ToString()];
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
                                            //f.ceilingTexID = texture;
                                            //f.CeilingTexture = GetTexture(texture);
                                            if (cImages.ContainsKey(texture.ToString()))
                                                f.CeilingTexture = cImages[texture.ToString()];
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
                                tWall = new Wall(scale, "");
                                //tWall.texID = texture;
                                //tWall.Texture = GetTexture(texture);
                                if (cImages.ContainsKey(texture.ToString()))
                                    tWall.Texture = cImages[texture.ToString()];
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
                                    //tTex = new Texture(cMazeDirectory, buf.Substring(tab + 1), cmd);
                                    //cImages.Add(tTex);
                                    string filePath = buf.Substring(tab + 1);
                                    string fileName = filePath.Substring(filePath.LastIndexOf("\\") + 1);
                                    if (fileName == "")
                                        fileName = filePath;

                                    cImages[cmd.ToString()] = fileName;
                                    ImagePathConverter.Paths[fileName] = filePath;
                                }
                                else
                                    break;
                            }
                            break;
                        case -2:    //start pos
                            buf = fp.ReadLine();
                            sPos = new StartPos(scale, "");
                            tab = buf.IndexOf('\t');
                            tempPoint = new MPoint();
                            tempPoint.X = double.Parse(buf.Substring(0, tab));
                            tab2 = buf.IndexOf('\t', tab + 1);
                            tempPoint.Y = double.Parse(buf.Substring(tab + 1, tab2 - tab));
                            tempPoint.Z = double.Parse(buf.Substring(tab2 + 1));
                            sPos.MzPoint = tempPoint;
                            cStart.Add(sPos);
                            break;
                        case -3:    //end region
                            buf = fp.ReadLine();
                            tEn = new EndRegion(scale, "");
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
                            timeout = double.Parse(buf) / 1000;
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

        public bool ReadClassicFormat(ref StreamReader fp)
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
            StartPos sPos;
            int texture = 0;
            Floor tFloor;
            Wall tWall;
            Texture tTex;
            EndRegion tEn;
            Model mMod;
            Audio mAudio;
            string[] parsed;
            int flag1, flag2;
            int flag3=1; //mainly for visibility
            int flag4 = 0;//flip for wall
            int lineNum = 1;
            int id = 0;
            string label = "";
            while ((buf = fp.ReadLine()) != null)
            {
                lineNum++;               
                try
                {
                    parsed = buf.Split(new char[] { '\t' });
                    if(parsed.Length<=0)
                    {
                        continue;
                    }
                    else if(parsed.Length>2)
                    {
                        cmd =  Int32.Parse(parsed[0]);
                        tab2 = Int32.Parse(parsed[1]);
                        id = Int32.Parse(parsed[2]);
                        label = parsed[3];
                    }
                    else
                    {
                        cmd =  Int32.Parse(parsed[0]);
                        tab2 = Int32.Parse(parsed[1]);
                        id=0;
                        label="";
                    }
                    //tab = buf.IndexOf('\t');
                    // parsed[2] and parsed[3] carries ID and label respectively....

                    /*
                    tab = buf.IndexOf('\t');
                    if(tab==-1)
                        cmd = Int32.Parse(buf);
                    else
                        cmd = Int32.Parse(buf.Substring(0,tab));

                    tab2 = Int32.Parse(buf.Substring(tab + 1));*/
                    switch (cmd)
                    {
                        case 0:     //plane
                            flag3 = 1; //set default...
                            buf = fp.ReadLine(); lineNum++;
                            ReadColorLine(ref buf, ref texture, ref col);
                            buf = fp.ReadLine(); lineNum++;
                            tempPoint = new MPoint();
                            ReadALine(ref buf, ref tem, ref tempPoint);
                            buf = fp.ReadLine(); lineNum++;
                            tempPoint2 = new MPoint();
                            ReadALine(ref buf, ref tem2, ref tempPoint2);
                            buf = fp.ReadLine(); lineNum++;
                            tempPoint3 = new MPoint();
                            ReadALine(ref buf, ref tem3, ref tempPoint3);
                            buf = fp.ReadLine(); lineNum++;
                            tempPoint4 = new MPoint();
                            ReadALine(ref buf, ref tem4, ref tempPoint4);
                            if (tab2 > 5)
                            {
                                buf = fp.ReadLine(); lineNum++;
                                string[] parts = buf.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
                                flag1 = int.Parse(parts[0]);
                                flag2 = int.Parse(parts[1]);
                                flag3 = int.Parse(parts[2]);  //visibility
                                flag4 = int.Parse(parts[3]);  //flip texture for wall
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
                            if (flag1 == 1)
                            {
                                //floor
                                //if (tempPoint.Y < 0)
                                if (flag2 == 0)
                                {
                                    //new floor
                                    tFloor = new Floor(scale, label, id);
                                    //tFloor.floorTexID = texture;
                                    //tFloor.FloorTexture = GetTexture(texture);
                                    if (cImages.ContainsKey(texture.ToString()))
                                        tFloor.FloorTexture = cImages[texture.ToString()];
                                    tFloor.FloorColor = col;
                                    tFloor.MzPoint1 = tempPoint;
                                    tFloor.MzPoint2 = tempPoint2;
                                    tFloor.MzPoint3 = tempPoint3;
                                    tFloor.MzPoint4 = tempPoint4;
                                    tFloor.FloorVertex1 = new TPoint(tem.X, tem.Y);
                                    tFloor.FloorVertex2 = new TPoint(tem2.X, tem2.Y);
                                    tFloor.FloorVertex3 = new TPoint(tem3.X, tem3.Y);
                                    tFloor.FloorVertex4 = new TPoint(tem4.X, tem4.Y);
                                    int mappingIndex = Texture.GetMappingIndex(tFloor.FloorVertex1, tFloor.FloorVertex2, tFloor.FloorVertex3, tFloor.FloorVertex4);
                                    String mode = Texture.GetMode(tFloor.FloorVertex1, tFloor.FloorVertex2, tFloor.FloorVertex3, tFloor.FloorVertex4);
                                    double tileSize = Texture.GetTileSize(tFloor.FloorVertex1, tFloor.FloorVertex2, tFloor.FloorVertex3, tFloor.FloorVertex4, tFloor.MzPoint1, tFloor.MzPoint2, tFloor.MzPoint3, tFloor.MzPoint4);
                                    double aspectRatio = Texture.GetAspectRatio(tFloor.FloorVertex1, tFloor.FloorVertex2, tFloor.FloorVertex3, tFloor.FloorVertex4, tFloor.MzPoint1, tFloor.MzPoint2, tFloor.MzPoint3, tFloor.MzPoint4);
                                    tFloor.AssignInitVals(mode, mappingIndex, tileSize, aspectRatio, true);
                                    tFloor.Ceiling = false;
                                    tFloor.Visible = ((flag3 == 0) ? true : false);
                                    cFloor.Add(tFloor);
                                }
                                else
                                {
                                    //ceiling - search for its floor
                                    foreach (Floor f in cFloor)
                                    {
                                        if (AreEqual(f.MzPoint1.X, tempPoint.X) && AreEqual(f.MzPoint1.Z, tempPoint.Z) && AreEqual(f.MzPoint2.X, tempPoint2.X) && AreEqual(f.MzPoint2.Z, tempPoint2.Z))
                                        {
                                            //f.ceilingTexID = texture;
                                            if (cImages.ContainsKey(texture.ToString()))
                                                f.CeilingTexture = cImages[texture.ToString()];
                                            f.Ceiling = true;
                                            f.CeilingColor = col;
                                            f.CeilingVertex1 = new TPoint(tem.X, tem.Y);
                                            f.CeilingVertex2 = new TPoint(tem2.X, tem2.Y);
                                            f.CeilingVertex3 = new TPoint(tem3.X, tem3.Y);
                                            f.CeilingVertex4 = new TPoint(tem4.X, tem4.Y);
                                            f.CeilingHeight = tempPoint.Y - f.MzPoint1.Y;
                                            int mappingIndex = Texture.GetMappingIndex(f.CeilingVertex1, f.CeilingVertex2, f.CeilingVertex3, f.CeilingVertex4);
                                            String mode = Texture.GetMode(f.CeilingVertex1, f.CeilingVertex2, f.CeilingVertex3, f.CeilingVertex4);
                                            double tileSize = Texture.GetTileSize(f.CeilingVertex1, f.CeilingVertex2, f.CeilingVertex3, f.CeilingVertex4, f.MzPoint1, f.MzPoint2, f.MzPoint3, f.MzPoint4);
                                            double aspectRatio = Texture.GetAspectRatio(f.CeilingVertex1, f.CeilingVertex2, f.CeilingVertex3, f.CeilingVertex4, f.MzPoint1, f.MzPoint2, f.MzPoint3, f.MzPoint4);
                                            f.AssignInitVals(mode, mappingIndex, tileSize, aspectRatio, false);
                                            break;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                //wall                                
                                tWall = new Wall(scale, label, id);
                                //tWall.TextureIndex = texture;
                                if (cImages.ContainsKey(texture.ToString()))
                                    tWall.Texture = cImages[texture.ToString()];
                                tWall.Color = col;
                                tWall.MzPoint1 = tempPoint;
                                tWall.MzPoint2 = tempPoint2;
                                tWall.MzPoint3 = tempPoint3;
                                tWall.MzPoint4 = tempPoint4;
                                tWall.Vertex1 = new TPoint(tem.X, tem.Y);
                                tWall.Vertex2 = new TPoint(tem2.X, tem2.Y);
                                tWall.Vertex3 = new TPoint(tem3.X, tem3.Y);
                                tWall.Vertex4 = new TPoint(tem4.X, tem4.Y);
                                tWall.Visible = ((flag3 == 0) ? true : false);

                                if ((tempPoint.Y == tempPoint2.Y) && (tempPoint3.Y == tempPoint2.Y) && (tempPoint3.Y == tempPoint4.Y))
                                {
                                    //this is in fact a Floor object!
                                    tFloor = new Floor(scale, label, id);
                                    if (cImages.ContainsKey(texture.ToString()))
                                        tFloor.FloorTexture = cImages[texture.ToString()];
                                    tFloor.FloorColor = col;
                                    tFloor.MzPoint1 = tempPoint;
                                    tFloor.MzPoint2 = tempPoint2;
                                    tFloor.MzPoint3 = tempPoint3;
                                    tFloor.MzPoint4 = tempPoint4;
                                    tFloor.FloorVertex1 = new TPoint(tem.X, tem.Y);
                                    tFloor.FloorVertex2 = new TPoint(tem2.X, tem2.Y);
                                    tFloor.FloorVertex3 = new TPoint(tem3.X, tem3.Y);
                                    tFloor.FloorVertex4 = new TPoint(tem4.X, tem4.Y);
                                    int mappingIndex = Texture.GetMappingIndex(tFloor.FloorVertex1, tFloor.FloorVertex2, tFloor.FloorVertex3, tFloor.FloorVertex4);
                                    String mode = Texture.GetMode(tFloor.FloorVertex1, tFloor.FloorVertex2, tFloor.FloorVertex3, tFloor.FloorVertex4);
                                    double tileSize = Texture.GetTileSize(tFloor.FloorVertex1, tFloor.FloorVertex2, tFloor.FloorVertex3, tFloor.FloorVertex4, tFloor.MzPoint1, tFloor.MzPoint2, tFloor.MzPoint3, tFloor.MzPoint4);
                                    double aspectRatio = Texture.GetAspectRatio(tFloor.FloorVertex1, tFloor.FloorVertex2, tFloor.FloorVertex3, tFloor.FloorVertex4, tFloor.MzPoint1, tFloor.MzPoint2, tFloor.MzPoint3, tFloor.MzPoint4);
                                    tFloor.AssignInitVals(mode, mappingIndex, tileSize, aspectRatio, true);
                                    tFloor.Ceiling = false;
                                    tFloor.Visible = ((flag3 == 0) ? true : false);
                                    cFloor.Add(tFloor);
                                }
                                else
                                {
                                    //continue with wall loading..
                                    double tileSize = Texture.GetTileSize(tWall.Vertex1, tWall.Vertex2, tWall.Vertex3, tWall.Vertex4, tWall.MzPoint1, tWall.MzPoint2, tWall.MzPoint3, tWall.MzPoint4);
                                    String mode = Texture.GetMode(tWall.Vertex1, tWall.Vertex2, tWall.Vertex3, tWall.Vertex4);
                                    int mappingIndex = Texture.GetMappingIndex(tWall.Vertex1, tWall.Vertex2, tWall.Vertex3, tWall.Vertex4);
                                    double aspectRatio = Texture.GetAspectRatio(tWall.Vertex1, tWall.Vertex2, tWall.Vertex3, tWall.Vertex4, tWall.MzPoint1, tWall.MzPoint2, tWall.MzPoint3, tWall.MzPoint4);
                                    tWall.AssignInitVals(mode, mappingIndex, tileSize, aspectRatio, flag4 == 1);

                                    cWall.Add(tWall);
                                }
                            }
                            break;
                        case 1:     //triangle
                            buf = fp.ReadLine(); lineNum++;
                            buf = fp.ReadLine(); lineNum++;
                            buf = fp.ReadLine(); lineNum++;
                            buf = fp.ReadLine(); lineNum++;
                            break;
                        case -1:    //texture
                            tab = 0;
                            cmd = 0;
                            while (cmd != -1)
                            {
                                fp.Read();
                                buf = fp.ReadLine(); lineNum++;
                                tab = buf.IndexOf('\t');
                                if (tab != -1)
                                {
                                    cmd = int.Parse(buf.Substring(0, tab));
                                    //tTex = new Texture(cMazeDirectory, buf.Substring(tab + 1), cmd);
                                    //cImages.Add(tTex);
                                    string filePath = buf.Substring(tab + 1);
                                    string fileName = filePath.Substring(filePath.LastIndexOf("\\") + 1);
                                    if (fileName == "")
                                        fileName = filePath;

                                    cImages[cmd.ToString()] = fileName;
                                    ImagePathConverter.Paths[fileName] = filePath;
                                }
                                else
                                    break;
                            }
                            break;
                        case -2:    //start pos
                            buf = fp.ReadLine(); lineNum++;
                            sPos = new StartPos(scale, label, id);
                            parsed = buf.Split('\t');
                            tempPoint = new MPoint();
                            tempPoint.X = double.Parse(parsed[0]);
                            tempPoint.Y = double.Parse(parsed[1]);
                            tempPoint.Z = double.Parse(parsed[2]);
                            sPos.MzPoint = tempPoint;
                            if (parsed.Length > 3)
                                sPos.AngleYaw = int.Parse(parsed[3]);
                            if (parsed.Length > 4)
                                sPos.AnglePitch = int.Parse(parsed[4]);
                            cStart.Add(sPos);
                            //if (parsed.Length>4 && int.Parse(parsed[4]) > 0)
                            //    cStart.RandomAngle = true;
                            break;
                        case -3:    //end region
                            buf = fp.ReadLine(); lineNum++;
                            tEn = new EndRegion(scale, label, id);
                            parsed = buf.Split('\t');
                            tEn.MinX = float.Parse(parsed[0]);
                            tEn.MaxX = float.Parse(parsed[1]);
                            tEn.MinZ = float.Parse(parsed[2]);
                            tEn.MaxZ = float.Parse(parsed[3]);
                            tEn.SuccessMessage = parsed[4];
                            if (tab2 > 1)
                            {
                                buf = fp.ReadLine(); lineNum++;
                                parsed = buf.Split('\t');
                                tEn.Height = float.Parse(parsed[0]);
                                tEn.Offset = float.Parse(parsed[1]);
                                tEn.ReturnValue = int.Parse(parsed[2]);
                                //tEn.mode= int.Parse(parsed[3]);
                            }
                            cEndRegions.Add(tEn);

                            //old way
                            //buf = fp.ReadLine(); lineNum++;
                            //cEnd = new EndRegion(scale);
                            //parsed = buf.Split('\t');
                            //cEnd.MinX = double.Parse(parsed[0]);
                            //cEnd.MaxX = double.Parse(parsed[1]);
                            //cEnd.MinZ = double.Parse(parsed[2]);
                            //cEnd.MaxZ = double.Parse(parsed[3]);
                            //cEnd.SuccessMessage = parsed[4];
                            break;
                        case -4:    //timeout
                            buf = fp.ReadLine(); lineNum++;
                            int p = buf.IndexOf('\t', 1);

                            TimeoutValue = double.Parse(buf.Substring(0, p)) / 1000;
                            timeoutMessageText = buf.Substring(p + 1);

                            break;
                        case -21:    //startMessage
                            buf = fp.ReadLine(); lineNum++;
                            int p2 = buf.IndexOf('\t', 1);

                            startMessageEnable = int.Parse(buf.Substring(0, p2)) != 0;
                            startMessageText = buf.Substring(p2 + 1);

                            break;
                        case -5:    //designer
                            fp.Read();
                            buf = fp.ReadLine(); lineNum++;
                            //buf.CopyTo(0,desginer,0,buf.Length);
                            desginer = buf;
                            break;
                        case -6:    //comment
                            fp.Read();
                            buf = fp.ReadLine(); lineNum++;
                            //buf.CopyTo(0,comments,0,buf.Length);
                            comments = buf;
                            break;
                        case -7:    //scale
                            buf = fp.ReadLine(); lineNum++;
                            //this.Scale = double.Parse(buf);
                            this.SilentSetScale(double.Parse(buf));
                            break;
                        case -8:    //move and view speed..
                            buf = fp.ReadLine(); lineNum++;
                            tab = buf.IndexOf('\t');
                            //this.moveSpeed = double.Parse(buf.Substring(0,tab)); //Ignore Old Speeds
                            //this.turnSpeed = double.Parse(buf.Substring(tab + 1)); //Ignore Old Speeds
                            break;
                        case -9: //light source..
                            buf = fp.ReadLine(); lineNum++;
                            parsed = buf.Split('\t');
                            Light item = new Light(scale, label, id);
                            tempPoint = new MPoint();
                            tempPoint.X = double.Parse(parsed[0]);
                            tempPoint.Y = double.Parse(parsed[1]);
                            tempPoint.Z = double.Parse(parsed[2]);

                            if (parsed[3].Contains("0"))
                                item.Type = Light.LightTypes.Ambulatory;

                            item.MzPoint = tempPoint;

                            buf = fp.ReadLine(); lineNum++;
                            parsed = buf.Split('\t');

                            item.AmbientColor = Color.FromArgb((int)(255 * double.Parse(parsed[0])), (int)(255 * double.Parse(parsed[1])), (int)(255 * double.Parse(parsed[2])));
                            item.AmbientIntesity = float.Parse(parsed[3]);

                            buf = fp.ReadLine(); lineNum++;
                            parsed = buf.Split('\t');
                            item.DiffuseColor = Color.FromArgb((int)(255 * double.Parse(parsed[0])), (int)(255 * double.Parse(parsed[1])), (int)(255 * double.Parse(parsed[2])));
                            item.DiffuseIntesity = float.Parse(parsed[3]);
                            cLight.Add(item);

                            buf = fp.ReadLine(); lineNum++;
                            item.Attenuation = double.Parse(buf);

                            break;
                        case -10: //model list
                            tab = 0;
                            cmd = 0;
                            while (cmd != -1)
                            {
                                fp.Read();
                                buf = fp.ReadLine(); lineNum++;
                                tab = buf.IndexOf('\t');
                                if (tab != -1)
                                {
                                    cmd = int.Parse(buf.Substring(0, tab));
                                    //mMod = new Model(cMazeDirectory, buf.Substring(tab + 1), cmd);
                                    //modelLibraryItems.Add(mMod);
                                    string filePath = buf.Substring(tab + 1);
                                    string fileName = filePath.Substring(filePath.LastIndexOf("\\") + 1);
                                    if (fileName == "")
                                        fileName = filePath;

                                    cModels[cmd.ToString()] = fileName;
                                    ModelPathConverter.Paths[fileName] = filePath;
                                }
                                else
                                    break;
                            }
                            break;
                        case -11: //audio list
                            tab = 0;
                            cmd = 0;
                            while (cmd != -1)
                            {
                                fp.Read();
                                buf = fp.ReadLine(); lineNum++;
                                tab = buf.IndexOf('\t');
                                if (tab != -1)
                                {
                                    cmd = int.Parse(buf.Substring(0, tab));
                                    //mAudio = new Audio(cMazeDirectory, buf.Substring(tab + 1), cmd);
                                    //cAudio.Add(mAudio);
                                    string filePath = buf.Substring(tab + 1);
                                    string fileName = filePath.Substring(filePath.LastIndexOf("\\" + 1));
                                    if (fileName == "")
                                        fileName = filePath;

                                    cAudio[cmd.ToString()] = fileName;
                                    AudioPathConverter.Paths[fileName] = filePath;
                                }
                                else
                                    break;
                            }
                            break;
                        case 10:  //static model
                            buf = fp.ReadLine(); lineNum++;
                            StaticModel sm = new StaticModel(scale, label, id);
                            if (parsed.Length >= 4)
                            {
                                sm.ID = parsed[2];
                                sm.Label = parsed[3];
                            }
                            parsed = buf.Split('\t');
                            //sm.Model = GetModel(int.Parse(parsed[0]));
                            if (cModels.ContainsKey(parsed[0]))
                                sm.Model = cModels[parsed[0]];
                            tempPoint = new MPoint();
                            tempPoint.X = double.Parse(parsed[1]);
                            tempPoint.Y = double.Parse(parsed[2]);
                            tempPoint.Z = double.Parse(parsed[3]);
                            sm.MzPoint = tempPoint;

                            buf = fp.ReadLine(); lineNum++;
                            parsed = buf.Split('\t');

                            sm.ModelScale = double.Parse(parsed[0]);
                            tempPoint = new MPoint();
                            tempPoint.X = double.Parse(parsed[1]);
                            tempPoint.Y = double.Parse(parsed[2]);
                            tempPoint.Z = double.Parse(parsed[3]);
                            sm.MzPointRot = tempPoint;

                            buf = fp.ReadLine(); lineNum++;
                            parsed = buf.Split('\t');
                            if (parsed[0] == "1")
                                sm.Collision = true;
                            else
                                sm.Collision = false;
                            if (parsed.Length > 1 && parsed[1] == "1")
                                sm.Kinematic = true;
                            else
                                sm.Kinematic = false;
                            if (parsed.Length > 2)
                                sm.Mass = int.Parse(parsed[2]);
                            cStaticModels.Add(sm);
                            break;
                        case 11:  //dynamic model
                            buf = fp.ReadLine(); lineNum++;
                            parsed = buf.Split('\t');
                            DynamicObject dm = new DynamicObject(scale, label, id);
                            //dm.Model = GetModel(int.Parse(parsed[0]));
                            if (cModels.ContainsKey(parsed[0]))
                                dm.Model = cModels[parsed[0]];
                            tempPoint = new MPoint();
                            tempPoint.X = double.Parse(parsed[1]);
                            tempPoint.Y = double.Parse(parsed[2]);
                            tempPoint.Z = double.Parse(parsed[3]);
                            dm.MzPoint = tempPoint;

                            buf = fp.ReadLine(); lineNum++;
                            parsed = buf.Split('\t');

                            dm.ModelScale = double.Parse(parsed[0]);
                            tempPoint = new MPoint();
                            tempPoint.X = double.Parse(parsed[1]);
                            tempPoint.Y = double.Parse(parsed[2]);
                            tempPoint.Z = double.Parse(parsed[3]);
                            dm.MzPointRot = tempPoint;

                            buf = fp.ReadLine(); lineNum++;
                            parsed = buf.Split('\t');
                            if (parsed[0] == "1")
                                dm.Collision = true;
                            else
                                dm.Collision = false;
                            if (parsed.Length>1 && parsed[1] == "1")
                                dm.Kinematic = true;
                            else
                                dm.Kinematic = false;
                            if (parsed.Length > 2)
                                dm.Mass = int.Parse(parsed[2]);
                            if (tab2 == 6)
                            {
                                //dm.TriggerType = (DynamicModel.TriggerTypes) int.Parse( parsed[1] );
                                dm.Phase2Criteria = parsed[1];
                                dm.Phase1ActiveRadius = double.Parse(parsed[2]);
                                dm.EventAction = parsed[3];

                                buf = fp.ReadLine(); lineNum++;
                                parsed = buf.Split('\t');
                                dm.EndScale = double.Parse(parsed[0]);
                                tempPoint = new MPoint();
                                tempPoint.X = double.Parse(parsed[1]);
                                tempPoint.Y = double.Parse(parsed[2]);
                                tempPoint.Z = double.Parse(parsed[3]);
                                dm.MzEndPoint = tempPoint - dm.MzPoint;

                                buf = fp.ReadLine(); lineNum++;
                                parsed = buf.Split('\t');
                                dm.ActionTime = double.Parse(parsed[0]);
                                tempPoint = new MPoint();
                                tempPoint.X = double.Parse(parsed[1]);
                                tempPoint.Y = double.Parse(parsed[2]);
                                tempPoint.Z = double.Parse(parsed[3]);
                                dm.MzEndRot = tempPoint;

                                buf = fp.ReadLine(); lineNum++;
                                parsed = buf.Split('\t');
                                //dm.SwitchToModel = GetModel(int.Parse(parsed[0]));
                                if (cModels.ContainsKey(parsed[0]))
                                    dm.SwitchToModel = cModels[parsed[0]];
                                dm.Phase1HighlightStyle = (DynamicObject.HighlightTypes)int.Parse(parsed[1]);
                            }
                            else
                            {
                                buf = fp.ReadLine(); lineNum++;
                                parsed = buf.Split('\t');
                                dm.Phase1Criteria = HighlightTypeConverter.options[ int.Parse(parsed[0]) ];
                                dm.Phase1HighlightStyle = (DynamicObject.HighlightTypes)int.Parse(parsed[1]);
                                dm.Phase1ActiveRadius = double.Parse(parsed[2]);
                                dm.Phase1AutoTriggerTime = int.Parse(parsed[3]);
                                //dm.Phase1HighlightAudio = GetAudio(int.Parse(parsed[4]));
                                if (cAudio.ContainsKey(parsed[4]))
                                    dm.Phase1HighlightAudio = cAudio[parsed[4]];
                                dm.Phase1HighlightAudioLoop = (int.Parse(parsed[5])>0);
                                dm.Phase1HighlightAudioBehavior = (DynamicObject.AudioBehaviour)int.Parse(parsed[6]);

                                buf = fp.ReadLine(); lineNum++;
                                parsed = buf.Split('\t');
                                dm.Phase2Criteria = TriggerTypeConverter.options[int.Parse(parsed[0])];
                                dm.EventAction = parsed[1];                                
                                dm.Phase2ActiveRadius = double.Parse(parsed[2]);
                                dm.Phase2AutoTriggerTime = int.Parse(parsed[3]);
                                //dm.Phase2EventAudio = GetAudio(int.Parse(parsed[4]));
                                if (cAudio.ContainsKey(parsed[4]))
                                    dm.Phase2EventAudio = cAudio[parsed[4]];
                                dm.Phase2EventAudioLoop = (int.Parse(parsed[5])>0);
                                dm.Phase2EventAudioBehavior = int.Parse(parsed[6]);

                                buf = fp.ReadLine(); lineNum++;
                                parsed = buf.Split('\t');
                                dm.EndScale = double.Parse(parsed[0]);
                                tempPoint = new MPoint();
                                tempPoint.X = double.Parse(parsed[1]);
                                tempPoint.Y = double.Parse(parsed[2]);
                                tempPoint.Z = double.Parse(parsed[3]);
                                dm.MzEndPoint = tempPoint - dm.MzPoint;

                                buf = fp.ReadLine(); lineNum++;
                                parsed = buf.Split('\t');
                                dm.ActionTime = double.Parse(parsed[0]);
                                tempPoint = new MPoint();
                                tempPoint.X = double.Parse(parsed[1]);
                                tempPoint.Y = double.Parse(parsed[2]);
                                tempPoint.Z = double.Parse(parsed[3]);
                                dm.MzEndRot = tempPoint;

                                buf = fp.ReadLine(); lineNum++;
                                parsed = buf.Split('\t');
                                //dm.SwitchToModel = GetModel(int.Parse(parsed[0]));
                                if (cModels.ContainsKey(parsed[0]))
                                    dm.SwitchToModel = cModels[parsed[0]];
                            }

                            cDynamicObjects.Add(dm);
                            break;
                        case -20:
                            buf = fp.ReadLine(); lineNum++;
                            parsed = buf.Split('\t');
                            this.AmbientColor= Color.FromArgb((int)(255 * double.Parse(parsed[0])), (int)(255 * double.Parse(parsed[1])), (int)(255 * double.Parse(parsed[2])));
                            this.AmbientIntesity = float.Parse(parsed[3]);
                            break;
                        case -22:
                            buf = fp.ReadLine(); lineNum++;
                            parsed = buf.Split('\t');
                            //skyTexture = GetTexture(int.Parse(parsed[0]));
                            if (cImages.ContainsKey(parsed[0]))
                                skyTexture = cImages[parsed[0]];
                            break;
                        default:
                            tab2 = Int32.Parse(buf.Substring(tab + 1));
                            for (int i = 0; i < tab2; i++)
                            {
                                buf = fp.ReadLine(); lineNum++;
                            }
                            break;
                    }
                }
                catch
                {
                    if (MessageBox.Show("Error in Maze file\n\nDetails\nType: " + cmd + "\nValue: " + buf + "\n\nLine number: " + lineNum, "Warning", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Warning) == DialogResult.Abort)
                        break;
                }
            }   
            fp.Close();
            return true;
        }

        
        public bool ReadXMLformat_Info(XmlNode infoRoot)
        {
            foreach (XmlNode node in infoRoot.ChildNodes)
            {
                string text = node.InnerText; //or loop through its children as well

                switch (node.Name)
                {
                    case "Author":
                        this.Designer = Tools.getStringFromAttribute(node, "name");
                        this.Comments = Tools.getStringFromAttribute(node, "comments");

                        break;
                }

            }
            return true;
        }

        public bool ReadXMLformat_Global(XmlNode globalRoot)
        {
            foreach (XmlNode node in globalRoot.ChildNodes)
            {
                string text = node.InnerText; //or loop through its children as well
                float r = 1, g = 1, b = 1;

                switch (node.Name)
                {
                    case "General":
                        //this.scale = Tools.getDoubleFromAttribute(node, "scale", this.Scale);
                        break;
                    case "Avatar":
                        this.AvatarScale = Tools.getDoubleFromAttribute(node, "scale", this.AvatarScale);
                        this.avatarModelID = Tools.getIntFromAttribute(node, "id", -999);
                        this.AvatarInitRot.X = Tools.getDoubleFromAttribute(node, "rotX", 0);
                        this.AvatarInitRot.Y = Tools.getDoubleFromAttribute(node, "rotY", 0);
                        this.AvatarInitRot.Z = Tools.getDoubleFromAttribute(node, "rotZ", 0);

                        break;
                    case "Speed":
                        this.moveSpeed = Tools.getDoubleFromAttribute(node, "moveSpeed",this.MoveSpeed);
                        this.turnSpeed = Tools.getDoubleFromAttribute(node, "turnSpeed", this.TurnSpeed);
                        break;
                    case "AmbientLight":
                        r = (float)Tools.getDoubleFromAttribute(node, "r", 1);
                        g = (float)Tools.getDoubleFromAttribute(node, "g", 1);
                        b = (float)Tools.getDoubleFromAttribute(node, "b", 1);
                        //bool.TryParse(node.Attributes["globalLightEnabled"].InnerText, out globalLightEnabled); //is this a thing?
                        this.ambientColor = Tools.getColorFromNode(node,"");
                        this.ambientIntensity = (float)Tools.getDoubleFromAttribute(node, "intensity", this.AmbientIntesity); break;
                    case "StartMessage":
                        this.StartMessageText = Tools.getStringFromAttribute(node, "message",this.StartMessageText);
                        this.StartMessageEnable = Tools.getBoolFromAttribute(node, "enabled",this.StartMessageEnable);
                        break;
                    case "Timeout":
                        this.TimeoutMessageText = Tools.getStringFromAttribute(node, "message",this.TimeoutMessageText);
                        this.TimeoutEnable = Tools.getBoolFromAttribute(node, "enabled",this.TimeoutEnable);
                        this.TimeoutValue = Tools.getDoubleFromAttribute(node, "timeoutValue",this.TimeoutValue);
                        break;
                    case "PointOptions":
                        this.pointOutMessageText = Tools.getStringFromAttribute(node, "messageText", this.pointOutMessageText);
                        this.pointOutThreshold = Tools.getIntFromAttribute(node, "exitThreshold", this.pointOutThreshold);
                        ThresholdComparator eComp;
                        if (Enum.TryParse(Tools.getStringFromAttribute(node, "exitThresholdOperator", this.pointOutThresholdOperator.ToString()), out eComp))
                            this.pointOutThresholdOperator = eComp;
                        this.TimeoutValue = Tools.getDoubleFromAttribute(node, "timeoutValue", this.TimeoutValue);
                        break;
                    case "Skybox":
                        this.skyTextureID = Tools.getIntFromAttribute(node, "id", -999);
                        //For now, check for existence at end of load
                        break;
                    case "DefaultStartPosition":
                        this.defaultStartPosID = Tools.getIntFromAttribute(node, "id", -999);
                        break;
                    case "PerspectiveSettings":
                        this.avatarHeight = Tools.getDoubleFromAttribute(node, "avatarHeight", this.avatarHeight);
                        this.cameraHeight = Tools.getDoubleFromAttribute(node, "cameraHeight", this.cameraHeight);
                        this.ViewPerspective = Tools.getStringFromAttribute(node, "cameraMode", this.ViewPerspective);
                        this.fieldOfView = Tools.getDoubleFromAttribute(node, "fieldOfView", this.fieldOfView);
                        this.useFixedXLocation = Tools.getBoolFromAttribute(node, "fixCameraX", false);
                        this.useFixedZLocation = Tools.getBoolFromAttribute(node, "fixCameraZ", false);
                        this.fixedX = Tools.getDoubleFromAttribute(node, "fixedCameraX", this.fixedX);
                        this.fixedZ = Tools.getDoubleFromAttribute(node, "fixedCameraZ", this.fixedZ);
                        this.TopDownOrientation = Tools.getStringFromAttribute(node, "topDownOrientation", this.TopDownOrientation);
                        this.useXRayDisplay = Tools.getBoolFromAttribute(node, "xRayRendering", false);
                        //this.useMouseToOrient = Tools.getBoolFromAttribute(node, "useMouseToOrient", false);

                        break;
                }

            }
            return true;
        }

        public bool ReadXMLformat_Library(XmlNode libraryRoot) // same for Image,model & audio
        {
            foreach (XmlNode node in libraryRoot.ChildNodes)
            {
                string text = node.InnerText; //or loop through its children as well
                Texture tTex;
                //Model mMod;
                //Audio mAudio;
                int id;
                string filePath;


                switch (node.Name)
                {
                    case "Image":
                        id = Tools.getIntFromAttribute(node,"id");
                        filePath = Tools.getStringFromAttribute(node, "file");
                        if (filePath.Length<2)
                            break;
                        //tTex = new Texture(cMazeDirectory, filePath, id); 
                        //cImages.Add(tTex);
                        string fileName = filePath.Substring(filePath.LastIndexOf("\\") + 1);
                        if (fileName == "")
                            fileName = filePath;

                        cImages[id.ToString()] = fileName;
                        ImagePathConverter.Paths[fileName] = filePath;
                        break;
                    case "Model":
                        id = Tools.getIntFromAttribute(node, "id");
                        filePath = Tools.getStringFromAttribute(node, "file");
                        if (filePath.Length < 2)
                            break;
                        //mMod = new Model(cMazeDirectory, filename, id); 
                        //cModels.Add(mMod);
                        fileName = filePath.Substring(filePath.LastIndexOf("\\") + 1);
                        if (fileName == "")
                            fileName = filePath;

                        cModels[id.ToString()] = fileName;
                        ModelPathConverter.Paths[fileName] = filePath;
                        break;
                    case "Sound":
                        id = Tools.getIntFromAttribute(node, "id");
                        filePath = Tools.getStringFromAttribute(node, "file");
                        if (filePath.Length < 2)
                            break;
                        //mAudio = new Audio(cMazeDirectory, filePath, id);
                        //cAudio.Add(mAudio);
                        fileName = filePath.Substring(filePath.LastIndexOf("\\") + 1);
                        if (fileName == "")
                            fileName = filePath;

                        cAudio[id.ToString()] = fileName;
                        AudioPathConverter.Paths[fileName] = filePath;
                        break;
                }

            }
            return true;
        }
        public void DeleteByID(int id, MazeItemType type)
        {

            int index = 0;
            switch (type)
            {
                case MazeItemType.Wall:
                    index = GetMazeItemByID(id, type);
                    if (index >= 0)
                        cWall.RemoveAt(index);
                    break;
                case MazeItemType.CurvedWall:
                    index = GetMazeItemByID(id, type);
                    if (index >= 0)
                        cCurveWall.RemoveAt(index);
                    break;
                case MazeItemType.Light:
                    index = GetMazeItemByID(id, type);
                    if (index >= 0)
                        cLight.RemoveAt(index);
                    break;
                case MazeItemType.Floor:
                    index = GetMazeItemByID(id, type);
                    if (index >= 0)
                        cFloor.RemoveAt(index);
                    break;
                case MazeItemType.End:
                    index = GetMazeItemByID(id, type);
                    if (index >= 0)
                        cEndRegions.RemoveAt(index);
                    break;
                case MazeItemType.ActiveRegion:
                    index = GetMazeItemByID(id, type);
                    if (index >= 0)
                        cActRegions.RemoveAt(index);
                    break;
                case MazeItemType.Static:
                    index = GetMazeItemByID(id, type);
                    if (index >= 0)
                        cStaticModels.RemoveAt(index);
                    break;
                case MazeItemType.Dynamic:
                    index = GetMazeItemByID(id, type);
                    if (index >= 0)
                        cDynamicObjects.RemoveAt(index);
                    break;
                case MazeItemType.CustomObject:
                    //index = GetMazeItemByID(id, type);
                    //if (index >= 0)
                    //    curMaze.cObject.RemoveAt(index);
                    break;
                case MazeItemType.Start:
                    index = GetMazeItemByID(id, type);
                    if (index >= 0)
                        cStart.RemoveAt(index);
                    break;
            }
            /*if(index>=0) //if something deleted
            {
                UpdateTree();
                SyncSelections();
                RedrawFrame();
            }*/
        }

        public void DeleteItemInCollection(MazeItem mazeItem)
        {
            DeleteByID(mazeItem.GetID(), mazeItem.itemType);
        }

        public void AddItemToCollection(MazeItem mazeItem,int addAtIndex=-1)
        {
            switch (mazeItem.itemType)
            {
                case MazeItemType.Wall:
                    if(addAtIndex<0)
                        cWall.Add((Wall)mazeItem);
                    else
                        cWall.Insert(addAtIndex,(Wall)mazeItem);
                    break;
                case MazeItemType.CurvedWall:

                    if (addAtIndex<0)
                        cCurveWall.Add((CurvedWall)mazeItem);
                    else
                        cCurveWall.Insert(addAtIndex, (CurvedWall)mazeItem);
                    break;
                case MazeItemType.Light:

                    if (addAtIndex<0)
                        cLight.Add((Light)mazeItem);
                    else
                        cLight.Insert(addAtIndex, (Light)mazeItem);
                    break;
                case MazeItemType.Floor:

                    if (addAtIndex<0)
                        cFloor.Add((Floor)mazeItem);
                    else
                        cFloor.Insert(addAtIndex, (Floor)mazeItem);
                    break;
                case MazeItemType.End:

                    if (addAtIndex<0)
                        cEndRegions.Add((EndRegion)mazeItem);
                    else
                        cEndRegions.Insert(addAtIndex, (EndRegion)mazeItem);
                    break;
                case MazeItemType.ActiveRegion:

                    if (addAtIndex<0)
                        cActRegions.Add((ActiveRegion)mazeItem);
                    else
                        cActRegions.Insert(addAtIndex, (ActiveRegion)mazeItem);
                    break;
                case MazeItemType.Static:

                    if (addAtIndex<0)
                        cStaticModels.Add((StaticModel)mazeItem);
                    else
                        cStaticModels.Insert(addAtIndex, (StaticModel)mazeItem);
                    break;
                case MazeItemType.Dynamic:

                    if (addAtIndex<0)
                        cDynamicObjects.Add((DynamicObject)mazeItem);
                    else
                        cDynamicObjects.Insert(addAtIndex, (DynamicObject)mazeItem);
                    break;
                case MazeItemType.CustomObject:
                    //index = GetMazeItemByID(id, type);
                    //if (index >= 0)
                    //    curMaze.cObject.Add(mazeItem);
                    break;
                case MazeItemType.Start:

                    if (addAtIndex<0)
                        cStart.Add((StartPos)mazeItem);
                    else
                        cStart.Insert(addAtIndex, (StartPos)mazeItem);
                    break;
            }

        }

        public int GetMazeItemByID(int id, MazeItemType type)
        {
            int outID = -1;

            switch (type)
            {
                case MazeItemType.Wall:
                    for (int i = 0; i < cWall.Count; i++)
                    {
                        if (cWall[i].GetID() == id)
                        {
                            return (i);
                        }
                    }
                    break;
                case MazeItemType.CurvedWall:
                    for (int i = 0; i < cCurveWall.Count; i++)
                    {
                        if (cCurveWall[i].GetID() == id)
                        {
                            return (i);
                        }
                    }
                    break;
                case MazeItemType.Floor:
                    for (int i = 0; i < cFloor.Count; i++)
                    {
                        if (cFloor[i].GetID() == id)
                        {
                            return (i);
                        }
                    }
                    break;
                case MazeItemType.Light:
                    for (int i = 0; i < cLight.Count; i++)
                    {
                        if (cLight[i].GetID() == id)
                        {
                            return (i);
                        }
                    }
                    break;
                case MazeItemType.End:
                    for (int i = 0; i < cEndRegions.Count; i++)
                    {
                        if (cEndRegions[i].GetID() == id)
                        {
                            return (i);
                        }
                    }
                    break;

                case MazeItemType.ActiveRegion:
                    for (int i = 0; i < cActRegions.Count; i++)
                    {
                        if (cActRegions[i].GetID() == id)
                        {
                            return (i);
                        }
                    }
                    break;
                case MazeItemType.Static:
                    for (int i = 0; i < cStaticModels.Count; i++)
                    {
                        if (cStaticModels[i].GetID() == id)
                        {
                            return (i);
                        }
                    }
                    break;
                case MazeItemType.Dynamic:
                    for (int i = 0; i < cDynamicObjects.Count; i++)
                    {
                        if (cDynamicObjects[i].GetID() == id)
                        {
                            return (i);
                        }
                    }
                    break;
                case MazeItemType.Start:
                    for (int i = 0; i < cStart.Count; i++)
                    {
                        if (cStart[i].GetID() == id)
                        {
                            return (i);
                        }
                    }
                    break;
                case MazeItemType.CustomObject:
                    /*for (int i = 0; i < cMaze.cObject.Count; i++)
                    {
                        if (cMaze.cObject[i].GetID() == id)
                        {
                            return (i);
                        }
                    }*/
                    break;
            }

            return outID;
        }

        public bool ReadXMLformat_MazeItems(XmlNode itemsRoot) // walls& Floors & shapes
        {
            if (itemsRoot == null) //XML file doestn' contain maze
                return false;

            foreach (XmlNode node in itemsRoot.ChildNodes)
            {
                string text = node.InnerText; //or loop through its children as well
                Wall tWall;
                CurvedWall tCWall;
                Floor tFloor;
                StaticModel tModel;
                DynamicObject tDObject;
                EndRegion tEnRegion;
                ActiveRegion tActRegion;
                Light tLight;
                StartPos tStartPos;

                switch (node.Name)
                {
                    case "Walls":
                        foreach (XmlNode wallNode in node.ChildNodes)
                        {
                            if(wallNode.Name=="Wall")
                            {
                                tWall=new MazeMaker.Wall(wallNode);
                                cWall.Add(tWall);
                            }
                            else if(wallNode.Name=="CurvedWall")
                            {
                                tCWall = new MazeMaker.CurvedWall(wallNode);
                                cCurveWall.Add(tCWall);
                            }
                        }
                        break;
                    case "Floors":
                        foreach (XmlNode floorNode in node.ChildNodes)
                        {
                            if (floorNode.Name == "Floor")
                            {
                                tFloor = new MazeMaker.Floor(floorNode);
                                cFloor.Add(tFloor);
                            }
                        }
                        break;
                    case "StartPositions":
                        foreach (XmlNode startNode in node.ChildNodes)
                        {
                            if (startNode.Name == "StartPosition")
                            {
                                tStartPos = new MazeMaker.StartPos(startNode);
                                cStart.Add(tStartPos);
                            }
                        }
                        break;
                    case "EndRegions":
                        foreach (XmlNode endNode in node.ChildNodes)
                        {
                            if (endNode.Name == "EndRegion")
                            {
                                tEnRegion = new MazeMaker.EndRegion(endNode);
                                cEndRegions.Add(tEnRegion);
                            }
                        }
                        break;
                    case "ActiveRegions":
                        foreach (XmlNode actNode in node.ChildNodes)
                        {
                            if (actNode.Name == "ActiveRegion")
                            {
                                tActRegion = new MazeMaker.ActiveRegion(actNode);
                                cActRegions.Add(tActRegion);
                            }
                        }
                        break;
                    case "StaticModels":
                        foreach (XmlNode staticMNode in node.ChildNodes)
                        {
                            if (staticMNode.Name == "StaticModel")
                            {
                                tModel = new StaticModel(staticMNode);
                                cStaticModels.Add(tModel);
                            }
                        }
                        break;
                    case "DynamicObjects":
                        foreach (XmlNode dynamidONode in node.ChildNodes)
                        {
                            if (dynamidONode.Name == "DynamicObject")
                            {
                                tDObject = new MazeMaker.DynamicObject(dynamidONode);
                                cDynamicObjects.Add(tDObject);
                            }
                        }
                        break;
                    case "Lights":
                        foreach (XmlNode lightNode in node.ChildNodes)
                        {
                            if (lightNode.Name == "Light")
                            {
                                tLight = new MazeMaker.Light(lightNode);
                                cLight.Add(tLight);
                            }
                        }
                        break;

                }
            }
            return true;
        }

        string TryCopyFileDir(List<string> dirsToTry, string file, string mazPath, string typeToCopy, ref List<string> copiedFiles, List<string[]> replaceOrder)
        {
            bool ret = false;
            string origCopyFile = "";
            List<string> localDirsToTry;
            switch(typeToCopy)
            {
                case "image":
                    localDirsToTry = new List<string> {"Img","Images","Image","Textures","Tex","Texture" };
                    break;
                case "model":
                    localDirsToTry = new List<string> { "Model","Models","Obj","Objs"};
                    break;
                case "audio":
                    localDirsToTry = new List<string> { "Audio", "Sound", "Sounds" };
                    break;
                default:
                    return "";

            }
            ret = CopyFile(file, mazPath, typeToCopy, ref copiedFiles, replaceOrder,out origCopyFile, false);
            if (!ret)
            {
                foreach (string dir in dirsToTry)
                {
                    string newDir = Path.GetDirectoryName(dir);

                    ret = CopyFile(newDir + file, mazPath, typeToCopy, ref copiedFiles, replaceOrder, out origCopyFile, false);
                    if (ret)
                    {
                        return origCopyFile;
                    }

                    foreach (string dirLocal in localDirsToTry)
                    {
                        ret = CopyFile(newDir + "\\"+dirLocal+ "\\" + file, mazPath, typeToCopy, ref copiedFiles, replaceOrder,out origCopyFile, false);
                        if (ret)
                        {
                            return origCopyFile;
                        }
                    }
                }
                ret = CopyFile(file, mazPath, typeToCopy, ref copiedFiles, replaceOrder,out origCopyFile, true);
                if (ret)
                {
                    copiedFiles.Add("\n Manually selected replacement for " + file + "\n");
                    return origCopyFile;
                }
                else
                {
                    copiedFiles.Add("\n Copy Failed: Could not find " + file + "\n");
                    return "";
                }

            }
            else
                return origCopyFile;



        }

        bool CopyFile(string file, string mazPath, string type, ref List<string> copiedFiles, List<string[]> replaceOrder, out string origCopyFile, bool searchForFile=false)
        {
            string copiedFile = "";

            if (file != "")
            {
                string oldFilePath ="";


        
                string fileName = Path.GetFileName(file);
                string newFilePath = Path.GetFileName(mazPath) + "_assets\\" + type + "\\" + fileName;
                string newFilePathFull = mazPath + "_assets\\" + type + "\\" + fileName;


                switch (type)
                {
                    case "image":
                        oldFilePath = ImagePathConverter.Paths[fileName];

                        break;

                    case "audio":
                        oldFilePath = AudioPathConverter.Paths[fileName];

                        break;

                    case "model":
                        oldFilePath = ModelPathConverter.Paths[fileName];

                        break;
                }

                if (oldFilePath.Length <= 0||file.Contains(":"))
                {
                    oldFilePath = file;
                }

                if (!oldFilePath.Contains(":")) { 
                    oldFilePath= Path.GetDirectoryName(file)+"\\"+oldFilePath ;
                }


                copiedFile = this.RecursiveFileCopy(oldFilePath, mazPath, type, newFilePathFull, ref replaceOrder, searchForFile);

                if(copiedFile.Length>0)
                {
                    switch (type)
                    {
                        case "image":
                            
                            ImagePathConverter.Paths[fileName] = newFilePath;
                            break;

                        case "audio":
                            
                            AudioPathConverter.Paths[fileName] = newFilePath;
                            break;

                        case "model":
                           
                            ModelPathConverter.Paths[fileName] = newFilePath;
                            break;
                    }
                }else
                {
                    origCopyFile = "";
                    return false;
                }
                
                this.ReplaceFiles(replaceOrder);
            }

            if (!this.AddToLog(copiedFile, ref copiedFiles))
            {
                origCopyFile = "";
                //MazeListBuilder.ShowPM(mazPath, "\nPackage failed.", copiedFiles);
                return false;
            }

            origCopyFile = copiedFile;

            return true;
        }

        public string RecursiveFileCopy(string oldFilePath, string mazXpath, string type, string newFilePath, ref List<string[]> replaceOrder, bool searchForFile = false)
        {
            string oldFileName = Path.GetFileName(oldFilePath);
            if (oldFileName == "")
                oldFileName = oldFilePath;
            string mazxDirectory = Path.GetDirectoryName(mazXpath);

            // file already exists in assets
            if (File.Exists(newFilePath))
            {
                return "no new file";
            }

            // checks original location
            if (oldFilePath[0] == '.' && oldFilePath[1] == '.')
                oldFilePath = mazxDirectory + "\\" + oldFilePath;
            //MessageBox.Show(oldFilePath);
            if (File.Exists(oldFilePath))
            {
                File.Copy(oldFilePath, newFilePath);
                return oldFilePath;
            }

            // checks './fileName'
            oldFilePath = mazxDirectory + "\\" + oldFileName;
            //MessageBox.Show(oldFilePath);
            if (File.Exists(oldFilePath))
            {
                File.Copy(oldFilePath, newFilePath);
                return oldFilePath;
            }

            // checks './image/fileName'
            oldFilePath = mazxDirectory + "\\" + type + "\\" + oldFileName;
            //MessageBox.Show(oldFilePath);
            if (File.Exists(oldFilePath))
            {
                File.Copy(oldFilePath, newFilePath);
                return oldFilePath;
            }

            // checks './images/fileName'
            oldFilePath = mazxDirectory + "\\" + type + "s\\" + oldFileName;
            //MessageBox.Show(oldFilePath);
            if (File.Exists(oldFilePath))
            {
                File.Copy(oldFilePath, newFilePath);
                return oldFilePath;
            }

            // checks '../image/fileName'
            oldFilePath = mazxDirectory + "\\..\\" + type + "\\" + oldFileName;
            //MessageBox.Show(oldFilePath);
            if (File.Exists(oldFilePath))
            {
                File.Copy(oldFilePath, newFilePath);
                return oldFilePath;
            }

            // checks '../images/fileName'
            oldFilePath = mazxDirectory + "\\..\\" + type + "s\\" + oldFileName;
            //MessageBox.Show(oldFilePath);
            if (File.Exists(oldFilePath))
            {
                File.Copy(oldFilePath, newFilePath);
                return oldFilePath;
            }

            // give up
            if (searchForFile)
            { 
                MessageBox.Show("\'" + oldFileName + "\' could not be found. If it can't be found, the package will be abandoned");
                while (true)
                {
                    string fileExt = Path.GetExtension(oldFileName).ToLower();
                    List<string> imageExt = new List<string> { ".bmp", ".jpg", ".jpeg", ".gif", ".png" };

                    OpenFileDialog ofd = new OpenFileDialog();
                    if (fileExt == ".maz" || fileExt == ".mazx")
                        ofd.Filter = "Maze Files (*.maz;*.mazx)|*.maz;*.mazx";
                    else if (imageExt.Contains(fileExt))
                        ofd.Filter = "Image Files (*.bmp;*.jpg;*.jpeg;*.gif;*.png)|*.bmp;*.jpg;*.jpeg;*.gif;*.png";
                    else if (fileExt == ".wav" || fileExt == ".mp3")
                        ofd.Filter = "Audio Files (*.wav;*.mp3)|*.wav;*.mp3";
                    else if (fileExt == ".obj")
                        ofd.Filter = "Model Files (*.obj)|*.obj";
                    ofd.Title = "Finding/Replacing " + oldFileName;

                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        string newFileName = ofd.FileName.Substring(ofd.FileName.LastIndexOf("\\") + 1);
                        if (newFileName == "")
                            newFileName = ofd.FileName;

                        if (newFileName.Split('.')[0] != oldFileName.Split('.')[0])
                        {
                            switch (MessageBox.Show("The new file \'" + newFileName + "\' is different from \'" + oldFileName + "\'. Are you sure you want to use this file? Press \'No\' to search again! Press \'Cancel\' to abandon package!", "Are you sure?", MessageBoxButtons.YesNoCancel))
                            {
                                case DialogResult.Yes:
                                    break;

                                case DialogResult.No: // search again
                                    continue;

                                default:
                                    return "";
                            }
                        }

                        newFilePath = newFilePath.Substring(0, newFilePath.Length - newFilePath.Substring(newFilePath.LastIndexOf("\\") + 1).Length) + newFileName;

                        if (File.Exists(newFilePath))
                        {
                            return "";
                        }

                        File.Copy(ofd.FileName, newFilePath);
                        replaceOrder.Add(new string[] { type, oldFileName, newFileName, ofd.FileName });
                        return ofd.FileName;
                    }

                    return "";
                }
            }
            return "";
        }
            
        


        private bool AddToLog(string copiedFile, ref List<string> copiedFiles)
        {
            switch (copiedFile)
            {
                case "no new file":
                    return true;

                case "Failed":
                    //copiedFiles.Add("\nCopy Failed: " + copiedFile;
                    return false;

                default:
                    copiedFiles.Add(copiedFile);
                    return true;
            }
        }


        public void ReplaceFiles(List<string[]> replaceOrder)
        {
            if (replaceOrder.Count != 0)
            {
                foreach (Floor floor in this.cFloor)
                {
                    foreach (string[] replaceInfo in replaceOrder)
                    {
                        if (replaceInfo[0] == "image" && floor.FloorTexture == replaceInfo[1])
                        {
                            ImagePathConverter.Paths.Remove(replaceInfo[1]);
                            floor.FloorTexture = replaceInfo[2];
                            if (replaceInfo[2] != "")
                                ImagePathConverter.Paths[replaceInfo[2]] = replaceInfo[3];
                        }
                        if (replaceInfo[0] == "image" && floor.CeilingTexture == replaceInfo[1])
                        {
                            ImagePathConverter.Paths.Remove(replaceInfo[1]);
                            floor.CeilingTexture = replaceInfo[2];
                            if (replaceInfo[2] != "")
                                ImagePathConverter.Paths[replaceInfo[2]] = replaceInfo[3];
                        }
                    }
                }

                foreach (CurvedWall curvedWall in this.cCurveWall)
                {
                    foreach (string[] replaceInfo in replaceOrder)
                    {
                        if (replaceInfo[0] == "image" && curvedWall.Texture == replaceInfo[1])
                        {
                            ImagePathConverter.Paths.Remove(replaceInfo[1]);
                            curvedWall.Texture = replaceInfo[2];
                            if (replaceInfo[2] != "")
                                ImagePathConverter.Paths[replaceInfo[2]] = replaceInfo[3];
                        }
                    }
                }

                foreach (Wall wall in this.cWall)
                {
                    foreach (string[] replaceInfo in replaceOrder)
                    {
                        if (replaceInfo[0] == "image" && wall.Texture == replaceInfo[1])
                        {
                            ImagePathConverter.Paths.Remove(replaceInfo[1]);
                            wall.Texture = replaceInfo[2];
                            if (replaceInfo[2] != "")
                                ImagePathConverter.Paths[replaceInfo[2]] = replaceInfo[3];
                        }
                    }
                }

                foreach (ActiveRegion activeRegion in this.cActRegions)
                {
                    foreach (string[] replaceInfo in replaceOrder)
                    {
                        if (replaceInfo[0] == "audio" && activeRegion.Phase1HighlightAudio == replaceInfo[1])
                        {
                            AudioPathConverter.Paths.Remove(replaceInfo[1]);
                            activeRegion.Phase1HighlightAudio = replaceInfo[2];
                            if (replaceInfo[2] != "")
                                AudioPathConverter.Paths[replaceInfo[2]] = replaceInfo[3];
                        }
                        if (replaceInfo[0] == "audio" && activeRegion.Phase2EventAudio == replaceInfo[1])
                        {
                            AudioPathConverter.Paths.Remove(replaceInfo[1]);
                            activeRegion.Phase2EventAudio = replaceInfo[2];
                            if (replaceInfo[2] != "")
                                AudioPathConverter.Paths[replaceInfo[2]] = replaceInfo[3];
                        }
                    }
                }

                foreach (DynamicObject dynamicObject in this.cDynamicObjects)
                {
                    foreach (string[] replaceInfo in replaceOrder)
                    {
                        if (replaceInfo[0] == "audio" && dynamicObject.Phase1HighlightAudio == replaceInfo[1])
                        {
                            AudioPathConverter.Paths.Remove(replaceInfo[1]);
                            dynamicObject.Phase1HighlightAudio = replaceInfo[2];
                            if (replaceInfo[2] != "")
                                AudioPathConverter.Paths[replaceInfo[2]] = replaceInfo[3];
                        }
                        if (replaceInfo[0] == "audio" && dynamicObject.Phase2EventAudio == replaceInfo[1])
                        {
                            AudioPathConverter.Paths.Remove(replaceInfo[1]);
                            dynamicObject.Phase2EventAudio = replaceInfo[2];
                            if (replaceInfo[2] != "")
                                AudioPathConverter.Paths[replaceInfo[2]] = replaceInfo[3];
                        }
                        if (replaceInfo[0] == "model" && dynamicObject.Model == replaceInfo[1])
                        {
                            ModelPathConverter.Paths.Remove(replaceInfo[1]);
                            dynamicObject.Model = replaceInfo[2];
                            if (replaceInfo[2] != "")
                                ModelPathConverter.Paths[replaceInfo[2]] = replaceInfo[3];
                        }
                        if (replaceInfo[0] == "model" && dynamicObject.SwitchToModel == replaceInfo[1])
                        {
                            ModelPathConverter.Paths.Remove(replaceInfo[1]);
                            dynamicObject.SwitchToModel = replaceInfo[2];
                            if (replaceInfo[2] != "")
                                ModelPathConverter.Paths[replaceInfo[2]] = replaceInfo[3];
                        }
                    }
                }

                foreach (StaticModel staticModel in this.cStaticModels)
                {
                    foreach (string[] replaceInfo in replaceOrder)
                    {
                        if (replaceInfo[0] == "model" && staticModel.Model == replaceInfo[1])
                        {
                            ModelPathConverter.Paths.Remove(replaceInfo[1]);
                            staticModel.Model = replaceInfo[2];
                            if (replaceInfo[2] != "")
                                ModelPathConverter.Paths[replaceInfo[2]] = replaceInfo[3];
                        }
                    }
                }

                foreach (string[] replaceInfo in replaceOrder)
                {
                    if (replaceInfo[0] == "image" && this.SkyBoxTexture == replaceInfo[1])
                    {
                        ImagePathConverter.Paths.Remove(replaceInfo[1]);
                        this.SkyBoxTexture = replaceInfo[2];
                        if (replaceInfo[2] != "")
                            ImagePathConverter.Paths[replaceInfo[2]] = replaceInfo[3];
                    }
                    if (replaceInfo[0] == "model" && this.AvatarModel == replaceInfo[1])
                    {
                        ModelPathConverter.Paths.Remove(replaceInfo[1]);
                        this.AvatarModel = replaceInfo[2];
                        if (replaceInfo[2] != "")
                            ModelPathConverter.Paths[replaceInfo[2]] = replaceInfo[3];
                    }
                    if (replaceInfo[0] == "image") // unused files
                    {
                        ImagePathConverter.Paths.Remove(replaceInfo[1]);
                        if (replaceInfo[2] != "")
                            ImagePathConverter.Paths[replaceInfo[2]] = replaceInfo[3];
                    }
                    if (replaceInfo[0] == "audio")
                    {
                        AudioPathConverter.Paths.Remove(replaceInfo[1]);
                        if (replaceInfo[2] != "")
                            AudioPathConverter.Paths[replaceInfo[2]] = replaceInfo[3];
                    }
                    if (replaceInfo[0] == "model")
                    {
                        ModelPathConverter.Paths.Remove(replaceInfo[1]);
                        if (replaceInfo[2] != "")
                            ModelPathConverter.Paths[replaceInfo[2]] = replaceInfo[3];
                    }
                }

                replaceOrder.Clear();
            }
        }

        public bool Package(string mazPath, out List<string> copiedFiles, List<string[]> replaceOrder, bool zipMazX)
        {
            if (mazPath.Length == 0)
            {
                copiedFiles = new List<string>();
                return false;
            }
            bool ret;

            string origMazePath = this.FileName;

            string directory = Path.GetDirectoryName(mazPath);
            string fileName = Path.GetFileName(mazPath);
            string fileNameNoExt = Path.GetFileNameWithoutExtension(mazPath);


            ret = Package(this, null, mazPath, origMazePath, out copiedFiles, replaceOrder);

            if (!ret)
                return false;
            else
            {
                if(!zipMazX)
                    return true;
                else
                {
                    string tempPath = directory + "\\Temp"; // make temp dir
                    if (Directory.Exists(tempPath))
                        Directory.Delete(tempPath, true);
                    Directory.CreateDirectory(tempPath);

                    string newMazPath = tempPath + "\\" + fileName; // move stuff into temp dir
                    string assetsPath = mazPath + "_assets";
                    string newAssetsPath = tempPath + "\\" + fileName + "_assets";

                    Directory.Move(mazPath, newMazPath);
                    Directory.Move(assetsPath, newAssetsPath);

                    string zipPath = directory + "\\" + fileNameNoExt + ".mazx"; // zip stuff in temp dir
                    if (File.Exists(zipPath))
                        File.Delete(zipPath);

                    ZipFile.CreateFromDirectory(tempPath, zipPath);

                    Directory.Delete(tempPath, true); // delete temp dir

                    return true;
                }
                
            }

        }

        private bool Package(object sender, EventArgs e, string mazPath, string origMazePath, out List<string> copiedFiles, List<string[]> replaceOrder)
        {
            
            copiedFiles = new List<string>();
            string directory = Path.GetDirectoryName(mazPath);
            string assetsPath = mazPath + "_assets";

            if (!File.Exists(mazPath))   //TODO: save maze
            {
                this.SaveToMazeXML(mazPath);
            }

            if (File.Exists(mazPath))
            { 
                //curMaze.Package(mazPath, zip);   //Make Package function in maze.cs with everything below here
                string tempPath = directory + "\\Temp";
                if (Directory.Exists(tempPath))
                    Directory.Delete(tempPath, true);
                Directory.CreateDirectory(assetsPath + "\\image");
                Directory.CreateDirectory(assetsPath + "\\audio");
                Directory.CreateDirectory(assetsPath + "\\model");

                List<string> copiedFilesList = new List<string>();
                string fileToCopy = "";
                string typeToCopy = "";
                string oldFilePath = "";
                List<string> modelMaterialFileList = new List<string>();
                List<string> modelTextureFileList = new List<string>();
                bool ret = true;

                int itemId = -1;

                List<string> dirsToTry = new List<string> { origMazePath, Settings.userLibraryFolder, Settings.standardLibraryFolder, mazPath };
                
                foreach (Floor floor in this.cFloor) 
                {
                    if(floor.FloorTexture.Length>0)
                    {
                        fileToCopy = floor.FloorTexture;
                        typeToCopy = "image";
                        if (!copiedFiles.Contains(fileToCopy))
                        {

                            oldFilePath = TryCopyFileDir(dirsToTry, fileToCopy, mazPath, typeToCopy, ref copiedFiles, replaceOrder);

                            if (oldFilePath.Length == 0)
                            {
                                copiedFiles.Add("\nError Copying " + fileToCopy + "!\nPackage aborted");
                                return false;
                            }
                            else
                            {
                                copiedFilesList.Add(fileToCopy);
                                
                            }
                        }
                        
                    }
                    if (floor.CeilingTexture.Length > 0)
                    {
                        fileToCopy = floor.CeilingTexture;
                        typeToCopy = "image";
                        if (!copiedFilesList.Contains(fileToCopy))
                        {

                            oldFilePath = TryCopyFileDir(dirsToTry, fileToCopy, mazPath, typeToCopy, ref copiedFiles, replaceOrder);

                            if (oldFilePath.Length == 0)
                            {
                                copiedFiles.Add("\nError Copying " + fileToCopy + "!\nPackage aborted");
                                return false;
                            }
                            else
                            {
                                copiedFilesList.Add(fileToCopy);
                                copiedFiles.Add(": " + fileToCopy);
                            }
                        }
                    }
                }
                foreach (CurvedWall curvedWall in this.cCurveWall)
                {
                    if (curvedWall.Texture.Length > 0)
                    {
                        fileToCopy = curvedWall.Texture;
                        typeToCopy = "image";
                        if (!copiedFilesList.Contains(fileToCopy))
                        {

                            oldFilePath = TryCopyFileDir(dirsToTry, fileToCopy, mazPath, typeToCopy, ref copiedFiles, replaceOrder);

                            if (oldFilePath.Length == 0)
                            {
                                copiedFiles.Add("\nError Copying " + fileToCopy + "!\nPackage aborted");
                                return false;
                            }
                            else
                            {
                                copiedFilesList.Add(fileToCopy);
                                copiedFiles.Add(": " + fileToCopy);
                            }
                        }
                    }
                }
                foreach (Wall wall in this.cWall)
                {
                    if (wall.Texture.Length > 0)
                    {
                        fileToCopy = wall.Texture;
                        typeToCopy = "image";
                        if (!copiedFilesList.Contains(fileToCopy))
                        {

                            oldFilePath = TryCopyFileDir(dirsToTry, fileToCopy, mazPath, typeToCopy, ref copiedFiles, replaceOrder);

                            if (oldFilePath.Length == 0)
                            {
                                copiedFiles.Add("\nError Copying " + fileToCopy + "!\nPackage aborted");
                                return false;
                            }
                            else
                            {
                                copiedFilesList.Add(fileToCopy);
                                copiedFiles.Add(": " + fileToCopy);
                            }
                        }
                    }
                }
                foreach (ActiveRegion activeRegion in this.cActRegions)
                {
                    
                    if (activeRegion.Phase1HighlightAudio.Length > 0)
                    {
                        fileToCopy = activeRegion.Phase1HighlightAudio;
                        typeToCopy = "audio";
                        if (!copiedFilesList.Contains(fileToCopy))
                        {

                            oldFilePath = TryCopyFileDir(dirsToTry, fileToCopy, mazPath, typeToCopy, ref copiedFiles, replaceOrder);

                            if (oldFilePath.Length == 0)
                            {
                                copiedFiles.Add("\nError Copying " + fileToCopy + "!\nPackage aborted");
                                return false;
                            }
                            else
                            {
                                copiedFilesList.Add(fileToCopy);
                                copiedFiles.Add(": " + fileToCopy);
                            }
                        }
                    }
                    if (activeRegion.Phase2EventAudio.Length > 0)
                    {
                        fileToCopy = activeRegion.Phase2EventAudio;
                        typeToCopy = "audio";
                        if (!copiedFilesList.Contains(fileToCopy))
                        {

                            oldFilePath = TryCopyFileDir(dirsToTry, fileToCopy, mazPath, typeToCopy, ref copiedFiles, replaceOrder);

                            if (oldFilePath.Length == 0)
                            {
                                copiedFiles.Add("\nError Copying " + fileToCopy + "!\nPackage aborted");
                                return false;
                            }
                            else
                            {
                                copiedFilesList.Add(fileToCopy);
                                copiedFiles.Add(": " + fileToCopy);
                            }
                        }
                    }
                }
                foreach (DynamicObject dynamicObject in this.cDynamicObjects)
                {
                    if (dynamicObject.Phase1HighlightAudio.Length > 0)
                    {
                        fileToCopy = dynamicObject.Phase1HighlightAudio;
                        typeToCopy = "audio";
                        if (!copiedFilesList.Contains(fileToCopy))
                        {

                            oldFilePath = TryCopyFileDir(dirsToTry, fileToCopy, mazPath, typeToCopy, ref copiedFiles, replaceOrder);

                            if (oldFilePath.Length == 0)
                            {
                                copiedFiles.Add("\nError Copying " + fileToCopy + "!\nPackage aborted");
                                return false;
                            }
                            else
                            {
                                copiedFilesList.Add(fileToCopy);
                                copiedFiles.Add(": " + fileToCopy);
                            }
                        }
                    }
                    if (dynamicObject.Phase2EventAudio.Length > 0)
                    {
                        fileToCopy = dynamicObject.Phase2EventAudio;
                        typeToCopy = "audio";
                        if (!copiedFilesList.Contains(fileToCopy))
                        {

                            oldFilePath = TryCopyFileDir(dirsToTry, fileToCopy, mazPath, typeToCopy, ref copiedFiles, replaceOrder);

                            if (oldFilePath.Length == 0)
                            {
                                copiedFiles.Add("\nError Copying " + fileToCopy + "!\nPackage aborted");
                                return false;
                            }
                            else
                            {
                                copiedFilesList.Add(fileToCopy);
                                copiedFiles.Add(": " + fileToCopy);
                            }
                        }
                    }
                    if (dynamicObject.Model.Length > 0)
                    {
                        fileToCopy = dynamicObject.Model;
                        typeToCopy = "model";
                        if (!copiedFilesList.Contains(fileToCopy))
                        {

                            oldFilePath = TryCopyFileDir(dirsToTry, fileToCopy, mazPath, typeToCopy, ref copiedFiles, replaceOrder);

                            if (oldFilePath.Length == 0)
                            {
                                copiedFiles.Add("\nError Copying " + fileToCopy + "!\nPackage aborted");
                                return false;
                            }
                            else
                            {
                                copiedFilesList.Add(fileToCopy);
                                copiedFiles.Add(": " + fileToCopy);

                                ret = RecursiveModelCopy(oldFilePath, mazPath, ref modelMaterialFileList, ref modelTextureFileList, ref copiedFiles);
                                if (!ret)
                                {
                                    copiedFiles.Add("\nError while copying Model assets" + fileToCopy + "!\nPackage aborted");
                                }
                            }
                        }
                    }
                    if (dynamicObject.SwitchToModel.Length > 0)
                    {
                        fileToCopy = dynamicObject.SwitchToModel;
                        typeToCopy = "model";
                        if (!copiedFilesList.Contains(fileToCopy))
                        {

                            oldFilePath = TryCopyFileDir(dirsToTry, fileToCopy, mazPath, typeToCopy, ref copiedFiles, replaceOrder);

                            if (oldFilePath.Length == 0)
                            {
                                copiedFiles.Add("\nError Copying " + fileToCopy + "!\nPackage aborted");
                                return false;
                            }
                            else
                            {
                                copiedFilesList.Add(fileToCopy);
                                copiedFiles.Add(": " + fileToCopy);

                                ret = RecursiveModelCopy(oldFilePath, mazPath, ref modelMaterialFileList, ref modelTextureFileList, ref copiedFiles);
                                if (!ret)
                                {
                                    copiedFiles.Add("\nError while copying Model assets" + fileToCopy + "!\nPackage aborted");
                                }
                            }
                        }
                    }
                }
                foreach (StaticModel staticModel in this.cStaticModels)
                {
                    if (staticModel.Model.Length > 0)
                    {
                        fileToCopy = ModelPathConverter.Paths[staticModel.Model];
                        typeToCopy = "model";

                        if (!copiedFilesList.Contains(fileToCopy))
                        {

                            oldFilePath = TryCopyFileDir(dirsToTry, fileToCopy, mazPath, typeToCopy, ref copiedFiles, replaceOrder);

                            if (oldFilePath.Length == 0)
                            {
                                copiedFiles.Add("\nError Copying " + fileToCopy + "!\nPackage aborted");
                                return false;
                            }
                            else
                            {
                                copiedFilesList.Add(fileToCopy);
                                copiedFiles.Add(": " + fileToCopy);

                                ret = RecursiveModelCopy(oldFilePath, mazPath, ref modelMaterialFileList, ref modelTextureFileList,ref copiedFiles);
                                if(!ret)
                                {
                                    copiedFiles.Add("\nError while copying Model assets: " + fileToCopy + "!\nPackage aborted");
                                }
                            }
                        }
                    }
                }
                if (this.SkyBoxTexture.Length > 0)
                {
                    fileToCopy = this.SkyBoxTexture;
                    typeToCopy = "image";
                    if (!copiedFilesList.Contains(fileToCopy))
                    {

                        oldFilePath = TryCopyFileDir(dirsToTry, fileToCopy, mazPath, typeToCopy, ref copiedFiles, replaceOrder);

                        if (oldFilePath.Length == 0)
                        {
                            copiedFiles.Add("\nError Copying " + fileToCopy + "!\nPackage aborted");
                            return false;
                        }
                        else
                        {
                            copiedFilesList.Add(fileToCopy);
                            copiedFiles.Add(": " + fileToCopy);
                        }
                    }
                }
                if (this.AvatarModel.Length > 0)
                {
                    fileToCopy = this.AvatarModel;
                    typeToCopy = "model";
                    if (!copiedFilesList.Contains(fileToCopy))
                    {

                        oldFilePath = TryCopyFileDir(dirsToTry, fileToCopy, mazPath, typeToCopy, ref copiedFiles, replaceOrder);

                        if (oldFilePath.Length == 0)
                        {
                            copiedFiles.Add("\nError Copying " + fileToCopy + "!\nPackage aborted");
                            return false;
                        }
                        else
                        {
                            copiedFilesList.Add(fileToCopy);
                            copiedFiles.Add(": " + fileToCopy);

                            ret = RecursiveModelCopy(oldFilePath, mazPath, ref modelMaterialFileList, ref modelTextureFileList, ref copiedFiles);
                            if (!ret)
                            {
                                copiedFiles.Add("\nError while copying Model assets" + fileToCopy + "!\nPackage aborted");
                            }
                        }
                    }
                }
                this.SaveToMazeXML(mazPath);
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool RecursiveModelCopy(string origModelPath, string mazPath, ref List<string> materialFiles, ref List<string> imageFiles, ref List<string> copiedFiles)
        {
            if(origModelPath.Length==0||origModelPath=="no new file")
            {
                return true;
            }

       

            FileInfo fi = new FileInfo(origModelPath);
            StreamReader reader = fi.OpenText();

            materialFiles.Clear();
            imageFiles.Clear();

            string origModelDir=System.IO.Path.GetDirectoryName(origModelPath);
            string line;

            while ((line = reader.ReadLine()) != null)
            {
                string[] items = line.Split(' ');
                if(items.Length > 0 && items[0].Contains("mtllib"))
                {
                    for (int j=1;j<items.Length;j++)
                    {
                        if(items[j].ToLower().Contains(".mtl"))
                        {
                            string matFilePath = origModelDir + "\\" + items[j];
                            if (!materialFiles.Contains(matFilePath))
                                materialFiles.Add(matFilePath);
                            break;
                        }
                    }    
                    
                }
            }

            List<int> imageIdx = new List<int>();

            for (int j = 0; j < materialFiles.Count; j++)
            {
                string origMaterialDir = System.IO.Path.GetDirectoryName(materialFiles[j]);
                if (File.Exists(materialFiles[j]))
                { 
                    fi = new FileInfo(materialFiles[j]);
                    reader = fi.OpenText();

                    while ((line = reader.ReadLine()) != null)
                    {
                        string[] items = line.Split(' ');
                        if (items.Length > 0 &&
                            (items[0].Contains("map") || items[0].Contains("disp") || items[0].Contains("bump") || items[0].Contains("decal")|| items[0].Contains("refl")))
                        {
                            if (items.Length > 1 && items[items.Length-1].ToLower().Contains("."))
                            {
                                string imgFilePath = origMaterialDir + "\\" + items[items.Length - 1];
                                if (!imageFiles.Contains(imgFilePath))
                                { 
                                    imageFiles.Add(imgFilePath);
                                    imageIdx.Add(j);
                                }
                                break;
                            
                            }
                       
                        }
                    }
                }
                else
                {
                    return false;
                }
            }
        

            bool ret = true;
            for (int j = 0; j < materialFiles.Count; j++) {
                ret=CopyFile(materialFiles[j], mazPath, "model", origModelDir);
                if (!ret)
                {
                    return false;
                }
                else
                {
                    copiedFiles.Add("\n  Material: "+materialFiles[j]);
                }
            }

            if (imageFiles.Count > imageIdx.Count)
                return false;

            for (int j = 0; j < imageFiles.Count; j++)
            {
                string origMaterialDir = System.IO.Path.GetDirectoryName(materialFiles[imageIdx[j]]);
                ret =CopyFile(imageFiles[j], mazPath, "model", origMaterialDir);
                if (!ret)
                {
                    return false;
                }
                else
                {
                    copiedFiles.Add("\n  Texture: "+imageFiles[j]);
                }
            }

            return true;
        }
        public static bool CopyFile(string file, string mazPath, string type,string origFileRootDir="")
        {
            //string copiedFile = "no new file";


            if (file.Contains(":"))
            {
                origFileRootDir = Path.GetDirectoryName(file);
                file = Path.GetFileName(file);
            }
            else if (origFileRootDir.Length>0)
            {
                //origFileRootDir = Path.GetDirectoryName(file);
                if (file.Contains(":") && origFileRootDir.Contains(":"))
                    file = file.Substring(origFileRootDir.Length);

                
            }

            file = file.Replace("/", "\\");
            if (file.StartsWith("\\"))
            {
                file = file.Substring(1);
            }


            string oldFilePath = origFileRootDir + "\\" + file;
            
            string newRootMazPath = mazPath + "_assets\\" + type;
            string newFilePath = newRootMazPath+ "\\" + file;
            string mazDirectory = Path.GetDirectoryName(mazPath);
            string oldFileName = oldFilePath.Substring(oldFilePath.LastIndexOf("\\") + 1);

            if (file.Length == 0)
                return true;
            // copiedFile = MazeListBuilder.RecursiveFileCopy(oldFilePath, mazPath, type, newFilePath);
            if (oldFilePath != newFilePath)
            {
                if (oldFilePath[0] == '.' && oldFilePath[1] == '.')
                    oldFilePath = mazDirectory + "\\" + oldFilePath;
                //MessageBox.Show(oldFilePath);
                if (File.Exists(oldFilePath))
                {
                    string[] newDirs = file.Split('\\');
                    string newDir = newRootMazPath;
                    for (int d=0;d<newDirs.Length-1; d++)
                    {
                        if (newDirs[d].Length > 0)
                        {
                            newDir = newDir + "\\" + newDirs[d];
                            Directory.CreateDirectory(newDir);
                        }
                    }
                    File.Copy(oldFilePath, newFilePath,true);
                    return true;
                }

                oldFilePath = mazDirectory + "\\" + oldFileName;
                //MessageBox.Show(oldFilePath);
                if (File.Exists(oldFilePath))
                {
                    string[] newDirs = file.Split('\\');
                    string newDir = newRootMazPath;
                    for (int d = 0; d < newDirs.Length - 1; d++)
                    {
                        if (newDirs[d].Length > 0)
                        {
                            newDir = newDir + "\\" + newDirs[d];
                            Directory.CreateDirectory(newDir);
                        }
                    }
                    File.Copy(oldFilePath, newFilePath,true);
                    return true;
                }

                return false;
            }
            return true;
        }



        public bool ReadXMLformat(string inp)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(inp);

            XmlNode root = doc.DocumentElement.SelectSingleNode("/MazeFile");

            if (root == null) //XML file doestn' contain maze
                return false;


            XmlNode fileVersionNode = root.Attributes["version"];
            string fileVersion="unknown";
            if (fileVersionNode != null)
                fileVersion = fileVersionNode.InnerText;

            foreach (XmlNode node in doc.DocumentElement.ChildNodes)
            {
                string text = node.InnerText; //or loop through its children as well

                switch(node.Name)
                { 
                    case "Info":
                        ReadXMLformat_Info(node);
                        break;
                    case "Global":
                        ReadXMLformat_Global(node);
                        break;
                    case "ImageLibrary":
                        ReadXMLformat_Library(node);
                        break;
                    case "ModelLibrary":
                        ReadXMLformat_Library(node);
                        break;
                    case "AudioLibrary":
                        ReadXMLformat_Library(node);
                        break;
                    case "MazeItems":
                        ReadXMLformat_MazeItems(node);
                        break;

                }

            }

            ReassociatePositionsTexturesModelsAndAudio();
            ValidateIndicies();

            

            
            return true;
        }

        private void ValidateIndicies()
        {
            foreach (Wall w in cWall)
            {
                if (w.GetID()==-1)
                {
                    w.SetID();
                }
            }
            foreach (Light l in cLight)
            {
                if (l.GetID() == -1)
                {
                    l.SetID();
                }
            }
            foreach (StaticModel l in cStaticModels)
            {
                if (l.GetID() == -1)
                {
                    l.SetID();
                }
            }
            foreach (DynamicObject l in cDynamicObjects)
            {
                if(l.GetID() == -1)
                {
                    l.SetID();
                }
            }
            foreach (CustomObject c in cObject)
            {
                /*if (c.GetID() == -1)
                {
                    c.SetID();
                }*/
            }
            foreach (Floor f in cFloor)
            {
                if (f.GetID() == -1)
                {
                    f.SetID();
                }
            }
            foreach (StartPos sPos in cStart)
            {
                if (sPos.GetID() == -1)
                {
                    sPos.SetID();
                }

            }

            foreach (EndRegion en in cEndRegions)
            {
                if (en.GetID() == -1)
                {
                    en.SetID();
                }

            }

            foreach (ActiveRegion en in cActRegions)
            {
                if (en.GetID() == -1)
                {
                    en.SetID();
                }

            }
        }
        private void ReassociatePositionsTexturesModelsAndAudio()
        {
            //this.skyTexture = this.GetTexture(this.skyTextureID);
            if (cImages.ContainsKey(skyTextureID.ToString()))
                skyTexture = cImages[skyTextureID.ToString()];
            //this.AvatarModel = this.GetModel(this.avatarModelID);
            if (cModels.ContainsKey(avatarModelID.ToString()))
                AvatarModel = cModels[avatarModelID.ToString()];
            int startPosIndex = this.GetMazeItemByID(this.defaultStartPosID, MazeItemType.Start);
            int dObjectIndex;
            if (startPosIndex >= 0)
                this.DefaultStartPos = cStart[startPosIndex];
            else
                this.DefaultStartPos = null;

            foreach (Wall w in cWall)
            {
                //w.Texture = GetTexture(w.texID);
                if (cImages.ContainsKey(w.texID.ToString()))
                    w.Texture = cImages[w.texID.ToString()];
            }
            foreach (CurvedWall w in cCurveWall)
            {
                //w.Texture = GetTexture(w.texID);
                if (cImages.ContainsKey(w.texID.ToString()))
                    w.Texture = cImages[w.texID.ToString()];
            }
            foreach (StaticModel l in cStaticModels)
            {
                //l.Model = GetModel(l.modelID);
                if (cModels.ContainsKey(l.modelID.ToString()))
                    l.Model = cModels[l.modelID.ToString()];
            }
            foreach (DynamicObject l in cDynamicObjects)
            {
                //l.Model = GetModel(l.modelID);
                //l.SwitchToModel = GetModel(l.switchToModelID);
                if (cModels.ContainsKey(l.modelID.ToString()))
                    l.Model = cModels[l.modelID.ToString()];
                if (cModels.ContainsKey(l.switchToModelID.ToString()))
                    l.SwitchToModel = cModels[l.switchToModelID.ToString()];

                if (cAudio.ContainsKey(l.phase1HighlightAudioID.ToString()))
                    l.Phase1HighlightAudio = cAudio[l.phase1HighlightAudioID.ToString()];
                if (cAudio.ContainsKey(l.phase2EventAudioID.ToString()))
                    l.Phase2EventAudio = cAudio[l.phase2EventAudioID.ToString()];
            }
            foreach (CustomObject c in cObject)
            {
                /*if (c.texID != -999
                {
                    c.SetID();
                }*/
            }
            foreach (Floor f in cFloor)
            {
                //f.FloorTexture = GetTexture(f.floorTexID);
                //f.CeilingTexture = GetTexture(f.ceilingTexID);
                if (cImages.ContainsKey(f.floorTexID.ToString()))
                    f.FloorTexture = cImages[f.floorTexID.ToString()];
                if (cImages.ContainsKey(f.ceilingTexID.ToString()))
                    f.CeilingTexture = cImages[f.ceilingTexID.ToString()];
            }
            foreach (EndRegion en in cEndRegions)
            {
                startPosIndex = this.GetMazeItemByID(en.moveToPosID, MazeItemType.Start);
                if (startPosIndex >= 0)
                    en.MoveToPos = cStart[startPosIndex];
                else
                    en.MoveToPos = null;
            }

            foreach (ActiveRegion en in cActRegions)
            {
                //en.Phase1HighlightAudio = GetAudio(en.phase1HighlightAudioID);
                //en.Phase2EventAudio = GetAudio(en.phase2EventAudioID);
                if (cAudio.ContainsKey(en.phase1HighlightAudioID.ToString()))
                    en.Phase1HighlightAudio = en.phase1HighlightAudioID.ToString();
                if (cAudio.ContainsKey(en.phase2EventAudioID.ToString()))
                    en.Phase2EventAudio = cAudio[en.phase2EventAudioID.ToString()];

                startPosIndex = this.GetMazeItemByID(en.moveToPosID, MazeItemType.Start);
                if (startPosIndex >= 0)
                    en.MoveToPos = cStart[startPosIndex];
                else
                    en.MoveToPos = null;

                dObjectIndex = this.GetMazeItemByID(en.activatedObjectID, MazeItemType.Dynamic);
                if (dObjectIndex >= 0)
                    en.ActivatedObject = cDynamicObjects[dObjectIndex];
                else
                    en.ActivatedObject = null;
            }
        }

        public bool ReadFromClassicFile(string inp)
        {
            if (!File.Exists(inp))
                return false;

            StreamReader fp = new StreamReader(inp);
            if (fp == null)
            {
                return false;
            }

            cMazeDirectory=Path.GetDirectoryName(inp);
            
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
            return ReadClassicFormat(ref fp);

        }

        public bool ReadFromFileXML(string inp)
        {
            if (!File.Exists(inp))
                return false;

            StreamReader fp = new StreamReader(inp);
            string firstLine=fp.ReadLine();
            string secondLine = fp.ReadLine();

            if(firstLine.Contains("?xml")|| secondLine.Contains("?xml") || firstLine.Contains("MazeFile") || secondLine.Contains("MazeFile"))
            {
                fp.Close();

                cMazeDirectory = Path.GetDirectoryName(inp);
                SetName(inp);
                return ReadXMLformat(inp);
            }
            else if(firstLine.Contains("Maze File"))
            {
                return ReadClassicFormat(ref fp);
            }
            else
            {
                fp.Close();

                return false;
            }

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

        public bool CheckForClosePoint(int x, int y,ref PointF ret, int iViewOffsetX,int iViewOffsetY)
        {
            x = x - iViewOffsetX;
            y = y - iViewOffsetY;
            foreach (Wall w in cWall)
            {
                if (w.CheckClosePoints(x, y,ref ret)==true)
                {
                    ret.X += iViewOffsetX;
                    ret.Y += iViewOffsetY;
                    return true;
                }
            }
            return false;
        }


        public bool ResizeAllCoordinates(double scale)
        {
            //PointF p = new PointF();
            //RectangleF r = new RectangleF();
            foreach (Wall w in cWall)
            {

                w.Rescale(scale);
                w.changed = true;
            }
            foreach (CurvedWall w in cCurveWall)
            {

                w.Rescale(scale);
                w.changed = true;
            }
            foreach (Floor f in cFloor)
            {
                f.Rescale(scale);
                f.changed = true;
            }
            foreach(Light h in cLight)
            {
                h.Rescale(scale);
                h.changed = true;
            }
            foreach (StaticModel h in cStaticModels)
            {
                h.Rescale(scale);
                h.changed = true;
            }
            foreach (DynamicObject h in cDynamicObjects)
            {
                h.Rescale(scale);
                h.changed = true;
            }
            foreach (StartPos sPos in cStart)
            {
                sPos.Rescale(scale);
                sPos.changed = true;
            }
            foreach (EndRegion en in cEndRegions)
            {
                en.Rescale(scale);
                en.changed = true;

            }
            foreach (ActiveRegion en in cActRegions)
            {
                en.Rescale(scale);
                en.changed = true;

            }

            return true;
        }

        public bool ResizeAllCoordinatesXYZ(double scaleX,double scaleY,double scaleZ)
        {
            //PointF p = new PointF();
            //RectangleF r = new RectangleF();
            foreach (Wall w in cWall)
            {

                w.RescaleXYZ(scaleX,scaleY,scaleZ);
                w.changed = true;
            }
            foreach (CurvedWall w in cCurveWall)
            {

                w.RescaleXYZ(scaleX, scaleY, scaleZ);
                w.changed = true;
            }
            foreach (Floor f in cFloor)
            {
                f.RescaleXYZ(scaleX, scaleY, scaleZ);
                f.changed = true;
            }
            foreach (Light h in cLight)
            {
                h.RescaleXYZ(scaleX, scaleY, scaleZ);
                h.changed = true;
            }
            foreach (StaticModel h in cStaticModels)
            {
                h.RescaleXYZ(scaleX, scaleY, scaleZ);
                h.changed = true;
            }
            foreach (DynamicObject h in cDynamicObjects)
            {
                h.RescaleXYZ(scaleX, scaleY, scaleZ);
                h.changed = true;
            }
            foreach (StartPos sPos in cStart)
            {
                sPos.RescaleXYZ(scaleX, scaleY, scaleZ);
                sPos.changed = true;
            }
            foreach (EndRegion en in cEndRegions)
            {
                en.RescaleXYZ(scaleX, scaleY, scaleZ);
                en.changed = true;

            }
            foreach (ActiveRegion en in cActRegions)
            {
                en.RescaleXYZ(scaleX, scaleY, scaleZ);
                en.changed = true;

            }

            return true;
        }
        public bool GetMaxScreenCoordinates(out PointF leftTop, out PointF rightBottom)
        {
            leftTop = new PointF(0, 0);
            rightBottom = new PointF(0, 0);
            try
            {
                float minX = 10000, minY = 10000;
                float maxX = -10000, maxY = -10000;
                //RectangleF r = new RectangleF();
                foreach (Wall w in cWall)
                {
                    minX = Math.Min(minX, w.ScrPoint1.X);
                    minY = Math.Min(minY, w.ScrPoint1.Y);
                    minX = Math.Min(minX, w.ScrPoint2.X);
                    minY = Math.Min(minY, w.ScrPoint2.Y);

                    maxX = Math.Max(maxX, w.ScrPoint1.X);
                    maxY = Math.Max(maxY, w.ScrPoint1.Y);
                    maxX = Math.Max(maxX, w.ScrPoint2.X);
                    maxY = Math.Max(maxY, w.ScrPoint2.Y);
                }
                foreach (Floor f in cFloor)
                {
                    minX = Math.Min(minX, f.Rect.X);
                    minY = Math.Min(minY, f.Rect.Y);

                    maxX = Math.Max(maxX, f.Rect.X);
                    maxY = Math.Max(maxY, f.Rect.Y);
                }
                foreach (Light l in cLight)
                {
                    minX = Math.Min(minX, l.ScrPoint.X);
                    minY = Math.Min(minY, l.ScrPoint.Y);

                    maxX = Math.Max(maxX, l.ScrPoint.X);
                    maxY = Math.Max(maxY, l.ScrPoint.Y);
                }
                foreach (StaticModel f in cStaticModels)
                {
                    minX = Math.Min(minX, f.ScrPoint.X);
                    minY = Math.Min(minY, f.ScrPoint.Y);

                    maxX = Math.Max(maxX, f.ScrPoint.X);
                    maxY = Math.Max(maxY, f.ScrPoint.Y);
                }
                foreach (DynamicObject f in cDynamicObjects)
                {
                    minX = Math.Min(minX, f.ScrPoint.X);
                    minY = Math.Min(minY, f.ScrPoint.Y);

                    maxX = Math.Max(maxX, f.ScrPoint.X);
                    maxY = Math.Max(maxY, f.ScrPoint.Y);
                }
                foreach (StartPos sPos in cStart)
                {
                    minX = Math.Min(minX, sPos.ScrPoint.X);
                    minY = Math.Min(minY, sPos.ScrPoint.Y);

                    maxX = Math.Max(maxX, sPos.ScrPoint.X);
                    maxY = Math.Max(maxY, sPos.ScrPoint.Y);

                }
                foreach (EndRegion en in cEndRegions)
                {
                    minX = Math.Min(minX, en.Rect.X);
                    minY = Math.Min(minY, en.Rect.Y);

                    maxX = Math.Max(maxX, en.Rect.X);
                    maxY = Math.Max(maxY, en.Rect.Y);
                }
                leftTop.X = minX;
                leftTop.Y = minY;

                rightBottom.X = maxX;
                rightBottom.Y = maxY;

                return true;
            }
            catch //(System.Exception ex)
            {
                return false;
            }
        }

        public bool AutoFixPlacement()
        {
            PointF p = new PointF();
            float minX=10000, minY=10000;
            RectangleF r = new RectangleF();
            foreach (Wall w in cWall)
            {                
                minX = Math.Min(minX, w.ScrPoint1.X);
                minY = Math.Min(minY, w.ScrPoint1.Y);
                minX = Math.Min(minX, w.ScrPoint2.X);
                minY = Math.Min(minY, w.ScrPoint2.Y);
                w.changed = true;
            }
            foreach (CurvedWall w in cCurveWall)
            {
                minX = Math.Min(minX, w.ScrPointMid.X-(int)w.scrRadius);
                minY = Math.Min(minY, w.ScrPointMid.Y - (int)w.scrRadius);
                w.changed = true;
            }
            foreach (Floor f in cFloor)
            {
                minX = Math.Min(minX, f.Rect.X);
                minY = Math.Min(minY, f.Rect.Y);
                f.changed = true;
            }
            foreach (Light l in cLight)
            {
                minX = Math.Min(minX, l.ScrPoint.X);
                minY = Math.Min(minY, l.ScrPoint.Y);
                l.changed = true;
            }
            foreach (StaticModel f in cStaticModels)
            {
                minX = Math.Min(minX, f.ScrPoint.X);
                minY = Math.Min(minY, f.ScrPoint.Y);
                f.changed = true;
            }
            foreach (DynamicObject f in cDynamicObjects)
            {
                minX = Math.Min(minX, f.ScrPoint.X);
                minY = Math.Min(minY, f.ScrPoint.Y);
                f.changed = true;
            }
            foreach (StartPos sPos in cStart)
            {
                minX = Math.Min( minX, sPos.ScrPoint.X);
                minY = Math.Min( minY, sPos.ScrPoint.Y);
                sPos.changed = true;
            }
            foreach (EndRegion en in cEndRegions)
            {
                minX = Math.Min(minX, en.Rect.X);
                minY = Math.Min(minY, en.Rect.Y);
                en.changed = true;
            }
            foreach (ActiveRegion en in cActRegions)
            {
                minX = Math.Min(minX, en.Rect.X);
                minY = Math.Min(minY, en.Rect.Y);
                en.changed = true;
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
            foreach (CurvedWall w in cCurveWall)
            {


                p.X = w.ScrPoint1.X - minX;
                p.Y = w.ScrPoint1.Y - minY;
                w.ScrPoint1 = p;
                p.X = w.ScrPoint2.X - minX;
                p.Y = w.ScrPoint2.Y - minY;
                w.ScrPoint2 = p;
                p.X = w.ScrPointMid.X - minX;
                p.Y = w.ScrPointMid.Y - minY;
                w.ScrPointMid = p;

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
            foreach (DynamicObject d in cDynamicObjects)
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
            foreach (StartPos sPos in cStart)
            {
                p.X = sPos.ScrPoint.X - minX;
                p.Y = sPos.ScrPoint.Y - minY;
                sPos.ScrPoint = p;
            }
            foreach (EndRegion en in cEndRegions)
            {
                r.X = en.Rect.Left - minX;
                r.Y = en.Rect.Top - minY;
                r.Width = en.Rect.Width;
                r.Height = en.Rect.Height;
                en.Rect = r;
            }
            foreach (ActiveRegion en in cActRegions)
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

        //public void FinalCheckBeforeWrite()
        //{
        //    CheckForRemovedTextures();
        //    //CheckForRemovedModels();
        //}

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
            name = Path.GetFileNameWithoutExtension(inp);
        }

        //For access members...
        static public Maze mzP;
        //static public List<Texture> GetImages()
        //{
        //    return mzP.cImages;
        //}

        static public List<StartPos> GetStartPositions()
        {
            return mzP.cStart;
        }

        static public List<DynamicObject> GetDynamicObjects()
        {
            return mzP.cDynamicObjects;
        }
    }


    public class ViewPerspectiveConverter : ExpandableObjectConverter
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
            return new StandardValuesCollection(new string[] { "First-Person", "Top-Down", "3/4 View or Fixed" });
        }

    }

    public class OrientationConverter : ExpandableObjectConverter
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
            return new StandardValuesCollection(new string[] { "Free", "North", "South", "East", "West" });
        }

    }
}


