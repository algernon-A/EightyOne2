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
    public sealed class Loading : PatcherLoadingBase<OptionsPanel, Patcher>
    {
        /// <summary>
        /// Performs actions upon successful level loading completion.
        /// </summary>
        /// <param name="mode">Loading mode (e.g. game, editor, scenario, etc.).</param>
        protected override void LoadedActions(LoadMode mode)
        {
            base.LoadedActions(mode);

            SimulationManager simulationManager = Singleton<SimulationManager>.instance;

            // Push back edge fog to match original 81 tiles mod.
            simulationManager.AddAction(() => Object.FindObjectOfType<RenderProperties>().m_edgeFogDistance = 2800f);
            simulationManager.AddAction(() => Object.FindObjectOfType<FogEffect>().m_edgeFogDistance = 2800f);
            simulationManager.AddAction(() => Object.FindObjectOfType<FogProperties>().m_EdgeFogDistance = 2800f);

            // Update utility grids.
            simulationManager.AddAction(() => Singleton<WaterManager>.instance.UpdateGrid(-10000f, -10000f, 10000f, 10000f));
            simulationManager.AddAction(() => Singleton<ElectricityManager>.instance.UpdateGrid(-10000f, -10000f, 10000f, 10000f));
        }
    }
}