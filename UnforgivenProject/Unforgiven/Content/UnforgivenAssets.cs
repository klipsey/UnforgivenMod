using RoR2;
using UnityEngine;
using UnforgivenMod.Modules;
using RoR2.Projectile;
using RoR2.UI;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using R2API;
using UnityEngine.Rendering.PostProcessing;
using ThreeEyedGames;
using UnforgivenMod.Unforgiven.Components;
using Rewired.ComponentControls.Effects;
using static RoR2.Skills.ComboSkillDef;

namespace UnforgivenMod.Unforgiven.Content
{
    public static class UnforgivenAssets
    {
        //AssetBundle
        internal static AssetBundle mainAssetBundle;

        //Materials
        internal static Material commandoMat;

        //Projectiles
        internal static GameObject nadoPrefab;

        internal static GameObject nadoGhost;

        //Shader
        internal static Shader hotpoo = Resources.Load<Shader>("Shaders/Deferred/HGStandard");

        //Effects
        internal static GameObject bloodSplatterEffect;
        internal static GameObject windExplosionEffect;
        internal static GameObject beerExplosion;
        internal static GameObject bloodSpurtEffect;

        internal static GameObject spinSlashEffect;
        internal static GameObject spinEmpoweredSlashEffect;

        internal static GameObject specialSlashingEffect;
        internal static GameObject specialEmpoweredSlashingEffect;

        internal static GameObject swordSwingEffect;
        internal static GameObject swordSwingEmpoweredEffect;
        internal static GameObject stabSwingEffect;
        internal static GameObject unforgivenHitEffect;

        internal static GameObject batHitEffectRed;

        internal static GameObject dashEffect;

        internal static GameObject realNado;

        internal static GameObject spinNadoEffect;

        internal static GameObject dashCdEffect;

        internal static GameObject shieldEffect;

        internal static GameObject nadoUpEffect;

        internal static Material specialMaterial;
        //Sounds
        internal static NetworkSoundEventDef swordImpactSoundEvent;
        internal static NetworkSoundEventDef stabImpactSoundEvent;
        internal static NetworkSoundEventDef nadoImpactSoundEvent;
        //Colors
        internal static Color unforgivenColor = new Color(255f / 255f, 102f / 255f, 102f / 255f);
        internal static Color unforgivenSecondaryColor = Color.red;

        //UI
        internal static GameObject throwable;
        internal static GameObject throwableEnd;

        internal static Sprite secondaryIcon;
        internal static Sprite secondaryEmpoweredIcon;

        internal static GameObject unforgivenIndicator;

        //Crosshair
        public static void Init(AssetBundle assetBundle)
        {
            mainAssetBundle = assetBundle;

            CreateMaterials();

            CreateModels();

            CreateEffects();

            CreateSounds();

            CreateProjectiles();

            CreateUI();
        }


        private static void CleanChildren(Transform startingTrans)
        {
            for (int num = startingTrans.childCount - 1; num >= 0; num--)
            {
                if (startingTrans.GetChild(num).childCount > 0)
                {
                    CleanChildren(startingTrans.GetChild(num));
                }
                Object.DestroyImmediate(startingTrans.GetChild(num).gameObject);
            }
        }

        private static void CreateMaterials()
        {
            specialMaterial = Addressables.LoadAssetAsync<Material>("RoR2/Base/Imp/matImpBossDissolve.mat").WaitForCompletion();
        }

