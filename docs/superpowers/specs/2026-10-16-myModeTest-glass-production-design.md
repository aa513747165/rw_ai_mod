# myModeTest RimWorld 1.7 独立玻璃资源生产链设计

日期：2026-10-16  
状态：设计已在对话中确认，待实施  
目标游戏版本：RimWorld 1.7

## 1. 背景与目标

在 `D:\project\rw` 创建名为 `myModeTest` 的 RimWorld 1.7 Mod 基础框架，并参考 Hardcore SK（HSK）`1.7-super-unstable` 分支，独立实现首条资源生产链：玻璃资源链。

本 Mod 不依赖 HSK；只参考其生产流程、投入产出比例和部分已获授权复制的贴图。所有自定义内容使用 `MMT_` 前缀，避免与原版、HSK 或其他 Mod 冲突。

## 2. 已确认决策

| 项目 | 决策 |
|---|---|
| Mod 显示名 | `myModeTest` |
| 作者占位名 | `YourName` |
| packageId | `YourName.myModeTest` |
| C# 命名空间 | `MyModeTest` |
| 程序集名 | `MyModeTest` |
| 目标框架 | `net472` |
| 与 HSK 的关系 | 独立实现，不要求玩家安装 HSK |
| 首条资源链 | 建材玻璃链 |
| 首版范围 | 核心链等值实现 |
| 文本语言 | 中文 label、description 和配方文本；英文 defName |
| 贴图 | 复制已确认获得授权的 HSK 静态贴图 |
| 编译验证 | 本次不执行；当前没有 RimWorld 安装路径 |
| 游戏内验证 | 本次不执行；不声称已通过游戏加载测试 |

## 3. 范围

### 3.1 包含

- RimWorld 1.7 标准 Mod 目录与元数据。
- SDK 风格 C# 工程和最小 Mod 入口类。
- 四种资源：碎石、砂、玻璃配合料、玻璃。
- 石块/岩块 → 碎石 → 砂 → 玻璃配合料 → 玻璃的完整生产链。
- 手动与电动加工配方差异。
- 一个独立玻璃工坊建筑。
- 一个玻璃制造研究项目。
- 已授权 HSK 静态贴图的复制与路径映射。

### 3.2 不包含

- 强化玻璃。
- 混凝土、金属合金、石化、电子等其他 HSK 资源链。
- HSK 采矿机、完整研究树、设施联动系统。
- HSK 自定义动画工作台类、IdlePower 组件或 HSK 程序集。
- Harmony 或游戏行为补丁。
- 超出上述玻璃链范围的额外玩法内容。
- 本次 C# 编译与游戏内加载测试。

### 3.3 后续资源链路线

以下资源链是后续扩展方向，只作为路线记录，不属于本次实施和验收范围：

1. **木材加工链**：原木 → 木板；木材 → 柴火 → 木炭。
2. **混凝土链**：石块/岩块 → 碎石 → 砂；砂 + 碎石 → 混凝土；金属 + 混凝土 → 钢筋混凝土。
3. **玻璃扩展链**：玻璃配合料 + 碳 + 合成纤维 → 强化玻璃。
4. **基础金属合金链**：铁矿 + 煤 → 钢；铜矿 → 铜锭；锡矿 → 锡锭；铜锭 + 锡锭 → 青铜。
5. **石化材料链**：原油/化学原料 → 塑料、橡胶、合成纤维和碳材料。
6. **电子材料链**：砂 → 硅；硅 + 金属材料 → 电子元件及后续高科技部件。

后续每条资源链都应沿用本次的目录约定，按链拆分文件：

```text
1.7\Defs\ThingDefs_Resources\MMT_Items_Resource_<资源链>.xml
1.7\Defs\RecipeDefs\MMT_Recipes_<资源链>.xml
1.7\Defs\ThingDefs_Buildings\MMT_Buildings_<资源链>.xml
1.7\Defs\ResearchProjectDefs\MMT_Research_<资源链>.xml
1.7\Textures\Things\Item\MMT\<资源链>\
1.7\Textures\Things\Building\Production\MMT_<资源链>\
```

## 4. 总体结构

文件直接创建在 `D:\project\rw`：

