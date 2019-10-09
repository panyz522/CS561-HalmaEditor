using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace HalmaEditor.Data
{
    public class OutputManager
    {
        public string FilePath { get; set; }

        public string LinkedFilePath { get; set; }

        public event EventHandler OutputChanged;

        private FileSystemWatcher fileWatcher;

        private int cacheHash;

        public OutputManager(IOptionsMonitor<BoardOptions> options)
        {
            this.FilePath = options.CurrentValue.OutputFilePath;
        }

        public Move? GetStartEndPath()
        {
            try
            {
                var lines = File.ReadAllLines(this.FilePath);
                var paths = lines.Where(s => (s.StartsWith("J") || s.StartsWith("E"))).Select(s => {
                    var items = s.Split(" ");
                    var st = items[1].Split(",").Select(s => int.Parse(s)).ToArray();
                    var ed = items[2].Split(",").Select(s => int.Parse(s)).ToArray();
                    return new Move() {
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

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            Task.Delay(500).Wait();
            int cache = File.ReadAllText(this.LinkedFilePath).GetHashCode();
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
    }

    public struct Move
    {
        public bool IsJump;
        public (int, int) Start;
        public (int, int) End;
    }
}
