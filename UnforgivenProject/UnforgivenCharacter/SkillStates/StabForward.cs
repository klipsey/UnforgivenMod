using UnityEngine;
using EntityStates;
using UnforgivenMod.Modules.BaseStates;
using RoR2;
using UnityEngine.AddressableAssets;
using UnforgivenMod.Unforgiven.Content;
using static R2API.DamageAPI;
using UnityEngine.Networking;
using R2API.Networking;
using UnforgivenMod.Unforgiven.Components;
using R2API.Networking.Interfaces;

namespace UnforgivenMod.Unforgiven.SkillStates
{
    public class StabForward : BaseMeleeAttack
    {
        private bool hasGrantedStacks;

        protected GameObject swingEffectInstance;
        public override void OnEnter()
        {
            RefreshState();
            hitboxGroupName = "SteelTempestHitbox";

            damageType = empoweredSpecial ? DamageType.BypassArmor : DamageType.Generic;
            damageSource = DamageSource.Secondary;
            damageCoefficient = UnforgivenStaticValues.stabDamageCoefficient;
            procCoefficient = 1f;
            pushForce = 300f;
            bonusForce = Vector3.zero;
            baseDuration = 1.1f;

            //0-1 multiplier of baseduration, used to time when the hitbox is out (usually based on the run time of the animation)
            //for example, if attackStartPercentTime is 0.5, the attack will start hitting halfway through the ability. if baseduration is 3 seconds, the attack will start happening at 1.5 seconds
            attackStartPercentTime = 0.2f;
            attackEndPercentTime = 0.4f;

            //this is the point at which the attack can be interrupted by itself, continuing a combo
            earlyExitPercentTime = 0.5f;

            hitStopDuration = 0.05f;
            attackRecoil = 2f / attackSpeedStat;
            hitHopVelocity = 5f;

            swingSoundString = "sfx_unforgiven_stab";
            hitSoundString = "";
            playbackRateParam = "Slash.playbackRate";
            swingEffectPrefab = UnforgivenAssets.stabSwingEffect;
            hitEffectPrefab = UnforgivenAssets.unforgivenHitEffect;

            impactSound = UnforgivenAssets.stabImpactSoundEvent.index;

            muzzleString = "StabMuzzle";

            base.OnEnter();
        }

        protected override void OnHitEnemyAuthority()
        {
            base.OnHitEnemyAuthority();
            if (!hasGrantedStacks)
            {
                hasGrantedStacks = true;

                NetworkIdentity identity = base.gameObject.GetComponent<NetworkIdentity>();
                if (!identity) return;

                new SyncStacks(identity.netId, empowered).Send(NetworkDestination.Server);
            }
        }

        protected override void FireAttack()
        {
            if (base.isAuthority)
            {
                Vector3 direction = this.GetAimRay().direction;
                direction.y = Mathf.Max(direction.y, direction.y * 0.5f);
                this.FindModelChild("MeleePivot").rotation = Util.QuaternionSafeLookRotation(direction);
            }

            base.FireAttack();
        }

        public override void Update()
        {
            base.Update();

            if (base.isAuthority && base.GetComponent<UnforgivenTracker>().GetTrackingTarget() && inputBank.skill3.down)
            {
                if(!hasFired) FireAttack();
                outer.SetNextStateToMain();
                return;
            }
        }

        protected override void PlaySwingEffect()
        {
            Util.PlaySound(this.swingSoundString, this.gameObject);
            if (this.swingEffectPrefab)
            {
                Transform muzzleTransform = this.FindModelChild(this.muzzleString);
                if (muzzleTransform)
                {
                    this.swingEffectInstance = Object.Instantiate<GameObject>(this.swingEffectPrefab, muzzleTransform);
                }
            }
        }

        protected override void PlayAttackAnimation()
        {
            if (!this.unforgivenController.isUnsheathed)
            {
                this.unforgivenController.Unsheath();
                PlayCrossfade("Gesture, Override", "DrawStab", playbackRateParam, duration * 1.3f, duration * 0.05f);
            }
            else PlayCrossfade("Gesture, Override", "Stab", playbackRateParam, duration * 1.3f, duration * 0.05f);
        }

        public override void OnExit()
        {
            base.OnExit();

            if(this.swingEffectInstance) EntityState.Destroy(this.swingEffectInstance);
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