using RoR2;
using UnforgivenMod.Unforgiven.Content;
using UnityEngine;

namespace UnforgivenMod.Unforgiven.Components
{
    [DisallowMultipleComponent]
    public sealed class LastBreathSuspension : MonoBehaviour
    {
        private CharacterBody body;
        private Rigidbody rigid;
        private RigidbodyConstraints previousConstraints;
        private bool holdingRigidbody;

        public static void Init()
        {
            On.RoR2.CharacterBody.OnBuffFirstStackGained += OnBuffFirstStackGained;
            On.RoR2.CharacterBody.OnBuffFinalStackLost += OnBuffFinalStackLost;
            On.RoR2.CharacterMotor.BeforeCharacterUpdate += BeforeCharacterUpdate;
        }

        private static void OnBuffFirstStackGained(On.RoR2.CharacterBody.orig_OnBuffFirstStackGained orig,
            CharacterBody body, BuffDef buff)
        {
            orig(body, buff);
            if (buff != UnforgivenBuffs.specialSuspensionBuff || body.characterMotor || !body.rigidbody)
            {
                return;
            }

            LastBreathSuspension suspension = body.GetComponent<LastBreathSuspension>();
            if (!suspension)
            {
                suspension = body.gameObject.AddComponent<LastBreathSuspension>();
            }
            suspension.Hold(body);
        }

        private static void OnBuffFinalStackLost(On.RoR2.CharacterBody.orig_OnBuffFinalStackLost orig,
            CharacterBody body, BuffDef buff)
        {
            orig(body, buff);
            if (buff == UnforgivenBuffs.specialSuspensionBuff)
            {
                LastBreathSuspension suspension = body.GetComponent<LastBreathSuspension>();
                if (suspension)
                {
                    suspension.Release();
                }
            }
        }

        private static void BeforeCharacterUpdate(On.RoR2.CharacterMotor.orig_BeforeCharacterUpdate orig,
            CharacterMotor motor, float deltaTime)
        {
            bool suspended = motor.body && motor.body.healthComponent && motor.body.healthComponent.alive &&
                motor.body.HasBuff(UnforgivenBuffs.specialSuspensionBuff);
            if (suspended)
            {
                motor.rootMotion = Vector3.zero;
                motor.Motor.ForceUnground();
            }
            orig(motor, deltaTime);
            if (suspended)
            {
                motor.velocity = Vector3.zero;
            }
        }

        private void Hold(CharacterBody targetBody)
        {
            if (holdingRigidbody)
            {
                return;
            }
            body = targetBody;
            rigid = body.rigidbody;
            previousConstraints = rigid.constraints;
            if (!rigid.isKinematic)
            {
                rigid.velocity = Vector3.zero;
            }
            rigid.constraints |= RigidbodyConstraints.FreezePosition;
            holdingRigidbody = true;
        }

        private void FixedUpdate()
        {
            if (holdingRigidbody && (!body || !body.healthComponent || !body.healthComponent.alive ||
                !body.HasBuff(UnforgivenBuffs.specialSuspensionBuff)))
            {
                Release();
            }
        }

        private void Release()
        {
            if (holdingRigidbody && rigid)
            {
                rigid.constraints = previousConstraints;
            }
            holdingRigidbody = false;
        }

        private void OnDisable()
        {
            Release();
        }
    }
}
