using BepInEx.Configuration;
using UnforgivenMod.Modules;
using UnforgivenMod.Modules.Characters;
using RoR2;
using RoR2.Skills;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using RoR2.UI;
using R2API;
using R2API.Networking;
using UnityEngine.Networking;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using UnforgivenMod.Unforgiven.Components;
using UnforgivenMod.Unforgiven.Content;
using UnforgivenMod.Unforgiven.SkillStates;
using HG;
using EntityStates;
using R2API.Networking.Interfaces;
using EmotesAPI;
using System.Runtime.CompilerServices;
using static Rewired.Utils.Classes.Utility.ObjectInstanceTracker;
using System.Linq;

namespace UnforgivenMod.Unforgiven
{
    public class UnforgivenSurvivor : SurvivorBase<UnforgivenSurvivor>
    {
        public override string assetBundleName => "unforgiven";
        public override string bodyName => "UnforgivenBody";
        public override string masterName => "UnforgivenMonsterMaster";
        public override string modelPrefabName => "mdlUnforgiven";
        public override string displayPrefabName => "UnforgivenDisplay";

        public const string UNFORGIVEN_PREFIX = UnforgivenPlugin.DEVELOPER_PREFIX + "_UNFORGIVEN_";
        public override string survivorTokenPrefix => UNFORGIVEN_PREFIX;

        internal static GameObject characterPrefab;

        public static SkillDef firstBreath;

        public override BodyInfo bodyInfo => new BodyInfo
        {
            bodyName = bodyName,
            bodyNameToken = UNFORGIVEN_PREFIX + "NAME",
            subtitleNameToken = UNFORGIVEN_PREFIX + "SUBTITLE",

            characterPortrait = assetBundle.LoadAsset<Texture>("texUnforgivenIcon"),
            bodyColor = UnforgivenAssets.unforgivenColor,
            sortPosition = 7.9f,

            crosshair = Modules.CharacterAssets.LoadCrosshair("Standard"),
            podPrefab = RoR2.LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/SurvivorPod"),

            maxHealth = 110f,
            healthRegen = 1.5f,
            armor = 0f, 
            damage = 12f,

            jumpCount = 1,
        };

        public override CustomRendererInfo[] customRendererInfos => new CustomRendererInfo[]
        {
                new CustomRendererInfo
                {
                    childName = "Model",
                },
                new CustomRendererInfo
                {
                    childName = "KatanaModel",
                },
                new CustomRendererInfo
                {
                    childName = "SheathModel",
                },
                new CustomRendererInfo
                {
                    childName = "EmpoweredSword"
                },
                new CustomRendererInfo
                {
                    childName = "ArmModel"
                }
        };

        public override UnlockableDef characterUnlockableDef => UnforgivenUnlockables.characterUnlockableDef;

        public override ItemDisplaysBase itemDisplays => new UnforgivenItemDisplays();
        public override AssetBundle assetBundle { get; protected set; }
        public override GameObject bodyPrefab { get; protected set; }
        public override CharacterBody prefabCharacterBody { get; protected set; }
        public override GameObject characterModelObject { get; protected set; }
        public override CharacterModel prefabCharacterModel { get; protected set; }
        public override GameObject displayPrefab { get; protected set; }
        public override void Initialize()
        {

            //uncomment if you have multiple characters
            //ConfigEntry<bool> characterEnabled = Config.CharacterEnableConfig("Survivors", "Unforgiven");

            //if (!characterEnabled.Value)
            //    return;

            //need the character unlockable before you initialize the survivordef

            base.Initialize();
        }

        public override void InitializeCharacter()
        {
            UnforgivenConfig.Init();

            UnforgivenUnlockables.Init();

            base.InitializeCharacter();

            CameraParams.InitializeParams();

            DamageTypes.Init();

            UnforgivenStates.Init();
            UnforgivenTokens.Init();

            UnforgivenBuffs.Init(assetBundle);
            UnforgivenAssets.Init(assetBundle);

            InitializeEntityStateMachines();
            InitializeSkills();
            InitializeSkins();
            InitializeCharacterMaster();

            AdditionalBodySetup();

            characterPrefab = bodyPrefab;

            AddHooks();
        }

