// <copyright file="Patcher.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace EightyOne2
{
    using System.Reflection;
    using AlgernonCommons;
    using AlgernonCommons.Patching;
    using ColossalFramework;
    using HarmonyLib;
    using UnityEngine;
    using static Patches.DistrictManagerPatches;
    using static Patches.ElectricityManagerPatches;
    using static Patches.ImmaterialResourceManagerPatches;
    using static Patches.WaterManagerPatches;

    /// <summary>
    /// Handes mod patching.
    /// </summary>
    public sealed class Patcher : PatcherBase
    {
        /// <summary>
        /// Peforms any additional actions (such as custom patching) after PatchAll is called.
        /// Used here to perform post-Awake corrections for any managers instantiated before patching.
        /// </summary>
        /// <param name="harmonyInstance">Harmony instance for patching.</param>
        protected override void OnPatchAll(Harmony harmonyInstance)
        {
            base.OnPatchAll(harmonyInstance);

            // If any of the managers have already been instantiated, we need to replace their overlay textures.
            Logging.Message("checking for manager instantiation");

            // District manager.
            if (Singleton<DistrictManager>.exists)
            {
                Logging.Message("DistrictManager already instantiated");

                // Get district texture instance.
                DistrictManager districtManager = Singleton<DistrictManager>.instance;
                FieldInfo m_districtTexture1 = AccessTools.Field(typeof(DistrictManager), "m_districtTexture1");
                Texture2D districtTexture1 = m_districtTexture1.GetValue(districtManager) as Texture2D;

                // Check texture size.
                if (districtTexture1.width != ExpandedDistrictGridResolution)
                {
                    Logging.Error("invalid DistrictManager texture size (conflicting mod?); correcting");

                    // Texture size is not expanded - reset per DistrictManager.Awake.
                    m_districtTexture1.SetValue(
                        districtManager,
                        new Texture2D(ExpandedDistrictGridResolution, ExpandedDistrictGridResolution, TextureFormat.ARGB32, mipmap: false, linear: true)
                        {
                            wrapMode = TextureWrapMode.Clamp,
                        });

                    AccessTools.Field(typeof(DistrictManager), "m_districtTexture2").SetValue(
                        districtManager,
                        new Texture2D(ExpandedDistrictGridResolution, ExpandedDistrictGridResolution, TextureFormat.ARGB32, mipmap: false, linear: true)
                        {
                            wrapMode = TextureWrapMode.Clamp,
                        });

                    AccessTools.Field(typeof(DistrictManager), "m_parkTexture1").SetValue(
                        districtManager,
                        new Texture2D(ExpandedDistrictGridResolution, ExpandedDistrictGridResolution, TextureFormat.ARGB32, mipmap: false, linear: true)
                        {
                            wrapMode = TextureWrapMode.Clamp,
                        });

                    AccessTools.Field(typeof(DistrictManager), "m_parkTexture2").SetValue(
                        districtManager,
                        new Texture2D(ExpandedDistrictGridResolution, ExpandedDistrictGridResolution, TextureFormat.ARGB32, mipmap: false, linear: true)
                        {
                            wrapMode = TextureWrapMode.Clamp,
                        });

                    // Set ColorBuffer array.
                    AccessTools.Field(typeof(DistrictManager), "m_colorBuffer").SetValue(districtManager, new Color32[ExpandedDistrictGridArraySize]);

                    // Set initial modified area.
                    AccessTools.Field(typeof(DistrictManager), "m_districtsModifiedX2").SetValue(districtManager, ExpandedDistrictGridMax);
                    AccessTools.Field(typeof(DistrictManager), "m_districtsModifiedZ2").SetValue(districtManager, ExpandedDistrictGridMax);
                    AccessTools.Field(typeof(DistrictManager), "m_parksModifiedX2").SetValue(districtManager, ExpandedDistrictGridMax);
                    AccessTools.Field(typeof(DistrictManager), "m_parksModifiedZ2").SetValue(districtManager, ExpandedDistrictGridMax);
                }
            }
            else
            {
                Logging.Message("DistrictManager not yet instantiated");
            }

            if (Singleton<ElectricityManager>.exists)
            {
                Logging.Message("ElectricityManager already instantiated");

                // Get electricity texture instance.
                ElectricityManager electricityManager = Singleton<ElectricityManager>.instance;
                FieldInfo m_electricityTexture = AccessTools.Field(typeof(ElectricityManager), "m_electricityTexture");
                Texture2D electricityTexture = m_electricityTexture.GetValue(electricityManager) as Texture2D;

                // Check texture size.
                if (electricityTexture.width != ExpandedElectricityGridResolution)
                {
                    // Texture size is not expanded - reset per ElectricityManager.Awake.
                    Logging.Error("invalid ElectricityManager texture size (conflicting mod?); correcting");
                    Texture2D newTexture = new Texture2D(ExpandedElectricityGridResolution, ExpandedElectricityGridResolution, TextureFormat.RGBA32, mipmap: false, linear: true)
                    {
                        filterMode = FilterMode.Point,
                        wrapMode = TextureWrapMode.Clamp,
                    };

                    // Apply new texture and destroy the old.
                    m_electricityTexture.SetValue(electricityManager, newTexture);
                    Shader.SetGlobalTexture("_ElectricityTexture", newTexture);
                    Object.Destroy(electricityTexture);

                    Vector4 mapping = new Vector4
                    {
                        z = 1 / (ElectricityManager.ELECTRICITYGRID_CELL_SIZE * ExpandedElectricityGridResolution),
                        x = 0.5f,
                        y = 0.5f,
                        w = 1.0f / ExpandedElectricityGridResolution,
                    };
                    Shader.SetGlobalVector("_ElectricityMapping", mapping);

                    // Set intitial modified coordinates.
                    AccessTools.Field(typeof(ElectricityManager), "m_modifiedX2").SetValue(electricityManager, ExpandedElectricityGridMax);
                    AccessTools.Field(typeof(ElectricityManager), "m_modifiedZ2").SetValue(electricityManager, ExpandedElectricityGridMax);
                }
            }
            else
            {
                Logging.Message("ElectricityManager not yet instantiated");
            }

            if (Singleton<WaterManager>.exists)
            {
                Logging.Message("WaterManager already instantiated");

                // Get water texture instance.
                WaterManager waterManager = Singleton<WaterManager>.instance;
                FieldInfo m_waterTexture = AccessTools.Field(typeof(WaterManager), "m_waterTexture");
                Texture2D waterTexture = m_waterTexture.GetValue(waterManager) as Texture2D;

                // Check texture size.
                if (waterTexture.width != ExpandedWaterGridResolution)
                {
                    // Texture size is not expanded - reset per WaterManager.Awake.
                    Logging.Error("invalid WaterManager texture size (conflicting mod?); correcting");
                    Texture2D newTexture = new Texture2D(ExpandedWaterGridResolution, ExpandedWaterGridResolution, TextureFormat.RGBA32, mipmap: false, linear: true)
                    {
                        filterMode = FilterMode.Point,
                        wrapMode = TextureWrapMode.Clamp,
                    };

                    // Apply new texture and destroy the old.
                    m_waterTexture.SetValue(waterManager, newTexture);
                    Shader.SetGlobalTexture("_WaterTexture", newTexture);
                    Object.Destroy(waterTexture);

                    // Update texture mapping.
                    Vector4 mapping = new Vector4
                    {
                        z = 1 / (WaterManager.WATERGRID_CELL_SIZE * ExpandedWaterGridResolution),
                        x = 0.5f,
                        y = 0.5f,
                        w = 1.0f / ExpandedWaterGridResolution,
                    };
                    Shader.SetGlobalVector("_WaterMapping", mapping);

                    // Set intitial modified coordinates.
                    AccessTools.Field(typeof(WaterManager), "m_modifiedX2").SetValue(waterManager, ExpandedWaterGridMax);
                    AccessTools.Field(typeof(WaterManager), "m_modifiedZ2").SetValue(waterManager, ExpandedWaterGridMax);
                }
            }
            else
            {
                Logging.Message("WaterManager not yet instantiated");
            }

            // Immaterial resource manager.
            if (Singleton<ImmaterialResourceManager>.exists)
            {
                Logging.Message("ImmaterialResourceManager already instantiated");

                // Get district texture instance.
                ImmaterialResourceManager immaterialResourceManager = Singleton<ImmaterialResourceManager>.instance;
                FieldInfo m_resourceTexture = AccessTools.Field(typeof(ImmaterialResourceManager), "m_resourceTexture");
                Texture2D resourceTexture = m_resourceTexture.GetValue(immaterialResourceManager) as Texture2D;

                Logging.Message(resourceTexture.width);

                // Check texture size.
                if (resourceTexture.width != ExpandedImmaterialResourceGridResolution)
                {
                    Logging.Error("invalid ImmaterialResourceManager texture size (conflicting mod?); correcting");

                    // Texture size is not expanded - reset per ImmaterialResourceManager.Awake.
                    Texture2D newTexture = new Texture2D(ExpandedImmaterialResourceGridResolution, ExpandedImmaterialResourceGridResolution, TextureFormat.Alpha8, mipmap: false, linear: true)
                    {
                        wrapMode = TextureWrapMode.Clamp,
                    };
                    m_resourceTexture.SetValue(immaterialResourceManager, newTexture);
                    Shader.SetGlobalTexture("_ImmaterialResources", newTexture);

                    // Set array.
                    AccessTools.Field(typeof(ImmaterialResourceManager), "m_tempCircleMinX").SetValue(immaterialResourceManager, new int[ExpandedImmaterialResourceGridResolution]);
                    AccessTools.Field(typeof(ImmaterialResourceManager), "m_tempCircleMaxX").SetValue(immaterialResourceManager, new int[ExpandedImmaterialResourceGridResolution]);
                    AccessTools.Field(typeof(ImmaterialResourceManager), "m_tempSectorSlopes").SetValue(immaterialResourceManager, new float[ExpandedImmaterialResourceGridResolution]);
                    AccessTools.Field(typeof(ImmaterialResourceManager), "m_tempSectorDistances").SetValue(immaterialResourceManager, new float[ExpandedImmaterialResourceGridResolution]);

                    // Create new modified area arrays.
                    int[] modifiedX1 = new int[ExpandedImmaterialResourceGridResolution];
                    int[] modifiedX2 = new int[ExpandedImmaterialResourceGridResolution];
                    for (int i = 0; i < ExpandedImmaterialResourceGridResolution; ++i)
                    {
                        modifiedX1[i] = ExpandedImmaterialResourceGridMax;
                        modifiedX2[i] = ExpandedImmaterialResourceGridMax;
                    }

                    // Apply new modified area arrays.
                    AccessTools.Field(typeof(ImmaterialResourceManager), "m_modifiedX1").SetValue(immaterialResourceManager, modifiedX1);
                    AccessTools.Field(typeof(ImmaterialResourceManager), "m_modifiedX2").SetValue(immaterialResourceManager, modifiedX2);
                }
            }
            else
            {
                Logging.Message("ImmaterialResourceManager not yet instantiated");
            }
        }
    }
}
