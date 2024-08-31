using RoR2;
using RoR2.Skills;
using UnityEngine;

namespace UnforgivenMod.Unforgiven.Components
{
    public class UnforgivenPassive : MonoBehaviour
    {
        public SkillDef unforgivenPassive;

        public GenericSkill passiveSkillSlot;

        public bool isJump
        {
            get
            {
                if (unforgivenPassive && passiveSkillSlot)
                {
                    return passiveSkillSlot.skillDef == unforgivenPassive;
                }

                return false;
            }
        }
    }
}