using RoR2;
using UnforgivenMod.Modules.Achievements;
using UnforgivenMod.Unforgiven;

namespace UnforgivenMod.Unforgiven.Achievements
{
    //automatically creates language tokens "ACHIEVMENT_{identifier.ToUpper()}_NAME" and "ACHIEVMENT_{identifier.ToUpper()}_DESCRIPTION" 
    [RegisterAchievement(identifier, unlockableIdentifier, null, null)]
    public class UnforgivenMasterAchievement : BaseMasteryAchievement
    {
        public const string identifier = UnforgivenSurvivor.UNFORGIVEN_PREFIX + "masteryAchievement";
        public const string unlockableIdentifier = UnforgivenSurvivor.UNFORGIVEN_PREFIX + "masteryUnlockable";

        public override string RequiredCharacterBody => UnforgivenSurvivor.instance.bodyName;

        //difficulty coeff 3 is monsoon. 3.5 is typhoon for grandmastery skins
        public override float RequiredDifficultyCoefficient => 3;
    }
}