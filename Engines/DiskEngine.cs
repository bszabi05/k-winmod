using MyTool.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTool.Engines
{
    public class DiskEngine
    {
        /// <summary>
        /// Cleaning uer and system temp folders
        /// </summary>
        public async Task<long> CleanTemporaryFilesAsync()
        {
            return await Task.Run(() =>
            {
                LoggerService.LogInfo("Disk cleanup task started.");
                long bytesFreed = 0;


                string userTemp = Path.GetTempPath();
                LoggerService.LogInfo($"Cleaning User Temp directory: {userTemp}");
                bytesFreed += ClearFolderDirectory(userTemp);

                string sysTemp = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Temp");
                if (Directory.Exists(sysTemp))
                {
                    LoggerService.LogInfo($"Cleaning System Temp directory: {sysTemp}");
                    bytesFreed += ClearFolderDirectory(sysTemp);
                }

                double megabytes = BytesToMegaBytes(bytesFreed);
                LoggerService.LogInfo($"Disk cleanup task completed. Total storage reclaimed: {megabytes} MB.");
                return bytesFreed;
            });
        }

        /// <summary>
        /// Converts raw bytes to megabytes with 2 decimal precision.
        /// </summary>
        public static double BytesToMegaBytes(long bytes)
        {
            return Math.Round((double)bytes / (1024 * 1024), 2);
        }

        private long ClearFolderDirectory(string folderPath)
        {
            long freedInFolder = 0;
            DirectoryInfo di = new DirectoryInfo(folderPath);

            // Delete files
            foreach (FileInfo file in di.EnumerateFiles())
            {
                try
                {
                    long fileSize = file.Length;
                    file.Delete();
                    freedInFolder += fileSize;
                }
                catch
                {

                }
            }

            // Delete subfolders recursively
            foreach (DirectoryInfo dir in di.EnumerateDirectories())
            {
                if (dir.Name.Equals("MSBuildTemp", StringComparison.OrdinalIgnoreCase))  //skip just for dev purposes
                    continue;

                try
                {
                    dir.Delete(true);
                }
                catch
                {

                }
            }

            return freedInFolder;
        }
    }
}
