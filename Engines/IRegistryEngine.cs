using Microsoft.Win32;

namespace MyTool.Engines
{
    public interface IRegistryEngine
    {
        int GetDWordValue(string subKeyPath, string valueName, int defaultValue, RegistryHive hive = RegistryHive.CurrentUser);
        bool SetDWordValue(string subKeyPath, string valueName, int value, RegistryHive hive = RegistryHive.CurrentUser);
        bool DeleteValue(string subKeyPath, string valueName, RegistryHive hive = RegistryHive.CurrentUser);
    }
}
