using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace MyTool.Services
{
    public static class AppInfo
    {
        public const string CurrentVersion = "0.2.0-beta";
        public const string DisplayVersion = "v" + CurrentVersion;
        public const string GitHubRepoUrl = "https://github.com/bszabi05/k-winmod";
        public const string GitHubReleasesApiUrl = "https://api.github.com/repos/bszabi05/k-winmod/releases?per_page=5";
    }

    public class UpdateCheckResult
    {
        public bool IsSuccess { get; set; }
        public bool IsUpdateAvailable { get; set; }
        public string LatestVersion { get; set; } = string.Empty;
        public string ReleaseName { get; set; } = string.Empty;
        public string ReleaseUrl { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
    }

    public class UpdateService
    {
        private static readonly HttpClient _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(10)
        };

        static UpdateService()
        {
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("K-WinMod-UpdateChecker/1.0");
            _httpClient.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github+json");
        }

        public async Task<UpdateCheckResult> CheckForUpdatesAsync()
        {
            try
            {
                LoggerService.LogInfo("Checking GitHub for K-WinMod updates...");
                string json = await _httpClient.GetStringAsync(AppInfo.GitHubReleasesApiUrl);

                using JsonDocument doc = JsonDocument.Parse(json);
                JsonElement root = doc.RootElement;

                if (root.ValueKind != JsonValueKind.Array || root.GetArrayLength() == 0)
                {
                    return new UpdateCheckResult
                    {
                        IsSuccess = true,
                        IsUpdateAvailable = false,
                        LatestVersion = AppInfo.DisplayVersion
                    };
                }

                // Get the latest release (first item in the array)
                JsonElement latestRelease = root[0];
                string tagName = latestRelease.TryGetProperty("tag_name", out var tagProp) ? tagProp.GetString() ?? "" : "";
                string htmlUrl = latestRelease.TryGetProperty("html_url", out var urlProp) ? urlProp.GetString() ?? AppInfo.GitHubRepoUrl : AppInfo.GitHubRepoUrl;
                string releaseName = latestRelease.TryGetProperty("name", out var nameProp) ? nameProp.GetString() ?? tagName : tagName;

                bool isNewer = IsNewerVersion(tagName, AppInfo.CurrentVersion);

                LoggerService.LogInfo($"Update check complete. Current: {AppInfo.CurrentVersion}, Latest on GitHub: {tagName}, UpdateAvailable: {isNewer}");

                return new UpdateCheckResult
                {
                    IsSuccess = true,
                    IsUpdateAvailable = isNewer,
                    LatestVersion = tagName,
                    ReleaseName = releaseName,
                    ReleaseUrl = htmlUrl
                };
            }
            catch (Exception ex)
            {
                LoggerService.LogError("Failed to check for updates from GitHub", ex);
                return new UpdateCheckResult
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public static bool IsNewerVersion(string remoteTag, string currentVersion)
        {
            if (string.IsNullOrWhiteSpace(remoteTag)) return false;

            (Version remoteVer, bool remoteIsPrerelease) = ParseVersion(remoteTag);
            (Version currentVer, bool currentIsPrerelease) = ParseVersion(currentVersion);

            int cmp = remoteVer.CompareTo(currentVer);
            if (cmp > 0) return true;
            if (cmp < 0) return false;

            // Same numeric version: stable (non-prerelease) is newer than prerelease
            return currentIsPrerelease && !remoteIsPrerelease;
        }

        private static (Version version, bool isPrerelease) ParseVersion(string raw)
        {
            string cleaned = raw.Trim().TrimStart('v', 'V');
            bool isPre = cleaned.Contains('-');
            string numericPart = cleaned.Split('-')[0];

            if (!Version.TryParse(numericPart, out Version? parsed))
            {
                parsed = new Version(0, 0, 0);
            }

            return (parsed, isPre);
        }

        public static void OpenUrl(string url)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                LoggerService.LogError($"Failed to open URL: {url}", ex);
            }
        }
    }
}
