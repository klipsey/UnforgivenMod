using UnforgivenMod.Modules.BaseStates;
using UnforgivenMod.Unforgiven.SkillStates;

namespace UnforgivenMod.Unforgiven.Content
{
    public static class UnforgivenStates
    {
        public static void Init()
        {
            Modules.Content.AddEntityState(typeof(BaseUnforgivenSkillState));
            Modules.Content.AddEntityState(typeof(BaseUnforgivenState));
            Modules.Content.AddEntityState(typeof(MainState));

            Modules.Content.AddEntityState(typeof(SlashCombo));

            Modules.Content.AddEntityState(typeof(EnterStab));
            Modules.Content.AddEntityState(typeof(StabForward));
            Modules.Content.AddEntityState(typeof(Tornado));

            Modules.Content.AddEntityState(typeof(Dash));
            Modules.Content.AddEntityState(typeof(DashSpin));

            Modules.Content.AddEntityState(typeof(Special));
        }
    }
}
