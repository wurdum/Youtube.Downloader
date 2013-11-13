using System;
using System.Diagnostics;
using System.Threading;

namespace Youtube.Downloader.Console
{
    public class ProgressChangedHandler
    {
        private readonly SpeedCalc _speedCalc;
        private bool _isFirstRaise = true;

        public ProgressChangedHandler(string videoId, int index) {
            VideoId = videoId;
            Index = index;

            _speedCalc = new SpeedCalc();
        }

        public int Index { get; private set; }
        public string VideoId { get; private set; }

        public void OnProgressChanged(object sender, ProgressEventArgs a, int pos, ref SpinLock sync) {
            var isLocked = false;
            try {
                sync.Enter(ref isLocked);

                _speedCalc.Notify(a.BytesReceived);
                var speed = _speedCalc.GetSpeedKbps();

                System.Console.SetCursorPosition(0, pos);
                WriteDownloadState(a, speed);

                if (!_isFirstRaise)
                    return;

                _isFirstRaise = false;
                _speedCalc.Start();
            } finally {
                if (isLocked)
                    sync.Exit();
            }
        }

        public void OnFinished(object sender, ProgressEventArgs a, int pos, ref SpinLock sync) {
            var isLocked = false;
            try {
                sync.Enter(ref isLocked);

                System.Console.SetCursorPosition(0, pos);
                WriteDownloadState(a, 0);
            } finally {
                if (isLocked)
                    sync.Exit();

                _speedCalc.Stop();
            }
        }

        private static void WriteDownloadState(ProgressEventArgs a, int speed) {
            System.Console.ForegroundColor = ConsoleColor.Green;
            System.Console.Write("{0, 5}%\t", Math.Round(a.ProgressPercentage, 0));
            System.Console.ForegroundColor = ConsoleColor.Blue;
            System.Console.Write("{0, 5}kbps\t", speed);
            System.Console.ForegroundColor = ConsoleColor.White;
            System.Console.Write(a.Video.Title + '\n');
        }

        private class SpeedCalc
        {
            private readonly Stopwatch _stopwatch = new Stopwatch();
            private long _counter;
            private long _ctime, _ptime, _cbytes, _pbytes;
            private int _speed;

            public void Start() {
                _stopwatch.Start();
            }

            public void Stop() {
                _stopwatch.Stop();
            }

            public void Notify(long bytesReceived) {
                _ctime = _stopwatch.ElapsedMilliseconds;
                _cbytes = bytesReceived;

                _counter++;
            }

            public int GetSpeedKbps() {
                if (_counter % 1000 != 0 && _speed > 0) {
                    var vagueSpeed = Speed();
                    return (int)(_speed * .5 + vagueSpeed * .5);
                }

                if (_counter % 2000 != 0 && _speed > 0) {
                    var vagueSpeed = Speed();
                    return (int)(_speed * .2 + vagueSpeed * .8);
                }
                
                _speed = Speed();
                _ptime = _ctime;
                _pbytes = _cbytes;
                _counter = 0;
                return _speed;
            }

            private int Speed() {
                return ((int)(((_cbytes - _pbytes) / (double)1024) / ((_ctime - _ptime) / (double)1000)));
            }
        }
    }
}