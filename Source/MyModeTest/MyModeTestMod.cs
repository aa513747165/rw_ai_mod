using Verse;

namespace MyModeTest
{
    /// <summary>
    /// myModeTest 的 Mod 入口，确认程序集被 RimWorld 加载。
    /// </summary>
    public sealed class MyModeTestMod : Mod
    {
        public MyModeTestMod(ModContentPack content) : base(content)
        {
            // 流程开始：Mod 构造函数由游戏调用，记录加载日志。
            Log.Message("[myModeTest] Mod loaded.");
        }
    }
}
