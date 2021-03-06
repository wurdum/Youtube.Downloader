﻿using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using NLog;
using Youtube.Downloader.Http;

namespace Youtube.Downloader
{
    public class VideoDownloader
    {
        private readonly HttpLoader _httpLoader;
        private const int BufferSize = 4096;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public VideoDownloader(HttpLoader httpLoader, Video video, string savePath) {
            if (httpLoader == null)
                throw new ArgumentNullException("httpLoader");
            
            if (video == null)
                throw new ArgumentNullException("video");

            if (string.IsNullOrWhiteSpace(savePath))
                throw new ArgumentNullException("savePath");

            _httpLoader = httpLoader;
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

        public async Task<long> BeginDownload(bool allowContinue = true) {
            var httpRequest = _httpLoader.CreateStatefulRequest(Video.Format.DownloadUrl);

            long alreadyInFile = 0;
            if (allowContinue && File.Exists(SavePath)) {
                alreadyInFile = new FileInfo(SavePath).Length;
                httpRequest.AddRange(alreadyInFile);
            }

            if (!allowContinue && File.Exists(SavePath))
                File.Delete(SavePath);

            WebResponse httpResponse;
            try {
                httpResponse = await httpRequest.GetResponseAsync();
            } catch (WebException ex) {
                if (((HttpWebResponse)ex.Response).StatusCode != HttpStatusCode.RequestedRangeNotSatisfiable)
                    throw;

                Raise(Finished, new ProgressEventArgs(Video, 100, 0));
                return alreadyInFile;
            }

            var responseStream = httpResponse.GetResponseStream();

            var fileLength = httpResponse.ContentLength + alreadyInFile;
            if (responseStream == null)
                throw new NullReferenceException("Response stream for '" + Video.Title + "' is null");

            var totalDownloaded = alreadyInFile;
            using (var inHttp = Stream.Synchronized(responseStream))
            using (var outFile = Stream.Synchronized(new FileStream(SavePath, FileMode.Append))) {
                Logger.Debug("'{0}' downloading from '{1}' to '{2}' is started", Video.Id, Video.Format.DownloadUrl, SavePath);
                var buffer = new byte[BufferSize];
                int read;
                while ((read = await inHttp.ReadAsync(buffer, 0, buffer.Length)) > 0) {
                    await outFile.WriteAsync(buffer, 0, read);
                    totalDownloaded += read;

                    Raise(ProgressChanged, new ProgressEventArgs(Video, Math.Round(((double)totalDownloaded / fileLength) * 100, 2), totalDownloaded));
                }
            }

            Logger.Debug("'{0}' downloading is done", Video.Id);
            Raise(Finished, new ProgressEventArgs(Video, 100, 0));

            return totalDownloaded;
        }

        private static string GetFileName(Video video) {
            var invalidChars = Path.GetInvalidFileNameChars();
            var clearedFileName = new string(video.Title.Where(c => !invalidChars.Contains(c)).ToArray());
            return clearedFileName + "." + video.Format.Extention;
        }
    }

    public class ProgressEventArgs : EventArgs
    {
        public ProgressEventArgs(Video video, double progressPercentage, long bytesReceived) {
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