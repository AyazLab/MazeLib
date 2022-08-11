using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using MazeMaker;
using System.Drawing;

namespace MazeLib
{
    public class MazePathCollection
    {
        public int Index;
        public MazePathCollection(int index)
        {
            Index = index;
        }
        
        private double defaultScale = 17;

        public static string GetShortFileName(string fname)
        {
           int startIndex = Math.Max(fname.LastIndexOf('\\'), fname.LastIndexOf('/'));
           int lastIndex = fname.LastIndexOf('.');
            if (lastIndex == -1) return " ";

            if (startIndex < 0)
                startIndex = 0;
       
           return fname.Substring(startIndex, lastIndex - startIndex);
        }
        
        public List<MazePathItem> cPaths = new List<MazePathItem>();

        private List<int> getLoadedMelIndexFromLogFileName(string inp)
        {
            List<int> loadedMelIndex = new List<int>();
            foreach (MazePathItem mpi in cPaths)
            {
                if (string.Compare(inp, mpi.logFileName) == 0)
                    loadedMelIndex.Add(mpi.melIndex);
            }

            return loadedMelIndex;
        }

        public bool OpenLogFile(string inp, List<int> melIndex,List<MazePathItem> mpiList=null)
        {
            

            try
            {
                List<int> loadedMel = getLoadedMelIndexFromLogFileName(inp);

                StreamReader st = new StreamReader(inp);
                string line = "";

                //double prevTime = 0;
                //double time = 0;

                string fname =  GetShortFileName(inp);
                string maze="", walker="", date="";
                int logCount = -1;

                bool skip = true;
                MazePathItem mzPath = null;

                string mel = "";

                line = st.ReadLine();
                if (line.StartsWith("0\tLoading\t") && (line.Trim().ToLower().EndsWith("mel")|| line.Trim().ToLower().EndsWith("melx")))
                {
                    mel = line.Substring(10);
                }

                while (!st.EndOfStream)
                {

                    line = st.ReadLine(); 
                    if(line=="")
                    {
                        if (mzPath != null)
                            mzPath.UpdateSummary();                            
                    }
                    else if (line.Contains("Walker\t:"))
                    {                        
                        skip = true;
                        if(walker=="")
                            walker = line.Substring(line.IndexOf(":")+1).Trim();
                        maze = "";
                        date = "";                        
                    }
                    else if (line.Contains("Maze\t:"))
                    {
                        maze = GetShortFileName(line).Substring(1);

                        logCount++;
                        if (melIndex.Contains(logCount)&&!loadedMel.Contains(logCount))
                        {
                            skip = false;
                        }
                        else
                        {
                            skip = true;
                        }
                    }
                    else if (line.Contains("Time\t:")) 
                    {                        
                        if(skip==false)
                        {
                            date = line.Substring(line.IndexOf(":") + 1).Trim();
                            mzPath = new MazePathItem(maze, walker, date, mel);
                            mzPath.logFileName = inp;
                            mzPath.melIndex = logCount;
                            mzPath.Scale = defaultScale;
                            if(mpiList!=null)
                            {
                                foreach(MazePathItem mpi in mpiList)
                                {
                                    if(mpi.melIndex==mzPath.melIndex)
                                    {
                                        mzPath.ExpSubjectID = mpi.ExpSubjectID;
                                        mzPath.ExpSession = mpi.ExpSession;
                                        mzPath.ExpGroup = mpi.ExpGroup;
                                        mzPath.ExpCondition = mpi.ExpCondition;
                                        mzPath.ExpTrial = mpi.ExpTrial;
                                    }
                                }
                                

                                //LogExpInfo enterInfo = new LogExpInfo(mzPath);
                                //var dialogResult=enterInfo.ShowDialog();

                                //lastSubjectID = mzPath.ExpSubjectID;
                                //lastSession = mzPath.ExpSession;
                                //lastGroup = mzPath.ExpGroup;
                                //lastCondition = mzPath.ExpCondition;
                                //lastTrial = mzPath.ExpTrial;
                            }
                            cPaths.Add(mzPath);
                            
                        }
                    }
                    else if (skip)
                    {
                        //do  nothing..
                    }
                    else
                    {
                        mzPath.AddLine(line);
                    }

                }
                if (mzPath != null)
                {
                    
                    mzPath.UpdateSummary();
                }
                st.Close();
            }
            catch (System.Exception ex)
            {
                return false;
            }

            return true;
        }

