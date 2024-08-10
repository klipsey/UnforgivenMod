using RoR2.Skills;
using RoR2;
using UnityEngine;
using System;
using JetBrains.Annotations;

namespace UnforgivenMod.Unforgiven.Components
{
    public class ScaleCDwAttackSpeed : SkillDef
    {
        public override void OnExecute([NotNull] GenericSkill skillSlot)
        {
            base.OnExecute(skillSlot);
        }
        public override float GetRechargeInterval([NotNull] GenericSkill skillSlot)
        {
            skillSlot.finalRechargeInterval = Mathf.Min(baseRechargeInterval, Mathf.Max(0.5f, baseRechargeInterval / skillSlot.characterBody.attackSpeed));
            return Mathf.Min(baseRechargeInterval, Mathf.Max(0.5f, baseRechargeInterval / skillSlot.characterBody.attackSpeed));
        }
    }
}
