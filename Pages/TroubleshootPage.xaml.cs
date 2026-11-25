using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using MyTool.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace MyTool.Pages;

public partial class TroubleshootPage : Page
{
    private readonly TroubleshootService _troubleshootService = new TroubleshootService();

    public TroubleshootPage()
    {
        this.InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        await UpdateTouchpadWarningStatusAsync();
        _ = UpdateLatestRestorePointStatusAsync();
    }

    private async Task UpdateLatestRestorePointStatusAsync()
    {
        try
        {
            string? latest = await _troubleshootService.GetLatestRestorePointInfoAsync();
            LastRestorePointText.Text = !string.IsNullOrWhiteSpace(latest) ? latest : "None found or Protection off";
        }
        catch (Exception)
        {
            LastRestorePointText.Text = "Unavailable";
        }
    }

    private async void CreateRestorePoint_Click(object sender, RoutedEventArgs e)
    {
        CreateRestorePointButton.IsEnabled = false;
        OpenRestoreWizardButton.IsEnabled = false;
        RestorePointProgress.IsActive = true;
        CreateRestorePointButton.Content = "Creating...";

        MainWindow.Instance.ShowNotification("Creating System Restore Point... Please wait.");

        string checkpointName = $"K-WinMod Checkpoint - {DateTime.Now:yyyy-MM-dd HH:mm}";
        var (success, message) = await _troubleshootService.CreateRestorePointAsync(checkpointName);

        if (success)
        {
            CreateRestorePointButton.Content = "Created!";
            MainWindow.Instance.ShowNotification("System Restore Point created successfully!");
            await Task.Delay(1200);
            await UpdateLatestRestorePointStatusAsync();
            if (LastRestorePointText.Text.StartsWith("None") || LastRestorePointText.Text.StartsWith("Unavailable"))
            {
                LastRestorePointText.Text = $"{DateTime.Now:yyyy-MM-dd HH:mm} ({checkpointName})";
            }
        }
        else
        {
            CreateRestorePointButton.Content = "Failed";
            MainWindow.Instance.ShowNotification($"Restore point creation failed: {message}");
        }

        await Task.Delay(2500);

        RestorePointProgress.IsActive = false;
        CreateRestorePointButton.IsEnabled = true;
        OpenRestoreWizardButton.IsEnabled = true;
        CreateRestorePointButton.Content = "Create Checkpoint";
    }

    private void OpenRestoreWizard_Click(object sender, RoutedEventArgs e)
    {
        MainWindow.Instance.ShowNotification("Launching Windows System Restore (rstrui.exe)...");
        bool opened = _troubleshootService.OpenSystemRestoreWizard();
        if (!opened)
        {
            MainWindow.Instance.ShowNotification("Failed to launch System Restore Wizard. Ensure proper permissions.");
        }
    }

   // check I2C HID status
    private async Task UpdateTouchpadWarningStatusAsync()
    {
        bool hasError = await _troubleshootService.CheckTouchpadErrorStatusAsync();

        if (hasError)
        {
            TouchpadWarningIcon.Visibility = Visibility.Visible; 
        }
        else
        {
            TouchpadWarningIcon.Visibility = Visibility.Collapsed;
        }
    }

    private async void FixTouchpad_Click(object sender, RoutedEventArgs e)
    {
        TouchpadFixButton.IsEnabled = false;
        TouchpadProgress.IsActive = true;
        TouchpadFixButton.Content = "Fixing...";

        MainWindow.Instance.ShowNotification("Executing hardware cycle on Touchpad...");

        bool success = await _troubleshootService.RestartTouchpadDriverAsync();

        if (success)
        {
            TouchpadFixButton.Content = "Success!";
            MainWindow.Instance.ShowNotification("Touchpad device successfully restarted!");
        }
        else
        {
            TouchpadFixButton.Content = "Failed";
            MainWindow.Instance.ShowNotification("Touchpad fix failed. Ensure Admin rights.");
        }

        await UpdateTouchpadWarningStatusAsync();

        await Task.Delay(2000);

        TouchpadProgress.IsActive = false;
        TouchpadFixButton.IsEnabled = true;
        TouchpadFixButton.Content = "Run Fix";
    }

