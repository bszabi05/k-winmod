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
                long bytesFreed = 0;


                string userTemp = Path.GetTempPath();
                bytesFreed += ClearFolderDirectory(userTemp);

                string sysTemp = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Temp");
                if (Directory.Exists(sysTemp))
                {
                    bytesFreed += ClearFolderDirectory(sysTemp);
                }

                return bytesFreed;
            });
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

            // Delete subfolders revursively
            foreach (DirectoryInfo dir in di.EnumerateDirectories())
            {
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
