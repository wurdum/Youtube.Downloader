using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Youtube.Downloader.Mono.Web;

namespace Youtube.Downloader
{
    public static class UrlHelpers
    {
        public static string NormalizeYoutubeUrl(string url) {
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

        public static string GetPage(string url) {
            return GetPage(url, Encoding.UTF8);
        }

        public static string GetPage(string url, Encoding encoding) {
            using (var client = new WebClient { Encoding = encoding })
                return client.DownloadString(url);
        }

        public static IEnumerable<string> GetFormatsUrls(string url) {
            var urlMap = HttpUtility.ParseQueryString(url).Get("url_encoded_fmt_stream_map");
            var urls = urlMap.Split(',');
            return urls.Select(HttpUtility.ParseQueryString)
                .Select(q => string.Format("{0}&fallback_host={1}&signature={2}", q["url"], q["fallback_host"], q["sig"]))
                .Select(HttpUtility.UrlDecode);
        }
    }
}