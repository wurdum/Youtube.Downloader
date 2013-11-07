using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using NLog;

namespace Youtube.Downloader
{
    public class VideoDownloader
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public VideoDownloader(Video video, string savePath) {
            if (video == null)
                throw new ArgumentNullException("video");

            if (string.IsNullOrWhiteSpace(savePath))
                throw new ArgumentNullException("savePath");

            Video = video;
            SavePath = Path.Combine(savePath, GetFileName(video));
        }

        public string SavePath { get; private set; }
        public Video Video { get; private set; }

        public event EventHandler<ProgressEventArgs> ProgressChanged;
        public event EventHandler<ProgressEventArgs> Finished;

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

            client.DownloadProgressChanged += (s, a) => Raise(ProgressChanged, new ProgressEventArgs(Video, a.ProgressPercentage, a.BytesReceived));
            client.DownloadFileCompleted += (s, a) => Raise(Finished, new ProgressEventArgs(Video, 100, 0));
            
            Logger.Debug("'{0}' downloading from '{1}' to '{2}' is started", Video.Id, Video.Format.DownloadUrl, SavePath);
            Finished += (sender, args) => Logger.Debug("'{0}' downloading is done", Video.Id);

            return client.DownloadFileTaskAsync(Video.Format.DownloadUrl, SavePath);
        }

        private static string GetFileName(Video video) {
            var invalidChars = Path.GetInvalidFileNameChars();
            var clearedFileName = new string(video.Title.Where(c => !invalidChars.Contains(c)).ToArray());
            return clearedFileName + "." + video.Format.Extention;
        }
    }

    public class ProgressEventArgs : EventArgs
    {
        public ProgressEventArgs(Video video, int progressPercentage, long bytesReceived) {
            Video = video;
            ProgressPercentage = progressPercentage;
            BytesReceived = bytesReceived;
        }

        public Video Video { get; private set; }
        public double ProgressPercentage { get; private set; }
        public long BytesReceived { get; private set; }

        public override string ToString() {
            return string.Format("Video: {0}, ProgressPercentage: {1}", Video, ProgressPercentage);
        }
    }
}