using MyTool.Services;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MyTool.Engines
{
    public class SystemEngine
    {
        private readonly IRegistryEngine _registryEngine;

        public SystemEngine(IRegistryEngine? registryEngine = null)
        {
            _registryEngine = registryEngine ?? new RegistryEngine();
        }


        /// <summary>
        /// Restart Windows Explorer process to fix frozen taskbar or desktop UI
        /// </summary>
        public async Task<bool> RestartExplorerAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    LoggerService.LogInfo("Starting Windows Explorer restart process...");

                    string psCommand = "Stop-Process -Name explorer -Force; Start-Process explorer";

                    ProcessStartInfo psInfo = new ProcessStartInfo
                    {
                        FileName = "powershell.exe",
                        Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{psCommand}\"",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    using (Process psProcess = Process.Start(psInfo))
                    {
                        if (psProcess != null)
                        {
                            psProcess.WaitForExit();
                            LoggerService.LogInfo("Windows Explorer process successfully restarted.");
                            return true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    LoggerService.LogError("Critical error during Windows Explorer restart", ex);
                    return false;
                }

                return false;
            });
        }

        /// <summary>
        /// Stops Windows Update service, clears the SoftwareDistribution download cache, and restarts the service.
        /// </summary>
        public async Task<bool> ClearUpdateCacheAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    LoggerService.LogInfo("Starting Windows Update cache cleanup...");

                    string psCommand = "Stop-Service -Name wuauserv -Force; " +
                                       "Remove-Item -Path \"$env:SystemRoot\\SoftwareDistribution\\Download\\*\" -Force -Recurse -ErrorAction SilentlyContinue; " +
                                       "Start-Service -Name wuauserv";

                    ProcessStartInfo psInfo = new ProcessStartInfo
                    {
                        FileName = "powershell.exe",
                        Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{psCommand}\"",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    using (Process psProcess = Process.Start(psInfo))
                    {
                        if (psProcess != null)
                        {
                            psProcess.WaitForExit();
                            bool success = psProcess.ExitCode == 0;
                            if (success)
                                LoggerService.LogInfo("Windows Update cache successfully cleared.");
                            else
                                LoggerService.LogError($"Windows Update cache cleanup failed with exit code: {psProcess.ExitCode}");

                            return success;
                        }
                    }
                }
                catch (Exception ex)
                {
                    LoggerService.LogError("Critical error during Windows Update cache reset", ex);
                    return false;
                }

                return false;
            });
        }

        /// <summary>
        /// Triggers a light SFC scan and DISM health check to repair corrupted system files.
        /// </summary>
        public async Task<bool> RepairSystemFilesAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    LoggerService.LogInfo("Starting System File Checker (SFC / DISM)...");

                    string psCommand = "Dism /Online /Cleanup-Image /RestoreHealth; sfc /scannow";

                    ProcessStartInfo psInfo = new ProcessStartInfo
                    {
                        FileName = "powershell.exe",
                        Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{psCommand}\"",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    using (Process psProcess = Process.Start(psInfo))
                    {
                        if (psProcess != null)
                        {
                            psProcess.WaitForExit();
                            bool success = psProcess.ExitCode == 0;
                            if (success)
                                LoggerService.LogInfo("System file repair completed successfully.");
                            else
                                LoggerService.LogError($"System file repair finished with code: {psProcess.ExitCode}");

                            return success;
                        }
                    }
                }
                catch (Exception ex)
                {
                    LoggerService.LogError("Critical error during System File Repair", ex);
                    return false;
                }

                return false;
            });
        }

        /// <summary>
        /// Purges user and system Temp folders along with Prefetch cache.
        /// </summary>
        public async Task<bool> ClearTempFilesAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    LoggerService.LogInfo("Starting System Temp & Junk files cleanup...");

                    string psCommand = "Remove-Item -Path \"$env:TEMP\\*\" -Force -Recurse -ErrorAction SilentlyContinue; " +
                                       "Remove-Item -Path \"$env:SystemRoot\\Temp\\*\" -Force -Recurse -ErrorAction SilentlyContinue; " +
                                       "Remove-Item -Path \"$env:SystemRoot\\Prefetch\\*\" -Force -Recurse -ErrorAction SilentlyContinue";

                    ProcessStartInfo psInfo = new ProcessStartInfo
                    {
                        FileName = "powershell.exe",
                        Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{psCommand}\"",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    using (Process psProcess = Process.Start(psInfo))
                    {
                        if (psProcess != null)
                        {
                            psProcess.WaitForExit();
                            LoggerService.LogInfo("Temp and junk files cleanup completed.");
                            return true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    LoggerService.LogError("Critical error during Temp files cleanup", ex);
                    return false;
                }

                return false;
            });
        }

        /// <summary>
        /// Checks whether the Windows Game Mode feature is currently enabled.
        /// </summary>
        public bool IsGameModeEnabled(string keyPath, string valueName)
        {
            try
            {
                return _registryEngine.GetDWordValue(keyPath, valueName, 1) == 1;
            }
            catch (Exception ex)
            {
                LoggerService.LogError("Failed to read Game Mode registry key status", ex);
                return true;
            }
        }

        /// <summary>
        /// Enables or disables Windows Game Mode via Registry modification.
        /// </summary>
        public async Task<bool> SetGameModeStateAsync(string keyPath, string valueName, bool enable)
        {
            return await Task.Run(() =>
            {
                try
                {
                    LoggerService.LogInfo($"Setting Windows Game Mode state to: {enable}...");
                    int val = enable ? 1 : 0;
                    bool success = _registryEngine.SetDWordValue(keyPath, valueName, val);

                    if (success)
                        LoggerService.LogInfo($"Windows Game Mode state updated successfully to {enable}.");
                    else
                        LoggerService.LogError("Failed to write Game Mode registry key.");

                    return success;
                }
                catch (Exception ex)
                {
                    LoggerService.LogError("Critical error during Game Mode state update", ex);
                    return false;
                }
            });
        }

        /// <summary>
        /// Checks whether web search suggestions (Bing Search) in Start Menu are disabled.
        /// </summary>
        public bool IsBingSearchDisabled(string keyPath, string valueName)
        {
            try
            {
                int hkcu = _registryEngine.GetDWordValue(keyPath, valueName, -1, Microsoft.Win32.RegistryHive.CurrentUser);
                if (hkcu != -1) return hkcu == 1;

                int hklm = _registryEngine.GetDWordValue(keyPath, valueName, 0, Microsoft.Win32.RegistryHive.LocalMachine);
                return hklm == 1;
            }
            catch (Exception ex)
            {
                LoggerService.LogError("Failed to read Bing Search policy registry key status", ex);
                return false;
            }
        }

        /// <summary>
        /// Enables or disables Bing web search results inside the Windows Start Menu.
        /// </summary>
        public async Task<bool> SetBingSearchDisabledStateAsync(string keyPath, string valueName, bool disable)
        {
            return await Task.Run(() =>
            {
                try
                {
                    LoggerService.LogInfo($"Setting Start Menu Bing Search disable policy to: {disable}...");
                    int val = disable ? 1 : 0;
                    bool s1 = _registryEngine.SetDWordValue(keyPath, valueName, val, Microsoft.Win32.RegistryHive.CurrentUser);
                    bool s2 = _registryEngine.SetDWordValue(keyPath, valueName, val, Microsoft.Win32.RegistryHive.LocalMachine);
                    bool success = s1 || s2;

                    if (success)
                        LoggerService.LogInfo($"Start Menu Bing Search policy updated successfully to {disable}.");
                    else
                        LoggerService.LogError("Failed to write Bing Search policy registry key.");

                    return success;
                }
                catch (Exception ex)
                {
                    LoggerService.LogError("Critical error during Start Menu Bing Search state update", ex);
                    return false;
                }
            });
        }

        /// <summary>
        /// Checks if the hidden Windows Ultimate Performance power scheme is currently active.
        /// </summary>
        public bool IsUltimatePerformanceActive()
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "powercfg",
                    Arguments = "/getactivescheme",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process proc = Process.Start(psi))
                {
                    if (proc != null)
                    {
                        string output = proc.StandardOutput.ReadToEnd();
                        proc.WaitForExit();
                        return output.Contains("e9a42b02-d5df-448d-aa00-03f14749eb61");
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerService.LogError("Failed to query active Windows power scheme", ex);
            }

            return false;
        }

        /// <summary>
        /// Unlocks and activates the Ultimate Performance power scheme or restores Balanced profile.
        /// </summary>
        public async Task<bool> SetUltimatePerformanceStateAsync(bool enable)
        {
            return await Task.Run(() =>
            {
                try
                {
                    LoggerService.LogInfo($"Updating power plan scheme state (Ultimate Performance = {enable})...");

                    string psCommand = enable
                        ? "powercfg -duplicatescheme e9a42b02-d5df-448d-aa00-03f14749eb61; powercfg /s e9a42b02-d5df-448d-aa00-03f14749eb61"
                        : "powercfg /s 381b4222-f694-41f0-9685-ff5bb260df2e";

                    ProcessStartInfo psInfo = new ProcessStartInfo
                    {
                        FileName = "powershell.exe",
                        Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{psCommand}\"",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    using (Process psProcess = Process.Start(psInfo))
                    {
                        if (psProcess != null)
                        {
                            psProcess.WaitForExit();
                            bool success = psProcess.ExitCode == 0;

                            if (success)
                                LoggerService.LogInfo($"Power scheme modified successfully.");
                            else
                                LoggerService.LogError($"Power scheme update failed with exit code: {psProcess.ExitCode}");

                            return success;
                        }
                    }
                }
                catch (Exception ex)
                {
                    LoggerService.LogError("Critical error during Ultimate Performance power scheme modification", ex);
                    return false;
                }

                return false;
            });
        }
    }
}