using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MyTool.Models;
using MyTool.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MyTool.Pages
{
    public sealed partial class StartupPage : Page
    {
        private readonly StartupService _startupService = new StartupService();
        private ObservableCollection<StartupItem> _items = new ObservableCollection<StartupItem>();
        private bool _isLoading = false;

        public StartupPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            await LoadStartupItemsAsync();
        }

        private async Task LoadStartupItemsAsync()
        {
            _isLoading = true;
            LoadingRing.IsActive = true;
            RefreshButton.IsEnabled = false;
            StartupSummaryText.Text = "Scanning Windows startup entries...";

            try
            {
                var list = await _startupService.GetStartupItemsAsync();
                _items = new ObservableCollection<StartupItem>(list);
                StartupItemsList.ItemsSource = _items;
                UpdateSummaryText();
            }
            catch (Exception ex)
            {
                LoggerService.LogError("Error loading startup items in UI", ex);
                StartupSummaryText.Text = "Failed to load startup entries.";
            }
            finally
            {
                LoadingRing.IsActive = false;
                RefreshButton.IsEnabled = true;
                _isLoading = false;
            }
        }

        private void UpdateSummaryText()
        {
            int total = _items.Count;
            int enabled = _items.Count(i => i.IsEnabled);
            int disabled = total - enabled;
            StartupSummaryText.Text = $"{total} startup entries found ({enabled} enabled, {disabled} disabled)";
        }

        private async void Refresh_Click(object sender, RoutedEventArgs e)
        {
            await LoadStartupItemsAsync();
            MainWindow.Instance.ShowNotification("Startup list refreshed.");
        }

        private async void ItemToggle_Toggled(object sender, RoutedEventArgs e)
        {
            if (_isLoading || sender is not ToggleSwitch toggle || !toggle.IsLoaded || toggle.Tag is not StartupItem item)
                return;

            bool shouldEnable = toggle.IsOn;
            if (shouldEnable == item.IsEnabled)
                return;

            toggle.IsEnabled = false;
            bool success = await _startupService.SetStartupItemEnabledAsync(item, shouldEnable);

            if (success)
            {
                UpdateSummaryText();
                MainWindow.Instance.ShowNotification(shouldEnable
                    ? $"'{item.Name}' enabled at Windows startup."
                    : $"'{item.Name}' disabled from Windows startup.");
            }
            else
            {
                toggle.Toggled -= ItemToggle_Toggled;
                toggle.IsOn = !shouldEnable;
                toggle.Toggled += ItemToggle_Toggled;
                MainWindow.Instance.ShowNotification($"Failed to modify '{item.Name}'. Ensure Administrator privileges.");
            }

            toggle.IsEnabled = true;
        }

        private async void DeleteItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn || btn.Tag is not StartupItem item)
                return;

            var dialog = new ContentDialog
            {
                Title = "Remove Startup Entry",
                Content = $"Are you sure you want to permanently remove '{item.Name}' from Windows startup?\n\nNote: If you only want to stop it from launching temporarily, use the toggle switch instead.",
                PrimaryButtonText = "Remove",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();
            if (result != ContentDialogResult.Primary)
                return;

            bool success = await _startupService.DeleteStartupItemAsync(item);
            if (success)
            {
                _items.Remove(item);
                UpdateSummaryText();
                MainWindow.Instance.ShowNotification($"'{item.Name}' permanently removed from startup.");
            }
            else
            {
                MainWindow.Instance.ShowNotification($"Failed to remove '{item.Name}'. Administrator rights may be required.");
            }
        }
    }
}
