using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Data;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;
using System.Globalization;
using System.Xml;

namespace MazeMaker
{
    [TypeConverterAttribute(typeof(DynamicObjectConverter)), DescriptionAttribute("Dynamic Object Support")]
    [Serializable]
    public class DynamicObject : StaticModel
    {
        public DynamicObject(double dScale, string label="", int id = 0, bool generateNewID = false) //0 for new, -1 for temporary, other for set specific Id
        {
            this.Scale = dScale;
            if (generateNewID)
                id = 0;
            this.MazeColorRegular = Color.CadetBlue;
            this.SetID(id);
            this.Label = label;
            itemType = MazeItemType.Dynamic;
        }

        public DynamicObject(XmlNode dObjectNode)
        {
            this.MazeColorRegular = Color.CadetBlue;
            this.SetID(Tools.getIntFromAttribute(dObjectNode, "id", -1));
            this.Label = Tools.getStringFromAttribute(dObjectNode, "label", "");

            this.MzPoint = Tools.getXYZfromNode(dObjectNode, 0);
            XmlNode modelNode = dObjectNode.SelectSingleNode("Model");
            XmlNode physicsNode = dObjectNode.SelectSingleNode("Physics");

            this.modelID = Tools.getIntFromAttribute(modelNode, "id", -999);
            this.ModelScale = Tools.getDoubleFromAttribute(modelNode, "scale", 1);

            this.MzPointRot = Tools.getXYZfromNode(dObjectNode, -1, "Model");

            this.Collision = Tools.getBoolFromAttribute(physicsNode, "collision", this.Collision);
            this.Mass = Tools.getDoubleFromAttribute(physicsNode, "mass", this.Mass);
            this.Kinematic = Tools.getBoolFromAttribute(physicsNode, "kinematic", this.Kinematic);

            XmlNode phase1Node = dObjectNode.SelectSingleNode("Phase1Highlight");
            this.Phase1ActiveRadius = Tools.getDoubleFromAttribute(phase1Node, "radius", this.Phase1ActiveRadius);
            this.Phase1Criteria = Tools.getStringFromAttribute(phase1Node, "criteria", this.Phase1Criteria);
            HighlightTypes hType;
            if (Enum.TryParse(Tools.getStringFromAttribute(phase1Node, "highlightStyle", this.Phase1HighlightStyle.ToString()), out hType))
                this.Phase1HighlightStyle = hType;

            this.Phase1AutoTriggerTime = Tools.getDoubleFromAttribute(phase1Node, "triggerTime", 0);
            this.Phase1PointThreshold = Tools.getIntFromAttribute(phase1Node, "pointThreshold");

            AudioBehaviour aType;
            if (phase1Node != null)
            {
                XmlNode phase1AudioNode = phase1Node.SelectSingleNode("Audio");
                this.phase1HighlightAudioID = Tools.getIntFromAttribute(phase1AudioNode, "id", -999);
                this.Phase1HighlightAudioLoop = Tools.getBoolFromAttribute(phase1AudioNode, "loop", false);
                if(Enum.TryParse(Tools.getStringFromAttribute(phase1AudioNode, "unhighlightAction", this.Phase1HighlightAudioBehavior.ToString()),out aType))
                    this.Phase1HighlightAudioBehavior = aType;
            }

            XmlNode phase2Node = dObjectNode.SelectSingleNode("Phase2Event");
            this.Phase2ActiveRadius = Tools.getDoubleFromAttribute(phase2Node, "radius", this.Phase2ActiveRadius);
            this.Phase2Criteria = Tools.getStringFromAttribute(phase2Node, "criteria", this.Phase2Criteria);
            this.ActionTime = Tools.getDoubleFromAttribute(phase2Node, "actionTime", 0);
            this.EventAction = Tools.getStringFromAttribute(phase2Node, "triggerAction", this.EventAction);
            this.Phase2AutoTriggerTime = Tools.getDoubleFromAttribute(phase2Node, "triggerTime", 0);
            this.phase2PointThreshold = Tools.getDoubleFromAttribute(phase2Node, "pointThreshold", this.phase2PointThreshold);
            this.pointsGranted = Tools.getDoubleFromAttribute(phase2Node, "pointsGranted",this.pointsGranted);

            if (phase2Node != null)
            {
                XmlNode phase2AudioNode = phase2Node.SelectSingleNode("Audio");
                this.phase2EventAudioID = Tools.getIntFromAttribute(phase2AudioNode, "id", -999);
                this.phase2EventAudioLoop = Tools.getBoolFromAttribute(phase2AudioNode, "loop", false);
                //this.Phase2EventAudioBehavior = (AudioBehaviour)Enum.Parse(typeof(AudioBehaviour), Tools.getStringFromAttribute(phase2AudioNode, "unhighlightAction", "Continue"));


                XmlNode phase2ModelNode = phase2Node.SelectSingleNode("EndModel");
                this.switchToModelID = Tools.getIntFromAttribute(phase2ModelNode, "switchModelID", -999);
                this.EndScale = Tools.getDoubleFromAttribute(phase2ModelNode, "scale", 1);
                this.MzEndRot = Tools.getXYZfromNode(phase2Node, -1, "EndModel");
                this.MzEndPoint = Tools.getXYZfromNode(phase2Node, 0, "EndMzPoint");
                
            }

            itemType = MazeItemType.Dynamic;

        }



