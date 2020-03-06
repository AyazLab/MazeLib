#region Using directives

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Data;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Drawing2D;
using System.Xml;

#endregion

namespace MazeMaker
{
    //[TypeConverterAttribute(typeof(StartPosMoveConverter)), DescriptionAttribute("StartPosition Support")]
    [Serializable]
    public class ActiveRegion : EndRegion
    {
        public ActiveRegion(double dScale, string label="", int id = 0, bool generateNewID = false) //0 for new, -1 for temporary, other for set specific Id
        {
            this.Scale = dScale;
            if (generateNewID)
                id = 0;
            this.SetID(id);
            this.Label = label;

            this.itemType = MazeItemType.ActiveRegion;
        }

        public ActiveRegion(XmlNode actRegNode)
        {
            this.SetID(Tools.getIntFromAttribute(actRegNode, "id", -1));
            this.Label = Tools.getStringFromAttribute(actRegNode, "label", "");

            MPoint temp1 = Tools.getXYZfromNode(actRegNode, -2, "MzCoord");
            MPoint temp2 = Tools.getXYZfromNode(actRegNode, -3, "MzCoord");
            this.MinX = (float)temp1.X;
            this.MaxX = (float)temp2.X;
            this.MinZ = (float)temp1.Z;
            this.MaxZ = (float)temp2.Z;
            this.Height = (float)temp2.Y - (float)temp1.Y;
            this.Offset = (float)(temp2.Y + temp1.Y) / 2;

            //XmlNode parameterNode = actRegNode.SelectSingleNode("Parameters");
            //this.ReturnValue = Tools.getIntFromAttribute(parameterNode, "returnValue");
            //this.SuccessMessage = Tools.getStringFromAttribute(parameterNode, "successMessage");


            XmlNode phase1Node = actRegNode.SelectSingleNode("Phase1Highlight");
            this.Phase1AutoTriggerTime = Tools.getDoubleFromAttribute(phase1Node, "timeElapsedThreshold", 0);
            this.Phase1PointThreshold = Tools.getIntFromAttribute(phase1Node, "pointThreshold", this.Phase1PointThreshold);

            AudioBehaviour aType;
            if (phase1Node != null)
            {
                XmlNode phase1AudioNode = phase1Node.SelectSingleNode("Audio");
                this.phase1HighlightAudioID = Tools.getIntFromAttribute(phase1AudioNode, "id", -999);
                this.Phase1HighlightAudioLoop = Tools.getBoolFromAttribute(phase1AudioNode, "loop", false);
                if (Enum.TryParse(Tools.getStringFromAttribute(phase1AudioNode, "unhighlightAction", this.Phase1HighlightAudioBehavior.ToString()), out aType))
                    this.Phase1HighlightAudioBehavior = aType;
            }

            XmlNode phase2Node = actRegNode.SelectSingleNode("Phase2Event");
            //this.EventAction = Tools.getStringFromAttribute(phase2Node, "triggerAction", this.EventAction);
            this.Phase2AutoTriggerTime = Tools.getDoubleFromAttribute(phase2Node, "triggerTime", 0);
            this.phase2PointThreshold = Tools.getIntFromAttribute(phase2Node, "pointThreshold", this.phase2PointThreshold);
            this.pointsGranted = Tools.getDoubleFromAttribute(phase2Node, "pointsGranted", this.pointsGranted);

            this.InteractRequired = Tools.getBoolFromAttribute(phase2Node, "interactRequired", this.InteractRequired);
            this.Phase2AutoTriggerTime = Tools.getDoubleFromAttribute(phase2Node, "highlightTimeElapsedThreshold", this.Phase2AutoTriggerTime);
            this.ActivatedMessage = Tools.getStringFromAttribute(phase2Node, "activatedMessageText", this.ActivatedMessage);
            this.moveToPosID = Tools.getIntFromAttribute(phase2Node, "moveToPos", -999);
            this.activatedObjectID = Tools.getIntFromAttribute(phase2Node, "activatedObjectID", -999);

            if (phase2Node != null)
            {
                XmlNode phase2AudioNode = phase2Node.SelectSingleNode("Audio");
                this.phase2EventAudioID = Tools.getIntFromAttribute(phase2AudioNode, "id", -999);
                this.phase2EventAudioLoop = Tools.getBoolFromAttribute(phase2AudioNode, "loop", false);
                //this.Phase2EventAudioBehavior = (AudioBehaviour)Enum.Parse(typeof(AudioBehaviour), Tools.getStringFromAttribute(phase2AudioNode, "unhighlightAction", "Continue"));   
            }

            this.itemType = MazeItemType.ActiveRegion;
        }

