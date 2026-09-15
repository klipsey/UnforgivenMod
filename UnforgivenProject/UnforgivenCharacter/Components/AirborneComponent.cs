using UnityEngine;
using RoR2;
using UnityEngine.Networking;
using UnforgivenMod.Unforgiven.Content;

namespace UnforgivenMod.Unforgiven.Components
{
    [DisallowMultipleComponent]
    public class AirborneComponent : MonoBehaviour
    {
        public CharacterBody body;
        private bool hasBeenAirborne;

        public void Awake()
        {
            body = base.GetComponent<CharacterBody>();
        }

        public void FixedUpdate()
        {
            if (!NetworkServer.active)
            {
                return;
            }

            if (!body || !body.characterMotor)
            {
                Destroy(this);
                return;
            }

            bool hasAirborneBuff = body.HasBuff(UnforgivenBuffs.airborneBuff);
            if (!body.characterMotor.isGrounded)
            {
                hasBeenAirborne = true;
                if (!hasAirborneBuff)
                {
                    body.AddBuff(UnforgivenBuffs.airborneBuff);
                }
            }
            else if (hasBeenAirborne || hasAirborneBuff)
            {
                if (hasAirborneBuff)
                {
                    body.RemoveBuff(UnforgivenBuffs.airborneBuff);
                }
                Destroy(this);
            }
        }
    }
}
