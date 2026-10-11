using EntityStates;
using RoR2;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;
using UnforgivenMod.Modules.BaseStates;
using UnforgivenMod.Unforgiven.Content;
using R2API;

namespace UnforgivenMod.Unforgiven.SkillStates
{
    public class Special : BaseUnforgivenSkillState
    {
        public static float damageCoefficient = 2.5f;

        public static float finalDamageCoefficient = 8f;

        public static float procCoefficient = 1f;

        public bool crit;

        public bool isBuffered;
        public HurtBox target;
        public Vector3 arrivalPosition;
        private Vector3 attackPosition;

        private float maxRange = 12f;

        private float roundDuration = 0.3f;

        private float roundStopwatch;

        private int numRounds = 3;

        private int roundsCompleted = 0;

        private readonly HashSet<CharacterBody> suspendedTargets = new HashSet<CharacterBody>();

        protected GameObject swingEffectInstance;

        private GameObject swingEffectPrefab = UnforgivenAssets.swordSwingEffect;

        private GameObject swingEffectPrefab2 = UnforgivenAssets.swordSwingEmpoweredEffect;

        private GameObject slashEffectPrefab = UnforgivenAssets.specialSlashingEffect;

        private GameObject slashEffectPrefab2 = UnforgivenAssets.specialEmpoweredSlashingEffect;

        private CameraTargetParams.AimRequest aimRequest;

        public override void OnEnter()
        {
            base.OnEnter();
            swingEffectPrefab = UnforgivenSkinVFX.GetEffect(swingEffectPrefab, gameObject);
            swingEffectPrefab2 = UnforgivenSkinVFX.GetEffect(swingEffectPrefab2, gameObject);
            slashEffectPrefab = UnforgivenSkinVFX.GetEffect(slashEffectPrefab, gameObject);
            slashEffectPrefab2 = UnforgivenSkinVFX.GetEffect(slashEffectPrefab2, gameObject);

            if (!isAuthority)
            {
                TeleportHelper.OnTeleport(gameObject, arrivalPosition, arrivalPosition - transform.position);
            }
            characterBody.modelLocator.LateUpdate();
            attackPosition = target ? target.transform.position : characterBody.corePosition;
            if (isBuffered)
            {
                bool confirmed = unforgivenController.TryConfirmAirblade();
                if (isAuthority)
                {
                    isBuffered = confirmed;
                }
            }

            if (base.cameraTargetParams)
            {
                aimRequest = base.cameraTargetParams.RequestAimType(CameraTargetParams.AimType.Aura);
            }

            characterBody.isSprinting = true;

            if (NetworkServer.active)
            {
                base.characterBody.AddBuff(RoR2Content.Buffs.HiddenInvincibility);
                characterBody.AddBuff(UnforgivenBuffs.lastBreathBuff);
                characterBody.AddBuff(UnforgivenBuffs.specialSuspensionBuff);
                if (target && target.healthComponent)
                {
                    HoldTarget(target.healthComponent.body);
                }
            }
            RefreshState();

            base.characterBody.SetAimTimer(1.5f);

            this.roundsCompleted = 0;
            float num = (this.attackSpeedStat - 1f) * 0.5f;
            this.roundDuration /= num + 1f;
            this.roundStopwatch = this.roundDuration;
            this.crit = base.RollCrit();

            this.unforgivenController.Unsheath();
            if (!isBuffered) base.PlayCrossfade("FullBody, Override", "Special", "Slash.playbackRate", this.numRounds * this.roundDuration, this.numRounds * this.roundDuration * 0.05f);
            Transform modelTransform = characterBody.modelLocator.modelTransform;
            if(modelTransform)
            {
                Animator animator = modelTransform.GetComponent<Animator>();
                TemporaryOverlayInstance temporaryOverlayInstance = TemporaryOverlayManager.AddOverlay(base.gameObject);
                temporaryOverlayInstance.duration = 1.2f;
                temporaryOverlayInstance.destroyComponentOnEnd = true;
                temporaryOverlayInstance.originalMaterial = UnforgivenSkinVFX.IsMonsoon(gameObject)
                    ? UnforgivenSkinVFX.specialMaterial : UnforgivenAssets.specialMaterial;
                temporaryOverlayInstance.inspectorCharacterModel = animator.gameObject.GetComponent<CharacterModel>();
                temporaryOverlayInstance.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                temporaryOverlayInstance.animateShaderAlpha = true;
            }
        }

        public override void OnExit()
        {
            if (NetworkServer.active)
            {
                base.characterBody.RemoveBuff(RoR2Content.Buffs.HiddenInvincibility);
                characterBody.RemoveBuff(UnforgivenBuffs.specialSuspensionBuff);
                ReleaseTargets();
                base.characterBody.RemoveBuff(UnforgivenBuffs.lastBreathBuff);
                base.characterBody.AddTimedBuff(UnforgivenBuffs.lastBreathBuff, 6f);
            }

            base.OnExit();

            aimRequest?.Dispose();

            if (this.swingEffectInstance) EntityState.Destroy(this.swingEffectInstance);

            isBuffered = false;

        }

