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
            skillSlot.finalRechargeInterval = this.baseRechargeInterval / (2f * skillSlot.characterBody.attackSpeed - 1f);
            return this.baseRechargeInterval / (2f * skillSlot.characterBody.attackSpeed - 1f);
        }
    }
}