        private static void CreateModels()
        {
        }
        #region effects
        private static void CreateEffects()
        {
            dashCdEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Nullifier/NullifyStack3Effect.prefab").WaitForCompletion().InstantiateClone("UnforgivenDashCdEffect", false);
            dashCdEffect.transform.GetChild(0).GetChild(0).gameObject.GetComponent<MeshRenderer>().materials[0].SetColor("_TintColor", Color.clear);
            dashCdEffect.transform.GetChild(0).GetChild(1).gameObject.GetComponent<MeshRenderer>().materials[0].SetColor("_TintColor", Color.clear);
            dashCdEffect.transform.GetChild(0).GetChild(2).gameObject.GetComponent<MeshRenderer>().materials[0].SetColor("_TintColor", Color.clear);

            shieldEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/OutOfCombatArmor/OutOfCombatArmorEffect.prefab").WaitForCompletion().InstantiateClone("UnforgivenShieldReadyEffect", false);
            shieldEffect.transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>().materials[0].SetColor("_TintColor", unforgivenColor);
            shieldEffect.transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>().materials[1].SetColor("_TintColor", unforgivenColor);
            var shieldMain = shieldEffect.transform.GetChild(0).GetChild(1).GetComponent<ParticleSystem>().main;
            shieldMain.startColor = Color.red;
            shieldEffect.transform.GetChild(0).Find("Trigger").Find("Sphere, Quick").GetComponent<ParticleSystemRenderer>().material.SetColor("_TintColor", Color.red);
            shieldEffect.transform.GetChild(0).Find("Trigger").Find("Sphere").GetComponent<ParticleSystemRenderer>().material.SetColor("_TintColor", Color.red);
            shieldEffect.transform.GetChild(0).Find("Trigger").Find("Point Light").GetComponent<Light>().color = Color.red;

            nadoUpEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/TeamWarCry/TeamWarCryAura.prefab").WaitForCompletion().InstantiateClone("UnforgivenNadoUpEffect", false);

            spinSlashEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Merc/MercSwordSlashWhirlwind.prefab").WaitForCompletion().InstantiateClone("UnforgivenSpinSlash");
            if(!spinSlashEffect.GetComponent<NetworkIdentity>()) spinSlashEffect.AddComponent<NetworkIdentity>();

            spinSlashEffect.transform.GetChild(0).gameObject.GetComponent<ParticleSystemRenderer>().material.SetColor("_TintColor", Color.red);

            spinSlashEffect.transform.GetChild(0).localScale *= 1.5f;

            Modules.Content.CreateAndAddEffectDef(spinSlashEffect);

            spinEmpoweredSlashEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Merc/MercSwordSlashWhirlwind.prefab").WaitForCompletion().InstantiateClone("UnforgivenSpinSlashEmpowered");
            if (!spinEmpoweredSlashEffect.GetComponent<NetworkIdentity>()) spinEmpoweredSlashEffect.AddComponent<NetworkIdentity>();

            spinEmpoweredSlashEffect.transform.GetChild(0).gameObject.GetComponent<ParticleSystemRenderer>().material = Object.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/Base/Imp/matImpSwipe.mat").WaitForCompletion());

            spinEmpoweredSlashEffect.transform.GetChild(0).localScale *= 1.5f;

            Modules.Content.CreateAndAddEffectDef(spinEmpoweredSlashEffect);


            specialSlashingEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Merc/EvisOverlapProjectileGhost.prefab").WaitForCompletion().InstantiateClone("UnforgivenSpecialSlash");
            if (!specialSlashingEffect.GetComponent<NetworkIdentity>()) specialSlashingEffect.AddComponent<NetworkIdentity>();
            Component.Destroy(specialSlashingEffect.GetComponent<ProjectileGhostController>());

            Object.Destroy(specialSlashingEffect.transform.Find("Point Light").gameObject);

            EffectComponent ec2 = specialSlashingEffect.AddComponent<EffectComponent>();
            ec2.applyScale = true;

            specialSlashingEffect.transform.Find("Slashes").gameObject.GetComponent<ParticleSystemRenderer>().material.SetColor("_TintColor", Color.red);

            specialSlashingEffect.transform.Find("BillboardSlashes").gameObject.GetComponent<ParticleSystemRenderer>().material = Object.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/Base/Common/VFX/matOmniRadialSlash1Generic.mat").WaitForCompletion());
            specialSlashingEffect.transform.Find("BillboardSlashes").gameObject.GetComponent<ParticleSystemRenderer>().material.SetColor("_TintColor", unforgivenColor);
            
            specialSlashingEffect.transform.Find("SharpSlashes").gameObject.GetComponent<ParticleSystemRenderer>().material = Object.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/Base/Common/VFX/matOmniHitspark2Generic.mat").WaitForCompletion());
            specialSlashingEffect.transform.Find("SharpSlashes").gameObject.GetComponent<ParticleSystemRenderer>().material.SetColor("_TintColor", unforgivenColor);

            var ssemain = specialSlashingEffect.transform.Find("Flashes").gameObject.GetComponent<ParticleSystem>().main;
            ssemain.startColor = unforgivenColor;

            specialSlashingEffect.transform.Find("Point Light").GetComponent<Light>().color = Color.red;

            specialSlashingEffect.transform.Find("Hologram").gameObject.GetComponent<ParticleSystemRenderer>().material.SetTexture("_RemapTex", Addressables.LoadAssetAsync<Texture>("RoR2/Base/Common/ColorRamps/texRampGolemElectric.png").WaitForCompletion());
            specialSlashingEffect.transform.Find("Hologram").gameObject.GetComponent<ParticleSystemRenderer>().material.SetColor("_TintColor", unforgivenColor);

            specialSlashingEffect.transform.Find("HologramReturn").gameObject.GetComponent<ParticleSystemRenderer>().material.SetTexture("_RemapTex", Addressables.LoadAssetAsync<Texture>("RoR2/Base/Common/ColorRamps/texRampGolemElectric.png").WaitForCompletion());
            specialSlashingEffect.transform.Find("HologramReturn").gameObject.GetComponent<ParticleSystemRenderer>().material.SetColor("_TintColor", unforgivenColor);

            Modules.Content.CreateAndAddEffectDef(specialSlashingEffect);


            specialEmpoweredSlashingEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Merc/EvisOverlapProjectileGhost.prefab").WaitForCompletion().InstantiateClone("UnforgivenSpecialEmpoweredSlash");
            if (!specialEmpoweredSlashingEffect.GetComponent<NetworkIdentity>()) specialEmpoweredSlashingEffect.AddComponent<NetworkIdentity>();
            Component.Destroy(specialEmpoweredSlashingEffect.GetComponent<ProjectileGhostController>());

            Object.Destroy(specialEmpoweredSlashingEffect.transform.Find("Point Light").gameObject);

            ec2 = specialEmpoweredSlashingEffect.AddComponent<EffectComponent>();
            ec2.applyScale = true;

            specialEmpoweredSlashingEffect.transform.Find("Slashes").gameObject.GetComponent<ParticleSystemRenderer>().material = Addressables.LoadAssetAsync<Material>("RoR2/Base/Imp/matImpSwipe.mat").WaitForCompletion();

            specialEmpoweredSlashingEffect.transform.Find("BillboardSlashes").gameObject.GetComponent<ParticleSystemRenderer>().material = Object.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/Base/Common/VFX/matOmniRadialSlash1Generic.mat").WaitForCompletion());
            specialEmpoweredSlashingEffect.transform.Find("BillboardSlashes").gameObject.GetComponent<ParticleSystemRenderer>().material.SetColor("_TintColor", unforgivenColor);

            specialEmpoweredSlashingEffect.transform.Find("SharpSlashes").gameObject.GetComponent<ParticleSystemRenderer>().material = Object.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/Base/Common/VFX/matOmniHitspark2Generic.mat").WaitForCompletion());
            specialEmpoweredSlashingEffect.transform.Find("SharpSlashes").gameObject.GetComponent<ParticleSystemRenderer>().material.SetColor("_TintColor", unforgivenColor);

            ssemain = specialEmpoweredSlashingEffect.transform.Find("Flashes").gameObject.GetComponent<ParticleSystem>().main;
            ssemain.startColor = unforgivenColor;

            specialEmpoweredSlashingEffect.transform.Find("Point Light").GetComponent<Light>().color = Color.red;

            specialEmpoweredSlashingEffect.transform.Find("Hologram").gameObject.GetComponent<ParticleSystemRenderer>().material.SetTexture("_RemapTex", Addressables.LoadAssetAsync<Texture>("RoR2/Base/Common/ColorRamps/texRampGolemElectric.png").WaitForCompletion());
            specialEmpoweredSlashingEffect.transform.Find("Hologram").gameObject.GetComponent<ParticleSystemRenderer>().material.SetColor("_TintColor", unforgivenColor);

            specialEmpoweredSlashingEffect.transform.Find("HologramReturn").gameObject.GetComponent<ParticleSystemRenderer>().material.SetTexture("_RemapTex", Addressables.LoadAssetAsync<Texture>("RoR2/Base/Common/ColorRamps/texRampGolemElectric.png").WaitForCompletion());
            specialEmpoweredSlashingEffect.transform.Find("HologramReturn").gameObject.GetComponent<ParticleSystemRenderer>().material.SetColor("_TintColor", unforgivenColor);

            Modules.Content.CreateAndAddEffectDef(specialEmpoweredSlashingEffect);

            windExplosionEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/ImpBoss/ImpBossBlink.prefab").WaitForCompletion().InstantiateClone("RoughneckBeerExplosion", false);

            Material bloodMat2 = Object.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/Base/Common/VFX/matBloodHumanLarge.mat").WaitForCompletion());
            Material bloodMat4 = Object.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/Base/moon2/matBloodSiphon.mat").WaitForCompletion());

            windExplosionEffect.transform.Find("Particles/LongLifeNoiseTrails").GetComponent<ParticleSystemRenderer>().material = bloodMat2;
            windExplosionEffect.transform.Find("Particles/LongLifeNoiseTrails").GetComponent<ParticleSystemRenderer>().material.SetTexture("_RemapTex", Addressables.LoadAssetAsync<Texture>("RoR2/Base/Common/ColorRamps/texRampHuntressSoft.png").WaitForCompletion());
            windExplosionEffect.transform.Find("Particles/LongLifeNoiseTrails, Bright").GetComponent<ParticleSystemRenderer>().material = bloodMat2;
            windExplosionEffect.transform.Find("Particles/LongLifeNoiseTrails, Bright").GetComponent<ParticleSystemRenderer>().material.SetTexture("_RemapTex", Addressables.LoadAssetAsync<Texture>("RoR2/Base/Common/ColorRamps/texRampHuntressSoft.png").WaitForCompletion());

            windExplosionEffect.transform.Find("Particles/Dash").GetComponent<ParticleSystemRenderer>().material = bloodMat2;
            windExplosionEffect.transform.Find("Particles/Dash").GetComponent<ParticleSystemRenderer>().material.SetTexture("_RemapTex", Addressables.LoadAssetAsync<Texture>("RoR2/Base/Common/ColorRamps/texRampHuntressSoft.png").WaitForCompletion());
            windExplosionEffect.transform.Find("Particles/Dash, Bright").GetComponent<ParticleSystemRenderer>().material = bloodMat2;
            windExplosionEffect.transform.Find("Particles/Dash, Bright").GetComponent<ParticleSystemRenderer>().material.SetTexture("_RemapTex", Addressables.LoadAssetAsync<Texture>("RoR2/Base/Common/ColorRamps/texRampHuntressSoft.png").WaitForCompletion());
            windExplosionEffect.transform.Find("Particles/DashRings").GetComponent<ParticleSystemRenderer>().material = bloodMat4;
            windExplosionEffect.transform.Find("Particles/DashRings").GetComponent<ParticleSystemRenderer>().material.SetColor("_TintColor", Color.cyan);
            windExplosionEffect.GetComponentInChildren<Light>().gameObject.SetActive(false);

            windExplosionEffect.GetComponentInChildren<PostProcessVolume>().sharedProfile = Addressables.LoadAssetAsync<PostProcessProfile>("RoR2/Base/title/ppLocalGold.asset").WaitForCompletion();

            Modules.Content.CreateAndAddEffectDef(windExplosionEffect);

            bloodSpurtEffect = mainAssetBundle.LoadAsset<GameObject>("BloodSpurtEffect");

            bloodSpurtEffect.transform.Find("Blood").GetComponent<ParticleSystemRenderer>().material = bloodMat2;
            bloodSpurtEffect.transform.Find("Trails").GetComponent<ParticleSystemRenderer>().trailMaterial = bloodMat2;

            dashEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Brother/BrotherDashEffect.prefab").WaitForCompletion().InstantiateClone("UnforgivenDashEffect");
            dashEffect.AddComponent<NetworkIdentity>();
            Object.Destroy(dashEffect.transform.Find("Point light").gameObject);
            Object.Destroy(dashEffect.transform.Find("Flash, White").gameObject);
            Object.Destroy(dashEffect.transform.Find("NoiseTrails").gameObject);
            dashEffect.transform.Find("Donut").localScale *= 0.5f;
            dashEffect.transform.Find("Donut, Distortion").localScale *= 0.5f;
            dashEffect.transform.Find("Dash").GetComponent<ParticleSystemRenderer>().material.SetTexture("_RemapTex", Addressables.LoadAssetAsync<Texture>("RoR2/Base/Common/ColorRamps/texRampDefault.png").WaitForCompletion());
            dashEffect.transform.Find("Dash").GetComponent<ParticleSystemRenderer>().material.SetColor("_TintColor", unforgivenColor);
            Modules.Content.CreateAndAddEffectDef(dashEffect);

            unforgivenHitEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Merc/OmniImpactVFXSlashMerc.prefab").WaitForCompletion().InstantiateClone("WandererImpact", false);
            unforgivenHitEffect.AddComponent<NetworkIdentity>();
            unforgivenHitEffect.GetComponent<OmniEffect>().enabled = false;
            Material material = Object.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/Base/Merc/matOmniHitspark3Merc.mat").WaitForCompletion());
            material.SetColor("_TintColor", Color.red);
            unforgivenHitEffect.transform.GetChild(1).gameObject.GetComponent<ParticleSystemRenderer>().material = material;
            unforgivenHitEffect.transform.GetChild(2).localScale = Vector3.one * 1.5f;
            unforgivenHitEffect.transform.GetChild(2).gameObject.GetComponent<ParticleSystemRenderer>().material = Object.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/DLC1/VoidSurvivor/matVoidSurvivorBlasterFireCorrupted.mat").WaitForCompletion());
            material = Object.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/Base/Imp/matImpSlashImpact.mat").WaitForCompletion());
            unforgivenHitEffect.transform.GetChild(5).gameObject.GetComponent<ParticleSystemRenderer>().material = material;
            unforgivenHitEffect.transform.GetChild(4).localScale = Vector3.one * 3f;
            unforgivenHitEffect.transform.GetChild(4).gameObject.GetComponent<ParticleSystemRenderer>().material = Object.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/Base/Imp/matImpDust.mat").WaitForCompletion());
            unforgivenHitEffect.transform.GetChild(6).GetChild(0).gameObject.GetComponent<ParticleSystemRenderer>().material = Object.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/DLC1/Common/Void/matOmniHitspark1Void.mat").WaitForCompletion());
            unforgivenHitEffect.transform.GetChild(6).gameObject.GetComponent<ParticleSystemRenderer>().material = Object.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/DLC1/Common/Void/matOmniHitspark2Void.mat").WaitForCompletion());
            unforgivenHitEffect.transform.GetChild(1).localScale = Vector3.one * 1.5f;
            unforgivenHitEffect.transform.GetChild(1).gameObject.SetActive(true);
            unforgivenHitEffect.transform.GetChild(2).gameObject.SetActive(true);
            unforgivenHitEffect.transform.GetChild(3).gameObject.SetActive(true);
            unforgivenHitEffect.transform.GetChild(4).gameObject.SetActive(true);
            unforgivenHitEffect.transform.GetChild(5).gameObject.SetActive(true);
            unforgivenHitEffect.transform.GetChild(6).gameObject.SetActive(true);
            unforgivenHitEffect.transform.GetChild(6).GetChild(0).gameObject.SetActive(true);
            unforgivenHitEffect.transform.GetChild(6).transform.localScale = new Vector3(1f, 1f, 3f);
            unforgivenHitEffect.transform.localScale = Vector3.one * 1.5f;
            Modules.Content.CreateAndAddEffectDef(unforgivenHitEffect);

            batHitEffectRed = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Bandit2/HitsparkBandit.prefab").WaitForCompletion().InstantiateClone("InterreogatorBatRedHitEffect");
            batHitEffectRed.AddComponent<NetworkIdentity>();
            batHitEffectRed.transform.Find("Particles").Find("TriangleSparksLarge").gameObject.GetComponent<ParticleSystemRenderer>().material.SetColor("_TintColor", Color.red);
            batHitEffectRed.transform.Find("Particles").Find("TriangleSparks").gameObject.GetComponent<ParticleSystemRenderer>().material.SetColor("_TintColor", Color.red);
            Modules.Content.CreateAndAddEffectDef(batHitEffectRed);

            swordSwingEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Merc/MercSwordSlash.prefab").WaitForCompletion().InstantiateClone("UnforgivenSwordSwing", false);
            swordSwingEffect.transform.GetChild(0).localScale *= 1.5f;
            swordSwingEffect.transform.GetChild(0).GetComponent<ParticleSystemRenderer>().material.SetColor("_TintColor", Color.red);
            var swing = swordSwingEffect.transform.GetChild(0).GetComponent<ParticleSystem>().main;
            swing.startLifetimeMultiplier *= 2f;

            swordSwingEmpoweredEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Merc/MercSwordSlash.prefab").WaitForCompletion().InstantiateClone("UnforgivenSwordEmpoweredSwing", false);
            swordSwingEmpoweredEffect.transform.GetChild(0).localScale *= 1.5f;
            swordSwingEmpoweredEffect.transform.GetChild(0).GetComponent<ParticleSystemRenderer>().material = Addressables.LoadAssetAsync<Material>("RoR2/Base/Imp/matImpSwipe.mat").WaitForCompletion();
            swordSwingEmpoweredEffect.transform.GetChild(0).GetComponent<ParticleSystemRenderer>().material.SetColor("_TintColor", unforgivenColor);
            swing = swordSwingEmpoweredEffect.transform.GetChild(0).GetComponent<ParticleSystem>().main;
            swing.startLifetimeMultiplier *= 2f;


            stabSwingEffect = mainAssetBundle.LoadEffect("SecondaryThrust", false);
            Component.Destroy(stabSwingEffect.GetComponent<EffectComponent>());

            bloodSplatterEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Brother/BrotherSlamImpact.prefab").WaitForCompletion().InstantiateClone("UnforgivenSplat", true);
            bloodSplatterEffect.AddComponent<NetworkIdentity>();
            bloodSplatterEffect.transform.GetChild(0).gameObject.SetActive(false);
            bloodSplatterEffect.transform.GetChild(1).gameObject.SetActive(false);
            bloodSplatterEffect.transform.GetChild(2).gameObject.SetActive(false);
            bloodSplatterEffect.transform.GetChild(3).gameObject.SetActive(false);
            bloodSplatterEffect.transform.GetChild(4).gameObject.SetActive(false);
            bloodSplatterEffect.transform.GetChild(5).gameObject.SetActive(false);
            bloodSplatterEffect.transform.GetChild(6).gameObject.SetActive(false);
            bloodSplatterEffect.transform.GetChild(7).gameObject.SetActive(false);
            bloodSplatterEffect.transform.GetChild(8).gameObject.SetActive(false);
            bloodSplatterEffect.transform.GetChild(9).gameObject.SetActive(false);
            bloodSplatterEffect.transform.GetChild(10).gameObject.SetActive(false);
            bloodSplatterEffect.transform.Find("Decal").GetComponent<Decal>().Material = Addressables.LoadAssetAsync<Material>("RoR2/Base/Imp/matImpDecal.mat").WaitForCompletion();
            bloodSplatterEffect.transform.Find("Decal").GetComponent<AnimateShaderAlpha>().timeMax = 10f;
            bloodSplatterEffect.transform.GetChild(12).gameObject.SetActive(false);
            bloodSplatterEffect.transform.GetChild(13).gameObject.SetActive(false);
            bloodSplatterEffect.transform.GetChild(14).gameObject.SetActive(false);
            bloodSplatterEffect.transform.GetChild(15).gameObject.SetActive(false);
            bloodSplatterEffect.transform.localScale = Vector3.one;
            UnforgivenMod.Modules.Content.CreateAndAddEffectDef(bloodSplatterEffect);
        }

