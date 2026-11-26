using RimWorld;
using Verse;

namespace MyModeTest
{
    /// <summary>
    /// 魔物派系标记：PawnKindDef 挂本扩展即由 BossPool.ResolveBossFaction 路由进 MMT_Monster
    /// 隐藏派系（四型 Boss 通用：元素/类人/机械/建筑类）。空标记类，派系归属判断走 XML，
    /// C# 零硬编码 defName；其它 mod 可 XPath 给自己单位挂该扩展复用魔物派系。
    /// </summary>
    public class DefModExtension_MMTMonsterFaction : DefModExtension
    {
    }

    /// <summary>
    /// MMT_Monster 派系惰性解析：世界生成 requiredCountAtGameStart=1 保证存在；
    /// 失败返回 null（静默回退 ResolveBossFaction 现有路由，不打断生成）。
    /// </summary>
    public static class MMTMonsterFaction
    {
        private static Faction cached;
        private static bool resolved;

        /// <summary>魔物派系实例（FirstFactionOfDef 惰性缓存）；取不到返回 null。</summary>
        public static Faction Get
        {
            get
            {
                if (!resolved)
                {
                    resolved = true;
                    if (Find.FactionManager != null)
                        cached = Find.FactionManager.FirstFactionOfDef(
                            DefDatabase<FactionDef>.GetNamedSilentFail("MMT_Monster"));
                }
                return cached;
            }
        }
    }
}
