using System;
using System.Diagnostics;
using System.Threading.Tasks;
using MyTool.Services;

namespace MyTool.Engines
{
    public class HardwareEngine
    {
        /// <summary>
        /// Find and restart I2C HID device
        /// </summary>
        public async Task<bool> RestartTouchpadDriverAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    LoggerService.LogInfo("Starting Touchpad driver reset process...");

                    string psCommand = "Get-PnpDevice | Where-Object {$_.FriendlyName -match 'I2C HID'} | Select-Object -ExpandProperty InstanceId";

                    ProcessStartInfo psInfo = new ProcessStartInfo
                    {
                        FileName = "powershell.exe",
                        Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{psCommand}\"",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    string instanceIdOutput = "";
                    using (Process psProcess = Process.Start(psInfo))
                    {
                        if (psProcess != null)
                        {
                            instanceIdOutput = psProcess.StandardOutput.ReadToEnd().Trim();
                            psProcess.WaitForExit();
                        }
                    }

                    if (string.IsNullOrWhiteSpace(instanceIdOutput))
                    {
                        LoggerService.LogInfo("Touchpad driver reset aborted: No I2C HID devices found on this system.");
                        return false;
                    }

                    string[] ids = instanceIdOutput.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                    string targetId = ids[0].Trim();

                    LoggerService.LogInfo($"Target device ID identified: {targetId}. Executing hardware cycle...");

                    bool disableResult = RunPnPUtil($"/disable-device \"{targetId}\"");
                    if (!disableResult)
                    {
                        LoggerService.LogError($"Failed to disable device: {targetId}");
                        return false;
                    }

                    Task.Delay(1000).Wait();

                    bool enableResult = RunPnPUtil($"/enable-device \"{targetId}\"");

                    if (enableResult)
                    {
                        LoggerService.LogInfo("Touchpad hardware cycle executed successfully.");
                    }
                    else
                    {
                        LoggerService.LogError($"Failed to re-enable device: {targetId}");
                    }

                    return enableResult;
                }
                catch (Exception ex)
                {
                    LoggerService.LogError("Critical error during Touchpad driver reset", ex);
                    return false;
                }
            });
        }

        /// <summary>
        /// Helper methid for PnPUtil
        /// </summary>
        private bool RunPnPUtil(string arguments)
        {
            try
            {
                ProcessStartInfo pnpInfo = new ProcessStartInfo
                {
                    FileName = "pnputil.exe",
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process pnpProcess = Process.Start(pnpInfo))
                {
                    if (pnpProcess != null)
                    {
                        string output = pnpProcess.StandardOutput.ReadToEnd();
                        pnpProcess.WaitForExit();

                        return output.Contains("successfully") || pnpProcess.ExitCode == 0;
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerService.LogError($"PnPUtil execution failed with arguments: {arguments}", ex);
                return false;
            }
            return false;
        }

        /// <summary>
        /// check device error status by name
        /// </summary>
        public async Task<bool> HasDeviceErrorAsync(string deviceNameMatch)
        {
            return await Task.Run(() =>
            {
                try
                {
                    LoggerService.LogInfo($"Checking error status for hardware matching: '{deviceNameMatch}'...");

                    // get ConfigManagerErrorCode by device 
                    string psCommand = $"Get-PnpDevice | Where-Object {{$_.FriendlyName -match '{deviceNameMatch}'}} | Select-Object -ExpandProperty ConfigManagerErrorCode";

                    ProcessStartInfo psInfo = new ProcessStartInfo
                    {
                        FileName = "powershell.exe",
                        Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{psCommand}\"",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    string output = "";
                    using (Process psProcess = Process.Start(psInfo))
                    {
                        if (psProcess != null)
                        {
                            output = psProcess.StandardOutput.ReadToEnd().Trim();
                            psProcess.WaitForExit();
                        }
                    }

                    if (string.IsNullOrWhiteSpace(output))
                    {
                        LoggerService.LogInfo($"No active hardware found matching '{deviceNameMatch}' during error check.");
                        return false;
                    }

                    string[] lines = output.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                    if (lines.Length > 0 && int.TryParse(lines[0].Trim(), out int errorCode))
                    {
                        LoggerService.LogInfo($"Hardware status checked for '{deviceNameMatch}'. Windows ConfigManagerErrorCode: {errorCode}");

                        return errorCode != 0;
                    }
                }
                catch (Exception ex)
                {
                    LoggerService.LogError($"Error while checking status for hardware: '{deviceNameMatch}'", ex);
                    return false;
                }
                return false;
            });
        }

        /// <summary>
        /// Restarts Windows Audio Services (Audiosrv and AudioEndpointBuilder)
        /// </summary>
        public async Task<bool> RestartAudioServicesAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    LoggerService.LogInfo("Starting Audio Services reset process...");

                    // PowerShell command to force restart Windows Audio services
                    string psCommand = "Get-Service Audiosrv, AudioEndpointBuilder | Restart-Service -Force";

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
                            string errorOutput = psProcess.StandardError.ReadToEnd();
                            psProcess.WaitForExit();

                            if (psProcess.ExitCode == 0)
                            {
                                LoggerService.LogInfo("Audio services successfully restarted.");
                                return true;
                            }
                            else
                            {
                                LoggerService.LogError($"Failed to restart Audio services. PowerShell Error: {errorOutput}");
                                return false;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LoggerService.LogError("Critical error during Audio services reset", ex);
                    return false;
                }

                return false;
            });
        }

       

        /// <summary>
        /// Clears stuck print jobs by restarting Spooler service and wiping the spool queue folder.
        /// </summary>
        public async Task<bool> ClearPrintSpoolerAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    LoggerService.LogInfo("Starting Print Spooler cleanup process...");

                    // PowerShell commands to stop spooler, delete files, and start spooler again
                    string psCommand = "Stop-Service -Name Spooler -Force; " +
                                       "Remove-Item -Path \"$env:SystemRoot\\System32\\spool\\PRINTERS\\*\" -Force -Recurse -ErrorAction SilentlyContinue; " +
                                       "Start-Service -Name Spooler";

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
                            if (psProcess.ExitCode == 0)
                            {
                                LoggerService.LogInfo("Print Spooler queue cleared and service restarted successfully.");
                                return true;
                            }
                            else
                            {
                                string error = psProcess.StandardError.ReadToEnd();
                                LoggerService.LogError($"Failed to clear Print Spooler. PowerShell error: {error}");
                                return false;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LoggerService.LogError("Critical error during Print Spooler reset", ex);
                    return false;
                }

                return false;
            });
        }

        /// <summary>
        /// Restarts the Windows Bluetooth Support Service (bthserv).
        /// </summary>
        public async Task<bool> RestartBluetoothAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    LoggerService.LogInfo("Starting Bluetooth service restart...");

                    string psCommand = "Restart-Service -Name 'bthserv' -Force";

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
                                LoggerService.LogInfo("Bluetooth service restarted successfully.");
                            else
                                LoggerService.LogError($"Bluetooth service restart failed with exit code: {psProcess.ExitCode}");

                            return success;
                        }
                    }
                }
                catch (Exception ex)
                {
                    LoggerService.LogError("Critical error during Bluetooth service restart", ex);
                    return false;
                }

                return false;
            });
        }

        

       

        /// <summary>
        /// Restarts display driver components to fix screen flickering or GPU hangs.
        /// </summary>
        public async Task<bool> RestartGpuDriverAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    LoggerService.LogInfo("Restarting GPU display driver subsystem...");

                    // PowerShell script to restart active PnP display adapters
                    string psCommand = "Get-PnpDevice -Class 'Display' -Status 'OK' | Disable-PnpDevice -Confirm:$false; " +
                                       "Start-Sleep -Seconds 1; " +
                                       "Get-PnpDevice -Class 'Display' | Enable-PnpDevice -Confirm:$false";

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
                                LoggerService.LogInfo("GPU Driver restarted successfully.");
                            else
                                LoggerService.LogError($"GPU Driver restart failed with exit code: {psProcess.ExitCode}");

                            return success;
                        }
                    }
                }
                catch (Exception ex)
                {
                    LoggerService.LogError("Critical error during GPU driver restart", ex);
                    return false;
                }

                return false;
            });
        }

    }
}