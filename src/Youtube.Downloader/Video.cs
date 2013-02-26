using System;
using Youtube.Downloader.Exceptions;
using Youtube.Downloader.Loaders;

namespace Youtube.Downloader
{
    public enum VideoQuality { Low = 1, Medium, High }
    public enum VideoType { Mobile = 1, Flash, Mp4, WebM, Unknown }
    public enum AudioType { Aac = 1, Mp3, Vorbis, Unknown }

    public class Video : IEquatable<Video>
    {
        private FormatsList _formatsList;

        public Video(string id, string title) {
            Id = id;
            Title = title;
        }

        public string Id { get; private set; }
        public string Title { get; private set; }
        public FormatsList FormatsList {
            get {
                if (_formatsList == null)
                    throw new NullReferenceException("Please load formats before access them.");
                
                return _formatsList;
            }
        }

        public void LoadFormats(FormatsLoader loader) {
            if (loader == null)
                throw new ArgumentNullException("loader");

            var formats = loader.LoadFormats(this);
            if (formats.Count == 0)
                throw new YoutubeParseException("No videos urls was found");

            _formatsList = formats;
        }

        #region quality memberts

        public bool Equals(Video other) {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return string.Equals(Id, other.Id) && string.Equals(Title, other.Title);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != this.GetType())
                return false;
            return Equals((Video) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return ((Id != null ? Id.GetHashCode() : 0)*397) ^ (Title != null ? Title.GetHashCode() : 0);
            }
        }

        #endregion

        public override string ToString() {
            return string.Format("Id: {0}, Title: {1}", Id, Title);
        }
    }
}