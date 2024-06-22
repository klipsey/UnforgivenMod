using EntityStates;
using RoR2;
using UnforgivenMod.Unforgiven.Content;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnforgivenMod.Modules.BaseStates;

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

            if (empowered) moddedDamageTypeHolder.Add(DamageTypes.KnockAirborne);

            impactSound = empowered ? UnforgivenAssets.nadoImpactSoundEvent.index : UnforgivenAssets.swordImpactSoundEvent.index;

            base.OnEnter();
            
            if(NetworkServer.active)
            {
                stacks = base.characterBody.GetBuffCount(UnforgivenBuffs.stabStackingBuff);
                base.characterBody.ClearTimedBuffs(UnforgivenBuffs.stabStackingBuff);
            }
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
            if (NetworkServer.active && !hasGrantedStacks)
            {
                hasGrantedStacks = true;
                if (stacks == 2)
                {
                    base.characterBody.AddTimedBuff(UnforgivenBuffs.stabMaxStacksBuff, 8f, 1);
                }
                else if(!base.characterBody.HasBuff(UnforgivenBuffs.stabMaxStacksBuff))
                {
                    for (int i = 0; i < stacks + 1; i++)
                    {
                        base.characterBody.AddTimedBuff(UnforgivenBuffs.stabStackingBuff, 6f, 2);
                    }
                }
                else if(empowered)
                {
                    base.characterBody.ClearTimedBuffs(UnforgivenBuffs.stabMaxStacksBuff);
                }
                if (base.characterBody.HasBuff(UnforgivenBuffs.stabMaxStacksBuff) && !hasPlayedSound)
                {
                    hasPlayedSound = true;
                    Util.PlaySound("sfx_unforgiven_max_stacks", base.gameObject);
                }
            }
        }
        public override void OnExit()
        {
            base.OnExit();
        }
    }
}
