using System;
using System.Globalization;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace MazeLib
{
    public class VersionChecker
    {
        private readonly string url;
        private readonly string productName;
        private readonly SynchronizationContext synchronizationContext;
        private WebClient webClient = new WebClient();
        public event EventHandler<VersionCheckerEventArgs> CheckCompleted;

        public VersionChecker(string productName, string url = "http://www.mazesuite.com/files/update/updates.dat", SynchronizationContext synchronizationContext = null)
        {
            this.productName = productName;
            this.url = url;
            this.synchronizationContext = synchronizationContext ?? SynchronizationContext.Current;
        }

        /// <summary>
        /// Check the versions. This method does not block the calling thread.
        /// </summary>
        public void Check()
        {
            Task.Factory.StartNew(() =>
            {
                var result = InternalCheckVersion();
                OnCheckCompleted(new VersionCheckerEventArgs(result));
            }, TaskCreationOptions.LongRunning)
            .ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    OnCheckCompleted(new VersionCheckerEventArgs(null, t.Exception));
                }
            });
        }

        /// <summary>
        /// Check the version asynchronously
        /// </summary>
        public Task<VersionCheckResult> CheckAsync()
        {
            return Task.Factory.StartNew(() =>
            {
                return InternalCheckVersion();
            }, TaskCreationOptions.LongRunning);
        }

        protected virtual void OnCheckCompleted(VersionCheckerEventArgs e)
        {
            CheckCompleted.Raise(this.synchronizationContext, this, e);
        }

        private VersionCheckResult InternalCheckVersion()
        {
            Version version = null;
            DateTime? releaseDate = null;

            string str = webClient.DownloadString(url);
            string[] parsed = str.Split(new char[] { '=', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < parsed.Length; i++)
            {
                if (parsed[i].ToLowerInvariant().Contains(productName.ToLowerInvariant()))
                {
                    Version.TryParse(parsed[i + 1], out version);
                }

                if (parsed[i].ToLowerInvariant() == "date")
                {
                    DateTime temp;
                    if (DateTime.TryParseExact(parsed[i + 1], "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out temp))
                    {
                        releaseDate = temp;
                    }
                }
            }

            return new VersionCheckResult(version, releaseDate);
        }
    }
}
