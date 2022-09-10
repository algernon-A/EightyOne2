// <copyright file="ElectricityManagerPatches.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace EightyOne2.Patches
{
    using System;
    using System.Collections.Generic;
    using System.Reflection.Emit;
    using System.Runtime.CompilerServices;
    using AlgernonCommons;
    using HarmonyLib;
    using UnityEngine;
    using static ElectricityManager;

    /// <summary>
    /// Harmomy patches for the game's electricity manager to implement 81 tiles functionality.
    /// </summary>
    [HarmonyPatch(typeof(ElectricityManager))]
    internal static class ElectricityManagerPatches
    {
        /// <summary>
        /// Game electricity grid array size (256 * 256 = 65,536).
        /// </summary>
        internal const int GameElectricityGridArraySize = ELECTRICITYGRID_RESOLUTION * ELECTRICITYGRID_RESOLUTION;

        /// <summary>
        /// Expanded electricity grid array size (462 * 462 = 213444).
        /// </summary>
        internal const int ExpandedElectricityGridArraySize = ExpandedElectricityGridResolution * ExpandedElectricityGridResolution;

        /// <summary>
        /// Game electricity grid height and width (256).
        /// </summary>
        internal const int GameElectricityGridResolution = ELECTRICITYGRID_RESOLUTION;

        /// <summary>
        /// Expanded electricity grid height and width (462).
        /// </summary>
        internal const int ExpandedElectricityGridResolution = 462;

        /// <summary>
        /// Game electricty grid half-resolution (128f).
        /// </summary>
        internal const float GameElectricityGridHalfResolution = GameElectricityGridResolution / 2;

        /// <summary>
        /// Expanded electricty grid half-resolution (231f).
        /// </summary>
        internal const float ExpandedElectricityGridHalfResolution = ExpandedElectricityGridResolution / 2;

        /// <summary>
        /// Game electricity grid resolution maximum bound (resolution - 1 = 255).
        /// </summary>
        internal const int GameElectricityGridMax = GameElectricityGridResolution - 1;

        /// <summary>
        /// Expanded electricity grid resolution maximum bound (resolution - 1 = 461).
        /// </summary>
        internal const int ExpandedElectricityGridMax = ExpandedElectricityGridResolution - 1;

        /// <summary>
        /// Replaces any references to default constants in the provided code with updated 81 tile values.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        internal static IEnumerable<CodeInstruction> ReplaceElectricityConstants(IEnumerable<CodeInstruction> instructions)
        {
            foreach (CodeInstruction instruction in instructions)
            {
                // Look for and update any relevant constants.
                if (instruction.LoadsConstant(GameElectricityGridResolution))
                {
                    // Electricity grid size, i.e. 256->462.
                    instruction.operand = ExpandedElectricityGridResolution;
                }
                else if (instruction.LoadsConstant(GameElectricityGridMax))
                {
                    // Electricity grid array maximum index i.e. 255->461.
                    instruction.operand = ExpandedElectricityGridMax;
                }
                else if (instruction.LoadsConstant(GameElectricityGridHalfResolution))
                {
                    // Electricity grid array half-size i.e. 12f8->231f.
                    instruction.operand = ExpandedElectricityGridHalfResolution;
                }

                /*else if (instruction.LoadsConstant(GameElectricyGridSize))
                {
                    // Electricity grid array total size i.e. 65536->232324.
                    instruction.operand = ExpandedElectricityGridSize;
                }*/

                yield return instruction;
            }
        }

        /// <summary>
        /// Reverse patch for ElectricityManager.UpdateNodeElectricity to access private method of original instance.
        /// </summary>
        /// <param name="instance">ElectricityManager instance.</param>
        /// <param name="nodeID">ID of of node to update.</param>
        /// <param name="value">Value to apply.</param>
        /// <exception cref="NotImplementedException">Reverse patch wasn't applied.</exception>
        [HarmonyReversePatch(HarmonyReversePatchType.Snapshot)]
        [HarmonyPatch("UpdateNodeElectricity")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void UpdateNodeElectricity(ElectricityManager instance, int nodeID, int value)
        {
            string message = "UpdateNodeElectricity reverse Harmony patch wasn't applied";
            Logging.Error(message, instance, nodeID, value);
            throw new NotImplementedException(message);
        }

        /// <summary>
        /// Harmony transpiler for ElectricityManager.Awake to replace hardcoded game constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch("Awake")]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> AwakeTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceElectricityConstants(instructions);

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
        /// Harmony transpiler for ElectricityManager.SimulationStepImpl to replace with call to our expanded electricity manager.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch("SimulationStepImpl")]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> SimulationStepImplTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldarg_1);
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ElectricityManager), "m_electricityGrid"));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(ElectricityManager), "m_pulseGroupCount"));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(ElectricityManager), "m_pulseUnitStart"));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(ElectricityManager), "m_pulseUnitEnd"));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(ElectricityManager), "m_processedCells"));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(ElectricityManager), "m_conductiveCells"));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(ElectricityManager), "m_canContinue"));
            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ExpandedElectricityManager), nameof(ExpandedElectricityManager.SimulationStepImpl)));
            yield return new CodeInstruction(OpCodes.Ret);
        }

        /// <summary>
        /// Harmony transpiler for ElectricityManager.TryDumpElectricity to replace hardcoded game constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(ElectricityManager.TryDumpElectricity), new Type[] { typeof(Vector3), typeof(int), typeof(int) })]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> TryDumpElectricity1Transpiler(IEnumerable<CodeInstruction> instructions) => ReplaceElectricityConstants(instructions);

        /// <summary>
        /// Harmony transpiler for ElectricityManager.TryDumpElectricity to replace hardcoded game constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch("TryDumpElectricity", new Type[] { typeof(int), typeof(int), typeof(int), typeof(int) })]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> TryDumpElectricity2Transpiler(IEnumerable<CodeInstruction> instructions) => ReplaceElectricityConstants(instructions);

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
            // Replace inverse constants.
            const float gameZ = 1f / (ELECTRICITYGRID_CELL_SIZE * GameElectricityGridResolution);
            const float expandedZ = 1f / (ELECTRICITYGRID_CELL_SIZE * ExpandedElectricityGridResolution);
            const float gameW = 1f / GameElectricityGridResolution;
            const float expandedW = 1f / ExpandedElectricityGridResolution;

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
        /// Harmony transpiler for ElectricityManager.UpdateGrid to replace with call to our expanded electricity manager.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(ElectricityManager.UpdateGrid))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> UpdateGridTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldarg_1);
            yield return new CodeInstruction(OpCodes.Ldarg_2);
            yield return new CodeInstruction(OpCodes.Ldarg_3);
            yield return new CodeInstruction(OpCodes.Ldarg_S, 4);
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ElectricityManager), "m_electricityGrid"));
            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ExpandedElectricityManager), nameof(ExpandedElectricityManager.UpdateGrid)));
            yield return new CodeInstruction(OpCodes.Ret);
        }

        /// <summary>
        /// Harmony transpiler for ElectricityManager.UpdateTexture to replace hardcoded game constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch("UpdateTexture")]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> UpdateTextureTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceElectricityConstants(instructions);
    }
}
