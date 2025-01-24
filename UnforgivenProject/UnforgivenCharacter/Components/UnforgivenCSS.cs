using UnityEngine;
using RoR2;
using UnforgivenMod.Unforgiven.Content;

namespace UnforgivenMod.Unforgiven.Components
{
    public class UnforgivenCSS : MonoBehaviour
    {
        private bool hasPlayed = false;
        private bool hasPlayed2 = false;
        private float timer = 0f;
        private void Awake()
        {
        }
        private void FixedUpdate()
        {
            timer += Time.fixedDeltaTime;
            if (!hasPlayed)
            {
                hasPlayed = true;
                Util.PlaySound("sfx_unforgiven_max_stacks", base.gameObject);
                EffectManager.SimpleMuzzleFlash(UnforgivenAssets.spinNadoEffect, base.gameObject, "SpinMuzzle", false);
            }
        }
    }
}