       // private bool selected = false;
        private double scale = 17;



        [Category("Options")]
        [Browsable(false)]
        [Description("Coordinate transformation coefficient")]
        public override double Scale
        {
            get { return scale; }
            set
            {
                //CalculateModifiedScaleMazeCoordinates(value);
                scale = value;
                ConvertFromMazeCoordinates();
                OnPropertyChanged("Scale");
            }
        }

        public override void CalculateModifiedScaleMazeCoordinates(double newScale)
        {
            if (newScale == 0)
                throw new Exception("Scale can not be zero!");
            if (scale != newScale)
            {
                float coef = (float)(newScale / scale);
                minX *= coef;
                maxX *= coef;
                minZ *= coef;
                maxZ *= coef;
            }
        }

        public override void SilentSetScale(double newScale)
        {
            scale = newScale;
            ConvertFromMazeCoordinates();
            OnPropertyChanged("Scale");
        }

        private Color colorCur = Color.PaleGreen;
        [Browsable(false)]
        [Category("Colors")]
        [Description("The Current color in the editor")]
        public override Color MazeColorRegular
        {
            get { return colorCur; }
            set { colorCur = value;}
        }

        private  Color colorSel = Color.DarkGreen;
        [Browsable(false)]
        [Category("Colors")]
        [Description("The Selection Mode color")]
        public override Color MazeColorSelected
        {
            get { return colorSel; }
            set { colorSel = value; }
        }
        private RectangleF scrRect = new RectangleF(0, 0, 0, 0);
        [Category("Location")]
        [Browsable(false)]
        [Description("Location on screen coordinates")]
        public override RectangleF Rect
        {
            get { return scrRect; }
            set { scrRect = value; ConvertFromScreenCoordinates(); }
        }





        private float minX = 0;
        [Category("2.Maze Coordinates")]
        [Description("Minimum X value of the active region")]
        public override float MinX
        {
            get { return minX; }
            set { minX = value; ConvertFromMazeCoordinates(); OnPropertyChanged("MinX"); }
        }
        private float maxX = 0;
        [Category("2.Maze Coordinates")]
        [Description("Maximum X value of the active region")]
        public override float MaxX
        {
            get { return maxX; }
            set { maxX = value; ConvertFromMazeCoordinates(); OnPropertyChanged("MaxX"); }
        }
        private float minZ = 0;
        [Category("2.Maze Coordinates")]
        [Description("Minimum Z value of the active region")]
        public override float MinZ
        {
            get { return minZ; }
            set { minZ = value; ConvertFromMazeCoordinates(); OnPropertyChanged("MinZ"); }
        }
        private float maxZ = 0;
        [Category("2.Maze Coordinates")]
        [Description("Minimum Z value of the active region")]
        public override float MaxZ
        {
            get { return maxZ; }
            set { maxZ = value; ConvertFromMazeCoordinates(); OnPropertyChanged("MaxZ"); }
        }

        private void ConvertFromScreenCoordinates()
        {
            minX = (float)(scrRect.Left / scale);
            maxX = (float)(scrRect.Right / scale);
            minZ = (float)(scrRect.Top / scale);
            maxZ = (float)(scrRect.Bottom / scale);
        }
        private void ConvertFromMazeCoordinates()
        {
            scrRect.X = (float)(minX * scale);
            scrRect.Y = (float)(minZ * scale);
            scrRect.Width = (float)Math.Abs((maxX * scale) - scrRect.Left);
            scrRect.Height = (float)Math.Abs(((maxZ * scale) - scrRect.Top));
        }



        private float height = 2;
        [Category("3.Y Parameters")]
        [Description("Height of the active region on Y axis")]
        public override float Height
        {
            get { return height; }
            set { height = value; OnPropertyChanged("Height"); }
        }

