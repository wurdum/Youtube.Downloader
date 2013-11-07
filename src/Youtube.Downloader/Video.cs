namespace Youtube.Downloader
{
    public class Video
    {
        public Video(string id, string title, Format format) {
            Id = id;
            Title = title;
            Format = format;
        }

        public string Id { get; private set; }
        public string Title { get; private set; }
        public Format Format { get; private set; }
    }
}