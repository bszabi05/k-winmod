using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using MyTool.Services;

namespace MyTool
{
    public partial class OptimizationPage : Page
    {

        private readonly OptimizationService _optimizationService = new OptimizationService(); /// service for optimization functions

        public OptimizationPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e) // when navigated to page
        {
            base.OnNavigatedTo(e);

            // get telemetry status
            TelemetryToggle.Toggled -= TelemetryToggle_Toggled;
            TelemetryToggle.IsOn = _optimizationService.IsTelemetryEnabled();
            TelemetryToggle.Toggled += TelemetryToggle_Toggled;

            // get suggestinn status
            TipsToggle.Toggled -= TipsToggle_Toggled;
            TipsToggle.IsOn = _optimizationService.IsTipsEnabled();
            TipsToggle.Toggled += TipsToggle_Toggled;

            // Ultimate Performance status
            UltimatePerfToggle.Toggled -= UltimatePerfToggle_Toggled;
            UltimatePerfToggle.IsOn = _optimizationService.IsUltimatePerformanceActive();
            UltimatePerfToggle.Toggled += UltimatePerfToggle_Toggled;

            // Game Mode status
            GameModeToggle.Toggled -= GameModeToggle_Toggled;
            GameModeToggle.IsOn = _optimizationService.IsGameModeEnabled();
            GameModeToggle.Toggled += GameModeToggle_Toggled;

            // Bing Search status
            BingSearchToggle.Toggled -= BingSearchToggle_Toggled;
            BingSearchToggle.IsOn = _optimizationService.IsBingSearchDisabled();
            BingSearchToggle.Toggled += BingSearchToggle_Toggled;
        }

        
        private async void TelemetryToggle_Toggled(object sender, RoutedEventArgs e) // changing telemetry 
        {
            bool shouldEnable = TelemetryToggle.IsOn;
            MainWindow.Instance.ShowNotification(shouldEnable ? "Enabling Windows Telemetry..." : "Disabling Windows Telemetry...");

            TelemetryToggle.Toggled -= TelemetryToggle_Toggled;
            bool success = await _optimizationService.SetTelemetryStateAsync(shouldEnable);

            if (success)
            {
                MainWindow.Instance.ShowNotification(shouldEnable ? "Telemetry ENABLED (Default Windows behavior)." : "Telemetry DISABLED (Background logging stopped).");
            }
            else
            {
                MainWindow.Instance.ShowNotification("Failed to modify settings. Ensure the app is running as Administrator.");
                TelemetryToggle.IsOn = !shouldEnable; 
            }

            TelemetryToggle.Toggled += TelemetryToggle_Toggled;
        }

        
        private async void CleanDisk_Click(object sender, RoutedEventArgs e) // cleaning temp files
        {
            MainWindow.Instance.ShowNotification("Cleaning temporary files...");

            long totalBytesFreed = await _optimizationService.CleanTemporaryFilesAsync();

            double megabytesFreed = Math.Round((double)totalBytesFreed / (1024 * 1024), 2);
            MainWindow.Instance.ShowNotification($"Disk cleanup completed! Freed up: {megabytesFreed} MB.");
        }

        
        private async void FlushDNS_Click(object sender, RoutedEventArgs e) // cleaning DNS cache
        {
            MainWindow.Instance.ShowNotification("Flushing DNS Cache...");

            bool success = await _optimizationService.FlushDNSCacheAsync();

            MainWindow.Instance.ShowNotification(success ? "DNS Cache successfully flushed!" : "Failed to flush DNS Cache.");
        }

      
        private async void OptimizeRAM_Click(object sender, RoutedEventArgs e) // optimizing memory
        {
            MainWindow.Instance.ShowNotification("Optimizing system memory...");

            int optimizedProcesses = await _optimizationService.OptimizeMemoryAsync();

            MainWindow.Instance.ShowNotification($"Memory optimization finished! Trimmed {optimizedProcesses} active processes.");
        }

        private async void TipsToggle_Toggled(object sender, RoutedEventArgs e) // suggestion, tips, ads 
        {
            bool shouldEnable = TipsToggle.IsOn;
            MainWindow.Instance.ShowNotification(shouldEnable ? "Enabling Windows Tips & Suggestions..." : "Disabling Windows Tips & Ads...");

            TipsToggle.Toggled -= TipsToggle_Toggled;

            bool success = await _optimizationService.SetTipsStateAsync(shouldEnable);

            if (success)
            {
                MainWindow.Instance.ShowNotification(shouldEnable ? "Windows Tips and suggestions are now ENABLED." : "Windows Tips and promotional ads are now DISABLED.");
            }
            else
            {
                MainWindow.Instance.ShowNotification("Failed to modify settings. Ensure the app has proper permissions.");
                TipsToggle.IsOn = !shouldEnable; 
            }

            TipsToggle.Toggled += TipsToggle_Toggled;
        }

        private async void UltimatePerfToggle_Toggled(object sender, RoutedEventArgs e)
        {
            bool shouldEnable = UltimatePerfToggle.IsOn;
            MainWindow.Instance.ShowNotification(shouldEnable ? "Enabling Ultimate Performance Mode..." : "Restoring default power plan...");

            UltimatePerfToggle.Toggled -= UltimatePerfToggle_Toggled;
            bool success = await _optimizationService.SetUltimatePerformanceStateAsync(shouldEnable);

            if (success)
            {
                MainWindow.Instance.ShowNotification(shouldEnable ? "Ultimate Performance power plan ACTIVATED." : "Power plan restored to Balanced.");
            }
            else
            {
                MainWindow.Instance.ShowNotification("Failed to update power plan. Run as Administrator.");
                UltimatePerfToggle.IsOn = !shouldEnable;
            }
            UltimatePerfToggle.Toggled += UltimatePerfToggle_Toggled;
        }

        private async void GameModeToggle_Toggled(object sender, RoutedEventArgs e)
        {
            bool shouldEnable = GameModeToggle.IsOn;
            MainWindow.Instance.ShowNotification(shouldEnable ? "Enabling Windows Game Mode..." : "Disabling Windows Game Mode...");

            GameModeToggle.Toggled -= GameModeToggle_Toggled;
            bool success = await _optimizationService.SetGameModeStateAsync(shouldEnable);

            if (success)
            {
                MainWindow.Instance.ShowNotification(shouldEnable ? "Windows Game Mode ENABLED." : "Windows Game Mode DISABLED.");
            }
            else
            {
                MainWindow.Instance.ShowNotification("Failed to change Game Mode settings.");
                GameModeToggle.IsOn = !shouldEnable;
            }
            GameModeToggle.Toggled += GameModeToggle_Toggled;
        }

        private async void BingSearchToggle_Toggled(object sender, RoutedEventArgs e)
        {
            bool shouldDisable = BingSearchToggle.IsOn;
            MainWindow.Instance.ShowNotification(shouldDisable ? "Disabling Bing Search in Start Menu..." : "Enabling Bing Search in Start Menu...");

            BingSearchToggle.Toggled -= BingSearchToggle_Toggled;
            bool success = await _optimizationService.SetBingSearchDisabledStateAsync(shouldDisable);

            if (success)
            {
                MainWindow.Instance.ShowNotification(shouldDisable ? "Start Menu Bing Search DISABLED." : "Start Menu Bing Search ENABLED.");
            }
            else
            {
                MainWindow.Instance.ShowNotification("Failed to change Start Menu Search policies.");
                BingSearchToggle.IsOn = !shouldDisable;
            }
            BingSearchToggle.Toggled += BingSearchToggle_Toggled;
        }
    }
}