        #endregion

        #region projectiles
        private static void CreateProjectiles()
        {
            nadoPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Lemurian/Fireball.prefab").WaitForCompletion().InstantiateClone("UnforgivenTornadoProjectile");
            if(!nadoPrefab.GetComponent<NetworkIdentity>()) nadoPrefab.AddComponent<NetworkIdentity>();

            nadoPrefab.AddComponent<DamageAPI.ModdedDamageTypeHolderComponent>().Add(DamageTypes.KnockAirborne);

            nadoPrefab.transform.localScale = new Vector3(1, 1, 1);
            GameObject.Destroy(nadoPrefab.GetComponent<SphereCollider>());
            GameObject.Destroy(nadoPrefab.GetComponent<ProjectileSingleTargetImpact>());

            ProjectileSimple nadoSimple = nadoPrefab.GetComponent<ProjectileSimple>();
            nadoSimple.lifetime = 1.25f;
            nadoSimple.desiredForwardSpeed = 50f;
            nadoSimple.enableVelocityOverLifetime = true;
            nadoSimple.velocityOverLifetime = new AnimationCurve(new Keyframe[] { new Keyframe(0f, 1f), new Keyframe(0.85f, 0.1f), new Keyframe(1f, 0f) });

            GameObject hitbox = new GameObject("NadoHitbox");
            hitbox.transform.parent = nadoPrefab.transform;
            hitbox.transform.localScale = new Vector3(13f, 13f, 13f);
            hitbox.transform.localPosition = new Vector3(0, 0f, 0);

            HitBox[] nadoHitbox = new HitBox[] { hitbox.AddComponent<HitBox>() };
            nadoPrefab.AddComponent<HitBoxGroup>().hitBoxes = nadoHitbox;
            ProjectileOverlapAttack nadoOverlapAttack = nadoPrefab.AddComponent<ProjectileOverlapAttack>();
            nadoOverlapAttack.damageCoefficient = 1f;
            nadoOverlapAttack.forceVector = Vector3.up * 4000f;
            nadoOverlapAttack.impactEffect = unforgivenHitEffect;

            ProjectileController nadoController = nadoPrefab.GetComponent<ProjectileController>();
            nadoController.allowPrediction = true;

            nadoGhost = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Vulture/WindbladeProjectileGhost.prefab").WaitForCompletion().InstantiateClone("UnforgivenNadoGhost");
            if(!nadoGhost.GetComponent<NetworkIdentity>()) nadoGhost.AddComponent<NetworkIdentity>();

            ParticleSystemRenderer baseNadoRend = nadoGhost.transform.Find("Holder").Find("Base").gameObject.GetComponent<ParticleSystemRenderer>();
            
            baseNadoRend.material.SetColor("_TintColor", new Color(255f / 255f, 16f / 255f, 16f / 255f));

            realNado = mainAssetBundle.LoadAsset<GameObject>("UnforgivenTornado");
            realNado.GetComponent<ParticleSystemRenderer>().material = baseNadoRend.material;
            realNado.GetComponent<ParticleSystemRenderer>().mesh = baseNadoRend.mesh;
            realNado.transform.SetParent(nadoGhost.transform.Find("Holder"));

            spinNadoEffect = mainAssetBundle.LoadAsset<GameObject>("UnforgivenTornado").InstantiateClone("UnforgivenSpinTornado");
            spinNadoEffect.AddComponent<NetworkIdentity>();
            spinNadoEffect.transform.rotation = new Quaternion(0f, Quaternion.identity.y, Quaternion.identity.z, Quaternion.identity.w);
            var main2 = spinNadoEffect.GetComponent<ParticleSystem>().main;
            main2.loop = false;
            main2.duration = 0.5f;
            spinNadoEffect.GetComponent<ParticleSystemRenderer>().material = baseNadoRend.material;
            spinNadoEffect.GetComponent<ParticleSystemRenderer>().mesh = baseNadoRend.mesh;

            spinNadoEffect.AddComponent<EffectComponent>();
            spinNadoEffect.GetComponent<EffectComponent>().positionAtReferencedTransform = true;

            Modules.Content.CreateAndAddEffectDef(spinNadoEffect);

            GameObject.Destroy(nadoGhost.transform.Find("Holder").Find("Base").gameObject);
            Component.Destroy(nadoGhost.transform.Find("Holder").gameObject.GetComponent<RotateAroundAxis>());

            nadoController.ghostPrefab = nadoGhost;

            Modules.Content.AddProjectilePrefab(nadoPrefab);
        }
        #endregion

