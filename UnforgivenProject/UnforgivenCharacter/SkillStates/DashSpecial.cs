using EntityStates;
using RoR2;
using UnforgivenMod.Modules.BaseStates;
using UnforgivenMod.Unforgiven.Components;
using UnforgivenMod.Unforgiven.Content;
using UnityEngine;
using UnityEngine.Networking;

namespace UnforgivenMod.Unforgiven.SkillStates
{
    public class DashSpecial : BaseUnforgivenSkillState
    {
        private const float baseDuration = 0.167f;
        private HurtBox target;
        private CharacterBody victimBody;
        private float duration;
        private bool hasStartedDash;
        private bool holdBuffer;
        private CameraTargetParams.AimRequest aimRequest;

        public override void OnEnter()
        {
            base.OnEnter();

            if (isAuthority)
            {
                target = FindTarget();
                holdBuffer = unforgivenController.bufferedSpin;
            }

            if (!target || !target.healthComponent || !target.healthComponent.alive || !target.healthComponent.body)
            {
                Cancel();
                return;
            }

            victimBody = target.healthComponent.body;
            hasStartedDash = true;
            duration = baseDuration / attackSpeedStat;

            if (cameraTargetParams)
            {
                aimRequest = cameraTargetParams.RequestAimType(CameraTargetParams.AimType.Aura);
            }

            if (NetworkServer.active)
            {
                characterBody.AddBuff(RoR2Content.Buffs.HiddenInvincibility);
                characterBody.AddBuff(UnforgivenBuffs.specialSuspensionBuff);
                victimBody.AddBuff(UnforgivenBuffs.specialSuspensionBuff);
            }

            characterMotor.Motor.ForceUnground();
            characterMotor.rootMotion = Vector3.zero;
            characterMotor.velocity = Vector3.zero;
            unforgivenController.Unsheath();
            PlayCrossfade("FullBody, Override", "Dash", 0.05f);
            Util.PlaySound("Play_merc_shift_slice", gameObject);
        }

        private HurtBox FindTarget()
        {
            Ray aimRay = inputBank.GetAimRay();
            Vector3 aimPosition = inputBank.GetAimRaycast(60f, out RaycastHit hit)
                ? hit.point : aimRay.GetPoint(60f);
            HurtBox bestTarget = null;
            float bestDistance = float.PositiveInfinity;
            HurtBox[] candidates = new SphereSearch
            {
                origin = characterBody.corePosition,
                radius = 60f,
                mask = LayerIndex.entityPrecise.mask
            }.RefreshCandidates()
                .FilterCandidatesByHurtBoxTeam(TeamMask.GetEnemyTeams(GetTeam()))
                .FilterCandidatesByDistinctHurtBoxEntities()
                .GetHurtBoxes();

            foreach (HurtBox candidate in candidates)
            {
                if (UnforgivenSpecialTrackerSkillDef.TryGetEligibleBody(candidate, out _) &&
                    (candidate.transform.position - aimPosition).sqrMagnitude < bestDistance)
                {
                    bestTarget = candidate;
                    bestDistance = (candidate.transform.position - aimPosition).sqrMagnitude;
                }
            }
            return bestTarget;
        }

        private bool TryGetArrivalPosition(out Vector3 position)
        {
            Vector3 targetPosition = target.transform.position;
            Vector3 approach = Vector3.ProjectOnPlane(characterBody.corePosition - targetPosition, Vector3.up).normalized;
            if (approach == Vector3.zero)
            {
                approach = -characterDirection.forward;
            }

            CapsuleCollider capsule = characterMotor.Motor.Capsule;
            Vector3 scale = transform.lossyScale;
            float radius = capsule.radius * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.z));
            float halfHeight = Mathf.Max(0f, capsule.height * Mathf.Abs(scale.y) * 0.5f - radius);
            Vector3 coreOffset = characterBody.corePosition - transform.position;
            Collider targetCollider = target.GetComponent<Collider>();
            float targetRadius = targetCollider ? targetCollider.bounds.extents.magnitude : victimBody.radius;
            for (int height = 0; height < 2; height++)
            {
                for (int i = 0; i < 8; i++)
                {
                    Vector3 direction = Quaternion.AngleAxis(i * 45f, Vector3.up) * approach;
                    Vector3 surface = targetCollider
                        ? targetCollider.ClosestPoint(targetPosition + direction * (targetRadius + 1f))
                        : targetPosition + direction * targetRadius;
                    position = surface + direction * (radius + 0.5f) - coreOffset +
                        Vector3.up * (height * (halfHeight + radius));
                    Vector3 capsuleCenter = position + transform.TransformVector(capsule.center);
                    if (!Physics.CheckCapsule(capsuleCenter + Vector3.up * halfHeight,
                        capsuleCenter - Vector3.up * halfHeight, radius,
                        LayerIndex.world.mask, QueryTriggerInteraction.Ignore))
                    {
                        return true;
                    }
                }
            }

            position = transform.position;
            return false;
        }

        private void Cancel()
        {
            if (isAuthority)
            {
                skillLocator.special.AddOneStock();
                outer.SetNextStateToMain();
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (!hasStartedDash || !isAuthority || fixedAge < duration)
            {
                return;
            }

            if (!target || !target.healthComponent || !target.healthComponent.alive)
            {
                Cancel();
                return;
            }

            if (!TryGetArrivalPosition(out Vector3 arrivalPosition))
            {
                Log.Warning("Last Breath could not find an unobstructed position next to its target; refunding the skill.");
                Cancel();
                return;
            }

            Vector3 arrivalCorePosition = arrivalPosition + characterBody.corePosition - transform.position;
            TeleportHelper.TeleportGameObject(gameObject, arrivalPosition);
            characterMotor.velocity = Vector3.zero;
            characterDirection.forward = Vector3.ProjectOnPlane(target.transform.position - arrivalCorePosition, Vector3.up).normalized;
            outer.SetNextState(new Special
            {
                target = target,
                arrivalPosition = arrivalPosition,
                isBuffered = holdBuffer,
                activatorSkillSlot = activatorSkillSlot
            });
        }

        public override void OnExit()
        {
            aimRequest?.Dispose();
            if (NetworkServer.active && hasStartedDash)
            {
                characterBody.RemoveBuff(RoR2Content.Buffs.HiddenInvincibility);
                characterBody.RemoveBuff(UnforgivenBuffs.specialSuspensionBuff);
                if (victimBody)
                {
                    victimBody.RemoveBuff(UnforgivenBuffs.specialSuspensionBuff);
                }
            }
            base.OnExit();
        }

        public override void OnSerialize(NetworkWriter writer)
        {
            base.OnSerialize(writer);
            writer.Write(HurtBoxReference.FromHurtBox(target));
            writer.Write(holdBuffer);
        }

        public override void OnDeserialize(NetworkReader reader)
        {
            base.OnDeserialize(reader);
            target = reader.ReadHurtBoxReference().ResolveHurtBox();
            holdBuffer = reader.ReadBoolean();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
