using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using WinRT.Interop;

namespace MyTool
{
    public partial class MainWindow : Window
    {
        private bool isMenuOpen = true;
        public static MainWindow Instance { get; private set; }

        private System.Threading.CancellationTokenSource _notificationCts;

        public MainWindow()
        {
            Instance = this;

            try
            {
                Microsoft.Windows.ApplicationModel.DynamicDependency.Bootstrap.Initialize(0x00010003); // some sdk version need this dynamic init
            }
            catch (Exception)
            {
            }
            this.InitializeComponent();
            ContentFrame.Navigate(typeof(DashboardPage));
            IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            Microsoft.UI.WindowId windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
            Microsoft.UI.Windowing.AppWindow appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);

            if (appWindow != null)
            {
                var windowSize = new Windows.Graphics.SizeInt32(1400, 900); 
                appWindow.Resize(windowSize); // resizing
                var displayArea = Microsoft.UI.Windowing.DisplayArea.GetFromWindowId(windowId, Microsoft.UI.Windowing.DisplayAreaFallback.Primary); 

                if (displayArea != null) // centering
                {
                    int centerX = (displayArea.WorkArea.Width - windowSize.Width) / 2;
                    int centerY = (displayArea.WorkArea.Height - windowSize.Height) / 2;

                    appWindow.Move(new Windows.Graphics.PointInt32(centerX, centerY));
                }
   
            }

        }

        private void MenuButton_Click(object sender, RoutedEventArgs e) // handler for menu buttons
        {
            if (sender is Button btn && btn.Tag != null)
            {
                string pageName = btn.Tag.ToString();
                Type pageType = Type.GetType($"MyTool.{pageName}");

                if (pageType != null)
                {
                    ContentFrame.Navigate(pageType);
                }
            }
        }

        private void ToggleMenu_Click(object sender, RoutedEventArgs e) // hamburger
        {
            if (isMenuOpen)
            {
                MenuColumn.Width = new GridLength(64);

                BrandPanel.Visibility = Visibility.Collapsed;
                FooterText.Visibility = Visibility.Collapsed;

                isMenuOpen = false;
            }
            else
            {
                MenuColumn.Width = new GridLength(240);

                BrandPanel.Visibility = Visibility.Visible;
                FooterText.Visibility = Visibility.Visible;

                isMenuOpen = true;
            }
        }

        public async void ShowNotification(string message)
        {
            _notificationCts?.Cancel();
            _notificationCts = new System.Threading.CancellationTokenSource();
            var token = _notificationCts.Token;

            double windowWidth = this.Content.ActualSize.X;
            if (windowWidth <= 0) windowWidth = 1400;
            double leftPosition = (windowWidth - NotificationToast.Width) / 2;
            Canvas.SetLeft(NotificationToast, leftPosition);

            StatusText.Text = message;
            NotificationToast.Visibility = Visibility.Visible;

            try
            {
                for (int i = -80; i <= 20; i += 10)
                {
                    Canvas.SetTop(NotificationToast, i);
                    await Task.Delay(10);
                }
                Canvas.SetTop(NotificationToast, 20);

                await Task.Delay(3000, token);

                for (int i = 20; i >= -80; i -= 10)
                {
                    Canvas.SetTop(NotificationToast, i);
                    await Task.Delay(10);
                }

                NotificationToast.Visibility = Visibility.Collapsed;
            }
            catch (TaskCanceledException) { }
        }
    }
}