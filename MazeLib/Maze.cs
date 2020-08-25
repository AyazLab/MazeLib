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

#endregion

namespace MazeMaker
{
    public class Maze
    {
        public Maze()
        {
            NameFactory.Reset();
        }

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


        public List<Texture> cImages = new List<Texture>();
        [Category("2.Collections")]
        [Description("Texture Image Files. Place these files to the same directory of with the Maze file or place them in the user library directory")]
        public List<Texture> Image
        {
            get { return cImages; }
            set { cImages = value; }
        }

        [Category("2.Collections")]
        [Description("Available Texture Image Number in the List")]
        [ReadOnly(true)]
        public int ImageCount
        {
            get { return cImages.Count; }
        }

        int modelIDCounter = 100;
        public Dictionary<string, string> cModels = new Dictionary<string, string>();
        [Category("2.Collections")]
        [Description("Model Files. Place these files to the same directory of with the Maze file or place them in the global models directory")]
        public Dictionary<string, string> Model
        {
            get { return ModelPathConverter.Paths; }
            set { ModelPathConverter.Paths = value; }
        }
        
        [Category("2.Collections")]
        [Description("Available Model Number in the List")]
        [ReadOnly(true)]
        public int ModelCount
        {
            get { return ModelPathConverter.Paths.Count; }
        }

        public List<Audio> cAudio = new List<Audio>();
        [Category("2.Collections")]
        [Description("Audio Files. Place these files to the same directory of with the Maze file or place them in the user library directory")]
        public List<Audio> Audio
        {
            get { return cAudio; }
            set { cAudio = value; }
        }