        private int phase1Criteria = 1;
        // [Category("Model Interaction")]
        [Category("5.Phase 1: Highlight")]
        [TypeConverter(typeof(HighlightTypeConverter))]
        [Description("Highlight Trigger Criteria")]
        [DisplayName("Criteria")]
        public String Phase1Criteria
        //public TriggerTypes TriggerType
        {
            get
            {
                //return (TriggerTypes)triggerType;

                if (phase1Criteria >= 0 && phase1Criteria < HighlightTypeConverter.options.Length)
                {
                    return HighlightTypeConverter.options[phase1Criteria];
                }
                else
                    return "Error";

                //if (phase1Criteria == 0)
                //    return "No Type";
                //else if (phase1Criteria == 1)
                //    return "Proximity";
                //else if (phase1Criteria == 2)
                //    return "Test";
                //else if (phase1Criteria == 3)
                //    return "Proximity and Test";
                //else return "Error";
            }
            set
            {

                for (int i = 0; i < HighlightTypeConverter.options.Length; i++)
                {
                    if (value == HighlightTypeConverter.options[i])
                    {
                        phase1Criteria = i;
                        break;
                    }
                }
                OnPropertyChanged("Criteria");


                /*
                    //triggerType = (int)value;
                    if (value == "No Type")
                        triggerType = 0;
                    else if (value == "Proximity")
                        triggerType = 1;
                    else if (value == "Test")
                        triggerType = 2;
                    else if (value == "Proximity and Test")
                        triggerType = 3;
                    else
                        triggerType = int.Parse(value);
                 * */
            }
        }

        private int phase2Criteria = 1;
       // [Category("Model Interaction")]
         [Category("6.Phase 2: Event")]
        [TypeConverter(typeof(TriggerTypeConverter))]
        [Description("Criteria required to activate dynamic object")]
        [DisplayName("Criteria")]
        public String Phase2Criteria
        //public TriggerTypes TriggerType
        {
            get
            {
                if (phase2Criteria >= 0 && phase2Criteria < TriggerTypeConverter.options.Length)
                {
                    return TriggerTypeConverter.options[phase2Criteria];
                }
                else
                    return "Error";
                /*
                //return (TriggerTypes)triggerType;
                if (triggerType == 0)
                    return "No Type";
                else if (triggerType == 1)
                    return "Proximity";
                else if (triggerType == 2)
                    return "Test";
                else if (triggerType == 3)
                    return "Proximity and Test";
                else return "Error";
                 */ 
            }
            set
            {
                for (int i = 0; i < TriggerTypeConverter.options.Length; i++)
                {
                    if (value == TriggerTypeConverter.options[i])
                    {
                        phase2Criteria = i;
                        break;
                    }
                }
                OnPropertyChanged("TriggerCriteria");
                /*
                //triggerType = (int)value;
                if (value == "No Type")
                    triggerType = 0;
                else if (value == "Proximity")
                    triggerType = 1;
                else if (value == "Test")
                    triggerType = 2;
                else if (value == "Proximity and Test")
                    triggerType = 3;
                else
                    triggerType = int.Parse(value);
                 * */
            }
        }
        private int eventAction = 1;
       // [Category("Model Interaction")]
         [Category("6.Phase 2: Event")]
        [TypeConverter(typeof(EventActionConverter))]
        [Description("Model behavior when activated")]
        [DisplayName("Model Action")]
        public String EventAction
        {
            get 
            {
                if (eventAction == 0)
                    return "No Action";
                else if (eventAction == 1)
                    return "Move/Rotate";
                else if (eventAction == 2)
                    return "Change Model";
                else if (eventAction == 3)
                    return "Destroy Model";
                else return "Error";
            }
            set 
            {
                if (value == "No Action")
                    eventAction = 0;
                else if (value == "Move/Rotate")
                    eventAction = 1;
                else if (value == "Change Model")
                    eventAction = 2;
                else if (value == "Destroy Model")
                    eventAction = 3;
                else
                    eventAction = int.Parse(value);
                OnPropertyChanged("TriggerAction");
            }
           
        }

        private double phase1ActiveRadius = 2;
        [Category("5.Phase 1: Highlight")]
        [Description("Active Radius for checking proximity")]
        [DisplayName("Active Radius")]
        public double Phase1ActiveRadius
        {
            get { return phase1ActiveRadius; }
            set { phase1ActiveRadius = value;
                OnPropertyChanged("ActiveRadius");
            }
        }

        private double phase1PointThreshold = 0;
        [Category("5.Phase 1: Highlight")]
        [Description("Points required to Highlight in addition to other criteria")]
        [DisplayName("Point Threshold")]
        public double Phase1PointThreshold
        {
            get { return phase1PointThreshold; }
            set { phase1PointThreshold = value; OnPropertyChanged("Point Threshold Highlight"); }
        }

        private double phase2ActiveRadius = 1;
         [Category("6.Phase 2: Event")]
        [Description("Active Radius for checking proximity")]
        [DisplayName("Active Radius")]
        public double Phase2ActiveRadius
        {
            get { return phase2ActiveRadius; }
            set { phase2ActiveRadius = value; OnPropertyChanged("ActiveRadius");}
        }

