﻿// ReSharper Disable All 

using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using System.Xml;

namespace MazeLib
{
    // Based on http://www.codeproject.com/Articles/5454/A-Pretty-Good-Splash-Screen-in-C
    public partial class SplashScreen : Form
    {
        #region Member Variables
        // Threading
        private static SplashScreen ms_frmSplash = null;
        private static Thread ms_oThread = null;

        // Fade in and out.
        private double m_dblOpacityIncrement = .05;
        private double m_dblOpacityDecrement = .08;
        private const int TIMER_INTERVAL = 50;

        // Status and progress bar
        private string m_sStatus;
        private string m_sTimeRemaining;
        private double m_dblCompletionFraction = 0.0;
        private Rectangle m_rProgress;

        // Progress smoothing
        private double m_dblLastCompletionFraction = 0.0;
        private double m_dblPBIncrementPerTimerInterval = .015;

        // Self-calibration support
        private int m_iIndex = 1;
        private int m_iActualTicks = 0;
        private ArrayList m_alPreviousCompletionFraction;
        private ArrayList m_alActualTimes = new ArrayList();
        private DateTime m_dtStart;
        private bool m_bFirstLaunch = false;
        private bool m_bDTSet = false;

        #endregion Member Variables

        public SplashScreen()
        {
            InitializeComponent();
            this.Opacity = 0.0;
            updateTimer.Interval = TIMER_INTERVAL;
            updateTimer.Start();
            this.ProgressColor1 = Color.FromArgb(58, 96, 151);
            this.ProgressColor2 = Color.FromArgb(181, 237, 254);

        }

        #region Public Static Methods
        // A static method to create the thread and 
        // launch the SplashScreen.
        static public void ShowSplashScreen<TSplashScreen>() where TSplashScreen : SplashScreen, new()
        {
            // Make sure it's only launched once.
            if (ms_frmSplash != null)
                return;
            ms_oThread = new Thread(new ThreadStart(() => SplashScreen.ShowForm<TSplashScreen>()));
            ms_oThread.IsBackground = true;
            ms_oThread.SetApartmentState(ApartmentState.STA);
            ms_oThread.Start();
            while (ms_frmSplash == null || ms_frmSplash.IsHandleCreated == false)
            {
                System.Threading.Thread.Sleep(TIMER_INTERVAL);
            }
        }

        // Close the form without setting the parent.
        static public void CloseForm()
        {
            if (ms_frmSplash != null && ms_frmSplash.IsDisposed == false)
            {
                // Make it start going away.
                ms_frmSplash.m_dblOpacityIncrement = -ms_frmSplash.m_dblOpacityDecrement;
            }
            ms_oThread = null;	// we don't need these any more.
            ms_frmSplash = null;
        }

        // A static method to set the status and update the reference.
        static public void SetStatus(string newStatus)
        {
            SetStatus(newStatus, true);
        }

        // A static method to set the status and optionally update the reference.
        // This is useful if you are in a section of code that has a variable
        // set of status string updates.  In that case, don't set the reference.
        static public void SetStatus(string newStatus, bool setReference)
        {
            if (ms_frmSplash == null)
                return;

            ms_frmSplash.m_sStatus = newStatus;

            if (setReference)
                ms_frmSplash.SetReferenceInternal();
        }

        // Static method called from the initializing application to 
        // give the splash screen reference points.  Not needed if
        // you are using a lot of status strings.
        static public void SetReferencePoint()
        {
            if (ms_frmSplash == null)
                return;
            ms_frmSplash.SetReferenceInternal();

        }
        #endregion Public Static Methods

        #region Private Methods

        // A private entry point for the thread.
        static private void ShowForm<TSplashScreen>() where TSplashScreen : SplashScreen, new()
        {
            ms_frmSplash = new TSplashScreen();
            Application.Run(ms_frmSplash);
        }

