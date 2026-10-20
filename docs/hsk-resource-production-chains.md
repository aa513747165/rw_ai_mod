# HSK 官方核心模块资源生产链路（简表）

- 数据源：`Hardcore-SK` 分支 `1.7-super-unstable`，提交 `b99b17d2`；由 `tools/build_resource_chains.js` 从 [详细版](hsk-resource-production-paths.md) 自动生成。
- 范围：`Core_SK` 与官方 `Core_SK_*` 模块。
- 命名：`中文名(defName)`；中文名取自 HSK 1.6 中文翻译包（`Hardcore-SK/1.6`，Core_SK 主翻译包优先），未覆盖时取游戏本体简体中文语言包（Core + DLC），仍无翻译时保留英文 label。
- 链路格式：`输入A×数量 + 输入B×数量 -> 产出×数量 @ 建筑 ｜ 关键数值 ｜ 前置 ｜ `配方defName` ｜ 备注/条件`。
- 多产物配方会在每个产出资源小节下重复出现；`[木质]` 等方括号表示材料类别；`×按矿点规则` 表示数量由矿点营地规则决定。

## 1. 汇总

| 类别 | 链路数 |
| --- | --- |
| 采集与自然产出 | 208 |
| 工作台配方 | 302 |
| 发酵/缓处理配方 | 68 |
| **合计** | 578 |

## 2. 采集与自然产出

### 2.1 矿脉开采（29）

- 铅矾矿(MineableLead) -> 铅矾矿(Anglesite)×75 ｜ 散布常见度=0.8，矿团规模=6~13
- 铝核陨石(AluminiumBoulder) -> 铝土矿(Aluminium)×100
- 铝土矿(MineableAluminium) -> 铝土矿(Aluminium)×75 ｜ 散布常见度=0.65，矿团规模=11~19
- 煤矿(MineableCoal) -> 原煤(Coal)×75 ｜ 散布常见度=1.2，矿团规模=11~18
- 冰石矿(MineableColdstone) -> 冰石(Coldstone)×50 ｜ 散布常见度=0.3，矿团规模=4~7
- 铜核陨石(CopperBoulder) -> 铜矿(Copper)×100
- 铜矿(MineableCopper) -> 铜矿(Copper)×75 ｜ 散布常见度=0.95，矿团规模=15~25
- 萤石矿(MineableGlowstone) -> 萤石(Glowstone)×50 ｜ 散布常见度=0.35，矿团规模=3~6
- 金矿(MineableGold) -> 金矿(Gold)×50 ｜ 散布常见度=0.15，矿团规模=3~9 ｜ PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Buildings/Buildings_Natural.xml
- 钛铁矿(MineableTitanium) -> 钛铁矿(Ilmenite)×75 ｜ 散布常见度=0.25，矿团规模=3~9
- 巨型陨石(BigIronBoulder) -> 铁矿石(Iron)×240
- 铁核陨石(IronBoulder) -> 铁矿石(Iron)×120
- 铁矿(MineableSteel) -> 铁矿石(Iron)×35 ｜ 散布常见度=1.1，矿团规模=20~40 ｜ PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Buildings/Buildings_Natural.xml
- 铁核陨石(MineralBoulder) -> 铁矿石(Iron)×75
- 镍矿矿藏(MineableNickel) -> 镍矿(Nickel)×50 ｜ 散布常见度=0.4，矿团规模=5~10
- 硝石矿(MineableNitre) -> 硝石(Nitre)×75 ｜ 散布常见度=1.05，矿团规模=8~16
- 压实的盐(MineableSalt) -> 盐(Salt)×150 ｜ 散布常见度=1.2，矿团规模=5~10
- 银矿(MineableSilver) -> 白银(Silver)×250 ｜ 散布常见度=0.25，矿团规模=7~15 ｜ PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Buildings/Buildings_Natural.xml
- 银核陨石(SilverBoulder) -> 白银(Silver)×100
- 锡矿(MineableTin) -> 锡矿(Tin)×75 ｜ 散布常见度=1.1，矿团规模=9~17
- 铀矿带(MineableUranium) -> 铀矿(Uranium)×40 ｜ 散布常见度=0.18，矿团规模=4~9 ｜ PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Resources/Items_Resource_Radioactive.xml
- 铀核陨石(UraniumBoulder) -> 铀矿(Uranium)×75
- 黑钨矿(MineableTungsten) -> 黑钨矿(Wolframite)×50 ｜ 散布常见度=0.15，矿团规模=3~9
- 翡翠矿(MineableJade) -> 翡翠(Jade)×75 ｜ 散布常见度=0.25，矿团规模=7~12 ｜ PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Buildings/Buildings_Natural.xml
- 花岗岩陨石(GraniteBoulder) -> 花岗岩块(ChunkGranite)×1
- 石灰岩陨石(LimestoneBoulder) -> 石灰岩块(ChunkLimestone)×1
- 大理石陨石(MarbleBoulder) -> 大理石块(ChunkMarble)×1
- 砂岩陨石(SandstoneBoulder) -> 砂岩块(ChunkSandstone)×1
- 板岩陨石(SlateBoulder) -> 板岩块(ChunkSlate)×1

### 2.2 深钻井（19）

- 深钻井 -> 铅矾矿(Anglesite)×20 ｜ 每批规模=32~78，常见度=1.2
- 深钻井 -> 铝土矿(Aluminium)×25 ｜ 每批规模=40~80，常见度=1.35
- 深钻井 -> 原煤(Coal)×50 ｜ 每批规模=95~150，常见度=2.0
- 深钻井 -> 冰石(Coldstone)×15 ｜ 每批规模=15~35，常见度=1
- 深钻井 -> 铜矿(Copper)×40 ｜ 每批规模=72~145，常见度=1.5
- 深钻井 -> 萤石(Glowstone)×15 ｜ 每批规模=15~35，常见度=1
- 深钻井 -> 金矿(Gold)×10 ｜ 每批规模=1~12，常见度=0.6 ｜ PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Resources/Items_Resource_Ore.xml
- 深钻井 -> 钛铁矿(Ilmenite)×15 ｜ 每批规模=25~58，常见度=1.1
- 深钻井 -> 铁矿石(Iron)×35 ｜ 每批规模=80~175，常见度=3.2
- 深钻井 -> 镍矿(Nickel)×20 ｜ 每批规模=32~68，常见度=1.2
- 深钻井 -> 硝石(Nitre)×30 ｜ 每批规模=42~85，常见度=1.6
- 深钻井 -> 泥炭(Peat)×50 ｜ 每批规模=45~95，常见度=0.8
- 深钻井 -> 碎石(CrushedStone)×30
- 深钻井 -> 盐(Salt)×40 ｜ 每批规模=60~120，常见度=1.8
- 深钻井 -> 沙(SandResource)×50
- 深钻井 -> 白银(Silver)×20 ｜ 每批规模=20~40，常见度=1 ｜ PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Resources/Items_Resource_Ore.xml
- 深钻井 -> 锡矿(Tin)×40 ｜ 每批规模=58~115，常见度=1.6
- 深钻井 -> 铀矿(Uranium)×10 ｜ 每批规模=15~35，常见度=0.6 ｜ PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Resources/Items_Resource_Radioactive.xml
- 深钻井 -> 黑钨矿(Wolframite)×10 ｜ 每批规模=5~25，常见度=0.6

### 2.3 野外矿点（11）

- 野外矿点(WorkSite) -> 铅矾矿(Anglesite)×按矿点规则 ｜ 权重=3.5 ｜ Ideology 工作台/矿点营地产出 ｜ PatchApplied=Mods/Core_SK_DLCModule/Ideology/Patches/SK_Patches_Ideology_WorkSite.xml
- 野外矿点(WorkSite) -> 铝土矿(Aluminium)×按矿点规则 ｜ 权重=2 ｜ Ideology 工作台/矿点营地产出 ｜ PatchApplied=Mods/Core_SK_DLCModule/Ideology/Patches/SK_Patches_Ideology_WorkSite.xml
- 野外矿点(WorkSite) -> 铜矿(Copper)×按矿点规则 ｜ 权重=3 ｜ Ideology 工作台/矿点营地产出 ｜ PatchApplied=Mods/Core_SK_DLCModule/Ideology/Patches/SK_Patches_Ideology_WorkSite.xml
- 野外矿点(WorkSite) -> 金矿(Gold)×按矿点规则 ｜ 权重=0.75 ｜ Ideology 工作台/矿点营地产出 ｜ PatchApplied=Mods/Core_SK_DLCModule/Ideology/Patches/SK_Patches_Ideology_WorkSite.xml
- 野外矿点(WorkSite) -> 钛铁矿(Ilmenite)×按矿点规则 ｜ 权重=0.7 ｜ Ideology 工作台/矿点营地产出 ｜ PatchApplied=Mods/Core_SK_DLCModule/Ideology/Patches/SK_Patches_Ideology_WorkSite.xml
- 野外矿点(WorkSite) -> 镍矿(Nickel)×按矿点规则 ｜ 权重=1 ｜ Ideology 工作台/矿点营地产出 ｜ PatchApplied=Mods/Core_SK_DLCModule/Ideology/Patches/SK_Patches_Ideology_WorkSite.xml
- 野外矿点(WorkSite) -> 白银(Silver)×按矿点规则 ｜ 权重=0.95 ｜ Ideology 工作台/矿点营地产出 ｜ PatchApplied=Mods/Core_SK_DLCModule/Ideology/Patches/SK_Patches_Ideology_WorkSite.xml
- 野外矿点(WorkSite) -> 锡矿(Tin)×按矿点规则 ｜ 权重=2.5 ｜ Ideology 工作台/矿点营地产出 ｜ PatchApplied=Mods/Core_SK_DLCModule/Ideology/Patches/SK_Patches_Ideology_WorkSite.xml
- 野外矿点(WorkSite) -> 黑钨矿(Wolframite)×按矿点规则 ｜ 权重=0.65 ｜ Ideology 工作台/矿点营地产出 ｜ PatchApplied=Mods/Core_SK_DLCModule/Ideology/Patches/SK_Patches_Ideology_WorkSite.xml
- 野外矿点(WorkSite) -> 普通零部件(ComponentIndustrial)×按矿点规则 ｜ 权重=0.7 ｜ Ideology 工作台/矿点营地产出 ｜ PatchApplied=Mods/Core_SK_DLCModule/Ideology/Patches/SK_Patches_Ideology_WorkSite.xml
- 野外矿点(WorkSite) -> 钢铁(Steel)×按矿点规则 ｜ 权重=4 ｜ Ideology 工作台/矿点营地产出 ｜ PatchApplied=Mods/Core_SK_DLCModule/Ideology/Patches/SK_Patches_Ideology_WorkSite.xml

### 2.4 摧毁残留（50）

- 铝核陨石(AluminiumBoulder) -> 铝土矿(Aluminium)×50 ｜ 摧毁来源物后获得
- 铜核陨石(CopperBoulder) -> 铜矿(Copper)×50 ｜ 摧毁来源物后获得
- 巨型陨石(BigIronBoulder) -> 铁矿石(Iron)×100 ｜ 摧毁来源物后获得
- 铁核陨石(IronBoulder) -> 铁矿石(Iron)×60 ｜ 摧毁来源物后获得
- 铁核陨石(MineralBoulder) -> 铁矿石(Iron)×35 ｜ 摧毁来源物后获得
- 银核陨石(SilverBoulder) -> 白银(Silver)×50 ｜ 摧毁来源物后获得
- 古代反应堆（虫巢）(NuclearHive) -> 铀矿(Uranium)×95 ｜ 摧毁来源物后获得
- 载客穿梭机(PassengerShuttle) -> 铀矿(Uranium)×50 ｜ 摧毁来源物后获得 ｜ PatchApplied=Mods/Core_SK_DLCModule/Odyssey/Patches/ThingDefs_Buildings/Buildings_Misc_Odyssey.xml
- 穿梭机(Shuttle) -> 铀矿(Uranium)×50 ｜ 摧毁来源物后获得 ｜ PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Buildings/Buildings_Exotic.xml; PatchApplied=Mods/Core_SK_CoreModule/Patches/Core/ThingDefs_Buildings/Buildings_Exotic.xml
- 铀核陨石(UraniumBoulder) -> 铀矿(Uranium)×35 ｜ 摧毁来源物后获得
- 敌舰(EnemyShipPart) -> 普通零部件(ComponentIndustrial)×20 ｜ 摧毁来源物后获得
- 古代反应堆（虫巢）(NuclearHive) -> 普通零部件(ComponentIndustrial)×35 ｜ 摧毁来源物后获得
- 载客穿梭机(PassengerShuttle) -> 普通零部件(ComponentIndustrial)×4 ｜ 摧毁来源物后获得 ｜ PatchApplied=Mods/Core_SK_DLCModule/Odyssey/Patches/ThingDefs_Buildings/Buildings_Misc_Odyssey.xml
- 穿梭机(Shuttle) -> 普通零部件(ComponentIndustrial)×4 ｜ 摧毁来源物后获得 ｜ PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Buildings/Buildings_Exotic.xml; PatchApplied=Mods/Core_SK_CoreModule/Patches/Core/ThingDefs_Buildings/Buildings_Exotic.xml
- 坠毁的穿梭机(ShuttleCrashed) -> 普通零部件(ComponentIndustrial)×1 ｜ 摧毁来源物后获得 ｜ PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Buildings/Buildings_Exotic.xml; PatchApplied=Mods/Core_SK_CoreModule/Patches/Core/ThingDefs_Buildings/Buildings_Exotic.xml
- 敌舰(EnemyShipPart) -> 电子元件(ElectronicComponents)×8 ｜ 摧毁来源物后获得
- 载客穿梭机(PassengerShuttle) -> 电子元件(ElectronicComponents)×5 ｜ 摧毁来源物后获得 ｜ PatchApplied=Mods/Core_SK_DLCModule/Odyssey/Patches/ThingDefs_Buildings/Buildings_Misc_Odyssey.xml
- 穿梭机(Shuttle) -> 电子元件(ElectronicComponents)×5 ｜ 摧毁来源物后获得 ｜ PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Buildings/Buildings_Exotic.xml; PatchApplied=Mods/Core_SK_CoreModule/Patches/Core/ThingDefs_Buildings/Buildings_Exotic.xml
- 坠毁的穿梭机(ShuttleCrashed) -> 电子元件(ElectronicComponents)×3 ｜ 摧毁来源物后获得 ｜ PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Buildings/Buildings_Exotic.xml; PatchApplied=Mods/Core_SK_CoreModule/Patches/Core/ThingDefs_Buildings/Buildings_Exotic.xml
- 敌舰(EnemyShipPart) -> 机械组件(Mechanism)×10 ｜ 摧毁来源物后获得
- 古代反应堆（虫巢）(NuclearHive) -> 机械组件(Mechanism)×20 ｜ 摧毁来源物后获得
- 载客穿梭机(PassengerShuttle) -> 机械组件(Mechanism)×14 ｜ 摧毁来源物后获得 ｜ PatchApplied=Mods/Core_SK_DLCModule/Odyssey/Patches/ThingDefs_Buildings/Buildings_Misc_Odyssey.xml
- 穿梭机(Shuttle) -> 机械组件(Mechanism)×14 ｜ 摧毁来源物后获得 ｜ PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Buildings/Buildings_Exotic.xml; PatchApplied=Mods/Core_SK_CoreModule/Patches/Core/ThingDefs_Buildings/Buildings_Exotic.xml
- 坠毁的穿梭机(ShuttleCrashed) -> 机械组件(Mechanism)×2 ｜ 摧毁来源物后获得 ｜ PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Buildings/Buildings_Exotic.xml; PatchApplied=Mods/Core_SK_CoreModule/Patches/Core/ThingDefs_Buildings/Buildings_Exotic.xml
- 飞船部件（枯萎）(DefoliatorShipPart) -> 高级零部件(ComponentSpacer)×4 ｜ 摧毁来源物后获得 ｜ PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Buildings/Buildings_Exotic.xml
- 敌舰(EnemyShipPart) -> 高级零部件(ComponentSpacer)×5 ｜ 摧毁来源物后获得
- 飞船部件（心灵）(PsychicDronerShipPart) -> 高级零部件(ComponentSpacer)×4 ｜ 摧毁来源物后获得 ｜ PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Buildings/Buildings_Exotic.xml
- 敌舰(EnemyShipPart) -> 钢铁(Steel)×150 ｜ 摧毁来源物后获得
- 古代反应堆（虫巢）(NuclearHive) -> 钢铁(Steel)×155 ｜ 摧毁来源物后获得
- 载客穿梭机(PassengerShuttle) -> 钢铁(Steel)×50 ｜ 摧毁来源物后获得 ｜ PatchApplied=Mods/Core_SK_DLCModule/Odyssey/Patches/ThingDefs_Buildings/Buildings_Misc_Odyssey.xml
- 穿梭机(Shuttle) -> 钢铁(Steel)×50 ｜ 摧毁来源物后获得 ｜ PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Buildings/Buildings_Exotic.xml; PatchApplied=Mods/Core_SK_CoreModule/Patches/Core/ThingDefs_Buildings/Buildings_Exotic.xml
- 坠毁的穿梭机(ShuttleCrashed) -> 钢铁(Steel)×7 ｜ 摧毁来源物后获得 ｜ PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Buildings/Buildings_Exotic.xml; PatchApplied=Mods/Core_SK_CoreModule/Patches/Core/ThingDefs_Buildings/Buildings_Exotic.xml
- 古代反应堆（虫巢）(NuclearHive) -> 电线(Wire)×75 ｜ 摧毁来源物后获得
- 载客穿梭机(PassengerShuttle) -> 电线(Wire)×40 ｜ 摧毁来源物后获得 ｜ PatchApplied=Mods/Core_SK_DLCModule/Odyssey/Patches/ThingDefs_Buildings/Buildings_Misc_Odyssey.xml
- 穿梭机(Shuttle) -> 电线(Wire)×40 ｜ 摧毁来源物后获得 ｜ PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Buildings/Buildings_Exotic.xml; PatchApplied=Mods/Core_SK_CoreModule/Patches/Core/ThingDefs_Buildings/Buildings_Exotic.xml
- 坠毁的穿梭机(ShuttleCrashed) -> 电线(Wire)×18 ｜ 摧毁来源物后获得 ｜ PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Buildings/Buildings_Exotic.xml; PatchApplied=Mods/Core_SK_CoreModule/Patches/Core/ThingDefs_Buildings/Buildings_Exotic.xml
- 飞船部件（枯萎）(DefoliatorShipPart) -> 伪人格核心(AIPersonaCore)×1 ｜ 摧毁来源物后获得 ｜ PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Buildings/Buildings_Exotic.xml
- 飞船部件（心灵）(PsychicDronerShipPart) -> 伪人格核心(AIPersonaCore)×1 ｜ 摧毁来源物后获得 ｜ PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Buildings/Buildings_Exotic.xml
- 蚀叶器(Defoliator) -> killedLeavings(killedLeavings)×钛铁合金(Plasteel)=50, 贫铀合金(DepletedUranium)=35, 碳纤维锭(CarbonAlloy)=15, 先进零部件(ComponentAdvanced)=8, 集成电路(Electronics)=4, 钢渣块(ChunkSlagSteel)=8 ｜ 摧毁来源物后获得 ｜ PatchApplied=Mods/Core_SK_DLCModule/Royalty/Patches/ThingDefs_Buildings/MechClusterBuildingCostPatch.xml
- 机械孕育器(MechAssembler) -> killedLeavings(killedLeavings)×钢渣块(ChunkSlagSteel)=4 ｜ 摧毁来源物后获得 ｜ PatchApplied=Mods/Core_SK_DLCModule/Royalty/Patches/ThingDefs_Buildings/MechClusterBuildingCostPatch.xml
- 机械舱(MechCapsule) -> killedLeavings(killedLeavings)×钢渣块(ChunkSlagSteel)=3 ｜ 摧毁来源物后获得 ｜ PatchApplied=Mods/Core_SK_DLCModule/Royalty/Patches/ThingDefs_Buildings/MechClusterBuildingCostPatch.xml
- 机械低角护盾(ShieldGeneratorBullets) -> killedLeavings(killedLeavings)×钛铁合金(Plasteel)=10, 贫铀合金(DepletedUranium)=10, 碳纤维锭(CarbonAlloy)=5, 先进零部件(ComponentAdvanced)=3, 集成电路(Electronics)=1, 钢渣块(ChunkSlagSteel)=4 ｜ 摧毁来源物后获得 ｜ PatchApplied=Mods/Core_SK_DLCModule/Royalty/Patches/ThingDefs_Buildings/MechClusterBuildingCostPatch.xml
- 机械高角护盾(ShieldGeneratorMortar) -> killedLeavings(killedLeavings)×钛铁合金(Plasteel)=10, 贫铀合金(DepletedUranium)=10, 碳纤维锭(CarbonAlloy)=5, 先进零部件(ComponentAdvanced)=3, 集成电路(Electronics)=1, 钢渣块(ChunkSlagSteel)=4 ｜ 摧毁来源物后获得 ｜ PatchApplied=Mods/Core_SK_DLCModule/Royalty/Patches/ThingDefs_Buildings/MechClusterBuildingCostPatch.xml
- 太空无人机(Spacedrone) -> killedLeavings(killedLeavings)×钛铁合金(Plasteel)=50, 钢渣块(ChunkSlagSteel)=4, 先进零部件(ComponentAdvanced)=3, 集成电路(Electronics)=1 ｜ 摧毁来源物后获得 ｜ PatchApplied=Mods/Core_SK_DLCModule/Ideology/Patches/ThingDefs_Buildings/Buildings_Exotic.xml
- 自动电荷冲击炮塔(Turret_AutoChargeBlaster) -> killedLeavings(killedLeavings)×钛铁合金(Plasteel)=35, 贫铀合金(DepletedUranium)=10, 碳纤维锭(CarbonAlloy)=10, 先进零部件(ComponentAdvanced)=4, 钢渣块(ChunkSlagSteel)=3 ｜ 摧毁来源物后获得 ｜ PatchApplied=Mods/Core_SK_DLCModule/Royalty/Patches/ThingDefs_Buildings/MechClusterBuildingCostPatch.xml; PatchApplied=Mods/Core_SK_DLCModule/Royalty/Patches/ThingDefs_Buildings/MechClusterBuildingStatPatch.xml
- 自动地狱火加农炮塔(Turret_AutoInferno) -> killedLeavings(killedLeavings)×钛铁合金(Plasteel)=40, 贫铀合金(DepletedUranium)=20, 碳纤维锭(CarbonAlloy)=10, 先进零部件(ComponentAdvanced)=6, 钢渣块(ChunkSlagSteel)=3 ｜ 摧毁来源物后获得 ｜ PatchApplied=Mods/Core_SK_DLCModule/Royalty/Patches/ThingDefs_Buildings/MechClusterBuildingCostPatch.xml; PatchApplied=Mods/Core_SK_DLCModule/Royalty/Patches/ThingDefs_Buildings/MechClusterBuildingStatPatch.xml
- 小型爆能炮塔(Turret_AutoMiniTurret) -> killedLeavings(killedLeavings)×钢渣块(ChunkSlagSteel)=2 ｜ 摧毁来源物后获得 ｜ PatchApplied=Mods/Core_SK_DLCModule/Royalty/Patches/ThingDefs_Buildings/MechClusterBuildingCostPatch.xml; PatchApplied=Mods/Core_SK_DLCModule/Royalty/Patches/ThingDefs_Buildings/MechClusterBuildingStatPatch.xml
- 自动迫击炮(Turret_AutoMortar) -> killedLeavings(killedLeavings)×钛铁合金(Plasteel)=40, 贫铀合金(DepletedUranium)=20, 碳纤维锭(CarbonAlloy)=10, 先进零部件(ComponentAdvanced)=6, 钢渣块(ChunkSlagSteel)=3 ｜ 摧毁来源物后获得 ｜ PatchApplied=Mods/Core_SK_DLCModule/Royalty/Patches/ThingDefs_Buildings/MechClusterBuildingCostPatch.xml; PatchApplied=Mods/Core_SK_DLCModule/Royalty/Patches/ThingDefs_Buildings/MechClusterBuildingStatPatch.xml
- 载客穿梭机(PassengerShuttle) -> 穿梭机引擎(ShuttleEngine)×1 ｜ 摧毁来源物后获得 ｜ PatchApplied=Mods/Core_SK_DLCModule/Odyssey/Patches/ThingDefs_Buildings/Buildings_Misc_Odyssey.xml
- 穿梭机(Shuttle) -> 穿梭机引擎(ShuttleEngine)×1 ｜ 摧毁来源物后获得 ｜ PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Buildings/Buildings_Exotic.xml; PatchApplied=Mods/Core_SK_CoreModule/Patches/Core/ThingDefs_Buildings/Buildings_Exotic.xml

