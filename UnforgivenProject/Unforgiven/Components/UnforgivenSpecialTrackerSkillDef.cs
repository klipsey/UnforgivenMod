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
            public UnforgivenTracker tracker;        
        }

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
            bool target = false;
            if (tracker)
            {
                HurtBox hurtBox = tracker.GetTrackingTarget();
                if (hurtBox && hurtBox.healthComponent && hurtBox.healthComponent.body && hurtBox.healthComponent.body.characterMotor)
                {
                    if (hurtBox.healthComponent.body.HasBuff(UnforgivenBuffs.airborneBuff) || !hurtBox.healthComponent.body.characterMotor.isGrounded || hurtBox.healthComponent.body.characterMotor.isFlying)
                    {
                        target = true;
                    }
                }
                else if(hurtBox && hurtBox.healthComponent && hurtBox.healthComponent.body && !hurtBox.healthComponent.body.characterMotor)
                {
                    target = true;
                }
                else
                {
                    HurtBox[] hurtBoxes = new SphereSearch
                    {
                        origin = tracker.gameObject.transform.position,
                        radius = 40f,
                        mask = LayerIndex.entityPrecise.mask
                    }.RefreshCandidates().FilterCandidatesByHurtBoxTeam(TeamMask.GetEnemyTeams(tracker.gameObject.GetComponent<CharacterBody>().teamComponent.teamIndex)).OrderCandidatesByDistance()
                    .FilterCandidatesByDistinctHurtBoxEntities().GetHurtBoxes();
                    foreach(HurtBox hurtBox2 in hurtBoxes)
                    {
                        if (hurtBox2 && hurtBox2.healthComponent && hurtBox2.healthComponent.body && hurtBox2.healthComponent.body.characterMotor)
                        {
                            if (hurtBox2.healthComponent.body.HasBuff(UnforgivenBuffs.airborneBuff) || !hurtBox2.healthComponent.body.characterMotor.isGrounded || hurtBox2.healthComponent.body.characterMotor.isFlying)
                            {
                                target = true;
                            }
                        }
                        else if (hurtBox2 && hurtBox2.healthComponent && hurtBox2.healthComponent.body && !hurtBox2.healthComponent.body.characterMotor)
                        {
                            target = true;
                        }
                    }
                }
            }
            return target;
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
