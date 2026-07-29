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

## 接口

> 第三方工具可通过命名管道实时接收数据，或直接解析 `backpack.json` 文件。

### 命名管道

管道名：`\\.\pipe\ky3-backpack`

帧格式：**4 字节** 数据长度（uint32 LE）+ **12 字节** 事件名（ASCII，末尾补 `\0`）+ 数据正文（UTF-8 JSON）

| 事件 | 正文类型 |
|---|---|
| `weapon` | 武器数组 |
| `artifact` | 圣遗物数组 |
| `avatar` | 角色数组 |
| `material` | 材料数组 |
| `prop` | 账号资产对象 |

### backpack.json

同步后写入 `<游戏目录>/output/backpack.json`。顶层结构：

```json
{
  "source": "ky3-backpack", "version": 1,
  "account": { ... },
  "characters": [ ... ],
  "weapons":    [ ... ],
  "artifacts":  [ ... ],
  "materials":  [ ... ]
}
```

| 字段 | 类型 | 说明 |
|---|---|---|
| `source` | string | 固定值 `"ky3-backpack"` |
| `version` | int | 格式版本号，当前为 `1` |
| `account` | object | 账号资产，见下 |
| `characters` | array | 角色列表 |
| `weapons` | array | 武器列表 |
| `artifacts` | array | 圣遗物列表 |
| `materials` | array | 材料列表 |

---

#### account

```json
{ "playerLevel": 56, "primogem": 1158, "mora": 18942097,
  "worldLevel": 8, "resin": 200, "genesisCrystal": 3,
  "legendaryKey": 3, "homeCoin": 300, "toyToken": 5,
  "qiyuCoin": 140, "reshowCrystal": 720 }
```

| 字段 | 类型 | 说明 |
|---|---|---|
| `playerLevel` | int | 冒险等级 |
| `primogem` | int | 原石 |
| `mora` | int | 摩拉 |
| `worldLevel` | int | 世界等级（0-9）|
| `resin` | int | 原粹树脂当前值 |
| `genesisCrystal` | int | 创世结晶 |
| `legendaryKey` | int | 传说密钥 |
| `homeCoin` | int | 洞天宝钱 |
| `toyToken` | int | 玩具勋章 |
| `qiyuCoin` | int | 千音币 |
| `reshowCrystal` | int | 重映结晶 |

---

#### 角色

```json
{ "id": 10000104, "name": "娜维娅", "element": "岩", "rarity": 5,
  "level": 90, "ascension": 6, "friendship": 10, "constellation": 6,
  "skills":  [{ "id": 11042, "level": 10 }],
  "passives": [{ "id": 10439, "extra": 3 }],
  "equips": ["681646283794107680"] }
```

| 字段 | 类型 | 说明 |
|---|---|---|
| `id` | int | 角色 ID |
| `name` | string | 角色名（中文）|
| `element` | string | 元素（中文，如 `"岩"`）|
| `rarity` | int | 稀有度（4 或 5）|
| `level` | int | 等级（1–90）|
| `ascension` | int | 突破阶段（0–6）|
| `friendship` | int | 好感度（1–10）|
| `constellation` | int | 已解锁命座数（0–6）|
| `skills` | array | 技能等级列表；每项 `{ id, level }`，`id` 为技能 ID，`level` 为等级（1–15）|
| `passives` | array | 固有天赋列表；每项 `{ id, extra }`，`extra` 为天赋额外等级（通常为 0 或 3）|
| `equips` | array | 装备 GUID 字符串列表，武器在前，随后最多 5 件圣遗物 |

---

#### 武器

```json
{ "id": 11509, "guid": "681646283794166235", "name": "雾切之回光",
  "type": "单手剑", "rank": 5, "mainStat": "暴击伤害",
  "level": 90, "ascension": 6, "refine": 1 }
```

| 字段 | 类型 | 说明 |
|---|---|---|
| `id` | int | 武器 ID |
| `guid` | string | 实例唯一 ID（uint64 字符串）|
| `name` | string | 武器名（中文）|
| `type` | string | 武器类型：`单手剑` / `双手剑` / `长柄武器` / `法器` / `弓` |
| `rank` | int | 稀有度（1–5）|
| `mainStat` | string | 主属性名称（中文）|
| `level` | int | 等级（1–90）|
| `ascension` | int | 突破阶段（0–6）|
| `refine` | int | 精炼等级（1–5）|

---

#### 圣遗物

百分比属性 `value` 保留 1 位小数，`rolls` 每项保留 2 位；固定值属性均取整。

```json
{ "id": 72001, "guid": "681646283793999714",
  "set": "追忆之注连", "name": "追忆之注连·绯樱丛簪",
  "slot": "花", "rank": 5, "level": 20, "initSubStats": 4,
  "mainStat": "生命值",
  "subStats": [
    { "type": "暴击率",   "value": 3.9,  "rolls": [3.90] },
    { "type": "暴击伤害", "value": 21.8, "rolls": [7.80, 7.00, 7.00] }
  ],
  "locked": true }
```

| 字段 | 类型 | 说明 |
|---|---|---|
| `id` | int | 圣遗物 ID |
| `guid` | string | 实例唯一 ID（uint64 字符串）|
| `set` | string | 套装名称（中文）|
| `name` | string | 单件名称（中文）|
| `slot` | string | 部位：`花` / `羽` / `沙` / `杯` / `冠` |
| `rank` | int | 稀有度（1–5）|
| `level` | int | 强化等级（0–20）|
| `initSubStats` | int | 初始副词条数（3 或 4）|
| `mainStat` | string | 主属性名称（中文）|
| `subStats` | array | 副属性列表（最多 4 条）|
| `subStats[].type` | string | 副属性名称（中文）|
| `subStats[].value` | number | 副属性总值 |
| `subStats[].rolls` | array | 每次词条强化的数值，长度等于强化次数 |
| `locked` | bool | 是否已上锁 |

---

#### 材料

```json
{ "id": 104003, "name": "精锻用魔矿", "type": "矿石", "count": 42 }
```

| 字段 | 类型 | 说明 |
|---|---|---|
| `id` | int | 材料 ID |
| `name` | string | 材料名称（中文）|
| `type` | string | 子类型（中文），见「特性」分类表 |
| `count` | int | 数量 |

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
