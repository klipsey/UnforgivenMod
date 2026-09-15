using RoR2.Skills;
using RoR2;
using UnityEngine;
using System;
using JetBrains.Annotations;
using UnforgivenMod.Unforgiven.Content;
using UnforgivenMod.Unforgiven.SkillStates;

namespace UnforgivenMod.Unforgiven.Components
{
    public class ScaleCDwAttackSpeed : SkillDef
    {
        public override Sprite GetCurrentIcon([NotNull] GenericSkill skillSlot)
        {
            return skillSlot.characterBody.HasBuff(UnforgivenBuffs.stabMaxStacksBuff)
                ? UnforgivenAssets.secondaryEmpoweredIcon
                : base.GetCurrentIcon(skillSlot);
        }

        public override void OnFixedUpdate([NotNull] GenericSkill skillSlot, float deltaTime)
        {
            if (beginSkillCooldownOnSkillEnd && skillSlot.stateMachine &&
                skillSlot.stateMachine.state is DashSpin spin && spin.IsWaitingForDash)
            {
                deltaTime = 0f;
            }
            base.OnFixedUpdate(skillSlot, deltaTime);
        }
        public override float GetRechargeInterval([NotNull] GenericSkill skillSlot)
        {
            skillSlot.finalRechargeInterval = Mathf.Min(baseRechargeInterval, Mathf.Max(0.5f, baseRechargeInterval / skillSlot.characterBody.attackSpeed));
            return Mathf.Min(baseRechargeInterval, Mathf.Max(0.5f, baseRechargeInterval / skillSlot.characterBody.attackSpeed));
        }
    }
}
