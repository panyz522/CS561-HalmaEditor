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
        public BoardOptions Options { get; set; }

        public string FilePath { get; set; }

        public string LinkedFilePath { get; set; }

        public bool IsSingleMode { get; set; } = true;

        public bool IsWhite { get; set; } = true;

        public float TimeLeft { get; set; } = 1.0f;

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

        public BoardManager(IOptionsMonitor<BoardOptions> options)
        {
            this.Options = options.CurrentValue;
            this.FilePath = options.CurrentValue.FilePath;
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
            this.fileWatcher.Changed += this.OnChanged;
            this.fileWatcher.EnableRaisingEvents = true;

            this.LinkedFilePath = this.FilePath;
        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            Task.Delay(500).Wait();
            this.OpenInput(this.LinkedFilePath);
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
}