        public void SetAllScales(double scale)
        {
            defaultScale = scale;
            foreach(MazePathItem mpi in cPaths)
            {
                mpi.Scale = scale;
            }
        }

        public void UnselectAll()
        {
            foreach (MazePathItem mpi in cPaths)
            {
                mpi.selected = false;
            }
        }

        public void PaintSelected(ref Graphics gr)
        {
            PaintSelected(ref gr, Color.Transparent);
        }

        public void PaintSelected(ref Graphics gr,Color clr)
        {
            int index = 0;
            foreach (MazePathItem mzp in cPaths)
            {
                index++;
                if (mzp.selected)
                {

                    if (clr == Color.Transparent && mzp.MazeColorRegular == Color.Transparent)
                    { 
                        mzp.Paint(ref gr, GetColor(index), true);
                    }
                    else if (clr == Color.Transparent)
                        mzp.Paint(ref gr, mzp.MazeColorRegular, true);
                    else
                        mzp.Paint(ref gr, clr, true);
                }
            }
        }

        public void ResetColors()
        {
            foreach (MazePathItem mzp in cPaths)
            {
                mzp.MazeColorRegular = Color.Transparent;
                
            }
        }

        public MazePathItem[] GetSelected()
        {
            int numSelected = 0;
            foreach (MazePathItem mpi in cPaths)
            {
                if(mpi.selected)
                    numSelected++;
            }

            MazePathItem[] ret= new MazePathItem[numSelected];
            int index = 0;
            foreach (MazePathItem mpi in cPaths)
            {
                if (mpi.selected)
                {
                    ret[index] = mpi;
                    index++;
                }
            }
            return ret;
        }

        public MazePathItem[] RemoveSelected()
        {
            int numSelected = 0;
            foreach (MazePathItem mpi in cPaths)
            {
                if (mpi.selected)
                    numSelected++;
            }

            MazePathItem[] ret = new MazePathItem[numSelected];

            for(int i=0;i<numSelected;i++)
            {
                foreach (MazePathItem mpi in cPaths)
                {
                    if (mpi.selected)
                    {
                        RemovePathByIndex(mpi.logIndex);
                        break;
                    }
                }
            }
            return ret;
        }

        public void SelectAll()
        {
            foreach (MazePathItem mpi in cPaths)
            {
                mpi.selected = true;
            }
        }

