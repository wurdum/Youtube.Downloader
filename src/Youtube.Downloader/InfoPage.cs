using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using NLog;
using Youtube.Downloader.Http;

namespace Youtube.Downloader
{
    public class InfoPage
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly object _sync = new object();
        private readonly HttpLoader _httpLoader;
        private readonly HttpUtilities _httpHelper;
        private readonly string _id;
        private string _page;
        private NameValueCollection _info;

        public InfoPage(HttpLoader httpLoader, HttpUtilities _httpHelper, string id) {
            _httpLoader = httpLoader;
            this._httpHelper = _httpHelper;
            _id = id;
        }

        protected string Page {
            get { return _page ?? (_page = LoadPage()); }
        }

        protected NameValueCollection Info {
            get { return _info ?? (_info = LoadInfo()); }
        }

        private string LoadPage() {
            _httpLoader.GetPage(UrlsComposer.YoutubeUrl);
            _httpLoader.GetPage(UrlsComposer.YoutubeVideoPage(_id));

            foreach (var url in UrlsComposer.YoutubeVideoInfoPages(_id)) {
                var page = _httpLoader.GetPage(url);
                if (page.Contains("&token"))
                    return page;
            }

            throw new UnauthorizedAccessException("No access to video info page for '" + _id + "'");
        }

        private NameValueCollection LoadInfo() {
            return _httpHelper.ParseQueryString(Page, Encoding.UTF8);
        }

        public string GetTitle() {
            var title = _httpHelper.UrlDecode(Info["title"], Encoding.UTF8);
            Logger.Debug("'{0}' title is '{1}'", _id, title);
            return title;
        }

        public IEnumerable<KeyValuePair<int, string>> GetVideoUrls() {
            var urlsStr = Info["url_encoded_fmt_stream_map"] + "," + Info["adaptive_fmts"];
            KeyValuePair<int, string>[] urls;
            lock (_sync) {
                urls = urlsStr.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(l => _httpHelper.HtmlDecode(l))
                    .Select(l => _httpHelper.ParseQueryString(l, Encoding.UTF8))
                    .Select(l => new KeyValuePair<int, string>(Convert.ToInt32(l["itag"]), l["url"] + "&signature=" + l["sig"]))
                    .ToArray();
            }

            Logger.Debug("'{0}' found '{1}' urls", _id, urls.Length);

            return urls;
        }
    }
}