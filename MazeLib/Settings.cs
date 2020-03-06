
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace MazeMaker
{
    public static class Settings
    {
        public static string userLibraryFolder;
        public static string standardLibraryFolder;
        public static string mWalkerIniPath;

        [DllImport("kernel32.dll", EntryPoint = "GetPrivateProfileString")]
        private static extern int GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, StringBuilder lpReturnedString, int nSize, string lpFileName);

        public static void InitializeSettings()
        {
            standardLibraryFolder = Application.ExecutablePath.Substring(0, Application.ExecutablePath.LastIndexOf('\\')) + "\\Library";
            userLibraryFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\MazeSuite\\Library";
            StringBuilder buffer = new StringBuilder(260);
            mWalkerIniPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\MazeWalker\\MazeWalker.ini";
            if(System.IO.File.Exists(mWalkerIniPath))
            { 
                GetPrivateProfileString("LibraryPath", "LibraryPath", userLibraryFolder, buffer, 260, mWalkerIniPath);
                userLibraryFolder = buffer.ToString();
            }
        }

    }
}
