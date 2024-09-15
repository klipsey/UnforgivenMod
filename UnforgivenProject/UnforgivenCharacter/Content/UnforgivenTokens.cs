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

            string desc = "The Wanderer is low skill floor high skill ceiling survivor that can use animation cancels to keep his dps high.<color=#CCD3E0>" + Environment.NewLine + Environment.NewLine;
            desc = desc + "< ! > Use Steel Tempest between Swift Strikes to maximize your damage." + Environment.NewLine + Environment.NewLine;
            desc = desc + "< ! > Keep track of your Steel Tempest stacks as its tornado can be a devastating skill to clear out enemies." + Environment.NewLine + Environment.NewLine;
            desc = desc + "< ! > Using Steel Tempest during Sweeping Blade slashes in a circle allowing for more AOE damage." + Environment.NewLine + Environment.NewLine;
            desc = desc + "< ! > Using Last Breath during Sweeping Blades + Steel Tempest allows you to transfer the AOE damage to Last Breaths targets." + Environment.NewLine + Environment.NewLine;

            string lore = "if you die to yasuo, you got outplayed " +
                "it means he had to calculate (and execute) the fight exactly to his specifications, taking into account your cooldowns and his, the position of both junglers, the creep waves and the general \"feel\" of the lane (an ability that has to be honed through years, maybe decades of what you call merely \"gaming\"). " +
                "Indeed, to be defeated by yasuo is to realise that you have found your better, both as a player and as a man. Your mettle failed you, but the yasuo player is solid steel -hard, cold, reliable- and as he secures first blood and you anxiously await the surrender vote, his mind is still operating at full capacity preparing strategies for a near infinite number of possibilities. " +
                "I'll be taking my LP now, make sure you do your bans correctly next time.";
            string outro = "..and so he left, 10/0.";
            string outroFailure = "..and so he vanished, 0/10.";
            
            Language.Add(prefix + "NAME", "Wanderer");
            Language.Add(prefix + "DESCRIPTION", desc);
            Language.Add(prefix + "SUBTITLE", "The Unforgiven");
            Language.Add(prefix + "LORE", lore);
            Language.Add(prefix + "OUTRO_FLAVOR", outro);
            Language.Add(prefix + "OUTRO_FAILURE", outroFailure);

            #region Skins
            Language.Add(prefix + "MASTERY_SKIN_NAME", "Alternate");
            #endregion

            #region Passive
            Language.Add(prefix + "PASSIVE_NAME", "Way of the Wind");
            Language.Add(prefix + "PASSIVE_DESCRIPTION", $"Build up a passive <style=cIsHealing>shield</style> while moving. " +
                $"At max stacks, the <style=cIsHealing>shield</style> breaks and grants a <style=cIsHealing>barrier</style>. <style=cIsDamage>Critical Strike Chance</style> is increased by <style=cIsDamage>1.5x</style>.");
            #endregion

            #region Primary
            Language.Add(prefix + "PRIMARY_SWING_NAME", "Swift Strikes");
            Language.Add(prefix + "PRIMARY_SWING_DESCRIPTION", $"Slash in front dealing <style=cIsDamage>{UnforgivenStaticValues.swingDamageCoefficient * 100f}% damage</style>.");
            #endregion

            #region Secondary
            Language.Add(prefix + "SECONDARY_STEEL_NAME", "Steel Tempest");
            Language.Add(prefix + "SECONDARY_STEEL_DESCRIPTION", $"<style=cIsUtility>Swift</style>. Stab forward dealing <style=cIsDamage>{UnforgivenStaticValues.stabDamageCoefficient * 100f}% damage</style>. " +
                $"On hit, gain a stack of <color=#FFBF66>Gathering Storm</color>. At 2 stacks your next <color=#FFBF66>Steel Tempest</color> will fire a tornado dealing <style=cIsDamage>{UnforgivenStaticValues.tornadoDamageCoefficient * 100f}% damage</style>.");
            #endregion

            #region Utility 
            Language.Add(prefix + "UTILITY_SWEEP_NAME", "Sweeping Blade");
            Language.Add(prefix + "UTILITY_SWEEP_DESCRIPTION", $"Dash towards an enemy dealing <style=cIsDamage>{UnforgivenStaticValues.dashDamageCoefficient * 100f}% damage</style>. " +
                $"After each dash, your next dash will deal an additional <style=cIsDamage>{UnforgivenStaticValues.dashStackingDamageCoefficient * 100f}% damage</style> up to a cap of <style=cIsDamage>{UnforgivenStaticValues.dashStackingDamageCoefficient * 4f * 100f}% bonus damage</style>.");

            #endregion

            #region Special
            Language.Add(prefix + "SPECIAL_BREATH_NAME", "Last Breath");
            Language.Add(prefix + "SPECIAL_BREATH_DESCRIPTION", $"Blink to a targeted or nearby <style=cIsUtility>airborne</style> enemy and rapidly attack for <style=cIsDamage>2x{UnforgivenStaticValues.specialFirstDamageCoefficient * 100f}% + {UnforgivenStaticValues.specialFinalDamageCoefficient * 100f}% damage</style>. " +
                $"Gain bonus <style=cIsDamage>armor penetration</style> for 6 seconds.");

            Language.Add(prefix + "SPECIAL_SCEP_BREATH_NAME", "First Breath");
            Language.Add(prefix + "SPECIAL_SCEP_BREATH_DESCRIPTION", $"Blink to a targeted or nearby <style=cIsUtility>airborne</style> enemy and rapidly attack for <style=cIsDamage>2x{UnforgivenStaticValues.specialFirstDamageCoefficient * 100f}% + {UnforgivenStaticValues.specialFinalDamageCoefficient * 100f}% damage</style>. " +
                $"Gain bonus <style=cIsDamage>armor penetration</style> for 6 seconds." + Tokens.ScepterDescription("<style=cIsUtility>Reset your secondary cooldown</style>. Enemies hit by <color=#FFBF66>First Breath</color> can be targetted by <color=#FFBF66>Sweeping Blade</color> again."));
            #endregion

            #region Achievements
            Language.Add(Tokens.GetAchievementNameToken(UnforgivenMasteryAchievement.identifier), "Wanderer: Mastery");
            Language.Add(Tokens.GetAchievementDescriptionToken(UnforgivenMasteryAchievement.identifier), "As Wanderer, beat the game or obliterate on Monsoon.");
            /*
            Language.Add(Tokens.GetAchievementNameToken(UnforgivenUnlockAchievement.identifier), "Dressed to Kill");
            Language.Add(Tokens.GetAchievementDescriptionToken(UnforgivenUnlockAchievement.identifier), "Get a Backstab.");
            */
            #endregion

            #endregion
        }
    }
}