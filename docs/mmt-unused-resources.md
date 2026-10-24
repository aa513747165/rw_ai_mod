# MMT 闲置资源梳理（后继更新一 · 需求 8）

> 生成方式：扫描 `1.7/Defs` 全部资源 ThingDef，统计其在配方 ingredients/products、建筑 costList、
> 燃料 filter 中的引用；再核对 stuffProps（建筑材料）/药品/医疗/可食用等直接用途。
> 结论分三档：**有直接用途**（不算闲置）、**有产出但无后继消费**、**孤儿**（无产出无消费）。
> 本报告只梳理不删除；是否清理或补后继配方由作者决定。

## 一、有直接用途（可忽略）

这些东西没有出现在任何配方/成本里，但本身能被直接使用，属正常：

| defName | 名称 | 用途 |
|---|---|---|
| MMT_HerbalTea / MMT_ActivatedCarbon / MMT_BerserkHerb / MMT_Methadone / MMT_Ibuprofen | 草药茶/活性炭丸/狂暴草/美沙酮/止痛药 | 药品（可直接服用） |
| MMT_Bandagekitcrude / MMT_HerbMedicine / MMT_BasicMedicineKit | 粗制绷带/草药包/基础医药包 | 医疗用品 |
| MMT_Flaxcloth / MMT_HempCloth / MMT_Velour / MMT_Kevlar / MMT_ChitinPlating | 亚麻布/大麻布/丝绒/凯夫拉/甲壳板 | stuff 织物（做衣服） |
| MMT_CupronickelAlloy / MMT_LeadBar / MMT_BetaPoly | 梅尔基奥合金/铅合金/β聚合物 | stuff 金属材料（建造） |
| MMT_ClayBrick / MMT_Ceramics | 粘土砖/陶瓷 | stuff 砖/石材（建造） |
| MMT_BioferriteAlloy | 生物铁合金 | stuff 金属材料（建造），有配方产出 |

## 二、有产出配方但没有任何用途（建议补后继配方或清理）

| defName | 名称 | 产出于 |
|---|---|---|
| MMT_Dirt | 土堆 | MMT_MakeDirt（手工/电动 3 条） |
| MMT_ReinforcedGlass | 强化玻璃块 | MMT_MakeReinforcedGlass |
| MMT_MetalCan | 金属罐 | MMT_MakeMetalCan |
| MMT_OxidizedChemfuel | 氧化化合燃料 | MMT_MakeOxidizedChemfuel |
| MMT_RobotParts | 机器人部件 | MMT_MakeRobotParts |
| MMT_MineralSonarModule | 矿物声纳模块 | MMT_MakeMineralSonarModule |
| MMT_Tallow | 兽脂 | MMT_MakeTallow |
| MMT_PaintingSupplies | 绘画用品 | MMT_CookPaint / MMT_CookPaintDye |

## 三、孤儿资源（无产出也无消费）

| defName | 名称 | 所在文件 |
|---|---|---|
| MMT_RawCaoutchouc | 天然橡胶 | MMT_Items_Resource_Crops.xml（无作物产出它，可能是作物收割产物挂载遗漏） |

## 四、遗留建筑

| defName | 名称 | 说明 |
|---|---|---|
| MMT_Hopper | 矿料进料口 | 自动研磨机/筛子已改为内置燃料仓（原料即燃料），进料口不再有配对的 `wantsHopperAdjacent` 建筑。为兼容含进料口的旧存档，**保留 def 不删**；新存档中已无法摆放（PlaceWorker_NextToHopperAccepter 无目标）。 |