```text
D:\project\rw\
├─ About\
│  └─ About.xml
├─ 1.7\
│  ├─ Assemblies\
│  ├─ Defs\
│  │  ├─ RecipeDefs\
│  │  │  └─ MMT_Recipes_Glass.xml
│  │  ├─ ResearchProjectDefs\
│  │  │  └─ MMT_Research_Glass.xml
│  │  ├─ ThingDefs_Buildings\
│  │  │  └─ MMT_Buildings_Glassworks.xml
│  │  └─ ThingDefs_Resources\
│  │     └─ MMT_Items_Resource_Glass.xml
│  └─ Textures\
│     └─ Things\
│        ├─ Building\
│        │  └─ Production\
│        │     └─ MMT_GlassworksTable\
│        └─ Item\
│           └─ MMT\
├─ Source\
│  └─ MyModeTest\
│     ├─ MyModeTest.csproj
│     └─ MyModeTestMod.cs
├─ LoadFolders.xml
├─ MyModeTest.sln
├─ .gitignore
└─ README.md
```

`LoadFolders.xml` 只声明加载 `1.7` 目录。`About.xml` 只在 `supportedVersions` 中声明 `1.7`。

## 5. 资源链设计

```text
石块 / 岩块
    ↓ 切石台或机械加工台
碎石
    ↓ 切石台或机械加工台
砂
    ↓ 玻璃工坊
玻璃配合料
    ↓ 玻璃工坊
玻璃
```

### 5.1 资源 Def

| defName | 中文名 | HSK 基准价值 | 堆叠上限 | 图形 |
|---|---|---:|---:|---|
| `MMT_CrushedStone` | 碎石 | 0.3 | 900 | `Things/Item/MMT/CrushedStone` |
| `MMT_Sand` | 砂 | 0.3 | 900 | `Things/Item/MMT/Sand` |
| `MMT_GlassBatch` | 玻璃配合料 | 1.2 | 300 | `Things/Item/MMT/GlassBatch` |
| `MMT_Glass` | 玻璃 | 5.0 | 150 | `Things/Item/MMT/Glass` |

资源继承原版 `ResourceBase`。玻璃使用 `Graphic_StackCount`，其余资源使用 `Graphic_Single`。首版只把玻璃作为资源产物，不实现玻璃建材或后续消费配方。

### 5.2 生产配方

| defName | 输入 | 输出 | 工作量 | 工作台 | 研究 |
|---|---|---|---:|---|---|
| `MMT_MakeCrushedStone_Hand` | StoneBlocks ×20 | 碎石 ×20 | 1200 | `TableStonecutter` | 无 |
| `MMT_MakeCrushedStone_Electric` | StoneBlocks ×40 | 碎石 ×40 | 1200 | `TableMachining` | 无 |
| `MMT_MakeCrushedStone_Hand_Chunk` | StoneChunks ×1 | 碎石 ×32 | 2400 | `TableStonecutter` | 无 |
| `MMT_MakeCrushedStone_Electric_Chunk` | StoneChunks ×1 | 碎石 ×32 | 1200 | `TableMachining` | 无 |
| `MMT_MakeSand_Hand` | 碎石 ×10 | 砂 ×25 | 600 | `TableStonecutter` | `Stonecutting` |
| `MMT_MakeSand_Electric` | 碎石 ×20 | 砂 ×50 | 600 | `TableMachining` | 无 |
| `MMT_MakeSand_Hand_Chunk` | StoneChunks ×1 | 砂 ×80 | 3000 | `TableStonecutter` | `Stonecutting` |
| `MMT_MakeSand_Electric_Chunk` | StoneChunks ×1 | 砂 ×80 | 1500 | `TableMachining` | 无 |
| `MMT_MakeGlassBatch` | 砂 ×75 | 玻璃配合料 ×25 | 900 | `MMT_GlassworksTable` | `MMT_Glassworking` |
| `MMT_MakeGlass` | 玻璃配合料 ×20 | 玻璃 ×10 | 600 | `MMT_GlassworksTable` | `MMT_Glassworking` |

配方使用原版 `StoneBlocks` 与 `StoneChunks` 分类作为输入；自定义资源通过具体 `thingDefs` 引用。

## 6. 建筑与研究

### 6.1 玻璃工坊

新增 `MMT_GlassworksTable`，中文名“玻璃工坊”。该建筑是普通电力工作台，不复制 HSK 的自定义动画工作台实现。

主要属性：

- 尺寸：`3×1`
- 建造成本：金属材料 ×130、工业零部件 ×10
- 建造工作量：1600
- 最大耐久：180
- 电力消耗：400 W
- 研究前置：`MMT_Glassworking`
- 建造技能要求：9
- 配方：`MMT_MakeGlassBatch`、`MMT_MakeGlass`
- 使用标准 `CompProperties_Power`、`CompProperties_Flickable`、`CompProperties_Breakdownable`、`CompProperties_Glower`、`CompProperties_HeatPusher`

