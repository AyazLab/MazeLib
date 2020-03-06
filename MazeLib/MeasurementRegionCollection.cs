using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;

namespace MazeLib
{
    public class MeasurementRegionCollection
    {
        public int Index;

        public MeasurementRegionCollection(int index)
         {   
            Index = index;
         }

        private List<MeasurementRegion> curRegions = new List<MeasurementRegion>();
        public List<MeasurementRegion> Regions
        {
            get { return curRegions;   }
            set { curRegions = value;  }
        }

        private string mzFile= "";
        public string MzFile
        {
            get { return mzFile; }
            set { mzFile = value; }
        }
        
        public void Clear()
        {
            curRegions.Clear();
        }

        public void SetAllScales(double scale)
        {
            foreach (MeasurementRegion mpr in curRegions)
            {
                mpr.Scale = scale;
            }
        }

        public bool ReadFromFile(string inp)
        {
            if (string.Compare(Path.GetExtension(inp).ToLower(),".mzreg")==0)
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(inp);



                try
                {
                    doc.Load(inp);
                }
                catch (Exception e)
                {
                    MessageBox.Show("Error unable to access\n" + inp + "\nFile in use or permission denied", "Project Read Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
                if (doc == null)
                {
                    MessageBox.Show("Error unable to access\n" + inp + "\nFile in use or permission denied", "Project Read Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                XmlNode root = doc.DocumentElement.SelectSingleNode("/MazeAnalyzer_Region_Definition_File");

                XmlNode fileVersionNode = root.Attributes["version"];
                string fileVersion = "unknown";
                if (fileVersion != null)
                    fileVersion = fileVersionNode.InnerText;

                XmlNode infoNode = doc.DocumentElement.SelectSingleNode("Info");
                string mazeName = "unknown";
                if (infoNode.Attributes["MazeName"] != null)
                    mazeName = infoNode.Attributes["MazeName"].InnerText;


                XmlNode regionNode;
                foreach (XmlElement subNode in root.ChildNodes)
                {
                    if (string.Compare(subNode.Name, "Regions") == 0)
                    {
                        foreach (XmlElement subNode2 in subNode.ChildNodes)
                        {
                            switch (subNode2.Name)
                            {
                                case "Region":
                                    regionNode = subNode2.Attributes["Name"];
                                    if (regionNode == null)
                                        continue;
                                    String regName = regionNode.InnerText;
                                    Guid g;
                                    regionNode = subNode2.Attributes["Guid"];
                                    if (regionNode != null)
                                        g = Guid.Parse(regionNode.InnerText);
                                    else
                                        g = System.Guid.NewGuid();

                                    MeasurementRegion m = new MeasurementRegion(regName, inp,g);
                                    
                                    //m.Scale = scale
                                    regionNode = subNode2.Attributes["RegionGroup"];
                                    if (regionNode != null)
                                        m.RegionGroup = regionNode.InnerText;

                                    


                                    regionNode = subNode2.Attributes["ROI"];
                                    string roi = "Inside";
                                    if (regionNode != null)
                                    {
                                        roi = regionNode.InnerText;
                                        switch (roi)
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

                                    double ymin = 0, ymax = 0;
                                    if (regionNode != null)
                                        double.TryParse(regionNode.InnerText, out ymin);
                                    m.Ymin = ymin;
                                    regionNode = subNode2.Attributes["Ymax"];
                                    if (regionNode != null)
                                        double.TryParse(regionNode.InnerText, out ymax);
                                    m.Ymax = ymax;

                                    XmlNode vertexAttr;
                                    foreach (XmlNode vertexNode in subNode2.ChildNodes)
                                    {
                                        if (string.Compare(vertexNode.Name, "Vertex") == 0)
                                        {

                                            PointF p = new PointF(0, 0);
                                            float x = 0, y = 0;
                                            vertexAttr = vertexNode.Attributes["X"];
                                            if (vertexAttr != null)
                                                float.TryParse(vertexAttr.InnerText, out x);
                                            p.X = x;
                                            vertexAttr = vertexNode.Attributes["Y"];
                                            if (vertexAttr != null)
                                                float.TryParse(vertexAttr.InnerText, out y);
                                            p.Y = y;
                                            m.Vertices.Add(p);
                                        }


                                    }
                                    m.ConvertFromMazeToScreen();
                                    m.Editing = false;

                                    bool alreadyLoaded = false;
                                    foreach(MeasurementRegion r in curRegions)
                                    {
                                        if (r.regGuid == m.regGuid)
                                        { 
                                            alreadyLoaded = true;
                                            break;
                                        }
                                    }
                                    
                                    if(!alreadyLoaded)
                                        curRegions.Add(m);

                                    break;
                            }
                        }

                        return true;
                    }




                   
                }

            }
            else
                return ReadFromClassicFile(inp);

            return false;
        }

        public bool ReadFromClassicFile(string inp)
        {
            try
            {
                StreamReader fp = new StreamReader(inp);
                if (fp == null)
                {
                    return false;
                }

                //read header
                string line = fp.ReadLine();

                //check if file is right...
                if (line.StartsWith("MazeAnalyzer Region Definition File") == false)
                    return false;

                //read maze file
                line = fp.ReadLine();
                if (mzFile != Path.GetFileNameWithoutExtension(line))
                {
                    //maze files do not match!
                    MessageBox.Show("Region definitions were created for another maze","Warning",MessageBoxButtons.OK,MessageBoxIcon.Warning);
                }

                //read region count
                line = fp.ReadLine();

                int count = int.Parse(line);
                int subCount = 0;
                string[] parsed;
                int i, j;
                char[] param = new char[] { '\t' };
                for (i = 0; i < count; i++)
                {
                    line = fp.ReadLine();
                    parsed = line.Split(param);
                    MeasurementRegion m = new MeasurementRegion(parsed[0],inp);
                    m.Scale = float.Parse(parsed[1]);
                    m.ROI = (MeasurementRegion.RegionSection) int.Parse(parsed[2]);
                    m.Ymin = double.Parse(parsed[3]);
                    m.Ymax = double.Parse(parsed[4]);
                    subCount = int.Parse(parsed[5]);
                    for (j = 0; j < subCount; j++)
                    {
                        line = fp.ReadLine();
                        parsed = line.Split(param);
                        PointF p = new PointF();
                        p.X = float.Parse(parsed[0]);
                        p.Y = float.Parse(parsed[1]);
                        m.Vertices.Add(p);
                    }
                    m.ConvertFromMazeToScreen();
                    m.Editing = false;
                    curRegions.Add(m);
                }                
                fp.Close();
                return true;
            }
            catch //(System.Exception ex)
            {
                return false;
            }
            
        }

        public string saveRegionDefinitionFile(string messageText = "Save Region Definition File")
        {
            SaveFileDialog a = new SaveFileDialog();
            a.Title = messageText;
            a.Filter = "Maze Region Definition Files | *.mzReg";
            a.FilterIndex = 1;
            a.DefaultExt = ".mzReg";
            a.RestoreDirectory = true;

            if (a.ShowDialog() == DialogResult.OK)
            {
                WriteToFile(a.FileName);
                return a.FileName;
            }
            else
                return "";
        }

        public bool WriteToFile(string inp)
        {
            string filename = inp;
            string path = Path.GetDirectoryName(filename);

            XmlElement regionNode;
            XmlDocument doc = new XmlDocument();
            XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            XmlElement root = doc.DocumentElement;
            doc.InsertBefore(xmlDeclaration, root);

            XmlElement projectXMLnode = doc.CreateElement(string.Empty, "MazeAnalyzer_Region_Definition_File", string.Empty);
            doc.AppendChild(projectXMLnode);
            projectXMLnode.SetAttribute("version", "2.0");
            projectXMLnode.SetAttribute("url", "http://www.mazesuite.com");


            XmlElement infoNode = doc.CreateElement(string.Empty, "Info", string.Empty);
            projectXMLnode.AppendChild(infoNode);
            infoNode.SetAttribute("MazeName", Path.GetFileNameWithoutExtension(mzFile));

          
            
            XmlElement regionCollectionNode, vertexNode;
            regionCollectionNode = doc.CreateElement(string.Empty, "Regions", string.Empty);
            projectXMLnode.AppendChild(regionCollectionNode);

            foreach (MazeLib.MeasurementRegion mzR in curRegions)
            {
                // = new Uri(Directory.GetCurrentDirectory());
                regionNode = doc.CreateElement(string.Empty, "Region", string.Empty);
                regionCollectionNode.AppendChild(regionNode);
                regionNode.SetAttribute("Name", mzR.Name);
                regionNode.SetAttribute("Guid", mzR.regGuid.ToString());
                regionNode.SetAttribute("Ymax", mzR.Ymax.ToString());
                regionNode.SetAttribute("Ymin", mzR.Ymin.ToString());
                regionNode.SetAttribute("RegionGroup", mzR.RegionGroup);
                regionNode.SetAttribute("ROI", mzR.ROI.ToString());
                foreach (PointF vertex in mzR.Vertices)
                {
                    vertexNode = doc.CreateElement(string.Empty, "Vertex", string.Empty);
                    regionNode.AppendChild(vertexNode);

                    vertexNode.SetAttribute("X", vertex.X.ToString());
                    vertexNode.SetAttribute("Y", vertex.Y.ToString());
                }
            }

            try
            { 
                doc.Save(inp);
                return true;
            }
            catch
            {
                return false;
            }
            
        }

    }
}