### 2.5 植物收获（50）

- 竹(Plant_TreeBamboo) -> 竹节(Bamboo)×10 ｜ 生长天数=4 ｜ 播种标签: Ground, Hydroponic ｜ PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Plants/Plants_Cultivated_Manufacture.xml
- 红木(PlantTreeRedwood) -> 红木(RedWoodLog)×225 ｜ 生长天数=85
- 木菇(Plant_Timbershroom) -> 原木(WoodLog)×60 ｜ 播种标签: Fungiponics ｜ PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Plants/Plants_Cave.xml
- 灌木(Plant_Bush) -> 柴火(Kindling)×5 ｜ 生长天数=5 ｜ 播种标签: Ground ｜ PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Plants/Plants_Bushes.xml
- 杜鹃(Plant_Rhododendron) -> 柴火(Kindling)×6 ｜ 生长天数=7 ｜ 播种标签: Ground
- 可可树(Plant_TreeCocoa) -> 可可豆(Rawcocoa)×16 ｜ 生长天数=9 ｜ 播种标签: Ground, Hydroponic ｜ PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Plants/Plants_Cultivated_Fruits.xml
- 咖啡树(Plant_Coffee) -> 咖啡果(RawCoffee)×7 ｜ 生长天数=8 ｜ 播种标签: Ground, Hydroponic
- 香蒲(Plant_Bulrush) -> 干草(Hay)×8 ｜ PatchApplied=Mods/Core_SK_DLCModule/Odyssey/Patches/ThingDefs_Plants/Plants_Water.xml
- 草(Plant_Grass) -> 干草(Hay)×3 ｜ 生长天数=2.3 ｜ PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Plants/Plants_Wild.xml
- 干草(Plant_Haygrass) -> 干草(Hay)×8 ｜ 生长天数=2.5 ｜ 播种标签: Ground, Hydroponic ｜ PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Plants/Plants_Wild.xml
- 芦苇(Plant_Reeds) -> 干草(Hay)×3 ｜ PatchApplied=Mods/Core_SK_DLCModule/Odyssey/Patches/ThingDefs_Plants/Plants_Water.xml
- 高草(Plant_TallGrass) -> 干草(Hay)×6 ｜ 生长天数=2.8 ｜ PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Plants/Plants_Wild.xml
- 黄草(Plant_YellowGrass) -> 干草(Hay)×8 ｜ PatchApplied=Mods/Core_SK_DLCModule/Odyssey/Patches/ThingDefs_Plants/Plants_Grass.xml
- 金丝桃(Plant_Hypericum) -> 金丝桃(Hypericum)×4 ｜ 生长天数=6 ｜ 播种标签: Hydroponic
- 薄荷(Plant_Mint) -> 薄荷叶(MintLeaves)×5 ｜ 生长天数=6.5 ｜ 播种标签: Hydroponic
- 洋葱(Plant_Onion) -> 洋葱(Rawonion)×4 ｜ 生长天数=4.5 ｜ 播种标签: Ground, Hydroponic
- 甘蔗(Plant_Sugarcane) -> 甘蔗(Rawsugarcane)×9 ｜ 生长天数=5 ｜ 播种标签: Ground, Hydroponic
- 茶树(Plant_Tea) -> 茶叶(RawTea)×10 ｜ 生长天数=7 ｜ 播种标签: Ground, Hydroponic
- 烟草(Plant_Tobacco) -> 烟叶(RawTobacco)×7 ｜ 生长天数=6 ｜ 播种标签: Ground, Hydroponic
- 小麦(Plantwheat) -> 小麦(Rawwheat)×18 ｜ 生长天数=5.2 ｜ 播种标签: Ground, Hydroponic
- 野玫瑰(Plant_WildRose) -> 野玫瑰(WildRose)×6 ｜ 生长天数=8 ｜ 播种标签: Hydroponic
- 芦荟植株(Plant_Aloe) -> 芦荟叶(aloe)×4 ｜ 生长天数=4.7 ｜ 播种标签: Ground, Hydroponic
- 闪帽菇(Agarilux) -> 闪帽菇(GleamcapStem)×4 ｜ 生长天数=9 ｜ 播种标签: Fungiponics ｜ PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Plants/Plants_Cave.xml
- 水藻(PlantAlgae) -> 水藻(RawAlgae)×5 ｜ 生长天数=4
- 苹果树(Plant_TreeApple) -> 苹果(Rawapple)×32 ｜ 生长天数=18.6 ｜ 播种标签: Ground
- 香蕉树(Plant_TreeBanana) -> 香蕉(Rawbanana)×30 ｜ 生长天数=14 ｜ 播种标签: Ground
- 豆子(Plantbean) -> 豆荚(Rawbean)×20 ｜ 生长天数=9.2 ｜ 播种标签: Ground, Hydroponic
- 橡胶树(Plant_TreeHevea) -> 天然橡胶(RawCaoutchouc)×20 ｜ 生长天数=6.5 ｜ 播种标签: Ground
- 胡萝卜(PlantCarrot) -> 胡萝卜(RawCarrots)×9 ｜ 生长天数=5.4 ｜ 播种标签: Ground, Hydroponic
- 椰子树(Plant_TreePalm) -> 椰子(RawCoconut)×22 ｜ 生长天数=12 ｜ PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Plants/Plants_Trees.xml
- 巨叶(Plant_GiantLeaf) -> 巨叶(RawGiantLeaf)×5 ｜ 生长天数=6
- 炽光菇(Glowstool) -> 炽光菇(RawGlowbulb)×5 ｜ 生长天数=12 ｜ 播种标签: Fungiponics ｜ PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Plants/Plants_Cave.xml
- 葡萄(Plant_TreeGrape) -> 葡萄(Rawgrape)×16 ｜ 生长天数=11 ｜ 播种标签: Ground, Hydroponic
- 中性花(Plant_NeutroFlower) -> 中性花瓣(NeutroPetals)×5 ｜ 生长天数=6.2 ｜ 播种标签: Hydroponic
- 橙树(Plant_TreeOrange) -> 橙(Raworange)×40 ｜ 生长天数=13 ｜ 播种标签: Ground
- 桃树(Plant_TreePeach) -> 桃子(Rawpeach)×35 ｜ 生长天数=18 ｜ 播种标签: Ground
- 蘑菇(Plant_Mushroom) -> 蘑菇(Rawmushroom)×12 ｜ 生长天数=5.1 ｜ 播种标签: Ground, Hydroponic
- 棉花(Plant_Cotton) -> 原棉(RawCotton)×8 ｜ 生长天数=5.6 ｜ 播种标签: Ground, Hydroponic ｜ PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Plants/Plants_Cultivated_Manufacture.xml
- 恶魔菇(Plant_Devilstrand) -> 生恶魔菇(RawDevilstrand)×8 ｜ 生长天数=15 ｜ 播种标签: Hydroponic ｜ PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Plants/Plants_Cultivated_Manufacture.xml
- 亚麻花(Plant_Flax) -> 生亚麻(RawFlax)×9 ｜ 生长天数=8.1 ｜ 播种标签: Ground, Hydroponic
- 玉米(Plant_Corn) -> 玉米(RawCorn)×30 ｜ 生长天数=9 ｜ 播种标签: Ground, Hydroponic ｜ PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Plants/Plants_Cultivated_Vegetables.xml
- 土豆(Plant_Potato) -> 土豆(RawPotatoes)×13 ｜ 生长天数=5 ｜ 播种标签: Ground, Hydroponic ｜ PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Plants/Plants_Cultivated_Vegetables.xml
- 水稻(Plant_Rice) -> 稻米(RawRice)×16 ｜ 生长天数=4.6 ｜ 播种标签: Ground, Hydroponic ｜ PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Plants/Plants_Cultivated_Vegetables.xml
- 微光菇(Bryolux) -> 微光菇(RawShimmershroom)×5 ｜ 生长天数=18 ｜ 播种标签: Fungiponics ｜ PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Plants/Plants_Cave.xml
- 西红柿(PlantTomato) -> 西红柿(RawTomatoes)×10 ｜ 生长天数=3.7 ｜ 播种标签: Ground, Hydroponic
- 西瓜植株(Plant_Watermelon) -> 西瓜(RawWatermelon)×5 ｜ 生长天数=6.2 ｜ 播种标签: Ground, Hydroponic
- 厄兆噬树(Plant_TreeHarbinger) -> 活铁(Bioferrite)×按规则 ｜ PatchOperationConditional; PatchApplied=Mods/Core_SK_DLCModule/Anomaly/Patches/ThingDefs_Plants/Plants_Special_Anomaly.xml; PatchOperationConditional
- 精神叶(Plant_Psychoid) -> 精神叶(PsychoidLeaves)×10 ｜ 生长天数=6.6 ｜ 播种标签: Ground, Hydroponic ｜ PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Plants/Plants_Cultivated_Manufacture.xml
- 啤酒花(Plant_Hops) -> 啤酒花(RawHops)×8 ｜ 生长天数=5.4 ｜ 播种标签: Ground, Hydroponic ｜ PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Plants/Plants_Cultivated_Manufacture.xml
- 大麻(Plant_Smokeleaf) -> 大麻叶(SmokeleafLeaves)×4 ｜ 生长天数=10.8 ｜ 播种标签: Ground, Hydroponic ｜ PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingDefs_Plants/Plants_Cultivated_Manufacture.xml

### 2.6 动物挤奶（18）

- 野牛(Bison) -> 奶(Milk)×9 ｜ 组件=CompProperties_Milkable ｜ PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Farm.xml
- 水牛(Buffalo) -> 奶(Milk)×14 ｜ 组件=CompProperties_Milkable
- 驯鹿(Caribou) -> 奶(Milk)×12 ｜ 组件=CompProperties_Milkable ｜ PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Temperate.xml
- 牛(Cow) -> 奶(Milk)×16 ｜ 组件=CompProperties_Milkable ｜ PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Farm.xml
- 单峰骆驼(Dromedary) -> 奶(Milk)×11 ｜ 组件=CompProperties_Milkable ｜ PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Arid.xml
- 驼鹿(Elk) -> 奶(Milk)×10 ｜ 组件=CompProperties_Milkable ｜ PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Temperate.xml
- 山羊(Goat) -> 奶(Milk)×13 ｜ 组件=CompProperties_Milkable ｜ PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Farm.xml
- 马(Horse) -> 奶(Milk)×10 ｜ 组件=CompProperties_Milkable ｜ PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Horse.xml
- 长鬃马(Horse_Wooly) -> 奶(Milk)×10 ｜ 组件=CompProperties_Milkable
- 野山羊(Ibex) -> 奶(Milk)×7 ｜ 组件=CompProperties_Milkable ｜ PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Temperate.xml
- 雪牛(Muffalo) -> 奶(Milk)×13 ｜ 组件=CompProperties_Milkable ｜ PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Arid.xml
- 霍加狓(Okapi) -> 奶(Milk)×16 ｜ 组件=CompProperties_Milkable
- 绵羊(Sheep) -> 奶(Milk)×10 ｜ 组件=CompProperties_Milkable ｜ PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Arid.xml
- 北极德西里(Animal_Desiri) -> 未提纯的共感粉(Synchronite)×1 ｜ 组件=CompProperties_Milkable
- 沙漠德西里(Animal_Desiri_Desert) -> 未提纯的共感粉(Synchronite)×1 ｜ 组件=CompProperties_Milkable
- 温带德西里(Animal_Desiri_TemperateOne) -> 未提纯的共感粉(Synchronite)×1 ｜ 组件=CompProperties_Milkable
- 温带德西里(Animal_Desiri_TemperateTwo) -> 未提纯的共感粉(Synchronite)×1 ｜ 组件=CompProperties_Milkable
- 都市德西里(Animal_Desiri_Urban) -> 未提纯的共感粉(Synchronite)×1 ｜ 组件=CompProperties_Milkable

### 2.7 动物剪毛（12）

- 羊驼(Alpaca) -> 羊驼毛(WoolAlpaca)×50 ｜ 组件=CompProperties_Shearable ｜ PatchApplied=Mods/Core_SK/Patches/SK_Patches_LeatherRemover.xml; PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Farm.xml
- 单峰骆驼(Dromedary) -> 骆驼毛(WoolDromedary)×80 ｜ 组件=CompProperties_Shearable ｜ PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Arid.xml
- 爆炸羊(Boomalope) -> 含胺乳化燃料(FSX)×6 ｜ 组件=SK.CompProperties_ShearableRenameable ｜ PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Tropical.xml
- 爆炸鼠(Boomrat) -> 含胺乳化燃料(FSX)×1 ｜ 组件=SK.CompProperties_ShearableRenameable ｜ PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Rodentlike.xml
- 水牛(Buffalo) -> 雪牛毛(WoolMuffalo)×110 ｜ 组件=CompProperties_Shearable
- 绵羊(Sheep) -> 羊毛(WoolSheep)×45 ｜ 组件=CompProperties_Shearable ｜ PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Arid.xml
- 皮蠹甲虫(Crashbug) -> 甲壳(Chitin)×7 ｜ 组件=SK.CompProperties_ShearableRenameable
- 巨型甲虫(Megascarab) -> 甲壳(Chitin)×30 ｜ 组件=SK.CompProperties_ShearableRenameable ｜ PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Insects.xml
- 巨型蜘蛛(Megaspider) -> 甲壳(Chitin)×60 ｜ 组件=SK.CompProperties_ShearableRenameable ｜ PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Insects.xml
- 蝎子(Scorpion) -> 甲壳(Chitin)×125 ｜ 组件=SK.CompProperties_ShearableRenameable
- 穴居阔步虫(Spelopede) -> 甲壳(Chitin)×45 ｜ 组件=SK.CompProperties_ShearableRenameable ｜ PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Insects.xml
- 特拉甲虫(Terramorph) -> 甲壳(Chitin)×80 ｜ 组件=SK.CompProperties_ShearableRenameable

### 2.8 动物产受精蛋（9）

- 青足龙蛇(Behir) -> 犬魔蛋（已受精）(EggBehirFertilized)×1 ｜ 组件=CompProperties_EggLayer
- 芬尼克斯(Feenix) -> 芬尼克斯蛋（已受精）(EggFenixFertilized)×1 ｜ 组件=CompProperties_EggLayer
- 绿飞龙(GreenWyvern) -> 绿飞龙蛋（已受精）(EggGreenWyvernFertilized)×1 ｜ 组件=CompProperties_EggLayer
- 拉科斯帝鳄蜥(Lacosdile) -> 拉科斯帝鳄蜥（已受精）(EggLacosdileFertilized)×5~7 ｜ 组件=CompProperties_EggLayer
- 夜灵(Nightling) -> 夜灵蛋（已受精）(EggNightlingFertilized)×1~2 ｜ 组件=CompProperties_EggLayer
- 松鸡(Ptarmigan) -> 松鸡蛋（已受精）(EggPtarmiganFertilized)×1~5 ｜ 组件=CompProperties_EggLayer
- 翼猛龙(FRaptor) -> 翼猛龙蛋（已受精）(EggFRaptorFertilized)×1~2 ｜ 组件=CompProperties_EggLayer
- 眼镜蛇(Cobra) -> 眼镜蛇蛋（已受精）(EggCobraFertilized)×1~3 ｜ 组件=CompProperties_EggLayer ｜ PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Tropical.xml
- 鬣蜥(Iguana) -> 鬣蜥蛋（已受精）(EggIguanaFertilized)×1~3 ｜ 组件=CompProperties_EggLayer ｜ PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Arid.xml

### 2.9 动物产未受精蛋（9）

- 青足龙蛇(Behir) -> Behir Egg (Unfertilized)(EggBehirUnfertilized)×1 ｜ 组件=CompProperties_EggLayer
- 芬尼克斯(Feenix) -> Fenix Egg (Unfertilized)(EggFenixUnfertilized)×1 ｜ 组件=CompProperties_EggLayer
- 绿飞龙(GreenWyvern) -> Green wyvern Egg (Unfertilized)(EggGreenWyvernUnfertilized)×1 ｜ 组件=CompProperties_EggLayer
- 拉科斯帝鳄蜥(Lacosdile) -> Lacosdile Egg (Unfertilized)(EggLacosdileUnfertilized)×5~7 ｜ 组件=CompProperties_EggLayer
- 夜灵(Nightling) -> Nightling egg (Unfertilized)(EggNightlingUnfertilized)×1~2 ｜ 组件=CompProperties_EggLayer
- 松鸡(Ptarmigan) -> ptarmigan egg (Unfertilized)(EggPtarmiganUnfertilized)×1~5 ｜ 组件=CompProperties_EggLayer
- 翼猛龙(FRaptor) -> Raptor Egg (Unfertilized)(EggFRaptorUnfertilized)×1~2 ｜ 组件=CompProperties_EggLayer
- 眼镜蛇(Cobra) -> 眼镜蛇蛋（未受精）(EggCobraUnfertilized)×1~3 ｜ 组件=CompProperties_EggLayer ｜ PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Tropical.xml
- 鬣蜥(Iguana) -> 鬣蜥蛋（未受精）(EggIguanaUnfertilized)×1~3 ｜ 组件=CompProperties_EggLayer ｜ PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/ThingsDefs_Races/Races_Animal_Arid.xml

### 2.10 物质生成（1）

- 反物质(AntimatterResourceInfo) -> 反物质(Antimatter)×2

## 3. 工作台配方（按产出资源分组）

### 3.1 原材料（46）

#### 铅矾矿(Anglesite)

- 无机物(Matter)×50 -> 铅矾矿(Anglesite)×100 @ 无机物转化器(MatterConverter) ｜ 工作量=12000 ｜ 研究:物质转换 II(Super_matter_E1)，技能:制作≥15 ｜ `ConvertToLead`

#### 灰烬(Ash)

- — -> 灰烬(Ash)×2 @ 篝火(Campfire), 电力火化炉(ElectricCrematorium), 窑炉(Kiln) ｜ 工作量=200 ｜ 研究:基础加工知识(Craft_0) ｜ `BurnApparel` ｜ PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/RecipeDefs/Recipes_Cremation.xml
- [木质] 或 [木本植物] 或 [织物] 或 [皮革]×20 -> 灰烬(Ash)×3 @ 篝火(Campfire), 电力火化炉(ElectricCrematorium), 窑炉(Kiln) ｜ 工作量=400 ｜ 研究:基础加工知识(Craft_0) ｜ `BurnThings`
- — -> 灰烬(Ash)×4 @ 篝火(Campfire), 电力火化炉(ElectricCrematorium), 窑炉(Kiln) ｜ 工作量=600 ｜ 研究:基础加工知识(Craft_0) ｜ `CremateCorpse` ｜ PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/RecipeDefs/Recipes_Cremation.xml
- [尸体]×1 -> 灰烬(Ash)×4 ｜ 工作量=800 ｜ 研究:基础加工知识(Craft_0) ｜ `CremateCorpseCampfire`

#### 铝土矿(Aluminium)

- 无机物(Matter)×50 -> 铝土矿(Aluminium)×80 @ 无机物转化器(MatterConverter) ｜ 工作量=12000 ｜ 研究:物质转换 II(Super_matter_E1)，技能:制作≥15 ｜ `ConvertToAluminium`

#### 碳聚物(Carbon)

- 聚合物(Polymers)×25 + [煤类]×25 + 合成橡胶(Rubber)×15 -> 碳聚物(Carbon)×12 @ 石化产品实验室(TableChemlab) ｜ 工作量=4000 ｜ 研究:石油化学 V(Oil_Industry_C6)，技能:制作≥15 ｜ `MakeCarbon`

#### 木炭(Charcoal)

