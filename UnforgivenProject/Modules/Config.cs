using System.Runtime.CompilerServices;
using BepInEx.Configuration;
using RiskOfOptions;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;
using UnityEngine;
namespace UnforgivenMod.Modules
{
    public static class Config
    {
        public static ConfigFile MyConfig = UnforgivenPlugin.instance.Config;

        /// <summary>
        /// automatically makes config entries for disabling survivors
        /// </summary>
        /// <param name="section"></param>
        /// <param name="characterName"></param>
        /// <param name="description"></param>
        /// <param name="enabledByDefault"></param>
        public static ConfigEntry<bool> CharacterEnableConfig(string section, string characterName, string description = "", bool enabledByDefault = true)
        {

            if (string.IsNullOrEmpty(description))
            {
                description = "Set to false to disable this character and as much of its code and content as possible";
            }
            return BindAndOptions<bool>(section,
                                        "Enable " + characterName,
                                        enabledByDefault,
                                        description,
                                        true);
        }

        public static ConfigEntry<T> BindAndOptions<T>(string section, string name, T defaultValue, string description = "", bool restartRequired = false) =>
            BindAndOptions<T>(section, name, defaultValue, 0, 20, description, restartRequired);
        public static ConfigEntry<T> BindAndOptions<T>(string section, string name, T defaultValue, float min, float max, string description = "", bool restartRequired = false)
        {
            if (string.IsNullOrEmpty(description))
            {
                description = name;
            }

            if (restartRequired)
            {
                description += " (restart required)";
            }
            ConfigEntry<T> configEntry = MyConfig.Bind(section, name, defaultValue, description);

            if (UnforgivenPlugin.riskOfOptionsInstalled)
            {
                TryRegisterOption(configEntry, min, max, restartRequired);
            }

            return configEntry;
        }

        //back compat
        public static ConfigEntry<float> BindAndOptionsSlider(string section, string name, float defaultValue, string description, float min = 0, float max = 20, bool restartRequired = false) =>
            BindAndOptions<float>(section, name, defaultValue, min, max, description, restartRequired);

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static void TryRegisterOption<T>(ConfigEntry<T> entry, float min, float max, bool restartRequired)
        {
            if (entry is ConfigEntry<float> floatEntry)
            {
                ModSettingsManager.AddOption(new SliderOption(floatEntry, new SliderConfig
                {
                    min = min,
                    max = max,
                    FormatString = "{0:0.###}",
                    restartRequired = restartRequired
                }), UnforgivenPlugin.MODUID, UnforgivenPlugin.MODNAME);
            }
            else if (entry is ConfigEntry<int> intEntry)
            {
                ModSettingsManager.AddOption(new IntSliderOption(intEntry, new IntSliderConfig
                {
                    min = (int)min,
                    max = (int)max,
                    restartRequired = restartRequired
                }), UnforgivenPlugin.MODUID, UnforgivenPlugin.MODNAME);
            }
            else if (entry is ConfigEntry<bool> boolEntry)
            {
                ModSettingsManager.AddOption(new CheckBoxOption(boolEntry, restartRequired),
                    UnforgivenPlugin.MODUID, UnforgivenPlugin.MODNAME);
            }
            else if (entry is ConfigEntry<KeyboardShortcut> shortcutEntry)
            {
                ModSettingsManager.AddOption(new KeyBindOption(shortcutEntry, restartRequired),
                    UnforgivenPlugin.MODUID, UnforgivenPlugin.MODNAME);
            }
            else
            {
                Log.Warning($"Risk of Options does not have a control for {entry.Definition.Section}/{entry.Definition.Key} ({typeof(T).Name}).");
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void InitializeOptions(Texture portrait)
        {
            ModSettingsManager.SetModDescription(
                "Wanderer stats, skill coefficients and visual settings. Options marked restart required take full effect after restarting. " +
                "Gameplay settings must match on all multiplayer peers; they are not synchronized.",
                UnforgivenPlugin.MODUID, UnforgivenPlugin.MODNAME);

            if (!(portrait is Texture2D texture) || !texture)
            {
                Log.Error("Wanderer's portrait is missing; the Risk of Options icon could not be set.");
                return;
            }

            ModSettingsManager.SetModIcon(
                Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f)),
                UnforgivenPlugin.MODUID, UnforgivenPlugin.MODNAME);
        }

        //Taken from https://github.com/ToastedOven/CustomEmotesAPI/blob/main/CustomEmotesAPI/CustomEmotesAPI/CustomEmotesAPI.cs
        public static bool GetKeyPressed(KeyboardShortcut entry)
        {
            foreach (var item in entry.Modifiers)
            {
                if (!Input.GetKey(item))
                {
                    return false;
                }
            }
            return Input.GetKeyDown(entry.MainKey);
        }
    }
}