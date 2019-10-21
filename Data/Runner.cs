using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace HalmaEditor.Data
{
    public class Runner : IDisposable
    {
        public event EventHandler<RunnerTriggeredEventArgs> RunnerTrigger;

        public TitleData Title { get; set; }

        public string P1CmdString { get; set; }

        public string P1WorkDir { get; set; }

        public string P2CmdString { get; set; }

        public string P2WorkDir { get; set; }

        public BoardHub Hub { get; set; }

        public ILogger Log { get; set; }

        private Process process;

        public BoardManager BoundBoardManager { get; set; }

        public Runner(BoardHub hub, ILogger<Runner> log, IOptionsMonitor<BoardOptions> options, TitleData title)
        {
            this.P1CmdString = options.CurrentValue.Player1Cmd;
            this.P1WorkDir = options.CurrentValue.Player1WorkDir;
            this.P2CmdString = options.CurrentValue.Player2Cmd;
            this.P2WorkDir = options.CurrentValue.Player2WorkDir;
            this.Hub = hub;
            this.Log = log;
            this.Title = title;
            title.Title = "Game Runner";
        }

        public void OnBoardTriggered(object sender, RunnerTriggeredEventArgs e)
        {
            RunnerTrigger?.Invoke(sender, e);
        }

        public void BindBoardManager(string path)
        {
            if (!this.Hub.boardManagers.ContainsKey(path))
            {
                return;
            }
            this.BoundBoardManager = this.Hub.boardManagers[path];
            this.BoundBoardManager.InputFileChanged += this.OnBoardTriggered;
        }

        public void UnbindBoardManager()
        {
            if (this.BoundBoardManager == null)
                return;
            this.BoundBoardManager.InputFileChanged -= this.OnBoardTriggered;
            this.BoundBoardManager = null;
        }

        public async Task<(string, bool, double)> Run(int player)
        {
            string cmdStr = player == 1 ? this.P1CmdString : this.P1CmdString;
            string workDir = player == 1 ? this.P1WorkDir : this.P2WorkDir;
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                this.Cmd(cmdStr, workDir);
            }
            else
            {
                this.Bash(cmdStr, workDir);
            }

            try
            {
                this.process.Start();
            }
            catch (Exception e)
            {
                this.Log.LogError(e.ToString());
                return ($"Process start error! Please check command and workdir.\n{e.Message}", true, 0);
            }

            bool finished = true;
            using (var cts = new CancellationTokenSource((int)((this.BoundBoardManager?.TimeLeft ?? 10f) * 1000))) // Default to wait 10s
            {
                try
                {
                    await WaitForExitAsync(this.process, cts.Token);
                }
                catch (TaskCanceledException)
                {
                    this.Log.LogInformation("Execution Cancelled.");
                    finished = true;
                }
            }

            if (!finished)
            {
                this.process.Kill(true);
            }
            string result = await this.process.StandardOutput.ReadToEndAsync();

            double usedTime = 0;
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                usedTime = this.process.UserProcessorTime.TotalSeconds;

            this.process.Dispose();

            if (this.BoundBoardManager != null)
                this.BoundBoardManager.TimeUsedInRunner = usedTime;
            return (result, finished, usedTime);
        }

        public static Task WaitForExitAsync(Process process, CancellationToken cancellationToken = default)
        {
            var tcs = new TaskCompletionSource<object>();
            process.EnableRaisingEvents = true;
            process.Exited += (sender, args) => { try { tcs.TrySetResult(null); } catch { } };
            if (cancellationToken != default)
                cancellationToken.Register(tcs.SetCanceled);

            return tcs.Task;
        }

        public void Bash(string cmd, string workdir)
        {
            var escapedArgs = cmd.Replace("\"", "\\\"");

            this.process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    WorkingDirectory = workdir,
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{escapedArgs}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };
        }

        public void Cmd(string cmd, string workdir)
        {
            this.process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    WorkingDirectory = workdir,
                    FileName = "cmd.exe",
                    Arguments = $"/C \"{cmd}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };
        }

        public void Dispose()
        {
            this.UnbindBoardManager();
            try
            {
                if (!(this.process?.HasExited ?? true))
                {
                    this.process.Kill(true);
                }
            }
            catch { }
        }
    }
}
