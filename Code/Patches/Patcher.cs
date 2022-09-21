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
    using static Patches.ElectricityManagerPatches;
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

            Logging.Message("checking for manager instantiation");

            // If any of the utility managers have already been instantiated, we need to replace their overlay textures.
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
        }
    }
}