        private float offset = 0;
        [Category("3.Y Parameters")]
        [Description("Center of the active region on Y axis")]
        public override float Offset
        {
            get { return offset; }
            set { offset = value; OnPropertyChanged("Offset"); }
        }

        private int returnValue = 0;
        [Category("3.Parameters")]
        [DisplayName("Return value")]
        [Description("Return value to indicate this end region after maze ends")]
        [Browsable(false)]
        public override int ReturnValue
        {
            get { return returnValue; }
            set { returnValue = value; OnPropertyChanged("Return"); }
        }

        //private int moveToPosID = -999;
        private StartPos moveToPos = null;
        [Category("5.Phase 2: Event")]
        [Description("What happens when ActiveRegion is activated")]
        [DisplayName("Move To")]
        [TypeConverter(typeof(StartPosMoveWNoneConverter))]
        //[Browsable(false)]
        public override StartPos MoveToPos
        {
            get { return moveToPos; }
            set
            {
                moveToPos = value;
                if (moveToPos == null)
                    moveToPosID = -999;
                else
                    moveToPosID = moveToPos.GetID();
                OnPropertyChanged("MoveToPos");
            }
        }

        private int pointThreshold = 0;
        [Category("4.Action")]
        [Description("MazePoints required to activate")]
        [DisplayName("Point Threshold")]
        [Browsable(false)]
        public override int PointThreshold
        {
            get { return pointThreshold; }
            set { pointThreshold = value; OnPropertyChanged("PointThreshold"); }
        }


        private string successMessage = "";
        [Category("4.Phase 1: Highlight")]
        [Description("Message to be shown when ActiveRegion is reached")]
        [DisplayName("Message Text")]
        [Browsable(false)]
        public override string SuccessMessage
        {
            get { return successMessage; }
            set { successMessage = "";}
        }

        private int phase1PointThreshold = 0;
        [Category("4.Phase 1: Highlight")]
        [Description("Points required to Highlight in addition to other criteria")]
        [DisplayName("Point Threshold")]
        public int Phase1PointThreshold
        {
            get { return phase1PointThreshold; }
            set { phase1PointThreshold = value; OnPropertyChanged("Point Threshold Highlight"); }
        }

        private double phase1AutoTriggerTime = 0;
        [Category("4.Phase 1: Highlight")]
        [Description("Time in seconds relative to beginning of maze required to Highlight")]
        [DisplayName("Maze Time Elapsed")]
        public double Phase1AutoTriggerTime
        {
            get
            {
                return phase1AutoTriggerTime;
            }
            set
            {
                phase1AutoTriggerTime = (int)value;
                OnPropertyChanged("TriggerTimeHighlight");
            }
        }

        private double phase2AutoTriggerTime = 0;
        [Category("5.Phase 2: Event")]
        [Description("Time in seconds relative to Region Highlighing required before Region is Activaed")]
        [DisplayName("Highlight Time Elapsed")]
        public double Phase2AutoTriggerTime
        {
            get
            {
                return phase2AutoTriggerTime;
            }
            set
            {
                OnPropertyChanged("P2TriggerTime");
                phase2AutoTriggerTime = (int)value;
            }
        }


        public int phase1HighlightAudioID = -999;
        private Audio phase1HighlightAudio = null;
        [Category("4.Phase 1: Highlight")]
        [Description("Select audio for highlight event. List can be edited from Maze Collection")]
        [TypeConverter(typeof(AudioConverter))]
        [DisplayName("Audio")]
        public Audio Phase1HighlightAudio
        {
            get { return phase1HighlightAudio; }
            set { phase1HighlightAudio = value; OnPropertyChanged("HighlightAudio"); }
        }

        public int phase2EventAudioID = -999;
        private Audio phase2EventAudio = null;
        [Category("5.Phase 2: Event")]
        [Description("Select audio for trigger event. List can be edited from Maze Collection")]
        [TypeConverter(typeof(AudioConverter))]
        [DisplayName("Audio")]
        public Audio Phase2EventAudio
        {
            get { return phase2EventAudio; }
            set { phase2EventAudio = value; OnPropertyChanged("triggerAudio"); }
        }

