# myModeTest RimWorld 1.7 煤炭资源链设计

日期：2026-10-19  
状态：设计待确认  
目标游戏版本：RimWorld 1.7

## 1. 背景与目标

金属链和石化链目前以木炭作为唯一碳源。本条小包新增煤炭资源，参考 Hardcore SK（HSK）的煤炭流程，独立实现：

```text
煤矿脉（手工开采）→ 煤炭
煤炭 ╲
      → 炼钢碳源（与木炭二选一）
木炭 ╱
煤炭/木炭 + 聚合物 + 橡胶 → 碳材料（碳源二选一）
```

本 Mod 不依赖 HSK；只参考其流程、核心数值和已获授权复制的贴图。

HSK 参考基准为本地克隆 HEAD：`b99b17d2fca6af14f3ef3f67cb9f926fb5c76910`（分支 `1.7-super-unstable`）。

## 2. 已确认决策

| 项目 | 决策 |
|---|---|
| 所属 Mod | `myModeTest` |
| packageId | `YourName.myModeTest` |
| 自定义 Def 前缀 | `MMT_` |
| 首版范围 | 煤矿脉 + 煤炭资源 + 钢/碳材料配方碳源双输入 |
| 与 HSK 的关系 | 独立实现，不要求玩家安装 HSK |
| 文本语言 | 中文 label、description；英文 defName |
| 贴图 | 从本地 HSK 克隆复制已确认获得授权的静态贴图 |
| 物品分类 | 使用原版 `ResourcesRaw`，不引入 HSK 的 `Coal` 自定义分类 |
| 深层钻井 | 不实现（HSK 煤炭含深钻字段，写入后续路线） |
| 研究门控 | 不需要研究（与金属矿脉一致） |
| 游戏内验证 | 本次不执行，不声称游戏内加载通过 |

## 3. 范围

### 3.1 包含

- 1 个资源：`MMT_Coal`
- 1 个矿脉建筑：`MMT_MineableCoal`
- 2 个既有配方修改：炼钢、制备碳材料的碳源改为煤炭/木炭双输入
- 1 张已授权贴图
- `tools/validate_mod.py` 的 `coal` 静态验证套件；金属/石化配方套件的多引用输入支持
- 后续路线记录

### 3.2 不包含

- 煤的深层钻井字段。
- 燃料系统（熔炉/发电燃料消耗）。
- 泥炭、焦煤等其他 HSK 燃料资源。
- 游戏内加载和平衡测试。

## 4. HSK 参考基准

HSK 参考来源固定为本地克隆 `b99b17d2fca6af14f3ef3f67cb9f926fb5c76910`。

| 内容 | HSK 基准 |
|---|---|
| 煤炭资源 | 价值 2、堆叠 600、可燃性 3、Medieval、颜色 (85,85,85) |
| 煤矿脉 | 耐久 1500、产量 75、常见度 1.2、矿块 11~18、颜色 (65,34,34)/(147,144,134) |
| 钢配方碳源 | `Coal` 分类 ×13（含煤和木炭） |

与 HSK 的差异：

1. 不引入 `Coal` 物品分类，直接在配方 filter 中列出 `MMT_Coal` 和 `MMT_Charcoal` 两个 defName（语义等价）。
2. 不复制 HSK 煤炭的 `MaxBurningTempCelsius`/`BurnDurationHours`（非原版统计，燃料系统后续再评估）。
3. 深层钻井字段不实现。

RimWorld 1.7 已核对的原版引用：`RockBase`、`ResourcesRaw`、`Things/Building/Linked/RockFlecked_Atlas`（均在前几条链中验证过）。

## 5. 资源设计

| defName | 中文名 | 价值 | 堆叠 | 可燃性 | 图形 | 用途 |
|---|---|---:|---:|---:|---|---|
| `MMT_Coal` | 煤炭 | 2.0 | 600 | 3.0 | `Graphic_Single` | 冶炼碳源 |

