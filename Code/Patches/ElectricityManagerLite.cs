// <copyright file="ElectricityManagerLite.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace EightyOne2
{
    using System;
    using System.Collections.Generic;
    using HarmonyLib;
    using UnityEngine;
    using static ElectricityManager;

    /// <summary>
    /// Harmomy patches for the game's electricity manager to implement 81 tiles functionality.
    /// </summary>
    //[HarmonyPatch(typeof(ElectricityManager))]
    internal static class ElectricityManagerLite
    {
        // 68.85 (instead of 38.25).
        private const float ExpandedElectricityCellSize = ELECTRICITYGRID_CELL_SIZE * 9f / 5f;
        private const float GameHalfElectricityCellSize = ELECTRICITYGRID_CELL_SIZE / 2f;
        private const float ExpandedHalfElectricityCellSize = ExpandedElectricityCellSize / 2f;

        /// <summary>
        /// Harmony transpiler for ElectricityManager.CheckConductivity to replace hardcoded game constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(ElectricityManager.CheckConductivity))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> CheckConductivityTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceElectricityConstants(instructions);

        /// <summary>
        /// Harmony transpiler for ElectricityManager.CheckElectricity to replace hardcoded game constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(ElectricityManager.CheckElectricity))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> CheckElectricityTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceElectricityConstants(instructions);

        /// <summary>
        /// Harmony transpiler for ElectricityManager.ConductToCells to replace hardcoded game constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch("ConductToCells")]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> ConductToCellsTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceElectricityConstants(instructions);

        /// <summary>
        /// Harmony transpiler for ElectricityManager.ConductToNodes to replace hardcoded game constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch("ConductToNodes")]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> ConductToNodesTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceElectricityConstants(instructions);

        /// <summary>
        /// Harmony transpiler for ElectricityManager.TryDumpElectricity to replace hardcoded game constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(
            nameof(ElectricityManager.TryDumpElectricity),
            new Type[] { typeof(Vector3), typeof(int), typeof(int) },
            new ArgumentType[] { ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal })]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> TryDumpElectricityTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceElectricityConstants(instructions);

        /// <summary>
        /// Harmony transpiler for ElectricityManager.TryFetchElectricity to replace hardcoded game constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(ElectricityManager.TryFetchElectricity))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> TryFetchElectricityTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceElectricityConstants(instructions);

        /// <summary>
        /// Harmony transpiler for ElectricityManager.UpdateElectricityMapping to replace hardcoded game constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch("UpdateElectricityMapping")]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> UpdateElectricityMappingTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            // Replace inverse constant using expanded cell size.
            const float gameZ = 1f / (ELECTRICITYGRID_CELL_SIZE * ELECTRICITYGRID_RESOLUTION);
            const float expandedZ = 1f / (ExpandedElectricityCellSize * ELECTRICITYGRID_RESOLUTION);
            const float gameW = 1f / ElectricityManagerPatches.GameElectricyGridResolution;
            const float expandedW = 1f / ElectricityManagerPatches.ExpandedElectricityGridResolution;

            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.LoadsConstant(gameZ))
                {
                    instruction.operand = expandedZ;
                }
                else if (instruction.LoadsConstant(gameW))
                {
                    instruction.operand = expandedW;
                }

                yield return instruction;
            }
        }

        /// <summary>
        /// Harmony transpiler for ElectricityManager.UpdateGrid to replace hardcoded game constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(ElectricityManager.UpdateGrid))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> UpdateGridTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceElectricityConstants(instructions);

        /// <summary>
        /// Harmony transpiler for ElectricityManager.UpdateTexture to replace hardcoded game constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch("UpdateTexture")]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> UpdateTextureTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceElectricityConstants(instructions);

        /// <summary>
        /// Replaces any references to default constants in the provided code with updated 81 tile values.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        private static IEnumerable<CodeInstruction> ReplaceElectricityConstants(IEnumerable<CodeInstruction> instructions)
        {
            foreach (CodeInstruction instruction in instructions)
            {
                // Look for and update any relevant constants.
                if (instruction.LoadsConstant(ELECTRICITYGRID_CELL_SIZE))
                {
                    // Electricity grid cell size, i.e. 38.25f -> 68.85f.
                    instruction.operand = ExpandedElectricityCellSize;
                }
                else if (instruction.LoadsConstant(GameHalfElectricityCellSize))
                {
                    // Electricity grid cell half-size, i.e. 19.125f -> 34.425f.
                    instruction.operand = ExpandedHalfElectricityCellSize;
                }

                yield return instruction;
            }
        }
    }
}
