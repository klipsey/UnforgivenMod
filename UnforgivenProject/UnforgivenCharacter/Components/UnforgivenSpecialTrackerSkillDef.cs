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
            public CharacterBody body;        
        }

        public override BaseSkillInstanceData OnAssigned([NotNull] GenericSkill skillSlot)
        {
            return new InstanceData
            {
                body = skillSlot.gameObject.GetComponent<CharacterBody>(),            
            };
        }

        private static bool HasTarget([NotNull] GenericSkill skillSlot)
        {
            CharacterBody body = ((UnforgivenSpecialTrackerSkillDef.InstanceData)skillSlot.skillInstanceData).body;
            bool target = false;
            if (body)
            {
                HurtBox[] hurtBoxes = new SphereSearch
                {
                    origin = body.corePosition,
                    radius = 60f,
                    mask = LayerIndex.entityPrecise.mask
                }.RefreshCandidates().FilterCandidatesByHurtBoxTeam(TeamMask.GetEnemyTeams(body.teamComponent.teamIndex)).OrderCandidatesByDistance()
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
