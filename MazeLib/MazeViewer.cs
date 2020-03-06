using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.IO;
using System.Windows.Forms;
using MazeMaker;
using System.Xml;

namespace MazeLib
{
    public partial class MazeViewer : UserControl
    {

        //public MazeMaker.Maze curMaze = null;
        public MazeLib.ExtendedMaze curMaze = null;
        public MazePathCollection curMazePaths = new MazePathCollection(0);
        public MeasurementRegionCollection curRegions = new MeasurementRegionCollection(0);

        public List<MazeLib.ExtendedMaze> projMazeList = new List<MazeLib.ExtendedMaze>();
        public List<MazePathCollection> projPathList = new List<MazePathCollection>();
        public List<MeasurementRegionCollection> projRegList = new List<MeasurementRegionCollection>();

        public int selectedPath = -1;

        int iViewOffsetX = 0;
        int iViewOffsetY = 0;

        public string mazeFileName = "";

        public const int viewMargin = 10;

        public bool bShowFloor = true;
        public bool bShowPaths = true;
        public bool bShowRegions = true;
        public bool bFillRegions = false;
        public bool bShowEndRegions = true;
        public bool bShowActiveRegions = true;
        public bool bShowLights = true;
        public bool bShowModels = true;
        public bool bShowStartPositions = true;

        public bool bNewRegionFlag = false;

        public class ProjectInfo
        {
            public ProjectInfo()
            {
                ProjectName = "";
                ProjectDescription = "";
                ProjectFilename = "";
            }

            public string ProjectName;
            public string ProjectDescription;
            public string ProjectFilename;
        };

        public ProjectInfo projectInfo;


        public enum AnalyzerMode
        {
            none,end, ruler, point,rectRegion,customRegion,viewpan
        };

        public MazeViewer()
        {
            InitializeComponent();
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);
            projectInfo = new ProjectInfo();
        }

        private void MazeViewer_Load(object sender, EventArgs e)
        {

        }

