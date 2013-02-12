using System;
using System.Collections.Generic;
using System.Linq;
using Youtube.Downloader.Mono.Web;

namespace Youtube.Downloader
{
    public class VideoFormat
    {
        private VideoFormat(byte formatCode, VideoType videoType = VideoType.Unknown, int resolution = 0) {
            FormatCode = formatCode;
            VideoType = videoType;
            Resolution = resolution;
        }

        public byte FormatCode { get; internal set; }
        public int Resolution { get; private set; }
        public VideoType VideoType { get; private set; }
        public string DownloadUrl { get; internal set; }

        public string VideoExtension {
            get {
                switch (VideoType) {
                    case VideoType.Mp4:
                        return ".mp4";
                    case VideoType.Mobile:
                        return ".3gp";
                    case VideoType.Flash:
                        return ".flv";
                    case VideoType.WebM:
                        return ".webm";
                    default: return null;
                }
            }
        }

        public override string ToString() {
            return string.Format("Type: {0}, Resolution: {1}p", VideoType, Resolution);
        }

        public class Factory  {
            private readonly IEnumerable<string> _downloadUrls;

            private static IEnumerable<VideoFormat> _defaults = new List<VideoFormat> {
                new VideoFormat(5, VideoType.Flash, 240),
                new VideoFormat(6, VideoType.Flash, 270),
                new VideoFormat(13, VideoType.Mobile, 0),
                new VideoFormat(17, VideoType.Mobile, 144),
                new VideoFormat(18, VideoType.Mp4, 360),
                new VideoFormat(22, VideoType.Mp4, 720),
                new VideoFormat(34, VideoType.Flash, 360),
                new VideoFormat(35, VideoType.Flash, 480),
                new VideoFormat(36, VideoType.Mobile, 240),
                new VideoFormat(37, VideoType.Mp4, 1080),
                new VideoFormat(38, VideoType.Mp4, 3072),
                new VideoFormat(43, VideoType.WebM, 360),
                new VideoFormat(44, VideoType.WebM, 480),
                new VideoFormat(45, VideoType.WebM, 720),
                new VideoFormat(46, VideoType.WebM, 1080),
                new VideoFormat(82, VideoType.Mp4, 360),
                new VideoFormat(83, VideoType.Mp4, 240),
                new VideoFormat(84, VideoType.Mp4, 720),
                new VideoFormat(85, VideoType.Mp4, 520),
                new VideoFormat(100, VideoType.WebM, 360),
                new VideoFormat(101, VideoType.WebM, 360),
                new VideoFormat(102, VideoType.WebM, 720)
            };

            private Factory(IEnumerable<string> downloadUrls) {
                _downloadUrls = downloadUrls;
            }

            public ICollection<VideoFormat> LoadFormats() {
                return _downloadUrls.Select(url => new { DownloadUrl = url, Itag = HttpUtility.ParseQueryString(new Uri(url).Query)["itag"] })
                                    .Select(o => new {o.DownloadUrl, FormatCode = Byte.Parse(o.Itag)})
                                    .Select(o => CreateFormat(o.FormatCode, o.DownloadUrl.ToString()))
                                    .ToList();
            }

            public VideoFormat CreateFormat(byte formatCode, string downloadUrl) {
                var videoInfo = _defaults.SingleOrDefault(vi => vi.FormatCode == formatCode) ?? new VideoFormat(formatCode);
                videoInfo.DownloadUrl = downloadUrl;
                
                return videoInfo;
            }

            public static Factory Create(IEnumerable<string> downloadUrls) {
                return new Factory(downloadUrls);
            }
        }
    }
}