        private void AdditionalBodySetup()
        {
            AddHitboxes();
            bodyPrefab.AddComponent<UnforgivenController>();
            bodyPrefab.AddComponent<UnforgivenTracker>();
            bool tempAdd(CharacterBody body) => body.HasBuff(UnforgivenBuffs.dashCooldownBuff);
            bool tempAddShield(CharacterBody body) => body.HasBuff(UnforgivenBuffs.hasShieldBuff);
            bool tempNadoUp(CharacterBody body) => body.HasBuff(UnforgivenBuffs.stabMaxStacksBuff);
            float radius(CharacterBody body) => body.radius * 0.9f;
            float radius2(CharacterBody body) => body.radius;
            float radius3(CharacterBody body) => body.radius * 5f;
            if(!UnforgivenConfig.noShieldVisual.Value) TempVisualEffectAPI.AddTemporaryVisualEffect(UnforgivenAssets.shieldEffect, radius, tempAddShield);
            TempVisualEffectAPI.AddTemporaryVisualEffect(UnforgivenAssets.dashCdEffect, radius2, tempAdd);
            TempVisualEffectAPI.AddTemporaryVisualEffect(UnforgivenAssets.nadoUpEffect, radius3, tempNadoUp);
        }
        public void AddHitboxes()
        {
            Prefabs.SetupHitBoxGroup(characterModelObject, "MeleeHitbox", "MeleeHitbox");
            Prefabs.SetupHitBoxGroup(characterModelObject, "SteelTempestHitbox", "SteelTempestHitbox");
            Prefabs.SetupHitBoxGroup(characterModelObject, "SteelTempestSpinHitbox", "SteelTempestSpinHitbox");
        }

        public override void InitializeEntityStateMachines()
        {
            //clear existing state machines from your cloned body (probably commando)
            //omit all this if you want to just keep theirs
            Prefabs.ClearEntityStateMachines(bodyPrefab);

            //the main "Body" state machine has some special properties
            Prefabs.AddMainEntityStateMachine(bodyPrefab, "Body", typeof(SkillStates.MainState), typeof(EntityStates.SpawnTeleporterState));
            //if you set up a custom main characterstate, set it up here
            //don't forget to register custom entitystates in your UnforgivenStates.cs

            Prefabs.AddEntityStateMachine(bodyPrefab, "Weapon");
            Prefabs.AddEntityStateMachine(bodyPrefab, "Weapon2");
            Prefabs.AddEntityStateMachine(bodyPrefab, "Dash");
            Prefabs.AddEntityStateMachine(bodyPrefab, "Dash2");
        }

        #region skills
        public override void InitializeSkills()
        {
            bodyPrefab.AddComponent<UnforgivenPassive>();
            Skills.CreateSkillFamilies(bodyPrefab);
            AddPassiveSkills();
            AddPrimarySkills();
            AddSecondarySkills();
            AddUtilitySkills();
            AddSpecialSkills();
            if (UnforgivenPlugin.scepterInstalled) InitializeScepter();
        }

