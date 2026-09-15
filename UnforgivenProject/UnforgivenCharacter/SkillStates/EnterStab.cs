using UnforgivenMod.Unforgiven.Content;
using System.Collections.Generic;
using System.Text;
using RoR2;
using EntityStates;
using System.Reflection;
using UnforgivenMod.Modules.BaseStates;

namespace UnforgivenMod.Unforgiven.SkillStates
{
    public class EnterStab : BaseUnforgivenSkillState
    {
        EntityState savedState;
        bool hasChosenState;
        private EntityStateMachine dashStateMachine;
        private bool hasLoggedMissingDash;
        public bool HasBufferedSpin => hasChosenState;
        public override void OnEnter()
        {
            RefreshState();
            base.OnEnter();
            dashStateMachine = EntityStateMachine.FindByCustomName(base.gameObject, "Dash");
            if (!dashStateMachine)
            {
                LogMissingDashStateMachine();
            }
            else if (dashStateMachine.state is Dash)
            {
                BufferSpin();
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            EntityStateMachine b = dashStateMachine;
            if (!b)
            {
                LogMissingDashStateMachine();
            }

            if (b && b.state is Dash && !hasChosenState)
            {
                BufferSpin();
            }

            if (!base.isAuthority)
            {
                return;
            }

            if (hasChosenState)
            {
                this.outer.SetNextState(savedState);
                return;
            }

            if (empowered)
            {
                this.outer.SetNextState(new Tornado());
            }
            else
            {
                this.outer.SetNextState(new StabForward());
            }
        }

        private void BufferSpin()
        {
            hasChosenState = true;
            savedState = new DashSpin
            {
                empoweredSpin = empowered,
                activatorSkillSlot = this.activatorSkillSlot
            };
        }

        private void LogMissingDashStateMachine()
        {
            if (hasLoggedMissingDash)
            {
                return;
            }
            hasLoggedMissingDash = true;
            Log.Error("EnterStab could not find the required Dash state machine; continuing without waiting for a dash.");
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}