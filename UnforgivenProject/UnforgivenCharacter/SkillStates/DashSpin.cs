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
        private bool hasGrantedStacks;

        private bool activateNado;
        public override void OnEnter()
        {
            RefreshState();
            hitboxGroupName = "SteelTempestSpinHitbox";

            damageType = empoweredSpecial ? DamageType.BypassArmor : DamageType.Generic;
            damageCoefficient = empowered ? UnforgivenStaticValues.tornadoDamageCoefficient : UnforgivenStaticValues.stabDamageCoefficient;
            procCoefficient = 1f;
            pushForce = 300f;
            bonusForce = empowered ? Vector3.up * 3000f : Vector3.zero;
            baseDuration = 1.1f;
            //0-1 multiplier of baseduration, used to time when the hitbox is out (usually based on the run time of the animation)
            //for example, if attackStartPercentTime is 0.5, the attack will start hitting halfway through the ability. if baseduration is 3 seconds, the attack will start happening at 1.5 seconds
            attackStartPercentTime = 0.05f;
            attackEndPercentTime = 0.4f;
    
            //this is the point at which the attack can be interrupted by itself, continuing a combo
            earlyExitPercentTime = 0.5f;

            hitStopDuration = 0.05f;
            attackRecoil = 2f / attackSpeedStat;
            hitHopVelocity = 9f;

            swingSoundString = EntityStates.Merc.Weapon.GroundLight2.slash1Sound;
            hitSoundString = "sfx_unforgiven_stab";
            playbackRateParam = "Slash.playbackRate";
            muzzleString = "SpinMuzzle";
            swingEffectPrefab = empowered ? UnforgivenAssets.spinNadoEffect : (empoweredSpecial ? UnforgivenAssets.spinEmpoweredSlashEffect : UnforgivenAssets.spinSlashEffect);
            hitEffectPrefab = UnforgivenAssets.unforgivenHitEffect;

            if (empowered)
            {
                if (NetworkServer.active) base.characterBody.ClearTimedBuffs(UnforgivenBuffs.stabMaxStacksBuff);

                activateNado = true;

                moddedDamageTypeHolder.Add(DamageTypes.KnockAirborne);
            }
            impactSound = empowered ? UnforgivenAssets.nadoImpactSoundEvent.index : UnforgivenAssets.swordImpactSoundEvent.index;

            base.OnEnter();

            characterBody.isSprinting = true;
        }

        protected override void PlayAttackAnimation()
        {
            this.unforgivenController.Unsheath();
            base.PlayCrossfade("FullBody, Override", "DashSpin", 0.05f);
        }

        public override void FixedUpdate()
        {
            characterBody.isSprinting = true;

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
        public override void OnExit()
        {
            base.OnExit();

            unforgivenController.bufferedSpin = false;
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            if (stopwatch >= duration * earlyExitPercentTime)
            {
                return InterruptPriority.Any;
            }
            return InterruptPriority.PrioritySkill;
        }
    }
}