        private void AddPassiveSkills()
        {
            UnforgivenPassive passive = bodyPrefab.GetComponent<UnforgivenPassive>();

            SkillLocator skillLocator = bodyPrefab.GetComponent<SkillLocator>();

            skillLocator.passiveSkill.enabled = false;

            passive.unforgivenPassive = Skills.CreateSkillDef(new SkillDefInfo
            {
                skillName = UNFORGIVEN_PREFIX + "PASSIVE_NAME",
                skillNameToken = UNFORGIVEN_PREFIX + "PASSIVE_NAME",
                skillDescriptionToken = UNFORGIVEN_PREFIX + "PASSIVE_DESCRIPTION",
                skillIcon = assetBundle.LoadAsset<Sprite>("texPassiveIcon"),
                keywordTokens = new string[] { },
                activationState = new EntityStates.SerializableEntityStateType(typeof(EntityStates.Idle)),
                activationStateMachineName = "",
                baseMaxStock = 1,
                baseRechargeInterval = 0f,
                beginSkillCooldownOnSkillEnd = false,
                canceledFromSprinting = false,
                forceSprintDuringState = false,
                fullRestockOnAssign = true,
                interruptPriority = EntityStates.InterruptPriority.Any,
                resetCooldownTimerOnUse = false,
                isCombatSkill = false,
                mustKeyPress = false,
                cancelSprintingOnActivation = false,
                rechargeStock = 1,
                requiredStock = 2,
                stockToConsume = 1
            });

            Skills.AddPassiveSkills(passive.passiveSkillSlot.skillFamily, passive.unforgivenPassive);
        }

        private void AddPrimarySkills()
        {
            UnforgivenSteppedSkillDef unforgivenPrimary = Skills.CreateSkillDef<UnforgivenSteppedSkillDef>(new SkillDefInfo
                (
                    "Swift Strikes",
                    UNFORGIVEN_PREFIX + "PRIMARY_SWING_NAME",
                    UNFORGIVEN_PREFIX + "PRIMARY_SWING_DESCRIPTION",
                    assetBundle.LoadAsset<Sprite>("texPrimaryIcon"),
                    new SerializableEntityStateType(typeof(SlashCombo)),
                    "Weapon"
                ));
            unforgivenPrimary.stepCount = 2;
            unforgivenPrimary.stepGraceDuration = 0.1f;
            unforgivenPrimary.keywordTokens = new string[] { };

            Skills.AddPrimarySkills(bodyPrefab, unforgivenPrimary);
        }

        private void AddSecondarySkills()
        {
            SkillDef secondary = Skills.CreateSkillDef<ScaleCDwAttackSpeed>(new SkillDefInfo
            {
                skillName = "SteelTempest",
                skillNameToken = UNFORGIVEN_PREFIX + "SECONDARY_STEEL_NAME",
                skillDescriptionToken = UNFORGIVEN_PREFIX + "SECONDARY_STEEL_DESCRIPTION",
                keywordTokens = new string[] { Tokens.agileKeyword, Tokens.unforgivenSwiftKeyword },
                skillIcon = assetBundle.LoadAsset<Sprite>("texSecondaryIcon"),

                activationState = new SerializableEntityStateType(typeof(EnterStab)),

                activationStateMachineName = "Weapon2",
                interruptPriority = InterruptPriority.Skill,

                baseMaxStock = 1,
                baseRechargeInterval = 2.5f,
                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 1,

                resetCooldownTimerOnUse = false,
                fullRestockOnAssign = false,
                dontAllowPastMaxStocks = false,
                beginSkillCooldownOnSkillEnd = true,
                mustKeyPress = false,

                isCombatSkill = true,
                canceledFromSprinting = false,
                cancelSprintingOnActivation = false,
                forceSprintDuringState = false,
            });

            Skills.AddSecondarySkills(bodyPrefab, secondary);
        }

