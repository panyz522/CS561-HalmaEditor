using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using HalmaEditor.Tools;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HalmaEditor
{
    public class Program
    {
        public const string VerStr = "v1.5";
        public const string Repo = "CS561-HalmaEditor";
        public const string Owner = "panyz522";
        public static readonly Version Version = Version.Parse(VerStr.Substring(1));

        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    #if !DEBUG
                    webBuilder.UseContentRoot(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
                    #endif
                    webBuilder.UseStartup<Startup>();
                });
    }
}
