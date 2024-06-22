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
        private bool hasPlayedSound;

        private int stacks;

        private bool hasGrantedStacks;

        public override void OnEnter()
        {
            RefreshState();
            hitboxGroupName = "SteelTempestHitbox";

            damageType = empoweredSpecial ? DamageType.BypassArmor : DamageType.Generic;
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
            playbackRateParam = "Swing.playbackRate";
            swingEffectPrefab = UnforgivenAssets.swordSwingEffect;
            hitEffectPrefab = UnforgivenAssets.unforgivenHitEffect;

            impactSound = UnforgivenAssets.stabImpactSoundEvent.index;

            switch (swingIndex)
            {
                case 0:
                    muzzleString = "SwingMuzzle1";
                    break;
                case 1:
                    muzzleString = "SwingMuzzle2";
                    break;
                case 2:
                    muzzleString = "SwingMuzzle";
                    swingSoundString = EntityStates.Merc.Weapon.GroundLight2.slash3Sound;
                    break;
            }

            base.OnEnter();

            if (NetworkServer.active)
            {
                stacks = base.characterBody.GetBuffCount(UnforgivenBuffs.stabStackingBuff);
                base.characterBody.ClearTimedBuffs(UnforgivenBuffs.stabStackingBuff);
            }
        }

        protected override void OnHitEnemyAuthority()
        {
            base.OnHitEnemyAuthority();
            if (NetworkServer.active && !hasGrantedStacks)
            {
                hasGrantedStacks = true;
                if (stacks == 2)
                {
                    base.characterBody.AddTimedBuff(UnforgivenBuffs.stabMaxStacksBuff, 8f, 1);
                }
                else
                {
                    for (int i = 0; i < stacks + 1; i++)
                    {
                        base.characterBody.AddTimedBuff(UnforgivenBuffs.stabStackingBuff, 6f, 2);
                    }
                }
                if (base.characterBody.HasBuff(UnforgivenBuffs.stabMaxStacksBuff) && !hasPlayedSound)
                {
                    hasPlayedSound = true;
                    Util.PlaySound("sfx_unforgiven_max_stacks", base.gameObject);
                }
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

        protected override void PlaySwingEffect()
        {
            Util.PlaySound(this.swingSoundString, this.gameObject);
            if (this.swingEffectPrefab)
            {
                Transform muzzleTransform = this.FindModelChild(this.muzzleString);
                if (muzzleTransform)
                {
                    this.swingEffectPrefab = Object.Instantiate<GameObject>(this.swingEffectPrefab, muzzleTransform);
                }
            }
        }

        protected override void PlayAttackAnimation()
        {
            PlayCrossfade("Gesture, Override", "Swing" + (1 + swingIndex), playbackRateParam, duration * 1.3f, duration * 0.3f);
        }

        public override void OnExit()
        {
            base.OnExit();
        }
    }
}