        private void AddUtilitySkills()
        {
            SkillDef SweepingBlade = Skills.CreateSkillDef<DashTrackerSkillDef>(new SkillDefInfo
            {
                skillName = "SweepingBlade",
                skillNameToken = UNFORGIVEN_PREFIX + "UTILITY_SWEEP_NAME",
                skillDescriptionToken = UNFORGIVEN_PREFIX + "UTILITY_SWEEP_DESCRIPTION",
                keywordTokens = new string[] { },
                skillIcon = assetBundle.LoadAsset<Sprite>("texUtilityIcon"),

                activationState = new SerializableEntityStateType(typeof(Dash)),
                activationStateMachineName = "Dash",
                interruptPriority = InterruptPriority.Skill,

                baseRechargeInterval = 0.25f,
                baseMaxStock = 1,

                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 1,

                resetCooldownTimerOnUse = false,
                fullRestockOnAssign = true,
                dontAllowPastMaxStocks = false,
                mustKeyPress = false,
                beginSkillCooldownOnSkillEnd = false,

                isCombatSkill = true,
                canceledFromSprinting = false,
                cancelSprintingOnActivation = false,
                forceSprintDuringState = true,

            });

            Skills.AddUtilitySkills(bodyPrefab, SweepingBlade);
        }

        private void AddSpecialSkills()
        {
            SkillDef LastBreath = Skills.CreateSkillDef<UnforgivenSpecialTrackerSkillDef>(new SkillDefInfo
            {
                skillName = "LastBreath",
                skillNameToken = UNFORGIVEN_PREFIX + "SPECIAL_BREATH_NAME",
                skillDescriptionToken = UNFORGIVEN_PREFIX + "SPECIAL_BREATH_DESCRIPTION",
                keywordTokens = new string[] { },
                skillIcon = assetBundle.LoadAsset<Sprite>("texSpecialIcon"),

                activationState = new EntityStates.SerializableEntityStateType(typeof(DashSpecial)),
                activationStateMachineName = "Dash",
                interruptPriority = EntityStates.InterruptPriority.PrioritySkill,

                baseRechargeInterval = 9f,
                baseMaxStock = 1,

                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 1,

                resetCooldownTimerOnUse = false,
                fullRestockOnAssign = true,
                dontAllowPastMaxStocks = false,
                mustKeyPress = true,
                beginSkillCooldownOnSkillEnd = true,

                isCombatSkill = true,
                canceledFromSprinting = false,
                cancelSprintingOnActivation = false,
                forceSprintDuringState = true,
            });

            Skills.AddSpecialSkills(bodyPrefab, LastBreath);
        }
        
        private void InitializeScepter()
        {
            firstBreath = Skills.CreateSkillDef<UnforgivenSpecialTrackerSkillDef>(new SkillDefInfo
            {
                skillName = "FirstBreath",
                skillNameToken = UNFORGIVEN_PREFIX + "SPECIAL_SCEP_BREATH_NAME",
                skillDescriptionToken = UNFORGIVEN_PREFIX + "SPECIAL_SCEP_BREATH_DESCRIPTION",
                keywordTokens = new string[] { },
                skillIcon = assetBundle.LoadAsset<Sprite>("texSpecialIcon"),

                activationState = new EntityStates.SerializableEntityStateType(typeof(DashSpecial)),
                activationStateMachineName = "Dash",
                interruptPriority = EntityStates.InterruptPriority.PrioritySkill,

                baseRechargeInterval = 9f,
                baseMaxStock = 1,

                rechargeStock = 1,
                requiredStock = 1,
                stockToConsume = 1,

                resetCooldownTimerOnUse = false,
                fullRestockOnAssign = true,
                dontAllowPastMaxStocks = false,
                mustKeyPress = true,
                beginSkillCooldownOnSkillEnd = true,

                isCombatSkill = true,
                canceledFromSprinting = false,
                cancelSprintingOnActivation = false,
                forceSprintDuringState = true,
            });

            AncientScepter.AncientScepterItem.instance.RegisterScepterSkill(firstBreath, bodyName, SkillSlot.Special, 0);
        }
        
        #endregion skills

