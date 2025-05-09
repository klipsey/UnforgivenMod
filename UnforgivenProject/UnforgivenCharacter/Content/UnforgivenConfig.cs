using BepInEx.Configuration;
using UnforgivenMod.Modules;

namespace UnforgivenMod.Unforgiven.Content
{
    public static class UnforgivenConfig
    {
        public static ConfigEntry<bool> forceUnlock;
        public static ConfigEntry<bool> noShieldVisual;

        public static ConfigEntry<float> maxHealth;
        public static ConfigEntry<float> healthRegen;
        public static ConfigEntry<float> armor;
        public static ConfigEntry<float> shield;

        public static ConfigEntry<int> jumpCount;

        public static ConfigEntry<float> damage;
        public static ConfigEntry<float> attackSpeed;
        public static ConfigEntry<float> crit;

        public static ConfigEntry<float> moveSpeed;
        public static ConfigEntry<float> acceleration;
        public static ConfigEntry<float> jumpPower;

        public static ConfigEntry<bool> autoCalculateLevelStats;

        public static ConfigEntry<float> healthGrowth;
        public static ConfigEntry<float> regenGrowth;
        public static ConfigEntry<float> armorGrowth;
        public static ConfigEntry<float> shieldGrowth;

        public static ConfigEntry<float> damageGrowth;
        public static ConfigEntry<float> attackSpeedGrowth;
        public static ConfigEntry<float> critGrowth;

        public static ConfigEntry<float> moveSpeedGrowth;
        public static ConfigEntry<float> jumpPowerGrowth;

        public static ConfigEntry<float> swingDamageCoefficient;

        public static ConfigEntry<float> dashDamageCoefficient;

        public static ConfigEntry<float> dashStackingDamageCoefficient;

        public static ConfigEntry<int> baseMaxDashStacks;

        public static ConfigEntry<float> stabDamageCoefficient;

        public static ConfigEntry<float> tornadoDamageCoefficient;

        public static ConfigEntry<float> specialFirstDamageCoefficient;

        public static ConfigEntry<float> specialFinalDamageCoefficient;
        public static void Init()
        {
            string section = "Stats - 01";
            string section2 = "QOL - 02";

            damage = Config.BindAndOptions(section, "Change Base Damage Value", 12f);

            maxHealth = Config.BindAndOptions(section, "Change Max Health Value", 110f);
            healthRegen = Config.BindAndOptions(section, "Change Health Regen Value", 1.5f);
            armor = Config.BindAndOptions(section, "Change Armor Value", 0f);
            shield = Config.BindAndOptions(section, "Change Shield Value", 0f);

            jumpCount = Config.BindAndOptions(section, "Change Jump Count", 1);

            attackSpeed = Config.BindAndOptions(section, "Change Attack Speed Value", 1f);
            crit = Config.BindAndOptions(section, "Change Crit Value", 1f);

            moveSpeed = Config.BindAndOptions(section, "Change Move Speed Value", 7f);
            acceleration = Config.BindAndOptions(section, "Change Acceleration Value", 80f);
            jumpPower = Config.BindAndOptions(section, "Change Jump Power Value", 15f);

            autoCalculateLevelStats = Config.BindAndOptions(section, "Auto Calculate Level Stats", true);

            healthGrowth = Config.BindAndOptions(section, "Change Health Growth Value", 0.3f);
            regenGrowth = Config.BindAndOptions(section, "Change Regen Growth Value", 0.2f);
            armorGrowth = Config.BindAndOptions(section, "Change Armor Growth Value", 0f);
            shieldGrowth = Config.BindAndOptions(section, "Change Shield Growth Value", 0f);

            damageGrowth = Config.BindAndOptions(section, "Change Damage Growth Value", 0.2f);
            attackSpeedGrowth = Config.BindAndOptions(section, "Change Attack Speed Growth Value", 0f);
            critGrowth = Config.BindAndOptions(section, "Change Crit Growth Value", 0f);

            moveSpeedGrowth = Config.BindAndOptions(section, "Change Move Speed Growth Value", 0f);
            jumpPowerGrowth = Config.BindAndOptions(section, "Change Jump Power Growth Value", 0f);

            swingDamageCoefficient = Config.BindAndOptions(section, "Change Swift Strikes Damage Coefficient", 2.4f);

            dashDamageCoefficient = Config.BindAndOptions(section, "Change Sweeping Blade Damage Coefficient", 1.5f);

            dashStackingDamageCoefficient = Config.BindAndOptions(section, "Change Sweeping Blade Stacking Damage Coefficient", 0.5f);

            baseMaxDashStacks = Config.BindAndOptions(section, "Change Max Sweeping Blade Stacks", 4);

            stabDamageCoefficient = Config.BindAndOptions(section, "Change Steel Tempest Stab Damage Coefficient", 2.6f);

            tornadoDamageCoefficient = Config.BindAndOptions(section, "Change Steel Tempest Tornado Damage Coefficient", 4f);

            specialFirstDamageCoefficient = Config.BindAndOptions(section, "Change Last Breaths First 2 Slashes Damage Coefficient", 2.5f);

            specialFinalDamageCoefficient = Config.BindAndOptions(section, "Change Last Breaths Final Slash Damage Coefficient", 8f);

            forceUnlock = Config.BindAndOptions(
                section2,
                "Unlock Unforgiven",
                false,
                "Unlock Unforgiven.", true);

            noShieldVisual = Config.BindAndOptions(
                section2,
                "No Shield Visual",
                false,
                "Remove Wanderer's shield visual.", true);
        }
    }
}
