using UnforgivenMod.Modules.BaseStates;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;
using UnityEngine;
using UnforgivenMod.Unforgiven.Content;
using R2API;
using EntityStates;
using UnforgivenMod.Unforgiven.Components;

namespace UnforgivenMod.Unforgiven.SkillStates
{
    public class Dash : BaseUnforgivenSkillState
    {
        public int targetIndex = 0;
        public CharacterBody target;
        private Transform modelTransform;

        private UnforgivenTracker tracker;

        private HurtBoxGroup hurtboxGroup;

        private Vector3 direction;
        private float distance;
        private float duration;
        private float speed;
        private bool hasFired;

        private float stopwatch;
        private float damageCoefficient = UnforgivenStaticValues.dashDamageCoefficient;

        public static float hitRange = 4.5f;
        private float baseDuration = 0.2f;
        public static float baseExtraDuration = 0.1f;
        private float extraDuration;
        private float minDistance = 7f;
        public static float extraDistance = 5f;

        public static float baseChainPrepDuration = 0.067f;
        public static float basePrepDuration = 0.067f;
        private float prepDuration;
        private float prepStopwatch;

        private bool bufferedSecondary;

        public override void OnEnter()
        {
            RefreshState();
            base.OnEnter();

            if (base.characterBody && NetworkServer.active)
            {
                base.characterBody.bodyFlags |= CharacterBody.BodyFlags.IgnoreFallDamage;
                damageCoefficient = UnforgivenStaticValues.dashDamageCoefficient + base.characterBody.GetBuffCount(UnforgivenBuffs.stackingDashDamageBuff) * 0.75f;
            }

            this.tracker = base.GetComponent<UnforgivenTracker>();
            if (this.tracker)
            {
                HurtBox hurtbox = this.tracker.GetTrackingTarget();
                if (hurtbox && hurtbox.healthComponent && hurtbox.healthComponent.body) this.target = this.tracker.GetTrackingTarget().healthComponent.body;
            }

            if (!this.target)
            {
                this.outer.SetNextStateToMain();
                this.activatorSkillSlot.AddOneStock();
                return;
            }

            this.target.AddTimedBuff(UnforgivenBuffs.dashCooldownBuff, 6f);
            base.characterMotor.Motor.ForceUnground();

            Vector3 corePosition = Util.GetCorePosition(target);
            this.distance = Mathf.Max(this.minDistance, (base.transform.position - corePosition).magnitude);
            this.direction = (corePosition - base.transform.position).normalized;
            this.duration = this.baseDuration / this.attackSpeedStat;
            this.extraDuration = Dash.baseExtraDuration / this.attackSpeedStat;
            this.speed = this.distance / this.duration;

            this.prepDuration = Dash.baseChainPrepDuration / this.attackSpeedStat;

            this.modelTransform = base.GetModelTransform();
            if (this.modelTransform)
            {
                this.hurtboxGroup = this.modelTransform.GetComponent<HurtBoxGroup>();
            }
            if (this.hurtboxGroup)
            {
                HurtBoxGroup hurtBoxGroup = this.hurtboxGroup;
                int hurtBoxesDeactivatorCounter = hurtBoxGroup.hurtBoxesDeactivatorCounter + 1;
                hurtBoxGroup.hurtBoxesDeactivatorCounter = hurtBoxesDeactivatorCounter;
            }

            base.gameObject.layer = LayerIndex.fakeActor.intVal;
            base.characterMotor.Motor.RebuildCollidableLayers();

            base.PlayCrossfade("FullBody, Override", "Dash", 0.1f);
            Util.PlaySound("Play_merc_shift_slice", base.gameObject);
        }

        public override void OnExit()
        {
            base.gameObject.layer = LayerIndex.defaultLayer.intVal;
            base.characterMotor.Motor.RebuildCollidableLayers();
            base.characterMotor.velocity = Vector3.zero;

            if (this.hurtboxGroup)
            {
                HurtBoxGroup hurtBoxGroup = this.hurtboxGroup;
                int hurtBoxesDeactivatorCounter = hurtBoxGroup.hurtBoxesDeactivatorCounter - 1;
                hurtBoxGroup.hurtBoxesDeactivatorCounter = hurtBoxesDeactivatorCounter;
            }
            if (NetworkServer.active)
            {
                base.characterBody.bodyFlags &= ~CharacterBody.BodyFlags.IgnoreFallDamage;
                base.characterBody.AddTimedBuff(UnforgivenBuffs.stackingDashDamageBuff, 6f, 4);
            }
            base.OnExit();
        }

        private void Fire()
        {
            if (!this.hasFired)
            {
                this.hasFired = true;
                if (base.isAuthority)
                {
                    DamageInfo damageInfo = new DamageInfo
                    {
                        position = this.target.transform.position,
                        attacker = base.gameObject,
                        inflictor = base.gameObject,
                        damage = this.damageCoefficient * base.damageStat,
                        damageColorIndex = DamageColorIndex.Default,
                        damageType = empoweredSpecial ? DamageType.BypassArmor : DamageType.Generic,
                        crit = false,
                        force = Vector3.zero,
                        procChainMask = default(ProcChainMask),
                        procCoefficient = 1f
                    };

                    this.target.healthComponent.TakeDamage(damageInfo);
                    GlobalEventManager.instance.OnHitEnemy(damageInfo, this.target.gameObject);
                    GlobalEventManager.instance.OnHitAll(damageInfo, this.target.gameObject);

                    EffectManager.SpawnEffect(UnforgivenAssets.unforgivenHitEffect, new EffectData
                    {
                        origin = this.target.transform.position,
                        rotation = Quaternion.identity,
                        networkSoundEventIndex = UnforgivenAssets.swordImpactSoundEvent.index
                    }, true);
                }
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (this.inputBank.skill2.wasDown || this.inputBank.skill2.down) this.bufferedSecondary = true;

            if (base.isGrounded)
            {
                base.characterMotor.Motor.ForceUnground();
            }

            if (this.prepStopwatch >= this.prepDuration)
            {
                this.stopwatch += Time.fixedDeltaTime;

                if (this.stopwatch >= this.duration && extraDuration != 0) this.speed = extraDistance / this.extraDuration;

                base.characterDirection.forward = this.direction;
                base.characterMotor.rootMotion += this.direction * this.speed * Time.fixedDeltaTime;
                base.characterMotor.velocity = Vector3.zero;

                if (this.target)
                {
                    float dis = (Util.GetCorePosition(this.target) - base.transform.position).magnitude;
                    if (dis <= hitRange)
                    {
                        this.Fire();
                    }
                }

                if (base.isAuthority && this.stopwatch >= this.duration)
                {
                    if (this.skillLocator.secondary.CanExecute() && this.bufferedSecondary)
                    {
                        this.skillLocator.secondary.ExecuteIfReady();
                    }
                }

                if (base.isAuthority && this.stopwatch >= this.duration + baseExtraDuration)
                {
                    this.outer.SetNextStateToMain();
                }

            }
            else this.prepStopwatch += Time.fixedDeltaTime;

        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }

    }
}
