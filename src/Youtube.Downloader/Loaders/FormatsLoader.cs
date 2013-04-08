using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Youtube.Downloader.Exceptions;
using Youtube.Downloader.Helpers;

namespace Youtube.Downloader.Loaders
{
    public class FormatsLoader
    {
        #region predefined formats

        private static readonly IEnumerable<Format> Defaults = new List<Format> {
            new Format(5, VideoType.Flash, 240),
            new Format(6, VideoType.Flash, 270),
            new Format(13, VideoType.Mobile, 0),
            new Format(17, VideoType.Mobile, 144),
            new Format(18, VideoType.Mp4, 360),
            new Format(22, VideoType.Mp4, 720),
            new Format(34, VideoType.Flash, 360),
            new Format(35, VideoType.Flash, 480),
            new Format(36, VideoType.Mobile, 240),
            new Format(37, VideoType.Mp4, 1080),
            new Format(38, VideoType.Mp4, 3072),
            new Format(43, VideoType.WebM, 360),
            new Format(44, VideoType.WebM, 480),
            new Format(45, VideoType.WebM, 720),
            new Format(46, VideoType.WebM, 1080),
            new Format(82, VideoType.Mp4, 360),
            new Format(83, VideoType.Mp4, 240),
            new Format(84, VideoType.Mp4, 720),
            new Format(85, VideoType.Mp4, 520),
            new Format(100, VideoType.WebM, 360),
            new Format(101, VideoType.WebM, 360),
            new Format(102, VideoType.WebM, 720)
        };

        #endregion

        private const string VideoInfoGetUrlTemplate = "http://www.youtube.com/get_video_info?&video_id={0}&el=detailpage&ps=default&eurl=&gl=US&hl=en";
        private readonly HttpUtilities _httpHelper;
        private readonly HttpLoader _pagesLoader;

        public FormatsLoader() : this (new HttpUtilities(), new HttpLoader()) { }

        public FormatsLoader(HttpUtilities httpHelper, HttpLoader pagesLoader) {
            _httpHelper = httpHelper;
            _pagesLoader = pagesLoader;
        }

        public FormatsList LoadFormats(Video video) {
            if (video == null)
                throw new ArgumentNullException("video");

            var formatsUrl = String.Format(VideoInfoGetUrlTemplate, video.Id);
            var formatsPage = LoadFormatsPage(formatsUrl);
            if (!FormatsInResponse(formatsPage))
                throw new FormatsLoadingException("Formats page page error");

            var downloadUrls = GetFormatsUrls(formatsPage);
            var formats = CreateFormatsObjects(downloadUrls);

            return formats;
        }

        private string LoadFormatsPage(string formatsPageUrl) {
            try {
                return _pagesLoader.GetPage(formatsPageUrl);
            } catch (Exception ex) {
                throw new YoutubeParseException("Youtube page parsing error", ex);
            }
        }

        private bool FormatsInResponse(string formatsPage) {
            return _httpHelper.ParseQueryString(formatsPage, Encoding.UTF8)["status"] == "ok";
        }

        private IEnumerable<string> GetFormatsUrls(string url) {
            try {
                var urlMap = _httpHelper.ParseQueryString(url, Encoding.UTF8).Get("url_encoded_fmt_stream_map");
                var urls = urlMap.Split(',');
                return urls.Select(u => _httpHelper.ParseQueryString(u, Encoding.UTF8))
                           .Select(q => string.Format("{0}&fallback_host={1}&signature={2}", q["url"], q["fallback_host"], q["sig"]))
                           .Select(q => _httpHelper.UrlDecode(q, Encoding.UTF8));
            } catch (Exception ex) {
                throw new FormatsLoadingException("Formats urls parsing error", ex);
            }
        }

        private FormatsList CreateFormatsObjects(IEnumerable<string> downloadUrls) {
            try {
                var formats = new FormatsList();
                
                formats.AddRange(downloadUrls.Select(url => new {
                    downloadUrl = url, 
                    formatCode = Byte.Parse(_httpHelper.ParseQueryString(new Uri(url).Query, Encoding.UTF8)["itag"])
                }).Select(o => CreateFormat(o.formatCode, o.downloadUrl)));

                return formats;
            } catch (Exception ex) {
                throw new FormatsLoadingException("Formats object creation error", ex);
            }
        }

        private Format CreateFormat(byte formatCode, string downloadUrl) {
            var defaultFormat = Defaults.FirstOrDefault(d => d.FormatCode == formatCode);
            if (defaultFormat == null)
                return new Format(formatCode);

            var format = defaultFormat.Clone();
            format.DownloadUrl = downloadUrl;
            return format;
        }
    }
}