- [木质]×25 -> 木炭(Charcoal)×20 @ 电熔炼器(ElectricSmelter) ｜ 工作量=800 ｜ 研究:金属加工 II(Metals_C1)，技能:制作≥4 ｜ `MakeCharcoal_Electric`
- [木质]×25 -> 木炭(Charcoal)×20 @ 篝火(Campfire), 窑炉(Kiln), 熔炉(TableFurnace) ｜ 工作量=1600 ｜ 研究:金属加工 I(Craft_B1) ｜ `MakeCharcoal_Hand`

#### 堆肥料(Compost)

- 腐肉(RottedMeat) 或 [生肉] 或 [动物产品(生)] 或 [废料]×25 + 干草(Hay) 或 腐烂物(RottedMush) 或 灰烬(Ash) 或 [植物食材]×5 -> 堆肥料(Compost)×5 @ 屠宰点(ButcherSpot), 屠宰台(TableButcher), 厨房案桌(TableKitchen) ｜ 工作量=625 ｜ 研究:农业 I(SewageSludgeComposting) ｜ `MakeCompost`

#### 铜矿(Copper)

- 无机物(Matter)×50 -> 铜矿(Copper)×100 @ 无机物转化器(MatterConverter) ｜ 工作量=12000 ｜ 研究:物质转换 II(Super_matter_E1)，技能:制作≥15 ｜ `ConvertToCopper`

#### 肥料(Fertilizer)

- 堆肥料(Compost)×10 -> 肥料(Fertilizer)×10 ｜ 工作量=10000 ｜ `MakeFertilizer`

#### 金矿(Gold)

- 无机物(Matter)×60 -> 金矿(Gold)×20 @ 无机物转化器(MatterConverter) ｜ 工作量=12000 ｜ 研究:物质转换 II(Super_matter_E1)，技能:制作≥15 ｜ `ConvertToGold`

#### 钛铁矿(Ilmenite)

- 无机物(Matter)×200 -> 钛铁矿(Ilmenite)×20 @ 无机物转化器(MatterConverter) ｜ 工作量=15000 ｜ 研究:物质转换 II(Super_matter_E1)，技能:制作≥15 ｜ `ConvertToTitanium`

#### 铁矿石(Iron)

- 无机物(Matter)×50 -> 铁矿石(Iron)×100 @ 无机物转化器(MatterConverter) ｜ 工作量=12000 ｜ 研究:物质转换 II(Super_matter_E1)，技能:制作≥15 ｜ `ConvertToSteel`

#### 镍矿(Nickel)

- 无机物(Matter)×150 -> 镍矿(Nickel)×20 @ 无机物转化器(MatterConverter) ｜ 工作量=12000 ｜ 研究:物质转换 II(Super_matter_E1)，技能:制作≥15 ｜ `ConvertToNickel`

#### 石蜡(Paraffins)

- 无机物(Matter)×50 -> 石蜡(Paraffins)×100 @ 无机物转化器(MatterConverter) ｜ 工作量=12000 ｜ 研究:物质转换 II(Super_matter_E1)，技能:制作≥15 ｜ `ConvertToParaffins`

#### 土堆(Dirt)

- 沙(SandResource)×2 + 肥料(Fertilizer)×1 -> 土堆(Dirt)×3 @ 屠宰点(ButcherSpot) ｜ 工作量=300 ｜ 研究:基础加工知识(Craft_0) ｜ `make_dirt`
- 沙(SandResource)×20 + 肥料(Fertilizer)×10 -> 土堆(Dirt)×30 @ 电动混凝土搅拌机(EConcreteMixer) ｜ 工作量=1500 ｜ 研究:混凝土 II(Concrete_C2) ｜ `make_dirt_Electric`
- 沙(SandResource)×20 + 肥料(Fertilizer)×10 -> 土堆(Dirt)×30 @ 手动混凝土搅拌器(ConcreteMixer) ｜ 工作量=3000 ｜ 研究:混凝土 I(Concrete_C1) ｜ `make_dirt_hand`

#### 塑料(Plastic)

- 聚合物(Polymers)×30 -> 塑料(Plastic)×10 @ 石化产品实验室(TableChemlab) ｜ 工作量=800 ｜ 技能:制作≥10 ｜ `MakePlastic`
- 聚合物(Polymers)×20 + 硫酸盐(Sulphates)×5 -> 塑料(Plastic)×20 @ 石化产品实验室(TableChemlab) ｜ 工作量=2400 ｜ 技能:制作≥10 ｜ `MakePlasticSulphate`
- 聚合物(Polymers)×60 + 硫酸盐(Sulphates)×15 -> 塑料(Plastic)×60 @ 石化产品实验室(TableChemlab) ｜ 工作量=2400 ｜ 技能:制作≥10 ｜ `MakePlasticSulphateBulk`

#### 聚合物(Polymers)

- 无机物(Matter)×50 -> 聚合物(Polymers)×100 @ 无机物转化器(MatterConverter) ｜ 工作量=12000 ｜ 研究:物质转换 II(Super_matter_E1)，技能:制作≥15 ｜ `ConvertToPolymers`

#### 合成橡胶(Rubber)

- 聚合物(Polymers)×30 + 硫酸盐(Sulphates)×3 -> 合成橡胶(Rubber)×10 @ 石化产品实验室(TableChemlab) ｜ 工作量=800 ｜ 技能:制作≥10 ｜ `MakeRubber`
- 天然橡胶(RawCaoutchouc)×30 + 灰烬(Ash) 或 硫磺(Sulfur) 或 硫酸盐(Sulphates)×3 -> 合成橡胶(Rubber)×10 @ 窑炉(Kiln), 石化产品实验室(TableChemlab) ｜ 工作量=800 ｜ 研究:化学 I(BiofuelRefining)，技能:制作≥10 ｜ `VulcanizeRubber`

#### 碎石(CrushedStone)

- [石砖]×40 -> 碎石(CrushedStone)×40 @ 电动机械加工台(TableMachining) ｜ 工作量=1200 ｜ `MakeCrushedStone_Electric`
- [石块]×1 -> 碎石(CrushedStone)×32 @ 电动机械加工台(TableMachining) ｜ 工作量=1200 ｜ `MakeCrushedStone_Electric_Chunk`
- [石砖]×20 -> 碎石(CrushedStone)×20 @ 手动石料加工台(TableStonecutter) ｜ 工作量=1200 ｜ `MakeCrushedStone_Hand`
- [石块]×1 -> 碎石(CrushedStone)×32 @ 手动石料加工台(TableStonecutter) ｜ 工作量=2400 ｜ `MakeCrushedStone_Hand_Chunk`

#### 盐(Salt)

- — -> 盐(Salt)×5 @ 挖盐点(VG_SaltMine) ｜ 工作量=1800 ｜ `VG_Minesalt`

#### 沙(SandResource)

- 碎石(CrushedStone)×20 -> 沙(SandResource)×50 @ 电动机械加工台(TableMachining) ｜ 工作量=600 ｜ `MakeSand_Electric`
- [石块]×1 -> 沙(SandResource)×80 @ 电动机械加工台(TableMachining) ｜ 工作量=1500 ｜ `MakeSand_Electric_Chunk`
- 碎石(CrushedStone)×10 -> 沙(SandResource)×25 @ 手动石料加工台(TableStonecutter) ｜ 工作量=600 ｜ 研究:石料加工 I(Stonecutting) ｜ `MakeSand_Hand`
- [石块]×1 -> 沙(SandResource)×80 @ 手动石料加工台(TableStonecutter) ｜ 工作量=3000 ｜ 研究:石料加工 I(Stonecutting) ｜ `MakeSand_Hand_Chunk`

#### 硅(Silicon)

- 沙(SandResource)×50 -> 硅(Silicon)×5 @ 电熔炼器(ElectricSmelter) ｜ 工作量=800 ｜ 研究:金属加工 II(Metals_C1)，技能:制作≥5 ｜ `MakeSilicon_Electric`
- 沙(SandResource)×200 -> 硅(Silicon)×20 @ 电弧炉(EAF) ｜ 工作量=1800 ｜ 研究:金属加工 II(Metals_C1)，技能:制作≥5 ｜ `MakeSilicon_Set`

#### 白银(Silver)

- 无机物(Matter)×100 -> 白银(Silver)×300 @ 无机物转化器(MatterConverter) ｜ 工作量=12000 ｜ 研究:物质转换 II(Super_matter_E1)，技能:制作≥15 ｜ `ConvertToSilver`

#### 硫磺(Sulfur)

- 铁矿石(Iron)×35 -> 硫磺(Sulfur)×10 @ 弹药工作台(AmmoBench), 重型弹药工作台(TableHeavyAmmunition) ｜ 工作量=1200 ｜ `MakeSulfur`

#### 硫酸盐(Sulphates)

- 无机物(Matter)×50 -> 硫酸盐(Sulphates)×100 @ 无机物转化器(MatterConverter) ｜ 工作量=12000 ｜ 研究:物质转换 II(Super_matter_E1)，技能:制作≥15 ｜ `ConvertToSulphates`

#### 表面活性剂发泡液(Surfactant)

- 盐(Salt)×10 + 硫磺(Sulfur)×15 + [含酒精饮料]×15 -> 表面活性剂发泡液(Surfactant)×50 @ 酿造台(Brewery), 药物实验台(DrugLab), 电动酿造台(ElectricBrewery) ｜ 工作量=4000 ｜ 研究:建造 IV(Firefoam)，技能:制作≥8 ｜ `MakeSurfactant`
- 盐(Salt)×10 + 硫磺(Sulfur)×15 + 椰子(RawCoconut)×25 -> 表面活性剂发泡液(Surfactant)×50 @ 酿造台(Brewery), 药物实验台(DrugLab), 电动酿造台(ElectricBrewery) ｜ 工作量=4000 ｜ 研究:建造 IV(Firefoam)，技能:制作≥12, 智识≥4 ｜ `MakeSurfactant_CoconutSubstitute`
- 复合粘剂(Compaste)×15 + 聚合物(Polymers)×25 -> 表面活性剂发泡液(Surfactant)×50 @ 药物实验台(DrugLab) ｜ 工作量=4000 ｜ 研究:石油化学 IV(Oil_Industry_C5)，技能:制作≥12, 智识≥4 ｜ `MakeSurfactant_Polymers`

#### 人造纤维(SyntheticFibers)

- 聚合物(Polymers)×25 + 硫酸盐(Sulphates)×3 -> 人造纤维(SyntheticFibers)×5 @ 石化产品实验室(TableChemlab) ｜ 工作量=1200 ｜ 研究:石油化学 V(Oil_Industry_C6)，技能:制作≥14 ｜ `MakeSyntheticFibers`

#### 锡矿(Tin)

- 无机物(Matter)×50 -> 锡矿(Tin)×100 @ 无机物转化器(MatterConverter) ｜ 工作量=12000 ｜ 研究:物质转换 II(Super_matter_E1)，技能:制作≥15 ｜ `ConvertToTin`

#### 铀矿(Uranium)

- 无机物(Matter)×200 -> 铀矿(Uranium)×50 @ 无机物转化器(MatterConverter) ｜ 工作量=12000 ｜ 研究:物质转换 II(Super_matter_E1)，技能:制作≥15 ｜ `ConvertToUranium`

### 3.2 加工品（135）

#### 120mm坦克炮组件箱(CannonTurret_Crate)

- 武器部件(Weapon_Parts)×14 + 火炮武器部件(Cannon_Component)×2 + [金属锭类]×55 + 合成橡胶(Rubber)×25 -> 120mm坦克炮组件箱(CannonTurret_Crate)×1 @ 重型武器机床(HeavyArmsBench) ｜ 工作量=17500 ｜ 研究:火炮 II(Artillery_C2)，技能:制作≥14 ｜ `BuildCannonTurret_Crate`

#### 130mm重型导弹系统组件箱(RocketLauncher_Crate)

- 武器部件(Weapon_Parts)×10 + 发射器部件(Launcher_Component)×3 + [金属锭类]×35 -> 130mm重型导弹系统组件箱(RocketLauncher_Crate)×1 @ 重型武器机床(HeavyArmsBench) ｜ 工作量=19500 ｜ 研究:火箭弹 IV(Rockets_D2)，技能:制作≥15 ｜ `BuildRocketLauncher_Crate`

#### 130mm中型导弹系统组件箱(MediumMissileSystem_Crate)

- 武器部件(Weapon_Parts)×10 + 发射器部件(Launcher_Component)×2 + [金属锭类]×35 -> 130mm中型导弹系统组件箱(MediumMissileSystem_Crate)×1 @ 重型武器机床(HeavyArmsBench) ｜ 工作量=18500 ｜ 研究:火箭弹 III(Rockets_D1)，技能:制作≥14 ｜ `BuildMediumMissileSystem_Crate`

#### KPV机枪组件箱(KPV_Crate)

- 武器部件(Weapon_Parts)×17 + 重型武器部件(Heavy_Component)×1 + [金属锭类]×50 -> KPV机枪组件箱(KPV_Crate)×1 @ 重型武器机床(HeavyArmsBench) ｜ 工作量=11500 ｜ 研究:重型炮塔 II(Heavy_turrets_C2)，技能:制作≥7 ｜ `BuildKPV_Crate`

#### 14.5mm重型哨戒机枪组件箱(HeavyTurret_Crate)

- 武器部件(Weapon_Parts)×14 + 重型武器部件(Heavy_Component)×1 + [金属锭类]×35 + 集成电路(Electronics)×5 -> 14.5mm重型哨戒机枪组件箱(HeavyTurret_Crate)×1 @ 重型武器机床(HeavyArmsBench) ｜ 工作量=13500 ｜ 研究:自动炮塔 II(Autoturrets_D2)，技能:制作≥12 ｜ `BuildHeavyTurret_Crate`

#### 155mm榴弹炮组件箱(HowitzerTurret_Crate)

- 武器部件(Weapon_Parts)×18 + 火炮武器部件(Cannon_Component)×2 + [金属锭类]×70 + 合成橡胶(Rubber)×35 -> 155mm榴弹炮组件箱(HowitzerTurret_Crate)×1 @ 重型武器机床(HeavyArmsBench) ｜ 工作量=20500 ｜ 研究:火炮 II(Artillery_C2)，技能:制作≥15 ｜ `BuildHowitzerTurret_Crate`

#### 20mm火神炮组件箱(VulcanCannon_Crate)

- 武器部件(Weapon_Parts)×20 + 火炮武器部件(Cannon_Component)×1 + [金属锭类]×60 + 合成橡胶(Rubber)×25 -> 20mm火神炮组件箱(VulcanCannon_Crate)×1 @ 重型武器机床(HeavyArmsBench) ｜ 工作量=17500 ｜ 研究:重型炮塔 III(Heavy_turrets_C3)，技能:制作≥16 ｜ `BuildVulcanCannon_Crate`

#### 20x102mm厄利孔机炮组件箱(Oerlikonautocannon_Crate)

- 武器部件(Weapon_Parts)×15 + 火炮武器部件(Cannon_Component)×1 + [金属锭类]×45 + 合成橡胶(Rubber)×15 -> 20x102mm厄利孔机炮组件箱(Oerlikonautocannon_Crate)×1 @ 重型武器机床(HeavyArmsBench) ｜ 工作量=13500 ｜ 研究:重型炮塔 III(Heavy_turrets_C3)，技能:制作≥12 ｜ `BuildOerlikonautocannon_Crate`

#### AGS-17组件箱(AGSturret_Crate)

- 武器部件(Weapon_Parts)×10 + 火炮武器部件(Cannon_Component)×1 + [金属锭类]×20 + 合成橡胶(Rubber)×10 -> AGS-17组件箱(AGSturret_Crate)×1 @ 重型武器机床(HeavyArmsBench) ｜ 工作量=11500 ｜ 研究:发射器 II(Grenade_launchers_C2)，技能:制作≥8 ｜ `BuildAGSturret_Crate`

#### AGS-30组件箱(AGCturret_Crate)

- 武器部件(Weapon_Parts)×12 + 火炮武器部件(Cannon_Component)×1 + [金属锭类]×20 + 合成橡胶(Rubber)×10 -> AGS-30组件箱(AGCturret_Crate)×1 @ 重型武器机床(HeavyArmsBench) ｜ 工作量=15500 ｜ 研究:发射器 II(Grenade_launchers_C2)，技能:制作≥8 ｜ `BuildAGCturret_Crate`

#### GAU-8复仇者火炮组件箱(Avenger_Crate)

- 武器部件(Weapon_Parts)×22 + 火炮武器部件(Cannon_Component)×1 + [金属锭类]×60 + 合成橡胶(Rubber)×35 -> GAU-8复仇者火炮组件箱(Avenger_Crate)×1 @ 重型武器机床(HeavyArmsBench) ｜ 工作量=18500 ｜ 研究:重型炮塔 III(Heavy_turrets_C3)，技能:制作≥16 ｜ `BuildAvenger_Crate`

#### 35mm哨戒炮塔组件箱(NavalGunTurret_Crate)

- 武器部件(Weapon_Parts)×18 + 火炮武器部件(Cannon_Component)×1 + [金属锭类]×35 + 集成电路(Electronics)×5 -> 35mm哨戒炮塔组件箱(NavalGunTurret_Crate)×1 @ 重型武器机床(HeavyArmsBench) ｜ 工作量=14500 ｜ 研究:自动炮塔 III(Autoturrets_D3)，技能:制作≥13 ｜ `BuildNavalGunTurret_Crate`

#### 5.56mm轻型哨戒炮塔组件箱(SentryTurret_Crate)

- 武器部件(Weapon_Parts)×7 + 步枪部件(Rifle_Component)×2 + [金属锭类]×20 + 集成电路(Electronics)×5 -> 5.56mm轻型哨戒炮塔组件箱(SentryTurret_Crate)×1 @ 重型武器机床(HeavyArmsBench) ｜ 工作量=10500 ｜ 研究:自动炮塔 I(GunTurrets)，技能:制作≥11 ｜ `BuildSentryTurret_Crate`

#### M2 .50勃朗宁机枪组件箱(MGTurret_Crate)

- 武器部件(Weapon_Parts)×14 + 重型武器部件(Heavy_Component)×1 + [金属锭类]×40 -> M2 .50勃朗宁机枪组件箱(MGTurret_Crate)×1 @ 重型武器机床(HeavyArmsBench) ｜ 工作量=11500 ｜ 研究:重型炮塔 I(SniperTurret)，技能:制作≥7 ｜ `BuildMGTurret_Crate`

#### M134速射机枪组件箱(MinigunTurret_Crate)

- 武器部件(Weapon_Parts)×15 + 重型武器部件(Heavy_Component)×1 + [金属锭类]×45 -> M134速射机枪组件箱(MinigunTurret_Crate)×1 @ 重型武器机床(HeavyArmsBench) ｜ 工作量=10500 ｜ 研究:重型炮塔 II(Heavy_turrets_C2)，技能:制作≥8 ｜ `BuildMinigunTurret_Crate`

#### M240B机枪组件箱(M240B_Crate)

- 武器部件(Weapon_Parts)×10 + 重型武器部件(Heavy_Component)×1 + [金属锭类]×35 -> M240B机枪组件箱(M240B_Crate)×1 @ 重型武器机床(HeavyArmsBench) ｜ 工作量=10500 ｜ 研究:重型炮塔 I(SniperTurret)，技能:制作≥7 ｜ `BuildM240B_Crate`

#### 81mm迫击炮组件箱(Mortar_Crate)

- 武器部件(Weapon_Parts)×8 + 火炮武器部件(Cannon_Component)×1 + [金属锭类]×20 -> 81mm迫击炮组件箱(Mortar_Crate)×1 @ 重型武器机床(HeavyArmsBench) ｜ 工作量=12500 ｜ 研究:火炮 I(Mortars)，技能:制作≥9 ｜ `BuildMortar_Crate`

#### 90mm高射炮组件箱(FlakTurret_Crate)

- 武器部件(Weapon_Parts)×12 + 火炮武器部件(Cannon_Component)×2 + [金属锭类]×45 + 合成橡胶(Rubber)×20 -> 90mm高射炮组件箱(FlakTurret_Crate)×1 @ 重型武器机床(HeavyArmsBench) ｜ 工作量=14500 ｜ 研究:火炮 I(Mortars)，技能:制作≥12 ｜ `BuildFlakTurret_Crate`

#### 先进零部件(ComponentAdvanced)

- 护盾腰带(Apparel_ShieldBelt) 或 守护者护盾(Apparel_ShieldGuardian)×1 -> 先进零部件(ComponentAdvanced)×8 + 集成电路(Electronics)×1 @ 回收站(TableRecycling) ｜ 工作量=1000 ｜ 技能:制作≥10 ｜ `DeconstructPersonalShield`
- [超轻合金锭类]×30 -> 先进零部件(ComponentAdvanced)×10 @ 电动装配台(AdvToolBench), 手工装配台(FabricationBench) ｜ 工作量=2500 ｜ 研究:零部件 I(Components_D1)，技能:制作≥6 ｜ `Make_ComponentAdvanced`

#### 先进机械组件(AdvMechanism)

- [超轻合金锭类]×60 + 先进零部件(ComponentAdvanced)×20 + 人造纤维(SyntheticFibers)×10 -> 先进机械组件(AdvMechanism)×10 @ 电动装配台(AdvToolBench) ｜ 工作量=3200 ｜ 研究:零部件 I(Components_D1)，技能:制作≥16 ｜ `MakeAdvMechanism_Electric`
- [超轻合金锭类]×30 + 先进零部件(ComponentAdvanced)×10 + 人造纤维(SyntheticFibers)×5 -> 先进机械组件(AdvMechanism)×5 @ 手工装配台(FabricationBench) ｜ 工作量=3200 ｜ 研究:零部件 I(Components_D1)，技能:制作≥13 ｜ `MakeAdvMechanism_Hand`

#### 先进步枪部件(AdvRifle_Component)

- 人造纤维(SyntheticFibers)×5 + 塑料(Plastic)×15 + 合成橡胶(Rubber)×15 + 步枪部件(Rifle_Component)×1 + 先进机械组件(AdvMechanism)×2 -> 先进步枪部件(AdvRifle_Component)×1 @ 先进武器加工台(AdvWeaponCraftingWorkTable) ｜ 工作量=2100 ｜ 研究:步枪 II(Rifles_C2)，技能:制作≥13 ｜ `MakeAdvRifle_Component`

