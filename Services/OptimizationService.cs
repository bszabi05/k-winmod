using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using MyTool.Engines;

namespace MyTool.Services
{
    public class OptimizationService
    {
        //Loading engines
        private readonly RegistryEngine _registryEngine = new RegistryEngine();
        private readonly MemoryEngine _memoryEngine = new MemoryEngine();
        private readonly DiskEngine _diskEngine = new DiskEngine();
        private readonly NetworkEngine _networkEngine = new NetworkEngine();

        // Path for registry keys
        private const string TelemetryPath = @"Software\Policies\Microsoft\Windows\DataCollection";
        private const string TelemetryValueName = "AllowTelemetry";

        private const string TipsPath = @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager";
        private const string SoftLandingValue = "SubscribedContent-338389Enabled";
        private const string FeedbackValue = "SubscribedContent-338387Enabled";

        // Telemetry functions
        public bool IsTelemetryEnabled()
        {
            // Enabled by default
            return _registryEngine.GetDWordValue(TelemetryPath, TelemetryValueName, 1) == 1;
        }

        public async Task<bool> SetTelemetryStateAsync(bool enable)
        {
            return await Task.Run(() =>
            {
                int rawValue = enable ? 1 : 0;
                return _registryEngine.SetDWordValue(TelemetryPath, TelemetryValueName, rawValue);
            });
        }

        // Windows ads, sugestions functons
        public bool IsTipsEnabled()
        {
            // Enabled by default
            return _registryEngine.GetDWordValue(TipsPath, SoftLandingValue, 1) == 1;
        }

        public async Task<bool> SetTipsStateAsync(bool enable)
        {
            return await Task.Run(() =>
            {
                int rawValue = enable ? 1 : 0;
                bool success1 = _registryEngine.SetDWordValue(TipsPath, SoftLandingValue, rawValue);
                bool success2 = _registryEngine.SetDWordValue(TipsPath, FeedbackValue, rawValue);
                return success1 && success2;
            });
        }

        // Memory optimization functions
        public async Task<int> OptimizeMemoryAsync()
        {
            return await _memoryEngine.OptimizeMemoryAsync();
        }

        // Cleaning DNS cache
        public async Task<bool> FlushDNSCacheAsync()
        {
            return await _networkEngine.FlushDNSCacheAsync();
        }

        // Clearing temp files
        public async Task<long> CleanTemporaryFilesAsync()
        {
            return await _diskEngine.CleanTemporaryFilesAsync();
        }
    }
}