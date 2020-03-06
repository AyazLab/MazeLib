using System;

namespace MazeLib
{
    public class VersionCheckResult
    {
        public Version Version { get; private set; }

        public DateTime? ReleaseDate { get; private set; }

        public VersionCheckResult(Version version, DateTime? releaseDate)
        {
            this.Version = version;
            this.ReleaseDate = releaseDate;
        }
    }
}
