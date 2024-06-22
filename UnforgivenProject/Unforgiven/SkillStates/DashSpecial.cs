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
    public class DashSpecial : BaseUnforgivenSkillState
    {
        public int targetIndex = 0;
        public CharacterBody target;
        private Transform modelTransform;

        private UnforgivenTracker tracker;

        private HurtBoxGroup hurtboxGroup;

        private Vector3 lastKnownPosition;
        private Vector3 direction;
        private float distance;
        private float duration;
        private float speed;

        private float stopwatch;

        public static float stopTrackTime = 0.8f;
        private float baseDuration = 0.1f;
        public static float extraDuration = 0.1f;
        public static float extraDistance = 3.25f;
        public static float exitExtraDistance = 3.25f;

        public static float baseChainPrepDuration = 0.067f;
        public static float basePrepDuration = 0.067f;
        private float prepDuration;
        private float prepStopwatch;

        public override void OnEnter()
        {
            base.OnEnter();

            if (base.characterBody && NetworkServer.active) base.characterBody.bodyFlags |= CharacterBody.BodyFlags.IgnoreFallDamage;

            this.tracker = base.GetComponent<UnforgivenTracker>();
            if (this.tracker)
            {
                HurtBox h = this.tracker.GetTrackingTarget();
                if (h && h.healthComponent && h.healthComponent.body) this.target = this.tracker.GetTrackingTarget().healthComponent.body;
            }

            base.characterMotor.Motor.ForceUnground();

            this.distance = (base.transform.position - this.target.coreTransform.position).magnitude + 4f;
            this.direction = (this.target.coreTransform.position - base.transform.position).normalized;
            this.duration = this.baseDuration / this.attackSpeedStat;
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
            }
            base.OnExit();
        }

        private bool bufferedSecondary;
        private void ReadInputs()
        {
            if (this.inputBank.skill2.down || this.inputBank.skill2.wasDown) this.bufferedSecondary = true;
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();

            this.ReadInputs();
            if (this.prepStopwatch >= this.prepDuration)
            {
                this.stopwatch += Time.fixedDeltaTime;

                Vector3 target;
                if (this.target)
                {
                    target = this.target.coreTransform.position;
                    this.lastKnownPosition = target;
                }
                else target = this.lastKnownPosition;

                if (this.stopwatch < this.duration * 0.5f) this.direction = (target - base.transform.position).normalized;

                if (this.stopwatch >= this.duration && extraDuration != 0) this.speed = extraDistance / extraDuration;

                base.characterDirection.forward = this.direction;
                base.characterMotor.rootMotion += this.direction * this.speed * Time.fixedDeltaTime;
                base.characterMotor.velocity = Vector3.zero;

                base.gameObject.layer = LayerIndex.fakeActor.intVal;
                base.characterMotor.Motor.RebuildCollidableLayers();

                if (this.stopwatch >= this.duration)
                {
                    if(bufferedSecondary)
                    {
                        this.outer.SetNextState(new DashSpin
                        {
                            buffered = bufferedSecondary
                        });
                    }
                    else this.outer.SetNextState(new Special());
                    return;
                }

                if (this.stopwatch >= this.duration + extraDuration)
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
