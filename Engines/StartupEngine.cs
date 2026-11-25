using Microsoft.Win32;
using MyTool.Models;
using MyTool.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MyTool.Engines
{
    public class StartupEngine
    {
        private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string RunWow64KeyPath = @"Software\WOW6432Node\Microsoft\Windows\CurrentVersion\Run";

        private const string ApprovedRunPath = @"Software\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run";
        private const string ApprovedRun32Path = @"Software\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run32";
        private const string ApprovedFolderPath = @"Software\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\StartupFolder";

        public async Task<List<StartupItem>> GetStartupItemsAsync()
        {
            return await Task.Run(() =>
            {
                var items = new List<StartupItem>();

                try
                {
                    LoggerService.LogInfo("Scanning Windows Startup items...");

                    //  Current User Run
                    ReadRegistryStartupItems(items, RegistryHive.CurrentUser, RunKeyPath, ApprovedRunPath, "Registry (Current User)");

                    //  Local Machine Run (64-bit)
                    ReadRegistryStartupItems(items, RegistryHive.LocalMachine, RunKeyPath, ApprovedRunPath, "Registry (All Users)");

                    //  Local Machine Run (32-bit WOW6432Node)
                    ReadRegistryStartupItems(items, RegistryHive.LocalMachine, RunWow64KeyPath, ApprovedRun32Path, "Registry (All Users x86)");

                    //  User Startup Folder
                    string userStartup = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
                    ReadFolderStartupItems(items, userStartup, RegistryHive.CurrentUser, ApprovedFolderPath, "Startup Folder (User)");

                    //  Common Startup Folder
                    string commonStartup = Environment.GetFolderPath(Environment.SpecialFolder.CommonStartup);
                    ReadFolderStartupItems(items, commonStartup, RegistryHive.LocalMachine, ApprovedFolderPath, "Startup Folder (All Users)");

                    LoggerService.LogInfo($"Startup scan completed. Found {items.Count} items.");
                }
                catch (Exception ex)
                {
                    LoggerService.LogError("Error scanning Windows Startup items", ex);
                }

                return items.OrderBy(i => i.Name, StringComparer.OrdinalIgnoreCase).ToList();
            });
        }

        private static RegistryKey GetBaseKey(RegistryHive hive) => hive switch
        {
            RegistryHive.LocalMachine => Registry.LocalMachine,
            _ => Registry.CurrentUser
        };

        private void ReadRegistryStartupItems(List<StartupItem> items, RegistryHive hive, string runPath, string approvedPath, string sourceLabel)
        {
            try
            {
                using RegistryKey baseKey = GetBaseKey(hive);
                using RegistryKey? runKey = baseKey.OpenSubKey(runPath);
                if (runKey == null) return;

                foreach (string valueName in runKey.GetValueNames())
                {
                    if (string.IsNullOrWhiteSpace(valueName)) continue;

                    string command = runKey.GetValue(valueName)?.ToString() ?? string.Empty;
                    bool isEnabled = IsItemEnabled(baseKey, approvedPath, valueName);

                    items.Add(new StartupItem
                    {
                        Name = valueName,
                        Command = command,
                        Source = sourceLabel,
                        IsEnabled = isEnabled,
                        Hive = hive,
                        RegistryPath = runPath,
                        StartupApprovedPath = approvedPath,
                        IsFolderItem = false
                    });
                }
            }
            catch (Exception ex)
            {
                LoggerService.LogError($"Failed to read registry startup items from [{hive}\\{runPath}]", ex);
            }
        }

        private void ReadFolderStartupItems(List<StartupItem> items, string folderPath, RegistryHive hive, string approvedPath, string sourceLabel)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath)) return;

                using RegistryKey baseKey = GetBaseKey(hive);

                foreach (string filePath in Directory.GetFiles(folderPath))
                {
                    string fileName = Path.GetFileName(filePath);
                    if (fileName.Equals("desktop.ini", StringComparison.OrdinalIgnoreCase)) continue;

                    bool isEnabled = IsItemEnabled(baseKey, approvedPath, fileName);

                    items.Add(new StartupItem
                    {
                        Name = Path.GetFileNameWithoutExtension(fileName),
                        Command = filePath,
                        Source = sourceLabel,
                        IsEnabled = isEnabled,
                        Hive = hive,
                        StartupApprovedPath = approvedPath,
                        FilePath = filePath,
                        IsFolderItem = true
                    });
                }
            }
            catch (Exception ex)
            {
                LoggerService.LogError($"Failed to read startup folder '{folderPath}'", ex);
            }
        }

        private bool IsItemEnabled(RegistryKey baseKey, string approvedPath, string keyValueName)
        {
            try
            {
                using RegistryKey? approvedKey = baseKey.OpenSubKey(approvedPath);
                if (approvedKey != null)
                {
                    object? raw = approvedKey.GetValue(keyValueName);
                    if (raw is byte[] bytes && bytes.Length > 0)
                    {
                        // Even first byte (e.g. 0x02, 0x06) = Enabled; Odd (0x03, 0x07) = Disabled
                        return (bytes[0] & 1) == 0;
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerService.LogError($"Failed to read StartupApproved status for '{keyValueName}'", ex);
            }
            return true;
        }

        public async Task<bool> SetStartupItemEnabledAsync(StartupItem item, bool enable)
        {
            return await Task.Run(() =>
            {
                try
                {
                    string valueName = item.IsFolderItem ? Path.GetFileName(item.FilePath) : item.Name;
                    LoggerService.LogInfo($"Setting startup item '{item.Name}' enabled = {enable} ({item.Hive}\\{item.StartupApprovedPath})...");

                    using RegistryKey baseKey = GetBaseKey(item.Hive);
                    using RegistryKey? approvedKey = baseKey.CreateSubKey(item.StartupApprovedPath);
                    if (approvedKey == null) return false;

                    byte[] data = new byte[12];
                    if (enable)
                    {
                        data[0] = 0x02;
                    }
                    else
                    {
                        data[0] = 0x03;
                        byte[] timeBytes = BitConverter.GetBytes(DateTime.UtcNow.ToFileTimeUtc());
                        Array.Copy(timeBytes, 0, data, 4, 8);
                    }

                    approvedKey.SetValue(valueName, data, RegistryValueKind.Binary);
                    item.IsEnabled = enable;

                    LoggerService.LogInfo($"Startup item '{item.Name}' updated successfully.");
                    return true;
                }
                catch (Exception ex)
                {
                    LoggerService.LogError($"Failed to toggle startup item '{item.Name}'", ex);
                    return false;
                }
            });
        }

        public async Task<bool> DeleteStartupItemAsync(StartupItem item)
        {
            return await Task.Run(() =>
            {
                try
                {
                    LoggerService.LogInfo($"Deleting startup item '{item.Name}'...");

                    using RegistryKey baseKey = GetBaseKey(item.Hive);
                    string approvedValueName = item.IsFolderItem ? Path.GetFileName(item.FilePath) : item.Name;

                    if (item.IsFolderItem)
                    {
                        if (File.Exists(item.FilePath))
                        {
                            File.Delete(item.FilePath);
                        }
                    }
                    else
                    {
                        using RegistryKey? runKey = baseKey.OpenSubKey(item.RegistryPath, writable: true);
                        runKey?.DeleteValue(item.Name, throwOnMissingValue: false);
                    }

                    // Clean up StartupApproved entry if present
                    using RegistryKey? approvedKey = baseKey.OpenSubKey(item.StartupApprovedPath, writable: true);
                    approvedKey?.DeleteValue(approvedValueName, throwOnMissingValue: false);

                    LoggerService.LogInfo($"Startup item '{item.Name}' deleted successfully.");
                    return true;
                }
                catch (Exception ex)
                {
                    LoggerService.LogError($"Failed to delete startup item '{item.Name}'", ex);
                    return false;
                }
            });
        }
    }
}
