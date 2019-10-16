using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace HalmaEditor.Data
{
    public class BoardManager : IDisposable
    {
        public HashSet<(int, int)> LeftUps = new HashSet<(int, int)>() { (0, 0), (0, 1), (0, 2), (0, 3), (0, 4), (1, 0), (1, 1), (1, 2), (1, 3), (1, 4), (2, 0), (2, 1), (2, 2), (2, 3), (3, 0), (3, 1), (3, 2), (4, 0), (4, 1) };
        public HashSet<(int, int)> RightDowns = new HashSet<(int, int)>() { (11, 15), (11, 14), (12, 15), (12, 14), (12, 13), (13, 15), (13, 14), (13, 13), (13, 12), (14, 15), (14, 14), (14, 13), (14, 12), (14, 11), (15, 15), (15, 14), (15, 13), (15, 12), (15, 11) };

        public event EventHandler InputFileChanged;

        public BoardOptions Options { get; set; }

        public TitleData Title { get; set; }

        public BoardHub Hub { get; set; }

        public string FilePath { get; set; }

        public string LinkedFilePath { get; set; }

        public int LinkedFileCacheHash { get; set; }

        public bool IsSingleMode { get; set; } = true;

        public bool IsWhite { get; set; } = true;

        public float TimeLeft { get; set; } = 1.0f;

        public double TimeUsedInRunner { get; set; }

        public int[,] Tiles { get; set; } = new int[16, 16];

        private FileSystemWatcher fileWatcher;

        public (int, int) Count
        {
            get
            {
                int b = 0, w = 0;
                for (int i = 0; i < 16; i++)
                {
                    for (int j = 0; j < 16; j++)
                    {
                        if (this.Tiles[i, j] == 1) w += 1;
                        if (this.Tiles[i, j] == 2) b += 1;
                    }
                }
                return (w, b);
            }
        }

        public BoardManager(IOptionsMonitor<BoardOptions> options, BoardHub hub, TitleData title)
        {
            this.Title = title;
            this.Options = options.CurrentValue;
            this.FilePath = options.CurrentValue.FilePath;
            this.Hub = hub;
            for (int i = 0; i < 16; i++)
            {
                for (int j = 0; j < 16; j++)
                {
                    this.Tiles[i, j] = i % 3;
                }
            }
        }

        public bool OpenInput(string path)
        {
            static int TileMap(char t)
            {
                switch (t)
                {
                    case '.':
                        return 0;
                    case 'W':
                        return 1;
                    case 'B':
                        return 2;
                    default:
                        return 0;
                }
            }

            try
            {
                string[] lines = File.ReadAllLines(path);
                this.IsSingleMode = lines[0].StartsWith("SINGLE");
                this.IsWhite = lines[1].StartsWith("WHITE");
                this.TimeLeft = float.Parse(lines[2]);
                for (int i = 0; i < 16; i++)
                {
                    for (int j = 0; j < 16; j++)
                    {
                        this.Tiles[i, j] = TileMap(lines[i + 3][j]);
                    }
                }
            }
            catch
            {
                return false;
            }
            return true;
        }

        public bool SaveInput(string path)
        {
            static string TileMap(int id)
            {
                switch (id)
                {
                    case 0:
                        return ".";
                    case 1:
                        return "W";
                    case 2:
                        return "B";
                    default:
                        return ".";
                }
            }

            try
            {
                var lines = new List<string>();
                lines.Add(this.IsSingleMode ? "SINGLE" : "GAME");
                lines.Add(this.IsWhite ? "WHITE" : "BLACK");
                lines.Add(this.TimeLeft.ToString());
                for (int i = 0; i < 16; i++)
                {
                    lines.Add(string.Join("", Enumerable.Range(0, 16).Select(j => TileMap(this.Tiles[i, j]))));
                }
                File.WriteAllLines(path, lines);
            }
            catch
            {
                return false;
            }
            this.InputFileChanged?.Invoke(this, EventArgs.Empty);
            return true;
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
            this.fileWatcher.Changed += this.OnInputFileChanged;
            this.fileWatcher.EnableRaisingEvents = true;

            this.LinkedFilePath = this.FilePath;
            this.Title.Title = Path.GetFileName(this.LinkedFilePath);
            this.Hub.Register(this);
        }

        private void OnInputFileChanged(object sender, FileSystemEventArgs e)
        {
            Task.Delay(500).Wait();

            var hash = File.ReadAllText(this.LinkedFilePath).GetHashCode();
            if (this.LinkedFileCacheHash == hash)
            {
                return;
            }
            this.LinkedFileCacheHash = hash;
            this.OpenInput(this.LinkedFilePath);
            this.InputFileChanged?.Invoke(sender, e);
        }

        public void DelinkFile()
        {
            this.fileWatcher?.Dispose();
            this.Hub.Unregister(this.LinkedFilePath);
        }

        public void Dispose()
        {
            this.DelinkFile();
        }
    }
}
