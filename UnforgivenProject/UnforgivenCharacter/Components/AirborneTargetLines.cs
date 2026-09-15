using System;
using System.Collections.Generic;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Rendering;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace UnforgivenMod.Unforgiven.Components
{
    public sealed class AirborneTargetLines : IDisposable
    {
        private const string MaterialAddress = "RoR2/Base/Common/VFX/matTracerBrightTransparent.mat";
        private const string ShaderName = "Hopoo Games/FX/Cloud Remap";
        private const float DashPeriod = 1.5f;
        private const float DashLength = 0.75f;
        private const float LineHalfWidth = 0.03f;
        private const int MaxDashesPerTrail = 64;
        private static readonly Color32 LineColor = new Color32(160, 210, 225, 128);

        private readonly List<Vector3> vertices = new List<Vector3>();
        private readonly List<Color32> colors = new List<Color32>();
        private readonly List<Vector2> uvs = new List<Vector2>();
        private readonly List<int> triangles = new List<int>();
        private AsyncOperationHandle<Material> materialHandle;
        private Material material;
        private Mesh mesh;
        private bool disposed;

        public bool IsValid => !disposed && material && mesh;

        public AirborneTargetLines()
        {
            if (!SystemInfo.supports32bitsIndexBuffer)
            {
                Log.Error("Airborne target lines require 32-bit mesh indices.");
                Dispose();
                return;
            }

            materialHandle = Addressables.LoadAssetAsync<Material>(MaterialAddress);
            Material source = materialHandle.WaitForCompletion();
            if (materialHandle.Status != AsyncOperationStatus.Succeeded || !source)
            {
                Log.Error("Airborne target lines could not load " + MaterialAddress + ".");
                Dispose();
                return;
            }

            if (!source.shader || source.shader.name != ShaderName || !source.shader.isSupported)
            {
                Log.Error("Airborne target lines require the supported game shader " + ShaderName + ".");
                Dispose();
                return;
            }

            string[] requiredProperties =
            {
                "_SrcBlend", "_DstBlend", "_ZTest", "_Cull", "_MainTex", "_TintColor",
                "_DisableRemapOn", "_VertexColorOn", "_Boost", "_AlphaBoost",
                "_ExternalAlpha", "_Fade", "_CloudsOn", "_VertexOffsetOn"
            };
            for (int i = 0; i < requiredProperties.Length; i++)
            {
                if (!source.HasProperty(requiredProperties[i]))
                {
                    Log.Error("Airborne target line material is missing " + requiredProperties[i] + ".");
                    Dispose();
                    return;
                }
            }

            material = new Material(source)
            {
                name = "UnforgivenAirborneTargetLineMaterial",
                hideFlags = HideFlags.HideAndDontSave,
                renderQueue = (int)RenderQueue.Transparent,
                enableInstancing = false,
                shaderKeywords = new[] { "DISABLEREMAP", "VERTEXCOLOR" }
            };
            material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            material.SetFloat("_ZTest", (float)CompareFunction.LessEqual);
            material.SetFloat("_Cull", (float)CullMode.Off);
            if (material.HasProperty("_ZWrite"))
            {
                material.SetFloat("_ZWrite", 0f);
            }
            material.SetTexture("_MainTex", Texture2D.whiteTexture);
            material.SetTextureScale("_MainTex", Vector2.one);
            material.SetTextureOffset("_MainTex", Vector2.zero);
            material.SetColor("_TintColor", Color.white);
            material.SetFloat("_DisableRemapOn", 1f);
            material.SetFloat("_VertexColorOn", 1f);
            material.SetFloat("_CloudsOn", 0f);
            material.SetFloat("_VertexOffsetOn", 0f);
            material.SetFloat("_Boost", 1f);
            material.SetFloat("_AlphaBoost", 1f);
            material.SetFloat("_ExternalAlpha", 1f);
            material.SetFloat("_Fade", 1f);

            mesh = new Mesh
            {
                name = "UnforgivenAirborneTargetLineMesh",
                hideFlags = HideFlags.HideAndDontSave,
                indexFormat = IndexFormat.UInt32
            };
            mesh.MarkDynamic();
        }

        public void Draw(Vector3 origin, IReadOnlyList<CharacterBody> targets, Camera camera)
        {
            if (!IsValid || !camera || !camera.isActiveAndEnabled || targets == null || !IsFinite(origin))
            {
                return;
            }

            vertices.Clear();
            colors.Clear();
            uvs.Clear();
            triangles.Clear();

            Transform cameraTransform = camera.transform;
            Vector3 cameraOffset = cameraTransform.position - origin;
            Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
            for (int targetIndex = 0; targetIndex < targets.Count; targetIndex++)
            {
                CharacterBody target = targets[targetIndex];
                if (!target)
                {
                    continue;
                }

                Vector3 offset = target.corePosition - origin;
                if (!IsFinite(offset))
                {
                    continue;
                }
                float distance = offset.magnitude;
                if (!IsFinite(distance) || distance < 0.001f)
                {
                    continue;
                }

                Vector3 direction = offset / distance;
                Vector3 side = Vector3.Cross(direction, cameraOffset);
                if (side.sqrMagnitude < 0.0001f)
                {
                    side = Vector3.Cross(direction, cameraTransform.up);
                }
                if (side.sqrMagnitude < 0.0001f)
                {
                    side = Vector3.Cross(direction, cameraTransform.right);
                }
                side.Normalize();
                Vector3 halfWidth = side * LineHalfWidth;
                int dashCount = Mathf.Max(1, Mathf.CeilToInt(Mathf.Min(distance / DashPeriod, MaxDashesPerTrail)));
                float dashLength = dashCount == 1 ? distance : Mathf.Min(DashLength, distance / (2 * dashCount - 1));
                float step = dashCount == 1 ? 0f : (distance - dashLength) / (dashCount - 1);
                for (int dashIndex = 0; dashIndex < dashCount; dashIndex++)
                {
                    float along = dashIndex * step;
                    AddDash(direction * along, direction * Mathf.Min(distance, along + dashLength), halfWidth);
                }
                bounds.Encapsulate(offset);
            }

            if (vertices.Count == 0)
            {
                return;
            }

            mesh.Clear();
            mesh.SetVertices(vertices);
            mesh.SetColors(colors);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(triangles, 0, false);
            bounds.Expand(2f * LineHalfWidth);
            mesh.bounds = bounds;
            Graphics.DrawMesh(mesh, Matrix4x4.Translate(origin), material, LayerIndex.defaultLayer.intVal,
                camera, 0, null, ShadowCastingMode.Off, false, null, LightProbeUsage.Off);
        }

        private void AddDash(Vector3 start, Vector3 end, Vector3 halfWidth)
        {
            int first = vertices.Count;
            vertices.Add(start - halfWidth);
            vertices.Add(start + halfWidth);
            vertices.Add(end + halfWidth);
            vertices.Add(end - halfWidth);
            for (int i = 0; i < 4; i++)
            {
                colors.Add(LineColor);
            }
            uvs.Add(new Vector2(0f, 0f));
            uvs.Add(new Vector2(0f, 1f));
            uvs.Add(new Vector2(1f, 1f));
            uvs.Add(new Vector2(1f, 0f));
            triangles.Add(first);
            triangles.Add(first + 1);
            triangles.Add(first + 2);
            triangles.Add(first);
            triangles.Add(first + 2);
            triangles.Add(first + 3);
        }

        private static bool IsFinite(Vector3 value)
        {
            return IsFinite(value.x) && IsFinite(value.y) && IsFinite(value.z);
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }
            disposed = true;
            if (mesh)
            {
                UnityEngine.Object.Destroy(mesh);
                mesh = null;
            }
            if (material)
            {
                UnityEngine.Object.Destroy(material);
                material = null;
            }
            if (materialHandle.IsValid())
            {
                Addressables.Release(materialHandle);
            }
        }
    }
}
