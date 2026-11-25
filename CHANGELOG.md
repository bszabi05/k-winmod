# Changelog

Here you can track all the updates and new features added to **K-WinMod** in plain English.

---

## [0.2.0-beta] - 2026-11-25

### 🌟 New Features
* **🚀 Startup Apps Manager:** Added a brand-new `Startup Apps` page to view, enable, disable, or permanently remove programs that run automatically when Windows boots. Uses Windows' native `StartupApproved` binary registry keys (`Run`, `Run32`, `StartupFolder`) so changes stay completely in sync with Task Manager.
* **⚡ 1-Click Smart Boost (Dashboard):** Added a prominent Smart Boost hero banner to the Dashboard that concurrently trims idle process RAM, purges temporary junk files, and flushes DNS cache in one click, immediately refreshing live resource gauges.
* **🛡️ System Restore Checkpoints:** Added a System Protection & Backup card to the Troubleshoot page. Creates Windows System Restore Points on demand (automatically enabling System Protection if disabled and bypassing the 24-hour rate limit) and includes a quick-launch button for the native Windows System Restore wizard (`rstrui.exe`).
* **⚙️ Settings & GitHub Update Checker:** Added a `Settings & Info` page with an automatic background and manual GitHub Release checker (displaying a notification badge when a new version is available), one-click `Logs/` folder opening, and log cleanup controls.

### 🔧 Fixes & Improvements
* **Safe Temp Cleanup:** Updated `DiskEngine` to preserve `MSBuildTemp` inside `%TEMP%`, preventing cleanup runs from interfering with active Visual Studio / MSBuild sessions.
* **Restore Point Date Parsing:** Fixed WMI `CIM_DATETIME` parsing so the latest System Restore checkpoint date and description display reliably.
* **Centralized Versioning:** Added `AppInfo` to keep the UI footer, Settings badge, and Update Checker version synchronized.

---

## [0.1.3-beta] - 2026-11-21

### 🔧 Fixes & Performance Improvements
* **Fixed Registry Scope (HKLM vs HKCU):** Resolved a critical bug where Windows Telemetry and Start Menu Bing Search policies were written exclusively to `CurrentUser`, which caused Group Policy settings to be ignored by Windows background services. They are now properly written to `HKEY_LOCAL_MACHINE` and `HKEY_CURRENT_USER`.
* **Resolved Dashboard Memory Leaks:** Stopped background performance counters (`cpuCounter`, `diskCounter`, `networkCounter`) and `DispatcherTimer` from leaking and consuming background CPU cycles when navigating away from the Dashboard. Resources are now cleanly initialized and disposed during navigation and page unloads.
* **Asynchronous Execution Optimization:** Removed blocking `Task.Delay(1000).Wait()` calls in `HardwareEngine` and replaced them with non-blocking `await Task.Delay(1000)`.
* **Cleaned Project Dependencies:** Eliminated duplicate NuGet `PackageReference` entries and outdated XAML generator tags in `MyTool.csproj` to ensure deterministic builds.
* **Testability & Architecture:** Extracted `IRegistryEngine` and enabled constructor dependency injection across `OptimizationService` and `SystemEngine` for isolated unit testing.
* **UI Version Synchronization:** Corrected the application footer and documentation to reflect `v0.1.3-beta`.

---

## [0.1.2-beta] - 2026-08-04

### 🌟 New Features
* **Expanded Troubleshoot Toolkit:** The Troubleshoot tab has been upgraded into a comprehensive system diagnostic hub with 10 built-in, one-click repair utilities:
  * **Windows Audio Restart:** Instantly restarts audio services to resolve sound crackling, high latency, or missing audio devices.
  * **Taskbar & UI Shell Reset:** Restarts `explorer.exe` to restore frozen taskbars, missing desktop icons, or an unresponsive Start menu.
  * **Full Network Adapter Reset:** Flushes DNS, clears Winsock catalog, resets IP stacks, and renews DHCP configuration to fix dropped Wi-Fi connections.
  * **Print Spooler Cleanup:** Forcefully clears stuck print queues and restarts the Spooler service to fix unresponsive printers.
  * **Bluetooth Stack Reset:** Restarts the Windows Bluetooth Support Service (`bthserv`) to fix stubborn connection loops and missing devices.
  * **Windows Update Cache Purge:** Clears corrupted update downloads (`SoftwareDistribution`) and resets update services to fix stuck downloads.
  * **System File Repair (SFC / DISM):** Scans system file integrity and repairs corrupted Windows images automatically.
  * **GPU Driver Reset:** Restarts active display adapter drivers to recover from screen freezes or graphical glitches.
  * **Junk & Temp Purge:** Wipes temporary files, system caches, and Prefetch data to free up valuable disk space.
* **Ultimate Performance Power Scheme:** Added a toggle switch to instantly unlock and activate the hidden Windows Ultimate Performance power plan, eliminating micro-latencies for maximum system responsiveness.
* **Windows Game Mode Control:** Added a toggle to enable or disable Windows Game Mode on the fly to prioritize hardware resources for gaming.
* **Start Menu Bing Search Toggle:** Allows users to disable online Bing web search suggestions inside the Start Menu for faster local searches and cleaner results.

### 🏗️ Architecture & Code Cleanups
* **Modular Engine Refactor:** Separated backend functionality into three specialized engines (`HardwareEngine`, `SystemEngine`, and `NetworkEngine`) coordinated by `TroubleshootService` to prevent class bloating and keep the codebase clean and maintainable.
* **Unified Service Configuration:** Cleaned up the architectural hierarchy by moving all Registry paths, keys, and policy constants into `OptimizationService`. `SystemEngine` now functions strictly as an execution engine receiving explicit parameters.
* **Integrated Logging & UI Refresh:** Updated header titles, descriptions, and vector icons (`FontIcon`), along with comprehensive `LoggerService` hooks across all newly introduced System Tweaks.

---

## [0.1.1-beta] - 2026-06-25

### 🌟 New Features
* **Troubleshoot Tab:** Added a brand new "Troubleshoot" page to the menu for fixing common computer headaches.
* **Laptop Touchpad Fix:** If your laptop's touchpad randomly freezes or stops responding, you can now hit "Run Fix" to safely restart its driver background service without rebooting your PC.
* **Smart Error Detection:** The app now checks your Windows Device Manager automatically. If it detects that your touchpad has an issue, a yellow warning triangle will pop up next to the fix button to let you know.
* **Error Logging:** Added a silent background system that creates a daily text file (`.log`) inside a `Logs` folder. If a button fails or throws an error, it gets saved there so you can easily send it to us for troubleshooting.

### 🔧 Fixes & Polish
* **Fixed Broken Buttons:** Fixed a background code bug that was causing the new Troubleshoot menu button to do absolutely nothing when clicked. It now opens instantly.
* **Under the Hood Cleanups:** Hooked up all the existing cleaning buttons (RAM, Disk, DNS) to the new logging system so everything runs smoother and safer.

---

## [0.1.0-beta] - 2026-06-17

### 🚀 Initial Release
* **Hello World!** The very first public beta version of **K-WinMod** is officially live.
* **One-Click Optimizations:** Integrated the core cleanup features:
  * Trimming wasted RAM to boost speed.
  * Disabling annoying Windows background telemetry and data tracking.
  * Turning off built-in Windows lock screen ads and popup suggestions.
  * Clearing out hidden temporary junk files to free up disk space.
  * Flushing the network DNS cache to fix sudden internet hiccups.
* **Modern Interface:** Launched the fully responsive, dark-themed user interface with sleek neon green styling.