    private async void FixAudio_Click(object sender, RoutedEventArgs e)
    {
        AudioFixButton.IsEnabled = false;
        AudioProgress.IsActive = true;
        AudioFixButton.Content = "Fixing...";

        MainWindow.Instance.ShowNotification("Restarting Windows Audio Services...");

        bool success = await _troubleshootService.RestartAudioServicesAsync();

        if (success)
        {
            AudioFixButton.Content = "Success!";
            MainWindow.Instance.ShowNotification("Audio services successfully restarted!");
        }
        else
        {
            AudioFixButton.Content = "Failed";
            MainWindow.Instance.ShowNotification("Audio fix failed. Ensure Admin rights.");
        }

        await Task.Delay(2000);

        AudioProgress.IsActive = false;
        AudioFixButton.IsEnabled = true;
        AudioFixButton.Content = "Run Fix";
    }

    private async void FixExplorer_Click(object sender, RoutedEventArgs e)
    {
        ExplorerFixButton.IsEnabled = false;
        ExplorerProgress.IsActive = true;
        ExplorerFixButton.Content = "Fixing...";

        MainWindow.Instance.ShowNotification("Restarting Windows Explorer shell...");

        bool success = await _troubleshootService.RestartExplorerAsync();

        if (success)
        {
            ExplorerFixButton.Content = "Success!";
            MainWindow.Instance.ShowNotification("Taskbar and Explorer shell restarted successfully!");
        }
        else
        {
            ExplorerFixButton.Content = "Failed";
            MainWindow.Instance.ShowNotification("Failed to restart Explorer shell.");
        }

        await Task.Delay(2000);

        ExplorerProgress.IsActive = false;
        ExplorerFixButton.IsEnabled = true;
        ExplorerFixButton.Content = "Run Fix";
    }

    private async void FixNetwork_Click(object sender, RoutedEventArgs e)
    {
        NetworkFixButton.IsEnabled = false;
        NetworkProgress.IsActive = true;
        NetworkFixButton.Content = "Fixing...";

        MainWindow.Instance.ShowNotification("Resetting Winsock and Network Adapters...");

        bool success = await _troubleshootService.ResetNetworkStackAsync();

        if (success)
        {
            NetworkFixButton.Content = "Success!";
            MainWindow.Instance.ShowNotification("Network stack successfully reset!");
        }
        else
        {
            NetworkFixButton.Content = "Failed";
            MainWindow.Instance.ShowNotification("Network reset failed. Admin rights required.");
        }

        await Task.Delay(2000);

        NetworkProgress.IsActive = false;
        NetworkFixButton.IsEnabled = true;
        NetworkFixButton.Content = "Run Fix";
    }

    private async void FixPrinter_Click(object sender, RoutedEventArgs e)
    {
        PrinterFixButton.IsEnabled = false;
        PrinterProgress.IsActive = true;
        PrinterFixButton.Content = "Fixing...";

        MainWindow.Instance.ShowNotification("Clearing stuck print queue...");

        bool success = await _troubleshootService.ClearPrintSpoolerAsync();

        if (success)
        {
            PrinterFixButton.Content = "Success!";
            MainWindow.Instance.ShowNotification("Print spooler cleared and service restarted!");
        }
        else
        {
            PrinterFixButton.Content = "Failed";
            MainWindow.Instance.ShowNotification("Failed to clear print queue. Admin rights required.");
        }

        await Task.Delay(2000);

        PrinterProgress.IsActive = false;
        PrinterFixButton.IsEnabled = true;
        PrinterFixButton.Content = "Run Fix";
    }

