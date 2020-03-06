using System;

namespace MazeLib
{
    public class VersionCheckerEventArgs : EventArgs
    {
        public Exception Error { get; private set; }

        public Version Version { get; private set; }

        public DateTime? ReleaseDate { get; private set; }

        public VersionCheckerEventArgs(VersionCheckResult result = null, Exception error = null)
        {
            if (result != null)
            {
                this.Version = result.Version;
                this.ReleaseDate = result.ReleaseDate;
            }
            
            this.Error = error;
        }
    }
}
