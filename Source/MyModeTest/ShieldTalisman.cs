using RimWorld;
using Verse;
using Verse.Sound;

namespace MyModeTest
{
    /// <summary>玉质护符参数（无额外字段，气泡贴图继承基类 bubbleTexPath）。</summary>
    public class CompProperties_ShieldTalisman : CompProperties_ShieldExBase
    {
        public CompProperties_ShieldTalisman()
        {
            compClass = typeof(CompShieldTalisman);
        }
    }

    /// <summary>
    /// 玉质护符（中世纪一次性护盾）：无电量概念，直接以护符自身耐久吸收伤害——
    /// 1 点伤害磨损 1 点耐久；耐久不足以全额吸收时，吸收剩余耐久等值伤害后护符碎裂、
    /// 溢出伤害穿透。不可修复、不可回充（修复即变相回充，违背一次性定位），破盾后永不重启。
    /// </summary>
    public class CompShieldTalisman : CompShieldExBase
    {
        /// <summary>气泡尺寸随剩余耐久比例缩放。</summary>
        protected override float ResourceFraction
        {
            get { return parent.MaxHitPoints <= 0 ? 0f : (float)parent.HitPoints / parent.MaxHitPoints; }
        }

        protected override string ToggleLabel
        {
            get { return "玉质护符"; }
        }

        protected override string ToggleDesc
        {
            get
            {
                return "以玉石封存防护力场的古代工艺：开启后全额吸收所有伤害（含近战），"
                    + "每吸收 1 点伤害磨损 1 点耐久；耐久耗尽护符碎裂消失，溢出的伤害将穿透护盾。"
                    + "护符不可修复、不可回充。";
            }
        }

        /// <summary>无使用门槛（任何人可佩戴）。</summary>
        protected override string GetActivateDisabledReason(Pawn pawn)
        {
            return null;
        }

        /// <summary>护符耐久不可恢复，破盾后永不自动重启。</summary>
        protected override bool CanAutoReset()
        {
            return false;
        }

        /// <summary>护符碎裂音效（1.7 的 SoundDefOf 无 EnergyShield_Broken 字段，按名惰性查表）。</summary>
        private static SoundDef brokenSound;

        private static SoundDef BrokenSound
        {
            get
            {
                if (brokenSound == null)
                    brokenSound = DefDatabase<SoundDef>.GetNamedSilentFail("EnergyShield_Broken");
                return brokenSound;
            }
        }

        /// <summary>
        /// 伤害结算：耐久充足则全额吸收并磨损等额耐久；不足则吸收剩余耐久等值伤害
        /// （余量改写 dinfo 穿透）并使护符碎裂。恒返回 true，不存在"破盾但物品保留"的中间态。
        /// 注意：碎裂销毁必须放在本方法内（基类随后才播吸收特效，已对 PawnOwner 为 null 做保护）。
        /// </summary>
        protected override bool ConsumeResource(DamageInfo dinfo)
        {
            int hp = parent.HitPoints;
            float amount = dinfo.Amount;
            if (amount < hp)
            {
                // 节点流转：全额吸收，扣等额耐久
                parent.HitPoints = hp - (int)amount;
                return true;
            }
            // 流程结束：耐久不足 -> 吸收 hp 等值伤害，剩余穿透，护符碎裂
            Pawn owner = PawnOwner; // 销毁前捕获穿戴者，用于碎裂反馈
            dinfo.SetAmount(amount - hp);
            parent.TakeDamage(new DamageInfo(DamageDefOf.Deterioration, hp));
            if (owner != null && owner.Map != null)
            {
                if (BrokenSound != null)
                    BrokenSound.PlayOneShot(new TargetInfo(owner.Position, owner.Map));
                Messages.Message("玉质护符在吸收 " + hp + " 点伤害后碎裂了", new LookTargets(owner),
                    MessageTypeDefOf.NegativeEvent, false);
            }
            return true;
        }
    }
}