        private double actionTime = 3;
         [Category("6.Phase 2: Event")]
        [Description("Total event completion time in seconds")]
        [DisplayName("Action Time")]
        public double ActionTime
        {
            get { return actionTime; }
            set { actionTime = value; OnPropertyChanged("ActionTime"); }
        }

        private double pointsGranted = 0;
        [Category("6.Phase 2: Event")]
        [Description("Points added to Maze Points when activated")]
        [DisplayName("Points Granted")]
        public double PointsGranted
        {
            get { return pointsGranted; }
            set { pointsGranted = value; OnPropertyChanged("Points Granted"); }
        }

        private double phase2PointThreshold = 0;
        [Category("6.Phase 2: Event")]
        [Description("Points required to Activate in addition to other criteria")]
        [DisplayName("Point Threshold")]
        public double Phase2PointThreshold
        {
            get { return phase2PointThreshold; }
            set { phase2PointThreshold = value; OnPropertyChanged("Point Threshold"); }
        }

        private double endScale = 1;
        [Category("6.Phase 2: Event")]
        [Description("End Scale of model after triggering action")]
        [DisplayName("End Scale")]
        public double EndScale
        {
            get { return endScale; }
            set { endScale = value; OnPropertyChanged("EndScale"); }
        }

        private MPoint mzEndPoint = new MPoint(0, 0);
        [Category("6.Phase 2: Event")]
        [DisplayName("End Point")]
        [Description("End location after action with respect to center of object after triggering action")]
        public MPoint MzEndPoint
        {
            get { return mzEndPoint; }
            set { mzEndPoint = value; this.ConvertFromMazeCoordinates(); OnPropertyChanged("EndPoint"); }
        }

        private MPoint mzEndRot = new MPoint(0, 0);
        [Category("6.Phase 2: Event")]
        [DisplayName("End Rotation")]
        [Description("End Rotation after action in absolute degrees after triggering action")]
        public MPoint MzEndRot
        {
            get { return mzEndRot; }
            set { mzEndRot = value; this.ConvertFromMazeCoordinates(); OnPropertyChanged("EndRotation"); }
        }

        public int switchToModelID = -999;
        private string switchToModel = "";
        [Category("6.Phase 2: Event")]
        [Description("Model to switch after the action. List can be edited from the Maze model collection")]
        [TypeConverter(typeof(ModelPathConverter))]
        public string SwitchToModel
        {
            get { return switchToModel; }
            set { switchToModel = value; }

            //set {
            //    switchToModel = value;
                
            //    if (switchToModel == null)
            //        switchToModelID = -999;
            //    else
            //        switchToModelID = switchToModel.Index;
                
            //    OnPropertyChanged("SwitchToModel");
            //}
        }

        public enum HighlightTypes
        {
           
            NoAction =0,
            Bounce=1,
            Shake=2,
            Rotate=3,
            GlowRed=4,
            GlowGreen=5,
            GlowBlue=6,
            ParticleColor1=10,
            ParticleColor2=11,
            ParticleColor3=12,
            ParticleColor4=13,
            ParticleColor5=14,
            ParticleColor6=15,
            ParticleColor7=16,
            ParticleColor8=17,
            ParticleColor9=18,
            ParticleColor10=19,
            ParticleColor11=20,
            ParticleColor12=21,
            FallingParticleColor1=22,
            FallingParticleColor2=23,
            FallingParticleColor3=24,
            FallingParticleColor4=25,
            FallingParticleColor5=26,
            FallingParticleColor6=27,
            FallingParticleColor7=28,
            FallingParticleColor8=29,
            FallingParticleColor9=30,
            FallingParticleColor10=31,
            FallingParticleColor11=32,
            FallingParticleColor12=33
        }              

