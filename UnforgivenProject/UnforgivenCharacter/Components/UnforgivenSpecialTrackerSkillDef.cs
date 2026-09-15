using JetBrains.Annotations;
using System;
using System.Collections.Generic;
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
            public readonly SphereSearch search = new SphereSearch
            {
                radius = 60f,
                mask = LayerIndex.entityPrecise.mask
            };
            public readonly List<HurtBox> hurtBoxes = new List<HurtBox>();
            public readonly HashSet<CharacterBody> uniqueTargets = new HashSet<CharacterBody>();
            public int searchFrame = -1;
            public float searchFixedTime = -1f;
            public Vector3 searchOrigin;
            public TeamIndex searchTeam;
        }

        public override BaseSkillInstanceData OnAssigned([NotNull] GenericSkill skillSlot)
        {
            return new InstanceData
            {
                body = skillSlot.gameObject.GetComponent<CharacterBody>(),
            };
        }

        private static InstanceData GetSearchData([NotNull] GenericSkill skillSlot)
        {
            InstanceData instanceData = (InstanceData)skillSlot.skillInstanceData;
            CharacterBody body = instanceData.body;
            if (!body)
            {
                instanceData.hurtBoxes.Clear();
                return instanceData;
            }

            Vector3 origin = body.corePosition;
            TeamIndex team = body.teamComponent.teamIndex;
            if (instanceData.searchFrame != Time.frameCount || instanceData.searchFixedTime != Time.fixedTime ||
                instanceData.searchOrigin != origin || instanceData.searchTeam != team)
            {
                instanceData.hurtBoxes.Clear();
                instanceData.search.origin = origin;
                instanceData.search.RefreshCandidates()
                    .FilterCandidatesByHurtBoxTeam(TeamMask.GetEnemyTeams(team))
                    .GetHurtBoxes(instanceData.hurtBoxes);
                instanceData.searchFrame = Time.frameCount;
                instanceData.searchFixedTime = Time.fixedTime;
                instanceData.searchOrigin = origin;
                instanceData.searchTeam = team;
            }
            return instanceData;
        }

        public static bool TryGetEligibleBody(HurtBox hurtBox, out CharacterBody targetBody)
        {
            targetBody = null;
            if (!hurtBox || !hurtBox.healthComponent || !hurtBox.healthComponent.alive || !hurtBox.healthComponent.body)
            {
                return false;
            }

            CharacterBody body = hurtBox.healthComponent.body;
            CharacterMotor motor = body.characterMotor;
            if (!motor || body.HasBuff(UnforgivenBuffs.airborneBuff) || !motor.isGrounded || motor.isFlying)
            {
                targetBody = body;
                return true;
            }
            return false;
        }

        private static bool HasTarget([NotNull] GenericSkill skillSlot)
        {
            foreach (HurtBox hurtBox in GetSearchData(skillSlot).hurtBoxes)
            {
                if (TryGetEligibleBody(hurtBox, out _))
                {
                    return true;
                }
            }
            return false;
        }

        public void GetEligibleTargets([NotNull] GenericSkill skillSlot, [NotNull] List<CharacterBody> targets)
        {
            if (targets == null)
            {
                throw new ArgumentNullException(nameof(targets));
            }

            targets.Clear();
            InstanceData instanceData = GetSearchData(skillSlot);
            instanceData.uniqueTargets.Clear();
            foreach (HurtBox hurtBox in instanceData.hurtBoxes)
            {
                if (TryGetEligibleBody(hurtBox, out CharacterBody targetBody) && instanceData.uniqueTargets.Add(targetBody))
                {
                    targets.Add(targetBody);
                }
            }
            instanceData.uniqueTargets.Clear();
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
