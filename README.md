[简体中文](README.md) | [English](README.en.md)

<p align="center">
  <img src="assets/logo.png" width="96" />
</p>

<h1 align="center">Backpack</h1>

<p align="center">
  一个可视化的原神背包浏览器 · 从游戏内同步 · 本地储存
</p>

<p align="center">
  <img alt="Platform" src="https://img.shields.io/badge/platform-Windows%2010%2B-blue?logo=windows" />
  <img alt="License" src="https://img.shields.io/badge/license-MIT-green" />
  <img alt="WinUI3" src="https://img.shields.io/badge/UI-WinUI3-blueviolet" />
  <img alt="CSharp" src="https://img.shields.io/badge/C%23%20%C2%B7%20C%2B%2B-tech-informational" />
</p>

<p align="center">
  <img src="assets/screenshot-artifacts.png" width="49%" />
  <img src="assets/screenshot-materials.png" width="49%" />
</p>

---

## 特性

- **无 OCR · 无截图** — 解析游戏原始数据

| 分类 | 展示内容 |
|---|---|
| 武器 | 名称 · 等级 · 精炼等级 · 稀有度 · 武器类型 · 主属性 |
| 圣遗物 | 名称 · 部位 · 强化等级 · 主属性 · 四件副属性（词条数）· 套装效果 |
| 材料 | 12 子类：角色突破 / 武器突破 / 天赋 / 角色培养 / 武器强化 / 精炼 / 地区特产 / 食材 / 素材 / 矿石 / 鱼 / 鱼饵 |
| 食物 | 名称 · 数量 · 品质 · 恢复类 · 攻击类 · 防御类 · 冒险类 · 特殊类 · 灵感类 |              
| 道具 | 名称 · 数量 · 品质 · 贵重类 · 冒险类 · 七国徽印 · 祈愿 · 小道具 · 消耗品 · 任务道具 |  
| 个人资产 | 原石 · 摩拉 · 创世结晶 · 洞天宝钱 · 玩具勋章 · 千音币等 |

## 系统要求

- Windows 10 2004 (19041) x64 或更高
- 原神国服 / ~~国际服~~（待适配）

## 安装

前往 [Releases](../../releases) 下载最新版安装包，运行后按提示完成安装。

## 使用

1. 启动 Backpack
2. **选择游戏路径**，定位到原神可执行文件
3. 点击 **同步背包**，等待游戏启动
4. 进入游戏后数据自动加载，窗口展示完整背包内容

> 数据会缓存到本地，下次打开无需重复同步。

## 技术栈

| 层 | 技术 |
|---|---|
| UI | WinUI3 · C# |
| 数据解析 | C++ · Protobuf |
| 本地存储 | SQLite |
| 进程通信 | 命名管道 IPC |

## 构建

**前置要求**

- Visual Studio 2022（含 C++ 桌面开发工作负载）
- .NET 10 SDK

**C++ 解析层**

```bash
msbuild backpack.vcxproj /p:Configuration=Release /p:Platform=x64
```

**C# 查看器**

```bash
dotnet build viewer\viewer.csproj -c Release -p:Platform=x64
```

发布（self-contained）：

```bash
dotnet publish viewer\viewer.csproj -c Release -p:Platform=x64 -r win-x64 --self-contained
```

## 许可证

[MIT](LICENSE) © 2026 KY3 Studio
