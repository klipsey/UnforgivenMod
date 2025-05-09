using EntityStates;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;
using UnityEngine;
using UnforgivenMod.Modules.BaseStates;
using UnforgivenMod.Unforgiven.Components;
using UnforgivenMod.Unforgiven.Content;
using UnityEngine.AddressableAssets;
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

        private float maxRange = 12f;

        private float roundDuration = 0.3f;

        private float roundStopwatch;

        private int numRounds = 3;

        private int roundsCompleted = 0;

        protected GameObject swingEffectInstance;

        private GameObject swingEffectPrefab = UnforgivenAssets.swordSwingEffect;

        private GameObject swingEffectPrefab2 = UnforgivenAssets.swordSwingEmpoweredEffect;

        private GameObject slashEffectPrefab = UnforgivenAssets.specialSlashingEffect;

        private GameObject slashEffectPrefab2 = UnforgivenAssets.specialEmpoweredSlashingEffect;

        private CameraTargetParams.AimRequest aimRequest;

        public override void OnEnter()
        {
            if(NetworkServer.active) base.characterBody.AddBuff(UnforgivenBuffs.lastBreathBuff);

            RefreshState();

            base.OnEnter();

            if (base.cameraTargetParams)
            {
                aimRequest = base.cameraTargetParams.RequestAimType(CameraTargetParams.AimType.Aura);
            }

            characterBody.isSprinting = true;

            if (NetworkServer.active)
            {
                base.characterBody.AddBuff(RoR2Content.Buffs.HiddenInvincibility);
            }

            base.characterBody.SetAimTimer(1.5f);

            this.roundsCompleted = 0;
            this.roundStopwatch = this.roundDuration;
            float num = (this.attackSpeedStat - 1f) * 0.5f;
            this.roundDuration /= num + 1f;
            this.crit = base.RollCrit();

            this.unforgivenController.Unsheath();
            if(!unforgivenController.bufferedSpin || !isBuffered) base.PlayCrossfade("FullBody, Override", "Special", "Slash.playbackRate", this.numRounds * this.roundDuration, this.numRounds * this.roundDuration * 0.05f);
            Transform modelTransform = characterBody.modelLocator.modelTransform;
            if(modelTransform)
            {
                Animator animator = modelTransform.GetComponent<Animator>();
                TemporaryOverlayInstance temporaryOverlayInstance = TemporaryOverlayManager.AddOverlay(base.gameObject);
                temporaryOverlayInstance.duration = 1.2f;
                temporaryOverlayInstance.destroyComponentOnEnd = true;
                temporaryOverlayInstance.originalMaterial = UnforgivenAssets.specialMaterial;
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
                base.characterBody.RemoveBuff(UnforgivenBuffs.lastBreathBuff);
                base.characterBody.AddTimedBuff(UnforgivenBuffs.lastBreathBuff, 6f);
            }

            base.OnExit();

            aimRequest?.Dispose();

            if (this.swingEffectInstance) EntityState.Destroy(this.swingEffectInstance);

            unforgivenController.bufferedSpin = false;

            isBuffered = false;

        }

        private void Fire()
        {
            bool final = this.roundsCompleted == this.numRounds - 1;

            float damage = final ? finalDamageCoefficient : damageCoefficient;

            Util.PlaySound(final ? EntityStates.Merc.Weapon.GroundLight2.slash3Sound : EntityStates.Merc.Weapon.GroundLight2.slash1Sound, base.gameObject);

            BlastAttack blastAttack = new BlastAttack
            {
                attacker = base.gameObject,
                procChainMask = default(ProcChainMask),
                losType = BlastAttack.LoSType.NearestHit,
                damageColorIndex = DamageColorIndex.Default,
                damageType = empoweredSpecial ? DamageType.BypassArmor : DamageType.Stun1s,
                procCoefficient = 1f,
                bonusForce = Vector3.down * 3000f,
                baseForce = 300f,
                baseDamage = damage * this.damageStat,
                falloffModel = BlastAttack.FalloffModel.None,
                radius = this.maxRange,
                position = base.transform.position,
                attackerFiltering = AttackerFiltering.NeverHitSelf,
                teamIndex = base.GetTeam(),
                inflictor = base.gameObject,
                crit = this.crit
            };

            blastAttack.damageType.damageSource = DamageSource.Special;

            if (skillLocator.special.skillDef == UnforgivenSurvivor.firstBreath)
            {
                blastAttack.AddModdedDamageType(DamageTypes.ResetDashes);

                if(skillLocator.secondary.stock != skillLocator.secondary.maxStock)
                {
                    skillLocator.secondary.Reset();
                }
            }

            blastAttack.Fire();

            if(this.slashEffectPrefab && this.slashEffectPrefab2)
            {
                EffectManager.SpawnEffect(final ? slashEffectPrefab2 : slashEffectPrefab, new EffectData
                {
                    origin = base.transform.position,
                }, transmit: true);
            }

            Transform muzzleTransform = this.roundsCompleted % 2 == 0 ? this.FindModelChild("SwingMuzzle2") : this.FindModelChild("SwingMuzzle1");
            if (muzzleTransform && this.swingEffectPrefab2 && this.swingEffectPrefab)
            {
                if (this.swingEffectInstance) EntityState.Destroy(swingEffectInstance);
                this.swingEffectInstance = UnityEngine.Object.Instantiate<GameObject>(final ? this.swingEffectPrefab2 : this.swingEffectPrefab, muzzleTransform);
            }

            Util.PlayAttackSpeedSound(final ? EntityStates.Merc.Weapon.GroundLight2.slash3Sound : EntityStates.Merc.Weapon.GroundLight2.slash1Sound, base.gameObject, attackSpeedStat);

            if (NetworkServer.active)
            {
                Vector3 direction = Vector3.down;
                if (!final) direction = UnityEngine.Random.insideUnitSphere;
                direction.y = Mathf.Max(0f, direction.y);
                direction = direction.normalized;
                List<HealthComponent> hits = new List<HealthComponent>();
                Collider[] hit = Physics.OverlapSphere(base.transform.position, this.maxRange, LayerIndex.entityPrecise.mask, QueryTriggerInteraction.UseGlobal);
                for (int i = 0; i < hit.Length; i++)
                {
                    HurtBox hurtBox = hit[i].GetComponent<HurtBox>();
                    if (hurtBox)
                    {
                        HealthComponent healthComponent = hurtBox.healthComponent;
                        if (healthComponent)
                        {
                            TeamComponent team = healthComponent.GetComponent<TeamComponent>();
                            bool enemy = team.teamIndex != base.teamComponent.teamIndex;
                            if (enemy)
                            {
                                if (!hits.Contains(healthComponent))
                                {
                                    hits.Add(healthComponent);
                                    if (healthComponent.body)
                                    {
                                        if (healthComponent.body.characterMotor) healthComponent.body.characterMotor.velocity = direction * 12f;
                                        else if (healthComponent.body.rigidbody) healthComponent.body.rigidbody.velocity = direction * 12f;

                                        healthComponent.body.AddTimedBuff(UnforgivenBuffs.specialSlamTrackerBuff, 2f, 1);
                                    }
                                }
                            }
                        }
                    }

                }
            }
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();

            characterBody.isSprinting = true;

            base.characterMotor.velocity = Vector3.zero;

            this.roundStopwatch += Time.fixedDeltaTime;
            bool flag3 = this.roundStopwatch >= this.roundDuration;
            if (flag3)
            {
                this.Fire();
                this.roundsCompleted++;
                this.roundStopwatch = 0f;
            }
            bool flag4 = this.roundsCompleted >= this.numRounds;
            if (flag4)
            {
                this.outer.SetNextStateToMain();
            }
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Death;
        }
    }
}