HSK 原版玻璃工坊依赖 `SK.ThingDef_AnimatedWorktable` 和 `SK.CompProperties_IdlePower`，首版不复制这些 C# 类型，以保持 Mod 独立。

### 6.2 研究

新增 `MMT_Glassworking`，中文名“玻璃制造”。

- 基准成本：300
- 科技等级：Medieval
- 前置研究：原版 `Stonecutting`
- 解锁：`MMT_GlassworksTable`

## 7. 贴图来源与映射

HSK 参考来源固定为：

- 仓库：`https://github.com/HardcoreSK/Hardcore-SK`
- 分支：`1.7-super-unstable`
- 提交：`0d665e2bab3982e4eb5ac4ee5ceaf3558dd11fac`

已确认可以复制所需贴图。只复制静态贴图，不复制动画帧。

| HSK 源路径 | myModeTest 目标路径 |
|---|---|
| `Textures/Things/Item/CrushedStone.png` | `Textures/Things/Item/MMT/CrushedStone.png` |
| `Textures/Things/Item/Resource/Sand.png` | `Textures/Things/Item/MMT/Sand.png` |
| `Textures/Things/Item/Resource/GlassBatch.png` | `Textures/Things/Item/MMT/GlassBatch.png` |
| `Textures/Things/Item/Glass/Glass_a.png` | `Textures/Things/Item/MMT/Glass/Glass_a.png` |
| `Textures/Things/Item/Glass/Glass_b.png` | `Textures/Things/Item/MMT/Glass/Glass_b.png` |
| `Textures/Things/Item/Glass/Glass_c.png` | `Textures/Things/Item/MMT/Glass/Glass_c.png` |
| `Textures/Things/Building/Production/GlassWorkingTable/GlassWorkingTabll_off_east.png` | `Textures/Things/Building/Production/MMT_GlassworksTable/MMT_GlassworksTable_east.png` |
| `Textures/Things/Building/Production/GlassWorkingTable/GlassWorkingTabll_off_north.png` | `Textures/Things/Building/Production/MMT_GlassworksTable/MMT_GlassworksTable_north.png` |
| `Textures/Things/Building/Production/GlassWorkingTable/GlassWorkingTabll_south.png` | `Textures/Things/Building/Production/MMT_GlassworksTable/MMT_GlassworksTable_south.png` |

所有贴图放在 `1.7/Textures` 下，XML 使用对应的 `MMT` 路径。

## 8. C# 工程

保留基础 C# 工程，用于后续扩展：

- SDK 风格项目。
- 目标框架：`net472`。
- 程序集输出到 `1.7/Assemblies`。
- 通过 `RimWorldPath` 指向游戏目录。
- 编译前检查 `Data\Managed\Assembly-CSharp.dll` 是否存在，缺失时给出明确错误。
- 最小入口类 `MyModeTestMod` 继承 `Verse.Mod`，构造时输出一条加载日志。

当前没有 RimWorld 安装路径，所以本次只生成工程和配置，不执行编译，不生成 DLL。

## 9. 错误处理与兼容性

- 所有自定义 Def 使用 `MMT_` 前缀，降低 ID 冲突概率。
- 生产链不调用 HSK 类型或 HSK Def，因此缺少 HSK 时不会报错。
- 碎石阶段只引用原版 `TableStonecutter`、`TableMachining`、`Stonecutting`、`StoneBlocks`、`StoneChunks`。
- 若这些原版 ID 在 RimWorld 1.7 中发生变化，需要以后用实际游戏环境进一步验证。
- 工程缺少 `RimWorldPath` 或程序集不存在时，编译目标应给出可读错误。

## 10. 验证计划

本次只执行静态验证：

1. 检查设计中列出的目录和文件均已创建。
2. 使用 XML 解析器解析全部 `.xml` 文件，确认 UTF-8 内容可解析。
3. 检查全部 PNG 文件头和非空大小。
4. 静态检查配方输入、输出、工作台和研究引用是否完整。
5. 检查贴图 XML 路径与实际文件路径一致。

明确不执行：

- `dotnet build`
- RimWorld 游戏启动或存档加载
- 游戏内配方平衡测试

因此实施完成后只能声明“静态检查通过”，不能声明“编译通过”或“游戏内加载通过”。

## 11. 验收标准

- 标准 Mod 元数据和 `LoadFolders.xml` 已生成。
- 玻璃链资源、配方、建筑和研究 Def 已生成。
- 已授权静态贴图按映射复制完成。
- C# 工程和最小入口类已生成。
- README 包含后续指定 `RimWorldPath` 编译的方法。
- 所有 XML、PNG 和静态引用检查通过。
- 最终报告明确说明未编译、未进行游戏内验证。
