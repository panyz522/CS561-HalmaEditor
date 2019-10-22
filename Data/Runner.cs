using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics;
using System.IO;
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

        public RunnerLog P1Log { get; set; }

        public RunnerLog P2Log { get; set; }

        public Runner(BoardHub hub, ILogger<Runner> log, IOptionsMonitor<BoardOptions> options, TitleData title)
        {
            this.P1CmdString = options.CurrentValue.Player1Cmd;
            this.P1WorkDir = options.CurrentValue.Player1WorkDir;
            this.P2CmdString = options.CurrentValue.Player2Cmd;
            this.P2WorkDir = options.CurrentValue.Player2WorkDir;
            this.P1Log = new RunnerLog(options.CurrentValue.Player1LogFilePath);
            this.P2Log = new RunnerLog(options.CurrentValue.Player2LogFilePath);
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
            if ((this.BoundBoardManager?.TimeLeft ?? 1f) <= 0)
            {
                return ("No time left!", true, 0);
            }
            string cmdStr = player == 1 ? this.P1CmdString : this.P2CmdString;
            string workDir = player == 1 ? this.P1WorkDir : this.P2WorkDir;
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                this.Cmd(cmdStr, workDir);
            }
            else
            {
                this.Bash(cmdStr, workDir);
            }

            DateTime startTime = DateTime.Now;
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
            double usedTime = (DateTime.Now - startTime).TotalSeconds;

            if (!finished)
            {
                this.process.Kill(true);
            }
            string result = await this.process.StandardOutput.ReadToEndAsync();
            this.process.Dispose();

            if (player == 1) { this.P1Log.AppendLog(result, usedTime); }
            else { this.P2Log.AppendLog(result, usedTime); }

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

    public class RunnerLog
    {
        public string FileName { get; set; }

        public RunnerLog(string fileName)
        {
            this.FileName = fileName;
        }

        public bool AppendLog(string log, double time)
        {
            try
            {
                File.AppendAllText(this.FileName, $"=== {DateTime.Now:HH:mm:ss.fff} === Time Elapsed: {time:F} s ===\n" + log + "\n");
            }
            catch
            {
                return false;
            }
            return true;
        }

        public bool DeleteLogFile()
        {
            try
            {
                File.Delete(this.FileName);
            }
            catch
            {
                return false;
            }
            return true;
        }
    }
}
