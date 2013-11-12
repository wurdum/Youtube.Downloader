using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using NUnit.Framework;
using Youtube.Downloader.Http;

namespace Youtube.Downloader.Tests
{
    class VideoDownloaderTests
    {
        private static readonly string AssemblyLocation = Path.GetDirectoryName(Path.GetFullPath(new Uri(Assembly.GetExecutingAssembly().CodeBase).AbsolutePath));
        private const string TempDirName = "tmp";
        private const string ResourceDirName = "Resources";
        private const string TemplateVideo = "Missing.mp4";
        private const string TemplatePartialVideo = "Missing-partial.mp4";
        private const string VideoUrl = "http://www.youtube.com/watch?v=UiyDmqO59QE";
        private string _resourcesDir;
        private string _tempDir;

        [SetUp]
        public void FixtureSetUp() {
            _tempDir = Path.Combine(AssemblyLocation, TempDirName);
            _resourcesDir = Path.Combine(AssemblyLocation, ResourceDirName);

            if (Directory.Exists(_tempDir))
                Directory.Delete(_tempDir, true);

            Directory.CreateDirectory(_tempDir);
        }

        [TearDown]
        public void FixtureTearDown() {
            if (Directory.Exists(_tempDir))
                Directory.Delete(_tempDir, true);
        }

        [Test]
        public void DownloadVideoTest() {
            var id = UrlsComposer.ParseId(VideoUrl);
            
            var videoParser = new VideoParser(new HttpLoader(true), new HttpUtilities(), id);
            var video = videoParser.GetInBestQuality();
            
            var videoDownloader = new VideoDownloader(video, _tempDir);
            Task.WaitAll(videoDownloader.BeginDownload(false));

            var downloadedBytes = GetVideoBytes(Path.Combine(_resourcesDir, TemplateVideo));
            var templateBytes = GetVideoBytes(videoDownloader.SavePath);
            Assert.AreEqual(downloadedBytes.Length, templateBytes.Length);
        }

        [Test]
        public void ContinueDownloadVideoText() {
            var id = UrlsComposer.ParseId(VideoUrl);

            var videoParser = new VideoParser(new HttpLoader(true), new HttpUtilities(), id);
            var video = videoParser.GetInBestQuality();

            var videoDownloader = new VideoDownloader(video, _tempDir);

            File.Copy(Path.Combine(_resourcesDir, TemplatePartialVideo), videoDownloader.SavePath);

            videoDownloader.BeginDownload().Wait();

            var templateBytes = GetVideoBytes(Path.Combine(_resourcesDir, TemplateVideo));
            var downloadedBytes = GetVideoBytes(videoDownloader.SavePath);
            Assert.AreEqual(downloadedBytes.Length, templateBytes.Length);
        }

        private byte[] GetVideoBytes(string path) {
            using (var reader = new FileStream(path, FileMode.Open, FileAccess.Read)) {
                var buffer = new byte[reader.Length];
                reader.Read(buffer, 0, (int) reader.Length);
                
                return buffer;
            }
        }
    }
}
