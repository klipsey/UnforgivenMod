using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using RoR2;
using UnityEngine.Networking;
using UnforgivenMod.Unforgiven.Content;

namespace UnforgivenMod.Unforgiven.Components
{
    public class AirborneComponent : MonoBehaviour
    {
        public CharacterBody body;

        public void Awake()
        {
            body = base.GetComponent<CharacterBody>();
        }

        public void Start()
        {
        }

        public void FixedUpdate()
        {
            if (body && !body.HasBuff(UnforgivenBuffs.airborneBuff))
            {
                if (body.characterMotor)
                {
                    if (!body.characterMotor.isGrounded)
                    {
                        if(NetworkServer.active) body.AddBuff(UnforgivenBuffs.airborneBuff);
                    }
                }
            }

            if (body.HasBuff(UnforgivenBuffs.airborneBuff) && body)
            {
                if (body.characterMotor)
                {
                    if (body.characterMotor.isGrounded)
                    {
                        if(NetworkServer.active) body.RemoveBuff(UnforgivenBuffs.airborneBuff);
                        Component.Destroy(this);
                    }
                }
            }
        }
    }
}
