using System.Collections.Generic;
using R2API;
using RoR2;
using RoR2.Projectile;
using ThreeEyedGames;
using UnityEngine;
using UnityEngine.Rendering;

namespace UnforgivenMod.Unforgiven.Content
{
    internal static class UnforgivenSkinVFX
    {
        private static readonly Dictionary<GameObject, GameObject> replacements = new Dictionary<GameObject, GameObject>();
        private static readonly Dictionary<Material, Material> materials = new Dictionary<Material, Material>();
        private static readonly Dictionary<(Texture, bool), Texture> textures = new Dictionary<(Texture, bool), Texture>();
        private static readonly string[] textureProperties =
            { "_RemapTex", "_MainTex", "_EmTex", "_FresnelRamp", "_PrintRamp", "_FlowHeightRamp" };
        private static SkinDef monsoonSkin;
        private static float monsoonHue;

        internal static Material specialMaterial { get; private set; }

        internal static void Init(SkinDef skin)
        {
            monsoonSkin = skin;
            Color.RGBToHSV(UnforgivenAssets.ascendentColor, out monsoonHue, out _, out _);
            GameObject[] effects =
            {
                UnforgivenAssets.spinSlashEffect, UnforgivenAssets.spinEmpoweredSlashEffect,
                UnforgivenAssets.airbladeSlashEffect, UnforgivenAssets.airbladeEmpoweredSlashEffect,
                UnforgivenAssets.airbladeNadoEffect, UnforgivenAssets.specialSlashingEffect,
                UnforgivenAssets.specialEmpoweredSlashingEffect, UnforgivenAssets.swordSwingEffect,
                UnforgivenAssets.swordSwingEmpoweredEffect, UnforgivenAssets.stabSwingEffect,
                UnforgivenAssets.unforgivenHitEffect, UnforgivenAssets.windExplosionEffect,
                UnforgivenAssets.dashEffect, UnforgivenAssets.spinNadoEffect,
                UnforgivenAssets.bloodSplatterEffect, UnforgivenAssets.bloodSpurtEffect,
                UnforgivenAssets.batHitEffectRed, UnforgivenAssets.shieldEffect,
                UnforgivenAssets.nadoUpEffect, UnforgivenAssets.nadoGhost,
                UnforgivenAssets.unforgivenIndicator
            };
            foreach (GameObject effect in effects)
            {
                GameObject replacement = effect.InstantiateClone(effect.name + "Monsoon", false);
                TintEffect(replacement);
                replacements.Add(effect, replacement);
                if (replacement.GetComponent<EffectComponent>())
                {
                    Modules.Content.CreateAndAddEffectDef(replacement);
                }
            }

            specialMaterial = TintMaterial(UnforgivenAssets.specialMaterial);
            GameObject tornado = UnforgivenAssets.nadoPrefab.InstantiateClone("UnforgivenMonsoonTornadoProjectile");
            tornado.GetComponent<ProjectileController>().ghostPrefab = replacements[UnforgivenAssets.nadoGhost];
            tornado.GetComponent<ProjectileOverlapAttack>().impactEffect = replacements[UnforgivenAssets.unforgivenHitEffect];
            replacements.Add(UnforgivenAssets.nadoPrefab, tornado);
            Modules.Content.AddProjectilePrefab(tornado);
        }

        internal static bool IsMonsoon(GameObject owner)
        {
            if (!owner || !monsoonSkin)
            {
                return false;
            }
            ModelLocator locator = owner.GetComponent<ModelLocator>();
            ModelSkinController controller = locator && locator.modelTransform
                ? locator.modelTransform.GetComponent<ModelSkinController>()
                : owner.GetComponentInChildren<ModelSkinController>();
            if (!controller || controller.skins == null)
            {
                return false;
            }
            int skinIndex = controller.currentSkinIndex;
            return skinIndex >= 0 && skinIndex < controller.skins.Length &&
                controller.skins[skinIndex] == monsoonSkin;
        }

        internal static GameObject GetEffect(GameObject effect, GameObject owner)
        {
            return effect && IsMonsoon(owner) && replacements.TryGetValue(effect, out GameObject replacement)
                ? replacement : effect;
        }

        internal static GameObject GetMonsoonEffect(GameObject effect)
        {
            return replacements[effect];
        }

        private static Color TintColor(Color color)
        {
            Color.RGBToHSV(color, out _, out float saturation, out float value);
            if (saturation == 0f)
            {
                return color;
            }
            Color tinted = Color.HSVToRGB(monsoonHue, saturation, value, true);
            tinted.a = color.a;
            return tinted;
        }

        private static Color GrayscaleColor(Color color)
        {
            float value = Mathf.Max(color.r, Mathf.Max(color.g, color.b));
            return new Color(value, value, value, color.a);
        }

        private static Gradient TintGradient(Gradient source)
        {
            GradientColorKey[] keys = source.colorKeys;
            for (int i = 0; i < keys.Length; i++)
            {
                keys[i].color = GrayscaleColor(keys[i].color);
            }
            Gradient gradient = new Gradient { mode = source.mode };
            gradient.SetKeys(keys, source.alphaKeys);
            return gradient;
        }

