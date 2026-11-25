using System;
using System.Threading.Tasks;
using MyTool.Engines;

namespace MyTool.Services
{
    public class TroubleshootService
    {
        private readonly HardwareEngine _hardwareEngine = new HardwareEngine();
        private readonly NetworkEngine _networkEngine = new NetworkEngine();
        private readonly SystemEngine _systemEngine = new SystemEngine();


        public async Task<bool> RestartTouchpadDriverAsync()
        {

            return await _hardwareEngine.RestartTouchpadDriverAsync();
        }

        // check I2C HID status
        public async Task<bool> CheckTouchpadErrorStatusAsync()
        {
            return await _hardwareEngine.HasDeviceErrorAsync("I2C HID");
        }

        public async Task<bool> RestartAudioServicesAsync()
        {
            return await _hardwareEngine.RestartAudioServicesAsync();
        }

        public async Task<bool> RestartExplorerAsync()
        {
            return await _systemEngine.RestartExplorerAsync();
        }

        public async Task<bool> ResetNetworkStackAsync()
        {
            return await _networkEngine.ResetNetworkStackAsync();
        }
        public async Task<bool> ClearPrintSpoolerAsync()
        {
            return await _hardwareEngine.ClearPrintSpoolerAsync();
        }

        public async Task<bool> RestartBluetoothAsync()
        {
            return await _hardwareEngine.RestartBluetoothAsync();
        }

        public async Task<bool> ClearUpdateCacheAsync()
        {
            return await _systemEngine.ClearUpdateCacheAsync();
        }

        public async Task<bool> RepairSystemFilesAsync()
        {
            return await _systemEngine.RepairSystemFilesAsync();
        }

        public async Task<bool> RestartGpuDriverAsync()
        {
            return await _hardwareEngine.RestartGpuDriverAsync();
        }

        public async Task<bool> ClearTempFilesAsync()
        {
            return await _systemEngine.ClearTempFilesAsync();
        }

        public async Task<(bool Success, string Message)> CreateRestorePointAsync(string description = "K-WinMod Restore Point")
        {
            return await _systemEngine.CreateRestorePointAsync(description);
        }

        public bool OpenSystemRestoreWizard()
        {
            return _systemEngine.OpenSystemRestoreWizard();
        }

        public async Task<string?> GetLatestRestorePointInfoAsync()
        {
            return await _systemEngine.GetLatestRestorePointInfoAsync();
        }
    }
}