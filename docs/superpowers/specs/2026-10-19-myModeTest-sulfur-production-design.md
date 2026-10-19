# myModeTest RimWorld 1.7 硫磺支线设计

日期：2026-10-19  
状态：设计待确认  
目标游戏版本：RimWorld 1.7

## 1. 背景与目标

石化链的橡胶和合成纤维配方在首版实施时简化了 HSK 配方（省略了硫化剂输入）。本条小包新增硫磺资源，恢复 HSK 等值配方，独立实现：

```text
硫磺矿脉（手工开采）→ 硫磺
聚合物 + 硫磺 → 橡胶（恢复 HSK 等值）
聚合物 + 硫磺 → 合成纤维（恢复 HSK 等值）
```

本 Mod 不依赖 HSK；只参考其流程、核心数值和已获授权复制的贴图。

HSK 参考基准为本地克隆 HEAD：`b99b17d2fca6af14f3ef3f67cb9f926fb5c76910`（分支 `1.7-super-unstable`）。

## 2. 已确认决策

| 项目 | 决策 |
|---|---|
| 所属 Mod | `myModeTest` |
| packageId | `YourName.myModeTest` |
| 自定义 Def 前缀 | `MMT_` |
| 首版范围 | 硫磺矿脉 + 硫磺资源 + 橡胶/合成纤维配方恢复 HSK 等值 |
| 与 HSK 的关系 | 独立实现，不要求玩家安装 HSK |
| 文本语言 | 中文 label、description；英文 defName |
| 贴图 | 从本地 HSK 克隆复制已确认获得授权的静态贴图 |
| 物品分类 | 使用原版 `ResourcesRaw`，不引入 HSK 的 `Chemical` 自定义分类 |
| 资源合并 | HSK 的 `Sulfur`（硫磺）与 `Sulphates`（硫酸盐）合并为单一资源 `MMT_Sulfur`（YAGNI） |
| 研究门控 | 不需要新研究（配方仍由 `MMT_BasicPetrochemistry` 门控） |
| 游戏内验证 | 本次不执行，不声称游戏内加载通过 |

## 3. 范围

### 3.1 包含

- 1 个资源：`MMT_Sulfur`
- 1 个矿脉建筑：`MMT_MineableSulfur`
- 2 个既有配方修改：合成橡胶、合成纤维增加硫磺输入
- 1 张已授权贴图
- `tools/validate_mod.py` 的 `sulfur` 静态验证套件；石化配方套件期望更新
- 后续路线记录

### 3.2 不包含

- 铁矿石提炼硫磺配方（HSK `MakeSulfur` 等值，写入后续路线）。
- 塑料硫磺填充配方（HSK `MakePlasticSulphate` 等值，写入后续路线）。
- 火药/弹药等硫磺下游用途。
- 游戏内加载和平衡测试。

## 4. HSK 参考基准

HSK 参考来源固定为本地克隆 `b99b17d2fca6af14f3ef3f67cb9f926fb5c76910`。

| 内容 | HSK 基准 |
|---|---|
| 硫磺资源（`Sulfur`） | 价值 7、堆叠 300、可燃性 2、耐久 5、Industrial、颜色 (255,255,255)，带爆炸 comp |
| 硫酸盐资源（`Sulphates`） | 价值 7、堆叠 300、可燃性 2、耐久 10、Industrial，带烟雾爆炸 comp |
| 橡胶配方（`MakeRubber`） | 聚合物 30 + 硫酸盐 3 → 橡胶 10，工作量 800，Crafting 10 |
| 纤维配方（`MakeSyntheticFibers`） | 聚合物 25 + 硫酸盐 3 → 纤维 5，工作量 1200，Crafting 14 |
| 硫磺来源 | 铁矿 35 → 硫磺 10（弹药台）、硫磺提取器（UF）、石化厂；无矿脉 |

与 HSK 的差异：

1. HSK 的 `Sulfur` 和 `Sulphates` 两种资源合并为 `MMT_Sulfur`，统一作为硫化/填充剂。
2. 不复制爆炸 comp（`CompProperties_Explosive`）：手动流程下硫磺仅作配方原料，避免储存风险，后续评估。
3. 不引入 HSK 的 `Chemical` 物品分类，使用原版 `ResourcesRaw`。
4. HSK 无硫磺矿脉；`MMT_MineableSulfur` 为自设计内容（与油苗、煤矿脉一致的自设计先例），数值待平衡。
5. 橡胶/纤维的技能要求保持本 Mod 现状（Crafting 6/8），不恢复 HSK 的 10/14，维持手动流程的低门槛定位。

RimWorld 1.7 已核对的原版引用：`RockBase`、`ResourcesRaw`、`Things/Building/Linked/RockFlecked_Atlas`（均在前几条链中验证过）。

## 5. 资源设计

