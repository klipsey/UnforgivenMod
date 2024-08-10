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
        public static float baseDuration = 0.1f;

        public static float baseExtraDuration = 0.05f;

        public static float extraDistance = 2.5f;

        public static float hitRange = 4.5f;

        public int targetIndex = 0;

        public CharacterBody victimBody;

        private Transform modelTransform;

        private HurtBox hurtbox;

        private UnforgivenTracker tracker;
        private int maxStacks => UnforgivenStaticValues.baseMaxDashStacks + (base.skillLocator.utility.maxStock - 1);
        private Vector3 direction;
        private float distance;
        private float duration;
        private float extraDuration;
        private float speed;
        private bool hasFired;
        private float damageCoefficient = UnforgivenStaticValues.dashDamageCoefficient;
        private float minDistance = 7f;

        public override void OnEnter()
        {
            RefreshState();
            base.OnEnter();

            if (skillLocator.secondary.rechargeStopwatch >= skillLocator.secondary.finalRechargeInterval - 0.5f)
            {
                skillLocator.secondary.rechargeStopwatch = skillLocator.secondary.finalRechargeInterval;
            }

            if (base.characterBody && NetworkServer.active)
            {
                base.characterBody.bodyFlags |= CharacterBody.BodyFlags.IgnoreFallDamage;
                damageCoefficient = UnforgivenStaticValues.dashDamageCoefficient + base.characterBody.GetBuffCount(UnforgivenBuffs.stackingDashDamageBuff) * UnforgivenStaticValues.dashStackingDamageCoefficient;
            }

            this.tracker = base.GetComponent<UnforgivenTracker>();
            if (this.tracker && base.isAuthority)
            {
                hurtbox = this.tracker.GetTrackingTarget();
            }

            if (hurtbox && hurtbox.healthComponent && hurtbox.healthComponent.body) this.victimBody = hurtbox.healthComponent.body;

            if (!this.victimBody)
            {
                this.outer.SetNextStateToMain();
                this.activatorSkillSlot.AddOneStock();
                return;
            }

            if (NetworkServer.active)
            {
                this.victimBody.AddTimedBuff(UnforgivenBuffs.dashCooldownBuff, 6f);
                this.characterBody.AddBuff(RoR2Content.Buffs.HiddenInvincibility);
            }

            base.characterMotor.Motor.ForceUnground();

            Vector3 corePosition = Util.GetCorePosition(victimBody);
            this.distance = Mathf.Max(this.minDistance, (base.transform.position - corePosition).magnitude);
            this.direction = (corePosition - base.transform.position).normalized;
            this.duration = Dash.baseDuration / this.attackSpeedStat;
            this.extraDuration = Dash.baseExtraDuration / this.attackSpeedStat;
            this.speed = this.distance / this.duration;

            base.gameObject.layer = LayerIndex.fakeActor.intVal;
            base.characterMotor.Motor.RebuildCollidableLayers();

            this.unforgivenController.Unsheath();

            base.PlayCrossfade("FullBody, Override", "Dash", 0.1f);
            Util.PlaySound("Play_merc_shift_slice", base.gameObject);


            if (base.isGrounded)
            {
                base.characterMotor.Motor.ForceUnground();
            }
        }

        public override void OnExit()
        {
            base.gameObject.layer = LayerIndex.defaultLayer.intVal;
            base.characterMotor.Motor.RebuildCollidableLayers();
            base.characterMotor.velocity = Vector3.zero;

            if (NetworkServer.active)
            {
                this.characterBody.RemoveBuff(RoR2Content.Buffs.HiddenInvincibility);
                base.characterBody.bodyFlags &= ~CharacterBody.BodyFlags.IgnoreFallDamage;

                int stacks = base.characterBody.GetBuffCount(UnforgivenBuffs.stackingDashDamageBuff);
                if (stacks >= maxStacks) stacks = maxStacks - 1;

                base.characterBody.ClearTimedBuffs(UnforgivenBuffs.stackingDashDamageBuff);

                for (int i = 0; i < stacks + 1; i++)
                {
                    characterBody.AddTimedBuff(UnforgivenBuffs.stackingDashDamageBuff, 6f, maxStacks);
                }
            }
            base.OnExit();
        }

        private void Fire()
        {
            if (!this.hasFired)
            {
                this.hasFired = true;
                if (NetworkServer.active)
                {
                    DamageInfo damageInfo = new DamageInfo
                    {
                        position = this.victimBody.transform.position,
                        attacker = base.gameObject,
                        inflictor = base.gameObject,
                        damage = this.damageCoefficient * base.damageStat,
                        damageColorIndex = DamageColorIndex.Default,
                        damageType = empoweredSpecial ? DamageType.BypassArmor : DamageType.Generic,
                        crit = RollCrit(),
                        force = Vector3.zero,
                        procChainMask = default(ProcChainMask),
                        procCoefficient = 1f
                    };

                    this.victimBody.healthComponent.TakeDamage(damageInfo);
                    GlobalEventManager.instance.OnHitEnemy(damageInfo, this.victimBody.gameObject);
                    GlobalEventManager.instance.OnHitAll(damageInfo, this.victimBody.gameObject);

                    EffectManager.SpawnEffect(UnforgivenAssets.unforgivenHitEffect, new EffectData
                    {
                        origin = this.victimBody.transform.position,
                        rotation = Quaternion.identity,
                        networkSoundEventIndex = UnforgivenAssets.swordImpactSoundEvent.index
                    }, true);
                }
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.fixedAge >= this.duration && extraDuration != 0) this.speed = extraDistance / this.extraDuration;

            base.characterDirection.forward = this.direction;
            base.characterMotor.rootMotion += this.direction * this.speed * Time.fixedDeltaTime;
            base.characterMotor.velocity = Vector3.zero;

            if (this.victimBody)
            {
                this.Fire();
            }

            if (base.isAuthority && base.fixedAge >= this.duration + extraDuration)
            {
                this.outer.SetNextStateToMain();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }

        public override void OnSerialize(NetworkWriter writer)
        {
            writer.Write(HurtBoxReference.FromHurtBox(hurtbox));
        }
        public override void OnDeserialize(NetworkReader reader)
        {
            hurtbox = reader.ReadHurtBoxReference().ResolveHurtBox();
        }
    }
}
