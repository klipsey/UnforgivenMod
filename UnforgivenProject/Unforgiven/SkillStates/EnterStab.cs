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
        public override void OnEnter()
        {
            RefreshState();
            base.OnEnter();

            EntityStateMachine b = null;
            EntityStateMachine[] components = base.gameObject.GetComponents<EntityStateMachine>();
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i].customName == "Body")
                {
                    b = components[i];

                    break;
                }
            }

            if (b && b.state is Dash)
            {
                b.SetNextStateToMain();
                this.outer.SetNextState(new DashSpin());
                return;
            }

            if (empowered)
            {
                this.outer.SetNextState(new Tornado());
                return;
            }
            else this.outer.SetNextState(new StabForward());
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}