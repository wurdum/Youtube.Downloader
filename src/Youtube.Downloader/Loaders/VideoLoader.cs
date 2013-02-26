using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Youtube.Downloader.Exceptions;
using Youtube.Downloader.Helpers;

namespace Youtube.Downloader.Loaders
{
    public class VideoLoader
    {
        private const string UnavailableContainerTemplate = "<div id=\"watch-player-unavailable\">";
        private const string VideoTitlePattern = @"\<meta name=""title"" content=""(?<title>.*)""\>";
        private readonly HttpUtilities _httpHelper;
        private readonly HttpLoader _pagesLoader;

        public VideoLoader() : this (new HttpUtilities(), new HttpLoader()) { }

        public VideoLoader(HttpUtilities httpHelper, HttpLoader pagesLoader) {
            _httpHelper = httpHelper;
            _pagesLoader = pagesLoader;
        }

        public Video LoadVideo(string videoUrl) {
            if (videoUrl == null)
                throw new ArgumentNullException("videoUrl");

            var videoPageUrl = NormalizeYoutubeUrl(videoUrl);
            var videoPageSource = LoadVideoPage(videoPageUrl);

            if (IsUnavailable(videoPageSource))
                throw new VideoNotAvailableException("Video is not available for your country");

            var id = GetVideoId(videoPageUrl);
            var title = GetVideoTitle(videoPageSource);

            return new Video(id, title);
            
        }

        private string LoadVideoPage(string videoPageUrl) {
            try {
                return _pagesLoader.GetPage(videoPageUrl);
            } catch (Exception ex) {
                throw new YoutubeParseException("Youtube page parsing error", ex);
            }
        }

        private bool IsUnavailable(string pageSource) {
            return pageSource.Contains(UnavailableContainerTemplate);
        }

        private string GetVideoId(string videoPageUrl) {
            try {
                return _httpHelper.ParseQueryString(new Uri(videoPageUrl).Query, Encoding.UTF8)["v"];
            } catch {
                throw new YoutubeParseException("Video id parsing error");
            }
        }

        private string GetVideoTitle(string pageSource) {
            var invalidChars = Path.GetInvalidFileNameChars();
            var videoTitleMatch = Regex.Match(pageSource, VideoTitlePattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

            if (!videoTitleMatch.Groups["title"].Success)
                throw new YoutubeParseException("Video title parsing error");

            var videoTitle = videoTitleMatch.Groups["title"].Value;
            videoTitle = _httpHelper.HtmlDecode(videoTitle);

            return new string(videoTitle.Where(x => !invalidChars.Contains(x)).ToArray());
        }

        private string NormalizeYoutubeUrl(string url) {
            url = url.Trim();

            if (url.StartsWith("https://"))
                url = "http://" + url.Substring(8);
            if (!url.StartsWith("http://"))
                url = "http://" + url;

            url = url.Replace("youtu.be/", "youtube.com/watch?v=").Replace("www.youtube.com", "youtube.com");

            if (url.StartsWith("http://youtube.com/v/"))
                url = url.Replace("youtube.com/v/", "youtube.com/watch?v=");

            if (!Regex.IsMatch(url, @"http\:\/\/youtube\.com\/watch\?.*&?v\=.*"))
                throw new ArgumentException("URL is not a valid youtube URL!");

            return url;
        }
    }
}