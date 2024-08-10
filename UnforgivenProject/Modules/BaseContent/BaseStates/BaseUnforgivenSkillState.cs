using EntityStates;
using RoR2;
using UnforgivenMod.Unforgiven.Components;
using UnforgivenMod.Unforgiven.Content;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;

namespace UnforgivenMod.Modules.BaseStates
{
    public abstract class BaseUnforgivenSkillState : BaseSkillState
    {
        protected UnforgivenController unforgivenController;

        protected bool empowered;

        protected bool empoweredSpecial;
        public virtual void AddRecoil2(float x1, float x2, float y1, float y2)
        {
            this.AddRecoil(x1, x2, y1, y2);
        }
        public override void OnEnter()
        {
            RefreshState();
            base.OnEnter    ();
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
        }
        protected void RefreshState()
        {
            if (!unforgivenController)
            {
                unforgivenController = base.GetComponent<UnforgivenController>();
            }
            empowered = base.characterBody.HasBuff(UnforgivenBuffs.stabMaxStacksBuff);
            empoweredSpecial = base.characterBody.HasBuff(UnforgivenBuffs.lastBreathBuff);
        }
    }
}
