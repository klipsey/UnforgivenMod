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
        public override void OnEnter()
        {
            RefreshState();
            base.OnEnter();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            EntityStateMachine b = null;
            EntityStateMachine[] components = base.gameObject.GetComponents<EntityStateMachine>();
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i].customName == "Dash")
                {
                    b = components[i];

                    break;
                }
            }

            if (b && b.state is Dash && !hasChosenState)
            {
                hasChosenState = true;
                unforgivenController.bufferedSpin = true;
                savedState = new DashSpin();
                return;
            }

            if (b && b.state is DashSpecial && !hasChosenState)
            {
                hasChosenState = true;
                unforgivenController.bufferedSpin = true;
                savedState = new DashSpin();
                return;
            }

            if(!hasChosenState)
            {
                if (empowered)
                {
                    this.outer.SetNextState(new Tornado());
                    return;
                }
                else
                {
                    this.outer.SetNextState(new StabForward());
                    return;
                }
            }
            else if(!(b.state is DashSpecial || b.state is Dash))
            {
                this.outer.SetNextState(savedState);
                return;
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}