        int numDrawPoints=0;
        public Image PaintAnalyzerItemsToBuffer() //Draws to Background (everything but Maze)
        {
            Image buffer = new Bitmap(this.Width, this.Height); //I usually use 32BppARGB as my color depth
            Graphics gr = Graphics.FromImage(buffer);

            try
            {

                if (curMaze == null) return buffer;

                //Graphics ClientDC = this.CreateGraphics();

                //PaintMaze(ref offScreenDC);

                gr.TranslateTransform(iViewOffsetX, iViewOffsetY);
                gr.SmoothingMode =
                    System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                if (bFillRegions)
                {
                    foreach (MeasurementRegion m in curRegions.Regions)
                    {
                        m.Paint(ref gr, true);
                    }
                }

                if (bShowPaths)
                { 
                    if(selectedPath>-1)
                    {
                        curMazePaths.PaintSelected(ref gr);
                    }
                    else if(curMazePaths.cPaths.Count>0)               
                    {
                        curMazePaths.PaintSelected(ref gr);
                    }
                }

                if(bShowRegions||curMode!=AnalyzerMode.customRegion||curMode!=AnalyzerMode.rectRegion)
                { 
                    foreach (MeasurementRegion m in curRegions.Regions)
                    {
                        m.Paint(ref gr);
                    }
                }
                gr.TranslateTransform(-iViewOffsetX, -iViewOffsetY);
                Pen yellowPen = new Pen(Color.Yellow, 3);
                switch (curMode) //draw ruler and temp objects
                {

                    case AnalyzerMode.point:
                        gr.DrawEllipse(Pens.Black, tempPoint.X - 3, tempPoint.Y - 3, 6, 6);
                        break;
                    case AnalyzerMode.customRegion:
                        numDrawPoints = curRegions.Regions[curRegions.Regions.Count - 1].Vertices.Count;
                        gr.DrawEllipse(Pens.Black, tempPoint.X - 3, tempPoint.Y - 3, 6, 6);
                        if(numDrawPoints>0)
                            gr.DrawLine(Pens.Black, lastPoint, tempPoint);
                        if(numDrawPoints>1)
                            gr.DrawLine(Pens.Black, initPoint, tempPoint);
                        if(numDrawPoints==2)
                            gr.DrawLine(Pens.Black, initPoint, lastPoint);
                        break;
                    case AnalyzerMode.rectRegion:
                        numDrawPoints = curRegions.Regions[curRegions.Regions.Count - 1].Vertices.Count;
                        gr.DrawEllipse(Pens.Black, tempPoint.X - 3, tempPoint.Y - 3, 6, 6);
                        if (numDrawPoints > 0)
                            gr.DrawRectangle(Pens.Black, Math.Min(initPoint.X, tempPoint.X), Math.Min(initPoint.Y, tempPoint.Y), Math.Abs(initPoint.X - tempPoint.X), Math.Abs(initPoint.Y - tempPoint.Y));

                        break;
                    case AnalyzerMode.ruler:
                        
                        //gr.DrawEllipse(yellowPen, tempPoint.X - 3, tempPoint.Y - 3, 6, 6);
                        if (numDrawPoints > 0)
                        {
                            gr.DrawEllipse(yellowPen, lastPoint.X - 3, lastPoint.Y - 3, 6, 6);
                            gr.DrawLine(yellowPen, lastPoint, tempPoint);
                        }
                        
                        break;
                }
                
                //ClientDC.DrawImage(offScreenBmpBG, 0, 0);

                
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message);
            }
            gr.Dispose();
            return buffer;
        }

        public MazeLib.MazeItemTheme curMazeTheme = new MazeLib.MazeItemTheme();
        

        Image PaintMazeToBuffer()
        {
            Image buffer = new Bitmap(this.Width, this.Height); 
            Graphics gr = Graphics.FromImage(buffer);
            try
            {
                gr.TranslateTransform(iViewOffsetX, iViewOffsetY);

                gr.FillRectangle(Brushes.AliceBlue, 0, 0, this.Width, this.Height);

                gr.SmoothingMode =
                    System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                if (bShowFloor)
                {
                    foreach (Floor f in curMaze.cFloor)
                    {
                        curMazeTheme.SetColor(f);
                        f.Paint(ref gr);
                    }
                }
                if (bShowActiveRegions)
                {
                    foreach (ActiveRegion en in curMaze.cActRegions)
                    {
                        curMazeTheme.SetColor(en);
                        en.Paint(ref gr);
                    }
                }
                if(bShowEndRegions)
                { 
                    foreach (EndRegion en in curMaze.cEndRegions)
                    {
                        curMazeTheme.SetColor(en);
                        en.Paint(ref gr);
                    }
                }
                foreach (Wall w in curMaze.cWall)
                {
                    curMazeTheme.SetColor(w);
                    w.Paint(ref gr);
                }
                foreach (CurvedWall w in curMaze.cCurveWall)
                {
                    curMazeTheme.SetColor(w);
                    w.Paint(ref gr);
                }
                if (bShowLights)
                { 
                    foreach (Light l in curMaze.cLight)
                    {
                        l.Paint(ref gr);
                    }
                }
                if(bShowModels)
                { 
                    foreach (StaticModel c in curMaze.cStaticModels)
                    {
                        c.Paint(ref gr);
                    }
                    foreach (DynamicObject c in curMaze.cDynamicObjects)
                    {
                        c.Paint(ref gr);
                    }
                    foreach (CustomObject c in curMaze.cObject)
                    {
                        c.Paint(ref gr);
                    }
                }
                if(bShowStartPositions)
                {
                    foreach(StartPos sPos in curMaze.cStart)
                    {
                        sPos.Paint(ref gr);
                    }
                }
                //if (curMaze.cEnd != null)
                //{
                //    curMaze.cEnd.Paint(ref gr);
                //}

                //toolStripStatusLabel1.Text = "Painted...";

                gr.TranslateTransform(-iViewOffsetX, -iViewOffsetY);
            }
            catch(Exception ex)
            {
                 //toolStripStatusLabel1.Text = "EXCEPTION : " + ex.Message;
                MessageBox.Show("EXCEPTION : " + ex.Message); 
            }
            gr.Dispose();

            return buffer;
        }
        
        private void MazeViewer_Paint(object sender, PaintEventArgs e)
        {
            Image buffer = PaintAnalyzerItemsToBuffer();
            e.Graphics.DrawImage(buffer, 0, 0);
            buffer.Dispose();     
        }

        public bool RemoveMazeByIndex(int index)
        {
            int indexToDelete=0;
            foreach (MazeLib.ExtendedMaze mz in projMazeList)
            {

                if (mz.Index == index)
                    break;
                indexToDelete++;
            }

            bool isCurMaze = false;

            if(projMazeList.Count>indexToDelete)
            {
                if (index == curMaze.Index)
                    isCurMaze = true;

                if (isCurMaze)
                    NextMaze();

                

                projMazeList.RemoveAt(indexToDelete);
                projPathList.RemoveAt(indexToDelete);
                projRegList.RemoveAt(indexToDelete);

                if (projMazeList.Count == 0)
                    CloseMaze();
                
                return true;
            }
            return false;
        }

        
        public void ClearAll()
        {
            projMazeList.Clear();
            projPathList.Clear();
            projRegList.Clear();
            NewMaze();
            curMaze = null;
            curMazePaths=new MazePathCollection(0);
            curRegions = new MeasurementRegionCollection(0);
            
    }

        private MazeLib.ExtendedMaze GetMazeByIndex(int index)
        {
            foreach (MazeLib.ExtendedMaze mz in projMazeList)
            {
                if (mz.Index == index)
                    return mz;
            }

            return null;
        }

        private MazeLib.ExtendedMaze GetMazeByListIndex(int listIndex)
        {
            int i = 0;
            foreach (MazeLib.ExtendedMaze mz in projMazeList)
            {
                
                if (i == listIndex)
                    return mz;
            }

            return null;
        }

        private bool RemoveMazeByListIndex(int listIndex)
        {
            return RemoveMazeByIndex(GetMazeByListIndex(listIndex).Index);
        }



        public MazePathCollection GetPathsByIndex(int index)
        {
            foreach (MazePathCollection mzP in projPathList)
            {
                if (mzP.Index == index)
                    return mzP;
            }

            return null;
        }

        public MeasurementRegionCollection GetRegionsByIndex(int index)
        {
            foreach (MeasurementRegionCollection mzR in projRegList)
            {
                if (mzR.Index == index)
                    return mzR;
            }

            return null;
        }

        private int GetMazeIndexByName(string mzName)
        {
            foreach (MazeLib.ExtendedMaze mz in projMazeList)
            {
                if (string.Compare(mzName, mz.FileName) == 0)
                    return mz.Index;
            }

            return -1;
        }

        private bool mazeInProject(string mzName)
        {
            return (GetMazeIndexByName(mzName) >= 0);
        }

        private int GetLogFileIndexByName(string logName)
        {
            int i = -1;
            foreach (MazeLib.MazePathItem mpi in curMazePaths.cPaths)
            {
                i++;
                if (string.Compare(logName, mpi.logFileName) == 0)
                    return i;
            }

            return -1;
        }

        public bool pathInCurrentPaths(string logName)
        {
            return (GetLogFileIndexByName(logName) >= 0);
        }



        private int getNextIndex()
        {
            int i = 0;
            foreach (MazeLib.ExtendedMaze mz in projMazeList)
            {
                if (mz.Index >= i)
                    i = mz.Index + 1;
            }

            return i;
        }

        public void SetMazeByListIndex(int listIndex)
        {
            
            UpdateIndicies();
            if (curMaze == null)
                return;

            if (listIndex > projMazeList.Count)
                return;
            curMode = AnalyzerMode.none;

            curMaze = projMazeList[listIndex];

            
            //curMaze.SetMinMaxValues();
            iViewOffsetX = (int)-(curMaze.minX) + viewMargin;
            iViewOffsetY = (int)-(curMaze.minY) + viewMargin;
            //this.Width = Math.Max( (int) curMaze.maxX ,w);
            //this.Height = Math.Max( (int )(curMaze.maxY), h);
            //UpdateTheSize(mz., h);
            SetName(curMaze.FileName);
            RefreshView(true);
            curMazePaths = GetPathsByIndex(curMaze.Index);
            curRegions = GetRegionsByIndex(curMaze.Index);
            curMazePaths.SetAllScales(curMaze.Scale);
            curRegions.SetAllScales(curMaze.Scale);
            
            return;
        }

        public void NextMaze()
        {
            if (curMaze == null)
                return;
            if (curMaze.Index < projMazeList.Count-1)
                SetMazeByListIndex(curMaze.Index +1);
            else if(projMazeList.Count>0)
                SetMazeByListIndex(0);
            else { NewMaze(); }

            //return i;
        }

        public void PrevMaze()
        {
            if (curMaze == null)
                return;
            if (curMaze.Index > 0)
                SetMazeByListIndex(curMaze.Index - 1);
            else if (projMazeList.Count > 0)
                SetMazeByListIndex(projMazeList.Count - 1);
            else { NewMaze(); }
        }


        public void NewMaze()
        {
            //tabPage1.BackgroundImage = null;

            //curMaze = new MazeMaker.Maze();
            
            curMaze = new MazeLib.ExtendedMaze(getNextIndex());

            curMazePaths = new MazePathCollection(curMaze.Index);
            curRegions = new MeasurementRegionCollection(curMaze.Index);
            curRegions.MzFile = curMaze.FileName;

            //toogleElementsToolStrip(true);
            //ChangeMode(Mode.none);
            
            this.Invalidate();
            //mazeNumber++;
            //this.Text = "MazeMaker - Maze" + mazeNumber.ToString();
            //curMaze.Name = "Maze" + mazeNumber.ToString();
            Maze.mzP = curMaze;
        }

        public bool ReadProject(string inp)
        {
            int iExt = CheckInputFileExtension(inp);
            ClearAll();
            
            if(iExt==3)
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(inp);


                
                try
                {
                    doc.Load(inp);
                }
                catch(Exception e)
                {
                    MessageBox.Show("Error unable to access\n" + inp + "\nFile in use or permission denied", "Project Read Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
                if (doc == null)
                {
                    MessageBox.Show("Error unable to access\n" + inp + "\nFile in use or permission denied", "Project Read Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                XmlNode root = doc.DocumentElement.SelectSingleNode("/MazeAnalyzerProject");

                XmlNode fileVersionNode = root.Attributes["version"];
                string fileVersion = "unknown";
                if (fileVersion != null)
                     fileVersion = fileVersionNode.InnerText;

                XmlNode infoNode = doc.DocumentElement.SelectSingleNode("Info");
                string projectName;
                if (infoNode.Attributes["ProjectName"] != null)
                    projectName = infoNode.Attributes["ProjectName"].InnerText;
                else
                    projectName = "Unnamed Project";

                string projectDescrip = infoNode.InnerText;
                if (projectDescrip == null)
                    projectDescrip = "";

                Environment.CurrentDirectory = Path.GetDirectoryName(inp);

                string absolutePath;

                List<int> selected= new List<int>();

                bool mazeSucceeded = false;

                foreach(XmlNode mazeNode in root.ChildNodes)
                {
                    if(mazeNode.Name=="Maze")
                    {
                        XmlNode fname = mazeNode.Attributes["file"];
                        if (fname == null)
                            continue;
                        else if(!fname.InnerText.Contains(":"))
                        {
                            absolutePath = Path.Combine(Directory.GetCurrentDirectory(), fname.InnerText);
                            absolutePath=absolutePath.Replace("\\", "/");
                            absolutePath = absolutePath.Replace("%20", " ");
                        }
                        else
                        {
                            absolutePath = fname.InnerText;
                        }
                        XmlNode regionNode;
                        mazeSucceeded = ReadMazeFile(absolutePath, true); //mazes skipped loading if not valid
                        if (!mazeSucceeded)
                            continue;
                        else
                        {
                            foreach (XmlElement subNode1 in mazeNode.ChildNodes)
                            {
                                switch (subNode1.Name)
                                {
                                    case "LogFile":
                                        if (subNode1.Attributes["file"] == null)
                                            continue;
                                        fname = subNode1.Attributes["file"];
                                        
                                        if (!fname.InnerText.Contains(":"))
                                        {
                                            absolutePath = Path.Combine(Directory.GetCurrentDirectory(), fname.InnerText);
                                            absolutePath = absolutePath.Replace("\\", "/");
                                            absolutePath = absolutePath.Replace("%20", " ");
                                        }
                                        else
                                        {
                                            absolutePath = fname.InnerText;
                                        }
                                        
                                        foreach (XmlNode subNode2 in subNode1.ChildNodes)
                                        {
                                            switch (subNode2.Name)
                                            {
                                                case "PathInfo":

                                                    selected.Clear();
                                                    int melIndex = 0;
                                                    if(subNode2.Attributes["melIndex"]!=null)
                                                        int.TryParse(subNode2.Attributes["melIndex"].InnerText, out melIndex);
                                                    selected.Add(melIndex);
                                                    bool pathRead = curMazePaths.OpenLogFile(absolutePath, selected);
                                                    if (pathRead)
                                                    {
                                                        bShowPaths = true;

                                                        if (subNode2.Attributes["Group"] != null)
                                                            curMazePaths.cPaths[curMazePaths.cPaths.Count - 1].ExpGroup = subNode2.Attributes["Group"].InnerText;
                                                        if (subNode2.Attributes["Subject"] != null)
                                                            curMazePaths.cPaths[curMazePaths.cPaths.Count - 1].ExpSubjectID = subNode2.Attributes["Subject"].InnerText;
                                                        if (subNode2.Attributes["Condition"] != null)
                                                            curMazePaths.cPaths[curMazePaths.cPaths.Count - 1].ExpCondition = subNode2.Attributes["Condition"].InnerText;

                                                        int sessionNo = 0;
                                                        if (subNode2.Attributes["Session"] != null)
                                                            int.TryParse(subNode2.Attributes["Session"].InnerText, out sessionNo);
                                                        curMazePaths.cPaths[curMazePaths.cPaths.Count - 1].ExpSession = sessionNo;

                                                        int trialNo = 0;
                                                        if (subNode2.Attributes["Trial"] != null)
                                                            int.TryParse(subNode2.Attributes["Trial"].InnerText, out trialNo);
                                                        curMazePaths.cPaths[curMazePaths.cPaths.Count - 1].ExpTrial = trialNo;

                                                        string clrString = "";
                                                        if (subNode2.Attributes["Color"] != null)
                                                            clrString = subNode2.Attributes["Color"].InnerText;
                                                        if (clrString.Length > 4)
                                                        {
                                                            string[] clrParts = clrString.Split(',');
                                                            int r = 0, g = 0, b = 0;
                                                            if (clrParts.Length == 3)
                                                            {
                                                                int.TryParse(clrParts[0], out r);
                                                                int.TryParse(clrParts[1], out g);
                                                                int.TryParse(clrParts[2], out b);

                                                                r = Math.Max(Math.Min(r, 255), 0);
                                                                g = Math.Max(Math.Min(g, 255), 0);
                                                                b = Math.Max(Math.Min(b, 255), 0);


                                                                curMazePaths.cPaths[curMazePaths.cPaths.Count - 1].MazeColorRegular = Color.FromArgb(r, g, b);

                                                            }

                                                        }
                                                        curMazePaths.SelectAll();
                                                    }
                                                    break;

                                            }
                                        }
                                        break;
                                    case "Regions":
                                        foreach (XmlElement subNode2 in subNode1.ChildNodes)
                                        { 
                                            switch(subNode2.Name)
                                            { 
                                                case "RegionDefinitionFile":
                                                    fname = subNode2.Attributes["file"];
                                                    if (fname == null)
                                                        continue;
                                                    else if (!fname.InnerText.Contains(":"))
                                                    {
                                                        absolutePath = Path.Combine(Directory.GetCurrentDirectory(), fname.InnerText);
                                                        absolutePath = absolutePath.Replace("\\", "/");
                                                        absolutePath = absolutePath.Replace("%20", " ");
                                                    }
                                                    else
                                                    {
                                                        absolutePath = fname.InnerText;
                                                    }
                                                    curRegions.ReadFromFile(absolutePath);
                                                    break;
                                                case "Region":
                                                    regionNode = subNode2.Attributes["Name"];
                                                    if (regionNode == null)
                                                        continue;
                                                    MeasurementRegion m = new MeasurementRegion(regionNode.InnerText, inp);
                                                    //m.Scale = scale
                                                    regionNode = subNode2.Attributes["RegionGroup"];
                                                    if (regionNode != null)
                                                        m.RegionGroup = regionNode.InnerText;
                                                    

                                                    regionNode = subNode2.Attributes["ROI"];
                                                    string roi= "Inside";
                                                    if(regionNode!=null)
                                                    {
                                                        roi = regionNode.InnerText;
                                                        switch(roi)
                                                        {
                                                            case "Inside":
                                                                m.ROI = MeasurementRegion.RegionSection.Inside;
                                                                break;
                                                            case "Outside":
                                                                m.ROI = MeasurementRegion.RegionSection.Outside;
                                                                break;
                                                        }
                                                    }
                                                    else
                                                        m.ROI = MeasurementRegion.RegionSection.Inside;

                                                    regionNode = subNode2.Attributes["Ymin"];

                                                    double ymin = 0,ymax = 0;
                                                    if(regionNode!=null)
                                                        double.TryParse(regionNode.InnerText,out ymin);
                                                    m.Ymin = ymin;
                                                    regionNode = subNode2.Attributes["Ymax"];
                                                    if (regionNode != null)
                                                        double.TryParse(regionNode.InnerText, out ymax);
                                                    m.Ymax = ymax;

                                                    XmlNode vertexAttr;
                                                    foreach(XmlNode vertexNode in subNode2.ChildNodes)
                                                    {
                                                        if(string.Compare(vertexNode.Name,"Vertex")==0)
                                                        {
                                                            
                                                            PointF p = new PointF(0,0);
                                                            float x=0, y = 0;
                                                            vertexAttr = vertexNode.Attributes["X"];
                                                            if(vertexAttr!=null)
                                                                float.TryParse(vertexAttr.InnerText,out x);
                                                            p.X = x;
                                                            vertexAttr = vertexNode.Attributes["Y"];
                                                            if (vertexAttr != null)
                                                                float.TryParse(vertexAttr.InnerText,out y);
                                                            p.Y = y;
                                                            m.Vertices.Add(p);
                                                        }
                                                        
                                                       
                                                    }
                                                    m.ConvertFromMazeToScreen();
                                                    m.Editing = false;
                                                    m.Scale = curMaze.Scale;
                                                    curRegions.Regions.Add(m);
                                                    
                                                    break;
                                            }
                                        }
                                        break;
                                }
                            }
                        }
                    }
                }



                

                projectInfo.ProjectDescription = projectDescrip;
                projectInfo.ProjectFilename = inp;
                if (projectName.Length < 1)
                    projectInfo.ProjectName = Path.GetFileNameWithoutExtension(inp);
                else
                    projectInfo.ProjectName = projectName;
                
                return true;
            }

            return false;
        }

        public bool ReadMazeFile(string inp, bool skipDuplicateCheck=false)
        {
            int iExt = CheckInputFileExtension(inp);
            if (iExt == 1)
            {
                if (curMaze != null)
                {
                    //MessageBox a = new MessageBox("Maze Already in Project, Do you want to add again?", "MazeAnalyzer", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (!skipDuplicateCheck&&mazeInProject(inp))
                        if (MessageBox.Show("Maze Already in Project, Do you want to add again?", "MazeAnalyzer", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                        {

                        }
                        else
                            return false;

  //                  if (!CloseMaze())
//                        return false;
                }


                NewMaze();

                if (curMaze.ReadFromFileXML(inp) || curMaze.ReadFromClassicFile(inp))
                {
                    this.Text = "MazeAnalyzer - " + inp;
                    curMaze.SetMinMaxValues();
                    iViewOffsetX = (int)-(curMaze.minX) + viewMargin;
                    iViewOffsetY = (int)-(curMaze.minY) + viewMargin;
                    //this.Width = Math.Max( (int) curMaze.maxX ,w);
                    //this.Height = Math.Max( (int )(curMaze.maxY), h);
                    //UpdateTheSize(w, h);
                    SetName(inp);
                    RefreshView(true);
                    curMazePaths.SetAllScales(curMaze.Scale);

                    projMazeList.Add(curMaze);
                    projPathList.Add(curMazePaths);
                    projRegList.Add(curRegions);
                    return true;
                }
                else
                {
                    CloseMaze();
                    MessageBox.Show("Error: "+inp+"\nNot a maze file or corrupted maze file", "MazeAnalyzer", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                
                }

            }
            //else if (iExt == 2)
            //{
            //    MazeListBuilder dia = new MazeListBuilder();
            //    dia.ReadFromFile(inp);
            //    dia.ShowDialog();
            //}
            return false;
        }

        public bool ImportLogFile(string logFile,List<List<MazePathItem>> selectedPaths)
        {
            int index = 0;
            List<int> melIndex = new List<int>();
            foreach(List<MazePathItem> selectedPathsInMaze in selectedPaths)
            {
                melIndex.Clear();
                foreach (MazePathItem mpi in selectedPathsInMaze)
                {
                    melIndex.Add(mpi.melIndex);
                }
                projPathList[index].OpenLogFile(logFile, melIndex, selectedPathsInMaze);
                projPathList[index].SelectAll();
                
                index++;
            }

            curMazePaths = projPathList[curMaze.Index];
            return true;
        }



        public void SetName(String inp)
        {
            mazeFileName = Path.GetFileNameWithoutExtension(inp);
            curRegions.MzFile = mazeFileName;
        }
        public void SetScale(double scale)
        {
            if(curMaze!=null)
            { 
                curMaze.Scale = scale;
                curMazePaths.SetAllScales(curMaze.Scale);
                curRegions.SetAllScales(curMaze.Scale);
                curMaze.SetMinMaxValues();
                iViewOffsetX = (int)-(curMaze.minX) + viewMargin;
                iViewOffsetY = (int)-(curMaze.minY) + viewMargin;
            }
        }
        public void UpdateIndicies()
        {
            int i = 0;
            foreach(ExtendedMaze exMaze in projMazeList)
            {
                exMaze.Index = i;
                i++;
            }
            i = 0;

            foreach (MazePathCollection mpc in projPathList)
            {
                mpc.Index = i;
                int i2 = 0;
                foreach(MazePathItem mpi in mpc.cPaths)
                {
                    mpi.logIndex = i2;
                    i2++;
                }
                i++;
            }
            i = 0;

            foreach (MeasurementRegionCollection mzR in projRegList)
            {
                mzR.Index = i;
                i++;
            }
            i = 0;

            foreach (MazePathItem mpi in curMazePaths.cPaths)
            {
                mpi.logIndex = i;
                i++;
            }
        }

        private int CheckInputFileExtension(string inp)
        {
            string ext = Path.GetExtension(inp);
            ext.ToLower();
            if (ext.CompareTo(".maz") == 0)
                return 1;
            else if (ext.CompareTo(".mel") == 0)
                return 2;
            else if (ext.CompareTo(".mzproj") == 0)
                return 3;
            return 1; //Make default to Maze files...
        }
        public bool CloseMaze()
        {
            if (curMaze != null)
            {

                RemoveMazeByIndex(curMaze.Index);
                
                curMaze = null;
                curMazePaths.cPaths.Clear();
                //toogleElementsToolStrip(false);


                this.Invalidate();
                //propertyGrid1.SelectedObject = null;
                this.Text = "MazeAnalyzer";
                //tabPage1.BackgroundImage = MazeMaker.Properties.Resources.back;
                //ChangeMode(Mode.none);
                return true;
                //}
            }
            return false;
        }


        public void UpdateTheSize(int w, int h)
        {
            //w -= 11;
            //h -= 31;
            //w += 40;
            //h += 40;
            if (curMaze != null)
            {
                int newW=w, newH=h;

                if (curMaze.maxX+iViewOffsetX >= w)
                    newW = (int)curMaze.maxX + iViewOffsetX + viewMargin;

                if (curMaze.maxY + iViewOffsetY >= h)
                    newH = (int)(curMaze.maxY)+iViewOffsetY+ viewMargin;

                this.Size = new System.Drawing.Size(newW, newH);
                //this.ClientSize = new System.Drawing.Size(newW, newH);
                //this.Width = Math.Max((int)curMaze.maxX, w);
                //this.Height = Math.Max((int)(curMaze.maxY), h);                
                //this.Size = new System.Drawing.Size(Math.Max((int)curMaze.maxX, w), Math.Max((int)(curMaze.maxY), h));
            }
            else
            {
                //this.Width = w;
                //this.Height = h;
                this.Size = new System.Drawing.Size(w, h);
            }
            
        }

        Image mazeBuffer;
        public void RefreshView(bool redrawMaze=false)
        {
            if (curMaze != null)
            {
                
                if(redrawMaze)
                {
                    mazeBuffer = PaintMazeToBuffer();
                    this.BackgroundImage = mazeBuffer;
                    
                }
                this.Invalidate();

                //offScreenBmpTop = new Bitmap(Width, Height);
                //offScreenDCTop = Graphics.FromImage(offScreenBmpTop);
               //DrawOnTop();
            }
            else
            {
                this.Invalidate();
                this.BackgroundImage = MazeLib.Properties.Resources.mazeAnalyzerBG;
            }
        }

        private void MazeViewer_Resize(object sender, EventArgs e)
        {
            RefreshView(true);
        }

        private void MazeViewer_MouseClick(object sender, MouseEventArgs e)
        {
            //OffScreenDrawing();
            //Invalidate();
            //RefreshView();
        }

        public bool IsEmpty()
        {
            if (curMaze == null)
                return true;
            return false;
        }

        private void MazeViewer_VisibleChanged(object sender, EventArgs e)
        {
            if (this.Visible)
            {
                RefreshView(true);
            }
        }


        private bool bDefineRegion = false;

        

        public AnalyzerMode curMode=AnalyzerMode.none;
        // 0- End
        // 1- Region start
        // 2- Point start (instead of a region)
        // 
        
        public void EnterDefineRegionMode(AnalyzerMode newMode)  
        {
            if (curMaze == null)
            {
                curMode = AnalyzerMode.none;
                return;
            }

            if (newMode == AnalyzerMode.customRegion && (curRegions.Regions.Count == 0 || curMode!=AnalyzerMode.customRegion||curRegions.Regions[curRegions.Regions.Count - 1].Vertices.Count > 2))
            {
                bDefineRegion = true;
                this.Cursor = Cursors.Cross;
                if (curRegions.Regions.Count > 0)
                {
                    curRegions.Regions[curRegions.Regions.Count - 1].Editing = false;
                    if ((curMode == AnalyzerMode.rectRegion) && curRegions.Regions[curRegions.Regions.Count - 1].Vertices.Count <= 2) //remove incomplete regions
                        curRegions.Regions.RemoveAt(curRegions.Regions.Count - 1);
                }
                curMode = newMode;
                curRegions.Regions.Add(new MeasurementRegion("Region" + (curRegions.Regions.Count+1).ToString()));
                bNewRegionFlag = true;
            }
            else if(newMode == AnalyzerMode.rectRegion && (curRegions.Regions.Count == 0 || curMode != AnalyzerMode.rectRegion || curRegions.Regions[curRegions.Regions.Count - 1].Vertices.Count > 1))
            {
                bDefineRegion = true;
                this.Cursor = Cursors.Cross;
                if (curRegions.Regions.Count > 0)
                {
                    curRegions.Regions[curRegions.Regions.Count - 1].Editing = false;
                    if ((curMode == AnalyzerMode.customRegion ) && curRegions.Regions[curRegions.Regions.Count - 1].Vertices.Count <= 2) //remove incomplete regions
                        curRegions.Regions.RemoveAt(curRegions.Regions.Count - 1);
                }
                curMode = newMode;
                curRegions.Regions.Add(new MeasurementRegion("RectRegion" + (curRegions.Regions.Count + 1).ToString()));
                bNewRegionFlag = true;
            }
            else if (newMode == AnalyzerMode.point && (curMode!=AnalyzerMode.point || curRegions.Regions.Count == 0 || (curMode==AnalyzerMode.point&&curRegions.Regions[curRegions.Regions.Count - 1].Vertices.Count == 1))) // Always a new point
            {
                bDefineRegion = true;
                this.Cursor = Cursors.Cross;
                if(curRegions.Regions.Count>0)
                { 
                    curRegions.Regions[curRegions.Regions.Count - 1].Editing = false;
                    if ((curMode==AnalyzerMode.customRegion||curMode==AnalyzerMode.rectRegion)&&curRegions.Regions[curRegions.Regions.Count - 1].Vertices.Count <= 2) //remove incomplete regions
                        curRegions.Regions.RemoveAt(curRegions.Regions.Count - 1);
                }
                curMode = newMode;
                curRegions.Regions.Add(new MeasurementRegion("Point" + (curRegions.Regions.Count + 1).ToString()));
                bNewRegionFlag = true;

            }
            
            else//No mode or othermode, cleanup
            {
                curMode = newMode;
                bDefineRegion = false;
                if (curRegions.Regions.Count > 0)
                {
                    curRegions.Regions[curRegions.Regions.Count - 1].Editing = false;
                    if (curRegions.Regions[curRegions.Regions.Count - 1].Name.StartsWith("Point"))
                    {
                        //this is a point
                        if (curRegions.Regions[curRegions.Regions.Count - 1].Vertices.Count < 1)
                            curRegions.Regions.RemoveAt(curRegions.Regions.Count - 1);
                    }
                    else
                    {
                        //this is not enough for region.
                        if (curRegions.Regions[curRegions.Regions.Count - 1].Vertices.Count <= 2)
                            curRegions.Regions.RemoveAt(curRegions.Regions.Count - 1);
                    }
                }

                if (curMode == AnalyzerMode.ruler)
                {
                    numDrawPoints = 0;
                    this.Cursor = Cursors.Cross;
                }
                else
                    this.Cursor = Cursors.Default;
                RefreshView();
            }
        }

        

        
        public void MazeViewer_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                //end define mode...
                EnterDefineRegionMode(AnalyzerMode.none);
            }
            else if (e.Button == MouseButtons.Left && (curMode!=AnalyzerMode.none||curMode!=AnalyzerMode.viewpan))
            {
                //add a new point...
                try
                {
                    if (curMode==AnalyzerMode.customRegion && curRegions.Regions.Count > 0)
                    {
                        curRegions.Regions[curRegions.Regions.Count - 1].Scale = curMaze.Scale;
                        curRegions.Regions[curRegions.Regions.Count - 1].AddPoint(e.X - iViewOffsetX, e.Y - iViewOffsetY);
                        lastPoint = e.Location;

                        if (curRegions.Regions[curRegions.Regions.Count - 1].Vertices.Count == 1) //while definng the second point capture the first
                            initPoint = lastPoint;
                        //MessageBox.Show("Added point! " + e.X + " , " + e.Y);
                            
                        RefreshView();
                    }
                    else if (curMode == AnalyzerMode.rectRegion && curRegions.Regions.Count > 0)
                    {
                        lastPoint = e.Location;
                        curRegions.Regions[curRegions.Regions.Count - 1].Scale = curMaze.Scale;
                        if (curRegions.Regions[curRegions.Regions.Count - 1].Vertices.Count == 1) //while definng the second point capture the first
                            curRegions.Regions[curRegions.Regions.Count - 1].AddPoint(lastPoint.X - iViewOffsetX, initPoint.Y - iViewOffsetY);

                        curRegions.Regions[curRegions.Regions.Count - 1].AddPoint(lastPoint.X - iViewOffsetX, lastPoint.Y - iViewOffsetY);

                        if (curRegions.Regions[curRegions.Regions.Count - 1].Vertices.Count == 1) //while definng the second point capture the first
                            initPoint = lastPoint;
                       
                        if (curRegions.Regions[curRegions.Regions.Count - 1].Vertices.Count == 3)
                        {
                            //***// Finish rectangle here
                            
                            curRegions.Regions[curRegions.Regions.Count - 1].AddPoint(initPoint.X - iViewOffsetX, lastPoint.Y - iViewOffsetY);
                            
                            curRegions.Regions[curRegions.Regions.Count - 1].Editing = false;
                            EnterDefineRegionMode(AnalyzerMode.rectRegion);

                        }

                            RefreshView();
                    }
                    else if (curMode == AnalyzerMode.point && curRegions.Regions.Count > 0) 
                    {
                        curRegions.Regions[curRegions.Regions.Count - 1].Scale = curMaze.Scale;
                        curRegions.Regions[curRegions.Regions.Count - 1].AddPoint(e.X - iViewOffsetX, e.Y - iViewOffsetY);
                        lastPoint = e.Location;

                        

                        EnterDefineRegionMode(AnalyzerMode.point); //begin putting new point
                        RefreshView();
                    } 
                    else if (curMode == AnalyzerMode.ruler)
                    {
                        numDrawPoints++;
                        lastPoint = e.Location;
                        RefreshView();
                    }
                }
                catch //(System.Exception ex)
                {                        
                }
                    
            }
            
            /*else
            {
                string res = "";
                foreach (MeasurementRegion m in curRegions.Regions)
                {
                    if (m.IsPointIn(e.X, e.Y))
                    {
                        res += m.Name + "\n";
                    }
                }
                if (string.IsNullOrEmpty(res)==false)
                {
                    //MessageBox.Show(res,"Region selection");
                    toolTip1.Show(res, this, e.X, e.Y, 1000);
                }

            }*/
        }


        public void ShowMeasurementRegionManager()
        {
            MeasurementRegionManager mg = new MeasurementRegionManager(ref curRegions);
            mg.ShowDialog();
            RefreshView();
        }
        Point tempPoint, lastPoint, initPoint;
        private void MazeViewer_MouseMove(object sender, MouseEventArgs e)
        {
            tempPoint = e.Location;

            if(curMode!=AnalyzerMode.none)
                this.Invalidate(new Rectangle(tempPoint.X - 960, tempPoint.Y - 600, 1920, 1200));

            
        }

        public void toClipboard()
        {
            if (curMaze == null)
                return;

            
            Image mazeBuffer = PaintMazeToBuffer();
            Graphics clipBoardGR = Graphics.FromImage(mazeBuffer);
            Image buffer = PaintAnalyzerItemsToBuffer();
            clipBoardGR.DrawImage(buffer, 0, 0);
            
            clipBoardGR.Dispose();
            RectangleF mazeSize = new Rectangle(viewMargin, viewMargin, (int)(curMaze.maxX - curMaze.minX), (int)(curMaze.maxY - curMaze.minY));
            Bitmap output = new Bitmap(mazeBuffer);
            mazeBuffer.Dispose();
            buffer.Dispose();
            Clipboard.Clear();
            Clipboard.SetImage(output.Clone(mazeSize, mazeBuffer.PixelFormat));
            output.Dispose();
        }

    }
}
