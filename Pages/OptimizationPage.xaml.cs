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
    }
}