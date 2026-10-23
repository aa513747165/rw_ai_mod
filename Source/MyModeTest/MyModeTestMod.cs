using HarmonyLib;
using Verse;

namespace MyModeTest
{
    /// <summary>
    /// myModeTest 的 Mod 入口，确认程序集被 RimWorld 加载并注册 Harmony 补丁。
    /// </summary>
    public sealed class MyModeTestMod : Mod
    {
        public MyModeTestMod(ModContentPack content) : base(content)
        {
            // 流程开始：Mod 构造函数由游戏调用，记录加载日志并应用全部 Harmony 补丁。
            Log.Message("[myModeTest] Mod loaded.");
            new Harmony("MyModeTest.Harmony").PatchAll();
        }
    }
}
