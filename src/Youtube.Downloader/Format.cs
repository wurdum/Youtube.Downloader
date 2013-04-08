using System;

namespace Youtube.Downloader
{
    public class Format : IEquatable<Format>
    {
        public Format(byte formatCode, VideoType videoType = VideoType.Unknown, int resolution = 0) {
            FormatCode = formatCode;
            VideoType = videoType;
            Resolution = resolution;
        }

        public byte FormatCode { get; set; }
        public string DownloadUrl { set; get; }
        public int Resolution { get; private set; }
        public VideoType VideoType { get; private set; }

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

        #region equality members

        public bool Equals(Format other) {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return FormatCode == other.FormatCode && string.Equals(DownloadUrl, other.DownloadUrl) && Resolution == other.Resolution &&
                   VideoType == other.VideoType;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != this.GetType())
                return false;
            return Equals((Format) obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = FormatCode.GetHashCode();
                hashCode = (hashCode*397) ^ (DownloadUrl != null ? DownloadUrl.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ Resolution;
                hashCode = (hashCode*397) ^ (int) VideoType;
                return hashCode;
            }
        }

        #endregion

        public Format Clone() {
            var format = new Format(FormatCode, VideoType, Resolution) {DownloadUrl = DownloadUrl};
            return format;
        }

        public override string ToString() {
            return string.Format("FormatCode: {0}, DownloadUrl: {1}, Resolution: {2}", FormatCode, DownloadUrl, Resolution);
        }
    }
}