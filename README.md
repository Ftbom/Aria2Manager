# Aria2Manager

<p align="center">
  <a href="https://github.com/Ftbom/Aria2Manager/blob/master/README.md"><b>English</b></a> | 
  <a href="https://github.com/Ftbom/Aria2Manager/blob/master/README-zh.md">简体中文</a>
</p>

A cross-platform desktop manager for Aria2.

---

## 📦 Version & Download Guide

This project currently maintains two UI framework branches. Please visit the [Releases](https://github.com/Ftbom/Aria2Manager/releases) page to download according to your needs:

### 1. Choose UI Branch
* **v2.x (Avalonia - Recommended)**: Supports Windows, Linux, and macOS. Actively updated.
* **v1.x (WPF - Maintenance Only)**: The classic Windows-only branch. Currently in maintenance mode for bug fixes only; no new features will be added.

### 2. Choose Package Type
* **Standard** (no suffix): A pure Aria2 RPC client, **without the built-in Aria2 executable**. Suitable for users who have already configured an Aria2 service locally or on a server.
* **Bundle** (with `-bundle` suffix): **Includes the official Aria2 executable**. Out-of-the-box experience, no extra configuration required.
  >*Due to Aria2 official support limitations, the bundle package is only available for Windows x64.*

>[!NOTE]
>**Aria2 Executable Invocation Order**:
>1. **System Environment Variables**: Prioritizes the system-installed Aria2 (needs to be installed via `apt` / `brew`, etc., and added to the PATH).
>2. **Local Subdirectory**: If not installed on the system, it will attempt to invoke the executable within the `Aria2` folder located in the same directory as the application (you can download and place it there yourself).

---

## ✨ Key Features

* **Multi-server Support**: Add and easily switch between multiple Aria2 server nodes.
* **Visual Configuration**: Modify various Aria2 parameters through an intuitive graphical interface.
* **Add Download Tasks**: Create tasks via URLs, Torrent files, or Metalink files.
* **Task Control**: Start, pause, resume, and delete tasks easily.
* **System Notifications**: Receive desktop notifications when task statuses change.
* **Local File Integration**: For local Aria2 servers, easily open download directories, clean up local files, and edit the local Aria2 configuration file.
* **Additional Features**: Automatically update BT Trackers list, and check for software and Aria2 core updates.

---

## 📸 Screenshots

### v2.x Cross-platform (Avalonia)
<p align="center">
  <img src="ScreenShots/MainWindow.png" width="49%" alt="Main Window - Light" />
  <img src="ScreenShots/MainWindowDark.png" width="49%" alt="Main Window - Dark" />
</p>

<details>
  <summary><b>✨ Click to expand for more screenshots</b></summary>
  <br/>

  <p align="center">
    <img src="ScreenShots/Aria2TaskInfoWindow1.png" width="49%" alt="Task Details 1" />
    <img src="ScreenShots/Aria2TaskInfoWindow2.png" width="49%" alt="Task Details 2" />
  </p>
  
  <p align="center">
    <img src="ScreenShots/Aria2OptionsWindow.png" width="49%" alt="Aria2 Options" style="vertical-align: top;" />
    <img src="ScreenShots/SettingsWindow.png" width="49%" alt="App Settings" style="vertical-align: top;" />
  </p>
</details>

### v1.x Classic (WPF)
<p align="center">
  <img src="ScreenShots/WpfMainWindow.png" width="49%" alt="Main Window - Light" />
  <img src="ScreenShots/WpfMainWindowDark.png" width="49%" alt="Main Window - Dark" />
</p>

---

## 🛠️ Development Environment Setup

This project depends on [NuGet packages](https://github.com/Ftbom?tab=packages&repo_name=DesktopNotifications) hosted on GitHub Packages. Before building the project, you must prepare a GitHub Personal Access Token (PAT) with `read:packages` permissions and configure the following system environment variables for `nuget.config` to read:

* `GITHUB_PACKAGE_USER`: Your GitHub username
* `GITHUB_PACKAGE_TOKEN`: Your GitHub Token

---

## 🙏 Acknowledgements

This project uses or is inspired by the following open-source projects:

* [Aria2.Net](https://github.com/rogerfar/Aria2.NET) - Core RPC communication library
* [Avalonia UI](https://github.com/AvaloniaUI/Avalonia) - Excellent cross-platform UI framework
* [MahApps.Metro](https://github.com/MahApps/MahApps.Metro) - WPF UI theme
* [AM-Downloader](https://github.com/antikmozib/AM-Downloader) - Main UI design reference
* [AriaNg](https://github.com/mayswind/AriaNg) - Aria2 feature and logic implementation reference