#### 先进狙击枪部件(AdvSniper_Component)

- 人造纤维(SyntheticFibers)×10 + 塑料(Plastic)×20 + 合成橡胶(Rubber)×15 + 集成电路(Electronics)×4 + 狙击枪部件(Sniper_Component)×2 + 先进机械组件(AdvMechanism)×3 -> 先进狙击枪部件(AdvSniper_Component)×1 @ 先进武器加工台(AdvWeaponCraftingWorkTable) ｜ 工作量=2100 ｜ 研究:狙击步枪 III(Sniper_rifles_D1)，技能:制作≥15 ｜ `MakeAdvSniper_Component`

#### 亚力可合金（铝镍钴合金）(AlnicoAlloy)

- 钢铁(Steel)×8 + 铝合金(AluminiumBar)×3 + 钴合金(Cobalt)×3 + 镍合金(NickelBar)×6 -> 亚力可合金（铝镍钴合金）(AlnicoAlloy)×20 @ 电弧炉(EAF) ｜ 工作量=1650 ｜ 研究:金属加工 IV(Metals_C3)，技能:制作≥13 ｜ `MakeAlnicoAlloy`

#### α阿尔法聚合物(AlphaPoly)

- 伪人格核心(AIPersonaCore)×1 -> 生物电子元件(BioMicrochips)×1 + α阿尔法聚合物(AlphaPoly)×30 + 碳纤维锭(CarbonAlloy)×40 @ 自动化装配器(RobAssem) ｜ 工作量=4000 ｜ 研究:零部件 II(Components_D2)，技能:制作≥16 ｜ `DisassembleAIPersonaCore`
- 贫铀合金(DepletedUranium)×5 + 碳纤维锭(CarbonAlloy)×5 + 反物质(Antimatter)×3 -> α阿尔法聚合物(AlphaPoly)×15 @ 量子加工台(Matterfab) ｜ 工作量=8000 ｜ 技能:制作≥19 ｜ `ProduceAPoly`

#### 铝合金(AluminiumBar)

- 铝土矿(Aluminium)×20 -> 铝合金(AluminiumBar)×20 @ 电熔炼器(ElectricSmelter) ｜ 工作量=1200 ｜ 研究:金属加工 III(Metals_C2)，技能:制作≥7 ｜ `MakeAluminiumBars_Electric`

#### BGM-71“陶式”反坦克导弹组件箱(TOW_Crate)

- 武器部件(Weapon_Parts)×15 + 发射器部件(Launcher_Component)×1 + [金属锭类]×30 + 合成橡胶(Rubber)×15 -> BGM-71“陶式”反坦克导弹组件箱(TOW_Crate)×1 @ 重型武器机床(HeavyArmsBench) ｜ 工作量=16500 ｜ 研究:火箭弹 III(Rockets_D1)，技能:制作≥13 ｜ `BuildTOW_Crate`

#### FGM-148“标枪”反坦克导弹组件箱(LightMissileSystem_Crate)

- 武器部件(Weapon_Parts)×16 + 发射器部件(Launcher_Component)×1 + [金属锭类]×35 + 合成橡胶(Rubber)×15 -> FGM-148“标枪”反坦克导弹组件箱(LightMissileSystem_Crate)×1 @ 重型武器机床(HeavyArmsBench) ｜ 工作量=16500 ｜ 研究:火箭弹 II(Rockets_C2)，技能:制作≥13 ｜ `BuildLightMissileSystem_Crate`

#### 自动反坦克火箭发射系统组件箱(SentryRocketLauncher_Crate)

- 武器部件(Weapon_Parts)×15 + 发射器部件(Launcher_Component)×1 + [金属锭类]×35 + 集成电路(Electronics)×5 -> 自动反坦克火箭发射系统组件箱(SentryRocketLauncher_Crate)×1 @ 重型武器机床(HeavyArmsBench) ｜ 工作量=16500 ｜ 研究:自动炮塔 III(Autoturrets_D3)，技能:制作≥15 ｜ `BuildSentryRocketLauncher_Crate`

#### 弩炮组件箱(Balista_Crate)

- [木质]×90 + [金属锭类]×30 -> 弩炮组件箱(Balista_Crate)×1 @ 手工加工台(CraftingSpot), 重型武器机床(HeavyArmsBench) ｜ 工作量=4500 ｜ 研究:攻城武器(Ballista_B1)，技能:制作≥3 ｜ `BuildBalista_Crate`

#### 竹板(BambooPlank)

- 竹节(Bamboo)×20 -> 竹板(BambooPlank)×30 @ 电动锯木台(TableSawmillElectric) ｜ 工作量=1200 ｜ 研究:金属加工 II(Metals_C1)，技能:制作≥9 ｜ `MakeBambooPlanks_Electric`
- 竹节(Bamboo)×10 -> 竹板(BambooPlank)×15 @ 手工锯台(TableSawmillHand) ｜ 工作量=1200 ｜ 研究:基础加工知识(Craft_0)，技能:制作≥6 ｜ `MakeBambooPlanks_Hand`

#### β贝塔聚合物(BetaPoly)

- α阿尔法聚合物(AlphaPoly)×3 + 生物合成材料(BiosyntheticMaterial)×5 -> β贝塔聚合物(BetaPoly)×5 @ 量子加工台(Matterfab) ｜ 工作量=12000 ｜ 技能:制作≥20 ｜ `ProduceBPoly`

#### 生物电子元件(BioMicrochips)

- 伪人格核心(AIPersonaCore)×1 -> 生物电子元件(BioMicrochips)×1 + α阿尔法聚合物(AlphaPoly)×30 + 碳纤维锭(CarbonAlloy)×40 @ 自动化装配器(RobAssem) ｜ 工作量=4000 ｜ 研究:零部件 II(Components_D2)，技能:制作≥16 ｜ `DisassembleAIPersonaCore`
- 生物质(Biomatter)×60 + 超织物(Hyperweave)×15 + 微芯片集成电路(Microchips)×2 -> 生物电子元件(BioMicrochips)×1 @ 自动化装配器(RobAssem) ｜ 工作量=7000 ｜ 研究:零部件 III(Components_D3)，技能:制作≥18 ｜ `MakeBioelectronicDevices`

#### 生物铁合金(BioferriteAlloy)

- 活铁(Bioferrite)×10 + 铁矿石(Iron)×10 + [煤类]×5 -> 生物铁合金(BioferriteAlloy)×10 @ 活铁塑造台(BioferriteShaper) ｜ 工作量=2100 ｜ 研究:活铁塑造(BioferriteShaping)，技能:制作≥9 ｜ `MakeBioferriteAlloy`

#### 生物合成材料(BiosyntheticMaterial)

- 生物质(Biomatter)×15 + 人造纤维(SyntheticFibers)×9 + 塑料(Plastic)×30 -> 生物合成材料(BiosyntheticMaterial)×5 @ 自动化装配器(RobAssem) ｜ 工作量=2400 ｜ 技能:制作≥18 ｜ `BuildBiosyntheticMaterial`

#### 青铜(Bronze)

- 铜矿(Copper) 或 铜(CopperBar)×20 + 锡(TinBar) 或 铝合金(AluminiumBar)×10 -> 青铜(Bronze)×30 @ 电熔炼器(ElectricSmelter) ｜ 工作量=700 ｜ 研究:金属加工 II(Metals_C1)，技能:制作≥5 ｜ `MakeBronze_Electric`
- 铜矿(Copper) 或 铜(CopperBar)×20 + 锡(TinBar) 或 铝合金(AluminiumBar)×10 -> 青铜(Bronze)×30 @ 熔炉(TableFurnace) ｜ 工作量=1000 ｜ 研究:金属加工 I(Craft_B1)，技能:制作≥2 ｜ `MakeBronze_Hand`

#### 火炮武器部件(Cannon_Component)

- [轻合金锭类] 或 [超轻合金锭类]×20 + 重型武器部件(Heavy_Component)×1 + 先进机械组件(AdvMechanism)×5 -> 火炮武器部件(Cannon_Component)×1 @ 先进武器加工台(AdvWeaponCraftingWorkTable), 重型武器机床(HeavyArmsBench) ｜ 工作量=3100 ｜ 研究:炮塔 I(Turrets_31)，技能:制作≥14 ｜ `MakeCannon_Component`

#### 碳纤维锭(CarbonAlloy)

- 伪人格核心(AIPersonaCore)×1 -> 生物电子元件(BioMicrochips)×1 + α阿尔法聚合物(AlphaPoly)×30 + 碳纤维锭(CarbonAlloy)×40 @ 自动化装配器(RobAssem) ｜ 工作量=4000 ｜ 研究:零部件 II(Components_D2)，技能:制作≥16 ｜ `DisassembleAIPersonaCore`
- 人造纤维(SyntheticFibers)×10 + 碳聚物(Carbon)×10 -> 碳纤维锭(CarbonAlloy)×10 @ 石化产品实验室(TableChemlab) ｜ 工作量=2800 ｜ 研究:石油化学 V(Oil_Industry_C6)，技能:制作≥15 ｜ `MakeCarbonAlloy`

#### 投石机组件箱(Catapult_Crate)

- [木质]×160 + [金属锭类]×60 -> 投石机组件箱(Catapult_Crate)×1 @ 手工加工台(CraftingSpot), 重型武器机床(HeavyArmsBench) ｜ 工作量=10500 ｜ 研究:攻城武器(Ballista_B1)，技能:制作≥5 ｜ `BuildCatapult_Crate`

#### 陶瓷(Ceramics)

- 软粘土(SoftClay)×20 + 玻璃生料(GlassBatch)×5 -> 陶瓷(Ceramics)×10 @ 电熔炼器(ElectricSmelter), 窑炉(Kiln) ｜ 工作量=1200 ｜ 研究:金属加工 I(Craft_B1)，技能:制作≥8 ｜ `MakeCeramics`

#### 电荷武器部件(Charged_Component)

- [超轻合金锭类]×10 + [高碳金属锭类]×20 + 磁性材料(MagneticMaterial)×2 + 先进机械组件(AdvMechanism)×2 -> 电荷武器部件(Charged_Component)×1 @ 未来武器工作台(MechWeaponCraftingWorkTable) ｜ 工作量=2100 ｜ 研究:定向能武器(ChargedShot)，技能:制作≥14 ｜ `MakeCharged_Component`

#### 化合燃料(Chemfuel)

- [食物]×40 -> 化合燃料(Chemfuel)×15 ｜ 工作量=2500 ｜ 技能:制作≥5 ｜ `MakeChemfuel`

#### 甲壳板(ChitinPlating)

- 甲壳(Chitin)×50 + 人造纤维(SyntheticFibers)×4 + 复合粘剂(Compaste)×1 -> 甲壳板(ChitinPlating)×15 @ 纺织机(TableLoom) ｜ 工作量=1080 ｜ 研究:服装生产 IV(AdvancedFabrication)，技能:制作≥12 ｜ `MakeChitinPlating`

#### 铬合金(Chromium)

- 铁矿石(Iron)×25 -> 铬合金(Chromium)×10 @ 电弧炉(EAF) ｜ 工作量=1200 ｜ 研究:金属加工 IV(Metals_C3)，技能:制作≥11 ｜ `ProcessForChromium`

#### 粘土砖(ClayBrick)

- 软粘土(SoftClay)×15 -> 粘土砖(ClayBrick)×15 @ 电熔炼器(ElectricSmelter), 窑炉(Kiln) ｜ 工作量=800 ｜ 研究:金属加工 I(Craft_B1)，技能:制作≥6 ｜ `MakeClayBricks`

#### 布(Cloth)

- 原棉(RawCotton)×20 -> 布(Cloth)×20 @ 纺织机(TableLoom) ｜ 工作量=600 ｜ 研究:服装生产 II(Apparel_B1)，技能:制作≥2 ｜ `MakeCloth`

#### 钴合金(Cobalt)

- 镍矿(Nickel)×30 -> 钴合金(Cobalt)×15 @ 电弧炉(EAF) ｜ 工作量=1300 ｜ 研究:金属加工 IV(Metals_C3)，技能:制作≥14 ｜ `ProcessCobalt`

#### 普通零部件(ComponentIndustrial)

- [轻合金锭类] 或 [超轻合金锭类]×30 -> 普通零部件(ComponentIndustrial)×10 @ 电动装配台(AdvToolBench), 手工装配台(FabricationBench) ｜ 工作量=2000 ｜ 研究:基础加工知识(Craft_0)，技能:制作≥3 ｜ `Make_ComponentIndustrial` ｜ PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/RecipeDefs/Recipes_Components.xml

#### 复合粘剂(Compaste)

- 化合燃料(Chemfuel)×10 + 合成橡胶(Rubber)×5 + 石蜡(Paraffins)×2 -> 复合粘剂(Compaste)×20 @ 纺织机(TableLoom) ｜ 工作量=3200 ｜ 研究:服装生产 IV(AdvancedFabrication)，技能:制作≥5 ｜ `MakeCompaste`

#### 混凝土(ConcreteResource)

- 沙(SandResource)×45 + 碎石(CrushedStone)×30 -> 混凝土(ConcreteResource)×15 @ 手动混凝土搅拌器(ConcreteMixer) ｜ 工作量=1200 ｜ 研究:混凝土 I(Concrete_C1)，技能:制作≥3 ｜ `MixConcrete`
- 沙(SandResource)×45 + 碎石(CrushedStone)×30 -> 混凝土(ConcreteResource)×15 @ 电动混凝土搅拌机(EConcreteMixer) ｜ 工作量=600 ｜ 研究:混凝土 II(Concrete_C2)，技能:制作≥6 ｜ `MixConcreteElectric`

#### 铜(CopperBar)

- 铜矿(Copper)×20 -> 铜(CopperBar)×20 @ 电熔炼器(ElectricSmelter) ｜ 工作量=500 ｜ 研究:金属加工 II(Metals_C1)，技能:制作≥4 ｜ `MakeCopperBars_Electric`
- 铜矿(Copper)×10 -> 铜(CopperBar)×10 @ 熔炉(TableFurnace) ｜ 工作量=700 ｜ 研究:金属加工 I(Craft_B1)，技能:制作≥1 ｜ `MakeCopperBars_Hand`

#### 贫铀合金(DepletedUranium)

- 铀矿(Uranium)×10 -> 贫铀合金(DepletedUranium)×10 @ 电弧炉(EAF) ｜ 工作量=1700 ｜ 研究:金属加工 IV(Metals_C3)，技能:制作≥11 ｜ `MakeDepletedUraniumAlloy`

#### 魔菇布(DevilstrandCloth)

- 生恶魔菇(RawDevilstrand)×15 + 人造纤维(SyntheticFibers)×2 -> 魔菇布(DevilstrandCloth)×15 @ 纺织机(TableLoom) ｜ 工作量=600 ｜ 研究:服装生产 IV(AdvancedFabrication)，技能:制作≥13 ｜ `MakeDevilstrand`

#### 电子元件(ElectronicComponents)

- [高碳金属锭类]×35 + 玻璃块(Glass)×10 + 普通零部件(ComponentIndustrial)×10 + 纯银合金(SilverBar)×10 -> 电子元件(ElectronicComponents)×5 @ 电子器件工作台(TableElectronics) ｜ 工作量=2500 ｜ 技能:制作≥8 ｜ `MakeElectronicComponents`

#### 钛铁合金(Plasteel)

- 钛合金(Titanium)×5 + 钢铁(Steel)×12 + 碳纤维锭(CarbonAlloy)×3 -> 钛铁合金(Plasteel)×15 @ 电弧炉(EAF) ｜ 工作量=2400 ｜ 研究:金属加工 V(Metals_D1)，技能:制作≥15 ｜ `MakeFerrotitaniumAlloy`

#### 柴火(Kindling)

- [木质]×20 -> 柴火(Kindling)×40 @ 电动锯木台(TableSawmillElectric) ｜ 工作量=500 ｜ 研究:金属加工 II(Metals_C1)，技能:制作≥4 ｜ `MakeKindling_Electric`
- [木质]×10 -> 柴火(Kindling)×20 @ 手工锯台(TableSawmillHand) ｜ 工作量=500 ｜ 研究:基础加工知识(Craft_0) ｜ `MakeKindling_Hand`

#### 玻璃生料(GlassBatch)

- 沙(SandResource)×75 -> 玻璃生料(GlassBatch)×25 @ 玻璃制作台(GlassworkTable), 熔炉(TableFurnace) ｜ 工作量=900 ｜ 研究:玻璃制作 I(Glass_B1)，技能:制作≥3 ｜ `MakeGlassBatch`

#### 玻璃块(Glass)

- 玻璃生料(GlassBatch)×20 -> 玻璃块(Glass)×10 @ 玻璃制作台(GlassworkTable), 熔炉(TableFurnace) ｜ 工作量=600 ｜ 研究:玻璃制作 I(Glass_B1)，技能:制作≥5 ｜ `MakeGlass`

#### 纯金合金(GoldBar)

- 金矿(Gold)×12 + [高碳金属]×3 -> 纯金合金(GoldBar)×15 @ 电熔炼器(ElectricSmelter) ｜ 工作量=500 ｜ 研究:金属加工 II(Metals_C1)，技能:制作≥6 ｜ `MakeGoldBars_Electric`
- 金矿(Gold)×12 + [高碳金属]×3 -> 纯金合金(GoldBar)×15 @ 熔炉(TableFurnace) ｜ 工作量=750 ｜ 研究:金属加工 I(Craft_B1)，技能:制作≥3 ｜ `MakeGoldBars_Hand`

#### 火药(Powder)

- 硝石(Nitre)×75 + [煤类]×20 + 硫磺(Sulfur)×20 -> 火药(Powder)×20 @ 弹药工作台(AmmoBench), 重型弹药工作台(TableHeavyAmmunition) ｜ 工作量=3000 ｜ 技能:制作≥3 ｜ `MakePowder`

#### 重型武器部件(Heavy_Component)

- [轻合金锭类] 或 [超轻合金锭类]×25 + [高碳金属锭类]×15 + 普通零部件(ComponentIndustrial)×12 + 电线(Wire)×15 + 机械组件(Mechanism)×6 -> 重型武器部件(Heavy_Component)×1 @ 先进武器加工台(AdvWeaponCraftingWorkTable), 重型武器机床(HeavyArmsBench) ｜ 工作量=2800 ｜ 研究:重型武器 I(HeavyTurrets)，技能:制作≥12 ｜ `MakeHeavy_Component`

#### 大麻布(HempCloth)

- 大麻叶(SmokeleafLeaves)×30 -> 大麻布(HempCloth)×15 @ 纺织机(TableLoom) ｜ 工作量=1200 ｜ 研究:服装生产 II(Apparel_B1)，技能:制作≥6 ｜ `MakeHempCloth`

#### 动力电池(Hexcell)

- 磁性材料(MagneticMaterial)×2 + [高碳金属锭类]×4 + 微芯片集成电路(Microchips)×1 -> 动力电池(Hexcell)×10 @ 电子器件工作台(TableElectronics) ｜ 工作量=6000 ｜ 研究:机器人工程 I(Droids_D1)，技能:制作≥14 ｜ `MakeHexcell`

#### 超织物(Hyperweave)

- 合成纤维(Synthread)×10 + 魔菇布(DevilstrandCloth)×10 -> 超织物(Hyperweave)×10 @ 纺织机(TableLoom) ｜ 工作量=600 ｜ 研究:服装生产 V(Apparel_D1)，技能:制作≥17 ｜ `MakeHyperweaveCloth`

#### 无机物(Matter)

- [原材料]×300 -> 无机物(Matter)×150 @ 无机物转化器(MatterConverter) ｜ 工作量=10000 ｜ 研究:物质转换 I(Super_matter_D1)，技能:制作≥15 ｜ `ConvertResourcesRaw`

#### 集成电路(Electronics)

- 护盾腰带(Apparel_ShieldBelt) 或 守护者护盾(Apparel_ShieldGuardian)×1 -> 先进零部件(ComponentAdvanced)×8 + 集成电路(Electronics)×1 @ 回收站(TableRecycling) ｜ 工作量=1000 ｜ 技能:制作≥10 ｜ `DeconstructPersonalShield`
- 硅(Silicon)×25 + 塑料(Plastic)×20 + 电子元件(ElectronicComponents)×3 + 纯金合金(GoldBar)×2 -> 集成电路(Electronics)×5 @ 电子器件工作台(TableElectronics) ｜ 工作量=3500 ｜ 研究:电子 II(Electronics_C2)，技能:制作≥12 ｜ `MakeIntegratedCircuits`

#### 凯夫拉（芳纶）(Kevlar)

- 塑料(Plastic)×30 + 人造纤维(SyntheticFibers)×15 -> 凯夫拉（芳纶）(Kevlar)×10 @ 纺织机(TableLoom) ｜ 工作量=1800 ｜ 研究:服装生产 IV(AdvancedFabrication)，技能:制作≥13 ｜ `MakeKevlar`

#### 光束武器部件(Laser_Component)

- [超轻合金锭类]×20 + [高碳金属锭类]×10 + 磁性材料(MagneticMaterial)×4 + 微芯片集成电路(Microchips)×1 -> 光束武器部件(Laser_Component)×1 @ 未来武器工作台(MechWeaponCraftingWorkTable) ｜ 工作量=2100 ｜ 研究:定向能武器(ChargedShot)，技能:制作≥15 ｜ `MakeLaser_Component`

#### 光束炮塔组件箱(LaserTurret_Crate)

- 武器部件(Weapon_Parts)×8 + 光束武器部件(Laser_Component)×1 + [金属锭类]×30 + 集成电路(Electronics)×5 -> 光束炮塔组件箱(LaserTurret_Crate)×1 @ 重型武器机床(HeavyArmsBench) ｜ 工作量=17500 ｜ 研究:自动炮塔 IV(Autoturrets_E1)，技能:制作≥17 ｜ `BuildLaserTurret_Crate`

#### 发射器部件(Launcher_Component)

- 集成电路(Electronics)×2 + 重型武器部件(Heavy_Component)×1 + 先进机械组件(AdvMechanism)×3 + 玻璃块(Glass)×12 -> 发射器部件(Launcher_Component)×1 @ 先进武器加工台(AdvWeaponCraftingWorkTable), 重型武器机床(HeavyArmsBench) ｜ 工作量=3100 ｜ 研究:火箭弹 I(Rockets_C1)，技能:制作≥14 ｜ `MakeLauncher_Component`

