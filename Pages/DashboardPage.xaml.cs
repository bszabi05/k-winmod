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
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using System.Diagnostics;


namespace MyTool
{
    public partial class DashboardPage : Page
    {
        private DispatcherTimer timer;
        private PerformanceCounter diskCounter;
        private PerformanceCounter networkCounter;
        private PerformanceCounter cpuCounter;

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetPhysicallyInstalledSystemMemory(out long TotalMemoryInKilobytes);

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

            cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            diskCounter = new PerformanceCounter("PhysicalDisk", "% Disk Time", "_Total");

            PerformanceCounterCategory category = new PerformanceCounterCategory("Network Interface");
            string[] instanceNames = category.GetInstanceNames();

            if (instanceNames.Length > 0)
            {
                string firstInterface = instanceNames[0];
                networkCounter = new PerformanceCounter("Network Interface", "Bytes Received/sec", firstInterface);
            }

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();

            UpdateMetrics();
            LoadSystemInfo();
            LoadStorageDrives();

        }

        private void Timer_Tick(object sender, object e)
        {
            UpdateMetrics();
        }

        private void UpdateMetrics()
        {
            float cpuUsage = cpuCounter.NextValue();
            CpuRing.Value = cpuUsage;
            CpuText.Text = $"{Math.Round(cpuUsage)}%";

            MEMORYSTATUSEX memStatus = new MEMORYSTATUSEX();
            if (GlobalMemoryStatusEx(memStatus))
            {
                uint ramUsagePercentage = memStatus.dwMemoryLoad;
                RamRing.Value = ramUsagePercentage;
                RamText.Text = $"{ramUsagePercentage}%";
            }

            float diskUsage = diskCounter.NextValue();
            DiskRing.Value = diskUsage > 100 ? 100 : diskUsage;
            DiskText.Text = $"{Math.Round(diskUsage)}%";

            if (networkCounter != null)
            {
                float networkBytes = networkCounter.NextValue();
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

        private void LoadSystemInfo()
        {
            OsText.Text = $"OS: {Environment.OSVersion.VersionString}";
            UserText.Text = $"User: {Environment.UserName} @ {Environment.MachineName}";
            ArchitectureText.Text = $"Architecture: {Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE")} ({RuntimeInformation.ProcessArchitecture})";
        }

        private void LoadStorageDrives()
        {
            StoragePanel.Children.Clear();

            foreach (var drive in System.IO.DriveInfo.GetDrives())
            {
                if (drive.IsReady && drive.DriveType == System.IO.DriveType.Fixed)
                {
                    long totalSpace = drive.TotalSize;
                    long freeSpace = drive.AvailableFreeSpace;
                    long usedSpace = totalSpace - freeSpace;

                    double usedPercentage = ((double)usedSpace / totalSpace) * 100;
                    double totalGb = Math.Round((double)totalSpace / (1024 * 1024 * 1024), 1);
                    double freeGb = Math.Round((double)freeSpace / (1024 * 1024 * 1024), 1);

                    StackPanel driveRow = new StackPanel { Spacing = 5, Margin = new Thickness(0, 0, 0, 5) };

                    TextBlock label = new TextBlock
                    {
                        Text = $"{drive.Name} ({drive.VolumeLabel}) - {freeGb} GB free of {totalGb} GB",
                        Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(240, 255, 255, 100)),
                        FontSize = 13
                    };

                    ProgressBar progressBar = new ProgressBar
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
    }
}