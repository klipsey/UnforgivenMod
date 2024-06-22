using HG;
using Newtonsoft.Json.Linq;
using R2API;
using RoR2;
using RoR2.Projectile;
using UnforgivenMod.Modules;
using UnforgivenMod.Unforgiven.Components;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;
using static RoR2.DotController;

namespace UnforgivenMod.Unforgiven.Content
{
    public static class DamageTypes
    {
        public static DamageAPI.ModdedDamageType Default;
        public static DamageAPI.ModdedDamageType KnockAirborne;

        internal static void Init()
        {
            Default = DamageAPI.ReserveDamageType();
            KnockAirborne = DamageAPI.ReserveDamageType();
            Hook();
        }
        private static void Hook()
        {
            GlobalEventManager.onServerDamageDealt += GlobalEventManager_onServerDamageDealt;
        }
        private static void GlobalEventManager_onServerDamageDealt(DamageReport damageReport)
        {
            DamageInfo damageInfo = damageReport.damageInfo;
            if (!damageReport.attackerBody || !damageReport.victimBody)
            {
                return;
            }
            HealthComponent victim = damageReport.victim;
            GameObject inflictorObject = damageInfo.inflictor;
            CharacterBody victimBody = damageReport.victimBody;
            EntityStateMachine victimMachine = victimBody.GetComponent<EntityStateMachine>();
            CharacterBody attackerBody = damageReport.attackerBody;
            GameObject attackerObject = damageReport.attacker.gameObject;
            UnforgivenController iController = attackerBody.GetComponent<UnforgivenController>();
            if (NetworkServer.active)
            {
                if (iController && attackerBody.baseNameToken == "KENKO_UNFORGIVEN_NAME")
                {
                    if(damageInfo.HasModdedDamageType(KnockAirborne)) 
                    {
                        victimBody.gameObject.AddComponent<AirborneComponent>();
                    }
                }
            }
        }
    }
}
