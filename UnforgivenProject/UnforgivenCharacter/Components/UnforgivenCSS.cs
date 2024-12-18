﻿using UnityEngine;
using RoR2;

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
            if (!hasPlayed && timer >= 0.4f)
            {
                hasPlayed = true;
                Util.PlaySound("sfx_driver_gun_throw", this.gameObject);
            }

            if (!hasPlayed2 && timer >= 0.8f)
            {
                hasPlayed2 = true;
                Util.PlaySound("sfx_driver_button_foley", this.gameObject);
            }
        }
    }
}
