# Aria2Manager

<p align="center">
  <a href="https://github.com/Ftbom/Aria2Manager/blob/master/README.md">English</a> | 
  <a href="https://github.com/Ftbom/Aria2Manager/blob/master/README-zh.md">简体中文</a>
</p>

轻量级 Aria2 服务器桌面管理工具

## 🚀 运行与环境

本程序基于 .NET 开发，如未安装运行环境，请根据首次运行时的弹窗提示进行安装，或直接通过命令行执行以下命令安装：

```powershell
winget install 'Microsoft.DotNet.DesktopRuntime.10'
```

## ✨ 核心功能

- [x] **多服务器管理**：支持添加和管理多个 Aria2 服务器
- [x] **创建下载任务**：支持添加 Url、Torrent 种子及 Metalink 文件
- [x] **任务控制**：支持任务的添加、暂停、恢复、删除，支持按需选择要下载的文件
- [x] **配置可视化**：支持在界面中直接修改 Aria2 配置项
- [x] **本地文件管理**：支持打开下载文件目录和删除已下载的本地文件
- [x] **实时通知**：对任务的开始、暂停、完成及移除等状态进行即时通知
- [x] **扩展功能**：自动更新BT Trackers、检查 Aria2 更新、检查程序更新
- [ ] **跨平台支持**：当前支持 Windows，基于 Avalonia 的 Linux 版本正在规划中

## 📸 界面截图

<p align="center">
  <img src="ScreenShots/MainWindow.png" width="49%" alt="主界面 - 浅色" />
  <img src="ScreenShots/MainWindowDark.png" width="49%" alt="主界面 - 深色" />
</p>

## 🙏 致谢

本项目使用了以下开源项目或参考了其设计：

* [Aria2.Net](https://github.com/rogerfar/Aria2.NET) - Aria2 RPC 通信库
* [MahApps/MahApps.Metro](https://github.com/MahApps/MahApps.Metro) - WPF UI 主题框架
* [AM-Downloader](https://github.com/antikmozib/AM-Downloader) - 主界面 UI 设计参考
* [AriaNg](https://github.com/mayswind/AriaNg) - Aria2 功能实现参考