        #region sounds
        private static void CreateSounds()
        {
            swordImpactSoundEvent = Modules.Content.CreateAndAddNetworkSoundEventDef("Play_merc_sword_impact");
            stabImpactSoundEvent = Modules.Content.CreateAndAddNetworkSoundEventDef("sfx_unforgiven_stab_impact");
            nadoImpactSoundEvent = Modules.Content.CreateAndAddNetworkSoundEventDef("sfx_unforgiven_nado_impact");
        }
        #endregion

        private static void CreateUI()
        {
            throwable = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/BasicThrowableVisualizer.prefab").WaitForCompletion();
            throwableEnd = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Huntress/HuntressArrowRainIndicator.prefab").WaitForCompletion();

            secondaryIcon = mainAssetBundle.LoadAsset<Sprite>("texSecondaryIcon");
            secondaryEmpoweredIcon = mainAssetBundle.LoadAsset<Sprite>("texSecondaryEmpoweredIcon");

            unforgivenIndicator = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Huntress/HuntressTrackingIndicator.prefab").WaitForCompletion().InstantiateClone("UnforgivenTracker", false);
            Material component = Object.Instantiate(Addressables.LoadAssetAsync<Material>("RoR2/Base/UI/matUIOverbrighten2x.mat").WaitForCompletion());
            Object.DestroyImmediate(unforgivenIndicator.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>());
            SpriteRenderer balls = unforgivenIndicator.transform.GetChild(0).gameObject.AddComponent<SpriteRenderer>();
            balls.SetMaterial(component);
            balls.sprite = mainAssetBundle.LoadAsset<Sprite>("texUnforgivenIndicator");
            unforgivenIndicator.transform.GetChild(1).gameObject.SetActive(false);
            Sprite sprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/UI/texCrosshair2.png").WaitForCompletion();
            Material component2 = unforgivenIndicator.transform.GetChild(2).gameObject.GetComponent<SpriteRenderer>().material;
            Object.DestroyImmediate(unforgivenIndicator.transform.GetChild(2).gameObject.GetComponent<SpriteRenderer>());
            SpriteRenderer balls2 = unforgivenIndicator.transform.GetChild(2).gameObject.AddComponent<SpriteRenderer>();
            balls2.SetMaterial(component2);
            balls2.sprite = sprite;
            balls2.color = unforgivenColor;
        }

