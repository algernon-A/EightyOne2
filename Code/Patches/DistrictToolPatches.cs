// <copyright file="DistrictToolPatches.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace EightyOne2
{
    using System;
    using System.Collections.Generic;
    using HarmonyLib;
    using UnityEngine;
    using static DistrictManagerPatches;

    /// <summary>
    /// Harmomy patches for the game's district tool to implement 81 tiles functionality.
    /// </summary>
    [HarmonyPatch(typeof(DistrictTool))]
    internal class DistrictToolPatches
    {
        /// <summary>
        /// Harmony transpiler for BuildingTool.ApplyBrush to replace hardcoded game constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(DistrictTool.ApplyBrush), new Type[] { typeof(DistrictTool.Layer), typeof(byte), typeof(float), typeof(Vector3), typeof(Vector3) })]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> ApplyBrushTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceDistrictToolConstants(instructions);

        /// <summary>
        /// Harmony transpiler for BuildingTool.CheckNeighbourCells to replace hardcoded game constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch("CheckNeighbourCells")]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> CheckNeighbourCellsTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceDistrictToolConstants(instructions);

        /// <summary>
        /// Harmony transpiler for BuildingTool.ForceDistrictAlpha to replace hardcoded game constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch("ForceDistrictAlpha")]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> ForceDistrictAlphaTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceDistrictToolConstants(instructions);

        /// <summary>
        /// Harmony transpiler for BuildingTool.SetDistrictAlpha to replace hardcoded game constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch("SetDistrictAlpha")]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> SetDistrictAlphaTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceDistrictToolConstants(instructions);

        /// <summary>
        /// Replaces any references to default constants in the provided code with updated 81 tile values.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        private static IEnumerable<CodeInstruction> ReplaceDistrictToolConstants(IEnumerable<CodeInstruction> instructions)
        {
            // Look for and update any relevant constants.
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.LoadsConstant(GameDistrictGridResolution))
                {
                    // District grid resolution, i.e. 512 -> 900.
                    instruction.operand = ExpandedDistrictGridResolution;
                }

                yield return instruction;
            }
        }
    }
}
