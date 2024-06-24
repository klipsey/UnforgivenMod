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
        private bool hasPlayedSound;

        public bool buffered;

        private int stacks;

        private bool hasGrantedStacks;
        public override void OnEnter()
        {
            RefreshState();
            hitboxGroupName = "SteelTempestSpinHitbox";

            damageType = empoweredSpecial ? DamageType.BypassArmor : DamageType.Generic;
            damageCoefficient = empowered ? UnforgivenStaticValues.tornadoDamageCoefficient : UnforgivenStaticValues.stabDamageCoefficient;
            procCoefficient = 1f;
            pushForce = 300f;
            bonusForce = empowered ? Vector3.up * 3000f : Vector3.zero;
            baseDuration = 0.67f;

            //0-1 multiplier of baseduration, used to time when the hitbox is out (usually based on the run time of the animation)
            //for example, if attackStartPercentTime is 0.5, the attack will start hitting halfway through the ability. if baseduration is 3 seconds, the attack will start happening at 1.5 seconds
            attackStartPercentTime = 0f;
            attackEndPercentTime = 0.2f;

            //this is the point at which the attack can be interrupted by itself, continuing a combo
            earlyExitPercentTime = 0.2f;

            hitStopDuration = 0.05f;
            attackRecoil = 2f / attackSpeedStat;
            hitHopVelocity = 9f;

            swingSoundString = EntityStates.Merc.Weapon.GroundLight2.slash1Sound;
            hitSoundString = "sfx_unforgiven_stab";
            playbackRateParam = "Swing.playbackRate";
            swingEffectPrefab = null;
            hitEffectPrefab = UnforgivenAssets.unforgivenHitEffect;

            if (empowered)
            {
                if (NetworkServer.active) base.characterBody.ClearTimedBuffs(UnforgivenBuffs.stabMaxStacksBuff);

                moddedDamageTypeHolder.Add(DamageTypes.KnockAirborne);
            }
            impactSound = empowered ? UnforgivenAssets.nadoImpactSoundEvent.index : UnforgivenAssets.swordImpactSoundEvent.index;

            base.OnEnter();
        }

        protected override void PlayAttackAnimation()
        {
            base.PlayCrossfade("FullBody, Override", "SpinDash", "Slash.playbackRate", this.duration, 0.05f);
        }

        public override void FixedUpdate()
        {
            if(buffered && stopwatch >= duration && base.isAuthority)
            {
                outer.SetNextState(new Special());
                return;
            }
            base.FixedUpdate();
        }
        protected override void OnHitEnemyAuthority()
        {
            base.OnHitEnemyAuthority();
            if (!hasGrantedStacks)
            {
                hasGrantedStacks = true;
                NetworkIdentity identity = base.gameObject.GetComponent<NetworkIdentity>();
                if (!identity) return;

                new SyncStacks(identity.netId).Send(NetworkDestination.Server);
            }
        }
        public override void OnExit()
        {
            base.OnExit();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            if (stopwatch >= duration * earlyExitPercentTime)
            {
                return InterruptPriority.Any;
            }
            return InterruptPriority.Skill;
        }
    }
}