        #region helpers
        private static GameObject CreateImpactExplosionEffect(string effectName, Material bloodMat, Material decal, float scale = 1f)
        {
            GameObject newEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Brother/BrotherSlamImpact.prefab").WaitForCompletion().InstantiateClone(effectName, true);

            newEffect.transform.Find("Spikes, Small").gameObject.SetActive(false);

            newEffect.transform.Find("PP").gameObject.SetActive(false);
            newEffect.transform.Find("Point light").gameObject.SetActive(false);
            newEffect.transform.Find("Flash Lines").GetComponent<ParticleSystemRenderer>().material = Addressables.LoadAssetAsync<Material>("RoR2/Base/Common/VFX/matOpaqueDustLargeDirectional.mat").WaitForCompletion();

            newEffect.transform.GetChild(3).GetComponent<ParticleSystemRenderer>().material = bloodMat;
            newEffect.transform.Find("Flash Lines, Fire").GetComponent<ParticleSystemRenderer>().material = bloodMat;
            newEffect.transform.GetChild(6).GetComponent<ParticleSystemRenderer>().material = bloodMat;
            newEffect.transform.Find("Fire").GetComponent<ParticleSystemRenderer>().material = bloodMat;

            var boom = newEffect.transform.Find("Fire").GetComponent<ParticleSystem>().main;
            boom.startLifetimeMultiplier = 0.5f;
            boom = newEffect.transform.Find("Flash Lines, Fire").GetComponent<ParticleSystem>().main;
            boom.startLifetimeMultiplier = 0.3f;
            boom = newEffect.transform.GetChild(6).GetComponent<ParticleSystem>().main;
            boom.startLifetimeMultiplier = 0.4f;

            newEffect.transform.Find("Physics").GetComponent<ParticleSystemRenderer>().material = Addressables.LoadAssetAsync<Material>("RoR2/Base/MagmaWorm/matFracturedGround.mat").WaitForCompletion();

            newEffect.transform.Find("Decal").GetComponent<Decal>().Material = decal;
            newEffect.transform.Find("Decal").GetComponent<AnimateShaderAlpha>().timeMax = 10f;

            newEffect.transform.Find("FoamSplash").gameObject.SetActive(false);
            newEffect.transform.Find("FoamBilllboard").gameObject.SetActive(false);
            newEffect.transform.Find("Dust").gameObject.SetActive(false);
            newEffect.transform.Find("Dust, Directional").gameObject.SetActive(false);

            newEffect.transform.localScale = Vector3.one * scale;

            newEffect.AddComponent<NetworkIdentity>();

            ParticleSystemColorFromEffectData PSCFED = newEffect.AddComponent<ParticleSystemColorFromEffectData>();
            PSCFED.particleSystems = new ParticleSystem[]
            {
                newEffect.transform.Find("Fire").GetComponent<ParticleSystem>(),
                newEffect.transform.Find("Flash Lines, Fire").GetComponent<ParticleSystem>(),
                newEffect.transform.GetChild(6).GetComponent<ParticleSystem>(),
                newEffect.transform.GetChild(3).GetComponent<ParticleSystem>()
            };
            PSCFED.effectComponent = newEffect.GetComponent<EffectComponent>();

            UnforgivenMod.Modules.Content.CreateAndAddEffectDef(newEffect);

            return newEffect;
        }
        public static Material CreateMaterial(string materialName, float emission, Color emissionColor, float normalStrength)
        {
            if (!commandoMat) commandoMat = Resources.Load<GameObject>("Prefabs/CharacterBodies/CommandoBody").GetComponentInChildren<CharacterModel>().baseRendererInfos[0].defaultMaterial;

            Material mat = UnityEngine.Object.Instantiate<Material>(commandoMat);
            Material tempMat = mainAssetBundle.LoadAsset<Material>(materialName);

            if (!tempMat) return commandoMat;

            mat.name = materialName;
            mat.SetColor("_Color", tempMat.GetColor("_Color"));
            mat.SetTexture("_MainTex", tempMat.GetTexture("_MainTex"));
            mat.SetColor("_EmColor", emissionColor);
            mat.SetFloat("_EmPower", emission);
            mat.SetTexture("_EmTex", tempMat.GetTexture("_EmissionMap"));
            mat.SetFloat("_NormalStrength", normalStrength);

            return mat;
        }

        public static Material CreateMaterial(string materialName)
        {
            return CreateMaterial(materialName, 0f);
        }

        public static Material CreateMaterial(string materialName, float emission)
        {
            return CreateMaterial(materialName, emission, Color.black);
        }

        public static Material CreateMaterial(string materialName, float emission, Color emissionColor)
        {
            return CreateMaterial(materialName, emission, emissionColor, 0f);
        }
        #endregion
    }
}