        private static ParticleSystem.MinMaxGradient TintGradient(ParticleSystem.MinMaxGradient source)
        {
            ParticleSystemGradientMode mode = source.mode;
            source.colorMin = GrayscaleColor(source.colorMin);
            source.colorMax = GrayscaleColor(source.colorMax);
            if (mode == ParticleSystemGradientMode.Gradient || mode == ParticleSystemGradientMode.TwoGradients ||
                mode == ParticleSystemGradientMode.RandomColor)
            {
                source.gradientMax = TintGradient(source.gradientMax);
                if (mode == ParticleSystemGradientMode.TwoGradients)
                {
                    source.gradientMin = TintGradient(source.gradientMin);
                }
            }
            source.mode = mode;
            return source;
        }

        private static Material TintMaterial(Material source)
        {
            if (!source)
            {
                return source;
            }
            if (materials.TryGetValue(source, out Material cached))
            {
                return cached;
            }
            Material material = new Material(source) { name = source.name + "Monsoon" };
            Shader shader = material.shader;
            bool hasColorProperty = false;
            for (int i = 0; i < shader.GetPropertyCount(); i++)
            {
                if (shader.GetPropertyType(i) == ShaderPropertyType.Color)
                {
                    hasColorProperty = true;
                    string property = shader.GetPropertyName(i);
                    Color color = material.GetColor(property);
                    Color tinted = TintColor(color);
                    if (property == "_TintColor" || property == "_Color")
                    {
                        float value = Mathf.Max(color.r, Mathf.Max(color.g, color.b));
                        tinted = Color.HSVToRGB(monsoonHue, 1f, value, true);
                        tinted.a = color.a;
                    }
                    material.SetColor(property, tinted);
                }
            }
            foreach (string property in textureProperties)
            {
                if (material.HasProperty(property) && material.GetTexture(property) is Texture2D texture)
                {
                    material.SetTexture(property, TintTexture(texture, hasColorProperty));
                }
            }
            materials.Add(source, material);
            return material;
        }

        private static Texture TintTexture(Texture2D source, bool grayscale)
        {
            var key = ((Texture)source, grayscale);
            if (textures.TryGetValue(key, out Texture cached))
            {
                return cached;
            }
            if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null)
            {
                return source;
            }
            // Color ramps can contain baked red and may not be CPU-readable.
            RenderTexture previous = RenderTexture.active;
            RenderTexture temporary = RenderTexture.GetTemporary(source.width, source.height, 0,
                RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            Texture2D texture = new Texture2D(source.width, source.height, TextureFormat.RGBAFloat,
                source.mipmapCount > 1, true);
            try
            {
                Graphics.Blit(source, temporary);
                RenderTexture.active = temporary;
                texture.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0);
            }
            finally
            {
                RenderTexture.active = previous;
                RenderTexture.ReleaseTemporary(temporary);
            }
            Color[] pixels = texture.GetPixels();
            bool changed = false;
            for (int i = 0; i < pixels.Length; i++)
            {
                Color tinted = grayscale ? GrayscaleColor(pixels[i]) : TintColor(pixels[i]);
                changed |= tinted != pixels[i];
                pixels[i] = tinted;
            }
            if (!changed)
            {
                Object.Destroy(texture);
                textures.Add(key, source);
                return source;
            }
            texture.name = source.name + "Monsoon";
            texture.wrapMode = source.wrapMode;
            texture.filterMode = source.filterMode;
            texture.anisoLevel = source.anisoLevel;
            texture.SetPixels(pixels);
            texture.Apply(true, true);
            textures.Add(key, texture);
            return texture;
        }

        private static void TintEffect(GameObject effect)
        {
            foreach (Renderer renderer in effect.GetComponentsInChildren<Renderer>(true))
            {
                Material[] rendererMaterials = renderer.sharedMaterials;
                for (int i = 0; i < rendererMaterials.Length; i++)
                {
                    rendererMaterials[i] = TintMaterial(rendererMaterials[i]);
                }
                renderer.sharedMaterials = rendererMaterials;
                if (renderer is ParticleSystemRenderer particles)
                {
                    particles.trailMaterial = TintMaterial(particles.trailMaterial);
                }
                if (renderer is TrailRenderer trail)
                {
                    trail.colorGradient = TintGradient(trail.colorGradient);
                }
                if (renderer is LineRenderer line)
                {
                    line.colorGradient = TintGradient(line.colorGradient);
                }
                if (renderer is SpriteRenderer sprite)
                {
                    sprite.color = GrayscaleColor(sprite.color);
                }
            }
            foreach (ParticleSystem particles in effect.GetComponentsInChildren<ParticleSystem>(true))
            {
                // Apply orange once in the material, not once per multiplied color factor.
                var main = particles.main;
                main.startColor = TintGradient(main.startColor);
                var lifetime = particles.colorOverLifetime;
                if (lifetime.enabled) lifetime.color = TintGradient(lifetime.color);
                var speed = particles.colorBySpeed;
                if (speed.enabled) speed.color = TintGradient(speed.color);
                var trails = particles.trails;
                if (trails.enabled)
                {
                    trails.colorOverLifetime = TintGradient(trails.colorOverLifetime);
                    trails.colorOverTrail = TintGradient(trails.colorOverTrail);
                }
            }
            foreach (Light light in effect.GetComponentsInChildren<Light>(true))
            {
                light.color = TintColor(light.color);
            }
            foreach (Decal decal in effect.GetComponentsInChildren<Decal>(true))
            {
                decal.Material = TintMaterial(decal.Material);
            }
        }
    }
}
