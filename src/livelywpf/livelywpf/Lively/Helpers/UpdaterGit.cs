using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace livelywpf.Lively.Helpers
{
    static class UpdaterGit
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        /// <summary>
        /// Get latest release from github after given delay.
        /// Throws exception.
        /// </summary>
        /// <param name="repositoryName"></param>
        /// <param name="userName"></param>
        /// <param name="startDelay"></param>
        /// <returns></returns>
        public static async Task<Release> GetLatestRelease(string repositoryName, string userName, int startDelay = 45000)
        {
            await Task.Delay(startDelay); //45sec delay (computer startup..)
                                          //await Task.Delay(1000);
            GitHubClient client = new GitHubClient(new ProductHeaderValue(repositoryName));
            var releases = await client.Repository.Release.GetAll(userName, repositoryName);
            var latest = releases[0];

            return latest;
        }

        /// <summary>
        /// Get github release asset download url.
        /// Returns first asset matching substring (make sure asset name is unique to get correct file.), case is ignored.
        /// Throws exception.
        /// </summary>
        /// <param name="assetNameSubstring"></param>
        /// <param name="release"></param>
        /// <param name="repositoryName"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public static async Task<string> GetAssetUrl(string assetNameSubstring, Release release, string repositoryName, string userName)
        {
            GitHubClient client = new GitHubClient(new ProductHeaderValue(repositoryName));
            var allAssets = await client.Repository.Release.GetAllAssets(userName, repositoryName, release.Id);
            //var requiredAssets = allAssets.Single(x => x.Name.Equals(assetName, StringComparison.OrdinalIgnoreCase));
            var requiredAsset = allAssets.First(x => Contains(x.Name, assetNameSubstring, StringComparison.OrdinalIgnoreCase)); 
            return requiredAsset.BrowserDownloadUrl;
        }

        /// <summary>
        /// Compare current software assembly version with given release version.
        /// </summary>
        /// <param name="release"></param>
        /// <returns> =0 same, >0 git greater, <0 pgm greater</returns>
        public static int CompareAssemblyVersion(Release release)
        {
            string tmp = Regex.Replace(release.TagName, "[A-Za-z ]", "");
            var gitVersion = new Version(tmp);
            var appVersion = new Version(System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
            var result = gitVersion.CompareTo(appVersion);

            return result;
        }

        /// <summary>
        /// String Contains method with StringComparison property.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="substring"></param>
        /// <param name="comp"></param>
        /// <returns></returns>
        public static bool Contains(String str, String substring,
                                    StringComparison comp)
        {
            if (substring == null)
                throw new ArgumentNullException("substring",
                                             "substring cannot be null.");
            else if (!Enum.IsDefined(typeof(StringComparison), comp))
                throw new ArgumentException("comp is not a member of StringComparison",
                                         "comp");

            return str.IndexOf(substring, comp) >= 0;
        }
    }
}
