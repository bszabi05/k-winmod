# K-WinMod

[![Version](https://img.shields.io/badge/version-0.1.0--beta-green.svg)](https://github.com/kotrabdev/k-winmod)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![Platform](https://img.shields.io/badge/platform-Windows%2010%20%7C%2011-blue.svg)]()

**K-WinMod** is an ultra-fast, minimalist, and modern system utility designed to clean, speed up, and fine-tune your Windows 10 or 11 operating system. It provides a centralized, lightweight dashboard where users can instantly reclaim system resources, eliminate unwanted operating system clutter, and boost overall responsiveness with a single click—all wrapped in a sleek, dark-themed user interface with sharp neon green accents.

The application is built to deliver a transparent and safe optimization experience. It completely replaces bloated third-party software and mysterious, unverified scripts by handling all system adjustments directly and securely in the background, ensuring your computer runs leaner and faster without compromising stability.

---

## 🚀 Key Features (v0.1.0-beta)

The application architecture is broken down into completely isolated, dedicated **Engines** to ensure maximum safety and modularity:

* **MemoryEngine (RAM Optimization):** Intelligently and safely flushes process working sets using the native Win32 API (`EmptyWorkingSet`).
* **RegistryEngine (System Tweaks):** Radically disables unwanted Windows telemetry, data collection, and promotional tips via direct Registry modifications.
* **DiskEngine (Storage Cleanup):** Safely scans user and system temporary (`Temp`) directories to sweep away unlocked junk files.
* **NetworkEngine (Network Acceleration):** Performs a clean, native flush of the Windows DNS resolver cache to guarantee seamless network connectivity.

---
## 📦 Installation & Download / Letöltés

### 📥 Direct Download
You can always download the latest compiled, portable version of the application directly from the GitHub Releases page:
👉 **[Download K-WinMod Latest Release](https://github.com/bszabi05/k-winmod/releases)**

---
## 🛠️ Tech Stack

* **Framework:** WinUI 3 (Windows App SDK 1.x)
* **Language:** C# 10+ / .NET 6+
* **Design Language:** Fluent Design (Dark mode with custom neon green `#A3E635` accents)
* **Architecture:** Exemplary layered structure (UI → `OptimizationService` Facade → Dedicated Core Engines)

---

## ⚖️ License

Distributed under the **MIT License**. See the `LICENSE.txt` file for the full legal text.

---

**Developed with ❤️ by bszabi05 - 2026**