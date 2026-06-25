using Microsoft.Win32;
using MyTool.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTool.Engines
{
    public class RegistryEngine
    {
        /// <summary>
        /// Read a DWORD value from a Registry key
        /// </summary>
        public int GetDWordValue(string subKeyPath, string valueName, int defaultValue)
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(subKeyPath))
                {
                    if (key != null)
                    {
                        object value = key.GetValue(valueName);
                        if (value is int intValue)
                        {
                            return intValue;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerService.LogError($"Registry Read Error at path '{subKeyPath}' for value '{valueName}'", ex);
            }
            return defaultValue;
        }

        /// <summary>
        /// Write a DWORD value to a Registry key
        /// </summary>
        public bool SetDWordValue(string subKeyPath, string valueName, int value)
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(subKeyPath))
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
                LoggerService.LogError($"Registry Write Error at path '{subKeyPath}' for value '{valueName}' to {value}", ex);
            }
            return false;
        }
    }
}
