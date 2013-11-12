using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using NLog.Config;
using NLog.Targets;
using Youtube.Downloader.Http;
using Out = System.Console;

namespace Youtube.Downloader.Console
{
    class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly ProgramOptionsSet OptionsSet = new ProgramOptionsSet();
        private static SpinLock Sync = new SpinLock(true);

        static void Main(string[] args) {
            args = new[] { "http://www.youtube.com/watch?v=GoBNld-EbA4", "http://www.youtube.com/watch?v=4yqNjMBZUmY" };
            OptionsSet.Add("p|path=", "Path where to save videos. If not specified saves to Desktop.", o => OptionsSet.PathToSave = o);
            OptionsSet.Add("e|preferextention=", "Preferable video extention, like mp4 or webm", o => OptionsSet.PreferExtention = o);
            OptionsSet.Add("f|formatsonly", "Just show info about available video formats", o => OptionsSet.FormatsOnly = !string.IsNullOrWhiteSpace(o));
            OptionsSet.Add("m|mediumquality", "Download videos in medium quality", o => OptionsSet.MediumQuoality = !string.IsNullOrWhiteSpace(o));
            OptionsSet.Add("h|help", "Show help", o => OptionsSet.Help = !string.IsNullOrWhiteSpace(o));

            string error;
            if (!OptionsSet.Parse(args, out error)) {
                OptionsSet.ShowHelp(error);
                return;
            }

            if (OptionsSet.Help) {
                OptionsSet.ShowHelp();
                return;
            }

            ConfigureLogger();

            if (OptionsSet.FormatsOnly) {
                DownloadFormats();
                return;
            }

            DownloadVideos();
        }

        private static void DownloadFormats() {
            Logger.Debug("downloading formats from '{0}'", string.Join(", ", OptionsSet.Urls));

            WriteSeparator("Formats");
            var downloadingTasks = new List<Task>();
            for (var index = 0; index < OptionsSet.Urls.Count; index++) {
                var url = OptionsSet.Urls[index];
                var id = UrlsComposer.ParseId(url);
                downloadingTasks.Add(new Task(() => {
                    var videoParser = new VideoParser(new HttpLoader(true), new HttpUtilities(), id);
                    var title = videoParser.GetTitle();
                    var formats = videoParser.GetFormatsList();

                    var isLocked = false;
                    try {
                        Sync.Enter(ref isLocked);

                        WriteSeparator("[" + url + "]");
                        WriteSeparator(title);

                        Out.WriteLine("{0, 10}\t{1, 10}\t{2, 10}\t{3, 10}", "tag", "resolution", "extention", "is specific");
                        Out.WriteLine();
                        foreach (var format in formats)
                            Out.WriteLine("{0, 10}\t{1, 10}\t{2, 10}\t{3, 10}", format.Tag, format.Resolution, format.Extention, format.IsSpecific);
                    } finally {
                        if (isLocked)
                            Sync.Exit();
                    }
                }));
            }

            foreach (var task in downloadingTasks)
                task.Start();

            Task.WaitAll(downloadingTasks.ToArray());

            Out.WriteLine("\nDone!");
        }

        private static void DownloadVideos() {
            Logger.Debug("downloading from '{0}'", string.Join(", ", OptionsSet.Urls));

            WriteSeparator("Downloading");
            var zeroCursorPosition = Out.CursorTop;
            var downloadingTasks = new List<Task>();
            var handlers = new Dictionary<string, ProgressChangedHandler>();
            for (var index = 0; index < OptionsSet.Urls.Count; index++) {
                var top = index;
                var id = UrlsComposer.ParseId(OptionsSet.Urls[index]);
                handlers.Add(id, new ProgressChangedHandler(id, index));

                downloadingTasks.Add(new Task(() => {
                    var httpLoader = new HttpLoader(true);
                    var videoParser = new VideoParser(httpLoader, new HttpUtilities(), id);
                    var video = OptionsSet.MediumQuoality ? 
                        videoParser.GetInMediiumQuality(OptionsSet.PreferExtention) :
                        videoParser.GetInBestQuality(OptionsSet.PreferExtention);
                    var videoDownloader = new VideoDownloader(httpLoader, video, OptionsSet.PathToSave);

                    videoDownloader.ProgressChanged += (o, a) => handlers[a.Video.Id].OnProgressChanged(o, a, zeroCursorPosition + top, ref Sync);
                    videoDownloader.Finished += (o, a) => handlers[a.Video.Id].OnFinished(o, a, zeroCursorPosition + top, ref Sync);
                    videoDownloader.BeginDownload().Wait();
                }));
            }

            foreach (var task in downloadingTasks)
                task.Start();

            Task.WaitAll(downloadingTasks.ToArray());

            Out.WriteLine("\nDone!");
        }

        private static void WriteSeparator(string msg) {
            var dashLength = (int)Math.Floor((Out.WindowWidth - msg.Length)/2.0);
            Out.Write(new string('-', dashLength));
            Out.Write(msg);
            Out.WriteLine(new string('-', dashLength));
        }

        private static void ConfigureLogger() {
            const string layout = @"${date:format=HH\:MM\:ss} ${logger} ${message}";
            var config = new LoggingConfiguration();
            var console = new ColoredConsoleTarget { Layout = layout };
            var file = new FileTarget { FileName = @"${basedir}\log.txt", Layout = layout };
            
            config.AddTarget("console", console);
            config.AddTarget("file", file);
            config.LoggingRules.Add(new LoggingRule("*", LogLevel.Info, console));
            config.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, file));

            LogManager.Configuration = config;
        }
    }
}