        private bool phase1HighlightAudioLoop = false;
        [Category("4.Phase 1: Highlight")]
        [Description("If true, enables continuous play of selected audio during highlight.")]
        [DisplayName("Audio Loop")]
        public bool Phase1HighlightAudioLoop
        {
            get
            {
                return phase1HighlightAudioLoop;
            }
            set
            {
                phase1HighlightAudioLoop = value;
                OnPropertyChanged("AudioLoop");
            }
        }
        //
        //  0 - Stop
        //  1 - Pause
        //  2 - Continue
        //  3 - Stop at Distance

        public enum AudioBehaviour
        {
            Stop, Pause, Continue, VolumeByDistance
        }

        private int phase1HighlightAudioBehavior = 0;
        [Category("4.Phase 1: Highlight")]
        [Description("Describes behavior for Audio playback when higlight ends.\nStop->Ends playback, Pause->Pauses playback and will resume when model is highlighted, Continue->Continues playback to the end of audio file, VolumeByDistance -> Stops audio on unhighlight and varies volume by distance from object during playback")]
        [DisplayName("Audio on Unhighlight")]
        public AudioBehaviour Phase1HighlightAudioBehavior
        {
            get
            {
                return (AudioBehaviour)phase1HighlightAudioBehavior;
            }
            set
            {
                phase1HighlightAudioBehavior = (int)value;
                OnPropertyChanged("highlightAudioBehavior");
            }
        }

        private bool phase2EventAudioLoop = false;
        [Category("5.Phase 2: Event")]
        [Description("If true, enables continuous play of selected audio after action is triggered.")]
        [DisplayName("Audio Loop")]
        public bool Phase2EventAudioLoop
        {
            get
            {
                return phase2EventAudioLoop;
            }
            set
            {
                phase2EventAudioLoop = value;
                OnPropertyChanged("AudioLoop");
            }
        }

        private bool interactRequired = false;
        [Category("5.Phase 2: Event")]
        [Description("If true, requires Interaction from keyboard or API")]
        [DisplayName("[Interact] to Activate")]
        public bool InteractRequired
        {
            get
            {
                return interactRequired;
            }
            set
            {
                interactRequired = value;
                OnPropertyChanged("InteractRequired");
            }
        }

        private int phase2EventAudioBehavior = 0;
        [Category("5.Phase 2: Event")]
        [Description("Misc parameter for Trigger Audio")]
        [Browsable(false)]
        [DisplayName("Audio Misc")]
        public int Phase2EventAudioBehavior
        {
            get
            {
                return phase2EventAudioBehavior;
            }
            set
            {
                phase2EventAudioBehavior = (int)value;
                OnPropertyChanged("AudioMisc");
            }
        }

        public int activatedObjectID = -999;
        private DynamicObject activatedObject = null;
        [Category("5.Phase 2: Event")]
        [Description("Force Activation of a Dynamic Object (overrides other requirements)")]
        [DisplayName("Activate Object")]
        [TypeConverter(typeof(DynamicObjectConverter))]
        public DynamicObject ActivatedObject
        {
            get { return activatedObject; }
            set
            {
                activatedObject = value;

                if (activatedObject == null)
                    activatedObjectID = -999;
                else
                    activatedObjectID = activatedObject.GetID();
                OnPropertyChanged("activatedObjectID");
            }
        }

        private string activatedMessage = "";
        [Category("5.Phase 2: Event")]
        [Description("Message to be shown when region is Activated")]
        [DisplayName("Activated Message Text")]
        public string ActivatedMessage
        {
            get { return activatedMessage; }
            set { activatedMessage = value; OnPropertyChanged("ActiveRegionText"); }
        }

        private int phase2PointThreshold = 0;
        [Category("5.Phase 2: Event")]
        [Description("MazePoints required to activate")]
        [DisplayName("Point Threshold")]
        public int Phase2PointThreshold
        {
            get { return phase2PointThreshold; }
            set { phase2PointThreshold = value; OnPropertyChanged("PointThreshold"); }
        }
        

        


        private double pointsGranted = 1;
        [Category("5.Phase 2: Event")]
        [Description("Points added to Maze Points when activated")]
        [DisplayName("Points Granted")]
        public double PointsGranted
        {
            get { return pointsGranted; }
            set { pointsGranted = value; OnPropertyChanged("Points Granted"); }
        }

