# Changelog

Here you can track all the updates and new features added to **K-WinMod** in plain English.

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