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
    }
}