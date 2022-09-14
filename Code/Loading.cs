// <copyright file="Loading.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace EightyOne2
{
    using AlgernonCommons.Patching;
    using ColossalFramework;
    using ICities;
    using UnityEngine;

    /// <summary>
    /// Main loading class: the mod runs from here.
    /// </summary>
    public sealed class Loading : PatcherLoadingBase<OptionsPanel, PatcherBase>
    {
        /// <summary>
        /// Performs actions upon successful level loading completion.
        /// </summary>
        /// <param name="mode">Loading mode (e.g. game, editor, scenario, etc.).</param>
        protected override void LoadedActions(LoadMode mode)
        {
            base.LoadedActions(mode);

            // Force update of all district and park areas.
            Singleton<SimulationManager>.instance.AddAction(() =>
            {
                DistrictManager districtManager = Singleton<DistrictManager>.instance;
                districtManager.AreaModified(0, 0, Patches.DistrictManagerPatches.ExpandedDistrictGridMax, Patches.DistrictManagerPatches.ExpandedDistrictGridMax, true);
                districtManager.ParksAreaModified(0, 0, Patches.DistrictManagerPatches.ExpandedDistrictGridMax, Patches.DistrictManagerPatches.ExpandedDistrictGridMax, true);
                districtManager.NamesModified();
                districtManager.ParkNamesModified();
            });

            // Force update of electricity map.
            SimulationManager.instance.AddAction(() =>
            {
                ElectricityManager.instance.UpdateGrid(-100000f, -100000f, 100000f, 100000f);
                Vector4 value = default;
                value.z = 1 / (ElectricityManager.ELECTRICITYGRID_CELL_SIZE * Patches.ElectricityManagerPatches.ExpandedElectricityGridResolution);
                value.x = 0.5f;
                value.y = 0.5f;
                value.w = 1.0f / Patches.ElectricityManagerPatches.ExpandedElectricityGridResolution;
                Shader.SetGlobalVector("_ElectricityMapping", value);
            });

            // Force update of water map.
            SimulationManager.instance.AddAction(() =>
            {
                WaterManager.instance.UpdateGrid(-100000f, -100000f, 100000f, 100000f);
                Vector4 value = default(Vector4);
                value.z = 0.000102124184f;
                value.x = 0.5f;
                value.y = 0.5f;
                value.w = 0.00390625f;
                Shader.SetGlobalVector("_WaterMapping", value);
            });

            // Push back edge fog to match original 81 tiles mod.
            SimulationManager.instance.AddAction(() => Object.FindObjectOfType<RenderProperties>().m_edgeFogDistance = 2800f);
            SimulationManager.instance.AddAction(() => Object.FindObjectOfType<FogEffect>().m_edgeFogDistance = 2800f);
            SimulationManager.instance.AddAction(() => Object.FindObjectOfType<FogProperties>().m_EdgeFogDistance = 2800f);
        }
    }
}