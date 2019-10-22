using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace HalmaEditor.Data
{
    public class OutputManager : IDisposable
    {
        public string FilePath { get; set; }

        public string LinkedFilePath { get; set; }

        private ILogger Log { get; set; }

        public event EventHandler OutputChanged;

        private FileSystemWatcher fileWatcher;

        private int cacheHash;

        public OutputManager(IOptionsMonitor<BoardOptions> options, ILogger<OutputManager> logger)
        {
            this.FilePath = options.CurrentValue.OutputFilePath;
            this.Log = logger;
        }

        public Move? GetStartEndPath()
        {
            try
            {
                var lines = File.ReadAllLines(this.FilePath);
                var paths = lines.Where(s => (s.StartsWith("J") || s.StartsWith("E"))).Select(s =>
                {
                    var items = s.Split(" ");
                    var st = items[1].Split(",").Select(s => int.Parse(s)).ToArray();
                    var ed = items[2].Split(",").Select(s => int.Parse(s)).ToArray();
                    return new Move()
                    {
                        IsJump = items[0] == "J",
                        Start = (st[1], st[0]),
                        End = (ed[1], ed[0])
                    };
                }).ToArray();
                return new Move() { Start = paths[0].Start, End = paths[^1].End };
            }
            catch
            {
                return null;
            }
        }

        public void LinkFile()
        {
            this.DelinkFile();
            this.fileWatcher = new FileSystemWatcher
            {
                Path = Path.GetDirectoryName(this.FilePath),
                NotifyFilter = NotifyFilters.LastWrite,
                Filter = Path.GetFileName(this.FilePath)
            };

            // Add event handlers.
            this.fileWatcher.Changed += this.OnChanged;
            this.fileWatcher.EnableRaisingEvents = true;

            this.LinkedFilePath = this.FilePath;
        }

        /// <summary>
        /// Read File and check hash (make sure it's different)
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The FileSystemEventArgs.</param>
        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            int cache = this.cacheHash;
            int trycnt = 0;
            while (true)
            {
                Task.Delay(500).Wait();
                try
                {
                    cache = File.ReadAllText(this.LinkedFilePath).GetHashCode();
                }
                catch (Exception)
                {
                    if (trycnt > 3)
                    {
                        Log.LogWarning($"Read output failed {trycnt} times. Aborted");
                        return;
                    }
                    this.Log.LogInformation($"Read output failed {trycnt} times. Retrying...");
                    trycnt++;
                    continue;
                }
                break;
            }
            if (cache == this.cacheHash)
            {
                return;
            }
            this.cacheHash = cache;
            OutputChanged?.Invoke(sender, e);
        }

        public void DelinkFile()
        {
            this.fileWatcher?.Dispose();
        }

        public void Dispose()
        {
            this.DelinkFile();
        }
    }

    public struct Move
    {
        public bool IsJump;
        public (int, int) Start;
        public (int, int) End;
    }
}
