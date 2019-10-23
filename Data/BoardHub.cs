using HalmaEditor.Tools;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace HalmaEditor.Data
{
    public class BoardHub
    {
        public Dictionary<string, BoardManager> boardManagers = new Dictionary<string, BoardManager>();

        public ILogger Log { get; set; }

        public ReleaseChecker ReleaseChecker { get; set; }

        public ReleaseWithVer NewerRelease { get; set; }

        public bool ReleaseCheckDone { get; private set; } = false;

        public string NewVersionDir { get; set; }

        public event EventHandler ReleaseCheckFinished;

        public ReleaseInstaller Installer { get; set; }

        public BoardHub(ReleaseChecker releaseChecker, ILogger<BoardHub> logger, ReleaseInstaller installer)
        {
            this.ReleaseChecker = releaseChecker;
            this.Installer = installer;
            this.Log = logger;
            Task.Run(async () =>
            {
                this.NewerRelease = await this.ReleaseChecker.GetLatestReleaseIfNewerAsync(Program.Version, Program.Repo, Program.Owner, nameof(HalmaEditor));
                this.ReleaseCheckDone = true;
                try
                {
                    ReleaseCheckFinished?.Invoke(this, EventArgs.Empty);
                }
                catch { }
            });
        }

        public void Register(BoardManager manager)
        {
            this.boardManagers[manager.LinkedFilePath] = manager;
        }

        public void Unregister(string linkedFilePath)
        {
            if (linkedFilePath != null && this.boardManagers.ContainsKey(linkedFilePath))
                this.boardManagers.Remove(linkedFilePath);
        }

        public async Task<bool> InstallUpdateAsync()
        {
            if (this.NewerRelease == null)
            {
                this.Log.LogWarning("No newer version found.");
                return false;
            }
            string osVer = "win64";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                osVer = "osx64";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                osVer = "linux64";
            }
            var asset = this.NewerRelease.Release.Assets.Where((a) => a.Name.Contains(osVer)).FirstOrDefault();
            if (asset == null)
            {
                this.Log.LogWarning($"No release matches current os({osVer})");
                return false;
            }

            string curDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string targetDir = Path.GetDirectoryName(curDir);
            string archiveDir = Path.Combine(curDir, osVer);
            string targetArchiveDir = Path.Combine(targetDir, nameof(HalmaEditor) + this.NewerRelease.Version);
            this.NewVersionDir = targetArchiveDir;

            try
            {
                if (Directory.Exists(archiveDir))
                {
                    Directory.Delete(archiveDir, true);
                }
                await this.Installer.DownloadAndUnzipAsync(asset.BrowserDownloadUrl, curDir, curDir);
            }
            catch (Exception e)
            {
                this.Log.LogWarning($"Couldn't download and unzip file: {e.ToString()}");
                return false;
            }

            try
            {
                if (Directory.Exists(targetArchiveDir))
                {
                    Directory.Delete(targetArchiveDir, true);
                }
                Directory.Move(archiveDir, targetArchiveDir);
            }
            catch (Exception e)
            {
                this.Log.LogWarning($"Couldn't rename folder: {e.ToString()}");
                return false;
            }

            return true;
        }
    }
}
