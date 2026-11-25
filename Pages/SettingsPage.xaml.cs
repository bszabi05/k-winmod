using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MyTool.Services;
using System;
using System.Diagnostics;

namespace MyTool.Pages
{
    public sealed partial class SettingsPage : Page
    {
        private readonly UpdateService _updateService = new UpdateService();
        private string _latestReleaseUrl = AppInfo.GitHubRepoUrl + "/releases";

        public SettingsPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            CurrentVersionBadge.Text = AppInfo.DisplayVersion;
        }

        private async void CheckUpdates_Click(object sender, RoutedEventArgs e)
        {
            CheckUpdatesButton.IsEnabled = false;
            UpdateProgress.IsActive = true;
            DownloadUpdateButton.Visibility = Visibility.Collapsed;
            UpdateStatusText.Text = "Connecting to GitHub API...";

            var result = await _updateService.CheckForUpdatesAsync();

            if (!result.IsSuccess)
            {
                UpdateStatusText.Text = $"Update check failed: {result.ErrorMessage}";
                MainWindow.Instance.ShowNotification("Failed to check for updates. Check your internet connection.");
            }
            else if (result.IsUpdateAvailable)
            {
                _latestReleaseUrl = result.ReleaseUrl;
                UpdateStatusText.Text = $"New release available: {result.LatestVersion} ({result.ReleaseName})!";
                DownloadUpdateButton.Visibility = Visibility.Visible;
                MainWindow.Instance.ShowNotification($"Update available: {result.LatestVersion}!");
            }
            else
            {
                UpdateStatusText.Text = $"You are up to date! Latest release on GitHub: {result.LatestVersion}.";
                MainWindow.Instance.ShowNotification("K-WinMod is up to date.");
            }

            UpdateProgress.IsActive = false;
            CheckUpdatesButton.IsEnabled = true;
        }

        private void DownloadUpdate_Click(object sender, RoutedEventArgs e)
        {
            UpdateService.OpenUrl(_latestReleaseUrl);
        }

        private void OpenLogs_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string logDir = LoggerService.GetLogDirectory();
                Process.Start(new ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = logDir,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                LoggerService.LogError("Failed to open Logs folder", ex);
                MainWindow.Instance.ShowNotification("Could not open Logs folder.");
            }
        }

        private void ClearLogs_Click(object sender, RoutedEventArgs e)
        {
            int count = LoggerService.ClearLogs();
            MainWindow.Instance.ShowNotification($"Cleared {count} log file(s).");
        }

        private void OpenGitHub_Click(object sender, RoutedEventArgs e)
        {
            UpdateService.OpenUrl(AppInfo.GitHubRepoUrl);
        }
    }
}
