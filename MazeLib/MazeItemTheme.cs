using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace MazeLib
{
    public class MazeItemTheme
    {
        public Color wallColorReg;
        public Color wallColorSel;
        public Color floorColorReg;
        public Color floorColorSel;
        public Color endRegionReg;
        public Color endRegionSel;
        public Color activeRegionColorReg;
        public Color activeRegionColorSel;
        public Color bgColor;
        public int themeIndex;
        public string name;

        public MazeItemTheme()
        {
            wallColorReg = Color.Maroon;
            wallColorSel = Color.DarkBlue;
            floorColorReg = Color.CornflowerBlue;
            floorColorSel = Color.DodgerBlue;
            activeRegionColorReg = Color.PaleGreen;
            activeRegionColorSel = Color.DarkGreen;
            endRegionReg = Color.Goldenrod;
            endRegionSel = Color.Coral;
            bgColor = Color.White;
            themeIndex = 0;
            name = "Default";
        }

        public MazeItemTheme(String name)
        {
            wallColorReg = Color.Maroon;
            wallColorSel = Color.DarkBlue;
            floorColorReg = Color.CornflowerBlue;
            floorColorSel = Color.DodgerBlue;
            activeRegionColorReg = Color.PaleGreen;
            activeRegionColorSel = Color.DarkGreen;
            endRegionReg = Color.Goldenrod;
            endRegionSel = Color.Coral;
            bgColor = Color.White;
            themeIndex = -1;
            this.name = name;
        }

        public MazeItemTheme(MazeItemTheme set)
        {
            wallColorReg = set.wallColorReg;
            wallColorSel = set.wallColorSel;
            floorColorReg = set.floorColorReg;
            floorColorSel = set.floorColorSel;
            activeRegionColorReg = set.activeRegionColorReg;
            activeRegionColorSel = set.activeRegionColorSel;
            endRegionReg = set.endRegionReg;
            endRegionSel = set.endRegionSel;
            bgColor = set.bgColor;
            themeIndex = set.themeIndex;
            name = set.name;
        }

        public void SetColor(MazeMaker.MazeItem item)
        {

            switch(item.itemType)
            { 
                case MazeMaker.MazeItemType.Floor:
                    MazeMaker.Floor floor = (MazeMaker.Floor)item;
                    floor.MazeColorRegular = floorColorReg;
                    floor.MazeColorSelected = floorColorSel;
                    break;
                case MazeMaker.MazeItemType.Wall:
                    MazeMaker.Wall wall = (MazeMaker.Wall)item;
                    wall.MazeColorRegular = wallColorReg;
                    wall.MazeColorSelected= wallColorSel;
                    break;
                case MazeMaker.MazeItemType.CurvedWall:
                    MazeMaker.CurvedWall cwall = (MazeMaker.CurvedWall)item;
                    cwall.MazeColorRegular = wallColorReg;
                    cwall.MazeColorSelected = wallColorSel;
                    break;
                case MazeMaker.MazeItemType.ActiveRegion:
                    MazeMaker.ActiveRegion actReg = (MazeMaker.ActiveRegion)item;
                    actReg.MazeColorRegular = activeRegionColorReg;
                    actReg.MazeColorSelected = activeRegionColorSel;
                    break;
                case MazeMaker.MazeItemType.End:
                    MazeMaker.EndRegion end = (MazeMaker.EndRegion)item;
                    end.MazeColorRegular = endRegionReg;
                    end.MazeColorSelected = endRegionSel;
                    break;

            }

        }
    }

    public class MazeItemThemeLibrary
    {
        public List<MazeItemTheme> themeItems;

        public MazeItemThemeLibrary()
        {
            ColorConverter cConvert = new ColorConverter();
            themeItems = new List<MazeItemTheme>();

            themeItems.Add(new MazeItemTheme()); //Add default

            MazeItemTheme blackWhite = new MazeItemTheme("Black&White");
            blackWhite.wallColorReg = Color.Black;
            blackWhite.wallColorSel = Color.DarkSlateGray;
            blackWhite.floorColorReg = Color.GhostWhite;
            blackWhite.floorColorSel = Color.Gray;
            blackWhite.activeRegionColorReg = Color.SlateGray;
            blackWhite.activeRegionColorSel = Color.DarkGray;
            blackWhite.endRegionReg = Color.LightGray;
            blackWhite.endRegionSel = Color.DarkGray;
            blackWhite.bgColor = Color.White;
            themeItems.Add(blackWhite);

            MazeItemTheme WarmAccent = new MazeItemTheme("Accent");
            WarmAccent.floorColorReg = (Color)cConvert.ConvertFromString("#4ABDAC");
            WarmAccent.floorColorSel = ChangeColorBrightness(WarmAccent.floorColorReg, 0.3f);
            WarmAccent.wallColorReg = (Color)cConvert.ConvertFromString("#DFDCE3");
            WarmAccent.wallColorSel = ChangeColorBrightness(WarmAccent.wallColorReg, -0.3f);
            WarmAccent.activeRegionColorReg = (Color)cConvert.ConvertFromString("#F7B733");
            WarmAccent.activeRegionColorSel = ChangeColorBrightness(WarmAccent.activeRegionColorReg, -0.3f);
            WarmAccent.endRegionReg = (Color)cConvert.ConvertFromString("#FC4A1A");
            WarmAccent.endRegionSel = ChangeColorBrightness(WarmAccent.endRegionReg, -0.3f);
            WarmAccent.bgColor = Color.White;
            themeItems.Add(WarmAccent);

            MazeItemTheme neonContrast = new MazeItemTheme("Neon");
            neonContrast.floorColorReg = (Color)cConvert.ConvertFromString("#0E0B16");
            neonContrast.floorColorSel = ChangeColorBrightness(neonContrast.floorColorReg, -0.3f);
            neonContrast.wallColorReg = (Color)cConvert.ConvertFromString("#E7DFDD");
            neonContrast.wallColorSel = ChangeColorBrightness(neonContrast.wallColorReg, 0.3f);
            neonContrast.activeRegionColorReg = (Color)cConvert.ConvertFromString("#4717F6");
            neonContrast.activeRegionColorSel = ChangeColorBrightness(neonContrast.activeRegionColorReg, -0.3f);
            neonContrast.endRegionReg = (Color)cConvert.ConvertFromString("#A239CA");
            neonContrast.endRegionSel = ChangeColorBrightness(neonContrast.endRegionReg, -0.3f);
            neonContrast.bgColor = Color.White;
            themeItems.Add(neonContrast);

            MazeItemTheme muted = new MazeItemTheme("Muted");
            muted.floorColorReg = (Color)cConvert.ConvertFromString("#D5D5D5");
            muted.floorColorSel = ChangeColorBrightness(muted.floorColorReg, 0.3f);
            muted.wallColorReg = (Color)cConvert.ConvertFromString("#96858F");
            muted.wallColorSel = ChangeColorBrightness(muted.wallColorReg, -0.3f);
            muted.activeRegionColorReg = (Color)cConvert.ConvertFromString("#6D7933");
            muted.activeRegionColorSel = ChangeColorBrightness(muted.activeRegionColorReg, -0.3f);
            muted.endRegionReg = (Color)cConvert.ConvertFromString("#9099A2");
            muted.endRegionSel = ChangeColorBrightness(muted.endRegionReg, -0.3f);
            muted.bgColor = Color.White;
            themeItems.Add(muted);

            MazeItemTheme lightBlue = new MazeItemTheme("LightBlue");
            lightBlue.floorColorReg = Color.LightCyan;
            lightBlue.floorColorSel = ChangeColorBrightness(lightBlue.floorColorReg, 0.3f);
            lightBlue.wallColorReg = Color.SteelBlue;
            lightBlue.wallColorSel = ChangeColorBrightness(lightBlue.wallColorReg, 0.3f);
            lightBlue.activeRegionColorReg = (Color)cConvert.ConvertFromString("#FBA100");
            lightBlue.activeRegionColorSel = ChangeColorBrightness(lightBlue.activeRegionColorReg, 0.3f);
            lightBlue.endRegionReg = (Color)cConvert.ConvertFromString("#6BBAA7");
            lightBlue.endRegionSel = ChangeColorBrightness(lightBlue.endRegionReg, 0.3f);
            lightBlue.bgColor = Color.White;
            themeItems.Add(lightBlue);

            MazeItemTheme cool = new MazeItemTheme("Cool");
            cool.floorColorReg = Color.PowderBlue;
            cool.floorColorSel = ChangeColorBrightness(cool.floorColorReg, 0.3f);
            cool.wallColorReg = Color.Purple;
            cool.wallColorSel = ChangeColorBrightness(cool.wallColorReg, 0.3f);
            cool.activeRegionColorReg = Color.Plum;
            cool.activeRegionColorSel = ChangeColorBrightness(cool.activeRegionColorReg, 0.3f);
            cool.endRegionReg = Color.DarkTurquoise;
            cool.endRegionSel = ChangeColorBrightness(cool.endRegionReg, 0.3f);
            cool.bgColor = Color.White;
            themeItems.Add(cool);

            MazeItemTheme minimalWarm = new MazeItemTheme("Contrast");
            minimalWarm.floorColorReg = (Color)cConvert.ConvertFromString("#F5F5F5");
            minimalWarm.floorColorSel = ChangeColorBrightness(minimalWarm.floorColorReg, -0.3f);
            minimalWarm.wallColorReg = (Color)cConvert.ConvertFromString("#0F1626");
            minimalWarm.wallColorSel = ChangeColorBrightness(minimalWarm.wallColorReg, -0.3f);
            minimalWarm.activeRegionColorReg = (Color)cConvert.ConvertFromString("#F7B733");
            minimalWarm.activeRegionColorSel = ChangeColorBrightness(minimalWarm.activeRegionColorReg, 0.3f);
            minimalWarm.endRegionReg = (Color)cConvert.ConvertFromString("#FF533D");
            minimalWarm.endRegionSel = ChangeColorBrightness(minimalWarm.endRegionReg, 0.3f);
            minimalWarm.bgColor = Color.White;
            themeItems.Add(minimalWarm);

            MazeItemTheme apricot = new MazeItemTheme("Apricot");
            apricot.floorColorReg = (Color)cConvert.ConvertFromString("#DCC7AA");
            apricot.floorColorSel = ChangeColorBrightness(apricot.floorColorReg, 0.3f);
            apricot.wallColorReg = (Color)cConvert.ConvertFromString("#6B7A8F");
            apricot.wallColorSel = ChangeColorBrightness(apricot.wallColorReg, 0.3f);
            apricot.activeRegionColorReg = (Color)cConvert.ConvertFromString("#F7882F");
            apricot.activeRegionColorSel = ChangeColorBrightness(apricot.activeRegionColorReg, 0.3f);
            apricot.endRegionReg = (Color)cConvert.ConvertFromString("#F7C331");
            apricot.endRegionSel = ChangeColorBrightness(apricot.endRegionReg, 0.3f);
            apricot.bgColor = Color.White;
            themeItems.Add(apricot);

            MazeItemTheme dark = new MazeItemTheme("Dark");
            dark.floorColorReg = (Color)cConvert.ConvertFromString("#DCC7AA");
            dark.floorColorSel = ChangeColorBrightness(apricot.floorColorReg, 0.3f);
            dark.wallColorReg = (Color)cConvert.ConvertFromString("#6B7A8F");
            dark.wallColorSel = ChangeColorBrightness(apricot.wallColorReg, 0.3f);
            dark.activeRegionColorReg = (Color)cConvert.ConvertFromString("#F7882F");
            dark.activeRegionColorSel = ChangeColorBrightness(apricot.activeRegionColorReg, 0.3f);
            dark.endRegionReg = (Color)cConvert.ConvertFromString("#F7C331");
            dark.endRegionSel = ChangeColorBrightness(apricot.endRegionReg, 0.3f);
            dark.bgColor = Color.Black;
            themeItems.Add(dark);

            int i = 0;
            foreach(MazeItemTheme m in themeItems)
            {
                m.themeIndex = i;
                i++;
            }
        }

        private static Color ChangeColorBrightness(Color color, float correctionFactor)
        {
            float red = (float)color.R;
            float green = (float)color.G;
            float blue = (float)color.B;

            if (correctionFactor < 0)
            {
                correctionFactor = 1 + correctionFactor;
                red *= correctionFactor;
                green *= correctionFactor;
                blue *= correctionFactor;
            }
            else
            {
                red = (255 - red) * correctionFactor + red;
                green = (255 - green) * correctionFactor + green;
                blue = (255 - blue) * correctionFactor + blue;
            }

            return Color.FromArgb(color.A, (int)red, (int)green, (int)blue);
        }

  
        public MazeItemTheme GetThemeByIndex(int index)
        {
            if (index >= 0 && index < themeItems.Count)
                return themeItems[index];
            else
                return themeItems[0];
        }
    }
}