        private int phase1HighlightStyle = 1;
        [Category("5.Phase 1: Highlight")]
        //[TypeConverter(typeof(HighLightConverter))]
        [Description("Highlight Style")]
        public HighlightTypes Phase1HighlightStyle
        {
            get
            {
                return (HighlightTypes)phase1HighlightStyle;
                /*
                if (highlightStyle == 0)
                    return "No Action";
                else if (highlightStyle == 1)
                    return "Bounce";
                else if (highlightStyle == 2)
                    return "Shake";
                else if (highlightStyle == 3)
                    return "Rotate";
                else if (highlightStyle == 4)
                    return "Glow Red";
                else if (highlightStyle == 5)
                    return "Glow Green";
                else if (highlightStyle == 6)
                    return "Glow Blue";
                else if (highlightStyle == 10)
                    return "Particle Color 1";
                else if (highlightStyle == 11)
                    return "Particle Color 2";
                else if (highlightStyle == 12)
                    return "Particle Color 3";
                else if (highlightStyle == 13)
                    return "Particle Color 4";
                else if (highlightStyle == 14)
                    return "Particle Color 5";
                else if (highlightStyle == 15)
                    return "Particle Color 6";
                else if (highlightStyle == 16)
                    return "Particle Color 7";
                else if (highlightStyle == 17)
                    return "Particle Color 8";
                else if (highlightStyle == 18)
                    return "Particle Color 9";
                else if (highlightStyle == 19)
                    return "Particle Color 10";
                else if (highlightStyle == 20)
                    return "Particle Color 11";
                else if (highlightStyle == 21)
                    return "Particle Color 12";
                else if (highlightStyle == 22)
                    return "Falling Particle Color 1";
                else if (highlightStyle == 23)
                    return "Falling Particle Color 2";
                else if (highlightStyle == 24)
                    return "Falling Particle Color 3";
                else if (highlightStyle == 25)
                    return "Falling Particle Color 4";
                else if (highlightStyle == 26)
                    return "Falling Particle Color 5";
                else if (highlightStyle == 27)
                    return "Falling Particle Color 6";
                else if (highlightStyle == 28)
                    return "Falling Particle Color 7";
                else if (highlightStyle == 29)
                    return "Falling Particle Color 8";
                else if (highlightStyle == 30)
                    return "Falling Particle Color 9";
                else if (highlightStyle == 31)
                    return "Falling Particle Color 10";
                else if (highlightStyle == 32)
                    return "Falling Particle Color 11";
                else if (highlightStyle == 33)
                    return "Falling Particle Color 12";
                else return "Error";
                 * */
            }
            set 
            {
                phase1HighlightStyle = (int)value;
                OnPropertyChanged("HighlightStyle");
                /*
                if (value == "No Action")
                    highlightStyle = 0;
                else if (value == "Bounce")
                    highlightStyle = 1;
                else if (value == "Shake")
                    highlightStyle = 2;
                else if (value == "Rotate")
                    highlightStyle = 3;
                else if (value == "Glow Red")
                    highlightStyle = 4;
                else if (value == "Glow Green")
                    highlightStyle = 5;
                else if (value == "Glow Blue")
                    highlightStyle = 9;
                else if (value == "Particle Color 1")
                    highlightStyle = 10;
                else if (value == "Particle Color 2")
                    highlightStyle = 11;
                else if (value == "Particle Color 3")
                    highlightStyle = 12;
                else if (value == "Particle Color 4")
                    highlightStyle = 13;
                else if (value == "Particle Color 5")
                    highlightStyle = 14;
                else if (value == "Particle Color 6")
                    highlightStyle = 15;
                else if (value == "Particle Color 7")
                    highlightStyle = 16;
                else if (value == "Particle Color 8")
                    highlightStyle = 17;
                else if (value == "Particle Color 9")
                    highlightStyle = 18;
                else if (value == "Particle Color 10")
                    highlightStyle = 19;
                else if (value == "Particle Color 11")
                    highlightStyle = 20;
                else if (value == "Particle Color 12")
                    highlightStyle = 21;
                else if (value == "Falling Particle Color 1")
                    highlightStyle = 22;
                else if (value == "Falling Particle Color 2")
                    highlightStyle = 23;
                else if (value == "Falling Particle Color 3")
                    highlightStyle = 24;
                else if (value == "Falling Particle Color 4")
                    highlightStyle = 25;
                else if (value == "Falling Particle Color 5")
                    highlightStyle = 26;
                else if (value == "Falling Particle Color 6")
                    highlightStyle = 27;
                else if (value == "Falling Particle Color 7")
                    highlightStyle = 28;
                else if (value == "Falling Particle Color 8")
                    highlightStyle = 29;
                else if (value == "Falling Particle Color 9")
                    highlightStyle = 30;
                else if (value == "Falling Particle Color 10")
                    highlightStyle = 31;
                else if (value == "Falling Particle Color 11")
                    highlightStyle = 32;
                else if (value == "Falling Particle Color 12")
                    highlightStyle = 33;
                else
                    highlightStyle = int.Parse(value);
                 * */
            }
        }

        private double phase1AutoTriggerTime = 0;
        [Category("5.Phase 1: Highlight")]
        [Description("Time in seconds relative to beginning of maze for automated Trigger based on trigger criteria")]
        [DisplayName("Trigger Time")]
        public double Phase1AutoTriggerTime
        {
            get
            {
                return phase1AutoTriggerTime;
            }
            set
            {
                phase1AutoTriggerTime = (double)value;
                OnPropertyChanged("TriggerTimeHighlight");
            }
        }

        private double phase2AutoTriggerTime = 0;
        [Category("6.Phase 2: Event")]
        [Description("Time in seconds relative to highlight time (in Phase 1) for automated Trigger based on trigger criteria")]
        [DisplayName("Trigger Time")]
        public double Phase2AutoTriggerTime
        {
            get
            {
                return phase2AutoTriggerTime;
            }
            set
            {
                OnPropertyChanged("P2TriggerTime");
                phase2AutoTriggerTime = (double)value;
            }
        }


        public int phase1HighlightAudioID = -999;
        private Audio phase1HighlightAudio = null;
        [Category("5.Phase 1: Highlight")]
        [Description("Select audio for highlight event. List can be edited from Maze Collection")]
        [TypeConverter(typeof(AudioConverter))]
        [DisplayName("Audio")]
        public Audio Phase1HighlightAudio
        {
            get { return phase1HighlightAudio; }
            set { phase1HighlightAudio = value;
                if (phase1HighlightAudio != null)
                    phase1HighlightAudioID = phase1HighlightAudio.Index;
                else
                    phase1HighlightAudioID = -999;
                OnPropertyChanged("Phase1HighlightAudio"); }
        }

