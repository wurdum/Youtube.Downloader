using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Youtube.Downloader.Exceptions;
using Youtube.Downloader.Mono.Web;

namespace Youtube.Downloader
{
    public enum VideoQuality { Low = 1, Medium, High }
    public enum VideoType { Mobile = 1, Flash, Mp4, WebM, Unknown }
    public enum AudioType { Aac = 1, Mp3, Vorbis, Unknown }

    public class Video
    {
        public Video(string title, ICollection<VideoFormat> formats) {
            Title = title;
            Formats = formats;
        }

        public string Title { get; private set; }
        public ICollection<VideoFormat> Formats { get; private set; }

        public VideoFormat GetMp4(VideoQuality quality) {
            var mp4Videos = Formats.Where(f => f.VideoType == VideoType.Mp4);
            switch (quality) {
                case VideoQuality.Low:
                    return mp4Videos.OrderBy(f => f.Resolution).First();
                case VideoQuality.Medium:
                    var average = mp4Videos.Average(f => f.Resolution);
                    return mp4Videos.Select(f => new {Format = f, Delta = Math.Abs(f.Resolution - average)})
                                    .OrderBy(o => o.Delta)
                                    .First().Format;
                case VideoQuality.High:
                    return mp4Videos.OrderByDescending(f => f.Resolution).First();
                default: throw new ArgumentException("Unknown video quality", "quality");
            }
        }

        public override string ToString() {
            return string.Format("Title: {0}", Title);
        }

        public class Factory
        {
            private readonly string _videoUrl;

            private Factory(string videoUrl) {
                _videoUrl = videoUrl;
            }

            private const string VideoInfoGetUrlTemplate = "http://www.youtube.com/get_video_info?&video_id={0}&el=detailpage&ps=default&eurl=&gl=US&hl=en";
            private const string UnavailableContainerTemplate = "<div id=\"watch-player-unavailable\">";
            private const string VideoTitlePattern = @"\<meta name=""title"" content=""(?<title>.*)""\>";

            public Video LoadVideo() {
                if (_videoUrl == null)
                    throw new ArgumentNullException("_videoUrl");

                var videoPageUrl = UrlHelpers.NormalizeYoutubeUrl(_videoUrl);
                try {
                    var videoPageSource = UrlHelpers.GetPage(videoPageUrl);
                    if (IsUnavailable(videoPageSource))
                        throw new VideoNotAvailableException("Video is not available for your country");
                
                    var id = GetVideoId(videoPageUrl);
                    var title = GetVideoTitle(videoPageSource);
                    var formats = GetVideoFormats(id);

                    return new Video(title, formats);
                } catch (Exception ex) {
                    throw new YoutubeParseException("Youtube page parsing error", ex);
                }
            }

            private bool IsUnavailable(string pageSource) {
                return pageSource.Contains(UnavailableContainerTemplate);
            }

            private string GetVideoId(string videoPageUrl) {
                return HttpUtility.ParseQueryString(new Uri(videoPageUrl).Query)["v"];
            }

            private string GetVideoTitle(string pageSource) {
                try {
                    var invalidChars = Path.GetInvalidFileNameChars();
                    var videoTitleMatch = Regex.Match(pageSource, VideoTitlePattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

                    var videoTitle = videoTitleMatch.Groups["title"].Value;
                    videoTitle = HttpUtility.HtmlDecode(videoTitle);
                    
                    return new string(videoTitle.Where(x => !invalidChars.Contains(x)).ToArray());
                } catch {
                    throw new YoutubeParseException("Video title parsing error");
                }
            }

            private ICollection<VideoFormat> GetVideoFormats(string id) {
                var formatsUrl = String.Format(VideoInfoGetUrlTemplate, id);
                var formatsPage = UrlHelpers.GetPage(formatsUrl);

                var differentFormatVideoPages = UrlHelpers.GetFormatsUrls(formatsPage);
                var videoFormats = VideoFormat.Factory.Create(differentFormatVideoPages).LoadFormats();
                if (!videoFormats.Any())
                    throw new YoutubeParseException("No videos urls was found");
                
                return videoFormats;
            }

            public static Factory Create(string videoUrl) {
                return new Factory(videoUrl);
            }
        }
    }
}