        private void Fire()
        {
            bool final = this.roundsCompleted == this.numRounds - 1;

            float damage = final ? finalDamageCoefficient : damageCoefficient;

            Util.PlaySound(final ? EntityStates.Merc.Weapon.GroundLight2.slash3Sound : EntityStates.Merc.Weapon.GroundLight2.slash1Sound, base.gameObject);

            bool resetsDashes = skillLocator.special.skillDef == UnforgivenSurvivor.firstBreath;
            if (base.isAuthority && resetsDashes && skillLocator.secondary.stock != skillLocator.secondary.maxStock)
            {
                skillLocator.secondary.Reset();
            }

            if (NetworkServer.active)
            {
                if (target)
                {
                    attackPosition = target.transform.position;
                }
                if (final)
                {
                    ReleaseTargets();
                }

                BlastAttack blastAttack = new BlastAttack
                {
                    attacker = base.gameObject,
                    procChainMask = default(ProcChainMask),
                    losType = BlastAttack.LoSType.NearestHit,
                    damageColorIndex = DamageColorIndex.Default,
                    damageType = DamageType.Stun1s | (empoweredSpecial ? DamageType.BypassArmor : DamageType.Generic),
                    procCoefficient = Special.procCoefficient,
                    bonusForce = final ? Vector3.down * 3000f : Vector3.zero,
                    baseForce = 0f,
                    baseDamage = damage * this.damageStat,
                    falloffModel = BlastAttack.FalloffModel.None,
                    radius = this.maxRange,
                    position = attackPosition,
                    attackerFiltering = AttackerFiltering.NeverHitSelf,
                    teamIndex = base.GetTeam(),
                    inflictor = base.gameObject,
                    crit = this.crit
                };

                blastAttack.damageType.damageSource = DamageSource.Special;

                if (resetsDashes)
                {
                    blastAttack.AddModdedDamageType(DamageTypes.ResetDashes);
                }

                BlastAttack.Result result = blastAttack.Fire();
                foreach (BlastAttack.HitPoint hit in result.hitPoints)
                {
                    CharacterBody body = hit.hurtBox && hit.hurtBox.healthComponent ? hit.hurtBox.healthComponent.body : null;
                    if (!body || !body.healthComponent.alive)
                    {
                        continue;
                    }
                    if (final)
                    {
                        body.AddTimedBuff(UnforgivenBuffs.specialSlamTrackerBuff, 2f, 1);
                    }
                    else
                    {
                        HoldTarget(body);
                    }
                }

                if(this.slashEffectPrefab && this.slashEffectPrefab2)
                {
                    EffectManager.SpawnEffect(final ? slashEffectPrefab2 : slashEffectPrefab, new EffectData
                    {
                        origin = attackPosition
                    }, transmit: true);
                }
            }

            Transform muzzleTransform = this.roundsCompleted % 2 == 0 ? this.FindModelChild("SwingMuzzle2") : this.FindModelChild("SwingMuzzle1");
            if (muzzleTransform && this.swingEffectPrefab2 && this.swingEffectPrefab)
            {
                if (this.swingEffectInstance) EntityState.Destroy(swingEffectInstance);
                this.swingEffectInstance = UnityEngine.Object.Instantiate<GameObject>(final ? this.swingEffectPrefab2 : this.swingEffectPrefab, muzzleTransform);
            }

            Util.PlayAttackSpeedSound(final ? EntityStates.Merc.Weapon.GroundLight2.slash3Sound : EntityStates.Merc.Weapon.GroundLight2.slash1Sound, base.gameObject, attackSpeedStat);
        }

        private void HoldTarget(CharacterBody body)
        {
            if (body && body.healthComponent && body.healthComponent.alive && suspendedTargets.Add(body))
            {
                body.AddBuff(UnforgivenBuffs.specialSuspensionBuff);
            }
        }

        private void ReleaseTargets()
        {
            foreach (CharacterBody body in suspendedTargets)
            {
                if (body)
                {
                    body.RemoveBuff(UnforgivenBuffs.specialSuspensionBuff);
                }
            }
            suspendedTargets.Clear();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            characterBody.isSprinting = true;

            base.characterMotor.velocity = Vector3.zero;

            this.roundStopwatch += Time.fixedDeltaTime;
            bool flag3 = this.roundStopwatch >= this.roundDuration;
            if (flag3 && this.roundsCompleted < this.numRounds)
            {
                this.Fire();
                this.roundsCompleted++;
                this.roundStopwatch = 0f;
            }
            bool flag4 = this.roundsCompleted >= this.numRounds;
            if (flag4 && base.isAuthority)
            {
                this.outer.SetNextStateToMain();
            }
        }

        public override void OnSerialize(NetworkWriter writer)
        {
            base.OnSerialize(writer);
            writer.Write(isBuffered);
            writer.Write(HurtBoxReference.FromHurtBox(target));
            writer.Write(arrivalPosition);
        }

        public override void OnDeserialize(NetworkReader reader)
        {
            base.OnDeserialize(reader);
            isBuffered = reader.ReadBoolean();
            target = reader.ReadHurtBoxReference().ResolveHurtBox();
            arrivalPosition = reader.ReadVector3();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Death;
        }
    }
}
