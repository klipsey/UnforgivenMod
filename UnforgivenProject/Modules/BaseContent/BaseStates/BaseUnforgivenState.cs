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
    public abstract class BaseUnforgivenState : BaseState
    {
        protected UnforgivenController unforgivenController;

        public override void OnEnter()
        {
            base.OnEnter();
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            RefreshState();
        }
        protected void RefreshState()
        {
            if (!unforgivenController)
            {
                unforgivenController = base.GetComponent<UnforgivenController>();
            }
        }
    }
}