        #region skins
        public override void InitializeSkins()
        {
            ModelSkinController skinController = prefabCharacterModel.gameObject.AddComponent<ModelSkinController>();

            CharacterModel.RendererInfo[] defaultRendererinfos = prefabCharacterModel.baseRendererInfos;

            List<SkinDef> skins = new List<SkinDef>();

            #region DefaultSkin
            //this creates a SkinDef with all default fields
            SkinDef defaultSkin = Skins.CreateSkinDef("DEFAULT_SKIN",
                assetBundle.LoadAsset<Sprite>("texDefaultSkin"),
                defaultRendererinfos,
                prefabCharacterModel.gameObject);

            //these are your Mesh Replacements. The order here is based on your CustomRendererInfos from earlier
            //pass in meshes as they are named in your assetbundle
            //currently not needed as with only 1 skin they will simply take the default meshes
            //uncomment this when you have another skin
            defaultSkin.meshReplacements = Modules.Skins.getMeshReplacements(assetBundle, defaultRendererinfos,
                "meshBody",
                "meshDefaultSword",
                "meshSheath",
                "meshEmpoweredSword",
                "meshArm");

            //add new skindef to our list of skindefs. this is what we'll be passing to the SkinController
            /*
            defaultSkin.gameObjectActivations = new SkinDef.GameObjectActivation[]
            {
                new SkinDef.GameObjectActivation
                {
                    gameObject = childLocator.FindChildGameObject("Tie"),
                    shouldActivate = true,
                }
            };
            */

            skins.Add(defaultSkin);
            #endregion

            //uncomment this when you have a mastery skin

            #region AscendencySkin

            ////creating a new skindef as we did before
            SkinDef ascendencySkin = Modules.Skins.CreateSkinDef(UNFORGIVEN_PREFIX + "MASTERY_SKIN_NAME",
                assetBundle.LoadAsset<Sprite>("texWhirlwindSkin"),
                defaultRendererinfos,
                prefabCharacterModel.gameObject,
                UnforgivenUnlockables.masterySkinUnlockableDef);

            ////adding the mesh replacements as above. 
            ////if you don't want to replace the mesh (for example, you only want to replace the material), pass in null so the order is preserved
            ascendencySkin.meshReplacements = Modules.Skins.getMeshReplacements(assetBundle, defaultRendererinfos,
                "meshAscendancyBody",
                "meshAscendancySword",//no gun mesh replacement. use same gun mesh
                "meshAscendancySheath",
                "meshAscendancySwordEmpowered",
                "meshAscendancyArm");

            ////masterySkin has a new set of RendererInfos (based on default rendererinfos)
            ////you can simply access the RendererInfos' materials and set them to the new materials for your skin.
            ascendencySkin.rendererInfos[0].defaultMaterial = UnforgivenAssets.ascendencyMat;
            ascendencySkin.rendererInfos[1].defaultMaterial = UnforgivenAssets.ascendencyMat;
            ascendencySkin.rendererInfos[2].defaultMaterial = UnforgivenAssets.ascendencyMat;
            ascendencySkin.rendererInfos[3].defaultMaterial = UnforgivenAssets.ascendencyMat;
            ascendencySkin.rendererInfos[4].defaultMaterial = UnforgivenAssets.ascendencyMat;

            ////here's a barebones example of using gameobjectactivations that could probably be streamlined or rewritten entirely, truthfully, but it works
            ////simply find an object on your child locator you want to activate/deactivate and set if you want to activate/deacitvate it with this skin

            skins.Add(ascendencySkin);

            #endregion

            skinController.skins = skins.ToArray();
        }
        #endregion skins


        //Character Master is what governs the AI of your character when it is not controlled by a player (artifact of vengeance, goobo)
        public override void InitializeCharacterMaster()
        {
            //if you're lazy or prototyping you can simply copy the AI of a different character to be used
            //Modules.Prefabs.CloneDopplegangerMaster(bodyPrefab, masterName, "Merc");

            //how to set up AI in code
            UnforgivenAI.Init(bodyPrefab, masterName);

            //how to load a master set up in unity, can be an empty gameobject with just AISkillDriver components
            //assetBundle.LoadMaster(bodyPrefab, masterName);
        }

