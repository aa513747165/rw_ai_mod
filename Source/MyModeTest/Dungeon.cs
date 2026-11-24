using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace MyModeTest
{
    /// <summary>
    /// 副本分档奖励条目（mmt-dungeon-design.md §3.1）：XML 显式字段 + minTier/maxTier 档位门控。
    /// defName 走字符串惰性解析（GetNamedSilentFail），缺失静默跳过——其它 mod 可 XPath 追加自己的条目。
    /// </summary>
    public class DungeonRewardEntry
    {
        /// <summary>奖励物品 defName（惰性解析，缺失静默跳过）。</summary>
        public string thingDefName;

        /// <summary>数量区间。</summary>
        public IntRange countRange = new IntRange(1, 1);

        /// <summary>出现的最低档位（1-3）。</summary>
        public int minTier = 1;

        /// <summary>出现的最高档位（默认 3）。</summary>
        public int maxTier = 3;

        private ThingDef resolved;
        private bool searched;

        /// <summary>惰性解析 ThingDef（缺失返回 null，不报错）。</summary>
        public ThingDef ThingDef
        {
            get
            {
                if (!searched)
                {
                    searched = true;
                    if (!thingDefName.NullOrEmpty())
                        resolved = DefDatabase<ThingDef>.GetNamedSilentFail(thingDefName);
                }
                return resolved;
            }
        }

        /// <summary>该条目是否在指定档位出现。</summary>
        public bool AvailableAt(int tier)
        {
            return tier >= minTier && tier <= maxTier && ThingDef != null;
        }
    }

    /// <summary>装备箱类别。</summary>
    public enum DungeonGearKind
    {
        /// <summary>武器/防具各半随机。</summary>
        Any,
        Weapon,
        Apparel,
    }

    /// <summary>副本装备箱条目：按档位掷随机强化/附魔装备（LootRoll），品质地板随档位（§3.2）。</summary>
    public class DungeonGearEntry
    {
        public IntRange count = new IntRange(1, 1);
        public int minTier = 1;
        public int maxTier = 3;
        public DungeonGearKind kind = DungeonGearKind.Any;

        public bool AvailableAt(int tier)
        {
            return tier >= minTier && tier <= maxTier;
        }
    }

    /// <summary>布局建筑加权条目（defName 字符串 + 权重；惰性解析静默回退）。</summary>
    public class DungeonWeightedThing
    {
        public string defName;
        public float weight = 1f;
    }

    /// <summary>
    /// 副本布局参数（SitePartDef 的 DefModExtension，mmt-dungeon-design.md §4.2）：
    /// 全部 def 引用为字符串惰性解析、缺失静默跳过；默认清单只用原版 def，其它 mod 可 XPath 追加。
    /// 宿主为 mod 派系时墙体材质与炮塔由派系自适应机制优先推导，本表仅兜底。
    /// </summary>
    public class DungeonLayoutExtension : DefModExtension
    {
        /// <summary>外墙（默认原版 Wall）。</summary>
        public string wallDefName = "Wall";

        /// <summary>外墙/掩体材质（默认空 → 守卫装备采样 → GenStuff 默认）。</summary>
        public string wallStuffDefName;

        /// <summary>库房门（默认原版 Door）。</summary>
        public string doorDefName = "Door";

        /// <summary>掩体列表（默认 Sandbags，可多项加权）。</summary>
        public List<DungeonWeightedThing> coverDefs;

        /// <summary>炮塔白名单（兜底/人工追加用，默认空；优先级低于派系自适应扫描）。</summary>
        public List<string> turretDefs;

        /// <summary>照明列表（默认 TorchLamp）。</summary>
        public List<DungeonWeightedThing> lightDefs;

        /// <summary>外墙半径（中心到墙的格数）区间。</summary>
        public IntRange wallRadiusRange = new IntRange(13, 17);

        /// <summary>奖励库房内半径区间（1 = 3×3 内部）。</summary>
        public IntRange vaultRadiusRange = new IntRange(1, 2);

        /// <summary>守卫哨位数量区间。</summary>
        public IntRange postCountRange = new IntRange(2, 4);

        /// <summary>外墙入口数量区间。</summary>
        public IntRange entranceCountRange = new IntRange(1, 2);

        /// <summary>Boss PawnKindDef defName（可空 = 无 Boss；Boss 本体为 MMT 独占单位，后续自建后填入即生效）。</summary>
        public string bossPawnKindDefName;

        /// <summary>是否有 Boss 巢位（有则库房偏移、中央留开阔巢区）。</summary>
        public bool HasBossNest
        {
            get { return !bossPawnKindDefName.NullOrEmpty(); }
        }

        public ThingDef ResolveWallDef()
        {
            return DefDatabase<ThingDef>.GetNamedSilentFail(wallDefName ?? "Wall") ?? ThingDefOf.Wall;
        }

        public ThingDef ResolveDoorDef()
        {
            return DefDatabase<ThingDef>.GetNamedSilentFail(doorDefName ?? "Door");
        }

        public ThingDef ResolveWallStuff(ThingDef wallDef)
        {
            ThingDef stuff = wallStuffDefName.NullOrEmpty() ? null : DefDatabase<ThingDef>.GetNamedSilentFail(wallStuffDefName);
            if (stuff == null && wallDef != null && wallDef.MadeFromStuff)
                stuff = GenStuff.DefaultStuffFor(wallDef);
            return stuff;
        }

        /// <summary>从加权列表随机抽一个可解析的 ThingDef（全部缺失返回 null）。</summary>
        public ThingDef ResolveWeighted(List<DungeonWeightedThing> list)
        {
            if (list.NullOrEmpty())
                return null;
            List<Pair<ThingDef, float>> valid = new List<Pair<ThingDef, float>>();
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] == null || list[i].defName.NullOrEmpty() || list[i].weight <= 0f)
                    continue;
                ThingDef d = DefDatabase<ThingDef>.GetNamedSilentFail(list[i].defName);
                if (d != null)
                    valid.Add(new Pair<ThingDef, float>(d, list[i].weight));
            }
            if (valid.Count == 0)
                return null;
            return valid.RandomElementByWeight(p => p.Second).First;
        }
    }

    /// <summary>
    /// 副本据点生成（mmt-dungeon-design.md §1-§4）：派系选取（跨 Mod 敌对派系自动兼容）+
    /// 威胁点公式 + 世界据点创建。扫描仪与未来任务链共用入口；全部回退静默。
    /// </summary>
    public static class DungeonSiteGen
    {
        /// <summary>守卫小队预算占比（§2.2，待定项：实测后可调）。</summary>
        public const float GuardBudgetFraction = 0.7f;

        /// <summary>防御工事预算占比（§2.2）。</summary>
        public const float FortBudgetFraction = 0.3f;

        /// <summary>守卫威胁点上限（超出后多数派系 PawnGroupMaker 无法抽组，静默封顶）。</summary>
        public const float GuardPointsCap = 10000f;

        /// <summary>据点过期时间（30 天，沿用探索据点规则）。</summary>
        public const int SiteTimeoutTicks = 30 * 60000;

        /// <summary>原版机械集群 SitePart（Royalty 内容，惰性解析；未装 Royalty 时为 null，据点退化为无守卫）。</summary>
        private static SitePartDef mechClusterPart;
        private static bool mechClusterSearched;

        public static SitePartDef MechClusterPart
        {
            get
            {
                if (!mechClusterSearched)
                {
                    mechClusterSearched = true;
                    mechClusterPart = DefDatabase<SitePartDef>.GetNamedSilentFail("MechCluster");
                }
                return mechClusterPart;
            }
        }

        /// <summary>财富 → 基础威胁点（clamp(基地财富/100, 800, 5000)，走原版 wealthWatcher，§1.2）。</summary>
        public static float BaseThreatPoints(Map homeMap)
        {
            return Mathf.Clamp(homeMap.wealthWatcher.WealthTotal / 100f, 800f, 5000f);
        }

        /// <summary>
        /// 选取据点宿主派系（§2.1）：原版 TryGetRandomFactionForCombatPawnGroup 已满足
        /// "敌对 + 拥有可用战斗 PawnGroupMaker + 按派系出现权重随机"，mod 派系天然在列；
        /// 仅叠加 dungeonFactionBlacklist 过滤。枚举为空返回 null（调用方走机械集群回退）。
        /// </summary>
        public static Faction PickHostFaction(float guardPoints)
        {
            Faction faction;
            if (PawnGroupMakerUtility.TryGetRandomFactionForCombatPawnGroup(guardPoints, out faction,
                f => f != null && f.def != null && !MMTSettings.DungeonFactionBlacklisted(f.def.defName),
                false, false, false, true))
                return faction;
            return null;
        }

        /// <summary>找非水非占领的据点世界格（距家 6-30 格）。</summary>
        private static bool TryFindTile(Map homeMap, out PlanetTile tile)
        {
            tile = PlanetTile.Invalid;
            PlanetTile homeTile = homeMap.Tile;
            if (!homeTile.Valid)
                return false;
            return TileFinder.TryFindNewSiteTile(out tile, homeTile, 6, 30, true,
                null, 0f, false, TileFinderMode.Random, false, false, null, null);
        }

        /// <summary>MakeSite + 命名 + 入世界 + 30 天过期。</summary>
        private static Site FinalizeSite(List<SitePartDefWithParams> parts, PlanetTile tile, Faction faction, string label)
        {
            Site site = SiteMaker.MakeSite(parts, tile, faction, true, null);
            if (site == null)
                return null;
            site.customLabel = label;
            Find.WorldObjects.Add(site);
            site.GetComponent<TimeoutComp>()?.StartTimeout(SiteTimeoutTicks);
            return site;
        }

        /// <summary>
        /// 生成副本据点（扫描仪/任务链共用）：威胁点 = 基础 × 据点类型系数 × 档位系数（§1.2）。
        /// 档位经 SitePartParams.randomValue 带给 worker（vanilla 未使用该字段，存档安全）。
        /// 派系为空时回退机械集群守卫（§2.3）；仍为空则纯奖励点。
        /// </summary>
        public static Site GenerateDungeonSite(Map homeMap, SitePartDef partDef, int tier,
            float siteThreatFactor, float tierThreatFactor, string label)
        {
            if (homeMap == null || partDef == null)
                return null;
            if (!TryFindTile(homeMap, out PlanetTile tile))
                return null;
            float threatPoints = BaseThreatPoints(homeMap) * siteThreatFactor * tierThreatFactor;
            Faction faction = PickHostFaction(Mathf.Min(threatPoints * GuardBudgetFraction, GuardPointsCap));
            List<SitePartDefWithParams> parts = new List<SitePartDefWithParams>
            {
                new SitePartDefWithParams(partDef, new SitePartParams
                {
                    randomValue = tier,
                    threatPoints = threatPoints,
                }),
            };
            if (faction == null && MechClusterPart != null)
            {
                // 回退链（§2.3）：敌对派系枚举为空 → 原版机械集群守卫
                parts.Add(new SitePartDefWithParams(MechClusterPart,
                    MechClusterPart.Worker.GenerateDefaultParams(threatPoints, tile, null)));
            }
            return FinalizeSite(parts, tile, faction, label);
        }

        /// <summary>
        /// 生成一处随机据点（公会委托直建新副本入口，与深空扫描同池同威胁公式）：
        /// 仅从有守卫能力的池条目抽取（委托完成判定依赖敌对威胁）；档位由调用方按委托段位映射。
        /// </summary>
        public static Site GenerateRandomSite(Map homeMap, int tier)
        {
            if (homeMap == null)
                return null;
            ScanSiteEntry entry = CompDeepSpaceScanner.PickRandomSiteEntry(true);
            if (entry == null || entry.sitePart == null)
                return null;
            ScanTier tierCfg = CompDeepSpaceScanner.GetTierConfig(tier);
            float threat = BaseThreatPoints(homeMap) * entry.threatFactor * tierCfg.threatFactor;
            if (entry.factionGuards)
            {
                return GenerateDungeonSite(homeMap, entry.sitePart, tierCfg.tier,
                    entry.threatFactor, tierCfg.threatFactor, entry.label);
            }
            return GenerateStockpileSite(homeMap, entry.sitePart, tierCfg.tier,
                threat, entry.mechGuardFallback, entry.label);
        }

        /// <summary>
        /// 生成非派系据点（既有三类资源点）：档位同样经 randomValue 带给 worker（分档奖励用）；
        /// 守卫走原版机械集群 SitePart（mechGuardFallback 且装了 Royalty 时），沿用旧版行为。
        /// </summary>
        public static Site GenerateStockpileSite(Map homeMap, SitePartDef partDef, int tier,
            float threatPoints, bool mechGuardFallback, string label)
        {
            if (homeMap == null || partDef == null)
                return null;
            if (!TryFindTile(homeMap, out PlanetTile tile))
                return null;
            List<SitePartDefWithParams> parts = new List<SitePartDefWithParams>
            {
                new SitePartDefWithParams(partDef, new SitePartParams
                {
                    randomValue = tier,
                    threatPoints = 0f,
                }),
            };
            if (mechGuardFallback && MechClusterPart != null)
            {
                parts.Add(new SitePartDefWithParams(MechClusterPart,
                    MechClusterPart.Worker.GenerateDefaultParams(threatPoints, tile, null)));
            }
            return FinalizeSite(parts, tile, null, label);
        }
    }

    /// <summary>
    /// 副本分档奖励生成（§3.1/§3.2）：副本 worker 与旧奖励堆 worker 共用。
    /// 静态 rewards 由调用方自行处理（旧 worker 既有逻辑）；此处只处理 tieredRewards/gearRewards。
    /// </summary>
    public static class DungeonRewardGen
    {
        /// <summary>从地图读据点档位（parms.randomValue；旧存档/非法值按 I 档处理）。</summary>
        public static int TierOf(Map map, SitePartDef partDef)
        {
            Site site = map.Parent as Site;
            if (site != null)
            {
                foreach (SitePart p in site.parts)
                {
                    if (p.def == partDef)
                    {
                        int t = p.parms.randomValue;
                        return t >= 1 && t <= 3 ? t : 1;
                    }
                }
            }
            return 1;
        }

        /// <summary>
        /// 生成分档奖励（tieredRewards）与装备箱（gearRewards）。
        /// cellPicker：以某格为中心找落点（返回 Invalid 时用 fallback 格）。
        /// </summary>
        public static void SpawnTiered(Map map, StockpileRewardExtension ext, int tier,
            System.Func<IntVec3, IntVec3> cellPicker, IntVec3 fallbackCenter)
        {
            if (ext == null)
                return;
            ScanTier tierCfg = CompDeepSpaceScanner.GetTierConfig(tier);
            if (ext.tieredRewards != null)
            {
                foreach (DungeonRewardEntry entry in ext.tieredRewards)
                {
                    if (entry == null || !entry.AvailableAt(tier))
                        continue;
                    int count = entry.countRange.RandomInRange;
                    if (count <= 0)
                        continue;
                    Thing stack = ThingMaker.MakeThing(entry.ThingDef);
                    stack.stackCount = count;
                    IntVec3 cell = cellPicker != null ? cellPicker(fallbackCenter) : fallbackCenter;
                    GenSpawn.Spawn(stack, cell.IsValid ? cell : fallbackCenter, map);
                }
            }
            if (ext.gearRewards != null)
            {
                foreach (DungeonGearEntry gear in ext.gearRewards)
                {
                    if (gear == null || !gear.AvailableAt(tier))
                        continue;
                    int n = gear.count.RandomInRange;
                    for (int i = 0; i < n; i++)
                    {
                        ThingWithComps piece = DungeonGearGen.Generate(gear.kind, tierCfg.lootTier,
                            tierCfg.gearQualityMin, tierCfg.gearQualityMax);
                        if (piece == null)
                            continue;
                        IntVec3 cell = cellPicker != null ? cellPicker(fallbackCenter) : fallbackCenter;
                        GenSpawn.Spawn(piece, cell.IsValid ? cell : fallbackCenter, map);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 副本据点 worker（mmt-dungeon-design.md §4）：PostMapGenerate 程序化布局——
    /// 外墙围合（1~2 入口）+ 奖励库房 + 守卫哨位（掩体/炮塔）+ 照明；Boss 巢穴为同 worker 加大参数 + 中央巢位。
    /// 守卫经宿主派系 PawnGroupMaker 产出（mod 单位自动在列）；炮塔全库扫描 + 同包加权 + 预算过滤（§4.4）；
    /// 供电走原版 GenStep_Power（与原版任务据点同机制）。无 Comp、无 per-tick 扫描。
    /// </summary>
    public class SitePartWorker_MMT_Dungeon : SitePartWorker
    {
        // ==================== 炮塔全库扫描缓存（Def 库运行期不变，只扫一次，§4.4） ====================

        private class TurretCandidate
        {
            public ThingDef def;
            public ModContentPack pack;
            public float combatPower;
            public bool needsPower;
        }

        private static List<TurretCandidate> turretCache;

        private static List<TurretCandidate> TurretCache
        {
            get
            {
                if (turretCache != null)
                    return turretCache;
                turretCache = new List<TurretCandidate>();
                foreach (ThingDef d in DefDatabase<ThingDef>.AllDefsListForReading)
                {
                    if (d.building == null || d.building.turretGunDef == null)
                        continue;
                    if (MMTSettings.DungeonTurretBlacklisted(d.defName))
                        continue;
                    if (TurretGunUtility.NeedsShells(d)) // 迫击炮等需弹药/操炮，跳过
                        continue;
                    if (d.HasComp(typeof(CompMannable)) || d.HasComp(typeof(CompCanBeDormant)) || d.HasComp(typeof(CompInitiatable)))
                        continue;
                    float cp = d.building.combatPower;
                    if (cp <= 0f)
                        cp = d.BaseMarketValue / 10f;
                    if (cp <= 0f)
                        continue;
                    turretCache.Add(new TurretCandidate
                    {
                        def = d,
                        pack = d.modContentPack,
                        combatPower = cp,
                        needsPower = d.HasComp(typeof(CompPowerTrader)),
                    });
                }
                return turretCache;
            }
        }

        /// <summary>
        /// 炮塔选取（§4.4 优先级：扫描+预算过滤 → XML 白名单 → null 纯人力哨位）：
        /// techLevel ≤ 宿主派系、单座 combatPower ≤ 工事预算；同 modContentPack 权重 ×3。
        /// </summary>
        private static TurretCandidate TryPickTurret(DungeonLayoutExtension layout, Faction faction, float fortBudget)
        {
            if (faction != null)
            {
                int factionTech = (int)faction.def.techLevel;
                List<Pair<TurretCandidate, float>> valid = new List<Pair<TurretCandidate, float>>();
                List<TurretCandidate> cache = TurretCache;
                for (int i = 0; i < cache.Count; i++)
                {
                    TurretCandidate c = cache[i];
                    if ((int)c.def.techLevel > factionTech || c.combatPower > fortBudget)
                        continue;
                    float w = (c.pack != null && faction.def.modContentPack == c.pack) ? 3f : 1f;
                    valid.Add(new Pair<TurretCandidate, float>(c, w));
                }
                if (valid.Count > 0)
                    return valid.RandomElementByWeight(p => p.Second).First;
            }
            // XML 白名单兜底（派系为空无法自适应时）
            if (!layout.turretDefs.NullOrEmpty())
            {
                List<ThingDef> valid = new List<ThingDef>();
                for (int i = 0; i < layout.turretDefs.Count; i++)
                {
                    if (MMTSettings.DungeonTurretBlacklisted(layout.turretDefs[i]))
                        continue;
                    ThingDef d = DefDatabase<ThingDef>.GetNamedSilentFail(layout.turretDefs[i]);
                    if (d != null && d.building != null && d.building.turretGunDef != null)
                        valid.Add(d);
                }
                if (valid.Count > 0)
                {
                    ThingDef pick = valid.RandomElement();
                    return new TurretCandidate
                    {
                        def = pick,
                        pack = pick.modContentPack,
                        combatPower = pick.building.combatPower > 0f ? pick.building.combatPower : 1f,
                        needsPower = pick.HasComp(typeof(CompPowerTrader)),
                    };
                }
            }
            return null;
        }

        // ==================== 主流程 ====================

        public override void PostMapGenerate(Map map)
        {
            base.PostMapGenerate(map);
            Site site = map.Parent as Site;
            if (site == null)
                return;
            SitePart part = null;
            foreach (SitePart p in site.parts)
            {
                if (p.def == def)
                {
                    part = p;
                    break;
                }
            }
            int tier = DungeonRewardGen.TierOf(map, def);
            float threatPoints = part != null ? part.parms.threatPoints : 0f;
            DungeonLayoutExtension layout = def.GetModExtension<DungeonLayoutExtension>() ?? new DungeonLayoutExtension();
            StockpileRewardExtension rewardExt = def.GetModExtension<StockpileRewardExtension>();
            Faction faction = site.Faction;

            // 1. 守卫生成（先抽人不落位，§4.4 材质采样要用其装备）
            float guardPoints = Mathf.Min(threatPoints * DungeonSiteGen.GuardBudgetFraction, DungeonSiteGen.GuardPointsCap);
            List<Pawn> guards = null;
            if (faction != null && guardPoints > 0f)
                guards = GenerateGuards(faction, guardPoints, map);

            // 2. 墙体/掩体材质：守卫装备采样优先（§4.4），回退 XML/默认
            ThingDef wallDef = layout.ResolveWallDef();
            ThingDef wallStuff = SampleStuffFromGuards(guards) ?? layout.ResolveWallStuff(wallDef);

            // 3. 玩家蓝图选用：在设置面板为本据点类型启用过蓝图时随机抽一张，无则程序化布局兜底
            DungeonBlueprint blueprint = DungeonBlueprintStore.PickFor(def, tier);

            // 4. 布局中心：地图中心附近找可建造区（蓝图为其外接矩形半径）
            int half = blueprint != null
                ? Mathf.Max(blueprint.width, blueprint.height) / 2 + 1
                : layout.wallRadiusRange.RandomInRange;
            IntVec3 center = FindLayoutCenter(map, half);

            List<IntVec3> postCells;
            bool poweredTurret;
            if (blueprint != null)
            {
                // 蓝图路径：逐条落建筑（炮塔挂宿主派系），奖励散布内部空格（无独立库房）
                poweredTurret = StampBlueprint(map, blueprint, center, faction, out postCells);
                SpawnRewards(map, CellRect.CenteredOn(center, half - 1), rewardExt, tier);
            }
            else
            {
                // 5. 外墙 + 入口
                List<IntVec3> entranceCells;
                List<IntVec3> innerGateCells;
                BuildWalls(map, center, half, wallDef, wallStuff, layout, out entranceCells, out innerGateCells);

                // 6. 奖励库房（Boss 巢穴：库房偏移，中央留开阔巢位）
                IntVec3 vaultCenter = center;
                if (layout.HasBossNest)
                    vaultCenter = center + new IntVec3(layout.vaultRadiusRange.max + 3, 0, 0);
                int vaultHalf = layout.vaultRadiusRange.RandomInRange;
                CellRect vaultInterior = BuildVault(map, vaultCenter, vaultHalf, wallDef, wallStuff, layout, faction);

                // 7. 奖励堆 + 装备箱
                SpawnRewards(map, vaultInterior, rewardExt, tier);

                // 8. 守卫哨位（掩体 + 可选炮塔）与照明
                float fortBudget = threatPoints * DungeonSiteGen.FortBudgetFraction;
                poweredTurret = BuildPosts(map, center, half, layout, faction, wallStuff,
                    entranceCells, innerGateCells, fortBudget, out postCells);
                BuildLights(map, center, half, layout, faction);
            }

            // 8. 守卫落位 + lord（敌意全走原版机制）
            Lord lord = null;
            if (guards != null && guards.Count > 0)
            {
                lord = LordMaker.MakeNewLord(faction,
                    new LordJob_SitePawns(faction, center, Rand.RangeInclusive(2500, 5000)), map);
                SpawnGuards(map, guards, postCells, center, half, lord);
            }

            // 9. Boss（MMT 独占单位，defName 解析失败静默跳过——用户后续自建 Boss 后填入即生效）
            TrySpawnBoss(map, layout, faction, center, lord);

            // 10. 炮塔供电：原版 GenStep_Power（与任务据点同机制，自动接发电机+电池+电缆）
            if (poweredTurret)
                new GenStep_Power().Generate(map, new GenStepParams());
        }

        // ==================== 守卫 ====================

        /// <summary>经宿主派系 PawnGroupMaker 抽组守卫（mod 单位自动在列；失败静默返回空表）。</summary>
        private static List<Pawn> GenerateGuards(Faction faction, float points, Map map)
        {
            PawnGroupKindDef groupKind = PawnGroupKindDefOf.Combat;
            if (!faction.def.pawnGroupMakers.Any(m => m.kindDef == groupKind))
                groupKind = PawnGroupKindDefOf.Settlement;
            PawnGroupMakerParms parms = new PawnGroupMakerParms
            {
                groupKind = groupKind,
                tile = map.Tile,
                faction = faction,
                inhabitants = true,
                generateFightersOnly = true,
                seed = Rand.Int,
            };
            parms.points = Mathf.Max(points, faction.def.MinPointsToGeneratePawnGroup(groupKind, parms));
            List<Pawn> pawns = new List<Pawn>();
            foreach (Pawn p in PawnGroupMakerUtility.GeneratePawns(parms, false))
            {
                if (p != null)
                    pawns.Add(p);
            }
            return pawns;
        }

        /// <summary>守卫落位：优先哨位旁，其次据点内部空位；全部加入 lord。</summary>
        private static void SpawnGuards(Map map, List<Pawn> guards, List<IntVec3> postCells,
            IntVec3 center, int half, Lord lord)
        {
            for (int i = 0; i < guards.Count; i++)
            {
                IntVec3 near = (postCells.Count > 0)
                    ? postCells[i % postCells.Count]
                    : center;
                IntVec3 cell;
                if (!CellFinder.TryFindRandomSpawnCellForPawnNear(near, map, out cell, 6)
                    && !CellFinder.TryFindRandomSpawnCellForPawnNear(center, map, out cell, half))
                    cell = center;
                GenSpawn.Spawn(guards[i], cell, map);
                lord?.AddPawn(guards[i]);
            }
        }

        /// <summary>Boss 生成：PawnKindDef 惰性解析（用户后续自建 Boss 填入 defName 即生效）；
        /// 武器稀有度掷点由 LootRoll 的生成钩子自动结算（敌对派系），无需重复掷。</summary>
        private static void TrySpawnBoss(Map map, DungeonLayoutExtension layout, Faction faction,
            IntVec3 center, Lord lord)
        {
            if (!layout.HasBossNest || faction == null)
                return;
            PawnKindDef kind = DefDatabase<PawnKindDef>.GetNamedSilentFail(layout.bossPawnKindDefName);
            if (kind == null)
                return;
            PawnGenerationRequest req = new PawnGenerationRequest(kind, faction,
                PawnGenerationContext.NonPlayer, map.Tile, mustBeCapableOfViolence: true);
            Pawn boss = PawnGenerator.GeneratePawn(req);
            if (boss == null)
                return;
            IntVec3 cell;
            if (!CellFinder.TryFindRandomSpawnCellForPawnNear(center, map, out cell, 5))
                cell = center;
            GenSpawn.Spawn(boss, cell, map);
            lord?.AddPawn(boss);
        }

        // ==================== 材质采样（§4.4） ====================

        /// <summary>统计守卫武器/服装的 StuffDef 出现频次，得票最高者作为墙体与掩体材质（采不到返回 null）。</summary>
        private static ThingDef SampleStuffFromGuards(List<Pawn> guards)
        {
            if (guards == null || guards.Count == 0)
                return null;
            Dictionary<ThingDef, int> votes = new Dictionary<ThingDef, int>();
            for (int i = 0; i < guards.Count; i++)
            {
                CountStuff(votes, guards[i].equipment?.Primary);
                List<Apparel> worn = guards[i].apparel?.WornApparel;
                if (worn == null)
                    continue;
                for (int j = 0; j < worn.Count; j++)
                    CountStuff(votes, worn[j]);
            }
            ThingDef best = null;
            int bestCount = 0;
            foreach (KeyValuePair<ThingDef, int> kv in votes)
            {
                if (kv.Value > bestCount)
                {
                    bestCount = kv.Value;
                    best = kv.Key;
                }
            }
            return best;
        }

        private static void CountStuff(Dictionary<ThingDef, int> votes, ThingWithComps t)
        {
            ThingDef stuff = t?.Stuff;
            if (stuff == null)
                return;
            votes.TryGetValue(stuff, out int n);
            votes[stuff] = n + 1;
        }

        // ==================== 布局 ====================

        /// <summary>找布局中心：优先地图中心附近、矩形内大半可建造的格。</summary>
        private static IntVec3 FindLayoutCenter(Map map, int half)
        {
            IntVec3 center;
            if (RCellFinder.TryFindRandomCellNearTheCenterOfTheMapWith(
                c => RectBuildableRatio(map, CellRect.CenteredOn(c, half + 1)) >= 0.5f, map, out center))
                return center;
            return map.Center;
        }

        private static float RectBuildableRatio(Map map, CellRect rect)
        {
            int total = 0;
            int ok = 0;
            foreach (IntVec3 c in rect)
            {
                if (!c.InBounds(map))
                    continue;
                total++;
                if (CanPlaceBuilding(map, c, ThingDefOf.Wall))
                    ok++;
            }
            return total == 0 ? 0f : (float)ok / total;
        }

        /// <summary>通用建筑落位判定：界内 + 地形承载 + 无既有建筑/山体。</summary>
        private static bool CanPlaceBuilding(Map map, IntVec3 cell, ThingDef def)
        {
            if (!cell.InBounds(map))
                return false;
            if (cell.GetFirstBuilding(map) != null || cell.GetEdifice(map) != null)
                return false;
            if (def != null && def.terrainAffordanceNeeded != null)
            {
                TerrainDef terrain = cell.GetTerrain(map);
                if (terrain == null || terrain.affordances == null
                    || !terrain.affordances.Contains(def.terrainAffordanceNeeded))
                    return false;
            }
            return true;
        }

        /// <summary>生成建筑（失败返回 null，静默）；stuff 仅 madeFromStuff 时使用；faction 非空且可持阵营时挂派系。</summary>
        private static Thing TrySpawnBuilding(Map map, ThingDef def, ThingDef stuff, IntVec3 cell, Rot4 rot, Faction faction)
        {
            if (def == null || !CanPlaceBuilding(map, cell, def))
                return null;
            Thing thing = ThingMaker.MakeThing(def, def.MadeFromStuff ? stuff : null);
            if (thing == null)
                return null;
            GenSpawn.Spawn(thing, cell, map, rot);
            if (faction != null && thing.def.CanHaveFaction)
                thing.SetFaction(faction);
            return thing;
        }

        /// <summary>外墙围合（1~2 处入口豁口；中立无阵营）。返回入口外侧格与内侧哨位格。</summary>
        private static void BuildWalls(Map map, IntVec3 center, int half, ThingDef wallDef, ThingDef stuff,
            DungeonLayoutExtension layout, out List<IntVec3> entranceCells, out List<IntVec3> innerGateCells)
        {
            entranceCells = new List<IntVec3>();
            innerGateCells = new List<IntVec3>();
            CellRect ring = CellRect.CenteredOn(center, half);
            // 选 1~2 个入口：随机边 + 边上随机 2 格豁口（避开角部）
            int entranceCount = Mathf.Clamp(layout.entranceCountRange.RandomInRange, 1, 4);
            List<int> sides = new List<int> { 0, 1, 2, 3 }; // 0=北 1=东 2=南 3=西
            for (int k = sides.Count - 1; k > 0; k--)
            {
                int j = Rand.RangeInclusive(0, k);
                (sides[k], sides[j]) = (sides[j], sides[k]);
            }
            HashSet<IntVec3> gaps = new HashSet<IntVec3>();
            for (int i = 0; i < Mathf.Min(entranceCount, sides.Count); i++)
            {
                int side = sides[i];
                int offset = Rand.RangeInclusive(-half + 2, half - 3);
                IntVec3 gapCell = SideCell(center, half, side, offset);
                IntVec3 gapCell2 = SideCell(center, half, side, offset + 1);
                gaps.Add(gapCell);
                gaps.Add(gapCell2);
                IntVec3 outward = SideNormal(side);
                entranceCells.Add(gapCell + outward);
                innerGateCells.Add(gapCell - outward * 2);
            }
            foreach (IntVec3 cell in ring.EdgeCells)
            {
                if (gaps.Contains(cell))
                    continue;
                TrySpawnBuilding(map, wallDef, stuff, cell, Rot4.North, null);
            }
        }

        /// <summary>某条边上距中心偏移 offset 的外墙格。</summary>
        private static IntVec3 SideCell(IntVec3 center, int half, int side, int offset)
        {
            switch (side)
            {
                case 0: return center + new IntVec3(offset, 0, half);
                case 1: return center + new IntVec3(half, 0, offset);
                case 2: return center + new IntVec3(offset, 0, -half);
                default: return center + new IntVec3(-half, 0, offset);
            }
        }

        private static IntVec3 SideNormal(int side)
        {
            switch (side)
            {
                case 0: return new IntVec3(0, 0, 1);
                case 1: return new IntVec3(1, 0, 0);
                case 2: return new IntVec3(0, 0, -1);
                default: return new IntVec3(-1, 0, 0);
            }
        }

        /// <summary>奖励库房：墙 + 一扇门（门挂宿主派系，§4.3）。返回内部区域。</summary>
        private static CellRect BuildVault(Map map, IntVec3 center, int vaultHalf, ThingDef wallDef,
            ThingDef stuff, DungeonLayoutExtension layout, Faction faction)
        {
            CellRect interior = CellRect.CenteredOn(center, vaultHalf);
            CellRect ring = CellRect.CenteredOn(center, vaultHalf + 1);
            ThingDef doorDef = layout.ResolveDoorDef();
            int doorSide = Rand.RangeInclusive(0, 3);
            IntVec3 doorCell = SideCell(center, vaultHalf + 1, doorSide, 0);
            foreach (IntVec3 cell in ring.EdgeCells)
            {
                if (cell == doorCell)
                {
                    if (doorDef != null)
                        TrySpawnBuilding(map, doorDef, GenStuff.DefaultStuffFor(doorDef), cell, Rot4.North, faction);
                    continue;
                }
                TrySpawnBuilding(map, wallDef, stuff, cell, Rot4.North, null);
            }
            return interior;
        }

        /// <summary>守卫哨位：入口哨位 + 环内随机哨位；每哨位 2~3 掩体（贴墙内侧）+ 可选炮塔（靠内一格）。
        /// 炮塔数量受工事预算约束；返回是否生成了需供电炮塔，输出哨位格供守卫落位。</summary>
        private static bool BuildPosts(Map map, IntVec3 center, int half, DungeonLayoutExtension layout,
            Faction faction, ThingDef coverStuff, List<IntVec3> entranceCells, List<IntVec3> innerGateCells,
            float fortBudget, out List<IntVec3> postCells)
        {
            postCells = new List<IntVec3>();
            bool poweredTurret = false;
            int postCount = Mathf.Max(layout.postCountRange.RandomInRange, entranceCells.Count);
            // 哨位锚点：入口内侧哨位优先，其余沿环内侧随机
            List<IntVec3> anchors = new List<IntVec3>(innerGateCells);
            int guard = 0;
            while (anchors.Count < postCount && guard++ < 40)
            {
                int side = Rand.RangeInclusive(0, 3);
                int offset = Rand.RangeInclusive(-half + 3, half - 3);
                IntVec3 anchor = SideCell(center, half - 2, side, offset);
                bool tooClose = false;
                foreach (IntVec3 a in anchors)
                {
                    if ((a - anchor).LengthManhattan < 4)
                    {
                        tooClose = true;
                        break;
                    }
                }
                if (!tooClose)
                    anchors.Add(anchor);
            }
            TurretCandidate turret = TryPickTurret(layout, faction, fortBudget);
            int turretsLeft = turret == null ? 0 : Mathf.Min(anchors.Count, Mathf.FloorToInt(fortBudget / turret.combatPower));
            ThingDef coverDef = layout.ResolveWeighted(layout.coverDefs) ?? ThingDefOf.Sandbags;
            for (int i = 0; i < anchors.Count; i++)
            {
                IntVec3 anchor = anchors[i];
                IntVec3 outward = anchor - center;
                // 沿墙方向的切线（掩体横排）
                IntVec3 tangent = Mathf.Abs(outward.x) > Mathf.Abs(outward.z)
                    ? new IntVec3(0, 0, 1)
                    : new IntVec3(1, 0, 0);
                int coverCount = Rand.RangeInclusive(2, 3);
                for (int k = 0; k < coverCount; k++)
                {
                    IntVec3 coverCell = anchor + tangent * (k - 1);
                    TrySpawnBuilding(map, coverDef, coverStuff, coverCell, Rot4.North, null);
                }
                postCells.Add(anchor);
                // 炮塔放掩体靠内一格（朝向远离中心 = 朝外）
                if (turretsLeft > 0)
                {
                    IntVec3 inward = new IntVec3(-System.Math.Sign(outward.x), 0, -System.Math.Sign(outward.z));
                    IntVec3 turretCell = anchor + inward * 2;
                    Rot4 face = outward.x > 0 ? Rot4.East
                        : outward.x < 0 ? Rot4.West
                        : outward.z > 0 ? Rot4.North : Rot4.South;
                    if (TrySpawnBuilding(map, turret.def,
                        turret.def.MadeFromStuff ? GenStuff.DefaultStuffFor(turret.def) : null,
                        turretCell, face, faction) != null)
                    {
                        turretsLeft--;
                        if (turret.needsPower)
                            poweredTurret = true;
                    }
                }
            }
            return poweredTurret;
        }

        /// <summary>照明点缀（默认火把，生成时补满燃料；挂宿主派系）。</summary>
        private static void BuildLights(Map map, IntVec3 center, int half, DungeonLayoutExtension layout, Faction faction)
        {
            ThingDef lightDef = layout.ResolveWeighted(layout.lightDefs);
            if (lightDef == null)
                lightDef = ThingDefOf.TorchLamp;
            if (lightDef == null)
                return;
            int lightCount = Rand.RangeInclusive(3, 5);
            for (int i = 0; i < lightCount; i++)
            {
                IntVec3 cell = CellFinderLoose.RandomCellWith(
                    c => (c - center).LengthHorizontal <= half - 2 && (c - center).LengthHorizontal >= 3f
                        && c.Standable(map) && c.GetFirstBuilding(map) == null, map, 30);
                if (!cell.IsValid)
                    continue;
                Thing lamp = TrySpawnBuilding(map, lightDef,
                    lightDef.MadeFromStuff ? GenStuff.DefaultStuffFor(lightDef) : null, cell, Rot4.North, faction);
                CompRefuelable fuel = lamp?.TryGetComp<CompRefuelable>();
                if (fuel != null)
                    fuel.Refuel(fuel.Props.fuelCapacity);
            }
        }

        // ==================== 玩家蓝图 ====================

        /// <summary>
        /// 盖蓝图：按记录的偏移逐条落建筑（def 缺失静默跳过；炮塔挂宿主派系，其余中立；
        /// 材质缺失用 GenStuff 默认）。postCells 取炮塔/门格（无则中心）。返回是否生成了需供电建筑。
        /// </summary>
        private static bool StampBlueprint(Map map, DungeonBlueprint bp, IntVec3 center, Faction faction,
            out List<IntVec3> postCells)
        {
            postCells = new List<IntVec3>();
            bool needsPower = false;
            IntVec3 origin = center - new IntVec3(bp.width / 2, 0, bp.height / 2);
            for (int i = 0; i < bp.entries.Count; i++)
            {
                DungeonBlueprintEntry e = bp.entries[i];
                if (e == null || e.defName.NullOrEmpty())
                    continue;
                ThingDef bd = DefDatabase<ThingDef>.GetNamedSilentFail(e.defName);
                if (bd == null || bd.building == null)
                    continue;
                ThingDef stuff = null;
                if (bd.MadeFromStuff)
                {
                    if (!e.stuffDefName.NullOrEmpty())
                        stuff = DefDatabase<ThingDef>.GetNamedSilentFail(e.stuffDefName);
                    if (stuff == null)
                        stuff = GenStuff.DefaultStuffFor(bd);
                }
                IntVec3 cell = origin + e.offset;
                bool isTurret = bd.building.turretGunDef != null;
                Thing spawned = TrySpawnBuilding(map, bd, stuff, cell, new Rot4(e.rot), isTurret ? faction : null);
                if (spawned == null)
                    continue;
                if (isTurret || spawned is Building_Door)
                    postCells.Add(cell);
                if (bd.HasComp(typeof(CompPowerTrader)))
                    needsPower = true;
                // 燃料灯具生成时补满（与程序化布局一致）
                CompRefuelable fuel = spawned.TryGetComp<CompRefuelable>();
                if (fuel != null)
                    fuel.Refuel(fuel.Props.fuelCapacity);
            }
            if (postCells.Count == 0)
                postCells.Add(center);
            return needsPower;
        }

        // ==================== 奖励 ====================

        /// <summary>奖励堆 + 装备箱（§3.1/§3.2）：静态堆全档位，tieredRewards/gearRewards 走共享分档生成。</summary>
        private void SpawnRewards(Map map, CellRect interior, StockpileRewardExtension ext, int tier)
        {
            if (ext == null)
                return;
            List<IntVec3> cells = new List<IntVec3>();
            foreach (IntVec3 c in interior)
            {
                if (c.InBounds(map) && c.Standable(map) && c.GetFirstBuilding(map) == null)
                    cells.Add(c);
            }
            IntVec3 fallback = cells.Count > 0 ? cells[0] : map.Center;
            if (ext.rewards != null)
            {
                foreach (ThingDefCountRangeClass reward in ext.rewards)
                {
                    if (reward?.thingDef == null)
                        continue;
                    int count = reward.countRange.RandomInRange;
                    if (count <= 0)
                        continue;
                    Thing stack = ThingMaker.MakeThing(reward.thingDef);
                    stack.stackCount = count;
                    GenSpawn.Spawn(stack, PickCell(cells, fallback), map);
                }
            }
            DungeonRewardGen.SpawnTiered(map, ext, tier, c => PickCell(cells, c), fallback);
        }

        private static IntVec3 PickCell(List<IntVec3> cells, IntVec3 fallback)
        {
            return cells.Count > 0 ? cells.RandomElement() : fallback;
        }
    }

    /// <summary>
    /// 装备箱生成（§3.2）：武器走 PawnWeaponGenerator.AllWeaponPairs（含 mod 武器，
    /// 沿用原版技术等级过滤思路，≤ 玩家科技等级）；防具扫全库 IsApparel。
    /// 品质按档位地板/天花板掷点，强化/附魔走 LootRoll.Roll（§1.3 稀有度地板）。
    /// </summary>
    public static class DungeonGearGen
    {
        /// <summary>生成一件随机强化/附魔装备（品质在地板~天花板间均匀，掷点失败/无候选静默返回 null）。</summary>
        public static ThingWithComps Generate(DungeonGearKind kind, int lootTier, QualityCategory qMin, QualityCategory qMax)
        {
            TechLevel cap = TechLevel.Spacer;
            Faction player = Faction.OfPlayerSilentFail;
            if (player != null)
                cap = player.def.techLevel;
            bool wantWeapon = kind == DungeonGearKind.Weapon
                || (kind == DungeonGearKind.Any && Rand.Bool);
            ThingStuffPair pair = wantWeapon ? PickWeapon(cap) : PickApparel(cap);
            if (pair.thing == null && kind == DungeonGearKind.Any)
            {
                // 一类池空时换另一类（Any 语义兜底）
                pair = wantWeapon ? PickApparel(cap) : PickWeapon(cap);
            }
            if (pair.thing == null)
                return null;
            ThingWithComps gear = (ThingWithComps)ThingMaker.MakeThing(pair.thing, pair.stuff);
            QualityCategory q = (QualityCategory)Rand.RangeInclusive((int)qMin, (int)qMax);
            gear.TryGetComp<CompQuality>()?.SetQuality(q, ArtGenerationContext.Colony);
            LootRoll.Roll(gear, lootTier);
            return gear;
        }

        /// <summary>武器：原版全武器对（武器×材料）按科技等级过滤，权重沿用 commonality。</summary>
        private static ThingStuffPair PickWeapon(TechLevel cap)
        {
            List<ThingStuffPair> all = PawnWeaponGenerator.AllWeaponPairs;
            List<Pair<ThingStuffPair, float>> valid = new List<Pair<ThingStuffPair, float>>();
            for (int i = 0; i < all.Count; i++)
            {
                ThingStuffPair w = all[i];
                if ((int)w.thing.techLevel > (int)cap || w.thing.destroyOnDrop)
                    continue;
                if (w.stuff != null && !w.stuff.stuffProps.allowedInStuffGeneration)
                    continue;
                float weight = w.Commonality;
                if (weight <= 0f)
                    continue;
                valid.Add(new Pair<ThingStuffPair, float>(w, weight));
            }
            if (valid.Count == 0)
                return default(ThingStuffPair);
            return valid.RandomElementByWeight(p => p.Second).First;
        }

        // 防具候选缓存：Def 库加载后不变，按科技等级只扫一次（避免每件装备全库扫描）
        private static readonly Dictionary<TechLevel, List<Pair<ThingDef, float>>> apparelCache =
            new Dictionary<TechLevel, List<Pair<ThingDef, float>>>();

        /// <summary>防具：全库 IsApparel 过滤（含 mod 防具），权重 = generateCommonality（下限 0.05）。</summary>
        private static ThingStuffPair PickApparel(TechLevel cap)
        {
            List<Pair<ThingDef, float>> valid;
            if (!apparelCache.TryGetValue(cap, out valid))
            {
                valid = new List<Pair<ThingDef, float>>();
                foreach (ThingDef d in DefDatabase<ThingDef>.AllDefsListForReading)
                {
                    if (!d.IsApparel || (int)d.techLevel > (int)cap || d.destroyOnDrop || d.BaseMarketValue <= 0f)
                        continue;
                    if (d.apparel == null || d.apparel.bodyPartGroups.NullOrEmpty())
                        continue;
                    valid.Add(new Pair<ThingDef, float>(d, Mathf.Max(0.05f, d.generateCommonality)));
                }
                apparelCache[cap] = valid;
            }
            if (valid.Count == 0)
                return default(ThingStuffPair);
            ThingDef def = valid.RandomElementByWeight(p => p.Second).First;
            ThingDef stuff = def.MadeFromStuff ? GenStuff.RandomStuffByCommonalityFor(def, cap) : null;
            return new ThingStuffPair(def, stuff, 1f);
        }
    }
}