| defName | 中文名 | 价值 | 堆叠 | 可燃性 | 图形 | 用途 |
|---|---|---:|---:|---:|---|---|
| `MMT_Sulfur` | 硫磺 | 7.0 | 300 | 2.0 | `Graphic_Single` | 橡胶/纤维硫化剂 |

继承原版 `ResourceBase`，使用 `ResourcesRaw` 分类，颜色 `(255,255,255)`（贴图本身为黄色），`techLevel` 为 Industrial，`DeteriorationRate` 为 0，不设置 `stuffProps`，不带爆炸 comp。

## 6. 配方修改

### 6.1 合成橡胶（既有配方修改，恢复 HSK 等值）

`MMT_MakeRubber_Hand` 增加第二输入：硫磺 ×3（HSK `MakeRubber` 等值，硫酸盐→硫磺）。聚合物 30、产出 10、工作量 800、技能 Crafting 6 均不变；`fixedIngredientFilter` 同步追加 `MMT_Sulfur`。

### 6.2 合成纤维（既有配方修改，恢复 HSK 等值）

`MMT_MakeSyntheticFibers_Hand` 增加第二输入：硫磺 ×3（HSK `MakeSyntheticFibers` 等值，硫酸盐→硫磺）。聚合物 25、产出 5、工作量 1200、技能 Crafting 8 均不变；`fixedIngredientFilter` 同步追加 `MMT_Sulfur`。

## 7. 矿脉设计

新增 `MMT_MineableSulfur`，中文名"硫磺矿脉"，继承原版抽象父类 `RockBase`，使用原版图集贴图着色（无需复制贴图）：

- 产出：`MMT_Sulfur` ×75
- 耐久：1500
- color `(65,34,34)`，colorTwo `(216,190,80)`（黄色矿斑，标识硫磺）
- 散布常见度：0.8；矿块大小：10~16
- `isResourceRock`、`veinMineable`

注：HSK 无硫磺矿脉，本矿脉为自设计内容，常见度与油苗（0.8）一致，定位为中后期补充资源，数值待游戏内平衡。

## 8. 贴图映射

贴图从本地 HSK 克隆复制到 `1.7/Textures` 下的 `MMT` 路径。

| HSK 源路径（`Mods/Core_SK/Textures/`） | myModeTest 目标路径 |
|---|---|
| `Things/Item/Resource/Sulfur.png` | `Textures/Things/Item/MMT/MMT_Sulfur.png` |

## 9. 静态验证设计

扩展 `tools/validate_mod.py`：

1. 新增 `sulfur` 套件并纳入 `all`：检查硫磺资源 XML、矿脉定义和 1 张 PNG。
2. 石化配方期望更新：橡胶配方输入为聚合物 30 + 硫磺 3；纤维配方输入为聚合物 25 + 硫磺 3（多输入分支已存在，直接改期望常量）。
3. 检查矿脉 `mineableThing` 与 `MMT_Sulfur` 配对、`veinMineable` 存在。
4. 检查新 Def 使用 `MMT_` 前缀，新 XML 不包含 `SK.`。

## 10. 后续路线

以下内容是后续实施路线，只作为设计记录，不属于本次交付：

1. **铁矿提炼阶段**：恢复 HSK `MakeSulfur` 等值配方（铁矿石 35 → 硫磺 10），作为矿脉之外的硫磺来源。
2. **塑料填充阶段**：恢复 HSK `MakePlasticSulphate` 等值配方（聚合物 20 + 硫磺 5 → 塑料 20，比例更优）。
3. **火药/弹药阶段**：硫磺 + 硝石 → 火药，支撑弹药生产。
4. **爆炸属性评估阶段**：评估是否为硫磺恢复 HSK 的爆炸 comp。
5. **平衡调整阶段**：游戏内测试硫磺矿脉常见度与橡胶/纤维成本后再调整数值。

## 11. 验证计划

本次执行静态验证和 C# 编译验证：

1. `python tools/validate_mod.py sulfur` 通过。
2. `python tools/validate_mod.py all` 通过。
3. `dotnet build Source/MyModeTest/MyModeTest.csproj --no-restore` 通过，0 警告 0 错误。
4. Git 工作区干净。

明确不执行：

- RimWorld 游戏启动或存档加载
- 游戏内配方平衡测试

因此实施完成后只能声明"静态检查通过、C# 编译通过"，不能声明"游戏内加载通过"。

## 12. 验收标准

- `MMT_Sulfur` 资源和 `MMT_MineableSulfur` 矿脉定义完整。
- 橡胶和合成纤维配方恢复 HSK 等值（增加硫磺 ×3 输入）。
- 1 张已授权贴图按映射复制完成。
- `sulfur` 和 `all` 静态验证通过。
- C# 编译通过。
- 后续路线已写入本文档。
- 最终报告明确说明未进行游戏内验证。
