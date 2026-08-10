# Changelog

Here you can track all the updates and new features added to **K-WinMod** in plain English.

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