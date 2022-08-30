// <copyright file="DistrictManagerPatches.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace EightyOne2
{
    using System;
    using System.Collections.Generic;
    using System.Reflection.Emit;
    using HarmonyLib;
    using UnityEngine;
    using static DistrictManager;

    /// <summary>
    /// Harmony patches for the district manager to implement 81 tiles functionality.
    /// </summary>
    [HarmonyPatch(typeof(DistrictManager))]
    internal static class DistrictManagerPatches
    {
        /// <summary>
        /// Game district grid width and height.
        /// Logically, the game grid should be 500 using the given cell size (19.2), but they've gone with 512.   Not sure if rounding or overlap.
        /// </summary>
        internal const int GameDistrictGridResolution = DISTRICTGRID_RESOLUTION;

        /// <summary>
        /// Expanded district grid width and height.
        /// </summary>
        internal const int ExpandedDistrictGridResolution = 900;

        /// <summary>
        /// Expanded district grid array size.
        /// </summary>
        internal const int ExpandedDistrictGridArraySize = ExpandedDistrictGridResolution * ExpandedDistrictGridResolution;

        /// <summary>
        /// Game electricty grid half-resolution.
        /// </summary>
        internal const float GameDistrictGridHalfResolution = GameDistrictGridResolution / 2;

        /// <summary>
        /// Expanded electricty grid half-resolution.
        /// </summary>
        internal const float ExpandedDistrictGridHalfResolution = ExpandedDistrictGridResolution / 2;

        // Derived constants.
        private const int GameDistrictGridArraySize = GameDistrictGridResolution * GameDistrictGridResolution;
        private const int GameDistrictGridArrayQuarterSize = (GameDistrictGridResolution * GameDistrictGridResolution) / 4;
        private const int ExpandedDistrictGridArrayQuarterSize = (ExpandedDistrictGridResolution * ExpandedDistrictGridResolution) / 4;
        private const float GameDistrictAreaSize = GameDistrictGridResolution * DISTRICTGRID_CELL_SIZE;
        private const float ExpandedDistrictAreaSize = ExpandedDistrictGridResolution * DISTRICTGRID_CELL_SIZE;

        // Limits.
        private const int GameDistrictGridMax = GameDistrictGridResolution - 1;
        private const int ExpandedDistrictGridMax = ExpandedDistrictGridResolution - 1;

        /// <summary>
        /// Harmony transpiler for DistrictManager.HighlightPolicy setter to update code constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(DistrictManager.HighlightPolicy), MethodType.Setter)]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> HighlightPolicySetterTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceDistrictConstants(instructions);

        /// <summary>
        /// Harmony transpiler for DistrictManager.Awake to update texture size constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch("Awake")]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> AwakeTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            // Targeting the m_colorBuffer init.
            int skippedCount = 0;
            foreach (CodeInstruction instruction in ReplaceDistrictConstants(instructions))
            {
                if (instruction.LoadsConstant(GameDistrictGridArraySize))
                {
                    // Skip first two instances assigning m_districtGrid and m_parkGrid.
                    if (++skippedCount > 2)
                    {
                        // District grid size, i.e. 262144 -> 810000.
                        instruction.operand = ExpandedDistrictGridArraySize;
                    }
                }

                yield return instruction;
            }
        }

        /// <summary>
        /// Harmony transpiler for DistrictManager.BeginOverlayImpl to update code constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch("BeginOverlayImpl")]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> BeginOverlayImplTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            // Inverse floating-point constants.
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.LoadsConstant(1f / GameDistrictAreaSize))
                {
                    // 0.000101725258f.
                    instruction.operand = 1f / ExpandedDistrictAreaSize;
                }
                else if (instruction.LoadsConstant(GameDistrictAreaSize))
                {
                    // Grid size in metres, i.e. 9830.4f -> 17280.
                    instruction.operand = ExpandedDistrictAreaSize;
                }

                yield return instruction;
            }
        }

        /// <summary>
        /// Harmony transpiler for DistrictManager.GetDistrict to update code constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(DistrictManager.GetDistrict), new Type[] { typeof(int), typeof(int) })]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> GetDistrict1Transpiler(IEnumerable<CodeInstruction> instructions) => ReplaceDistrictConstants(instructions);

        /// <summary>
        /// Harmony transpiler for DistrictManager.GetDistrict to update code constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(DistrictManager.GetDistrict), new Type[] { typeof(Vector3) })]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> GetDistrict2Transpiler(IEnumerable<CodeInstruction> instructions) => ReplaceDistrictConstants(instructions);

        /// <summary>
        /// Harmony transpiler for DistrictManager.GetDistrictArea to update code constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(DistrictManager.GetDistrictArea))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> GetDistrictAreaTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceDistrictConstants(instructions);

        /// <summary>
        /// Harmony transpiler for DistrictManager.GetPark to update code constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(DistrictManager.GetPark))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> GetParkTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceDistrictConstants(instructions);

        /// <summary>
        /// Harmony transpiler for DistrictManager.GetParkArea to update code constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(DistrictManager.GetParkArea))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> GetParkAreaTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceDistrictConstants(instructions);

        /// <summary>
        /// Harmony transpiler for DistrictManager.ModifyCell to update code constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(DistrictManager.ModifyCell))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> ModifyCellTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceDistrictConstants(instructions);

        /// <summary>
        /// Harmony transpiler for DistrictManager.ModifyParkCell to update code constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(DistrictManager.ModifyParkCell))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> ModifyParkCellTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            // Custom job due to false postives with the standard replacer from alpha calculations.
            int replacedCount = 0;

            // Look for and update any relevant constants.
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.LoadsConstant(GameDistrictGridResolution))
                {
                    // District grid resolution, i.e. 512 -> 900.
                    instruction.operand = ExpandedDistrictGridResolution;
                }
                else if (instruction.LoadsConstant(GameDistrictGridHalfResolution))
                {
                    // District grid half-resolution, i.e. 256 -> 450 - but only for first two instances, the rest are alpha calculations.
                    if (replacedCount < 2)
                    {
                        instruction.operand = ExpandedDistrictGridHalfResolution;
                        ++replacedCount;
                    }
                }

                yield return instruction;
            }
        }

        /// <summary>
        /// Harmony transpiler for DistrictManager.MoveParkBuildings to update code constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch("MoveParkBuildings")]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> MoveParkBuildingsTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceDistrictConstants(instructions);

        /// <summary>
        /// Harmony transpiler for DistrictManager.MoveParkNodes to update code constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch("MoveParkNodes")]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> MoveParkNodesTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceDistrictConstants(instructions);

        /// <summary>
        /// Harmony transpiler for DistrictManager.MoveParkProps to update code constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch("MoveParkProps")]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> MoveParkPropsTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceDistrictConstants(instructions);

        /// <summary>
        /// Harmony transpiler for DistrictManager.MoveParkSegments to update code constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch("MoveParkSegments")]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> MoveParkSegmentsTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceDistrictConstants(instructions);

        /// <summary>
        /// Harmony transpiler for DistrictManager.MoveParkTrees to update code constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch("MoveParkTrees")]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> MoveParkTreesTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceDistrictConstants(instructions);

        /// <summary>
        /// Harmony transpiler for DistrictManager.NamesModified to update code constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(DistrictManager.NamesModified), new Type[] { })]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> NamesModified1Transpiler(IEnumerable<CodeInstruction> instructions) => NamesModifiedTranspiler(instructions);

        /// <summary>
        /// Harmony transpiler for DistrictManager.NamesModified to update code constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(DistrictManager.NamesModified), new Type[] { typeof(Cell[]) })]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> NamesModified12ranspiler(IEnumerable<CodeInstruction> instructions)
        {
            // Two different replacements for 0xFF.
            int ffCount = 0;

            // Look for and update any relevant constants.
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.LoadsConstant(0xFF))
                {
                    // Want to replace 255 as district grid half-resolution, except for the third occurrence, which is a bitmask.
                    if (++ffCount != 3)
                    {
                        // District grid half-resolution, i.e. 256 -> 450.
                        instruction.operand = (int)ExpandedDistrictGridHalfResolution;
                    }
                    else
                    {
                        // 0xFF mask for district grid half width, i.e. 0xFF->0x1FF.
                        instruction.operand = 0x1FF;
                    }
                }
                else if (instruction.LoadsConstant(GameDistrictAreaSize / 2f))
                {
                    // District grid half-size, i.e. 4915.2f -> 8640f.
                    instruction.operand = (int)ExpandedDistrictGridHalfResolution;
                }

                yield return instruction;
            }
        }

        /// <summary>
        /// Harmony transpiler for DistrictManager.ParkNamesModified to update code constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(DistrictManager.ParkNamesModified))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> ParkNamesModifiedTranspiler(IEnumerable<CodeInstruction> instructions) => NamesModifiedTranspiler(instructions);

        /// <summary>
        /// Harmony transpiler for DistrictManager.SampleDistrict to update code constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(DistrictManager.SampleDistrict), new Type[] { typeof(Vector3), typeof(Cell[]) })]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> SampleDistrictTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            // Parse instructions.
            IEnumerator<CodeInstruction> instructionEnumerator = instructions.GetEnumerator();
            while (instructionEnumerator.MoveNext())
            {
                CodeInstruction instruction = instructionEnumerator.Current;

                // Insert custom code to replace original local variables 2 and 3.
                if (instruction.opcode == OpCodes.Stloc_1)
                {
                    yield return instruction;

                    // Skip original code until the call to Mathf.Clamp.
                    do
                    {
                        instructionEnumerator.MoveNext();
                        instruction = instructionEnumerator.Current;
                    }
                    while (instruction.opcode != OpCodes.Call);

                    // Insert custom code: (int)((woldPos.x / 19.2f) + 450f), 0f, 899.
                    yield return new CodeInstruction(OpCodes.Ldarga_S, 0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Vector3), nameof(Vector3.x)));
                    yield return new CodeInstruction(OpCodes.Ldc_R4, DISTRICTGRID_CELL_SIZE);
                    yield return new CodeInstruction(OpCodes.Div);
                    yield return new CodeInstruction(OpCodes.Ldc_R4, ExpandedDistrictGridHalfResolution);
                    yield return new CodeInstruction(OpCodes.Add);
                    yield return new CodeInstruction(OpCodes.Conv_I4);
                    yield return new CodeInstruction(OpCodes.Ldc_I4_0);
                    yield return new CodeInstruction(OpCodes.Ldc_I4, ExpandedDistrictGridMax);

                    // Return Mathf.clamp and store result.
                    yield return instruction;
                    yield return new CodeInstruction(OpCodes.Stloc_2);

                    // Same again for worldPos.z.
                    // Skip original code until the call to Mathf.Clamp.
                    do
                    {
                        instructionEnumerator.MoveNext();
                        instruction = instructionEnumerator.Current;
                    }
                    while (instruction.opcode != OpCodes.Call);

                    // Insert custom code: (int)((woldPos.z / 19.2f)) + 450f, 0f, 899.
                    yield return new CodeInstruction(OpCodes.Ldarga_S, 0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Vector3), nameof(Vector3.z)));
                    yield return new CodeInstruction(OpCodes.Ldc_R4, DISTRICTGRID_CELL_SIZE);
                    yield return new CodeInstruction(OpCodes.Div);
                    yield return new CodeInstruction(OpCodes.Ldc_R4, ExpandedDistrictGridHalfResolution);
                    yield return new CodeInstruction(OpCodes.Add);
                    yield return new CodeInstruction(OpCodes.Conv_I4);
                    yield return new CodeInstruction(OpCodes.Ldc_I4_0);
                    yield return new CodeInstruction(OpCodes.Ldc_I4, ExpandedDistrictGridMax);

                    // then just resume normal processing.
                }
                else if (instruction.LoadsConstant(GameDistrictGridArrayQuarterSize))
                {
                    // Halfgrid squared, i.e 256 * 256 -> 450 * 450.
                    instruction.operand = ExpandedDistrictGridArrayQuarterSize;
                }
                else if (instruction.LoadsConstant(128f))
                {
                    // Quarter district grid resolution, i.e. 128 -> 225.
                    instruction.operand = 450f;
                }
                else if (instruction.LoadsConstant(GameDistrictGridResolution))
                {
                    // District grid resolution, i.e. 512 -> 900.
                    instruction.operand = ExpandedDistrictGridResolution;
                }
                else if (instruction.LoadsConstant(GameDistrictGridMax))
                {
                    // Maximum iteration value: district grid resolution - 1 , i.e. 511 -> 899.
                    instruction.operand = ExpandedDistrictGridMax;
                }

                yield return instruction;
            }
        }

        /// <summary>
        /// Harmony transpiler for DistrictManager.UnsetCityPolicy to update code constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(DistrictManager.UnsetCityPolicy))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> UnsetCityPolicyTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceDistrictConstants(instructions);

        /// <summary>
        /// Harmony transpiler for DistrictManager.UpdateNames to update code constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch("UpdateNames")]
        [HarmonyTranspiler]
        internal static IEnumerable<CodeInstruction> UpdateNamesTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            // Inverse floating-point constants.
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.LoadsConstant(GameDistrictAreaSize))
                {
                    // Grid size in metres, i.e. 9830.4f -> 17280.
                    instruction.operand = ExpandedDistrictAreaSize;
                }

                yield return instruction;
            }
        }

        /// <summary>
        /// Harmony transpiler for DistrictManager.UpdateTexture to update code constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch("UpdateTexture")]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> UpdateTextureTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            // Parse instructions.
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.LoadsConstant(GameAreaManagerPatches.GameAreaGridResolution))
                {
                    // Area grid resolution, i.e. 5->9.
                    instruction.opcode = OpCodes.Ldc_I4;
                    instruction.operand = GameAreaManagerPatches.ExpandedAreaGridResolution;
                }
                else if (instruction.LoadsConstant((int)GameDistrictGridHalfResolution))
                {
                    // District grid half-resolution, i.e. 256 -> 450.
                    instruction.operand = (int)ExpandedDistrictGridHalfResolution;
                }
                else if (instruction.LoadsConstant(GameDistrictGridResolution))
                {
                    // District grid resolution, i.e. 512 -> 900.
                    instruction.operand = ExpandedDistrictGridResolution;
                }
                else if (instruction.LoadsConstant(GameDistrictGridResolution - 2))
                {
                    // District grid resolution - 2 , i.e. 510 -> 898.
                    instruction.operand = ExpandedDistrictGridResolution - 2;
                }
                else if (instruction.LoadsConstant(GameDistrictGridArrayQuarterSize))
                {
                    // Halfgrid squared, i.e 256 * 256 -> 450 * 450.
                    instruction.operand = ExpandedDistrictGridArrayQuarterSize;
                }

                yield return instruction;
            }
        }

        /// <summary>
        /// Harmony transpiler for DistrictManager.SetCityPolicy to update code constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(DistrictManager.SetCityPolicy))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> SetCityPolicyTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceDistrictConstants(instructions);

        /// <summary>
        /// Replaces any references to default constants in the provided code with updated 81 tile values.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        private static IEnumerable<CodeInstruction> ReplaceDistrictConstants(IEnumerable<CodeInstruction> instructions)
        {
            // Look for and update any relevant constants.
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.LoadsConstant(GameDistrictGridResolution))
                {
                    // District grid resolution, i.e. 512 -> 900.
                    instruction.operand = ExpandedDistrictGridResolution;
                }
                else if (instruction.LoadsConstant(GameDistrictGridMax))
                {
                    // Maximum iteration value: district grid resolution - 1 , i.e. 511 -> 899.
                    instruction.operand = ExpandedDistrictGridMax;
                }
                else if (instruction.LoadsConstant(GameDistrictGridHalfResolution))
                {
                    // District grid half-resolution, i.e. 256 -> 450.
                    instruction.operand = ExpandedDistrictGridHalfResolution;
                }

                yield return instruction;
            }
        }

        /// <summary>
        /// Replaces any references to default constants in the provided code with updated 81 tile values.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        private static IEnumerable<CodeInstruction> NamesModifiedTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            // Look for and update any relevant constants.
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.LoadsConstant(0xFF))
                {
                    // 0xFF mask for district grid half width, i.e. 0xFF->0x1FF.
                    instruction.operand = 0x1FF;
                }
                else if (instruction.LoadsConstant(GameDistrictAreaSize / 2f))
                {
                    // District grid half-size, i.e. 4915.2f -> 8640f.
                    instruction.operand = ExpandedDistrictAreaSize / 2f;
                }

                yield return instruction;
            }
        }
    }
}
