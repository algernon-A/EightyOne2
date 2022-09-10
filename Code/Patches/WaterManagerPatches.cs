// <copyright file="WaterManagerPatches.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace EightyOne2.Patches
{
    using System;
    using System.Collections.Generic;
    using System.Reflection.Emit;
    using HarmonyLib;
    using UnityEngine;
    using static WaterManager;

    /// <summary>
    /// Harmomy patches for the game's water manager to implement 81 tiles functionality.
    /// </summary>
    [HarmonyPatch(typeof(WaterManager))]
    internal static class WaterManagerPatches
    {
        /// <summary>
        /// Game water grid array size (256 * 256 = 65,536).
        /// </summary>
        internal const int GameWaterGridArraySize = WATERGRID_RESOLUTION * WATERGRID_RESOLUTION;

        /// <summary>
        /// Expanded water grid array size (462 * 462 = 213444).
        /// </summary>
        internal const int ExpandedWaterGridArraySize = ExpandedWaterGridResolution * ExpandedWaterGridResolution;

        /// <summary>
        /// Game water grid height and width (256).
        /// </summary>
        internal const int GameWaterGridResolution = WATERGRID_RESOLUTION;

        /// <summary>
        /// Expanded water grid height and width (462).
        /// </summary>
        internal const int ExpandedWaterGridResolution = 462;

        /// <summary>
        /// Game water grid half-resolution (128f).
        /// </summary>
        internal const float GameWaterGridHalfResolution = GameWaterGridResolution / 2;

        /// <summary>
        /// Expanded water grid half-resolution (231f).
        /// </summary>
        internal const float ExpandedWaterGridHalfResolution = ExpandedWaterGridResolution / 2;

        /// <summary>
        /// Game water grid resolution maximum bound (resolution - 1 = 255).
        /// </summary>
        internal const int GameWaterGridMax = GameWaterGridResolution - 1;

        /// <summary>
        /// Expanded water grid resolution maximum bound (resolution - 1 = 461).
        /// </summary>
        internal const int ExpandedWaterGridMax = ExpandedWaterGridResolution - 1;

        /// <summary>
        /// Replaces any references to default constants in the provided code with updated 81 tile values.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        internal static IEnumerable<CodeInstruction> ReplaceWaterConstants(IEnumerable<CodeInstruction> instructions)
        {
            foreach (CodeInstruction instruction in instructions)
            {
                // Look for and update any relevant constants.
                if (instruction.LoadsConstant(GameWaterGridResolution))
                {
                    // Water grid size, i.e. 256->462.
                    instruction.operand = ExpandedWaterGridResolution;
                }
                else if (instruction.LoadsConstant(GameWaterGridMax))
                {
                    // Water grid array maximum index i.e. 255->461.
                    instruction.operand = ExpandedWaterGridMax;
                }
                else if (instruction.LoadsConstant(GameWaterGridHalfResolution))
                {
                    // Water grid array half-size i.e. 12f8->231f.
                    instruction.operand = ExpandedWaterGridHalfResolution;
                }

                yield return instruction;
            }
        }

        /// <summary>
        /// Harmony transpiler for WaterManager.Awake to replace hardcoded game constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch("Awake")]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> AwakeTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceWaterConstants(instructions);

        /// <summary>
        /// Harmony transpiler for WaterManager.CheckHeating to replace hardcoded game constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(WaterManager.CheckHeating))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> CheckHeatingTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceWaterConstants(instructions);

        /// <summary>
        /// Harmony transpiler for WaterManager.CheckHeatingImpl to replace hardcoded game constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch("CheckHeatingImpl")]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> CheckHeatingImplTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceWaterConstants(instructions);

        /// <summary>
        /// Harmony transpiler for WaterManager.CheckWater to replace hardcoded game constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(WaterManager.CheckWater))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> CheckWaterTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceWaterConstants(instructions);

        /// <summary>
        /// Harmony transpiler for WaterManager.CheckWaterImpl to replace hardcoded game constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch("CheckWaterImpl")]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> CheckWaterImplTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceWaterConstants(instructions);

        /// <summary>
        /// Harmony transpiler for WaterManager.SimulationStepImpl to replace with call to our expanded water manager.
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
            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(WaterManager), "m_waterGrid"));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(WaterManager), "m_waterPulseGroupCount"));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(WaterManager), "m_waterPulseUnitStart"));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(WaterManager), "m_waterPulseUnitEnd"));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(WaterManager), "m_sewagePulseGroupCount"));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(WaterManager), "m_sewagePulseUnitStart"));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(WaterManager), "m_sewagePulseUnitEnd"));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(WaterManager), "m_heatingPulseGroupCount"));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(WaterManager), "m_heatingPulseUnitStart"));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(WaterManager), "m_heatingPulseUnitEnd"));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(WaterManager), "m_processedCells"));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(WaterManager), "m_conductiveCells"));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(WaterManager), "m_canContinue"));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(WaterManager), "m_waterPulseGroups"));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(WaterManager), "m_sewagePulseGroups"));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(WaterManager), "m_heatingPulseGroups"));
            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ExpandedWaterManager), nameof(ExpandedWaterManager.SimulationStepImpl)));
            yield return new CodeInstruction(OpCodes.Ret);
        }

        /// <summary>
        /// Harmony transpiler for WaterManager.TryDumpSewage to replace hardcoded game constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(WaterManager.TryDumpSewage), new Type[] { typeof(Vector3), typeof(int), typeof(int) })]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> TryDumpSewageTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceWaterConstants(instructions);

        /// <summary>
        /// Harmony transpiler for WaterManager.TryDumpSewageImpl to replace hardcoded game constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch("TryDumpSewageImpl")]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> TryDumpSewageImplTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceWaterConstants(instructions);

        /// <summary>
        /// Harmony transpiler for WaterManager.TryFetchHeating to replace hardcoded game constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(WaterManager.TryFetchHeating))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> TryFetchHeatingTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceWaterConstants(instructions);

        /// <summary>
        /// Harmony transpiler for WaterManager.TryFetchHeatingImpl to replace hardcoded game constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch("TryFetchHeatingImpl")]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> TryFetchHeatingImplTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceWaterConstants(instructions);

        /// <summary>
        /// Harmony transpiler for WaterManager.TryFetchWater to replace hardcoded game constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(WaterManager.TryFetchWater), new Type[] { typeof(Vector3), typeof(int), typeof(int), typeof(byte) }, new ArgumentType[] { ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Ref })]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> TryFetchWaterTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceWaterConstants(instructions);

        /// <summary>
        /// Harmony transpiler for WaterManager.TryFetchWaterImpl to replace hardcoded game constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch("TryFetchWaterImpl")]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> TryFetchWaterImplImplTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceWaterConstants(instructions);

        /// <summary>
        /// Harmony transpiler for WaterManager.UpdateGrid to replace hardcoded game constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(WaterManager.UpdateGrid))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> UpdateGridTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceWaterConstants(instructions);

        /// <summary>
        /// Harmony transpiler for WaterManager.UpdateTexture to replace hardcoded game constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch("UpdateTexture")]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> UpdateTextureTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (CodeInstruction instruction in instructions)
            {
                // Look for and update any relevant constants.
                if (instruction.LoadsConstant(GameWaterGridResolution))
                {
                    // Water grid size, i.e. 256->462.
                    instruction.operand = ExpandedWaterGridResolution;
                }
                else if (instruction.LoadsConstant(GameWaterGridMax))
                {
                    // Water grid array maximum index i.e. 255->461.
                    instruction.operand = ExpandedWaterGridMax;
                }
                else if (instruction.LoadsConstant(2048))
                {
                    // Water grid resolution * 8 i.e. 2048->3696.
                    instruction.operand = ExpandedWaterGridResolution * 8;
                }

                yield return instruction;
            }
        }

        /// <summary>
        /// Harmony transpiler for WaterManager.UpdateWaterMapping to replace hardcoded game constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch("UpdateWaterMapping")]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> UpdateWaterMappingTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            // Replace inverse constants.
            const float gameZ = 1f / (WATERGRID_CELL_SIZE * GameWaterGridResolution);
            const float expandedZ = 1f / (WATERGRID_CELL_SIZE * ExpandedWaterGridResolution);
            const float gameW = 1f / GameWaterGridResolution;
            const float expandedW = 1f / ExpandedWaterGridResolution;

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
    }
}