        public override void Move(float mzXdir, float mzZdir)
        {
            minX += mzXdir;
            maxX += mzXdir;
            minZ += mzZdir;
            maxZ += mzZdir;
            
            ConvertFromMazeCoordinates();
        }

        public override void Rescale(double factor)
        {
            minX *= (float)factor;
            maxX *= (float)factor;
            minZ *= (float)factor;
            maxZ *= (float)factor;

            ConvertFromMazeCoordinates();
        }


        public override void RescaleXYZ(double scaleX, double scaleY, double scaleZ)
        {
            minX *= (float)scaleX;
            maxX *= (float)scaleX;
            height *= (float)scaleY;
            offset *= (float)scaleY;
            minZ *= (float)scaleZ;
            maxZ *= (float)scaleZ;

            ConvertFromMazeCoordinates();
        }

        public override bool InRegion(int x1, int y1, int x2, int y2) //currently requires entire area to encompase selection
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

        public override bool IfSelected(int x, int y)
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


        public override void Paint(ref Graphics gr)
        {
            Brush br;
            Pen p = new Pen(Color.Aquamarine, 4);
            if (selected == false)
            {
                //br = new SolidBrush(colorCur);
                br = new HatchBrush(HatchStyle.DashedDownwardDiagonal, Color.Green, colorCur);
            }
            else
            {
                //br = new SolidBrush(colorSel);
                br = new HatchBrush(HatchStyle.ForwardDiagonal, Color.Green, colorSel);

                if(this.activatedObject!=null)
                {
                     p = new Pen(Color.Aquamarine, 4);
                    gr.DrawLine(p, this.scrRect.Location.X+this.scrRect.Width/2,this.scrRect.Location.Y+this.scrRect.Height/2, this.activatedObject.ScrPoint.X, this.activatedObject.ScrPoint.Y);
                }

                if (this.moveToPos != null)
                {
                    
                    gr.DrawLine(p, this.scrRect.Location.X + this.scrRect.Width / 2, this.scrRect.Location.Y + this.scrRect.Height / 2, this.moveToPos.ScrPoint.X, this.moveToPos.ScrPoint.Y);
                }
            }
            p = new Pen(Color.Black, 1);
            gr.FillRectangle(br, scrRect);
            gr.DrawRectangle(p, Rectangle.Round(scrRect));
            br.Dispose();

            
            //gr.DrawRectangle(new Pen(new HatchBrush(HatchStyle.Weave,Color.Blue)), scrRect);
        }


        public override bool PrintToFile(ref StreamWriter fp)
        {
            /*try
            {
                fp.WriteLine("-3\t2\t" + this.GetID().ToString() + "\t" + this.Label);
                fp.WriteLine(minX.ToString(".##;-.##;0") + "\t" + maxX.ToString(".##;-.##;0") + "\t" + minZ.ToString(".##;-.##;0") + "\t" + maxZ.ToString(".##;-.##;0") + "\t" + successMessage);
                fp.WriteLine(height.ToString(".##;-.##;0") + "\t" + offset.ToString(".##;-.##;0") + "\t" + returnValue + "\t0");
                return true;
            }
            catch
            {
                MessageBox.Show("Couldn't write EndRegion...", "MazeMaker", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }*/
            return true;
        }