#### 铅合金(LeadBar)

- 铅矾矿(Anglesite)×20 -> 铅合金(LeadBar)×20 @ 电熔炼器(ElectricSmelter) ｜ 工作量=1000 ｜ 研究:金属加工 II(Metals_C1)，技能:制作≥6 ｜ `MakeLeadBars_Electric`
- 铅矾矿(Anglesite)×10 -> 铅合金(LeadBar)×10 @ 熔炉(TableFurnace) ｜ 工作量=1100 ｜ 研究:金属加工 I(Craft_B1)，技能:制作≥2 ｜ `MakeLeadBars_Hand`

#### 亚麻布(Flaxcloth)

- 生亚麻(RawFlax)×20 -> 亚麻布(Flaxcloth)×10 @ 皮革工作台(LeatherworkerTable), 纺织机(TableLoom) ｜ 工作量=600 ｜ 技能:制作≥4 ｜ `MakeLinen`

#### 磁性材料(MagneticMaterial)

- 无机物(Matter)×30 + 硫酸盐(Sulphates)×15 + [超轻合金锭类]×10 + [轻合金锭类]×5 -> 磁性材料(MagneticMaterial)×5 @ 自动化装配器(RobAssem) ｜ 工作量=2400 ｜ 研究:零部件 III(Components_D3)，技能:制作≥18 ｜ `BuildMagneticMaterial`

#### 机械组件(Mechanism)

- [轻合金锭类]×60 + 普通零部件(ComponentIndustrial)×20 -> 机械组件(Mechanism)×10 @ 电动装配台(AdvToolBench) ｜ 工作量=1800 ｜ 技能:制作≥7 ｜ `MakeSimpleMechanism_Electric`
- [轻合金锭类]×30 + 普通零部件(ComponentIndustrial)×10 -> 机械组件(Mechanism)×5 @ 手工装配台(FabricationBench) ｜ 工作量=1800 ｜ 技能:制作≥4 ｜ `MakeSimpleMechanism_Hand`

#### 梅尔基奥合金（铜镍铝合金）(CupronickelAlloy)

- 铜(CopperBar)×24 + 镍合金(NickelBar)×6 -> 梅尔基奥合金（铜镍铝合金）(CupronickelAlloy)×30 @ 电弧炉(EAF) ｜ 工作量=1600 ｜ 研究:金属加工 III(Metals_C2)，技能:制作≥12 ｜ `MakeCupronickelAlloy`

#### 修理包(MendingKit)

- 原始零部件(ComponentMedieval)×1 + 普通零部件(ComponentIndustrial)×1 + [织物]×10 + [轻合金锭类]×10 -> 修理包(MendingKit)×5 @ 电动装配台(AdvToolBench), 手工装配台(FabricationBench) ｜ 工作量=500 ｜ 研究:手工维修台(Repair_table_B1)，技能:制作≥8 ｜ `MakeMendingKit`

#### 金属罐(MetalCan)

- [金属锭类]×8 -> 金属罐(MetalCan)×15 @ 电熔炼器(ElectricSmelter) ｜ 工作量=800 ｜ 研究:金属加工 II(Metals_C1)，技能:制作≥2 ｜ `MakeMetalCan`

#### 微芯片集成电路(Microchips)

- 集成电路(Electronics)×2 + 碳纤维锭(CarbonAlloy)×5 + 纯金合金(GoldBar)×5 -> 微芯片集成电路(Microchips)×1 @ 电子器件工作台(TableElectronics) ｜ 工作量=3500 ｜ 研究:电子 III(Electronics_D1)，技能:制作≥16 ｜ `MakeMicrochipIntegratedCircuits`

#### 矿物声纳模块(MineralSonarModule)

- [金属锭类]×25 + 先进零部件(ComponentAdvanced)×5 + 电子元件(ElectronicComponents)×10 + 机械组件(Mechanism)×1 -> 矿物声纳模块(MineralSonarModule)×1 @ 电动装配台(AdvToolBench), 电子器件工作台(TableElectronics) ｜ 工作量=4800 ｜ 研究:采矿学 II(Mining_C1)，技能:制作≥10 ｜ `MakeMineralSonarModule`

#### 凝固汽油(Napalm)

- 化合燃料(Chemfuel)×30 + 聚合物(Polymers)×15 -> 凝固汽油(Napalm)×10 @ 弹药工作台(AmmoBench), 重型弹药工作台(TableHeavyAmmunition) ｜ 工作量=1200 ｜ 技能:制作≥5 ｜ `MakeNapalm`

#### 镍合金(NickelBar)

- 镍矿(Nickel)×15 -> 镍合金(NickelBar)×15 @ 电弧炉(EAF) ｜ 工作量=1500 ｜ 研究:金属加工 IV(Metals_C3)，技能:制作≥13 ｜ `ProcessNickelOre`

#### 镍钛合金(NitinolAlloy)

- 钛合金(Titanium)×4 + 镍合金(NickelBar)×11 -> 镍钛合金(NitinolAlloy)×15 @ 电弧炉(EAF) ｜ 工作量=2600 ｜ 研究:金属加工 V(Metals_D1)，技能:制作≥16 ｜ `MakeNitinolAlloy`

#### 氧化化合燃料(OxidizedChemfuel)

- 化合燃料(Chemfuel)×100 + 硝石(Nitre)×10 + 硫酸盐(Sulphates)×10 + 钷素燃料(Prometheum) 或 含胺乳化燃料(FSX)×5 -> 氧化化合燃料(OxidizedChemfuel)×75 @ 石化产品实验室(TableChemlab) ｜ 工作量=10000 ｜ 研究:基础逆重科技(BasicGravtech)，技能:制作≥12 ｜ `MakeOxidizedChemfuel`

#### 绘画用品(PaintingSupplies)

- 野玫瑰(WildRose) 或 金丝桃(Hypericum) 或 薄荷叶(MintLeaves)×10 + [木本植物]×2 -> 绘画用品(PaintingSupplies)×1 @ 电力炉灶(ElectricStove), 燃料炉灶(FueledStove) ｜ 工作量=550 ｜ 研究:艺术 I(Art_A1)，技能:制作≥5 ｜ `CookPaint`
- 染料(Dye)×10 + [木本植物]×2 -> 绘画用品(PaintingSupplies)×1 @ 电力炉灶(ElectricStove), 燃料炉灶(FueledStove) ｜ 工作量=550 ｜ 研究:艺术 I(Art_A1)，技能:制作≥5 ｜ `CookPaintDye`

#### 手枪部件(Pistol_Component)

- [轻合金锭类] 或 [超轻合金锭类]×6 + [高碳金属锭类]×3 + 普通零部件(ComponentIndustrial)×2 + 电线(Wire)×1 + 机械组件(Mechanism)×1 -> 手枪部件(Pistol_Component)×1 @ 先进武器加工台(AdvWeaponCraftingWorkTable), 武器加工台(WeaponCraftingWorkTable) ｜ 工作量=1200 ｜ 研究:手枪 I(Gunsmithing)，技能:制作≥6 ｜ `MakePistol_Component`

#### 离子武器部件(Plasma_Component)

- [超轻合金锭类]×20 + [高碳金属锭类]×10 + 生物合成材料(BiosyntheticMaterial)×4 + 微芯片集成电路(Microchips)×1 -> 离子武器部件(Plasma_Component)×1 @ 未来武器工作台(MechWeaponCraftingWorkTable) ｜ 工作量=2100 ｜ 研究:定向能武器(ChargedShot)，技能:制作≥15 ｜ `MakePlasma_Component`

#### 钨钴合金(PobediteAlloy)

- 钨合金(Tungsten)×3 + 碳纤维锭(CarbonAlloy)×3 + 钴合金(Cobalt)×3 -> 钨钴合金(PobediteAlloy)×10 @ 电弧炉(EAF) ｜ 工作量=2700 ｜ 研究:金属加工 V(Metals_D1)，技能:制作≥18 ｜ `MakePobediteAlloy`

#### 原始零部件(ComponentMedieval)

- [木本植物]×20 -> 原始零部件(ComponentMedieval)×10 @ 电动装配台(AdvToolBench), 手工加工台(CraftingSpot), 手工装配台(FabricationBench), 电动锯木台(TableSawmillElectric), 手工锯台(TableSawmillHand) ｜ 工作量=900 ｜ 研究:基础加工知识(Craft_0) ｜ `Make_ComponentMedieval`
- [轻合金锭类] 或 [高碳金属锭类] 或 [耐压合金锭类]×10 -> 原始零部件(ComponentMedieval)×10 @ 电动装配台(AdvToolBench), 手工加工台(CraftingSpot), 手工装配台(FabricationBench) ｜ 工作量=900 ｜ 研究:基础加工知识(Craft_0) ｜ `Make_ComponentMedievalBronze`

#### 红木木板(RedWoodPlank)

- 红木(RedWoodLog)×20 -> 红木木板(RedWoodPlank)×30 @ 电动锯木台(TableSawmillElectric) ｜ 工作量=1200 ｜ 研究:金属加工 II(Metals_C1)，技能:制作≥6 ｜ `MakeRedWoodPlanks_Electric`
- 红木(RedWoodLog)×10 -> 红木木板(RedWoodPlank)×15 @ 手工锯台(TableSawmillHand) ｜ 工作量=1200 ｜ 研究:基础加工知识(Craft_0)，技能:制作≥3 ｜ `MakeRedWoodPlanks_Hand`

#### 钢筋混凝土(ReinforcedConcrete)

- [金属锭类]×20 + 混凝土(ConcreteResource)×20 -> 钢筋混凝土(ReinforcedConcrete)×16 @ 电动装配台(AdvToolBench) ｜ 工作量=1200 ｜ 技能:制作≥7 ｜ `MakeReinforcedConcrete_Electric`
- [金属锭类]×10 + 混凝土(ConcreteResource)×10 -> 钢筋混凝土(ReinforcedConcrete)×8 @ 手动石料加工台(TableStonecutter) ｜ 工作量=1600 ｜ 研究:混凝土 I(Concrete_C1)，技能:制作≥4 ｜ `MakeReinforcedConcrete_Hand`

#### 强化玻璃块(ReinforcedGlass)

- 玻璃生料(GlassBatch)×40 + 碳聚物(Carbon)×5 + 人造纤维(SyntheticFibers)×5 -> 强化玻璃块(ReinforcedGlass)×5 @ 玻璃制作台(GlassworkTable) ｜ 工作量=1400 ｜ 研究:玻璃制作 II(Glass_C1)，技能:制作≥8 ｜ `MakeReinforcedGlass`

#### 步枪部件(Rifle_Component)

- [轻合金锭类] 或 [超轻合金锭类]×20 + [高碳金属锭类]×10 + 普通零部件(ComponentIndustrial)×6 + 电线(Wire)×8 + 机械组件(Mechanism)×3 -> 步枪部件(Rifle_Component)×1 @ 先进武器加工台(AdvWeaponCraftingWorkTable), 武器加工台(WeaponCraftingWorkTable) ｜ 工作量=2100 ｜ 研究:步枪 I(Rifles_C1)，技能:制作≥8 ｜ `MakeRifle_Component`

#### 机器人部件(RobotParts)

- [超轻合金锭类]×60 + 先进机械组件(AdvMechanism)×15 -> 机器人部件(RobotParts)×25 @ 电子器件工作台(TableElectronics) ｜ 工作量=8000 ｜ 研究:机器人工程 I(Droids_D1)，技能:制作≥15 ｜ `MakeRobotParts`

#### 斯特合金（铁硅铝磁合金）(FerrosiliconAlloy)

- 钢铁(Steel)×15 + 硅(Silicon)×10 + 铝合金(AluminiumBar)×5 -> 斯特合金（铁硅铝磁合金）(FerrosiliconAlloy)×30 @ 电熔炼器(ElectricSmelter) ｜ 工作量=1700 ｜ 研究:金属加工 III(Metals_C2)，技能:制作≥8 ｜ `MakeFerrosiliconAlloy`

#### 自动哨戒炮塔组件箱(SentryBlasterTurret_Crate)

- 武器部件(Weapon_Parts)×8 + 离子武器部件(Plasma_Component)×1 + [金属锭类]×30 + 集成电路(Electronics)×5 -> 自动哨戒炮塔组件箱(SentryBlasterTurret_Crate)×1 @ 重型武器机床(HeavyArmsBench) ｜ 工作量=17500 ｜ 研究:自动炮塔 IV(Autoturrets_E1)，技能:制作≥17 ｜ `BuildSentryBlasterTurret_Crate`

#### 霰弹枪部件(Shotgun_Component)

- [轻合金锭类] 或 [超轻合金锭类]×15 + [高碳金属锭类]×8 + 普通零部件(ComponentIndustrial)×4 + 电线(Wire)×5 + 机械组件(Mechanism)×2 -> 霰弹枪部件(Shotgun_Component)×1 @ 先进武器加工台(AdvWeaponCraftingWorkTable), 武器加工台(WeaponCraftingWorkTable) ｜ 工作量=1800 ｜ 研究:霰弹枪 I(GasOperation)，技能:制作≥7 ｜ `MakeShotgun_Component`

#### 冲锋枪部件(SMG_Component)

- [轻合金锭类] 或 [超轻合金锭类]×10 + [高碳金属锭类]×5 + 普通零部件(ComponentIndustrial)×4 + 电线(Wire)×6 + 机械组件(Mechanism)×2 -> 冲锋枪部件(SMG_Component)×1 @ 先进武器加工台(AdvWeaponCraftingWorkTable), 武器加工台(WeaponCraftingWorkTable) ｜ 工作量=1800 ｜ 研究:冲锋枪 I(BlowbackOperation)，技能:制作≥9 ｜ `MakeSMG_Component`

#### 狙击枪部件(Sniper_Component)

- 步枪部件(Rifle_Component)×1 + 普通零部件(ComponentIndustrial)×6 + 玻璃块(Glass)×12 -> 狙击枪部件(Sniper_Component)×1 @ 先进武器加工台(AdvWeaponCraftingWorkTable), 武器加工台(WeaponCraftingWorkTable) ｜ 工作量=2100 ｜ 研究:狙击步枪 I(Sniper_rifles_C1)，技能:制作≥10 ｜ `MakeSniper_Component`

#### 高级零部件(ComponentSpacer)

- [超轻合金锭类]×25 + [耐压合金锭类]×5 -> 高级零部件(ComponentSpacer)×10 @ 电动装配台(AdvToolBench) ｜ 工作量=3500 ｜ 研究:零部件 I(Components_D1)，技能:制作≥9 ｜ `Make_ComponentSpacer` ｜ PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/RecipeDefs/Recipes_Components.xml

#### 钢铁(Steel)

- 铁矿石(Iron)×15 + [煤类]×7 -> 钢铁(Steel)×20 @ 电熔炼器(ElectricSmelter) ｜ 工作量=900 ｜ 研究:金属加工 II(Metals_C1)，技能:制作≥6 ｜ `MakeSteelBars_Electric`
- 铁矿石(Iron)×15 + [煤类]×13 -> 钢铁(Steel)×20 @ 熔炉(TableFurnace) ｜ 工作量=1300 ｜ 研究:金属加工 I(Craft_B1)，技能:制作≥3 ｜ `MakeSteelBars_Hand`

#### 斯坦合金(SteelBar)

- 钢铁(Steel)×15 + 镍合金(NickelBar)×5 -> 斯坦合金(SteelBar)×20 @ 电弧炉(EAF) ｜ 工作量=1800 ｜ 研究:金属加工 IV(Metals_C3)，技能:制作≥11 ｜ `MakeFerronickelAlloy`

#### 司太立合金（钨铬钴合金）(StelliteAlloy)

- 钴合金(Cobalt)×5 + 铬合金(Chromium)×13 + 钨合金(Tungsten)×2 -> 司太立合金（钨铬钴合金）(StelliteAlloy)×20 @ 电弧炉(EAF) ｜ 工作量=2100 ｜ 研究:金属加工 V(Metals_D1)，技能:制作≥17 ｜ `MakeStelliteAlloy`

#### 纯银合金(SilverBar)

- 白银(Silver)×20 + [高碳金属]×5 -> 纯银合金(SilverBar)×15 @ 电熔炼器(ElectricSmelter) ｜ 工作量=650 ｜ 研究:金属加工 II(Metals_C1)，技能:制作≥5 ｜ `MakeSilverBars_Electric`
- 白银(Silver)×14 + [高碳金属]×3 -> 纯银合金(SilverBar)×10 @ 熔炉(TableFurnace) ｜ 工作量=750 ｜ 研究:金属加工 I(Craft_B1)，技能:制作≥2 ｜ `MakeSilverBars_Hand`

#### 合成纤维(Synthread)

- 原棉(RawCotton)×10 + 人造纤维(SyntheticFibers)×3 -> 合成纤维(Synthread)×10 @ 纺织机(TableLoom) ｜ 工作量=600 ｜ 研究:服装生产 IV(AdvancedFabrication)，技能:制作≥12 ｜ `MakeSynthreadCloth`

#### 兽脂(Tallow)

- [生肉]×30 -> 兽脂(Tallow)×15 @ 篝火(Campfire), 燃料炉灶(FueledStove) ｜ 工作量=1200 ｜ 研究:食品工程原理 I(Food_00)，技能:烹饪≥3 ｜ `MakeTallow`

#### 锡(TinBar)

- 锡矿(Tin)×20 -> 锡(TinBar)×20 @ 电熔炼器(ElectricSmelter) ｜ 工作量=500 ｜ 研究:金属加工 II(Metals_C1)，技能:制作≥3 ｜ `MakeTinBars_Electric`
- 锡矿(Tin)×10 -> 锡(TinBar)×10 @ 熔炉(TableFurnace) ｜ 工作量=600 ｜ 研究:金属加工 I(Craft_B1)，技能:制作≥1 ｜ `MakeTinBars_Hand`

#### 钛合金(Titanium)

- 钛铁矿(Ilmenite)×15 -> 钛合金(Titanium)×15 @ 电弧炉(EAF) ｜ 工作量=3300 ｜ 研究:金属加工 V(Metals_D1)，技能:制作≥16 ｜ `ProcessTitanium`

#### 钨合金(Tungsten)

- 黑钨矿(Wolframite)×15 -> 钨合金(Tungsten)×15 @ 电弧炉(EAF) ｜ 工作量=3000 ｜ 研究:金属加工 V(Metals_D1)，技能:制作≥17 ｜ `ProcessTungsten`

#### 超级零部件(ComponentUltra)

- [稀有合金锭类]×15 + [超硬合金锭类]×10 + [耐压合金锭类]×5 -> 超级零部件(ComponentUltra)×10 @ 自动化装配器(RobAssem) ｜ 工作量=4500 ｜ 研究:零部件 IV(Components_E1)，技能:制作≥13 ｜ `Make_ComponentUltra`

#### 丝绒(Velour)

- 复合粘剂(Compaste)×5 + [基础织物]×40 -> 丝绒(Velour)×25 @ 纺织机(TableLoom) ｜ 工作量=1800 ｜ 研究:服装生产 IV(AdvancedFabrication)，技能:制作≥12 ｜ `MakeVelour`

#### 武器部件(Weapon_Parts)

- [金属锭类]×25 + 普通零部件(ComponentIndustrial)×10 + 机械组件(Mechanism)×5 -> 武器部件(Weapon_Parts)×5 @ 先进武器加工台(AdvWeaponCraftingWorkTable), 未来武器工作台(MechWeaponCraftingWorkTable), 武器加工台(WeaponCraftingWorkTable) ｜ 工作量=1500 ｜ 研究:枪械(Machining)，技能:制作≥6 ｜ `MakeWeapon_Parts`

#### 电线(Wire)

- [高碳金属锭类]×20 -> 电线(Wire)×40 @ 电动装配台(AdvToolBench) ｜ 工作量=800 ｜ 技能:制作≥6 ｜ `MakeWires_Electric`
- [高碳金属锭类]×10 -> 电线(Wire)×20 @ 手工装配台(FabricationBench) ｜ 工作量=800 ｜ 研究:基础加工知识(Craft_0)，技能:制作≥3 ｜ `MakeWires_Hand`

#### 木板(WoodPlank)

- 原木(WoodLog)×20 -> 木板(WoodPlank)×30 @ 电动锯木台(TableSawmillElectric) ｜ 工作量=1200 ｜ 研究:金属加工 II(Metals_C1)，技能:制作≥6 ｜ `MakeWoodPlanks_Electric`
- 原木(WoodLog)×10 -> 木板(WoodPlank)×15 @ 手工锯台(TableSawmillHand) ｜ 工作量=1200 ｜ 研究:基础加工知识(Craft_0)，技能:制作≥3 ｜ `MakeWoodPlanks_Hand`

### 3.3 食物（86）

#### 苹果酒(Alcohol_Cider)

- 苹果酒箱(Crate_Cider)×1 -> 苹果酒(Alcohol_Cider)×75 @ 酿造台(Brewery), 电动酿造台(ElectricBrewery) ｜ 工作量=50 ｜ 研究:酿酒 I(Brewing) ｜ `EmptyCrate_cider`

#### 苹果白兰地(Alcohol_Applejack)

- 苹果白兰地箱(Crate_Applejack)×1 -> 苹果白兰地(Alcohol_Applejack)×75 @ 酿造台(Brewery), 电动酿造台(ElectricBrewery) ｜ 工作量=50 ｜ 研究:酿酒 I(Brewing) ｜ `EmptyCrate_Applejack`

#### 亚力酒(Alcohol_Arak)

- 亚力酒箱(Crate_Arak)×1 -> 亚力酒(Alcohol_Arak)×75 @ 酿造台(Brewery), 电动酿造台(ElectricBrewery) ｜ 工作量=50 ｜ 研究:酿酒 I(Brewing) ｜ `EmptyCrate_Arak`

#### 罐装主食(BasicCannedFood)

- [基础植物食材]×75 + 金属罐(MetalCan)×5 -> 罐装主食(BasicCannedFood)×75 @ 电压力锅(Canningstove) ｜ 工作量=1800 ｜ 研究:烹饪学 V(Food_C7) ｜ `MakeBasicCannedFood`

