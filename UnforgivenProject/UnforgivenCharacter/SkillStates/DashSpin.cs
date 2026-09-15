using EntityStates;
using RoR2;
using UnforgivenMod.Unforgiven.Content;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnforgivenMod.Modules.BaseStates;
using R2API.Networking;
using UnforgivenMod.Unforgiven.Components;
using R2API.Networking.Interfaces;

namespace UnforgivenMod.Unforgiven.SkillStates
{
    public class DashSpin : BaseMeleeAttack
    {
        public bool empoweredSpin;

        private bool hasGrantedStacks;

        private bool activateNado;
        private EntityStateMachine dashStateMachine;
        private bool waitingForDash;

        public bool IsWaitingForDash => !hasFired && dashStateMachine &&
            (dashStateMachine.state is Dash || dashStateMachine.state is DashSpecial || dashStateMachine.HasPendingState());

        public override void OnEnter()
        {
            RefreshState();
            dashStateMachine = EntityStateMachine.FindByCustomName(base.gameObject, "Dash");
            if (!dashStateMachine)
            {
                Log.Error("DashSpin could not find the required Dash state machine; the spin cannot wait for a dash.");
            }
            hitboxGroupName = "SteelTempestSpinHitbox";

            damageType = empoweredSpecial ? DamageType.BypassArmor : DamageType.Generic;
            damageSource = DamageSource.Secondary;
            damageCoefficient = empoweredSpin ? UnforgivenConfig.tornadoDamageCoefficient.Value : UnforgivenConfig.stabDamageCoefficient.Value;
            procCoefficient = 1f;
            pushForce = 300f;
            bonusForce = empoweredSpin ? Vector3.up * 3000f : Vector3.zero;
            baseDuration = 1.1f;
            attackStartPercentTime = 0f;
            attackEndPercentTime = 0.4f;
    
            //this is the point at which the attack can be interrupted by itself, continuing a combo
            earlyExitPercentTime = 0.5f;

            hitStopDuration = 0.05f;
            attackRecoil = 2f / attackSpeedStat;
            hitHopVelocity = 8f;

            swingSoundString = EntityStates.Merc.Weapon.GroundLight2.slash1Sound;
            hitSoundString = "sfx_unforgiven_stab";
            playbackRateParam = "Slash.playbackRate";
            muzzleString = "SpinMuzzle";
            swingEffectPrefab = empoweredSpin ? UnforgivenAssets.spinNadoEffect : (empoweredSpecial ? UnforgivenAssets.spinEmpoweredSlashEffect : UnforgivenAssets.spinSlashEffect);
            hitEffectPrefab = UnforgivenAssets.unforgivenHitEffect;

            if (empoweredSpin)
            {
                activateNado = true;

                moddedDamageTypeHolder.Add(DamageTypes.KnockAirborne);
            }
            impactSound = empoweredSpin ? UnforgivenAssets.nadoImpactSoundEvent.index : UnforgivenAssets.swordImpactSoundEvent.index;

            waitingForDash = IsWaitingForDash;
            base.OnEnter();

            characterBody.isSprinting = true;
        }

        protected override void PlayAttackAnimation()
        {
            if (IsWaitingForDash)
            {
                return;
            }

            this.unforgivenController.Unsheath();
            base.PlayCrossfade("FullBody, Override", "DashSpin", 0.05f);
        }

        public override void FixedUpdate()
        {
            characterBody.isSprinting = true;

            if (IsWaitingForDash)
            {
                waitingForDash = true;
                return;
            }

            if (waitingForDash)
            {
                waitingForDash = false;
                PlayAttackAnimation();
            }

            hitPauseTimer -= Time.fixedDeltaTime;

            if (hitPauseTimer <= 0f && inHitPause)
            {
                RemoveHitstop();
            }

            if (!inHitPause)
            {
                stopwatch += Time.fixedDeltaTime;
            }
            else
            {
                if (characterMotor) characterMotor.velocity = Vector3.zero;
                if (animator) animator.SetFloat(playbackRateParam, 0f);
            }

            bool fireStarted = stopwatch >= duration * attackStartPercentTime;
            bool fireEnded = stopwatch >= duration * attackEndPercentTime;

            //to guarantee attack comes out if at high attack speed the stopwatch skips past the firing duration between frames
            if (fireStarted && !fireEnded || fireStarted && fireEnded && !hasFired)
            {
                if (!hasFired)
                {
                    if (empoweredSpin && NetworkServer.active)
                    {
                        base.characterBody.ClearTimedBuffs(UnforgivenBuffs.stabMaxStacksBuff);
                    }
                    EnterAttack();
                }
                FireAttack();
            }

            if(base.isAuthority && stopwatch >= duration)
            {
                outer.SetNextStateToMain();
            }
        }
        protected override void OnHitEnemyAuthority()
        {
            base.OnHitEnemyAuthority();

            if (!hasGrantedStacks)
            {
                hasGrantedStacks = true;
                NetworkIdentity identity = base.gameObject.GetComponent<NetworkIdentity>();
                if (!identity) return;

                new SyncStacks(identity.netId, activateNado).Send(NetworkDestination.Server);
            }
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            if (stopwatch >= duration * earlyExitPercentTime)
            {
                return InterruptPriority.Any;
            }
            return InterruptPriority.PrioritySkill;
        }

        public override void OnSerialize(NetworkWriter writer)
        {
            base.OnSerialize(writer);
            writer.Write(empoweredSpin);
        }

        public override void OnDeserialize(NetworkReader reader)
        {
            base.OnDeserialize(reader);
            empoweredSpin = reader.ReadBoolean();
        }
    }
}
