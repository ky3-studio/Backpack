[简体中文](README.md) | [English](README.en.md)

<p align="center">
  <img src="assets/logo.png" width="96" />
</p>

<h1 align="center">Backpack</h1>

<p align="center">
  A Genshin Impact inventory browser &middot; Real-time in-game sync &middot; Local storage
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

## Features

- **No OCR · No screenshots** — Parses raw game data

| Category | Data |
|---|---|
| Weapons | Name · Level · Refinement · Rarity · Type · Main stat |
| Artifacts | Name · Slot · Enhancement · Main stat · 4 sub-stats (rolls) · Set effect |
| Materials | 12 sub-types: Char ascension / Weapon ascension / Talent / Char EXP / Weapon EXP / Refinement / Local specialty / Food / Crafting / Ore / Fish / Bait |
| Food | Name · Quantity · Quality · Healing / Attack / Defense / Exploration / Special / Inspirational |
| Gadgets | Name · Quantity · Quality · Precious / Exploration / Nation emblems / Wishes / Gadgets / Consumables / Quest items |
| Assets | Primogem · Mora · Genesis Crystal · Realm Currency · Toy Medal · Starglitter etc. |

## Requirements

- Windows 10 2004 (19041) x64 or later
- Genshin Impact CN / ~~Global~~ (coming soon)

## Installation

Download the latest installer from [Releases](../../releases) and follow the on-screen instructions.

## Usage

1. Launch Backpack
2. **Select Game Path** and point to the Genshin executable
3. Click **Sync Backpack** and wait for the game to start
4. Once in-game, data loads automatically and your inventory appears in the window

> Data is cached locally, so subsequent launches display the last snapshot without re-syncing.

## Tech Stack

| Layer | Technology |
|---|---|
| UI | WinUI3 · C# |
| Data parsing | C++ · Protobuf |
| Local storage | SQLite |
| IPC | Named Pipe |

## Building

**Prerequisites**

- Visual Studio 2022 with C++ Desktop Development workload
- .NET 10 SDK

**C++ parsing layer**

```bash
msbuild backpack.vcxproj /p:Configuration=Release /p:Platform=x64
```

**C# viewer**

```bash
dotnet build viewer\viewer.csproj -c Release -p:Platform=x64
```

Publish (self-contained):

```bash
dotnet publish viewer\viewer.csproj -c Release -p:Platform=x64 -r win-x64 --self-contained
```

## License

[MIT](LICENSE) © 2026 KY3 Studio
