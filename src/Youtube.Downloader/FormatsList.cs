using System;
using System.Collections.Generic;
using System.Linq;

namespace Youtube.Downloader
{
    public class FormatsList : List<Format>
    {
        public Format GetMp4(VideoQuality quality) {
            var mp4Videos = this.Where(f => f.VideoType == VideoType.Mp4);
            switch (quality) {
                case VideoQuality.Low:
                    return mp4Videos.OrderBy(f => f.Resolution).First();
                case VideoQuality.Medium:
                    var average = mp4Videos.Average(f => f.Resolution);
                    return mp4Videos.Select(f => new { Format = f, Delta = Math.Abs(f.Resolution - average) })
                                    .OrderBy(o => o.Delta)
                                    .First().Format;
                case VideoQuality.High:
                    return mp4Videos.OrderByDescending(f => f.Resolution).First();
                default: throw new ArgumentException("Unknown video quality", "quality");
            }
        }

        public override bool Equals(object obj) {
            var list = obj as FormatsList;
            if (list == null)
                return false;

            if (Count != list.Count)
                return false;

            for (var i = 0; i < Count; i++)
                if (!this[i].Equals(list[i]))
                    return false;

            return true;
        }
    }
}