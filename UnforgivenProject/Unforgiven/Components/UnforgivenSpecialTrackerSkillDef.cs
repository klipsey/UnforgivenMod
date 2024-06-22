using JetBrains.Annotations;
using UnityEngine;
using RoR2.Skills;
using RoR2;
using UnforgivenMod.Unforgiven.Content;

namespace UnforgivenMod.Unforgiven.Components
{
    public class UnforgivenSpecialTrackerSkillDef : SkillDef
    {
        protected class InstanceData : BaseSkillInstanceData
        {
            public UnforgivenTracker tracker;        }

        public override BaseSkillInstanceData OnAssigned([NotNull] GenericSkill skillSlot)
        {
            return new InstanceData
            {
                tracker = skillSlot.GetComponent<UnforgivenTracker>(),            
            };
        }

        private static bool HasTarget([NotNull] GenericSkill skillSlot)
        {
            UnforgivenTracker tracker = ((UnforgivenSpecialTrackerSkillDef.InstanceData)skillSlot.skillInstanceData).tracker;
            if (tracker)
            {
                HurtBox hurtBox = tracker.GetTrackingTarget();
                if (hurtBox && hurtBox.healthComponent && hurtBox.healthComponent.body)
                {
                    if (hurtBox.healthComponent.body.HasBuff(UnforgivenBuffs.airborneBuff) || (hurtBox.healthComponent.body.characterMotor && !hurtBox.healthComponent.body.characterMotor.isGrounded)) return true;
                }

            }
            return false;
        }

        public override bool CanExecute([NotNull] GenericSkill skillSlot)
        {
            if (!HasTarget(skillSlot))
            {
                return false;
            }
            return base.CanExecute(skillSlot);
        }

        public override bool IsReady([NotNull] GenericSkill skillSlot)
        {
            if (base.IsReady(skillSlot))
            {
                return HasTarget(skillSlot);
            }
            return false;
        }
    }
}
