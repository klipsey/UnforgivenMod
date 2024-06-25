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

        private HurtBoxGroup hurtboxGroup;

        private Vector3 lastKnownPosition;
        private Vector3 direction;
        private float distance;
        private float duration;
        private float speed;

        private float stopwatch;

        public static float stopTrackTime = 0.8f;
        private float baseDuration = 0.1f;
        public float extraDuration;
        public static float extraDistance = 3.25f;
        public static float exitExtraDistance = 3.25f;

        public static float baseChainPrepDuration = 0.067f;
        public static float basePrepDuration = 0.067f;
        private float prepDuration;
        private float prepStopwatch;
        private bool bufferedSecondary = false;

        public override void OnEnter()
        {
            RefreshState();
            base.OnEnter();

            RaycastHit hitInfo;
            Vector3 position = !inputBank.GetAimRaycast(60f, out hitInfo) ? Vector3.MoveTowards(inputBank.GetAimRay().GetPoint(60f), transform.position, 5f) : Vector3.MoveTowards(hitInfo.point, transform.position, 5f);
            position.y += 2.5f;

            HurtBox[] hurtBoxes = new SphereSearch
            {
                origin = position,
                radius = 30f,
                mask = LayerIndex.entityPrecise.mask
            }.RefreshCandidates().FilterCandidatesByHurtBoxTeam(TeamMask.GetEnemyTeams(this.teamComponent.teamIndex)).OrderCandidatesByDistance()
                .FilterCandidatesByDistinctHurtBoxEntities().GetHurtBoxes();

            if(hurtBoxes.Length > 0 )
            {
                foreach (HurtBox hurtBox in hurtBoxes)
                {
                    if (hurtBox && hurtBox.healthComponent && hurtBox.healthComponent.body && hurtBox.healthComponent.body.characterMotor)
                    {
                        if (hurtBox.healthComponent.body.HasBuff(UnforgivenBuffs.airborneBuff) || !hurtBox.healthComponent.body.characterMotor.isGrounded || hurtBox.healthComponent.body.characterMotor.isFlying)
                        {
                            this.target = hurtBox.healthComponent.body;
                            break;
                        }
                    }
                    else if (hurtBox && hurtBox.healthComponent && hurtBox.healthComponent.body && !hurtBox.healthComponent.body.characterMotor)
                    {
                        this.target = hurtBox.healthComponent.body;
                        break;
                    }
                }
            }

            if(!target)
            {
                HurtBox[] hurtBoxes2 = new SphereSearch
                {
                    origin = base.transform.position,
                    radius = 60f,
                    mask = LayerIndex.entityPrecise.mask
                }.RefreshCandidates().FilterCandidatesByHurtBoxTeam(TeamMask.GetEnemyTeams(this.teamComponent.teamIndex)).OrderCandidatesByDistance()
                .FilterCandidatesByDistinctHurtBoxEntities().GetHurtBoxes();
                foreach (HurtBox hurtBox in hurtBoxes2)
                {
                    if (hurtBox && hurtBox.healthComponent && hurtBox.healthComponent.body && hurtBox.healthComponent.body.characterMotor)
                    {
                        if (hurtBox.healthComponent.body.HasBuff(UnforgivenBuffs.airborneBuff) || !hurtBox.healthComponent.body.characterMotor.isGrounded || hurtBox.healthComponent.body.characterMotor.isFlying)
                        {
                            this.target = hurtBox.healthComponent.body;
                            break;
                        }
                    }
                    else if (hurtBox && hurtBox.healthComponent && hurtBox.healthComponent.body && !hurtBox.healthComponent.body.characterMotor)
                    {
                        this.target = hurtBox.healthComponent.body;
                        break;
                    }
                }
            }


            if(!this.target)
            {
                skillLocator.special.AddOneStock();
                outer.SetNextStateToMain();
                return;
            }

            if (base.characterBody && NetworkServer.active) base.characterBody.bodyFlags |= CharacterBody.BodyFlags.IgnoreFallDamage;

            base.characterMotor.Motor.ForceUnground();

            this.distance = (base.transform.position - this.target.coreTransform.position).magnitude + 4f;
            this.direction = (this.target.coreTransform.position - base.transform.position).normalized;
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

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (this.inputBank.skill2.wasDown || this.inputBank.skill2.down) this.bufferedSecondary = true;


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

                if (base.isAuthority && this.stopwatch >= this.duration)
                {
                    if(this.skillLocator.secondary.CanExecute() && this.bufferedSecondary)
                    {
                        unforgivenController.bufferedSpin = true;
                        this.skillLocator.secondary.ExecuteIfReady();
                    }
                }

                if (this.stopwatch >= this.duration + Dash.baseExtraDuration)
                {
                    this.outer.SetNextState(new Special());
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
