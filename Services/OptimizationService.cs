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
        // Loading engines
        private readonly IRegistryEngine _registryEngine;
        private readonly MemoryEngine _memoryEngine;
        private readonly DiskEngine _diskEngine;
        private readonly NetworkEngine _networkEngine;
        private readonly SystemEngine _systemEngine;

        public OptimizationService(
            IRegistryEngine? registryEngine = null,
            MemoryEngine? memoryEngine = null,
            DiskEngine? diskEngine = null,
            NetworkEngine? networkEngine = null,
            SystemEngine? systemEngine = null)
        {
            _registryEngine = registryEngine ?? new RegistryEngine();
            _memoryEngine = memoryEngine ?? new MemoryEngine();
            _diskEngine = diskEngine ?? new DiskEngine();
            _networkEngine = networkEngine ?? new NetworkEngine();
            _systemEngine = systemEngine ?? new SystemEngine(_registryEngine);
        }

        // Path for registry keys
        private const string TelemetryPath = @"Software\Policies\Microsoft\Windows\DataCollection";
        private const string TelemetryValueName = "AllowTelemetry";

        private const string TipsPath = @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager";
        private const string SoftLandingValue = "SubscribedContent-338389Enabled";
        private const string FeedbackValue = "SubscribedContent-338387Enabled";

        private const string GameModePath = @"Software\Microsoft\GameBar";
        private const string GameModeValue = "AllowAutoGameMode";

        private const string BingSearchPath = @"Software\Policies\Microsoft\Windows\Explorer";
        private const string BingSearchValue = "DisableSearchBoxSuggestions";

        // Telemetry functions
        public bool IsTelemetryEnabled()
        {
            // Windows Telemetry policy is primarily enforced in HKLM
            int hklmVal = _registryEngine.GetDWordValue(TelemetryPath, TelemetryValueName, -1, Microsoft.Win32.RegistryHive.LocalMachine);
            if (hklmVal != -1)
            {
                return hklmVal != 0;
            }
            return _registryEngine.GetDWordValue(TelemetryPath, TelemetryValueName, 1, Microsoft.Win32.RegistryHive.CurrentUser) != 0;
        }

        public async Task<bool> SetTelemetryStateAsync(bool enable)
        {
            return await Task.Run(() =>
            {
                int rawValue = enable ? 1 : 0;
                bool s1 = _registryEngine.SetDWordValue(TelemetryPath, TelemetryValueName, rawValue, Microsoft.Win32.RegistryHive.LocalMachine);
                bool s2 = _registryEngine.SetDWordValue(TelemetryPath, TelemetryValueName, rawValue, Microsoft.Win32.RegistryHive.CurrentUser);
                return s1 || s2;
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

        public bool IsGameModeEnabled()
        {
            return _systemEngine.IsGameModeEnabled(GameModePath, GameModeValue);
        }

        public async Task<bool> SetGameModeStateAsync(bool enable)
        {
            return await _systemEngine.SetGameModeStateAsync(GameModePath, GameModeValue, enable);
        }

        public bool IsBingSearchDisabled()
        {
            return _systemEngine.IsBingSearchDisabled(BingSearchPath, BingSearchValue);
        }

        public async Task<bool> SetBingSearchDisabledStateAsync(bool disable)
        {
            return await _systemEngine.SetBingSearchDisabledStateAsync(BingSearchPath, BingSearchValue, disable);
        }

        public bool IsUltimatePerformanceActive()
        {
            return _systemEngine.IsUltimatePerformanceActive();
        }

        public async Task<bool> SetUltimatePerformanceStateAsync(bool enable)
        {
            return await _systemEngine.SetUltimatePerformanceStateAsync(enable);
        }

        /// <summary>
        /// Executes a comprehensive one-click optimization:
        /// 1. Trims working sets of active processes to free RAM.
        /// 2. Cleans temporary and junk files from user and system temp folders.
        /// 3. Flushes the DNS resolver cache.
        /// </summary>
        public async Task<(int ProcessesOptimized, long BytesFreed, bool DnsFlushed)> RunSmartBoostAsync()
        {
            LoggerService.LogInfo(" Starting 1-Click Smart Boost sequence ");

            var memTask = OptimizeMemoryAsync();
            var diskTask = CleanTemporaryFilesAsync();
            var dnsTask = FlushDNSCacheAsync();

            await Task.WhenAll(memTask, diskTask, dnsTask);

            int processes = await memTask;
            long bytes = await diskTask;
            bool dns = await dnsTask;

            double mb = Math.Round((double)bytes / (1024 * 1024), 2);
            LoggerService.LogInfo($" Smart Boost completed: Trimmed {processes} processes, Freed {mb} MB, DNS Flushed = {dns} ");

            return (processes, bytes, dns);
        }
    }
}