        public override XmlElement toXMLnode(XmlDocument doc)
        {
            XmlElement actRegNode = doc.CreateElement(string.Empty, "ActiveRegion", string.Empty);
            actRegNode.SetAttribute("label", this.Label);
            actRegNode.SetAttribute("id", this.GetID().ToString());

            XmlElement mzCoordNode = doc.CreateElement(string.Empty, "MzCoord", string.Empty);
            actRegNode.AppendChild(mzCoordNode);
            mzCoordNode.SetAttribute("x1", this.minX.ToString());
            mzCoordNode.SetAttribute("x2", this.maxX.ToString());
            mzCoordNode.SetAttribute("z1", this.minZ.ToString());
            mzCoordNode.SetAttribute("z2", this.maxZ.ToString());
            mzCoordNode.SetAttribute("y1", (this.Offset - this.Height / 2).ToString());
            mzCoordNode.SetAttribute("y2", (this.Offset + this.Height / 2).ToString());

            XmlElement phase1Node = doc.CreateElement(string.Empty, "Phase1Highlight", string.Empty);
            actRegNode.AppendChild(phase1Node);
            phase1Node.SetAttribute("timeElapsedThreshold", this.phase1AutoTriggerTime.ToString());
            phase1Node.SetAttribute("pointThreshold", this.phase1PointThreshold.ToString());

            XmlElement audioNode1 = doc.CreateElement(string.Empty, "Audio", string.Empty);
            phase1Node.AppendChild(audioNode1);
            if (this.Phase1HighlightAudio != null)
            {
                audioNode1.SetAttribute("id", this.Phase1HighlightAudio.Index.ToString());
                audioNode1.SetAttribute("loop", this.Phase1HighlightAudioLoop.ToString());
                audioNode1.SetAttribute("unhighlightBehavior", this.Phase1HighlightAudioBehavior.ToString());
            }

            XmlElement phase2Node = doc.CreateElement(string.Empty, "Phase2Event", string.Empty);
            actRegNode.AppendChild(phase2Node);
            phase2Node.SetAttribute("interactRequired", this.InteractRequired.ToString());
            phase2Node.SetAttribute("highlightTimeElapsedThreshold", this.phase2AutoTriggerTime.ToString());
            phase2Node.SetAttribute("activatedMessageText", this.ActivatedMessage);
            if (this.ActivatedObject!=null)
            {
                phase2Node.SetAttribute("activatedObjectID", this.ActivatedObject.GetID().ToString());
            }
            if (this.MoveToPos != null)
            {
                phase2Node.SetAttribute("moveToPos", this.MoveToPos.GetID().ToString());
            }
            phase2Node.SetAttribute("pointThreshold", this.phase2PointThreshold.ToString());
            phase2Node.SetAttribute("pointsGranted", this.PointsGranted.ToString());


            XmlElement audioNode2 = doc.CreateElement(string.Empty, "Audio", string.Empty);
            phase2Node.AppendChild(audioNode2);
            if (this.Phase2EventAudio != null)
            {
                audioNode2.SetAttribute("id", this.Phase2EventAudio.Index.ToString());
                audioNode2.SetAttribute("loop", this.Phase2EventAudioLoop.ToString());
                //audioNode2.SetAttribute("audioBehavior", this.TriggerAudioBehaviour.ToString());
            }


            return actRegNode;
        }

        new public ActiveRegion Clone()
        {
            return Copy(true, 0);
        }

        new public  ActiveRegion Copy(bool clone, int offsetX=0, int offsetY=0)
        {
            ActiveRegion temp = new ActiveRegion(this.scale, this.Label,-1);
            //temp.ID = this.ID;
            temp.Height = this.Height;
            temp.MaxX = this.MaxX;
            temp.MaxZ = this.MaxZ;
            temp.MinX = this.MinX;
            temp.MinZ = this.MinZ;
            temp.Offset = this.Offset;
            temp.MazeColorRegular = this.MazeColorRegular;
            //temp.ReturnValue = this.ReturnValue;
            temp.SuccessMessage = this.SuccessMessage;
            temp.justCreated = this.justCreated;
            
            temp.phase1PointThreshold = this.phase1PointThreshold;
            temp.phase2PointThreshold = this.phase2PointThreshold;

            temp.Phase1HighlightAudio = this.Phase1HighlightAudio;
            temp.Phase1HighlightAudioBehavior = this.Phase1HighlightAudioBehavior;
            temp.Phase1HighlightAudioLoop = this.Phase1HighlightAudioLoop;
            temp.Phase1AutoTriggerTime = this.Phase1AutoTriggerTime;
            temp.Phase2AutoTriggerTime = this.Phase2AutoTriggerTime;
            temp.Phase2EventAudio = this.Phase2EventAudio;
            temp.Phase2EventAudioLoop = this.Phase2EventAudioLoop;
            temp.activatedObject = this.ActivatedObject;

            temp.Rect = new RectangleF(this.Rect.X + offsetX, this.Rect.Y + offsetY, this.Rect.Width, this.Rect.Height);
            if (clone)
            {
                temp.SetID(this.GetID());
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
