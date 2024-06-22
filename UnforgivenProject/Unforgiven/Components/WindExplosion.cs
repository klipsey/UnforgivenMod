using UnityEngine;
using RoR2;

namespace UnforgivenMod.Unforgiven.Components
{
    public class WindExplosion : MonoBehaviour
    {
        private void Awake()
        {
            if (this.transform)
            {
                EffectManager.SpawnEffect(Content.UnforgivenAssets.windExplosionEffect, new EffectData
                {
                    origin = this.transform.position,
                    rotation = Quaternion.identity,
                    scale = 1f
                }, false);

                Util.PlaySound("sfx_unforgiven_nado_impact", base.gameObject);
            }
        }

        private void LateUpdate()
        {
            if (this.transform) this.transform.localScale = Vector3.zero;
        }
    }
}