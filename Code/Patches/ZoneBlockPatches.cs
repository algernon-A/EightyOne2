// <copyright file="ZoneBlockPatches.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace EightyOne2
{
    using System.Collections.Generic;
    using HarmonyLib;
    using static ZoneManagerPatches;

    /// <summary>
    /// Harmomy patches for the game's zone blocks to implement 81 tiles functionality.
    /// </summary>
    [HarmonyPatch(typeof(ZoneBlock))]
    internal class ZoneBlockPatches
    {
        /// <summary>
        /// Harmony transpiler for ZoneBlock.CalculateBlock1 to replace hardcoded game constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(ZoneBlock.CalculateBlock1))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> CalculateBlock1Transpiler(IEnumerable<CodeInstruction> instructions) => ReplaceZoneConstants(instructions);

        /// <summary>
        /// Harmony transpiler for ZoneBlock.CalculateBlock2 to replace hardcoded game constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(ZoneBlock.CalculateBlock2))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> CalculateBlock2Transpiler(IEnumerable<CodeInstruction> instructions) => ReplaceZoneConstants(instructions);

        /// <summary>
        /// Harmony transpiler for ZoneBlock.SimulationStep to replace hardcoded game constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(ZoneBlock.SimulationStep))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> SimulationStepTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceZoneConstants(instructions);
    }
}