        public int phase2EventAudioID = -999;
        private Audio phase2EventAudio = null;
        [Category("6.Phase 2: Event")]
        [Description("Select audio for trigger event. List can be edited from Maze Collection")]
        [TypeConverter(typeof(AudioConverter))]
        [DisplayName("Audio")]
        public Audio Phase2EventAudio
        {
            get { return phase2EventAudio; }
            set { phase2EventAudio = value;
                if (phase2EventAudio != null)
                    phase2EventAudioID = phase2EventAudio.Index;
                else
                    phase2EventAudioID = -999;
                OnPropertyChanged("Phase2EventAudio"); }
        }

        private bool phase1HighlightAudioLoop = false;
        [Category("5.Phase 1: Highlight")]
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
        [Category("5.Phase 1: Highlight")]
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
        [Category("6.Phase 2: Event")]
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

        private int phase2EventAudioBehavior = 0;
        [Category("6.Phase 2: Event")]
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

        public override void Paint(ref Graphics gr)
        {
            Brush br;
            Pen p;
            Color p1Radius = Color.Red;
            Color p2Radius = Color.Green;
            Color moveToLoc = Color.Gray;

            if (selected == true)
            {
                br = new SolidBrush(MazeColorSelected);
                gr.FillEllipse(br, this.ScrPoint.X - 13, this.ScrPoint.Y - 13, 26, 26);

                p = new Pen(p1Radius,4);
                float factor  =(float)(this.Phase1ActiveRadius * this.Scale);
                
                gr.DrawEllipse(p, this.ScrPoint.X - factor, this.ScrPoint.Y - factor, factor*2, factor*2);

                p = new Pen(p2Radius, 4);
                factor = (float)(this.Phase2ActiveRadius * this.Scale);

                gr.DrawEllipse(p, this.ScrPoint.X - factor, this.ScrPoint.Y - factor, factor * 2, factor * 2);

                p = new Pen(moveToLoc, 4);
                float factorX = (float)(this.MzEndPoint.X * this.Scale);
                float factorZ = (float)(this.MzEndPoint.Z * this.Scale);

                if(eventAction==1&&!(factorX==0&&factorZ==0))//Move Rotate
                { 
                    gr.DrawEllipse(p, this.ScrPoint.X+ factorX - 10, this.ScrPoint.Y+factorZ - 10, 20, 20);
                    gr.DrawLine(p, this.ScrPoint.X,this.ScrPoint.Y,this.ScrPoint.X + factorX, this.ScrPoint.Y + factorZ);
                }
                //p = new Pen(Color.Beige, 8);
                //gr.DrawLine(p, scrPoint, new PointF((float)(Math.Cos(angle * Math.PI / 180) * 17) + scrPoint.X, -(float)Math.Sin(angle * Math.PI / 180) * 17 + scrPoint.Y));
                p.Dispose();
                br.Dispose();
            }
            br = new SolidBrush(MazeColorRegular);
            gr.FillEllipse(br, this.ScrPoint.X - 10, this.ScrPoint.Y - 10, 20, 20);
            gr.DrawEllipse(Pens.Black, this.ScrPoint.X - 10, this.ScrPoint.Y - 10, 20, 20);
            //p = new Pen(Color.Black, 5);
            //gr.DrawLine(p, scrPoint, new PointF((float)(Math.Cos(10 * Math.PI / 180) * 14) + scrPoint.X, -(float)Math.Sin(angle * Math.PI / 180) * 14 + scrPoint.Y));
            //p.Dispose();
            //p = new Pen(colorCur, 4);
            // gr.DrawLine(p, scrPoint, new PointF((float)(Math.Cos(angle * Math.PI / 180) * 13) + scrPoint.X, -(float)Math.Sin(angle * Math.PI / 180) * 13 + scrPoint.Y));
            if (this.Model != null)
            {
                Font ft = new Font(FontFamily.GenericSerif, 7);
                gr.DrawString("D", ft, Brushes.Azure, this.ScrPoint.X - 5, this.ScrPoint.Y - 5);
                ft.Dispose();
            }
            br.Dispose();
            //p.Dispose();
        }

        public override void Move(float mzXdir, float mzZdir)
        {
            MzPoint.X += mzXdir;
            MzPoint.Z += mzZdir;
            MzEndPoint.X += mzXdir;
            MzEndPoint.Z += mzZdir;

            ConvertFromMazeCoordinates();
        }

        public override void Rescale(double factor)
        {
            MzPoint.X *= factor;
            MzPoint.Z *= factor;
            MzEndPoint.X *= factor;
            MzEndPoint.Z *= factor;

            ConvertFromMazeCoordinates();
        }


        public override void RescaleXYZ(double scaleX, double scaleY, double scaleZ)
        {
            MzPoint.X *= scaleX;
            MzPoint.Y *= scaleY;
            MzPoint.Z *= scaleZ;
            MzEndPoint.X *= scaleX;
            MzEndPoint.Y *= scaleY;
            MzEndPoint.Z *= scaleZ;

            ConvertFromMazeCoordinates();
        }


        public override void Rotate(float degrees, float centerX = 0, float centerY = 0)
        {
            PointF midPoint;

            this.MzPointRot.X -= degrees;
            this.MzEndRot.X -= degrees;

            PointF temp = Tools.RotatePoint(new PointF((float)this.MzEndPoint.X, (float)this.MzEndPoint.Z), new PointF(0, 0), degrees);
            this.MzEndPoint.X = temp.X;
            this.MzEndPoint.Z = temp.Y;

            if (centerX != 0 && centerY != 0)
                midPoint = new PointF(centerX, centerY);
            else return;


            this.ScrPoint = Tools.RotatePoint(this.ScrPoint, midPoint, degrees);

        }

