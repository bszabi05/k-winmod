using Microsoft.Win32;

namespace MyTool.Models
{
    public class StartupItem
    {
        public string Name { get; set; } = string.Empty;
        public string Command { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public bool IsEnabled { get; set; } = true;
        public RegistryHive Hive { get; set; } = RegistryHive.CurrentUser;
        public string RegistryPath { get; set; } = string.Empty;
        public string StartupApprovedPath { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public bool IsFolderItem { get; set; }
    }
}