        public static bool ScanLogFile(string inp, string mazeFile, List<MazePathItem> list)
        {
            try
            {
                StreamReader st = new StreamReader(inp);
                string line = "" ;

                string fname = GetShortFileName(inp);
                string maze = "", walker = "", date = "";
                
                int logCount = -1;

                MazePathItem mzPath = null;

                string mel = "";

                line = st.ReadLine();
                if (line.StartsWith("0\tLoading\t") && (line.Trim().ToLower().EndsWith("mel")|| line.Trim().ToLower().EndsWith("melx")))
                {
                    mel = line.Substring(10);
                }

                while (!st.EndOfStream)
                {

                    line = st.ReadLine();
                    //if (line == "")
                    //{
                    //    if (mzPath != null)
                    //        mzPath.UpdateSummary();
                    //}
                    //else 
                    if (line.Contains("Walker\t:"))
                    {
                        //skip = false;
                        if (walker == "")
                            walker = line.Substring(line.IndexOf(":") + 1).Trim();
                        maze = "";
                        date = "";
                    }
                    else if (line.Contains("Maze\t:"))
                    {
                        maze = GetShortFileName(line).Substring(1);
                        //if (mazeFile.CompareTo(maze) == 0)
                        //{
                        //    skip = false;
                        //}
                        //else
                        //{
                        //    skip = true;
                        //}
                        logCount++;
                    }
                    else if (line.Contains("Time\t:"))
                    {
                        //if (skip == false)
                        {
                            date = line.Substring(line.IndexOf(":") + 1).Trim();
                            mzPath = new MazePathItem(maze, walker, date,mel);
                            mzPath.logFileName = inp;
                            mzPath.melIndex = logCount;
                            if (mazeFile.ToLower().Contains(maze.ToLower()))
                            {
                                mzPath.bMazeMatches = true;
                            }
                            list.Add(mzPath);
                        }
                    }
                    //else if (skip)
                    //{
                    //    //do  nothing..
                    //}
                    //else
                    //{
                    //    mzPath.AddLine(line);
                    //}

                }
                //if (mzPath != null)
                //{

                //    mzPath.UpdateSummary();
                //}
                st.Close();
            }
            catch (System.Exception ex)
            {
                return false;
            }

            return true;
        }
        private Color GetColor(int chan)
        {
            switch (chan)
            {
                case 0:
                    return Color.GhostWhite;
                case 1:
                    return Color.Blue;
                case 2:
                    return Color.Orchid;
                case 3:
                    return Color.Brown;
                case 4:
                    return Color.Peru;
                case 5:
                    //return Color.CadetBlue;
                    return Color.Moccasin;
                case 6:
                    return Color.Chartreuse;
                case 7:
                    return Color.Plum;
                case 8:
                    return Color.DarkCyan;
                case 9:
                    return Color.DarkGreen;
                case 10:
                    return Color.DarkKhaki;
                case 11:
                    return Color.DarkMagenta;
                case 12:
                    return Color.DarkOliveGreen;
                case 13:
                    return Color.DarkOrange;
                case 14:
                    return Color.DarkOrchid;
                case 15:
                    return Color.DarkRed;
                case 16:
                    return Color.DarkSalmon;
                case 17:
                    return Color.DarkSeaGreen;
                case 18:
                    return Color.DarkSlateBlue;
                case 19:
                    return Color.DarkTurquoise;
                case 20:
                    return Color.DarkViolet;
                case 21:
                    return Color.DeepPink;
                case 22:
                    return Color.DeepSkyBlue;
                case 23:
                    return Color.DodgerBlue;
                case 24:
                    return Color.Firebrick;
                case 25:
                    return Color.ForestGreen;
                case 26:
                    return Color.Fuchsia;
                case 27:
                    return Color.Gold;
                case 28:
                    return Color.Pink;
                case 29:
                    return Color.Goldenrod;
                case 30:
                    return Color.Gray;
                case 31:
                    return Color.Green;
                case 32:
                    return Color.GreenYellow;
                case 33:
                    return Color.Plum;
                case 34:
                    return Color.HotPink;
                case 35:
                    return Color.IndianRed;
                case 36:
                    return Color.Indigo;
                case 37:
                    return Color.Violet;
                case 38:
                    return Color.Khaki;
                case 39:
                    return Color.Purple;
                case 40:
                    return Color.Red;
                case 41:
                    return Color.LawnGreen;
                case 42:
                    return Color.Navy;
                case 43:
                    return Color.Lime;
                case 44:
                    return Color.Maroon;
                case 45:
                    return Color.MidnightBlue;
                case 46:
                    return Color.MediumSeaGreen;

                default:
                    return Color.Black;
            }

        }

        public bool RemovePathByIndex(int index)
        {
            int indexToDelete = 0;
            foreach (MazePathItem mpi in cPaths)
            {

                if (mpi.logIndex == index)
                    break;
                indexToDelete++;
            }


            if (cPaths.Count > indexToDelete)
            {

                cPaths.RemoveAt(indexToDelete);

                return true;
            }
            return false;
        }



    }
}