        public new bool PrintToFile(ref StreamWriter fp, Dictionary<string, string> cModels)
        {
            try
            {
                /*
                fp.WriteLine("11\t6");
                fp.WriteLine(  ((Model==null)?"-1":Model.Index.ToString())  + "\t" + this.MzPoint.X.ToString(".##;-.##;0") + "\t" + this.MzPoint.Y.ToString(".##;-.##;0") + "\t" + this.MzPoint.Z.ToString(".##;-.##;0"));
                fp.WriteLine(this.ModelScale.ToString(".##;-.##;0") + "\t" + this.MzPointRot.X.ToString(".##;-.##;0") + "\t" + this.MzPointRot.Y.ToString(".##;-.##;0") + "\t" + this.MzPointRot.Z.ToString(".##;-.##;0"));
                fp.WriteLine((this.Collision? "1" : "0") + "\t" + phase2Criteria.ToString() + "\t" + Phase1ActiveRadius.ToString() + "\t" + triggerAction.ToString() );
                                
                fp.WriteLine(endScale.ToString(".##;-.##;0") + "\t" + (this.MzEndPoint.X + this.MzPoint.X).ToString(".##;-.##;0") + "\t" + (this.MzEndPoint.Y + this.MzPoint.Y).ToString(".##;-.##;0") + "\t" + (this.MzEndPoint.Z + this.MzPoint.Z).ToString(".##;-.##;0"));
                
                fp.WriteLine(actionTime.ToString() + "\t" + this.MzEndRot.X.ToString(".##;-.##;0") + "\t" + this.MzEndRot.Y.ToString(".##;-.##;0") + "\t" + this.MzEndRot.Z.ToString(".##;-.##;0"));
                if (switchToModel == null)
                    fp.WriteLine("0\t" + highlightStyle.ToString());
                else
                    fp.WriteLine(switchToModel.Index.ToString() + "\t" + highlightStyle.ToString());
                return true;
                 */
                //  MODEL_DYNAMIC  8
                //  Model_ID pos_x pos_y pos_z
                //  Model_scale rot_x rot_y rot_z
                //  collision 
                //  phase1_criteria phase1_highlightstyle phase1_activeradius phase1_autoTriggerTime phase1_audio phase1_audioLoop phase1_audioMisc
                //  phase2_criteria phase2_triggerAction  phase2_activeradius phase2_autoTriggerTime phase2_audio phase2_audioLoop phase2_audioMisc 
                //  phase2_endScale phase2_endpos_x phase2_endpos_y phase2_endpos_z
                //  phase2_actionTime phase2_endrot_x phase2_endrot_y phase2_endrot_z
                //  phase2_endModel_ID 

                fp.WriteLine("11\t8\t" + this.GetID().ToString() + "\t" + this.Label);
                //fp.WriteLine(((Model == null) ? "-1" : Model.Index.ToString()) + "\t" + this.MzPoint.X.ToString(".##;-.##;0") + "\t" + this.MzPoint.Y.ToString(".##;-.##;0") + "\t" + this.MzPoint.Z.ToString(".##;-.##;0"));
                string modelID = "";
                if (cModels.ContainsKey(Model))
                    modelID = cModels[Model];
                fp.WriteLine(modelID + "\t" + this.MzPoint.X.ToString(".##;-.##;0") + "\t" + this.MzPoint.Y.ToString(".##;-.##;0") + "\t" + this.MzPoint.Z.ToString(".##;-.##;0"));
                fp.WriteLine(this.ModelScale.ToString(".##;-.##;0") + "\t" + this.MzPointRot.X.ToString(".##;-.##;0") + "\t" + this.MzPointRot.Y.ToString(".##;-.##;0") + "\t" + this.MzPointRot.Z.ToString(".##;-.##;0"));
                //fp.WriteLine((this.Collision ? "1" : "0"));
                fp.WriteLine((this.Collision ? "1" : "0") + "\t" + (this.Kinematic ? "1" : "0") + "\t" + this.Mass);
                fp.Write(phase1Criteria + "\t" + phase1HighlightStyle + "\t" + phase1ActiveRadius.ToString(".##;-.##;0") + "\t" + phase1AutoTriggerTime + "\t" );
                if (phase1HighlightAudio == null)
                {
                    fp.WriteLine("0\t" + (phase1HighlightAudioLoop?"1":"0") + "\t" + phase1HighlightAudioBehavior);
                }
                else
                {
                    fp.WriteLine(phase1HighlightAudio.Index + "\t" + (phase1HighlightAudioLoop ? "1" : "0") + "\t" + phase1HighlightAudioBehavior);
                }
                fp.Write(phase2Criteria + "\t" + eventAction + "\t" + phase2ActiveRadius.ToString(".##;-.##;0") + "\t" + phase2AutoTriggerTime + "\t");
                if (phase2EventAudio == null)
                {
                    fp.WriteLine("0\t" + (phase2EventAudioLoop?"1":"0") + "\t" + phase2EventAudioBehavior);
                }
                else
                {
                    fp.WriteLine(phase2EventAudio.Index + "\t" + (phase2EventAudioLoop?"1":"0") + "\t" + phase2EventAudioBehavior);
                }
                fp.WriteLine(endScale.ToString(".##;-.##;0") + "\t" + (this.MzEndPoint.X + this.MzPoint.X).ToString(".##;-.##;0") + "\t" + (this.MzEndPoint.Y + this.MzPoint.Y).ToString(".##;-.##;0") + "\t" + (this.MzEndPoint.Z + this.MzPoint.Z).ToString(".##;-.##;0"));
                fp.WriteLine(actionTime.ToString(".##;-.##;0") + "\t" + this.MzEndRot.X.ToString(".##;-.##;0") + "\t" + this.MzEndRot.Y.ToString(".##;-.##;0") + "\t" + this.MzEndRot.Z.ToString(".##;-.##;0"));
                if (switchToModel == null)
                    fp.WriteLine("0");
                else
                    //fp.WriteLine(switchToModel.Index.ToString());
                    modelID = "";
                    if (cModels.ContainsKey(switchToModel))
                        modelID = cModels[switchToModel];
                    fp.WriteLine(modelID);
                return true;
            }
            catch
            {
                MessageBox.Show("Couldn't write DynamicModel...", "MazeMaker", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public override XmlElement toXMLnode(XmlDocument doc, Dictionary<string, string> cModels)
        {
            XmlElement dynamicObjectNode = doc.CreateElement(string.Empty, "DynamicObject", string.Empty);
            dynamicObjectNode.SetAttribute("label", this.Label);
            dynamicObjectNode.SetAttribute("id", this.GetID().ToString());

            XmlElement mzPointnode = doc.CreateElement(string.Empty, "MzPoint", string.Empty);
            dynamicObjectNode.AppendChild(mzPointnode);
            mzPointnode.SetAttribute("x", this.MzPoint.X.ToString());
            mzPointnode.SetAttribute("y", this.MzPoint.Y.ToString());
            mzPointnode.SetAttribute("z", this.MzPoint.Z.ToString());

            XmlElement modelNode = doc.CreateElement(string.Empty, "Model", string.Empty);
            dynamicObjectNode.AppendChild(modelNode);
            //if (this.Model != null)
            //{
            //    modelNode.SetAttribute("id", this.Model.Index.ToString());
            //}

            string modelID = "";
            if (cModels.ContainsKey(Model))
                modelID = cModels[Model];
            modelNode.SetAttribute("id", modelID);

            modelNode.SetAttribute("scale", this.ModelScale.ToString());
            modelNode.SetAttribute("rotX", this.MzPointRot.X.ToString());
            modelNode.SetAttribute("rotY", this.MzPointRot.Y.ToString());
            modelNode.SetAttribute("rotZ", this.MzPointRot.Z.ToString());

            XmlElement physicsNode = doc.CreateElement(string.Empty, "Physics", string.Empty);
            dynamicObjectNode.AppendChild(physicsNode);
            physicsNode.SetAttribute("collision", this.Collision.ToString());
            physicsNode.SetAttribute("kinematic", this.Kinematic.ToString());
            physicsNode.SetAttribute("mass", this.Mass.ToString());

            XmlElement phase1Node = doc.CreateElement(string.Empty, "Phase1Highlight", string.Empty);
            dynamicObjectNode.AppendChild(phase1Node);
            phase1Node.SetAttribute("criteria", this.Phase1Criteria.ToString());
            phase1Node.SetAttribute("radius", this.Phase1ActiveRadius.ToString());
            phase1Node.SetAttribute("highlightStyle", this.Phase1HighlightStyle.ToString());
            phase1Node.SetAttribute("triggerTime", this.phase1AutoTriggerTime.ToString());
            phase1Node.SetAttribute("pointThreshold", this.Phase1PointThreshold.ToString());

            XmlElement audioNode1 = doc.CreateElement(string.Empty, "Audio", string.Empty);
            phase1Node.AppendChild(audioNode1);
            if(this.Phase1HighlightAudio!=null)
            { 
                audioNode1.SetAttribute("id", this.Phase1HighlightAudio.Index.ToString());
                audioNode1.SetAttribute("loop", this.Phase1HighlightAudioLoop.ToString());
                audioNode1.SetAttribute("unhighlightBehavior", this.Phase1HighlightAudioBehavior.ToString());
            }

            XmlElement phase2Node = doc.CreateElement(string.Empty, "Phase2Event", string.Empty);
            dynamicObjectNode.AppendChild(phase2Node);
            phase2Node.SetAttribute("criteria", this.Phase2Criteria.ToString());
            phase2Node.SetAttribute("radius", this.Phase2ActiveRadius.ToString());
            phase2Node.SetAttribute("triggerAction", this.EventAction.ToString());
            phase2Node.SetAttribute("triggerTime", this.phase2AutoTriggerTime.ToString());
            phase2Node.SetAttribute("actionTime", this.ActionTime.ToString());
            phase2Node.SetAttribute("pointThreshold", this.phase2PointThreshold.ToString());
            phase2Node.SetAttribute("pointsGranted", this.PointsGranted.ToString());

            XmlElement audioNode2 = doc.CreateElement(string.Empty, "Audio", string.Empty);
            phase2Node.AppendChild(audioNode2);
            if (this.Phase2EventAudio != null)
            {
                audioNode2.SetAttribute("id", this.Phase2EventAudio.Index.ToString());
                audioNode2.SetAttribute("loop", this.Phase2EventAudioLoop.ToString());
                audioNode2.SetAttribute("audioBehavior", this.Phase2EventAudioBehavior.ToString());
            }

            XmlElement endPosNode = doc.CreateElement(string.Empty, "EndMzPoint", string.Empty);
            phase2Node.AppendChild(endPosNode);
            endPosNode.SetAttribute("x", this.MzEndPoint.X.ToString());
            endPosNode.SetAttribute("y", this.MzEndPoint.Y.ToString());
            endPosNode.SetAttribute("z", this.MzEndPoint.Z.ToString());

            XmlElement endRotNode = doc.CreateElement(string.Empty, "EndModel", string.Empty);
            phase2Node.AppendChild(endRotNode);
            endRotNode.SetAttribute("rotX", this.MzEndRot.X.ToString());
            endRotNode.SetAttribute("rotY", this.MzEndRot.Y.ToString());
            endRotNode.SetAttribute("rotZ", this.MzEndRot.Z.ToString());
            //if (this.SwitchToModel != null)
            //    endRotNode.SetAttribute("switchModelID", this.SwitchToModel.Index.ToString());

            modelID = "";
            if (cModels.ContainsKey(SwitchToModel))
                modelID = cModels[SwitchToModel];
            endRotNode.SetAttribute("switchModelID", modelID);

            endRotNode.SetAttribute("endScale", this.EndScale.ToString());

            return dynamicObjectNode;
        }

        new public DynamicObject Clone()
        {
            return Copy(true, 0);
        }

        new public DynamicObject Copy(bool clone=false,int offsetX=0, int offsetY=0)
        {
            DynamicObject temp = new DynamicObject(this.scale, this.Label, -1);
            temp.Collision = this.Collision;
            temp.Kinematic = this.Kinematic;
            temp.Mass = this.Mass;
            temp.Model = this.Model;
            temp.ModelScale = this.ModelScale;
            
            temp.MzPoint = new MPoint(this.MzPoint);
            temp.MzPointRot = new MPoint(this.MzPointRot);
            temp.MazeColorRegular = this.MazeColorRegular;
            temp.MzEndPoint = new MPoint(this.MzEndPoint);
            temp.MzEndRot = new MPoint(this.MzEndRot);
            temp.ActionTime = this.ActionTime;
            temp.EndScale = this.EndScale;
            temp.Phase1HighlightAudio = this.Phase1HighlightAudio;
            temp.Phase1HighlightAudioBehavior = this.Phase1HighlightAudioBehavior;
            temp.Phase1HighlightAudioLoop = this.Phase1HighlightAudioLoop;
            temp.Phase1HighlightStyle = this.Phase1HighlightStyle;
            temp.Phase1ActiveRadius = this.phase1ActiveRadius;
            temp.Phase1AutoTriggerTime = this.Phase1AutoTriggerTime;
            temp.Phase1Criteria = this.Phase1Criteria;
            temp.Phase2ActiveRadius = this.phase2ActiveRadius;
            temp.Phase2AutoTriggerTime = this.Phase2AutoTriggerTime;
            temp.Phase2Criteria = this.Phase2Criteria;
            temp.SwitchToModel = this.SwitchToModel;
            temp.EventAction = this.EventAction;
            temp.Phase2EventAudio = this.Phase2EventAudio;
            temp.Phase2EventAudioLoop = this.Phase2EventAudioLoop;
            temp.justCreated = this.justCreated;

            temp.ScrPoint = new PointF(this.ScrPoint.X + offsetX, this.ScrPoint.Y + offsetY);
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

    public class DynamicObjectConverter : ExpandableObjectConverter
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
            List<DynamicObject> theArray = new List<DynamicObject>();
            //theArray.Add(new Texture(null,null,0));
            theArray.Add(null);
            List<DynamicObject> list = MazeMaker.Maze.GetDynamicObjects();
            foreach (DynamicObject dynO in list)
            {
                theArray.Add(dynO);
            }

            return new StandardValuesCollection(theArray.ToArray());
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, System.Type destinationType)
        {
            if (destinationType == typeof(DynamicObject))
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
                 value is DynamicObject)
            {
                DynamicObject dynO = (DynamicObject)value;
                String cmpString = "Activate " + (dynO.ID);
                if (dynO.Label.Length > 0)
                    cmpString += "[" + dynO.Label + "]";
                return cmpString;
            }
            else if (value == null)
                return "None";
            return base.ConvertTo(context, culture, value, destinationType);
        }
        public override object ConvertFrom(ITypeDescriptorContext context,
                              CultureInfo culture, object value)
        {
            if (value is string)
            {
                try
                {
                    //if (value == "[ none ]")
                    if (value == null || (string)value == "")
                        return null;
                    List<DynamicObject> cList = Maze.GetDynamicObjects();
                    foreach (DynamicObject dynO in cList)
                    {
                        String cmpString = "Activate " + (dynO.ID);
                        if (dynO.Label.Length > 0)
                            cmpString += "[" + dynO.Label + "]";
                        if (cmpString.CompareTo(value) == 0)
                            return dynO;
                    }

                    return null;
                }
                catch
                {
                    throw new ArgumentException("Can not convert '" + (string)value + "' to type Start Position");
                }
                
            }
            return base.ConvertFrom(context, culture, value);
        }

    }
}
