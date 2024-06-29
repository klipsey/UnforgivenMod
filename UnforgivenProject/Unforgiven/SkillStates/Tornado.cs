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
        public override void OnEnter()
        {
            RefreshState();
            base.OnEnter();

            this.duration = baseDuration / this.attackSpeedStat;
            base.characterBody.SetAimTimer(2f);

            if (!this.unforgivenController.isUnsheathed)
            {
                this.unforgivenController.Unsheath();
                base.PlayAnimation("FullBody, Override", "DrawSlash", "Slash.playbackRate", this.duration);
            }
            else base.PlayAnimation("FullBody, Override", "Slash2", "Slash.playbackRate", this.duration);

            if (NetworkServer.active) base.characterBody.ClearTimedBuffs(UnforgivenBuffs.stabMaxStacksBuff);

            this.Fire();
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        private void Fire()
        {
            Util.PlaySound("sfx_unforgiven_throw_nado", base.gameObject);
            DamageType lol = empoweredSpecial ? DamageType.BypassArmor : DamageType.Generic;

            if (base.isAuthority)
            {
                Ray aimRay = base.GetAimRay();

                ProjectileManager.instance.FireProjectile(new FireProjectileInfo
                {
                    projectilePrefab = nado,
                    position = aimRay.origin,
                    rotation = Util.QuaternionSafeLookRotation(aimRay.direction),
                    owner = base.gameObject,
                    damage = damageCoefficient * this.damageStat,
                    force = 800f,
                    crit = base.RollCrit(),
                    damageColorIndex = DamageColorIndex.Default,
                    target = null,
                    speedOverride = throwForce,
                    useSpeedOverride = false,
                    damageTypeOverride = lol
                });

            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if(base.isAuthority && base.fixedAge >= duration)
            {
                this.outer.SetNextStateToMain();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}