    private async void FixBluetooth_Click(object sender, RoutedEventArgs e)
    {
        BluetoothFixButton.IsEnabled = false;
        BluetoothProgress.IsActive = true;
        BluetoothFixButton.Content = "Fixing...";

        MainWindow.Instance.ShowNotification("Restarting Bluetooth service...");

        bool success = await _troubleshootService.RestartBluetoothAsync();

        if (success)
        {
            BluetoothFixButton.Content = "Success!";
            MainWindow.Instance.ShowNotification("Bluetooth service restarted successfully!");
        }
        else
        {
            BluetoothFixButton.Content = "Failed";
            MainWindow.Instance.ShowNotification("Failed to restart Bluetooth service.");
        }

        await Task.Delay(2000);

        BluetoothProgress.IsActive = false;
        BluetoothFixButton.IsEnabled = true;
        BluetoothFixButton.Content = "Run Fix";
    }

    private async void FixUpdateCache_Click(object sender, RoutedEventArgs e)
    {
        UpdateCacheFixButton.IsEnabled = false;
        UpdateCacheProgress.IsActive = true;
        UpdateCacheFixButton.Content = "Fixing...";

        MainWindow.Instance.ShowNotification("Clearing Windows Update cache...");

        bool success = await _troubleshootService.ClearUpdateCacheAsync();

        if (success)
        {
            UpdateCacheFixButton.Content = "Success!";
            MainWindow.Instance.ShowNotification("Windows Update cache cleared successfully!");
        }
        else
        {
            UpdateCacheFixButton.Content = "Failed";
            MainWindow.Instance.ShowNotification("Failed to clear update cache. Admin rights required.");
        }

        await Task.Delay(2000);

        UpdateCacheProgress.IsActive = false;
        UpdateCacheFixButton.IsEnabled = true;
        UpdateCacheFixButton.Content = "Run Fix";
    }

    private async void FixSfc_Click(object sender, RoutedEventArgs e)
    {
        SfcFixButton.IsEnabled = false;
        SfcProgress.IsActive = true;
        SfcFixButton.Content = "Fixing...";

        MainWindow.Instance.ShowNotification("Scanning & repairing system files (SFC/DISM)...");

        bool success = await _troubleshootService.RepairSystemFilesAsync();

        if (success)
        {
            SfcFixButton.Content = "Success!";
            MainWindow.Instance.ShowNotification("System files scanned and repaired successfully!");
        }
        else
        {
            SfcFixButton.Content = "Failed";
            MainWindow.Instance.ShowNotification("System repair failed or requires full Admin rights.");
        }

        await Task.Delay(2000);

        SfcProgress.IsActive = false;
        SfcFixButton.IsEnabled = true;
        SfcFixButton.Content = "Run Fix";
    }

    private async void FixGpu_Click(object sender, RoutedEventArgs e)
    {
        GpuFixButton.IsEnabled = false;
        GpuProgress.IsActive = true;
        GpuFixButton.Content = "Fixing...";

        MainWindow.Instance.ShowNotification("Restarting Graphics Display Driver...");

        bool success = await _troubleshootService.RestartGpuDriverAsync();

        if (success)
        {
            GpuFixButton.Content = "Success!";
            MainWindow.Instance.ShowNotification("GPU driver restarted successfully!");
        }
        else
        {
            GpuFixButton.Content = "Failed";
            MainWindow.Instance.ShowNotification("Failed to restart GPU driver.");
        }

        await Task.Delay(2000);

        GpuProgress.IsActive = false;
        GpuFixButton.IsEnabled = true;
        GpuFixButton.Content = "Run Fix";
    }

    private async void FixTemp_Click(object sender, RoutedEventArgs e)
    {
        TempFixButton.IsEnabled = false;
        TempProgress.IsActive = true;
        TempFixButton.Content = "Fixing...";

        MainWindow.Instance.ShowNotification("Purging temporary and junk files...");

        bool success = await _troubleshootService.ClearTempFilesAsync();

        if (success)
        {
            TempFixButton.Content = "Success!";
            MainWindow.Instance.ShowNotification("Temp and Prefetch cache cleared!");
        }
        else
        {
            TempFixButton.Content = "Failed";
            MainWindow.Instance.ShowNotification("Failed to clear temp files.");
        }

        await Task.Delay(2000);

        TempProgress.IsActive = false;
        TempFixButton.IsEnabled = true;
        TempFixButton.Content = "Run Fix";
    }


}