#### 浆果酒酿料(berrymash)

- 草莓(RawBerries)×35 + 酵母(Yeast)×5 -> 浆果酒酿料(berrymash)×10 @ 酿造台(Brewery), 电动酿造台(ElectricBrewery) ｜ 工作量=600 ｜ 研究:酿酒 I(Brewing)，技能:烹饪≥8 ｜ `Makeberrymash`

#### 浆果酒(Alcohol_BerryWine)

- 浆果酒箱(Crate_BerryWine)×1 -> 浆果酒(Alcohol_BerryWine)×75 @ 酿造台(Brewery), 电动酿造台(ElectricBrewery) ｜ 工作量=50 ｜ 研究:酿酒 I(Brewing) ｜ `EmptyCrate_strawberrywine`

#### 白兰地(Alcohol_Brandy)

- 白兰地箱(Crate_Brandy)×1 -> 白兰地(Alcohol_Brandy)×75 @ 酿造台(Brewery), 电动酿造台(ElectricBrewery) ｜ 工作量=50 ｜ 研究:酿酒 I(Brewing) ｜ `EmptyCrate_Brandy`

#### 面包(bread)

- 面粉(Flour)×10 -> 面包(bread)×2 @ 电烤箱(ElectricOven), 烤箱(TableOven) ｜ 研究:烘培 I(Bakery_B1)，技能:烹饪≥2 ｜ `MakeBread`

#### 罐装精肉(MetalCannedMeat_prime)

- 驼鹿肉(Meat_Muffalo) 或 [动物产品(生)]×75 + 金属罐(MetalCan)×5 -> 罐装精肉(MetalCannedMeat_prime)×75 @ 电压力锅(Canningstove) ｜ 工作量=1800 ｜ 研究:烹饪学 V(Food_C7) ｜ `MakeMetalCannedMeat_prime`

#### 罐装生肉(MetalCannedMeat_raw)

- [生肉]×75 + 金属罐(MetalCan)×5 -> 罐装生肉(MetalCannedMeat_raw)×75 @ 电压力锅(Canningstove) ｜ 工作量=1800 ｜ 研究:烹饪学 V(Food_C7) ｜ `MakeMetalCannedMeat_raw`

#### 焦糖(Caramel)

- 糖(Sugar)×9 -> 焦糖(Caramel)×3 @ 糖果桌(CandyTable) ｜ 工作量=900 ｜ 研究:甜品(Food_C4)，技能:烹饪≥6 ｜ `MakeCaramel`

#### 奶酪(Cheese)

- [烹饪辅料]×0.6 营养值 -> 奶酪(Cheese)×4 @ 厨房案桌(TableKitchen) ｜ 工作量=850 ｜ 研究:食物学 II(Food_B1)，技能:烹饪≥4 ｜ `Makecheese`

#### 芝士汉堡(CheeseBurger)

- 驼鹿肉(Meat_Muffalo) 或 罐装精肉(MetalCannedMeat_prime)×0.5 营养值 + 面包(bread)×0.6 营养值 + 陈年奶酪(AgedCheese) 或 奶酪(Cheese)×0.3 营养值 + 西红柿(RawTomatoes)×0.3 营养值 -> 芝士汉堡(CheeseBurger)×2 @ 专业化烹饪炉(ElectricStove_Pro) ｜ 工作量=900 ｜ 研究:高级烹饪学 II(Food_C6)，技能:烹饪≥13 ｜ `MakeBurgerCheese`

#### 芝士三明治(GrilledCheese)

- 奶酪(Cheese) 或 陈年奶酪(AgedCheese)×0.2 营养值 + 面包(bread)×0.1 营养值 -> 芝士三明治(GrilledCheese)×2 @ 电力炉灶(ElectricStove), 燃料炉灶(FueledStove), 简易烤架(TableGrill) ｜ 工作量=700 ｜ 研究:烹饪学 I(Food_B3)，技能:烹饪≥5 ｜ `MakeCheeseGrilled`
- 奶酪(Cheese) 或 陈年奶酪(AgedCheese)×1 营养值 + 面包(bread)×0.5 营养值 -> 芝士三明治(GrilledCheese)×8 @ 专业化烹饪炉(ElectricStove_Pro) ｜ 工作量=1900 ｜ 研究:烹饪学 IV(PackagedSurvivalMeal)，技能:烹饪≥7 ｜ `MakeCheeseGrilledBulk`

#### 巧克力牛奶(Chocomilk)

- 巧克力(Chocolate)×0.3 营养值 + [烹饪辅料]×0.9 营养值 -> 巧克力牛奶(Chocomilk)×3 @ 咖啡机(TableCoffee) ｜ 工作量=900 ｜ 研究:饮品(Food_C3)，技能:烹饪≥6 ｜ `MakeChocoMilk`

#### 苹果酒酿料(cidermash)

- 苹果(Rawapple)×35 + 酵母(Yeast)×5 -> 苹果酒酿料(cidermash)×10 @ 酿造台(Brewery), 电动酿造台(ElectricBrewery) ｜ 工作量=600 ｜ 研究:酿酒 I(Brewing)，技能:烹饪≥7 ｜ `Makecidermash`

#### 椰香咖喱(CoconutCurry)

- 豆腐(Tofu) 或 [生肉]×0.3 营养值 + 椰浆(coconutmilk)×0.4 营养值 + 薄荷叶(MintLeaves)×0.1 营养值 + 稻米(RawRice)×0.4 营养值 -> 椰香咖喱(CoconutCurry)×2 @ 电力炉灶(ElectricStove) ｜ 工作量=700 ｜ 研究:高级烹饪学 I(Food_C2)，技能:烹饪≥9 ｜ `MakeCoconutCurry`
- 豆腐(Tofu) 或 [生肉]×1.5 营养值 + 椰浆(coconutmilk)×2.5 营养值 + 稻米(RawRice)×2 营养值 -> 椰香咖喱(CoconutCurry)×8 @ 专业化烹饪炉(ElectricStove_Pro) ｜ 工作量=2000 ｜ 研究:烹饪学 IV(PackagedSurvivalMeal)，技能:烹饪≥11 ｜ `MakeCoconutCurryBulk`

#### 椰浆(coconutmilk)

- 椰子(RawCoconut)×0.5 营养值 -> 椰浆(coconutmilk)×5 @ 屠宰台(TableButcher), 厨房案桌(TableKitchen) ｜ 工作量=500 ｜ 研究:干肉饼(Pemmican) ｜ `HalfCoconut`

#### 干邑(Alcohol_Cognac)

- 干邑箱(Crate_Cognac)×1 -> 干邑(Alcohol_Cognac)×75 @ 酿造台(Brewery), 电动酿造台(ElectricBrewery) ｜ 工作量=50 ｜ 研究:酿酒 I(Brewing) ｜ `EmptyCrate_Cognac`

#### 曲奇(cookie)

- 糖(Sugar)×0.4 营养值 + 面粉(Flour)×0.3 营养值 -> 曲奇(cookie)×10 @ 电烤箱(ElectricOven), 烤箱(TableOven) ｜ 工作量=800 ｜ 研究:烘培 II(Bakery_B2)，技能:烹饪≥6 ｜ `MakeCookies`

#### 咖啡(CupCoffee)

- 咖啡豆（烘焙）(RoastedCoffeeBeans)×15 -> 咖啡(CupCoffee)×5 @ 咖啡机(TableCoffee) ｜ 工作量=1800 ｜ 研究:饮品(Food_C3)，技能:烹饪≥2 ｜ `BrewCoffee`
- 咖啡粉(GroundCoffee)×5 -> 咖啡(CupCoffee)×1 @ 篝火(Campfire), 电力炉灶(ElectricStove), 燃料炉灶(FueledStove) ｜ 工作量=500 ｜ 研究:食物学 III(Food_B2)，技能:烹饪≥8 ｜ `BrewCoffeeSingle`

#### 茶(Cuptea)

- 茶叶(RawTea)×15 + 糖(Sugar)×3 -> 茶(Cuptea)×3 @ 篝火(Campfire), 电力炉灶(ElectricStove), 燃料炉灶(FueledStove) ｜ 工作量=1200 ｜ 研究:食品工程原理 I(Food_00)，技能:烹饪≥2 ｜ `BrewTea`
- 茶叶(RawTea)×60 + 糖(Sugar)×12 -> 茶(Cuptea)×12 @ 咖啡机(TableCoffee) ｜ 工作量=2400 ｜ 研究:饮品(Food_C3)，技能:烹饪≥8 ｜ `BrewTeaBulk`

#### 罐装副食(ExtraCannedFood)

- [其他植物食材]×75 + 金属罐(MetalCan)×5 -> 罐装副食(ExtraCannedFood)×75 @ 电压力锅(Canningstove) ｜ 工作量=1800 ｜ 研究:烹饪学 V(Food_C7) ｜ `MakeExtraCannedFood`

#### 鱼饵(FishingBait)

- [基础植物食材] 或 [水果(生)] 或 [生肉]×2.5 营养值 -> 鱼饵(FishingBait)×5 @ 屠宰点(ButcherSpot), 屠宰台(TableButcher), 厨房案桌(TableKitchen) ｜ 工作量=1000 ｜ 研究:渔猎 I(Fish_A1)，技能:制作≥1 ｜ `MakeFishingBait`

#### 面粉(Flour)

- 小麦(Rawwheat) 或 玉米(RawCorn) 或 稻米(RawRice)×20 -> 面粉(Flour)×10 @ 石磨(GrindStone), 电磨(MillElectric) ｜ 工作量=600 ｜ 研究:烘培 I(Bakery_B1)，技能:烹饪≥2 ｜ `MakeFlour`

#### 水果饮料(FruitDrink)

- [水果(生)]×0.8 营养值 -> 水果饮料(FruitDrink)×3 @ 厨房案桌(TableKitchen) ｜ 工作量=1350 ｜ 研究:食物学 III(Food_B2)，技能:烹饪≥4 ｜ `MakeFruitDrink`

#### 水果派(PieBlueberry)

- 面粉(Flour)×0.08 营养值 + [水果(生)]×0.4 营养值 -> 水果派(PieBlueberry)×2 @ 电烤箱(ElectricOven), 烤箱(TableOven) ｜ 工作量=1200 ｜ 研究:烘培 II(Bakery_B2)，技能:烹饪≥8 ｜ `MakePieBlueberry`

#### 水果酸奶(FruitYogurt)

- [水果(生)]×0.4 营养值 + [烹饪辅料]×0.6 营养值 -> 水果酸奶(FruitYogurt)×3 @ 厨房案桌(TableKitchen) ｜ 研究:食物学 III(Food_B2)，技能:烹饪≥8 ｜ `MakeFruitYogurt`
- [水果(生)]×1.6 营养值 + [烹饪辅料]×2.5 营养值 -> 水果酸奶(FruitYogurt)×10 @ 厨房案桌(TableKitchen) ｜ 研究:烹饪学 IV(PackagedSurvivalMeal)，技能:烹饪≥12 ｜ `MakeFruitYogurt_Bulk`

#### 金酒(Alcohol_Gin)

- 金酒箱(Crate_Gin)×1 -> 金酒(Alcohol_Gin)×75 @ 酿造台(Brewery), 电动酿造台(ElectricBrewery) ｜ 工作量=50 ｜ 研究:酿酒 I(Brewing) ｜ `EmptyCrate_Gin`

#### 威士忌酿料(grainmash)

- 小麦(Rawwheat)×25 + 酵母(Yeast)×10 -> 威士忌酿料(grainmash)×10 @ 酿造台(Brewery), 电动酿造台(ElectricBrewery) ｜ 工作量=800 ｜ 研究:酿酒 II(Alcohol_B2)，技能:烹饪≥5 ｜ `Makegrainmash`

#### 咖啡粉(GroundCoffee)

- 咖啡豆（烘焙）(RoastedCoffeeBeans)×5 -> 咖啡粉(GroundCoffee)×5 @ 石磨(GrindStone), 电磨(MillElectric), 屠宰台(TableButcher), 厨房案桌(TableKitchen) ｜ 工作量=300 ｜ 研究:干肉饼(Pemmican)，技能:烹饪≥6 ｜ `GrindingCoffeeBeans`

#### 炖菜(HeartyStew)

- [生肉] 或 [替代生肉]×0.5 营养值 + 土豆(RawPotatoes)×0.4 营养值 + 胡萝卜(RawCarrots)×0.3 营养值 -> 炖菜(HeartyStew)×2 @ 电力炉灶(ElectricStove) ｜ 工作量=700 ｜ 研究:烹饪学 III(Food_C1)，技能:烹饪≥8 ｜ `CookHeartyStew`
- [生肉] 或 [替代生肉]×2.5 营养值 + 土豆(RawPotatoes)×2 营养值 + 胡萝卜(RawCarrots)×1.5 营养值 -> 炖菜(HeartyStew)×8 @ 专业化烹饪炉(ElectricStove_Pro) ｜ 工作量=1900 ｜ 研究:烹饪学 IV(PackagedSurvivalMeal)，技能:烹饪≥10 ｜ `CookHeartyStewBulk`

#### 冰淇淋(IceCream)

- 奶(Milk)×12 + 糖(Sugar) 或 野玫瑰(WildRose) 或 [水果(生)]×5 -> 冰淇淋(IceCream)×3 @ 糖果桌(CandyTable) ｜ 工作量=900 ｜ 研究:甜品(Food_C4)，技能:烹饪≥6 ｜ `MakeIceCream`

#### 卡鲁哇(Alcohol_Kahlua)

- 卡鲁哇箱(Crate_Kahlua)×1 -> 卡鲁哇(Alcohol_Kahlua)×75 @ 酿造台(Brewery), 电动酿造台(ElectricBrewery) ｜ 工作量=50 ｜ 研究:酿酒 I(Brewing) ｜ `EmptyCrate_Kahlua`

#### 烤肉串(Kebab)

- [生肉]×0.7 营养值 + 西红柿(RawTomatoes)×0.3 营养值 + [菌类(生)]×0.3 营养值 -> 烤肉串(Kebab)×2 @ 电力炉灶(ElectricStove) ｜ 工作量=700 ｜ 研究:烹饪学 III(Food_C1)，技能:烹饪≥8 ｜ `MakeKebab`
- [生肉]×3.5 营养值 + 西红柿(RawTomatoes)×1.5 营养值 + [菌类(生)]×1.5 营养值 -> 烤肉串(Kebab)×8 @ 专业化烹饪炉(ElectricStove_Pro) ｜ 工作量=1900 ｜ 研究:烹饪学 IV(PackagedSurvivalMeal)，技能:烹饪≥10 ｜ `MakeKebabBulk`

#### 细卷(MakiRolls)

- Meat_ThingDefFishSduiggles(Meat_ThingDefFishSduiggles)×0.5 营养值 + 水藻(RawAlgae)×0.2 营养值 + 稻米(RawRice)×0.5 营养值 -> 细卷(MakiRolls)×2 @ 电力炉灶(ElectricStove) ｜ 工作量=700 ｜ 研究:高级烹饪学 I(Food_C2)，技能:烹饪≥9 ｜ `MakeMakiRolls`
- Meat_ThingDefFishSduiggles(Meat_ThingDefFishSduiggles)×2.5 营养值 + 水藻(RawAlgae)×1 营养值 + 稻米(RawRice)×2.5 营养值 -> 细卷(MakiRolls)×8 @ 专业化烹饪炉(ElectricStove_Pro) ｜ 工作量=1900 ｜ 研究:烹饪学 IV(PackagedSurvivalMeal)，技能:烹饪≥11 ｜ `MakeMakiRollsBulk`

#### 肉汤(MeatSoup)

- [生肉] 或 [替代生肉]×0.65 营养值 + [基础植物食材] 或 [菌类(生)] 或 [其他植物食材]×0.5 营养值 -> 肉汤(MeatSoup)×3 @ 电力炉灶(ElectricStove), 燃料炉灶(FueledStove), 开放式炉灶(OpenHearth) ｜ 工作量=700 ｜ 研究:烹饪学 II(Food_B4)，技能:烹饪≥4 ｜ `CookMeatSoup`
- [生肉] 或 [替代生肉]×3.25 营养值 + [基础植物食材] 或 [菌类(生)] 或 [其他植物食材]×2.5 营养值 -> 肉汤(MeatSoup)×12 @ 专业化烹饪炉(ElectricStove_Pro) ｜ 工作量=1900 ｜ 研究:烹饪学 IV(PackagedSurvivalMeal)，技能:烹饪≥6 ｜ `CookMeatSoupBulk`

#### 味噌汤(MisoSoup)

- 豆腐(Tofu)×0.5 营养值 + [菌类(生)]×0.4 营养值 + 洋葱(Rawonion)×0.03 营养值 + 水藻(RawAlgae)×0.25 营养值 -> 味噌汤(MisoSoup)×2 @ 专业化烹饪炉(ElectricStove_Pro) ｜ 工作量=900 ｜ 研究:高级烹饪学 II(Food_C6)，技能:烹饪≥14 ｜ `MakeMisoSoup`

#### 私酿酒(Alcohol_Moonshine)

- 私酿酒箱(Crate_Moonshine)×1 -> 私酿酒(Alcohol_Moonshine)×75 @ 酿造台(Brewery), 电动酿造台(ElectricBrewery) ｜ 工作量=50 ｜ 研究:酿酒 I(Brewing) ｜ `EmptyCrate_Moonshine`

#### 幻菇秘串(MushroomMadness)

- 微光菇(RawShimmershroom)×0.4 营养值 + 炽光菇(RawGlowbulb)×0.4 营养值 + 蘑菇(Rawmushroom)×0.4 营养值 -> 幻菇秘串(MushroomMadness)×2 @ 电力炉灶(ElectricStove) ｜ 工作量=700 ｜ 研究:高级烹饪学 I(Food_C2)，技能:烹饪≥7 ｜ `MakeMushroomMadness`
- 微光菇(RawShimmershroom)×2 营养值 + 炽光菇(RawGlowbulb)×2 营养值 + 蘑菇(Rawmushroom)×2 营养值 -> 幻菇秘串(MushroomMadness)×8 @ 专业化烹饪炉(ElectricStove_Pro) ｜ 工作量=2000 ｜ 研究:烹饪学 IV(PackagedSurvivalMeal)，技能:烹饪≥9 ｜ `MakeMushroomMadnessBulk`

#### 炒蔬菜(FriedVegetables)

- [基础植物食材] 或 [菌类(生)]×0.45 营养值 -> 炒蔬菜(FriedVegetables)×1 @ 篝火(Campfire), 电力炉灶(ElectricStove), 燃料炉灶(FueledStove), 开放式炉灶(OpenHearth), 简易烤架(TableGrill) ｜ 工作量=500 ｜ 研究:食品工程原理 I(Food_00)，技能:烹饪≥0 ｜ `MakeVegetables`
- [基础植物食材] 或 [菌类(生)]×2.25 营养值 -> 炒蔬菜(FriedVegetables)×4 @ 专业化烹饪炉(ElectricStove_Pro) ｜ 工作量=1350 ｜ 研究:烹饪学 IV(PackagedSurvivalMeal)，技能:烹饪≥0 ｜ `MakeVegetablesBulk`

#### 俄罗斯饺子(Pelmeni)

- 驼鹿肉(Meat_Muffalo) 或 罐装精肉(MetalCannedMeat_prime)×0.7 营养值 + 面粉(Flour)×0.05 营养值 -> 俄罗斯饺子(Pelmeni)×2 @ 电力炉灶(ElectricStove) ｜ 工作量=800 ｜ 研究:烹饪学 III(Food_C1)，技能:烹饪≥8 ｜ `MakePelmeni`
- 驼鹿肉(Meat_Muffalo) 或 罐装精肉(MetalCannedMeat_prime)×3.5 营养值 + 面粉(Flour)×0.25 营养值 -> 俄罗斯饺子(Pelmeni)×8 @ 专业化烹饪炉(ElectricStove_Pro) ｜ 工作量=2200 ｜ 研究:烹饪学 IV(PackagedSurvivalMeal)，技能:烹饪≥10 ｜ `MakePelmeniBulk`

#### 干肉饼(Pemmican)

- 肉干(MealJerky)×0.45 + 果干(driedfruit)×0.15 -> 干肉饼(Pemmican)×20 @ 屠宰点(ButcherSpot), 屠宰台(TableButcher), 厨房案桌(TableKitchen) ｜ 工作量=300 ｜ 研究:食品工程原理 II(Food_0) ｜ `Make_Pemmican` ｜ PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/RecipeDefs_Meals/Recipes_Meals.xml
- 肉干(MealJerky)×2.25 + 果干(driedfruit)×0.75 -> 干肉饼(Pemmican)×80 @ 屠宰点(ButcherSpot), 屠宰台(TableButcher), 厨房案桌(TableKitchen) ｜ 工作量=1200 ｜ `Make_PemmicanBulk` ｜ PatchApplied=Mods/Core_SK_CoreModule/Patches/AutoGenPatches/RecipeDefs_Meals/Recipes_Meals.xml

#### 泡菜(PickledVeg)

- 土豆(RawPotatoes) 或 玉米(RawCorn) 或 豆荚(Rawbean) 或 西红柿(RawTomatoes) 或 胡萝卜(RawCarrots) 或 蘑菇(Rawmushroom) 或 水藻(RawAlgae)×3 营养值 + 盐(Salt)×0.02 营养值 -> 泡菜(PickledVeg)×15 @ 厨房案桌(TableKitchen) ｜ 工作量=1000 ｜ 研究:食物学 II(Food_B1) ｜ `PickleVegetable`

#### 披萨(Pizza)

- 面粉(Flour)×0.1 营养值 + 驼鹿肉(Meat_Muffalo) 或 罐装精肉(MetalCannedMeat_prime) 或 [菌类(生)]×0.6 营养值 + 奶酪(Cheese) 或 陈年奶酪(AgedCheese)×0.3 营养值 + 西红柿(RawTomatoes)×0.6 营养值 -> 披萨(Pizza)×3 @ 电烤箱(ElectricOven) ｜ 工作量=1500 ｜ 研究:烘培 III(Bakery_C1)，技能:烹饪≥15 ｜ `MakePizza`

#### 芝士焗土豆(PotatoesCheese)

