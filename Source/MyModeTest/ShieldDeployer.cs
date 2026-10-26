using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.Sound;

namespace MyModeTest
{
    /// <summary>
    /// 使用效果参数：使用物品后要生成的建筑 def（部署式护盾投影器）。
    /// </summary>
    public class CompProperties_UseEffectSpawnBuilding : CompProperties_UseEffect
    {
        /// <summary>使用后在使用者脚下生成的建筑 def（临时护盾投影器）。</summary>
        public ThingDef buildingDef;

        public CompProperties_UseEffectSpawnBuilding()
        {
            compClass = typeof(CompUseEffect_SpawnBuilding);
        }
    }

    /// <summary>
    /// 使用效果 Comp：在使用者脚下生成指定建筑（临时护盾投影器）并归属使用者阵营。
    /// 物品销毁由原版 CompUseEffect_DestroySelf 负责，本 Comp 只管生成。
    /// 非征召状态的部署走这条物品使用流程；征召状态的即时部署见 ShieldDeployGizmoPatch。
    /// </summary>
    public class CompUseEffect_SpawnBuilding : CompUseEffect
    {
        private CompProperties_UseEffectSpawnBuilding Props
        {
            get { return (CompProperties_UseEffectSpawnBuilding)props; }
        }

        /// <summary>流程：在使用者脚下生成建筑并归属其阵营（护盾只拦外部投射物）。</summary>
        public override void DoEffect(Pawn usedBy)
        {
            base.DoEffect(usedBy);
            if (Props.buildingDef == null || usedBy.Map == null)
                return;
            // 流程开始：生成建筑（护盾投影器可站立，与使用者同格无冲突）
            Thing building = GenSpawn.Spawn(Props.buildingDef, usedBy.Position, usedBy.Map, Rot4.North);
            building.SetFaction(usedBy.Faction);
            // 部署特效仅对护盾投影器播放（本 Comp 保持通用，可复用生成其他建筑）
            if (Props.buildingDef == MMT_ShieldDefOf.MMT_ShieldProjector)
                ShieldDeployUtility.PlayDeployFx(building);
            // 流程结束
        }
    }

    /// <summary>
    /// 部署式护盾的共享部署逻辑：找背包里的发生器、选落点、生成投影器、播放特效。
    /// 征召即时部署（gizmo）与非征召使用流程（CompUseEffect）共用。
    /// </summary>
    public static class ShieldDeployUtility
    {
        /// <summary>小人背包中的第一件部署式护盾发生器；没有返回 null。</summary>
        public static Thing FindCarriedShield(Pawn pawn)
        {
            if (pawn.inventory == null)
                return null;
            foreach (Thing t in pawn.inventory.innerContainer)
            {
                if (t.def == MMT_ShieldDefOf.MMT_DeployableShield)
                    return t;
            }
            return null;
        }

        /// <summary>征召状态即时部署：消耗背包中一件发生器，在脚下展开护盾投影器。</summary>
        public static void DeployFromPawn(Pawn pawn)
        {
            if (pawn.Map == null)
                return;
            Thing item = FindCarriedShield(pawn);
            if (item == null)
                return;
            IntVec3 cell = FindDeployCell(pawn);
            // stackLimit=1，整个物品即一次用量；显式从背包移除后销毁
            pawn.inventory.innerContainer.Remove(item);
            item.Destroy();
            SpawnProjector(cell, pawn.Map, pawn.Faction);
        }

        /// <summary>生成护盾投影器并播放部署特效。</summary>
        public static Thing SpawnProjector(IntVec3 cell, Map map, Faction faction)
        {
            Thing projector = GenSpawn.Spawn(MMT_ShieldDefOf.MMT_ShieldProjector, cell, map, Rot4.North);
            projector.SetFaction(faction);
            PlayDeployFx(projector);
            return projector;
        }

        /// <summary>部署特效：激活闪光 + 启动音效（复用 Royalty 资源；无 Royalty 时静默跳过）。</summary>
        public static void PlayDeployFx(Thing projector)
        {
            if (FleckDefOf.BroadshieldActivation != null)
                FleckMaker.Static(projector.TrueCenter(), projector.Map, FleckDefOf.BroadshieldActivation);
            if (SoundDefOf.Broadshield_Startup != null)
                SoundDefOf.Broadshield_Startup.PlayOneShot(new TargetInfo(projector.Position, projector.Map));
        }

        /// <summary>部署落点：优先小人脚下，按原版 Verb_DeployBroadshield 逻辑在半径 4 内找第一个可用格。</summary>
        private static IntVec3 FindDeployCell(Pawn pawn)
        {
            Map map = pawn.Map;
            int count = GenRadial.NumCellsInRadius(4f);
            for (int i = 0; i < count; i++)
            {
                IntVec3 cell = pawn.Position + GenRadial.RadialPattern[i];
                if (cell.IsValid && cell.InBounds(map))
                    return cell;
            }
            return pawn.Position;
        }

        /// <summary>"部署护盾"指令按钮（护盾发生器图标），点击立即部署。</summary>
        public static Command_Action MakeDeployCommand(Pawn pawn)
        {
            Command_Action cmd = new Command_Action();
            cmd.defaultLabel = "部署护盾";
            cmd.defaultDesc = "立即在脚下展开临时能量护盾（5×5 圆形范围），拦截外部射入的投射物，护盾内人员仍可向外射击。消耗一件背包中的部署式护盾发生器，护盾燃尽后自动焚毁。";
            cmd.icon = MMT_ShieldDefOf.MMT_DeployableShield.uiIcon;
            cmd.action = delegate { DeployFromPawn(pawn); };
            return cmd;
        }
    }

    /// <summary>
    /// 部署式护盾 gizmo 补丁：小人背包携带护盾发生器且处于征召状态时，
    /// 在指令栏追加"部署护盾"按钮（护盾图标），点击立即释放。
    /// 非征召状态不显示（走物品自带的 CompUsable 使用流程，有部署读条）。
    /// </summary>
    [HarmonyPatch(typeof(Pawn), "GetGizmos")]
    public static class ShieldDeployGizmoPatch
    {
        public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> values, Pawn __instance)
        {
            foreach (Gizmo gizmo in values)
                yield return gizmo;
            if (!__instance.Drafted || !__instance.IsColonistPlayerControlled)
                yield break;
            if (ShieldDeployUtility.FindCarriedShield(__instance) == null)
                yield break;
            yield return ShieldDeployUtility.MakeDeployCommand(__instance);
        }
    }

    [DefOf]
    public static class MMT_ShieldDefOf
    {
        /// <summary>部署式护盾发生器（物品）。</summary>
        public static ThingDef MMT_DeployableShield;
        /// <summary>临时护盾投影器（部署生成的建筑）。</summary>
        public static ThingDef MMT_ShieldProjector;

        static MMT_ShieldDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(MMT_ShieldDefOf));
        }
    }
}