        // Internal method for setting reference points.
        private void SetReferenceInternal()
        {
            if (m_bDTSet == false)
            {
                m_bDTSet = true;
                m_dtStart = DateTime.Now;
                ReadIncrements();
            }
            double dblMilliseconds = ElapsedMilliSeconds();
            m_alActualTimes.Add(dblMilliseconds);
            m_dblLastCompletionFraction = m_dblCompletionFraction;
            if (m_alPreviousCompletionFraction != null && m_iIndex < m_alPreviousCompletionFraction.Count)
                m_dblCompletionFraction = (double)m_alPreviousCompletionFraction[m_iIndex++];
            else
                m_dblCompletionFraction = (m_iIndex > 0) ? 1 : 0;
        }

        // Utility function to return elapsed Milliseconds since the 
        // SplashScreen was launched.
        private double ElapsedMilliSeconds()
        {
            TimeSpan ts = DateTime.Now - m_dtStart;
            return ts.TotalMilliseconds;
        }

        // Function to read the checkpoint intervals from the previous invocation of the
        // splashscreen from the XML file.
        private void ReadIncrements()
        {
            string sPBIncrementPerTimerInterval = SplashScreenXMLStorage.Interval;
            double dblResult;

            if (Double.TryParse(sPBIncrementPerTimerInterval, System.Globalization.NumberStyles.Float, System.Globalization.NumberFormatInfo.InvariantInfo, out dblResult) == true)
                m_dblPBIncrementPerTimerInterval = dblResult;
            else
                m_dblPBIncrementPerTimerInterval = .0015;

            string sPBPreviousPctComplete = SplashScreenXMLStorage.Percents;

            if (sPBPreviousPctComplete != "")
            {
                string[] aTimes = sPBPreviousPctComplete.Split(null);
                m_alPreviousCompletionFraction = new ArrayList();

                for (int i = 0; i < aTimes.Length; i++)
                {
                    double dblVal;
                    if (Double.TryParse(aTimes[i], System.Globalization.NumberStyles.Float, System.Globalization.NumberFormatInfo.InvariantInfo, out dblVal) == true)
                        m_alPreviousCompletionFraction.Add(dblVal);
                    else
                        m_alPreviousCompletionFraction.Add(1.0);
                }
            }
            else
            {
                m_bFirstLaunch = true;
                m_sTimeRemaining = "";
            }
        }

        // Method to store the intervals (in percent complete) from the current invocation of
        // the splash screen to XML storage.
        private void StoreIncrements()
        {
            string sPercent = "";
            double dblElapsedMilliseconds = ElapsedMilliSeconds();
            for (int i = 0; i < m_alActualTimes.Count; i++)
                sPercent += ((double)m_alActualTimes[i] / dblElapsedMilliseconds).ToString("0.####", System.Globalization.NumberFormatInfo.InvariantInfo) + " ";

            SplashScreenXMLStorage.Percents = sPercent;

            m_dblPBIncrementPerTimerInterval = 1.0 / (double)m_iActualTicks;

            SplashScreenXMLStorage.Interval = m_dblPBIncrementPerTimerInterval.ToString("#.000000", System.Globalization.NumberFormatInfo.InvariantInfo);
        }

        public static SplashScreen GetSplashScreen()
        {
            return ms_frmSplash;
        }

        #endregion Private Methods

        protected Color ProgressColor1 { get; set; }

        protected Color ProgressColor2 { get; set; }