        private void AddHooks()
        {
            HUD.onHudTargetChangedGlobal += HUDSetup;
            On.RoR2.UI.LoadoutPanelController.Rebuild += LoadoutPanelController_Rebuild;
            On.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalculateStats;
            On.RoR2.HealthComponent.TakeDamageProcess += HealthComponent_TakeDamageProcess;
            RoR2.GlobalEventManager.onCharacterDeathGlobal += GlobalEventManager_onCharacterDeathGlobal;

            if (UnforgivenPlugin.emotesInstalled) Emotes();
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static void Emotes()
        {
            On.RoR2.SurvivorCatalog.Init += (orig) =>
            {
                orig();
                var skele = UnforgivenAssets.mainAssetBundle.LoadAsset<GameObject>("unforgiven_emoteskeleton");
                CustomEmotesAPI.ImportArmature(UnforgivenSurvivor.characterPrefab, skele);
            };
        }
        private static void GlobalEventManager_onCharacterDeathGlobal(DamageReport damageReport)
        {
            CharacterBody attackerBody = damageReport.attackerBody;
            if (attackerBody && damageReport.attackerMaster && damageReport.victim && attackerBody.bodyIndex == BodyCatalog.FindBodyIndex("UnforgivenBody") && damageReport.victimBody.HasBuff(UnforgivenBuffs.airborneBuff))
            {
                if (damageReport.victim.gameObject.TryGetComponent<NetworkIdentity>(out var identity))
                {
                    new SyncWindExplosion(identity.netId, damageReport.victim.gameObject).Send(NetworkDestination.Clients);
                }
            }
        }
        private void HealthComponent_TakeDamageProcess(On.RoR2.HealthComponent.orig_TakeDamageProcess orig, HealthComponent self, DamageInfo damageInfo)
        {
            if (NetworkServer.active && self.alive || !self.godMode || self.ospTimer <= 0f)
            {
                CharacterBody victimBody = self.body;
                if (victimBody && victimBody.bodyIndex == BodyCatalog.FindBodyIndex("UnforgivenBody") && !damageInfo.rejected)
                {
                    if(victimBody.TryGetComponent<UnforgivenController>(out var uCont))
                    {
                        if(uCont.shieldAmount == 100f)
                        {
                            victimBody.healthComponent.AddBarrier(victimBody.healthComponent.fullCombinedHealth * 0.25f);
                            uCont.shieldAmount = 0f;
                            victimBody.RemoveBuff(UnforgivenBuffs.hasShieldBuff);
                            Util.PlaySound("sfx_unforgiven_nado_impact", victimBody.gameObject);
                            Util.PlaySound("sfx_unforgiven_lost_stacks", victimBody.gameObject);
                        }
                    }
                }
            }
            orig.Invoke(self, damageInfo);
        }
        private static void LoadoutPanelController_Rebuild(On.RoR2.UI.LoadoutPanelController.orig_Rebuild orig, LoadoutPanelController self)
        {
            orig(self);

            if (self.currentDisplayData.bodyIndex == BodyCatalog.FindBodyIndex("UnforgivenBody"))
            {
                foreach (LanguageTextMeshController i in self.gameObject.GetComponentsInChildren<LanguageTextMeshController>())
                {
                    if (i && i.token == "LOADOUT_SKILL_MISC") i.token = "Passive";
                }
            }
        }

        
        private void CharacterBody_RecalculateStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
        {
            orig(self);

            if (self)
            {
                if (self.bodyIndex == BodyCatalog.FindBodyIndex("UnforgivenBody"))
                {
                    self.crit *= 1.5f;
                    self.critMultiplier *= 0.9f;
                }
            }
        }
        

        private static void HUDSetup(HUD hud)
        {
            if (hud.targetBodyObject && hud.targetMaster && hud.targetMaster.bodyPrefab == UnforgivenSurvivor.characterPrefab)
            {
                if (!hud.targetMaster.hasAuthority) return;
                Transform skillsContainer = hud.equipmentIcons[0].gameObject.transform.parent;

                // ammo display for atomic
                Transform healthbarContainer = hud.transform.Find("MainContainer").Find("MainUIArea").Find("SpringCanvas").Find("BottomLeftCluster").Find("BarRoots").Find("LevelDisplayCluster");

                GameObject shieldTracker = GameObject.Instantiate(healthbarContainer.gameObject, hud.transform.Find("MainContainer").Find("MainUIArea").Find("SpringCanvas").Find("BottomLeftCluster"));
                shieldTracker.name = "ShieldTracker";
                shieldTracker.transform.SetParent(hud.transform.Find("MainContainer").Find("MainUIArea").Find("CrosshairCanvas").Find("CrosshairExtras"));

                GameObject.DestroyImmediate(shieldTracker.transform.GetChild(0).gameObject);
                MonoBehaviour.Destroy(shieldTracker.GetComponentInChildren<LevelText>());
                MonoBehaviour.Destroy(shieldTracker.GetComponentInChildren<ExpBar>());

                shieldTracker.transform.Find("LevelDisplayRoot").Find("ValueText").gameObject.SetActive(false);
                GameObject.DestroyImmediate(shieldTracker.transform.Find("ExpBarRoot").gameObject);

                shieldTracker.transform.Find("LevelDisplayRoot").GetComponent<RectTransform>().anchoredPosition = new Vector2(-12f, 0f);

                RectTransform rect = shieldTracker.GetComponent<RectTransform>();
                rect.localScale = new Vector3(0.8f, 0.8f, 1f);
                rect.anchorMin = new Vector2(0f, 0f);
                rect.anchorMax = new Vector2(0f, 0f);
                rect.offsetMin = new Vector2(120f, -40f);
                rect.offsetMax = new Vector2(120f, -40f);
                rect.pivot = new Vector2(0.5f, 0f);
                //positional data doesnt get sent to clients? Manually making offsets works..
                rect.anchoredPosition = new Vector2(50f, 0f);
                rect.localPosition = new Vector3(120f, -40f, 0f);

                GameObject chargeBarAmmo = GameObject.Instantiate(UnforgivenAssets.mainAssetBundle.LoadAsset<GameObject>("WeaponChargeBar"));
                chargeBarAmmo.name = "WindShieldMeter";
                chargeBarAmmo.transform.SetParent(hud.transform.Find("MainContainer").Find("MainUIArea").Find("CrosshairCanvas").Find("CrosshairExtras"));

                rect = chargeBarAmmo.GetComponent<RectTransform>();

                rect.localScale = new Vector3(0.75f, 0.1f, 1f);
                rect.anchorMin = new Vector2(100f, 2f);
                rect.anchorMax = new Vector2(100f, 2f);
                rect.pivot = new Vector2(0.5f, 0f);
                rect.anchoredPosition = new Vector2(100f, 2f);
                rect.localPosition = new Vector3(100f, 2f, 0f);
                rect.rotation = Quaternion.Euler(new Vector3(0f, 0f, 90f));

                PassiveShieldHudController stealthComponent = shieldTracker.AddComponent<PassiveShieldHudController>();

                stealthComponent.targetHUD = hud;
                stealthComponent.targetText = shieldTracker.transform.Find("LevelDisplayRoot").Find("PrefixText").gameObject.GetComponent<LanguageTextMeshController>();
                stealthComponent.durationDisplay = chargeBarAmmo;
                stealthComponent.durationBar = chargeBarAmmo.transform.GetChild(1).gameObject.GetComponent<UnityEngine.UI.Image>();
                stealthComponent.durationBarColor = chargeBarAmmo.transform.GetChild(0).gameObject.GetComponent<UnityEngine.UI.Image>();
            }
        }
    }
}