- 奶酪(Cheese) 或 陈年奶酪(AgedCheese)×0.4 营养值 + 土豆(RawPotatoes)×0.4 营养值 -> 芝士焗土豆(PotatoesCheese)×3 @ 电力炉灶(ElectricStove), 燃料炉灶(FueledStove), 简易烤架(TableGrill) ｜ 工作量=450 ｜ 研究:烹饪学 I(Food_B3)，技能:烹饪≥2 ｜ `MakePotatoesCheese`
- 奶酪(Cheese) 或 陈年奶酪(AgedCheese)×2 营养值 + 土豆(RawPotatoes)×2 营养值 -> 芝士焗土豆(PotatoesCheese)×12 @ 专业化烹饪炉(ElectricStove_Pro) ｜ 工作量=1200 ｜ 研究:烹饪学 IV(PackagedSurvivalMeal)，技能:烹饪≥4 ｜ `MakePotatoesCheeseBulk`

#### 加工过的猫粮(CatFood)

- [生肉] 或 [动物产品(生)]×0.5 营养值 + [植物食材]×0.5 营养值 -> 加工过的猫粮(CatFood)×20 @ 屠宰台(TableButcher), 厨房案桌(TableKitchen) ｜ 工作量=100 ｜ 研究:畜牧业 II(Animal_A2)，技能:烹饪≥0 ｜ `CookCatFood`

#### 咖啡豆（烘焙）(RoastedCoffeeBeans)

- 咖啡豆(CoffeeBeans)×5 -> 咖啡豆（烘焙）(RoastedCoffeeBeans)×5 @ 篝火(Campfire), 电烤箱(ElectricOven), 烤箱(TableOven) ｜ 工作量=300 ｜ 研究:干肉饼(Pemmican)，技能:烹饪≥6 ｜ `RoastingCoffeeBeans`

#### 烤肉(RoastedMeat)

- [生肉]×0.4 营养值 -> 烤肉(RoastedMeat)×1 @ 篝火(Campfire), 电力炉灶(ElectricStove), 燃料炉灶(FueledStove), 开放式炉灶(OpenHearth), 简易烤架(TableGrill) ｜ 工作量=500 ｜ 研究:食品工程原理 I(Food_00)，技能:烹饪≥0 ｜ `CookRoastedMeat`
- [生肉]×2 营养值 -> 烤肉(RoastedMeat)×4 @ 专业化烹饪炉(ElectricStove_Pro) ｜ 工作量=1350 ｜ 研究:烹饪学 IV(PackagedSurvivalMeal)，技能:烹饪≥1 ｜ `CookRoastedMeatBulk`

#### 朗姆酒(Alcohol_Rum)

- 朗姆酒箱(Crate_Rum)×1 -> 朗姆酒(Alcohol_Rum)×75 @ 酿造台(Brewery), 电动酿造台(ElectricBrewery) ｜ 工作量=50 ｜ 研究:酿酒 I(Brewing) ｜ `EmptyCrate_rum`

#### 朗姆酒酿料(rummash)

- 甘蔗(Rawsugarcane)×25 + 酵母(Yeast)×10 -> 朗姆酒酿料(rummash)×10 @ 酿造台(Brewery), 电动酿造台(ElectricBrewery) ｜ 工作量=650 ｜ 研究:酿酒 II(Alcohol_B2)，技能:烹饪≥10 ｜ `Makerummash`

#### 清酒(Alcohol_Sake)

- 清酒箱(Crate_Sake)×1 -> 清酒(Alcohol_Sake)×75 @ 酿造台(Brewery), 电动酿造台(ElectricBrewery) ｜ 工作量=50 ｜ 研究:酿酒 I(Brewing) ｜ `EmptyCrate_saki`

#### Saké 清酒酿料(sakemash)

- 稻米(RawRice)×25 + 酵母(Yeast)×10 -> Saké 清酒酿料(sakemash)×10 @ 酿造台(Brewery), 电动酿造台(ElectricBrewery) ｜ 工作量=800 ｜ 研究:酿酒 II(Alcohol_B2)，技能:烹饪≥12 ｜ `Makesakemash`

#### 腌肉(SaltedMeat)

- [生肉]×25 + 盐(Salt)×5 -> 腌肉(SaltedMeat)×25 @ 屠宰点(ButcherSpot), 屠宰台(TableButcher), 厨房案桌(TableKitchen) ｜ 工作量=1000 ｜ 研究:食品工程原理 II(Food_0) ｜ `SaltMeat`

#### 炒蛋(ScrambledEggs)

- 豆腐(Tofu) 或 [未受精蛋] 或 [受精蛋]×0.5 营养值 -> 炒蛋(ScrambledEggs)×1 @ 篝火(Campfire), 电力炉灶(ElectricStove), 燃料炉灶(FueledStove), 开放式炉灶(OpenHearth), 简易烤架(TableGrill) ｜ 工作量=500 ｜ 研究:食品工程原理 I(Food_00)，技能:烹饪≥0 ｜ `CookScrambledEggs`
- 豆腐(Tofu) 或 [未受精蛋] 或 [受精蛋]×2.5 营养值 -> 炒蛋(ScrambledEggs)×4 @ 专业化烹饪炉(ElectricStove_Pro) ｜ 工作量=1350 ｜ 研究:烹饪学 IV(PackagedSurvivalMeal)，技能:烹饪≥0 ｜ `CookScrambledEggsBulk`

#### 青贮饲料(Silage)

- 小麦(Rawwheat) 或 玉米(RawCorn) 或 稻米(RawRice)×1 营养值 + 干草(Hay)×3 营养值 -> 青贮饲料(Silage)×30 @ 屠宰台(TableButcher), 厨房案桌(TableKitchen) ｜ 工作量=300 ｜ 研究:畜牧业 II(Animal_A2) ｜ `MakeSilage`

#### 豆浆(SoyMilk)

- 豆荚(Rawbean)×10 -> 豆浆(SoyMilk)×4 @ 屠宰台(TableButcher), 厨房案桌(TableKitchen) ｜ 工作量=700 ｜ 研究:干肉饼(Pemmican)，技能:烹饪≥2 ｜ `Makesoymilk`

#### 糖(Sugar)

- 甘蔗(Rawsugarcane)×5 -> 糖(Sugar)×5 @ 石磨(GrindStone), 电磨(MillElectric) ｜ 工作量=500 ｜ 研究:烘培 II(Bakery_B2)，技能:烹饪≥2 ｜ `MakeSugar`
- 蜜糖(Honey) 或 虫胶(InsectJelly)×5 -> 糖(Sugar)×5 @ 电烤箱(ElectricOven), 烤箱(TableOven) ｜ 工作量=1000 ｜ 研究:烘培 II(Bakery_B2)，技能:烹饪≥1 ｜ `MeltSugar`

#### 甜面包(SweetBun)

- 糖(Sugar)×0.4 营养值 + 面粉(Flour)×0.5 营养值 -> 甜面包(SweetBun)×5 @ 电烤箱(ElectricOven) ｜ 工作量=800 ｜ 研究:烘培 III(Bakery_C1)，技能:烹饪≥2 ｜ `MakeSweetBun`

#### 太妃糖(Taffy)

- 糖(Sugar)×0.5 营养值 -> 太妃糖(Taffy)×5 @ 糖果桌(CandyTable), 电烤箱(ElectricOven) ｜ 工作量=900 ｜ 研究:烘培 III(Bakery_C1)，技能:烹饪≥10 ｜ `MakeTaffy`

#### 龙舌兰酒(Alcohol_Tequila)

- 龙舌兰酒箱(Crate_Tequila)×1 -> 龙舌兰酒(Alcohol_Tequila)×75 @ 酿造台(Brewery), 电动酿造台(ElectricBrewery) ｜ 工作量=50 ｜ 研究:酿酒 I(Brewing) ｜ `EmptyCrate_tequila`

#### 龙舌兰酿料(tequilamash)

- 龙舌兰果(RawAgave)×35 + 酵母(Yeast)×5 -> 龙舌兰酿料(tequilamash)×10 @ 酿造台(Brewery), 电动酿造台(ElectricBrewery) ｜ 工作量=850 ｜ 研究:酿酒 II(Alcohol_B2)，技能:烹饪≥12 ｜ `Maketequilamash`

#### 豆腐(Tofu)

- 豆荚(Rawbean)×1.6 营养值 -> 豆腐(Tofu)×5 @ 厨房案桌(TableKitchen) ｜ 工作量=600 ｜ 研究:食物学 II(Food_B1)，技能:烹饪≥6 ｜ `MakeTofu`

#### 伏特加(Alcohol_Vodka)

- 伏特加箱(Crate_Vodka)×1 -> 伏特加(Alcohol_Vodka)×75 @ 酿造台(Brewery), 电动酿造台(ElectricBrewery) ｜ 工作量=50 ｜ 研究:酿酒 I(Brewing) ｜ `EmptyCrate_vodka`

#### 伏特加酿料(vodkamash)

- 土豆(RawPotatoes)×35 + 酵母(Yeast)×5 -> 伏特加酿料(vodkamash)×10 @ 酿造台(Brewery), 电动酿造台(ElectricBrewery) ｜ 工作量=600 ｜ 研究:酿酒 II(Alcohol_B2)，技能:烹饪≥5 ｜ `Makevodkamash`

#### 威士忌(Alcohol_Whisky)

- 威士忌箱(Crate_Whiskey)×1 -> 威士忌(Alcohol_Whisky)×75 @ 酿造台(Brewery), 电动酿造台(ElectricBrewery) ｜ 工作量=50 ｜ 研究:酿酒 I(Brewing) ｜ `EmptyCrate_Whiskey`

#### 葡萄酒(Alcohol_Wine)

- 葡萄酒箱(Crate_Wine)×1 -> 葡萄酒(Alcohol_Wine)×75 @ 酿造台(Brewery), 电动酿造台(ElectricBrewery) ｜ 工作量=50 ｜ 研究:酿酒 I(Brewing) ｜ `EmptyCrate_Wine`

#### 葡萄酒酿料(winemash)

- 葡萄(Rawgrape)×35 + 酵母(Yeast)×5 -> 葡萄酒酿料(winemash)×10 @ 酿造台(Brewery), 电动酿造台(ElectricBrewery) ｜ 工作量=850 ｜ 研究:酿酒 II(Alcohol_B2)，技能:烹饪≥14 ｜ `Makewinemash`

#### 酵母(Yeast)

- [菌类(生)]×5 -> 酵母(Yeast)×15 @ 酿造台(Brewery), 电动酿造台(ElectricBrewery) ｜ 工作量=800 ｜ 研究:酿酒 I(Brewing)，技能:烹饪≥6 ｜ `Makeyeast`

### 3.4 医疗（17）

#### 活性炭(ActivatedCarbon)

- 木炭(Charcoal) 或 原煤(Coal)×5 -> 活性炭(ActivatedCarbon)×9 @ 药物实验台(DrugLab), 窑炉(Kiln), 医药工作台(meditable) ｜ 工作量=780 ｜ 研究:药物 IV(DrugProduction)，技能:医药≥3, 制作≥5 ｜ `MakeActivatedCarbon`

#### 抗生素(MedicalDrink)

- 生草药(MedicineHerbal)×1 + 啤酒花(RawHops) 或 酵母(Yeast)×10 -> 抗生素(MedicalDrink)×1 @ 医药工作台(meditable) ｜ 工作量=1000 ｜ 研究:药物 V(PenoxycylineProduction)，技能:医药≥10 ｜ `BrewMedicalDrink`

#### 安全解毒剂(AntidoteSafe)

- 中性胺(Neutroamine)×1 + 毒腺(PoisonGland)×1 -> 安全解毒剂(AntidoteSafe)×1 @ 药物实验台(DrugLab), 医药工作台(meditable) ｜ 工作量=900 ｜ 研究:药物 IV(DrugProduction)，技能:医药≥7 ｜ `MakeAntidoteSafe`

#### 绷带包(Bandagekit)

- [基础织物]×10 -> 绷带包(Bandagekit)×1 @ 电动裁缝工作台(ElectricTailoringBench), 裁缝工作台(HandTailoringBench), 医药工作台(meditable) ｜ 工作量=250 ｜ 研究:医疗用品制造 II(Medicine_A2)，技能:医药≥2, 制作≥2 ｜ `MakeBandageKit`
- [基础织物]×30 -> 绷带包(Bandagekit)×3 @ 电动裁缝工作台(ElectricTailoringBench), 裁缝工作台(HandTailoringBench), 医药工作台(meditable) ｜ 工作量=750 ｜ 研究:医疗用品制造 II(Medicine_A2)，技能:医药≥2, 制作≥2 ｜ `MakeBandageKitBulk`

#### 简易药包(Herbmedicine)

- 芦荟叶(aloe)×5 + 绷带包(Bandagekit)×1 -> 简易药包(Herbmedicine)×1 @ 药物实验台(DrugLab), 原始假肢工作台(TablePrimitiveProsthetic) ｜ 工作量=500 ｜ 研究:医疗用品制造 IV(Medicine_B2)，技能:医药≥4, 制作≥2 ｜ `MakeHerbmedicine`
- 野玫瑰(WildRose)×1 + 金丝桃(Hypericum)×1 + 薄荷叶(MintLeaves) 或 芦荟叶(aloe)×1 + 绷带包(Bandagekit)×1 -> 简易药包(Herbmedicine)×1 @ 药物实验台(DrugLab), 原始假肢工作台(TablePrimitiveProsthetic) ｜ 工作量=500 ｜ 研究:医疗用品制造 IV(Medicine_B2)，技能:医药≥4, 制作≥2 ｜ `MakeHerbmedicineMix`

#### 粗制绷带(Bandagekitcrude)

- [基础织物]×7 -> 粗制绷带(Bandagekitcrude)×1 @ 手工加工台(CraftingSpot), 电动裁缝工作台(ElectricTailoringBench), 裁缝工作台(HandTailoringBench), 皮革工作台(LeatherworkerTable), 医药工作台(meditable) ｜ 工作量=150 ｜ 研究:医疗用品制造 I(Medicine_A1)，技能:医药≥1, 制作≥1 ｜ `MakeBandagekitcrude`
- [基础织物]×21 -> 粗制绷带(Bandagekitcrude)×3 @ 手工加工台(CraftingSpot), 电动裁缝工作台(ElectricTailoringBench), 裁缝工作台(HandTailoringBench), 皮革工作台(LeatherworkerTable), 医药工作台(meditable) ｜ 工作量=450 ｜ 研究:医疗用品制造 I(Medicine_A1)，技能:医药≥1, 制作≥1 ｜ `MakeBandagekitcrudeBulk`

#### 草药酊剂(HealrootTincture)

- 生草药(MedicineHerbal)×3 + 灰烬(Ash)×3 -> 草药酊剂(HealrootTincture)×1 @ 屠宰点(ButcherSpot), 屠宰台(TableButcher), 厨房案桌(TableKitchen) ｜ 工作量=400 ｜ 研究:医疗用品制造 I(Medicine_A1)，技能:医药≥3 ｜ `MakeTinctureHealroot`

#### 草药包(HerbMedicine)

- 生草药(MedicineHerbal)×8 + 灰烬(Ash)×2 -> 草药包(HerbMedicine)×1 @ 屠宰点(ButcherSpot), 屠宰台(TableButcher), 厨房案桌(TableKitchen), 医药工作台(meditable) ｜ 工作量=500 ｜ 研究:医疗用品制造 III(Medicine_B1)，技能:医药≥5 ｜ `MakeHerbMedicine`

#### 草药茶(HerbalTea)

- 野玫瑰(WildRose)×2 + 金丝桃(Hypericum)×1 + 薄荷叶(MintLeaves)×2 -> 草药茶(HerbalTea)×6 @ 酿造台(Brewery), 药物实验台(DrugLab), 电动酿造台(ElectricBrewery), 医药工作台(meditable) ｜ 工作量=780 ｜ 研究:药物 IV(DrugProduction)，技能:医药≥3, 烹饪≥5 ｜ `MakeHerbalTea`

#### 蘑菇酊剂(MushroomTincture)

- 闪帽菇(GleamcapStem)×2 -> 蘑菇酊剂(MushroomTincture)×1 @ 屠宰点(ButcherSpot), 屠宰台(TableButcher), 厨房案桌(TableKitchen) ｜ 工作量=400 ｜ 研究:医疗用品制造 I(Medicine_A1)，技能:医药≥1 ｜ `MakeTincture`
- 闪帽菇(GleamcapStem)×10 -> 蘑菇酊剂(MushroomTincture)×4 @ 屠宰台(TableButcher), 厨房案桌(TableKitchen) ｜ 工作量=800 ｜ 研究:医疗用品制造 II(Medicine_A2)，技能:医药≥6 ｜ `MakeTinctureBulk`

#### 中性胺(Neutroamine)

- 中性花瓣(NeutroPetals)×8 + 金丝桃(Hypericum)×2 -> 中性胺(Neutroamine)×1 @ 药物实验台(DrugLab) ｜ 工作量=700 ｜ 研究:佩诺西林生产(Drugs_C2)，技能:医药≥6, 制作≥3 ｜ `MakeNaturalNeutroamine`

#### 止痛药(VG_Ibuprofen)

- 中性胺(Neutroamine)×4 -> 止痛药(VG_Ibuprofen)×4 @ 药物实验台(DrugLab) ｜ 工作量=720 ｜ 研究:药物 V(PenoxycylineProduction)，技能:医药≥10 ｜ `VG_BulkIbuprofen`

#### 简易解毒剂(AntidoteSimple)

- 生草药(MedicineHerbal)×1 + 毒腺(PoisonGland)×1 -> 简易解毒剂(AntidoteSimple)×1 @ 药物实验台(DrugLab), 屠宰台(TableButcher), 厨房案桌(TableKitchen), 医药工作台(meditable) ｜ 工作量=500 ｜ 研究:医疗用品制造 II(Medicine_A2)，技能:医药≥1 ｜ `MakeAntidoteSimple`

### 3.5 其他资源（1）

#### 绿色营养物质(soylentgreen)

- [类人尸体]×1 -> 绿色营养物质(soylentgreen)×45 @ 营养物质机(soylenttable) ｜ 工作量=1400 ｜ 研究:营养膏技术(NutrientPaste)，技能:制作≥12 ｜ `Make_SoylentGreen`

### 3.6 外部/原版（17）

#### 12号子弹（鹿弹）(Ammo_12Gauge_Buck)

- 未指定过滤器的材料 -> 12号子弹（鹿弹）(Ammo_12Gauge_Buck)×12号子弹（鹿弹）(Ammo_12Gauge_Buck)=40 ｜ 工作量=workAmount=2500 ｜ 技能:制作≥3 ｜ `MakeAmmo_12Gauge_Buck` ｜ PatchApplied=Mods/Core_SK/Patches/Mods_CombatExtended/Ammo/Shotgun/12Gauge.xml

#### 30x29mm 榴弹 (高爆双效弹)(Ammo_30x29mmGrenade_HEDP)

- [金属锭类]×40 + 火药(Powder) 或 含胺乳化燃料(FSX)×8 + [化学品]×5 + 普通零部件(ComponentIndustrial)×4 -> 30x29mm 榴弹 (高爆双效弹)(Ammo_30x29mmGrenade_HEDP)×20 ｜ 研究:枪械 II(Range_C2) ｜ `MakeAmmo_30x29mmGrenade_HEDP` ｜ PatchApplied=Mods/Core_SK/Patches/Mods_CombatExtended/Ammo/Grenade/30x29mmGrenade.xml

#### 30x29mm 榴弹 (烟雾弹)(Ammo_30x29mmGrenade_Smoke)

- [金属锭类]×42 + 塑料(Plastic)×15 + 普通零部件(ComponentIndustrial)×6 -> 30x29mm 榴弹 (烟雾弹)(Ammo_30x29mmGrenade_Smoke)×30x29mm 榴弹 (烟雾弹)(Ammo_30x29mmGrenade_Smoke)=20 ｜ 研究:枪械 II(Range_C2)，技能:Crafting≥Crafting=12 ｜ `MakeAmmo_30x29mmGrenade_Smoke` ｜ PatchApplied=Mods/Core_SK/Patches/Mods_CombatExtended/Ammo/Grenade/30x29mmGrenade.xml

#### 66mm 热能弹 (燃烧弹)(Ammo_66mmThermalBolt_Incendiary)

- [金属锭类]×40 + 火药(Powder) 或 含胺乳化燃料(FSX)×10 + 凝固汽油(Napalm)×15 -> 66mm 热能弹 (燃烧弹)(Ammo_66mmThermalBolt_Incendiary)×20 ｜ `MakeAmmo_66mmThermalBolt_Incendiary` ｜ PatchApplied=Mods/Core_SK/Patches/Mods_CombatExtended/Ammo/Advanced/66mmThermalBolt.xml

#### 人类肉(Meat_Human)

- [天然身体部位]×1 -> 人类肉(Meat_Human)×16 @ 屠宰台(TableButcher), 厨房案桌(TableKitchen) ｜ 工作量=250 ｜ 研究:食品工程原理 II(Food_0) ｜ `MinceLimbs`
- [天然身体部位]×1 -> 人类肉(Meat_Human)×8 @ 屠宰台(TableButcher), 厨房案桌(TableKitchen) ｜ 工作量=150 ｜ 研究:食品工程原理 II(Food_0) ｜ `MinceOrgans`

#### Meat_ThingDefFishSduiggles(Meat_ThingDefFishSduiggles)

- [鱼]×2 营养值 -> Meat_ThingDefFishSduiggles(Meat_ThingDefFishSduiggles)×30 @ 屠宰点(ButcherSpot), 屠宰台(TableButcher), 厨房案桌(TableKitchen) ｜ 工作量=400 ｜ 研究:渔猎 I(Fish_A1)，技能:烹饪≥2 ｜ `ProcessSmallFish`

#### 标枪(Pila)

- [木本植物]×10 + [金属锭类]×5 -> 标枪(Pila)×5 @ 手工加工台(CraftingSpot), 电力锻造台(ElectricSmithy), 锻造台(FueledSmithy) ｜ 工作量=2400 ｜ 技能:制作≥3 ｜ `MakeAmmo_Pilum`

#### 81mm 迫击炮弹 (死灵尘)(Shell_Deadlife)

