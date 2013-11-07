using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using Youtube.Downloader.Exceptions;

namespace Youtube.Downloader
{
    public static class UrlsComposer
    {
        public const string YoutubeUrl = "https://www.youtube.com/";
        public const string YoutubeVideoUrlTempl = "https://www.youtube.com/watch?v={0}&gl=US&hl=en&has_verified=1";
        public const string YoutubeVideoInfoUrlTempl = "https://www.youtube.com/get_video_info?&video_id={0}&el={1}&ps=default&eurl=&gl=US&hl=en";
        public static readonly string[] ElTypes = new[] { "embedded", "detailpage", "vevo" };

        private static SpinLock _spinLock = new SpinLock();
        private static readonly Regex IdRegex = new Regex(
            @"((https?://)?((((\w+\.)?youtube(-nocookie)?\.com/)(.*?\#/)?(((v|embed|e)/)|(((watch|movie))?(?:\?|\#!?)(?:.*?&)?v=)))|youtu\.be/))?(?<Id>[0-9A-Za-z_-]{11})(?(1).+)?",
            RegexOptions.Compiled);

        public static string ParseId(string url) {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentNullException("url");

            string id;
            var isLocked = false;

            try {
                _spinLock.Enter(ref isLocked);

                var groups = IdRegex.Match(url);
                if (!groups.Groups["Id"].Success || string.IsNullOrEmpty(id = groups.Groups["Id"].Value))
                    throw new IdNotFoundException("Id not found in url: " + url);
            } finally {
                if (isLocked)
                    _spinLock.Exit();
            }
            
            return id;
        }

        public static string YoutubeVideoPage(string id) {
            return FormatSafe(YoutubeVideoUrlTempl, id);
        }

        public static IEnumerable<string> YoutubeVideoInfoPages(string id) {
            var isLocked = false;
            var urls = new List<string>(ElTypes.Length);
            _spinLock.Enter(ref isLocked);
            foreach (var elType in ElTypes)
                urls.Add(string.Format(YoutubeVideoInfoUrlTempl, id, elType));
            _spinLock.Exit();
            return urls;
        }

        private static string FormatSafe(string tmpl, string id) {
            var isLocked = false;
            _spinLock.Enter(ref isLocked);
            var url = string.Format(tmpl, id);
            _spinLock.Exit();
            return url;
        }
    }
}