继承原版 `ResourceBase`，使用 `ResourcesRaw` 分类，颜色 `(85,85,85)`，`techLevel` 为 Medieval，`DeteriorationRate` 为 0，不设置 `stuffProps`。

## 6. 配方修改

### 6.1 冶炼钢（既有配方修改）

`MMT_SmeltSteel_Hand` 的碳源输入 filter 从单一 `MMT_Charcoal` 扩为 `MMT_Charcoal` + `MMT_Coal`，数量保持 13 不变；`fixedIngredientFilter` 同步追加 `MMT_Coal`。

### 6.2 制备碳材料（既有配方修改）

`MMT_MakeCarbon_Hand` 的碳源输入 filter 从单一 `MMT_Charcoal` 扩为 `MMT_Charcoal` + `MMT_Coal`，数量保持 25 不变；`fixedIngredientFilter` 同步追加 `MMT_Coal`。

## 7. 矿脉设计

新增 `MMT_MineableCoal`，中文名“煤矿脉”，继承原版抽象父类 `RockBase`，使用原版图集贴图着色（无需复制贴图）：

- 产出：`MMT_Coal` ×75
- 耐久：1500
- color `(65,34,34)`，colorTwo `(147,144,134)`
- 散布常见度：1.2；矿块大小：11~18
- `isResourceRock`、`veinMineable`

## 8. 贴图映射

贴图从本地 HSK 克隆复制到 `1.7/Textures` 下的 `MMT` 路径。

| HSK 源路径（`Mods/Core_SK/Textures/`） | myModeTest 目标路径 |
|---|---|
| `Things/Item/Resource/CoalOre.png` | `Textures/Things/Item/MMT/MMT_Coal.png` |

## 9. 静态验证设计

扩展 `tools/validate_mod.py`：

1. 新增 `coal` 套件并纳入 `all`：检查煤炭资源 XML、矿脉定义和 1 张 PNG。
2. 金属/石化配方验证器的多输入分支支持单个 filter 含多个 thingDefs（碳源二选一），期望格式改为元组列表。
3. 检查钢配方碳源为 `(MMT_Charcoal, MMT_Coal)` ×13，碳材料配方碳源为 `(MMT_Charcoal, MMT_Coal)` ×25。
4. 检查矿脉 `mineableThing` 与 `MMT_Coal` 配对、`veinMineable` 存在。
5. 检查新 Def 使用 `MMT_` 前缀，新 XML 不包含 `SK.`。

## 10. 后续路线

以下内容是后续实施路线，只作为设计记录，不属于本次交付：

1. **深层钻井阶段**：为煤炭补充 `deepCommonality` 等字段（HSK 等值：commonality 2.0、每格 2500）。
2. **燃料系统阶段**：熔炉/发电设备消耗煤或木炭，评估 `MaxBurningTempCelsius` 等燃烧统计的引入。
3. **焦炭阶段**：煤 → 焦炭的高温加工配方，作为更高效的冶炼碳源。
4. **平衡调整阶段**：游戏内测试煤矿脉常见度与木炭的性价比关系后再调整数值。

## 11. 验证计划

本次执行静态验证和 C# 编译验证：

1. `python tools/validate_mod.py coal` 通过。
2. `python tools/validate_mod.py all` 通过。
3. `dotnet build Source/MyModeTest/MyModeTest.csproj --no-restore` 通过，0 警告 0 错误。
4. Git 工作区干净。

明确不执行：

- RimWorld 游戏启动或存档加载
- 游戏内配方平衡测试

因此实施完成后只能声明“静态检查通过、C# 编译通过”，不能声明“游戏内加载通过”。

## 12. 验收标准

- `MMT_Coal` 资源和 `MMT_MineableCoal` 矿脉定义完整。
- 炼钢和碳材料配方的碳源为煤炭/木炭双输入。
- 1 张已授权贴图按映射复制完成。
- `coal` 和 `all` 静态验证通过。
- C# 编译通过。
- 后续路线已写入本文档。
- 最终报告明确说明未进行游戏内验证。
