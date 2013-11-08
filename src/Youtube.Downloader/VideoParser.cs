using System.Collections.Generic;
using Youtube.Downloader.Http;

namespace Youtube.Downloader
{
    public class VideoParser
    {
        private readonly HttpLoader _httpLoader;
        private readonly HttpUtilities _httpUtilities;
        private readonly string _id;

        protected InfoPage _infoPage;

        public VideoParser(HttpLoader httpLoader, HttpUtilities httpUtilities, string id) {
            _httpLoader = httpLoader;
            _httpUtilities = httpUtilities;
            _id = id;
        }

        protected InfoPage InfoPage {
            get { return _infoPage ?? (_infoPage = LoadInfoPage()); }
        }

        private InfoPage LoadInfoPage() {
            return new InfoPage(_httpLoader, _httpUtilities, _id);
        }

        public Video GetWithFormat(int tag) {
            var formats = new Formats(_id, InfoPage.GetVideoUrls());
            return new Video(_id, InfoPage.GetTitle(), formats.Get(tag));
        }

        public Video GetInBestQuality(string preferExtention = null) {
            var formats = new Formats(_id, InfoPage.GetVideoUrls());
            return new Video(_id, InfoPage.GetTitle(), formats.GetBest(preferExtention, true));
        }

        public Video GetInMediiumQuality(string preferExtention = null) {
            var formats = new Formats(_id, InfoPage.GetVideoUrls());
            return new Video(_id, InfoPage.GetTitle(), formats.GetMedium(preferExtention, true));
        }

        public string GetTitle() {
            return InfoPage.GetTitle();
        }

        public IEnumerable<Format> GetFormatsList() {
            return new Formats(_id, InfoPage.GetVideoUrls());
        }
    }
}