- [轻合金锭类] 或 [超轻合金锭类]×15 + [重金属锭类]×5 + 火药(Powder) 或 含胺乳化燃料(FSX)×6 + 生物铁合金(BioferriteAlloy)×25 -> 81mm 迫击炮弹 (死灵尘)(Shell_Deadlife)×81mm 迫击炮弹 (死灵尘)(Shell_Deadlife)=2 ｜ 工作量=1000 ｜ 研究:枪械 II(Range_C2), DeadlifeDust ｜ `MakeShell_Deadlife` ｜ PatchApplied=Mods/Core_SK/Patches/Mods_CombatExtended/Ammo/Shell/81mmMortar.xml; PatchApplied=Mods/Core_SK_DLCModule/Anomaly/Patches/CombatExtended/Ammo/SK_AnomalyAmmoPatch.xml

#### 81mm 迫击炮弹（高爆弹）(Shell_HighExplosive)

- [轻合金锭类] 或 [超轻合金锭类]×15 + [重金属锭类]×5 + 火药(Powder) 或 含胺乳化燃料(FSX)×6 + 普通零部件(ComponentIndustrial)×2 -> 81mm 迫击炮弹（高爆弹）(Shell_HighExplosive)×81mm 迫击炮弹（高爆弹）(Shell_HighExplosive)=1 ｜ 工作量=1000 ｜ `MakeShell_HighExplosive` ｜ PatchApplied=Mods/Core_SK/Patches/Mods_CombatExtended/Ammo/Shell/81mmMortar.xml

#### 81mm 迫击炮弹 (空爆弹)(Shell_HighExplosive_HFuzed)

- [轻合金锭类] 或 [超轻合金锭类]×15 + [重金属锭类]×5 + 火药(Powder) 或 含胺乳化燃料(FSX)×6 + 普通零部件(ComponentIndustrial)×2 + 电子元件(ElectronicComponents)×2 -> 81mm 迫击炮弹 (空爆弹)(Shell_HighExplosive_HFuzed)×81mm 迫击炮弹 (空爆弹)(Shell_HighExplosive_HFuzed)=1 ｜ 工作量=1000 ｜ `MakeShell_HighExplosive_HFuzed` ｜ PatchApplied=Mods/Core_SK/Patches/Mods_CombatExtended/Ammo/Shell/81mmMortar.xml

#### 81mm 迫击炮弹（燃烧弹）(Shell_Incendiary)

- [轻合金锭类] 或 [超轻合金锭类]×15 + [重金属锭类]×5 + 火药(Powder) 或 含胺乳化燃料(FSX)×6 + 钷素燃料(Prometheum)×2 -> 81mm 迫击炮弹（燃烧弹）(Shell_Incendiary)×81mm 迫击炮弹（燃烧弹）(Shell_Incendiary)=1 ｜ 工作量=1000 ｜ `MakeShell_Incendiary` ｜ PatchApplied=Mods/Core_SK/Patches/Mods_CombatExtended/Ammo/Shell/81mmMortar.xml

#### 81mm 迫击炮弹 (毒气弹)(Shell_Toxic)

- [轻合金锭类] 或 [超轻合金锭类]×12 + [重金属锭类]×4 + [化学品]×15 + 火药(Powder) 或 含胺乳化燃料(FSX)×3 + 普通零部件(ComponentIndustrial)×2 -> 81mm 迫击炮弹 (毒气弹)(Shell_Toxic)×81mm 迫击炮弹 (毒气弹)(Shell_Toxic)=1 ｜ 工作量=1000 ｜ 研究:枪械 II(Range_C2), ToxGas ｜ `MakeShell_Toxic` ｜ PatchApplied=Mods/Core_SK/Patches/Mods_CombatExtended/Ammo/Shell/81mmMortar.xml

#### Techprint_Blaster_weapons_Archotech(Techprint_Blaster_weapons_Archotech)

- Archotech Replica А-2 «Blaster»(Gun_ArchotechBlaster_Prototype)×1 -> Techprint_Blaster_weapons_Archotech(Techprint_Blaster_weapons_Archotech)×1 @ 未来武器工作台(MechWeaponCraftingWorkTable) ｜ 工作量=50000 ｜ 技能:制作≥15, 智识≥15, 艺术≥5 ｜ `ReverseEngineer_ArchotechBlaster`

#### Techprint_Bow_Archotech(Techprint_Bow_Archotech)

- Archotech Replica А-1 «Bow»(Bow_Archotech_Prototype)×1 -> Techprint_Bow_Archotech(Techprint_Bow_Archotech)×1 @ 未来武器工作台(MechWeaponCraftingWorkTable) ｜ 工作量=50000 ｜ 技能:制作≥15, 智识≥15, 艺术≥5 ｜ `ReverseEngineer_ArchotechBow`

#### Techprint_Melee_Archotech(Techprint_Melee_Archotech)

- Archotech Replica B-2 «Shortsword»(MeleeWeapon_Archotech1H_Prototype) 或 Archotech Replica B-1 «Broadsword»(MeleeWeapon_Archotech2H_Prototype)×1 -> Techprint_Melee_Archotech(Techprint_Melee_Archotech)×1 @ 未来武器工作台(MechWeaponCraftingWorkTable) ｜ 工作量=50000 ｜ 技能:制作≥15, 智识≥15, 艺术≥5 ｜ `ReverseEngineer_ArchotechSword`

#### Techprint_SMG_Archotech(Techprint_SMG_Archotech)

- Archotech Replica А-3 «SMG»(Gun_ArchotechSMG_Prototype)×1 -> Techprint_SMG_Archotech(Techprint_SMG_Archotech)×1 @ 未来武器工作台(MechWeaponCraftingWorkTable) ｜ 工作量=50000 ｜ 技能:制作≥15, 智识≥15, 艺术≥5 ｜ `ReverseEngineer_ArchotechSMG`

## 4. 发酵/缓处理配方（按产出资源分组）

### 4.1 原材料（37）

#### 灰烬(Ash)

- [木质]×100 + 土堆(Dirt)×10 -> 灰烬(Ash)×30 + 土堆(Dirt)×3 @ 木炭窑(CharcoalPit) ｜ 处理天数=1 ｜ `MakeAsh_Pit_Full`
- [木质]×50 + 土堆(Dirt)×5 -> 灰烬(Ash)×15 + 土堆(Dirt)×3 @ 木炭窑(CharcoalPit) ｜ 处理天数=1 ｜ `MakeAsh_Pit_Half`

#### 木炭(Charcoal)

- [木质]×100 + 土堆(Dirt)×10 -> 木炭(Charcoal)×80 + 土堆(Dirt)×6 @ 木炭窑(CharcoalPit) ｜ 处理天数=1 ｜ `MakeCharcoal_Pit_Full`
- [木质]×50 + 土堆(Dirt)×5 -> 木炭(Charcoal)×40 + 土堆(Dirt)×3 @ 木炭窑(CharcoalPit) ｜ 处理天数=1 ｜ `MakeCharcoal_Pit_Half`

#### 咖啡豆(CoffeeBeans)

- 咖啡果(RawCoffee)×5 -> 咖啡豆(CoffeeBeans)×5 @ 干燥架(DryingRack) ｜ 处理天数=5 ｜ `Make_CoffeeBeans`
- 咖啡果(RawCoffee)×5 -> 咖啡豆(CoffeeBeans)×5 @ 电动干燥架(ElectricDryingRack) ｜ 处理天数=3 ｜ `Make_CoffeeBeansIndustrial_x1`
- 咖啡果(RawCoffee)×50 -> 咖啡豆(CoffeeBeans)×50 @ 电动干燥架(ElectricDryingRack) ｜ 处理天数=3 ｜ `Make_CoffeeBeansIndustrial_x10`
- 咖啡果(RawCoffee)×25 -> 咖啡豆(CoffeeBeans)×25 @ 电动干燥架(ElectricDryingRack) ｜ 处理天数=3 ｜ `Make_CoffeeBeansIndustrial_x5`
- 咖啡果(RawCoffee)×25 -> 咖啡豆(CoffeeBeans)×25 @ 干燥架(DryingRack) ｜ 处理天数=5 ｜ `Make_CoffeeBeans_x5`

#### 干燥大麻叶(DriedLeavesSmokeleaf)

- 大麻叶(SmokeleafLeaves)×5 -> 干燥大麻叶(DriedLeavesSmokeleaf)×5 @ 干燥架(DryingRack) ｜ 处理天数=4 ｜ `Make_Smokeleaf`
- 大麻叶(SmokeleafLeaves)×5 -> 干燥大麻叶(DriedLeavesSmokeleaf)×5 @ 电动干燥架(ElectricDryingRack) ｜ 处理天数=2 ｜ `Make_SmokeleafIndustrial_x1`
- 大麻叶(SmokeleafLeaves)×50 -> 干燥大麻叶(DriedLeavesSmokeleaf)×50 @ 电动干燥架(ElectricDryingRack) ｜ 处理天数=2 ｜ `Make_SmokeleafIndustrial_x10`
- 大麻叶(SmokeleafLeaves)×25 -> 干燥大麻叶(DriedLeavesSmokeleaf)×25 @ 电动干燥架(ElectricDryingRack) ｜ 处理天数=2 ｜ `Make_SmokeleafIndustrial_x5`
- 大麻叶(SmokeleafLeaves)×25 -> 干燥大麻叶(DriedLeavesSmokeleaf)×25 @ 干燥架(DryingRack) ｜ 处理天数=4 ｜ `Make_Smokeleaf_x5`

#### 干燥烟叶(DriedLeavesTobacco)

- 烟叶(RawTobacco)×5 -> 干燥烟叶(DriedLeavesTobacco)×5 @ 干燥架(DryingRack) ｜ 处理天数=3 ｜ `Make_Tobacco`
- 烟叶(RawTobacco)×5 -> 干燥烟叶(DriedLeavesTobacco)×5 @ 电动干燥架(ElectricDryingRack) ｜ 处理天数=1 ｜ `Make_TobaccoIndustrial_x1`
- 烟叶(RawTobacco)×50 -> 干燥烟叶(DriedLeavesTobacco)×50 @ 电动干燥架(ElectricDryingRack) ｜ 处理天数=1 ｜ `Make_TobaccoIndustrial_x10`
- 烟叶(RawTobacco)×25 -> 干燥烟叶(DriedLeavesTobacco)×25 @ 电动干燥架(ElectricDryingRack) ｜ 处理天数=1 ｜ `Make_TobaccoIndustrial_x5`
- 烟叶(RawTobacco)×25 -> 干燥烟叶(DriedLeavesTobacco)×25 @ 干燥架(DryingRack) ｜ 处理天数=3 ｜ `Make_Tobacco_x5`

#### 肥料(Fertilizer)

- 堆肥料(Compost)×1 -> 肥料(Fertilizer)×1 @ 堆肥桶(CompostBarrel) ｜ 处理天数=2.8 ｜ `Make_Fertilizer`
- 堆肥料(Compost)×5 -> 肥料(Fertilizer)×5 @ 堆肥桶(CompostBarrel) ｜ 处理天数=2.8 ｜ `Make_Fertilizer_x5`

#### 硝石(Nitre)

- 化合燃料(Chemfuel)×10 -> 硝石(Nitre)×20 + 硫磺(Sulfur)×10 @ 硫磺提取器(SulfurExtractor) ｜ 处理天数=5 ｜ `CollectNitreGeyser`
- [尸体]×1 + 灰烬(Ash)×1 -> 硝石(Nitre)×30 @ 硝石转化坑(Saltpeter_pit) ｜ 处理天数=10 ｜ `Make_Nitre_Body`
- 粪便污泥(FecalSludge)×350 + 灰烬(Ash)×1 -> 硝石(Nitre)×15 @ 硝石转化坑(Saltpeter_pit) ｜ 处理天数=10 ｜ `Make_Nitre_Fecal`
- 腐烂物(RottedMush)×70 + 灰烬(Ash)×1 -> 硝石(Nitre)×7 @ 硝石转化坑(Saltpeter_pit) ｜ 处理天数=10 ｜ `Make_Nitre_Mush`

#### 土堆(Dirt)

- [木质]×100 + 土堆(Dirt)×10 -> 灰烬(Ash)×30 + 土堆(Dirt)×3 @ 木炭窑(CharcoalPit) ｜ 处理天数=1 ｜ `MakeAsh_Pit_Full`
- [木质]×50 + 土堆(Dirt)×5 -> 灰烬(Ash)×15 + 土堆(Dirt)×3 @ 木炭窑(CharcoalPit) ｜ 处理天数=1 ｜ `MakeAsh_Pit_Half`
- [木质]×100 + 土堆(Dirt)×10 -> 木炭(Charcoal)×80 + 土堆(Dirt)×6 @ 木炭窑(CharcoalPit) ｜ 处理天数=1 ｜ `MakeCharcoal_Pit_Full`
- [木质]×50 + 土堆(Dirt)×5 -> 木炭(Charcoal)×40 + 土堆(Dirt)×3 @ 木炭窑(CharcoalPit) ｜ 处理天数=1 ｜ `MakeCharcoal_Pit_Half`
- 软粘土(SoftClay)×100 + 土堆(Dirt)×5 -> 粘土砖(ClayBrick)×100 + 土堆(Dirt)×3 @ 木炭窑(CharcoalPit) ｜ 处理天数=1 ｜ `MakeClayBricks_Pit_Full`
- 软粘土(SoftClay)×50 + 土堆(Dirt)×5 -> 粘土砖(ClayBrick)×50 + 土堆(Dirt)×3 @ 木炭窑(CharcoalPit) ｜ 处理天数=1 ｜ `MakeClayBricks_Pit_Half`

#### 碎石(CrushedStone)

- [石砖]×20 -> 碎石(CrushedStone)×20 @ 工业碎石机(RockCrusher) ｜ 处理天数=0.3 ｜ `Crush_Blocks_Rubble`
- [石块]×1 -> 碎石(CrushedStone)×35 @ 工业碎石机(RockCrusher) ｜ 处理天数=0.6 ｜ `Crush_Chunk_Rubble`

#### 沙(SandResource)

- [石块]×1 -> 沙(SandResource)×80 @ 工业碎石机(RockCrusher) ｜ 处理天数=0.8 ｜ `CrushChunkSand`
- 碎石(CrushedStone)×10 -> 沙(SandResource)×25 @ 工业碎石机(RockCrusher) ｜ 处理天数=0.5 ｜ `CrushRubbleSand`

#### 硫磺(Sulfur)

- 化合燃料(Chemfuel)×10 -> 硝石(Nitre)×20 + 硫磺(Sulfur)×10 @ 硫磺提取器(SulfurExtractor) ｜ 处理天数=5 ｜ `CollectNitreGeyser`
- 化合燃料(Chemfuel)×10 -> 硫磺(Sulfur)×50 @ 硫磺提取器(SulfurExtractor) ｜ 处理天数=3 ｜ `CollectSulfurGeyser`

### 4.2 加工品（3）

#### 生物质(Biomatter)

- [尸体]×1 -> 生物质(Biomatter)×20 @ 器官实验室(TableOrganvat) ｜ 处理天数=1.5 ｜ `ExtractBiomatterCorpse`

#### 粘土砖(ClayBrick)

- 软粘土(SoftClay)×100 + 土堆(Dirt)×5 -> 粘土砖(ClayBrick)×100 + 土堆(Dirt)×3 @ 木炭窑(CharcoalPit) ｜ 处理天数=1 ｜ `MakeClayBricks_Pit_Full`
- 软粘土(SoftClay)×50 + 土堆(Dirt)×5 -> 粘土砖(ClayBrick)×50 + 土堆(Dirt)×3 @ 木炭窑(CharcoalPit) ｜ 处理天数=1 ｜ `MakeClayBricks_Pit_Half`

### 4.3 食物（28）

#### 陈年奶酪(AgedCheese)

- 奶(Milk)×10 -> 陈年奶酪(AgedCheese)×3 @ 干酪槽(CheeseBasin) ｜ 处理天数=8 ｜ `Make_AgedCheese`
- 奶(Milk)×100 -> 陈年奶酪(AgedCheese)×30 @ 乳酪桶(CheeseVat) ｜ 处理天数=8 ｜ `Make_AgedCheese_Vat_x10`
- 奶(Milk)×50 -> 陈年奶酪(AgedCheese)×15 @ 干酪槽(CheeseBasin) ｜ 处理天数=8 ｜ `Make_AgedCheese_x5`

#### 苹果酒(Alcohol_Cider)

- 苹果酒酿料(cidermash)×40 -> 苹果酒(Alcohol_Cider)×20 @ 酒类发酵桶(UniversalFermenter) ｜ 处理天数=4.8 ｜ `Make_Cider`

#### 啤酒(Beer)

- 麦芽汁(Wort)×20 -> 啤酒(Beer)×20 @ 酒类发酵桶(UniversalFermenter) ｜ 处理天数=6 ｜ `Make_Beer`

#### 浆果酒(Alcohol_BerryWine)

- 浆果酒酿料(berrymash)×25 -> 浆果酒(Alcohol_BerryWine)×20 @ 酒类发酵桶(UniversalFermenter) ｜ 处理天数=5.8 ｜ `Make_Berrywine`

#### 奶酪(Cheese)

- 奶(Milk) 或 豆浆(SoyMilk)×10 -> 奶酪(Cheese)×5 @ 干酪槽(CheeseBasin) ｜ 处理天数=4 ｜ `Make_Cheese`
- 奶(Milk) 或 豆浆(SoyMilk)×100 -> 奶酪(Cheese)×50 @ 乳酪桶(CheeseVat) ｜ 处理天数=4 ｜ `Make_Cheese_Vat_x10`
- 奶(Milk) 或 豆浆(SoyMilk)×50 -> 奶酪(Cheese)×25 @ 干酪槽(CheeseBasin) ｜ 处理天数=4 ｜ `Make_Cheese_x5`

#### 果干(driedfruit)

- [水果(生)]×0.75 营养值 -> 果干(driedfruit)×5 @ 干燥架(DryingRack) ｜ 处理天数=2 ｜ `Make_DriedFruit`
- [水果(生)]×0.75 营养值 -> 果干(driedfruit)×5 @ 电动干燥架(ElectricDryingRack) ｜ 处理天数=1 ｜ `Make_DriedFruitIndustrial_x1`
- [水果(生)]×7.5 营养值 -> 果干(driedfruit)×50 @ 电动干燥架(ElectricDryingRack) ｜ 处理天数=1 ｜ `Make_DriedFruitIndustrial_x10`
- [水果(生)]×3.75 营养值 -> 果干(driedfruit)×25 @ 电动干燥架(ElectricDryingRack) ｜ 处理天数=1 ｜ `Make_DriedFruitIndustrial_x5`
- [水果(生)]×3.75 营养值 -> 果干(driedfruit)×25 @ 干燥架(DryingRack) ｜ 处理天数=2 ｜ `Make_DriedFruit_x5`

#### 面粉(Flour)

- 小麦(Rawwheat) 或 玉米(RawCorn) 或 稻米(RawRice)×100 -> 面粉(Flour)×50 @ 风车磨坊(SK_Windmill) ｜ 处理天数=0.3 ｜ `Grind_Flour`

#### 肉干(MealJerky)

- [生肉]×10 -> 肉干(MealJerky)×10 @ 干肉架(JerkyRack) ｜ 处理天数=2 ｜ `Make_Jerky`
- [生肉]×10 -> 肉干(MealJerky)×10 @ 电动干燥架(ElectricDryingRack) ｜ 处理天数=0.5 ｜ `Make_JerkyIndustrial_x1`
- [生肉]×100 -> 肉干(MealJerky)×100 @ 电动干燥架(ElectricDryingRack) ｜ 处理天数=0.5 ｜ `Make_JerkyIndustrial_x10`
- [生肉]×50 -> 肉干(MealJerky)×50 @ 电动干燥架(ElectricDryingRack) ｜ 处理天数=0.5 ｜ `Make_JerkyIndustrial_x5`
- [生肉]×50 -> 肉干(MealJerky)×50 @ 干肉架(JerkyRack) ｜ 处理天数=2 ｜ `Make_Jerky_x5`

#### 朗姆酒(Alcohol_Rum)

- 朗姆酒酿料(rummash)×35 -> 朗姆酒(Alcohol_Rum)×20 @ 酒类发酵桶(UniversalFermenter) ｜ 处理天数=4.5 ｜ `Make_Rum`

#### 清酒(Alcohol_Sake)

- Saké 清酒酿料(sakemash)×30 -> 清酒(Alcohol_Sake)×20 @ 酒类发酵桶(UniversalFermenter) ｜ 处理天数=5.25 ｜ `Make_Sake`

#### 青贮饲料(Silage)

- 干草(Hay) 或 玉米(RawCorn) 或 稻米(RawRice) 或 小麦(Rawwheat)×150 -> 青贮饲料(Silage)×50 @ 青贮料堆(SilagePile) ｜ 处理天数=20 ｜ `Make_Silage`

#### 糖(Sugar)

- 甘蔗(Rawsugarcane)×20 -> 糖(Sugar)×20 @ 风车磨坊(SK_Windmill) ｜ 处理天数=0.3 ｜ `Grind_Sugar`

#### 龙舌兰酒(Alcohol_Tequila)

- 龙舌兰酿料(tequilamash)×40 -> 龙舌兰酒(Alcohol_Tequila)×20 @ 酒类发酵桶(UniversalFermenter) ｜ 处理天数=5.6 ｜ `Make_Tequila`

#### 伏特加(Alcohol_Vodka)

- 伏特加酿料(vodkamash)×35 -> 伏特加(Alcohol_Vodka)×20 @ 酒类发酵桶(UniversalFermenter) ｜ 处理天数=4.8 ｜ `Make_Vodka`

#### 威士忌(Alcohol_Whisky)

- 威士忌酿料(grainmash)×50 -> 威士忌(Alcohol_Whisky)×20 @ 酒类发酵桶(UniversalFermenter) ｜ 处理天数=7.25 ｜ `Make_Whisky`

#### 葡萄酒(Alcohol_Wine)

- 葡萄酒酿料(winemash)×35 -> 葡萄酒(Alcohol_Wine)×20 @ 酒类发酵桶(UniversalFermenter) ｜ 处理天数=5.8 ｜ `Make_Wine`

