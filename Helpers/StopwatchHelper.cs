using System.Diagnostics;

namespace SqlMigrate.Helpers
{
    public class StopwatchHelper : IDisposable
    {
        private readonly Stopwatch _stopwatch;

        public StopwatchHelper()
        {
            _stopwatch = new Stopwatch();
            _stopwatch.Start();
        }

        public void Stop()
        {
            _stopwatch.Stop();
            TimeSpan ts = _stopwatch.Elapsed;
            int hours = (int)ts.TotalHours;
            int minutes = (int)ts.TotalMinutes;
            int seconds = (int)ts.TotalSeconds % 60;
            Console.WriteLine($"[FINISH] TIMES UP : {hours:D2} hours, {minutes:D2} minutes, {seconds:D2} seconds.");
        }

        public void Dispose() => Stop();
    }
}

