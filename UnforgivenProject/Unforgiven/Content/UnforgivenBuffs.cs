using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace UnforgivenMod.Unforgiven.Content
{
    public static class UnforgivenBuffs
    {
        public static BuffDef stabStackingBuff;
        public static BuffDef stabMaxStacksBuff;
        public static BuffDef dashCooldownBuff;
        public static BuffDef stackingDashDamageBuff;
        public static BuffDef specialSlamTrackerBuff;
        public static BuffDef airborneBuff;
        public static BuffDef lastBreathBuff;
        public static void Init(AssetBundle assetBundle)
        {
            stabStackingBuff = Modules.Content.CreateAndAddBuff("UnforgivenStackingBuff", Addressables.LoadAssetAsync<Sprite>("RoR2/Base/CritOnUse/texBuffFullCritIcon.tif").WaitForCompletion(),
                Color.gray, true, false, false);

            stabMaxStacksBuff = Modules.Content.CreateAndAddBuff("UnforgivenMaxStacksBuff", Addressables.LoadAssetAsync<Sprite>("RoR2/Base/CritOnUse/texBuffFullCritIcon.tif").WaitForCompletion(),
                UnforgivenAssets.unforgivenColor, false, false, false);
            
            dashCooldownBuff = Modules.Content.CreateAndAddBuff("UnforgivenDashCooldown", Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/texMovespeedBuffIcon.tif").WaitForCompletion(),
                Color.gray, false, false, true);

            stackingDashDamageBuff = Modules.Content.CreateAndAddBuff("UnforgivenStackingDamageBuff", Addressables.LoadAssetAsync<Sprite>("RoR2/Base/AttackSpeedOnCrit/texBuffAttackSpeedOnCritIcon.tif").WaitForCompletion(),
                UnforgivenAssets.unforgivenColor, true, false, false);
            
            specialSlamTrackerBuff = Modules.Content.CreateAndAddBuff("UnforgivenSlamTrackerBuff", Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/texBuffSlow50Icon.tif").WaitForCompletion(),
                UnforgivenAssets.unforgivenColor, false, false, false);

            airborneBuff = Modules.Content.CreateAndAddBuff("UnforgivenAirborneTrackerBuff", Addressables.LoadAssetAsync<Sprite>("RoR2/Base/EliteLunar/texBuffAffixLunar.tif").WaitForCompletion(),
                UnforgivenAssets.unforgivenColor, false, false, false);

            lastBreathBuff = Modules.Content.CreateAndAddBuff("LastBreathBuff", Addressables.LoadAssetAsync<Sprite>("RoR2/Base/LunarSkillReplacements/texBuffLunarDetonatorIcon.tif").WaitForCompletion(),
                UnforgivenAssets.unforgivenColor, false, false, false);
        }
    }
}
