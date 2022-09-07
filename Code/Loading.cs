// <copyright file="Loading.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace EightyOne2
{
    using AlgernonCommons.Patching;
    using ColossalFramework;
    using ICities;

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
        }
    }
}