# K-WinMod

[![Version](https://img.shields.io/badge/version-0.2.0--beta-green.svg) ](https://github.com/bszabi05/k-winmod)[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![Platform](https://img.shields.io/badge/platform-Windows%2010%20%7C%2011-blue.svg)]()

**K-WinMod** is an ultra-fast, minimalist, and modern system utility designed to clean, speed up, and fine-tune your Windows 10 or 11 operating system. It provides a centralized, lightweight dashboard where users can instantly reclaim system resources, eliminate unwanted operating system clutter, and boost overall responsiveness with a single click—all wrapped in a sleek, dark-themed user interface with sharp neon green accents.

The application is built to deliver a transparent and safe optimization experience. It completely replaces bloated third-party software and mysterious, unverified scripts by handling all system adjustments directly and securely in the background, ensuring your computer runs leaner and faster without compromising stability.

---

## 🚀 Key Features

**K-WinMod** gives you a collection of simple, powerful tools to speed up your PC, protect your privacy, and fix sudden system or hardware headaches with a single click:

### 🧹 System Speedups & Performance
* **1-Click Smart Boost:** Runs RAM trimming, temporary file cleanup, and DNS cache flushing concurrently right from the Dashboard for instant responsiveness.
* **Startup Apps Manager:** View, enable, disable (via native Windows `StartupApproved` registry keys), or permanently remove programs that launch when Windows starts.
* **Ultimate Performance Power Scheme:** Instantly unlocks and activates the hidden Windows Ultimate Performance power plan to eliminate micro-latencies and boost responsiveness.
* **Windows Game Mode Control:** Easily toggle Windows Game Mode to prioritize CPU and GPU resources for gaming.
* **Free Up RAM Memory:** Instantly clears out wasted memory from background apps to give your games and heavy software more breathing room.
* **Clean Disk Garbage:** Scans and sweeps away temporary junk files, system caches, and Prefetch data to free up valuable storage space.
* **Fix Internet Hiccups:** Flushes your network DNS cache, clears Winsock catalogs, and resets IP stacks to fix connection drops.

### 🔒 Privacy & Anti-Annoyance Toggles
* **Block Windows Telemetry:** Completely shuts down Microsoft's background data collection, tracking, and system logging.
* **Disable Start Menu Bing Search:** Removes online Bing web search suggestions from your Start Menu for cleaner, faster local search results.
* **Turn Off Ads & Popups:** Disables annoying built-in Windows tips, suggested apps, and promotional notifications on your lock screen and desktop.

### 🛠️ Comprehensive Troubleshoot & Safety Toolkit
* **System Restore Checkpoints:** Create instant Windows System Restore Points in one click (automatically enabling system protection and bypassing frequency limits) or launch the Windows System Restore wizard (`rstrui.exe`).
* **10 One-Click System Diagnostic Fixes:** Includes dedicated one-click repairs for Audio services, Taskbar/Explorer restarts, Network adapters, Print Spooler, Bluetooth stack, Windows Update cache (`SoftwareDistribution`), System File Repair (`SFC / DISM`), GPU Driver resets, and Temp purges.
* **Unfreeze Laptop Touchpads:** Safely restarts background touchpad drivers instantly without needing to reboot your PC.
* **Automatic Error Detection:** Automatically scans your Windows Device Manager on startup and alerts you with a visual warning icon if device driver issues are detected.
* **Settings, Logs & Auto-Update Checker:** Built-in GitHub release checker (with automatic startup notifications), one-click access to the `Logs/` folder, and log cleanup tools.

---
## 🛠️ Tech Stack

* **Framework:** WinUI 3 (Windows App SDK 1.5)
* **Language:** C# 12 / .NET 8
* **Design Language:** Fluent Design (Dark mode with custom neon green `#A3E635` accents & Mica backdrop)
* **Architecture:** Layered architecture (UI Pages → `OptimizationService` / `TroubleshootService` / `StartupService` / `UpdateService` → Core Engines)

---
## 📦 Installation & Download

### 📥 Direct Download
You can always download the latest compiled, portable version of the application directly from the GitHub Releases page:
👉 **[Download K-WinMod Latest Release](https://github.com/bszabi05/k-winmod/releases)**

---

## 📅 Version History & Changelog

Looking for the latest updates, bug fixes, or want to see how the app has evolved? Check out our user-friendly changelog file:

👉 **[Read the Full CHANGELOG.md](CHANGELOG.md)**

---

## ⚖️ License

Distributed under the **MIT License**. See the `LICENSE.txt` file for the full legal text.

---

**Developed with ❤️ by bszabi05 - 2026**
