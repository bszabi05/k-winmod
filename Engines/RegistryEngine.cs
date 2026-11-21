using Microsoft.Win32;
using MyTool.Services;
using System;

namespace MyTool.Engines
{
    public class RegistryEngine : IRegistryEngine
    {
        private static RegistryKey GetBaseKey(RegistryHive hive) => hive switch
        {
            RegistryHive.LocalMachine => Registry.LocalMachine,
            _ => Registry.CurrentUser
        };

        /// <summary>
        /// Read a DWORD value from a Registry key (HKCU or HKLM)
        /// </summary>
        public int GetDWordValue(string subKeyPath, string valueName, int defaultValue, RegistryHive hive = RegistryHive.CurrentUser)
        {
            try
            {
                using (RegistryKey baseKey = GetBaseKey(hive))
                using (RegistryKey? key = baseKey.OpenSubKey(subKeyPath))
                {
                    if (key != null)
                    {
                        object? value = key.GetValue(valueName);
                        if (value is int intValue)
                        {
                            return intValue;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerService.LogError($"Registry Read Error at [{hive}\\{subKeyPath}] for '{valueName}'", ex);
            }
            return defaultValue;
        }

        /// <summary>
        /// Write a DWORD value to a Registry key (HKCU or HKLM)
        /// </summary>
        public bool SetDWordValue(string subKeyPath, string valueName, int value, RegistryHive hive = RegistryHive.CurrentUser)
        {
            try
            {
                using (RegistryKey baseKey = GetBaseKey(hive))
                using (RegistryKey? key = baseKey.CreateSubKey(subKeyPath))
                {
                    if (key != null)
                    {
                        key.SetValue(valueName, value, RegistryValueKind.DWord);
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerService.LogError($"Registry Write Error at [{hive}\\{subKeyPath}] for '{valueName}' to {value}", ex);
            }
            return false;
        }

        /// <summary>
        /// Delete a value from a Registry key (HKCU or HKLM)
        /// </summary>
        public bool DeleteValue(string subKeyPath, string valueName, RegistryHive hive = RegistryHive.CurrentUser)
        {
            try
            {
                using (RegistryKey baseKey = GetBaseKey(hive))
                using (RegistryKey? key = baseKey.OpenSubKey(subKeyPath, writable: true))
                {
                    if (key != null)
                    {
                        key.DeleteValue(valueName, throwOnMissingValue: false);
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerService.LogError($"Registry Delete Error at [{hive}\\{subKeyPath}] for '{valueName}'", ex);
            }
            return false;
        }
    }
}
