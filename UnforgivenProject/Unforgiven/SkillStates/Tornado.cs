using UnityEngine;
using EntityStates;
using UnforgivenMod.Modules.BaseStates;
using RoR2;
using UnityEngine.AddressableAssets;
using UnforgivenMod.Unforgiven.Content;
using static R2API.DamageAPI;
using RoR2.Projectile;
using R2API;
using UnityEngine.Networking;

namespace UnforgivenMod.Unforgiven.SkillStates
{
    public class Tornado : BaseUnforgivenSkillState
    {
        public static float damageCoefficient = UnforgivenStaticValues.tornadoDamageCoefficient;
        public static float procCoefficient = 1f;
        public static float baseDuration = 0.65f;
        public static float throwForce = 80f;

        public GameObject nado = UnforgivenAssets.nadoPrefab;

        private float duration;
        private float fireTime;
        private bool hasFired;
        private Animator animator;
        public override void OnEnter()
        {
            RefreshState();
            base.OnEnter();

            this.duration = baseDuration / this.attackSpeedStat;
            this.fireTime = 0.25f * this.duration;
            base.characterBody.SetAimTimer(2f);
            this.animator = base.GetModelAnimator();

            base.PlayAnimation("FullBody, Override", "ThrowTornado", "Slash.playbackRate", this.duration);

            if (NetworkServer.active) base.characterBody.ClearTimedBuffs(UnforgivenBuffs.stabMaxStacksBuff);
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        private void Fire()
        {
            if (!this.hasFired)
            {
                this.hasFired = true;
                Util.PlaySound("sfx_unforgiven_throw_nado", base.gameObject);
                nado.GetComponent<DamageAPI.ModdedDamageTypeHolderComponent>().Add(DamageTypes.KnockAirborne);
                nado.GetComponent<ProjectileDamage>().damageType = empoweredSpecial ? DamageType.BypassArmor : DamageType.Generic;

                if (base.isAuthority)
                {
                    Ray aimRay = base.GetAimRay();
                    ProjectileManager.instance.FireProjectile(nado,
                        aimRay.origin,
                        Util.QuaternionSafeLookRotation(aimRay.direction),
                        base.gameObject,
                        damageCoefficient * this.damageStat,
                        800f,
                        base.RollCrit(),
                        DamageColorIndex.Default,
                        null,
                        throwForce);
                }
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.fixedAge >= this.fireTime)
            {
                this.Fire();
            }

            if (base.fixedAge >= this.duration && base.isAuthority)
            {
                this.outer.SetNextStateToMain();
                return;
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}