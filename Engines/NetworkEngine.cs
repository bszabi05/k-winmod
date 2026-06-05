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
                            return p.ExitCode == 0; 
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"DNS Flush Error: {ex.Message}");
                }
                return false;
            });
        }
    }
}
