using MyTool.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTool.Engines
{
    public class NetworkEngine
    {
        /// <summary>
        /// Cleaning DNS cache (ipconfig /flushdns).
        /// </summary>
        public async Task<bool> FlushDNSCacheAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    LoggerService.LogInfo("Initiating DNS cache flush via ipconfig.exe...");
                    ProcessStartInfo psi = new ProcessStartInfo
                    {
                        FileName = "ipconfig.exe",
                        Arguments = "/flushdns",
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        WindowStyle = ProcessWindowStyle.Hidden
                    };

                    using (Process p = Process.Start(psi))
                    {
                        if (p != null)
                        {
                            p.WaitForExit();
                            bool isSuccess = p.ExitCode == 0;

                            if (isSuccess)
                                LoggerService.LogInfo("DNS cache successfully flushed.");
                            else
                                LoggerService.LogError($"ipconfig.exe exited with non-zero code: {p.ExitCode}");

                            return isSuccess;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"DNS Flush Error: {ex.Message}");
                    LoggerService.LogError("Critical error occurred during DNS cache flush", ex);
                }
                return false;
            });
        }
    }
}
