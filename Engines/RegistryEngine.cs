using Microsoft.Win32;
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
                System.Diagnostics.Debug.WriteLine($"Registry Read Error ({valueName}): {ex.Message}");
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
                System.Diagnostics.Debug.WriteLine($"Registry Write Error ({valueName}): {ex.Message}");
            }
            return false;
        }
    }
}