        #region Event Handlers
        // Tick Event handler for the Timer control.  Handle fade in and fade out and paint progress bar. 
        private void UpdateTimer_Tick(object sender, System.EventArgs e)
        {
            lblStatus.Text = m_sStatus;

            // Calculate opacity
            if (m_dblOpacityIncrement > 0)		// Starting up splash screen
            {
                m_iActualTicks++;
                if (this.Opacity < 1)
                    this.Opacity += m_dblOpacityIncrement;
            }
            else // Closing down splash screen
            {
                if (this.Opacity > 0)
                    this.Opacity += m_dblOpacityIncrement;
                else
                {
                    StoreIncrements();
                    updateTimer.Stop();
                    this.Close();
                }
            }

            // Paint progress bar
            if (m_bFirstLaunch == false && m_dblLastCompletionFraction < m_dblCompletionFraction)
            {
                m_dblLastCompletionFraction += m_dblPBIncrementPerTimerInterval;
                int width = (int)Math.Floor(pnlStatus.ClientRectangle.Width * m_dblLastCompletionFraction);
                int height = pnlStatus.ClientRectangle.Height;
                int x = pnlStatus.ClientRectangle.X;
                int y = pnlStatus.ClientRectangle.Y;
                if (width > 0 && height > 0)
                {
                    m_rProgress = new Rectangle(x, y, width, height);
                    if (!pnlStatus.IsDisposed)
                    {
                        Graphics g = pnlStatus.CreateGraphics();
                        LinearGradientBrush brBackground = new LinearGradientBrush(m_rProgress, this.ProgressColor1, this.ProgressColor2, LinearGradientMode.Horizontal);
                        g.FillRectangle(brBackground, m_rProgress);
                        g.Dispose();
                    }
                    int iSecondsLeft = 1 + (int)(TIMER_INTERVAL * ((1.0 - m_dblLastCompletionFraction) / m_dblPBIncrementPerTimerInterval)) / 1000;
                    m_sTimeRemaining = (iSecondsLeft == 1) ? string.Format("1 second remaining") : string.Format("{0} seconds remaining", iSecondsLeft);
                }
            }
            lblTimeRemaining.Text = m_sTimeRemaining;
        }

        private void SplashScreen_BackgroundImageChanged(object sender, EventArgs e)
        {
            if (this.BackgroundImage != null)
            {
                this.ClientSize = this.BackgroundImage.Size;
            }
        }

        #endregion Event Handlers
    }

    #region Auxiliary Classes
    /// <summary>
    /// A specialized class for managing XML storage for the splash screen.
    /// </summary>
    internal class SplashScreenXMLStorage
    {
        private static string ms_StoredValues = "Splash.dat";
        private static string ms_DefaultPercents = "";
        private static string ms_DefaultIncrement = ".015";


        // Get or set the string storing the percentage complete at each checkpoint.
        static public string Percents
        {
            get { return GetValue("Percents", ms_DefaultPercents); }
            set { SetValue("Percents", value); }
        }
        // Get or set how much time passes between updates.
        static public string Interval
        {
            get { return GetValue("Interval", ms_DefaultIncrement); }
            set { SetValue("Interval", value); }
        }

        // Store the file in a location where it can be written with only User rights. (Don't use install directory).
        static private string StoragePath
        {
            get { return Path.Combine(Application.UserAppDataPath, ms_StoredValues); }
        }

        // Helper method for getting inner text of named element.
        static private string GetValue(string name, string defaultValue)
        {
            if (!File.Exists(StoragePath))
                return defaultValue;

            try
            {
                XmlDocument docXML = new XmlDocument();
                docXML.Load(StoragePath);
                XmlElement elValue = docXML.DocumentElement.SelectSingleNode(name) as XmlElement;
                return (elValue == null) ? defaultValue : elValue.InnerText;
            }
            catch
            {
                return defaultValue;
            }
        }

        // Helper method for setting inner text of named element.  Creates document if it doesn't exist.
        static public void SetValue(string name,
             string stringValue)
        {
            XmlDocument docXML = new XmlDocument();
            XmlElement elRoot = null;

            if (!File.Exists(StoragePath))
            {
                elRoot = docXML.CreateElement("root");
                docXML.AppendChild(elRoot);
            }
            else
            {
                try
                {
                    docXML.Load(StoragePath);
                    elRoot = docXML.DocumentElement;
                }
                catch
                {
                    elRoot = docXML.CreateElement("root");
                    docXML.AppendChild(elRoot);
                }
            }

            XmlElement value = docXML.DocumentElement.SelectSingleNode(name) as XmlElement;
            if (value == null)
            {
                value = docXML.CreateElement(name);
                elRoot.AppendChild(value);
            }

            value.InnerText = stringValue;
            docXML.Save(StoragePath);
        }
    }
    #endregion Auxiliary Classes
}

