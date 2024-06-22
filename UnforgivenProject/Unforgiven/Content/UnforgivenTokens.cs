using System;
using UnforgivenMod.Modules;
using UnforgivenMod.Unforgiven;
using UnforgivenMod.Unforgiven.Achievements;
using UnityEngine.UIElements;

namespace UnforgivenMod.Unforgiven.Content
{
    public static class UnforgivenTokens
    {
        public static void Init()
        {
            AddUnforgivenTokens();

            ////uncomment this to spit out a lanuage file with all the above tokens that people can translate
            ////make sure you set Language.usingLanguageFolder and printingEnabled to true
            //Language.PrintOutput("Unforgiven.txt");
            //todo guide
            ////refer to guide on how to build and distribute your mod with the proper folders
        }

        public static void AddUnforgivenTokens()
        {
            #region Unforgiven
            string prefix = UnforgivenSurvivor.UNFORGIVEN_PREFIX;

            string desc = "<color=#CCD3E0>" + Environment.NewLine + Environment.NewLine;
            desc = desc + "< ! > " + Environment.NewLine + Environment.NewLine;
            desc = desc + "< ! > " + Environment.NewLine + Environment.NewLine;
            desc = desc + "< ! > " + Environment.NewLine + Environment.NewLine;
            desc = desc + "< ! > " + Environment.NewLine + Environment.NewLine;

            string lore = "Insert lore here";
            string outro = "..and so he left.";
            string outroFailure = "..and so he vanished.";
            
            Language.Add(prefix + "NAME", "Unforgiven");
            Language.Add(prefix + "DESCRIPTION", desc);
            Language.Add(prefix + "SUBTITLE", "Lost Wanderer");
            Language.Add(prefix + "LORE", lore);
            Language.Add(prefix + "OUTRO_FLAVOR", outro);
            Language.Add(prefix + "OUTRO_FAILURE", outroFailure);

            #region Skins
            Language.Add(prefix + "MASTERY_SKIN_NAME", "Alternate");
            #endregion

            #region Passive
            Language.Add(prefix + "PASSIVE_NAME", "Way of the Wanderer");
            Language.Add(prefix + "PASSIVE_DESCRIPTION", $"<color=#FFBF66>Unforgiven</color> builds up a passive <style=cIsHealing>shield</style> while moving. " +
                $"At max stacks, the shield breaks and grants a <style=cIsHealing>barrier</style>. <color=#FFBF66>Unforgiven</color> has <style=cIsDamage>2x crit chance</style>.");
            #endregion

            #region Primary
            Language.Add(prefix + "PRIMARY_SWING_NAME", "Swift Strikes");
            Language.Add(prefix + "PRIMARY_SWING_DESCRIPTION", $"Swing in front dealing <style=cIsDamage>{UnforgivenStaticValues.swingDamageCoefficient * 100f}% damage</style>.");
            #endregion

            #region Secondary
            Language.Add(prefix + "SECONDARY_STEEL_NAME", "Steel Tempest");
            Language.Add(prefix + "SECONDARY_STEEL_DESCRIPTION", $"<style=cIsUtility>Swift</style>. Stab forward dealing <style=cIsDamage>{UnforgivenStaticValues.stabDamageCoefficient * 100f}% damage</style>. " +
                $"On hit, gain a stack of <style=cIsUtility>Gathering Storm</style>. At 3 stacks, fire a tornado dealing <style=cIsDamage>{UnforgivenStaticValues.tornadoDamageCoefficient * 100f}& damage</style>.");
            #endregion

            #region Utility 
            Language.Add(prefix + "UTILITY_SWEEP_NAME", "Sweeping Blade");
            Language.Add(prefix + "UTILITY_SWEEP_DESCRIPTION", $"Dash towards an enemy dealing <style=cIsDamage>{UnforgivenStaticValues.dashDamageCoefficient * 100f}% damage</style>. " +
                $"After each dash, your next dash will deal and additional <style=cIsDamage>75% damage</style> up to a cap of <style=cIsDamage>300% damage</style>.");

            #endregion

            #region Special
            Language.Add(prefix + "SPECIAL_BREATH_NAME", "Last Breath");
            Language.Add(prefix + "SPECIAL_BREATH_DESCRIPTION", $"Dash towards an <style=cIsUtility>airborne</style> enemy then rapidly attack in an area for <style=cIsDamage>{UnforgivenStaticValues.specialFirstDamageCoefficient * 100f} x 2 + {UnforgivenStaticValues.specialFinalDamageCoefficient * 100f}% damage</style>.");
            #endregion

            #region Achievements
            Language.Add(Tokens.GetAchievementNameToken(UnforgivenMasterAchievement.identifier), "Unforgiven: Mastery");
            Language.Add(Tokens.GetAchievementDescriptionToken(UnforgivenMasterAchievement.identifier), "As Unforgiven, beat the game or obliterate on Monsoon.");
            /*
            Language.Add(Tokens.GetAchievementNameToken(UnforgivenUnlockAchievement.identifier), "Dressed to Kill");
            Language.Add(Tokens.GetAchievementDescriptionToken(UnforgivenUnlockAchievement.identifier), "Get a Backstab.");
            */
            #endregion

            #endregion
        }
    }
}