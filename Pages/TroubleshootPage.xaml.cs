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

   
}