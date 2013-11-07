using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Kent.Boogaart.KBCsv;
using NLog;

namespace Youtube.Downloader
{
    public class Formats : IEnumerable<Format>
    {
        private readonly string _id;
        public const string FormatsFileName = "formats.csv";
        public static ReadOnlyDictionary<int, Format> AvailableFormats;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        static Formats() {
            var filePath = Path.Combine("Data", FormatsFileName);
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Formats file '" + filePath + "' not found");

            var availableFormats = new Dictionary<int, Format>();
            using (var reader = new CsvReader(File.OpenRead(filePath)) { ValueSeparator = ';' }) {
                reader.ReadHeaderRecord();
                DataRecord record;
                while ((record = reader.ReadDataRecord()) != null) {
                    var tag = Convert.ToInt32(record["itag"]);
                    availableFormats.Add(tag, new Format {
                        Tag = tag,
                        Resolution = new Resolution(record["resolution"]),
                        Extention = record.GetValueOrNull("extension"),
                        IsSpecific = record["isspecific"] == "1"
                    });
                }
            }

            AvailableFormats = new ReadOnlyDictionary<int, Format>(availableFormats);
        }

        private static readonly object _sync = new object();
        private readonly IEnumerable<Format> _formats;

        public Formats(string id, IEnumerable<KeyValuePair<int, string>> videoUrls) {
            _id = id;
            _formats = ParseFormats(videoUrls);
            
            Logger.Debug("'{0}' parsed {1} formats", _id, _formats.Count());
        }

        public void Print(TextWriter outStream) {
            foreach (var format in _formats)
                outStream.WriteLine(format);
        }

        public Format Get(int tag) {
            lock (_sync) {
                return _formats.First(f => f.Tag == tag);
            }
        }

        public Format GetBest(bool skipSpecific = false) {
            Format bestFormat;
            lock (_sync) {
                bestFormat = _formats.Where(f => !skipSpecific || !f.IsSpecific).OrderByDescending(f => f.Resolution).First();
            }

            Logger.Debug("'{0}' '{1}' is chosen as best format", _id, bestFormat.Tag);

            return bestFormat;
        }

        #region IEnumerable implementation

        public IEnumerator<Format> GetEnumerator() {
            return _formats.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        #endregion

        private IEnumerable<Format> ParseFormats(IEnumerable<KeyValuePair<int, string>> videoUrls) {
            lock (_sync) {
                return videoUrls.Select(pair => {
                    var format = AvailableFormats.FirstOrDefault(f => f.Key == pair.Key).Value;
                    if (format.Equals(default(Format)))
                        throw new NullReferenceException("Format with itag '" + pair.Key + "' was not found");

                    format.DownloadUrl = pair.Value;
                    return format;
                }).ToArray();
            }
        }
    }

    public struct Format : IEquatable<Format>
    {
        public int Tag { get; set; }
        public Resolution Resolution { get; set; }
        public string Extention { get; set; }
        public bool IsSpecific { get; set; }
        public string DownloadUrl { get; set; }

        public bool Equals(Format other) {
            return Tag == other.Tag;
        }

        public override string ToString() {
            return string.Format("Tag: {0}, Resolution: {1}, Extention: {2}, IsSpecific: {3}, DownloadUrl: {4}", Tag, Resolution, Extention, IsSpecific, DownloadUrl);
        }
    }

    public class Resolution : IComparable<Resolution>, IEquatable<Resolution>
    {
        public const string UnknownResolution = "???";
        private readonly Regex _resRx = new Regex(@"\d+[pkx](\d+)?", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private readonly string _resolution;

        public Resolution(string resolution) {
            if (!Valid(resolution))
                throw new ArgumentException("Unrecognizable value is encountered");

            _resolution = resolution;
        }

        private bool Valid(string resolution) {
            return resolution.Equals(UnknownResolution) || _resRx.IsMatch(resolution);
        }

        public bool IsUnknown {
            get { return _resolution.Equals(UnknownResolution); }
        }

        public int CompareTo(Resolution other) {
            if (IsUnknown || other.IsUnknown)
                return CompareUnknown(other);

            return CompareInternal(other);
        }

        private int CompareUnknown(Resolution other) {
            if (IsUnknown && !other.IsUnknown)
                return -1;
            if (!IsUnknown && other.IsUnknown)
                return 1;
            return 0;
        }

        private int CompareInternal(Resolution other) {
            var self = ParseFirstNumber(_resolution);
            var oth = ParseFirstNumber(other.ToString());

            return self.CompareTo(oth);
        }

        private int ParseFirstNumber(string str) {
            var numbStr = str.TakeWhile(Char.IsDigit).Aggregate(string.Empty, (current, ch) => current + ch);
            return Int32.Parse(numbStr);
        }

        public override string ToString() {
            return _resolution;
        }

        public bool Equals(Resolution other) {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return string.Equals(_resolution, other._resolution);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != this.GetType())
                return false;
            return Equals((Resolution)obj);
        }

        public override int GetHashCode() {
            return (_resolution != null ? _resolution.GetHashCode() : 0);
        }
    }
}