using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NDesk.Options;

namespace Youtube.Downloader.Console
{
    public class ProgramOptionsSet : OptionSet
    {
        private static readonly Regex UrlRx = new Regex(@"^(https?://)?(www\.)?(youtube\.com|youtu\.be).*", 
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public List<string> Urls { get; set; }
        public string PathToSave { get; set; }
        public bool FormatsOnly { get; set; }
        public bool Help { get; set; }

        public bool Parse(string[] args, out string error ) {
            var urls = Parse(args).SelectMany(u => u.Split(new[] {" "}, StringSplitOptions.RemoveEmptyEntries)).ToList();
            if (string.IsNullOrWhiteSpace(PathToSave))
                PathToSave = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            var invalidUrls = urls.Where(url => !UrlRx.IsMatch(url)).ToArray();

            if (invalidUrls.Length > 0) {
                error = string.Format("'{0}' {1} not valid url{2}", string.Join(", ", invalidUrls), 
                    invalidUrls.Length == 1 ? "is" : "are", invalidUrls.Length == 1 ? "" : "s");
                return false;
            }

            if (!Directory.Exists(PathToSave)) {
                error = string.Format("Directory '{0}' doesn't exists", PathToSave);
                return false;
            }

            error = null;
            Urls = urls;
            
            return true;
        }

        public void ShowHelp(string message = null) {
            if (!string.IsNullOrWhiteSpace(message))
                System.Console.WriteLine(message);

            WriteCommandsExamples(System.Console.Out);
            WriteOptionDescriptions(System.Console.Out);
        }

        private void WriteCommandsExamples(TextWriter o) {
            o.WriteLine("");
            o.WriteLine("Examples:");
            o.Write("Youtube.Downloader.Console.exe http://www.youtube.com/watch?v=xxxxxxxxx http://www.youtube.com/watch?v=yyyyyyyyyy");
            o.WriteLine(" - download two videos");
            o.Write("Youtube.Downloader.Console.exe -f http://www.youtube.com/watch?v=xxxxxxxxx");
            o.WriteLine(" - just show list of available video formats");
            o.Write("Youtube.Downloader.Console.exe -p D:\\ http://www.youtube.com/watch?v=xxxxxxxxx");
            o.WriteLine(" - download video and save it to D:\\");
            o.Write("Youtube.Downloader.Console.exe -h");
            o.WriteLine(" - show help");
        }

        public new void WriteOptionDescriptions(TextWriter o) {
            o.WriteLine("");
            o.WriteLine("Options:");
            base.WriteOptionDescriptions(o);
            var listOption = "  [ ... ]";
            o.Write(listOption);
            o.Write(new string(' ', 29 - listOption.Length));
            o.WriteLine("List of videos urls from www.youtube.com separated by spaces");
        }
    }
}