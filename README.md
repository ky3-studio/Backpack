[简体中文](README.md) | [English](README.en.md)

<p align="center">
  <img src="assets/logo.png" width="120" />
</p>

# Backpack

原神背包数据导出工具  
无 OCR。无截图。

<p align="center">
  <img src="assets/preview.png" width="720" />
</p>

## 编译

### 环境要求

- Visual Studio 2022 Build Tools（MSVC v145）
- .NET 10 SDK
- Windows App SDK 2.2

### DLL（C++）

用 Visual Studio 打开 `backpack.vcxproj`，选择 **Release | x64** 生成，或命令行：

```bat
msbuild backpack.vcxproj /p:Configuration=Release /p:Platform=x64
```

输出：`x64\Release\backpack.dll`

### Viewer（C#）

```bat
cd viewer
dotnet build -c Release -p:Platform=x64
```

输出：`viewer\bin\x64\Release\net10.0-windows10.0.26100.0\win-x64\`

---

## 接口对接

DLL 会将 JSON 写入游戏 exe 同目录下的 `output\` 子目录，同时通过命名管道实时推送相同数据。

### 命名管道

**管道名：** `\\.\pipe\ky3-backpack`

每条消息独占一个连接，格式为固定 16 字节头 + UTF-8 JSON 正文：

| 偏移 | 大小 | 类型 | 说明 |
|------|------|------|------|
| 0 | 4 | `uint32_le` | JSON 正文字节长度 |
| 4 | 12 | `char[12]` | 事件名，不足补零 — 见下表 |
| 16 | N | `utf-8` | JSON 正文 |

每个事件建立一次独立连接。先读 16 字节头，再精确读取 N 字节正文。

**事件名：**

| 事件 | JSON 文件 | 内容 |
|------|-----------|------|
| `weapon_bag` | `output\weapon_bag.json` | 背包中全部武器 |
| `artifact_bag` | `output\artifact_bag.json` | 背包中全部圣遗物 |
| `material_bag` | `output\material_bag.json` | 背包中全部材料 |
| `player_prop` | `output\player_prop.json` | 玩家属性（冒险等级、世界等级、体力等） |

### JSON 格式

**weapon_bag.json**

```json
{
  "weapons": [
    {
      "id": 11301,
      "guid": "2251799814125937",
      "name": "黎明神剑",
      "type": "单手剑",
      "rank": 3,
      "specialProp": "攻击力",
      "level": 40,
      "promote": 2,
      "refine": 1
    }
  ]
}
```

**artifact_bag.json**

```json
{
  "artifacts": [
    {
      "id": 37543,
      "guid": "2251799814125938",
      "setName": "烬城勇者绘卷",
      "name": "驯兽师的护符",
      "slot": "生之花",
      "equipped": true,
      "level": 20,
      "rank": 5,
      "mainStat": { "type": "生命值", "typeRaw": "FIGHT_PROP_HP" },
      "subStats": [
        { "type": "防御力",   "typeRaw": "FIGHT_PROP_DEFENSE",       "value": 46,   "rolls": 2 },
        { "type": "暴击率",   "typeRaw": "FIGHT_PROP_CRITICAL",       "value": 2.7,  "rolls": 1 },
        { "type": "暴击伤害", "typeRaw": "FIGHT_PROP_CRITICAL_HURT",  "value": 15.5, "rolls": 2 },
        { "type": "攻击力%",  "typeRaw": "FIGHT_PROP_ATTACK_PERCENT", "value": 16.3, "rolls": 3 }
      ]
    }
  ]
}
```

**material_bag.json**

```json
{
  "materials": [
    { "id": 104001, "name": "甜甜花", "category": "specialty_mondstadt", "count": 12 }
  ]
}
```

---

## 目录结构

```
Backpack/
├── artifact/             圣遗物 Protobuf 解析与 JSON 输出
├── db/                   由官方 ExcelConfigData 生成的静态查找表
│   ├── artifact_db.h     词条数值 + 主属性 ID + 中文显示名
│   ├── artifact_set_db.h 道具 ID → 套装名 / 部件名 / 星级
│   └── weapon_db.h       武器元数据
├── hook/                 DLL 入口（dllmain）、MinHook 挂载、数据包分发
├── include/
│   ├── ipc.h             命名管道推送辅助
│   ├── offsets.h         RVA 常量
│   ├── output.h          JSON 格式字符串
│   └── proto.h           Protobuf varint 解析器
├── material/             材料 Protobuf 解析与 JSON 输出
├── prop/                 玩家属性解析与 JSON 输出
├── third_party/MinHook/  内嵌 MinHook，无外部依赖
├── weapon/               武器 Protobuf 解析与 JSON 输出
├── viewer/               WinUI 3 桌面查看器（C# / .NET 10）
└── backpack.vcxproj      C++ 项目文件（MSVC Release|x64）
```

## 运行时目录

程序运行后会在 exe 同目录自动创建以下子目录：

| 目录 | 内容 |
|------|------|
| `modules/` | `backpack.dll`，由 Viewer 启动时自动注入 |
| `data/` | `backpack.db`，SQLite 数据库，保存历史背包数据 |
| `output/` | `weapon_bag.json` / `artifact_bag.json` / `material_bag.json` / `player_prop.json`，每次同步后覆盖写入 |

---

## 问题反馈

如果遇到崩溃、数据不正确或功能异常，请在 [Issues](../../issues) 中提交，附上以下信息：

- 游戏版本
- 操作系统版本
- 复现步骤
- `output\` 目录下的相关 JSON 文件（如有）

## 贡献

Pull Request：

- 错误修复
- 新物品类型的解析支持

## 免责声明

本项目仅供学习研究使用，风险自负。

## 许可证

[MIT](LICENSE) © 2026 KY3 Studio
