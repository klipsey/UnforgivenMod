using RoR2;
using UnityEngine;
using UnforgivenMod.Unforgiven;
using UnforgivenMod.Unforgiven.Achievements;

namespace UnforgivenMod.Unforgiven.Content
{
    public static class UnforgivenUnlockables
    {
        public static UnlockableDef characterUnlockableDef = null;
        public static UnlockableDef ascendencySkinUnlockableDef = null;

        public static void Init()
        {
            ascendencySkinUnlockableDef = Modules.Content.CreateAndAddUnlockbleDef(UnforgivenMasteryAchievement.unlockableIdentifier,
                Modules.Tokens.GetAchievementNameToken(UnforgivenMasteryAchievement.unlockableIdentifier),
                UnforgivenSurvivor.instance.assetBundle.LoadAsset<Sprite>("texWhirlwindSkin"));
            /*
            if (true == false)
            {
                characterUnlockableDef = Modules.Content.CreateAndAddUnlockableDef(UnforgivenUnlockAchievement.unlockableIdentifier,
                Modules.Tokens.GetAchievementNameToken(UnforgivenUnlockAchievement.unlockableIdentifier),
                UnforgivenSurvivor.instance.assetBundle.LoadAsset<Sprite>("texUnforgivenIcon"));
            }
            */
        }
    }
}
