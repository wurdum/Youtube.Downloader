using System;
using System.Net;
using System.Threading.Tasks;

namespace Youtube.Downloader
{
    public class VideoDownloader
    {
        public VideoDownloader(Format video, string savePath) {
            if (video == null)
                throw new ArgumentNullException("video");

            if (savePath == null)
                throw new ArgumentNullException("savePath");

            Video = video;
            SavePath = savePath;
        }

        public string SavePath { get; private set; }
        public Format Video { get; private set; }

        public event EventHandler Started;
        public event EventHandler<ProgressEventArgs> ProgressChanged;
        public event EventHandler Finished;

        protected void Raise(EventHandler ev, EventArgs args) {
            if (ev != null)
                ev(this, args);
        }

        protected void Raise<T>(EventHandler<T> ev, T args) {
            if (ev != null)
                ev(this, args);
        }

        public Task Execute() {
            var client = new WebClient();

            client.DownloadProgressChanged += (s, a) => Raise(ProgressChanged, new ProgressEventArgs(a.ProgressPercentage));
            client.DownloadFileCompleted += (s, a) => Raise(Finished, a);
            
            Raise(Started, EventArgs.Empty);
            return client.DownloadFileTaskAsync(Video.DownloadUrl, SavePath);
        }
    }

    public class ProgressEventArgs : EventArgs
    {
        public ProgressEventArgs(double progressPercentage) {
            ProgressPercentage = progressPercentage;
        }

        public double ProgressPercentage { get; private set; }

        public override string ToString() {
            return string.Format("ProgressPercentage: {0}", ProgressPercentage);
        }
    }
}