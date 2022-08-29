// <copyright file="BuildingToolPatches.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace EightyOne2
{
    using System.Collections.Generic;
    using HarmonyLib;
    using static ZoneManagerPatches;

    /// <summary>
    /// Harmomy patches for the game's building tool to implement 81 tiles functionality.
    /// </summary>
    [HarmonyPatch(typeof(BuildingTool))]
    internal class BuildingToolPatches
    {
        /// <summary>
        /// Harmony transpiler for BuildingTool.SimulationStep to replace hardcoded game constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(BuildingTool.SimulationStep))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> SimulationStepTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceZoneConstants(instructions);
    }
}
