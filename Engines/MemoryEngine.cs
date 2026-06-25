using MyTool.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MyTool.Engines
{
    public class MemoryEngine
    {
        [DllImport("psapi.dll", SetLastError = true)]
        private static extern bool EmptyWorkingSet(IntPtr hProcess);

        /// <summary>
        /// Iterate in active processess and optimize the workingset
        /// </summary>
        public async Task<int> OptimizeMemoryAsync()
        {
            return await Task.Run(() =>
            {
                LoggerService.LogInfo("Memory optimization process started.");
                int optimizedCount = 0;
                Process[] processes = Process.GetProcesses();

                foreach (Process process in processes)
                {
                    try
                    {
                        // Skipping self processes and system processes
                        if (process.Id == 0 || process.Id == 4 || process.ProcessName == "Idle")
                            continue;

                        bool success = EmptyWorkingSet(process.Handle);
                        if (success)
                        {
                            optimizedCount++;
                        }
                    }
                    catch
                    {
                    }
                    finally
                    {
                        process.Dispose();
                    }
                }
                LoggerService.LogInfo($"Memory optimization finished. Trimmed working sets for {optimizedCount} processes.");
                return optimizedCount;
            });
        }
    }
}
