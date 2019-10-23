using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.IO.Compression;
using System.IO;

namespace HalmaEditor.Tools
{
    public class ReleaseInstaller
    {
        public ReleaseInstaller()
        {

        }

        public async Task DownloadAndUnzipAsync(string zipUrl, string tempDir, string targetDir)
        {
            string zipFileFullName = Path.Combine(tempDir, zipUrl.Split("/")[^1]);

            Directory.CreateDirectory(tempDir);
            if (File.Exists(zipFileFullName))
            {
                File.Delete(zipFileFullName);
            }
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            using (WebClient client = new WebClient())
            {
                await client.DownloadFileTaskAsync(zipUrl, zipFileFullName);
            }

            Directory.CreateDirectory(targetDir);
            ZipFile.ExtractToDirectory(zipFileFullName, targetDir);
        }
    }
}
