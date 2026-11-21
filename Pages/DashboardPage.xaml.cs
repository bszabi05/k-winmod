using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MyTool.Services;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace MyTool
{
    public partial class DashboardPage : Page
    {
        private DispatcherTimer? _timer;
        private PerformanceCounter? _diskCounter;
        private PerformanceCounter? _networkCounter;
        private PerformanceCounter? _cpuCounter;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private class MEMORYSTATUSEX
        {
            public uint dwLength;
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;
            public MEMORYSTATUSEX() { this.dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX)); }
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);

        public DashboardPage()
        {
            this.InitializeComponent();

            this.Loaded += DashboardPage_Loaded;
            this.Unloaded += DashboardPage_Unloaded;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            StartMonitoring();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            StopMonitoring();
        }

        private void DashboardPage_Loaded(object sender, RoutedEventArgs e)
        {
            StartMonitoring();
        }

        private void DashboardPage_Unloaded(object sender, RoutedEventArgs e)
        {
            StopMonitoring();
        }

        private void StartMonitoring()
        {
            try
            {
                if (_cpuCounter == null)
                    _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");

                if (_diskCounter == null)
                    _diskCounter = new PerformanceCounter("PhysicalDisk", "% Disk Time", "_Total");

                if (_networkCounter == null)
                {
                    try
                    {
                        var category = new PerformanceCounterCategory("Network Interface");
                        string[] instanceNames = category.GetInstanceNames();
                        if (instanceNames.Length > 0)
                        {
                            string targetInterface = instanceNames.FirstOrDefault(name =>
                                !name.Contains("Loopback", StringComparison.OrdinalIgnoreCase) &&
                                !name.Contains("isatap", StringComparison.OrdinalIgnoreCase)) ?? instanceNames[0];
                            _networkCounter = new PerformanceCounter("Network Interface", "Bytes Received/sec", targetInterface);
                        }
                    }
                    catch (Exception ex)
                    {
                        LoggerService.LogError("Failed to initialize Network PerformanceCounter", ex);
                    }
                }

                if (_timer == null)
                {
                    _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
                    _timer.Tick += Timer_Tick;
                }

                if (!_timer.IsEnabled)
                {
                    _timer.Start();
                }

                UpdateMetrics();
                LoadSystemInfo();
                LoadStorageDrives();
            }
            catch (Exception ex)
            {
                LoggerService.LogError("Error starting Dashboard monitoring", ex);
            }
        }

        private void StopMonitoring()
        {
            try
            {
                if (_timer != null)
                {
                    _timer.Stop();
                    _timer.Tick -= Timer_Tick;
                    _timer = null;
                }

                _cpuCounter?.Dispose();
                _cpuCounter = null;

                _diskCounter?.Dispose();
                _diskCounter = null;

                _networkCounter?.Dispose();
                _networkCounter = null;
            }
            catch (Exception ex)
            {
                LoggerService.LogError("Error stopping Dashboard monitoring", ex);
            }
        }

        private void Timer_Tick(object? sender, object e)
        {
            UpdateMetrics();
        }

        private void UpdateMetrics()
        {
            try
            {
                if (_cpuCounter != null)
                {
                    float cpuUsage = _cpuCounter.NextValue();
                    CpuRing.Value = cpuUsage;
                    CpuText.Text = $"{Math.Round(cpuUsage)}%";
                }

                var memStatus = new MEMORYSTATUSEX();
                if (GlobalMemoryStatusEx(memStatus))
                {
                    uint ramUsagePercentage = memStatus.dwMemoryLoad;
                    RamRing.Value = ramUsagePercentage;
                    RamText.Text = $"{ramUsagePercentage}%";
                }

                if (_diskCounter != null)
                {
                    float diskUsage = _diskCounter.NextValue();
                    DiskRing.Value = diskUsage > 100 ? 100 : diskUsage;
                    DiskText.Text = $"{Math.Round(diskUsage)}%";
                }

                if (_networkCounter != null)
                {
                    float networkBytes = _networkCounter.NextValue();
                    double networkKb = networkBytes / 1024;

                    if (networkKb > 1024)
                    {
                        NetworkText.Text = $"{Math.Round(networkKb / 1024, 1)} MB/s";
                    }
                    else
                    {
                        NetworkText.Text = $"{Math.Round(networkKb)} KB/s";
                    }
                }
                else
                {
                    NetworkText.Text = "N/A";
                }

                TimeSpan uptime = TimeSpan.FromMilliseconds(Environment.TickCount64);
                UptimeText.Text = $"{uptime.Days}d {uptime.Hours}h {uptime.Minutes}m";

                int processCount = Process.GetProcesses().Length;
                ProcessesText.Text = processCount.ToString();
            }
            catch (Exception ex)
            {
                LoggerService.LogError("Error updating Dashboard metrics", ex);
            }
        }

        private void LoadSystemInfo()
        {
            try
            {
                OsText.Text = $"OS: {Environment.OSVersion.VersionString}";
                UserText.Text = $"User: {Environment.UserName} @ {Environment.MachineName}";
                ArchitectureText.Text = $"Architecture: {Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE")} ({RuntimeInformation.ProcessArchitecture})";
            }
            catch (Exception ex)
            {
                LoggerService.LogError("Error loading system info", ex);
            }
        }

        private void LoadStorageDrives()
        {
            try
            {
                StoragePanel.Children.Clear();

                foreach (var drive in DriveInfo.GetDrives())
                {
                    if (drive.IsReady && drive.DriveType == DriveType.Fixed)
                    {
                        long totalSpace = drive.TotalSize;
                        long freeSpace = drive.AvailableFreeSpace;
                        long usedSpace = totalSpace - freeSpace;

                        double usedPercentage = totalSpace > 0 ? ((double)usedSpace / totalSpace) * 100 : 0;
                        double totalGb = Math.Round((double)totalSpace / (1024 * 1024 * 1024), 1);
                        double freeGb = Math.Round((double)freeSpace / (1024 * 1024 * 1024), 1);

                        var driveRow = new StackPanel { Spacing = 5, Margin = new Thickness(0, 0, 0, 5) };

                        var label = new TextBlock
                        {
                            Text = $"{drive.Name} ({drive.VolumeLabel}) - {freeGb} GB free of {totalGb} GB",
                            Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(240, 255, 255, 100)),
                            FontSize = 13
                        };

                        var progressBar = new ProgressBar
                        {
                            Value = usedPercentage,
                            Maximum = 100,
                            Height = 6,
                            CornerRadius = new CornerRadius(3),
                            Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(255, 163, 230, 53)),
                            Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(255, 34, 34, 44))
                        };

                        driveRow.Children.Add(label);
                        driveRow.Children.Add(progressBar);
                        StoragePanel.Children.Add(driveRow);
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerService.LogError("Error loading storage drives", ex);
            }
        }
    }
}