        [Category("2.Collections")]
        [Description("Available Audio Number in the List")]
        [ReadOnly(true)]
        public int AudioCount
        {
            get { return cAudio.Count; }
        }

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
        [Description("Exit Maze when Treshold Reached")]
        [DisplayName("Point Exit Threshold")]
        public int PointOutThreshold
        {
            get { return pointOutThreshold; }
            set { pointOutThreshold = value; }
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
        private Texture skyTexture = null;
        [Category("6.Skybox")]
        [Description("Select texture to be used for skybox. List can be edited at Texture Collection")]
        [TypeConverter(typeof(TextureConverter))]
        public Texture SkyBoxTexture
        {

            get { return skyTexture; }
            set
            {
                if (skyTexture == null)
                {
                    skyTextureID = -999;
                    foreach (Texture t in cImages)
                    {
                        t.Skybox = false;
                    }

                }
                else
                {
                    foreach (Texture t in cImages)
                    {
                        t.Skybox = false;
                    }
                    skyTextureID = skyTexture.Index;
                    skyTexture.Skybox = true;
                }
                skyTexture = value;
            }
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

        public bool SaveToMazeXML(string inp)
        {
            XmlDocument doc = new XmlDocument();

            SetName(inp);

            FinalCheckBeforeWrite();

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
            string modelID = "";
            if (cModels.ContainsKey(AvatarModel))
                modelID = cModels[AvatarModel];
            avatarNode.SetAttribute("id", modelID);
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
                    pointOptionsNode.SetAttribute("messageText", this.PointOutMessageText);

                    if (this.skyTexture != null)
                    { 
                            XmlElement skyboxNode = doc.CreateElement(string.Empty, "Skybox", string.Empty);
                            globalNode.AppendChild(skyboxNode);
                            skyboxNode.SetAttribute("id", this.skyTexture.Index.ToString());
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

                XmlElement texNode;
                foreach (Texture t in cImages)
                {
                    texNode = t.toXMLnode(doc);
                    imageLibraryNode.AppendChild(texNode);
                }

            XmlElement modelLibraryNode = doc.CreateElement(string.Empty, "ModelLibrary", string.Empty);
            mazeXMLnode.AppendChild(modelLibraryNode);

            XmlElement modelLibraryItem;
            foreach (string model in ModelPathConverter.Paths.Keys)
            {
                //modelNode = m.toXMLnode(doc);
                modelLibraryItem = doc.CreateElement(string.Empty, "Model", string.Empty);

                if (!cModels.ContainsKey(model))
                {
                    cModels[model] = modelIDCounter.ToString();
                    modelIDCounter++;
                }

                modelLibraryItem.SetAttribute("id", cModels[model]);
                string filePath = ModelPathConverter.Paths[model];
                if (filePath[1] == ':')
                {
                    filePath = MakeRelativePath(inp, filePath);
                }
                modelLibraryItem.SetAttribute("file", filePath);

                modelLibraryNode.AppendChild(modelLibraryItem);
            }

            XmlElement audioLibraryNode = doc.CreateElement(string.Empty, "AudioLibrary", string.Empty);
            mazeXMLnode.AppendChild(audioLibraryNode);

            XmlElement audioNode;
            foreach (Audio a in cAudio)
            {
                audioNode = a.toXMLnode(doc);
                audioLibraryNode.AppendChild(audioNode);
            }

            XmlElement mazeItemsNode = doc.CreateElement(string.Empty, "MazeItems", string.Empty);
            mazeXMLnode.AppendChild(mazeItemsNode);

            XmlElement wallsNode = doc.CreateElement(string.Empty, "Walls", string.Empty);
            mazeItemsNode.AppendChild(wallsNode);

            XmlElement wallNode;
            foreach (Wall w in cWall)
            {
                wallNode = w.toXMLnode(doc);
                wallsNode.AppendChild(wallNode);
            }

            XmlElement curvedWallNode;
            foreach (CurvedWall w in cCurveWall)
            {
                curvedWallNode = w.toXMLnode(doc);
                wallsNode.AppendChild(curvedWallNode);
            }

            XmlElement floorsNode = doc.CreateElement(string.Empty, "Floors", string.Empty);
            mazeItemsNode.AppendChild(floorsNode);

            XmlElement floorNode;
            foreach (Floor f in cFloor)
            {
                floorNode = f.toXMLnode(doc);
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
                dynamicObjectNode = d.toXMLnode(doc, cModels);
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
                activeRegionNode = ar.toXMLnode(doc);
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

            FinalCheckBeforeWrite();

            changed = false;

            fp.WriteLine("Maze File 1.2");
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
                            if (d.FloorTexture != null && d.FloorTexture.Index == t.Index)
                            {
                                used = true;
                                break;
                            }
                            else if (d.CeilingTexture != null && d.CeilingTexture.Index == t.Index)
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
                        fp.WriteLine("\t{0}\t{1}", "-1", filePath);
                    }

                }
                //index++;
            }

            fp.WriteLine("\t-1");

            ////Audio List//////
            fp.WriteLine("-11\t-1");
            foreach (Audio t in cAudio)
            {
                if (t.Name != "")
                {
                    //fp.WriteLine("\t{0}\t{1}", index, t.Name);   

                    //check weather the texture is in the list...

                    bool used = false;
                    if (used == false)
                    {
                        foreach (DynamicObject d in cDynamicObjects)
                        {
                            if (d.Phase1HighlightAudio != null && d.Phase1HighlightAudio.Index == t.Index)
                            {
                                used = true;
                                break;
                            }
                            if (d.Phase2EventAudio != null && d.Phase2EventAudio.Index == t.Index)
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
                l.PrintToFile(ref fp, cModels);
            }
            foreach (DynamicObject l in cDynamicObjects)
            {
                l.PrintToFile(ref fp, cModels);
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
            int textureIndex = 0;
            if (skyTexture != null) textureIndex = skyTexture.Index;
            fp.WriteLine(textureIndex);

            fp.Close();
            return true;
        }

        private Texture GetTexture(int id)
        {
            if (id == -999) //marked bad
                return null;

            foreach(Texture t in cImages)
            {
                if (t.Index == id)
                    return t;
            }
            return null;
        }

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

        private Audio GetAudio(int id)
        {
            if (id == -999)
                return null;

            foreach (Audio t in cAudio)
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
            StartPos sPos;
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
                                    tFloor = new Floor(scale,"");
                                    //tFloor.TextureIndex = texture;
                                    tFloor.FloorTexture = GetTexture(texture);
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
                                            f.CeilingTexture = GetTexture(texture);
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
                                    //tFloor.TextureIndex = texture;
                                    tFloor.FloorTexture = GetTexture(texture);
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
                                            //f.TextureIndex2 = texture;
                                            f.CeilingTexture = GetTexture(texture);
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
                                tWall.Visible = ((flag3 == 0) ? true : false);

                                if ((tempPoint.Y == tempPoint2.Y) && (tempPoint3.Y == tempPoint2.Y) && (tempPoint3.Y == tempPoint4.Y))
                                {
                                    //this is in fact a Floor object!
                                    tFloor = new Floor(scale, label, id);
                                    tFloor.FloorTexture = GetTexture(texture);
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
                                    tTex = new Texture(cMazeDirectory, buf.Substring(tab + 1), cmd);
                                    cImages.Add(tTex);
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
                                    mAudio = new Audio(cMazeDirectory, buf.Substring(tab + 1), cmd);
                                    cAudio.Add(mAudio);
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
                                dm.Phase1HighlightAudio = GetAudio(int.Parse(parsed[4]));
                                dm.Phase1HighlightAudioLoop = (int.Parse(parsed[5])>0);
                                dm.Phase1HighlightAudioBehavior = (DynamicObject.AudioBehaviour)int.Parse(parsed[6]);

                                buf = fp.ReadLine(); lineNum++;
                                parsed = buf.Split('\t');
                                dm.Phase2Criteria = TriggerTypeConverter.options[int.Parse(parsed[0])];
                                dm.EventAction = parsed[1];                                
                                dm.Phase2ActiveRadius = double.Parse(parsed[2]);
                                dm.Phase2AutoTriggerTime = int.Parse(parsed[3]);
                                dm.Phase2EventAudio = GetAudio(int.Parse(parsed[4]));
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
                            skyTexture = GetTexture(int.Parse(parsed[0]));
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
                Model mMod;
                Audio mAudio;
                int id;
                string filePath;


                switch (node.Name)
                {
                    case "Image":
                        id = Tools.getIntFromAttribute(node,"id");
                        filePath = Tools.getStringFromAttribute(node, "file");
                        if (filePath.Length<2)
                            break;
                        tTex = new Texture(cMazeDirectory, filePath, id); 
                        cImages.Add(tTex);
                        break;
                    case "Model":
                        id = Tools.getIntFromAttribute(node, "id");
                        filePath = Tools.getStringFromAttribute(node, "file");
                        if (filePath.Length < 2)
                            break;
                        //mMod = new Model(cMazeDirectory, filename, id); 
                        //modelLibraryItems.Add(mMod);
                        string fileName = filePath.Substring(filePath.LastIndexOf("\\") + 1);
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
                        mAudio = new Audio(cMazeDirectory, filePath, id);
                        cAudio.Add(mAudio);
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
            this.skyTexture = this.GetTexture(this.skyTextureID);
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
                w.Texture = GetTexture(w.texID);
            }
            foreach (CurvedWall w in cCurveWall)
            {
                w.Texture = GetTexture(w.texID);
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
                l.Phase1HighlightAudio = GetAudio(l.phase1HighlightAudioID);
                l.Phase2EventAudio = GetAudio(l.phase2EventAudioID);
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
                f.FloorTexture = GetTexture(f.floorTexID);
                f.CeilingTexture = GetTexture(f.ceilingTexID);
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
                en.Phase1HighlightAudio = GetAudio(en.phase1HighlightAudioID);
                en.Phase2EventAudio = GetAudio(en.phase2EventAudioID);

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

        public void FinalCheckBeforeWrite()
        {
            CheckForRemovedTextures();
            //CheckForRemovedModels();
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
            name = Path.GetFileNameWithoutExtension(inp);
        }

        //For access members...
        static public Maze mzP;
        static public List<Texture> GetImages()
        {
            return mzP.cImages;
        }

        static public List<StartPos> GetStartPositions()
        {
            return mzP.cStart;
        }

        static public List<DynamicObject> GetDynamicObjects()
        {
            return mzP.cDynamicObjects;
        }

        static public Dictionary<string, string> GetModels()
        {
            //return mzP.cModels;
            return ModelPathConverter.Paths;
        }
        static public List<Audio> GetAudios()
        {
            return mzP.cAudio;
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
                if (cImages.Contains(w.FloorTexture) == false)
                {
                    w.FloorTexture = null;
                }
                if (cImages.Contains(w.CeilingTexture) == false)
                {
                    w.CeilingTexture = null;
                }
            }
        }

        //public void CheckForRemovedModels()
        //{
        //    foreach (StaticModel s in cStaticModels)
        //    {
        //        if( cModels.Contains(s.Model)==false)
        //        {
        //            s.Model = null;
        //        }
        //    }
        //    foreach (DynamicObject s in cDynamicObjects)
        //    {
        //        if (cModels.Contains(s.Model) == false)
        //        {
        //            s.Model = null;
        //        }
        //        if(cModels.Contains(s.SwitchToModel)==false)
        //        {
        //            s.SwitchToModel = null;
        //        }
        //    }
        //}
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


