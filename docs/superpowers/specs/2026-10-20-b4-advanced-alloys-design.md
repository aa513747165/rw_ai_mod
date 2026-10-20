# B4 高级合金（EAF）设计 Spec

- 日期：2026-10-20
- 状态：已审阅（随实施计划一并批准）
- 依据：`docs/superpowers/specs/2026-10-20-b2-b8-batch-roadmap.md` 第 4 节；全局约定见该文件第 0 节，本文不重复
- 前置：B1/B2/B3 已完成（铝合金 `MMT_AluminiumBar` 存在，亚力可依赖满足）

## 1. 范围

| HSK 条目 | MMT 现状 | 动作 |
|---|---|---|
| 镍/钴/铬/钨/钛/司太立/镍钛诺/钴硬质/铁钛（9 条） | 已实现且数值一致 | 仅类别补齐（§4） |
| 贫铀合金 DepletedUranium | 无 | 新资源 + 原版铀×10 @ EAF |
| 斯坦合金 SteelBar（HSK 配方 defName 为 MakeFerronickelAlloy） | 无 | 新资源 + 钢+镍 @ EAF |
| 梅尔基奥 CupronickelAlloy | 无 | 新资源 + 铜+镍 @ EAF |
| 亚力可 AlnicoAlloy | 无 | 新资源 + 钢+铝+钴+镍 @ EAF |
| 硅 EAF 批量配方 MakeSilicon_Set | MMT 已有电熔版 | **采纳**：沙×200→硅×20 |

## 2. HSK 参考数值（实地核对，来源 `Hardcore-SK/Mods/Core_SK/Defs/RecipeDefs/Recipes_Materials_Alloys.xml` 与 `Items_Resource_Alloys_Industrial.xml`）

| HSK 配方 | 输入 | 输出 | workAmount | Crafting | 研究 |
|---|---|---|---|---|---|
| MakeDepletedUraniumAlloy | Uranium×10 | DepletedUranium×10 | 1700 | 11 | Metals_C3 |
| MakeFerronickelAlloy | Steel×15 + NickelBar×5 | SteelBar×20 | 1800 | 11 | Metals_C3 |
| MakeCupronickelAlloy | CopperBar×24 + NickelBar×6 | CupronickelAlloy×30 | 1600 | 12 | Metals_C2 |
| MakeAlnicoAlloy | Steel×8 + AluminiumBar×3 + Cobalt×3 + NickelBar×6 | AlnicoAlloy×20 | 1650 | 13 | Metals_C3 |
| MakeSilicon_Set | SandResource×200 | Silicon×20 | 1800 | 5 | Metals_C1 |

## 3. 决策

1. **类别归属以 HSK 实际 ThingDef 为准**（路线图第 4 节注明"spec 时逐一对照 HSK"，路线图粗略映射作废）：
   - 镍/钴/铬 → `MMT_RARBar`；钨 → `MMT_RARBar`+`MMT_USLDHBar`；钛 → `MMT_RARBar`+`MMT_USLDBar`
   - 司太立/镍钛诺/斯坦 → `MMT_USLDBar`；钴硬质 → `MMT_USLDBar`+`MMT_USLDHBar`
   - 梅尔基奥 → `MMT_HCMBar`+`MMT_SLDBar`；亚力可 → `MMT_HCMBar`+`MMT_USLDBar`；贫铀 → `MMT_RADBar`+`MMT_USLDHBar`
   - 铁钛（原版 Plasteel）B1 patch 已处理，不动
   - 安全性已验证：全部 RecipeDefs 的 `filter.categories` 无 `ResourcesRaw` 引用，合金移出该类不影响既有配方
2. **不新增研究**：HSK Metals_C2/C3 映射到 MMT 既有研究——梅尔基奥→`MMT_AdvancedMetalRefining`；斯坦/亚力可/贫铀→`MMT_AdvancedAlloys`；硅批量→`MMT_ElectricSmelting`
3. **硅批量配方采纳**（路线图可选项），defName `MMT_MakeSilicon_Bulk`，放入 `MMT_Recipes_Electric.xml` 归入 electric 套件（与 `MMT_ExtractSilicon_Electric` 同文件，避免 advanced-alloys 套件的资源名集合扩到玻璃/电子文件）
4. **stuffProps 只保留原版 stat**：HSK 的 `MeleePenetrationFactor`/`WorkTableWorkSpeedFactor` 是 SK StatDef，剔除；stuff categories 仅 `Metallic`（SolidMetallic/StrongMetallic/RuggedMetallic/HeavyMetallic 在 1.7 不存在，validate 已有禁用断言）
5. **中文 label**：HSK 克隆无简中翻译，按路线图命名：贫铀合金/斯坦合金/梅尔基奥合金/亚力可合金
6. **贴图**：复用 `Things/Item/MMT/MMT_Ingot/MMT_Ingot` + HSK color 着色，不新建贴图
7. **MMT_ALIAS 不改**：新资源后缀与 HSK 同名（DepletedUranium/SteelBar/CupronickelAlloy/AlnicoAlloy），`mmtToHsk()` 兜底直接匹配
8. **贫铀 useHitPoints**：HSK 无此字段，但 MMT 模式（stuffProps 资源均带 `useHitPoints=true`，validate 联动断言）优先，MMT 侧带上

## 4. 新增资源字段表

统一模式：`ParentName="ResourceBase"`、Graphic_StackCount、Metal_Drop 音效、tickerType Rare、statBases 仅 MarketValue/DeteriorationRate=0/Flammability=0/Mass、terrainAffordanceNeeded Medium。

| defName | label | MarketValue | Mass | stackLimit | color | thingCategories | statFactors（MaxHP/Beauty/WTM/WTB/Mass/MV/Door） |
|---|---|---|---|---|---|---|---|
| MMT_DepletedUranium | 贫铀合金 | 18 | 0.5 | 75 | (20,94,30) | MMT_RADBar, MMT_USLDHBar | 3.05/0/2.3/2.3/1.6/0.7/0.75 |
| MMT_SteelBar | 斯坦合金 | 8 | 0.4 | 150 | (120,147,131) | MMT_USLDBar | 2.2/1.4/1.5/1.5/1.3/0.7/0.8 |
| MMT_CupronickelAlloy | 梅尔基奥合金 | 8 | 0.4 | 150 | (168,157,125) | MMT_HCMBar, MMT_SLDBar | 2.1/1.6/1.4/1.4/1.15/0.7/0.75 |
| MMT_AlnicoAlloy | 亚力可合金 | 11 | 0.25 | 150 | (210,210,210) | MMT_HCMBar, MMT_USLDBar | 2.6/1.35/1.7/1.7/1.15/0.7/1.0 |

## 5. 验证

扩展现有 `advanced-alloys` 套件（不新建）：资源 8→12、配方 9→13、类别断言改逐条；electric 套件补 `MMT_MakeSilicon_Bulk`；`VANILLA_THINGS` 补 `Uranium`；EAF 配方列表断言同步。收尾 `validate all` 全绿 + `dotnet build` 0 错误 + 重跑对比文档。
