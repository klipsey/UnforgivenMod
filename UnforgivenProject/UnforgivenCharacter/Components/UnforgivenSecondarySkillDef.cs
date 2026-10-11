using RoR2.Skills;
using RoR2;
using UnityEngine;
using JetBrains.Annotations;
using UnforgivenMod.Unforgiven.Content;
using UnforgivenMod.Unforgiven.SkillStates;

namespace UnforgivenMod.Unforgiven.Components
{
    public class UnforgivenSecondarySkillDef : SkillDef
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
    }
}
