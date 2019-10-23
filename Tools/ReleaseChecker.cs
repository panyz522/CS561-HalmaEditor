using Octokit;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HalmaEditor.Tools
{
    public class ReleaseChecker
    {
        public ReleaseChecker()
        {
        }

        public async Task<ReleaseWithVer[]> GetAllReleasesAsync(string repo, string owner, string product)
        {
            var client = new GitHubClient(new ProductHeaderValue(product));
            var rawReleases = await client.Repository.Release.GetAll(owner, repo);
            var releases = rawReleases
                .Select((r) => new ReleaseWithVer { Version = Version.Parse(r.TagName.Substring(1)), Release = r })
                .OrderBy((r) => r.Version)
                .ToArray();
            return releases;
        }

        public async Task<ReleaseWithVer> GetLatestReleaseAsync(string repo, string owner, string product)
        {
            var releases = await this.GetAllReleasesAsync(repo, owner, product);
            if (releases.Length == 0)
            {
                return null;
            }
            return releases[^1];
        }

        public async Task<ReleaseWithVer> GetLatestReleaseIfNewerAsync(Version curVersion, string repo, string owner, string product)
        {
            var release = await this.GetLatestReleaseAsync(repo, owner, product);
            if (release == null || release.Version <= curVersion)
            {
                return null;
            }
            return release;
        }
    }

    public class ReleaseWithVer
    {
        public Version Version { get; set; }
        public Release Release { get; set; }
    }
}
