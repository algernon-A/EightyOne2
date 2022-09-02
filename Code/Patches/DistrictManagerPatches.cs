// <copyright file="DistrictManagerPatches.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace EightyOne2
{
    using System;
    using System.Collections.Generic;
    using System.Reflection.Emit;
    using ColossalFramework;
    using HarmonyLib;
    using UnityEngine;
    using static DistrictManager;

    /// <summary>
    /// Harmony patches for the district manager to implement 81 tiles functionality.
    /// </summary>
    [HarmonyPatch(typeof(DistrictManager))]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1313:Parameter names should begin with lower-case letter", Justification = "Harmony")]
    internal static class DistrictManagerPatches
    {
        /// <summary>
        /// Game district grid width and height = 512.
        /// Logically, the game grid should be 500 using the given cell size (19.2), but they've gone with 512.   Not sure if rounding or overlap.
        /// </summary>
        internal const int GameDistrictGridResolution = DISTRICTGRID_RESOLUTION;

        /// <summary>
        /// Expanded district grid width and height = 900.
        /// </summary>
        internal const int ExpandedDistrictGridResolution = 900;

        /// <summary>
        /// Expanded district grid array size = 900 * 900 = 810000.
        /// </summary>
        internal const int ExpandedDistrictGridArraySize = ExpandedDistrictGridResolution * ExpandedDistrictGridResolution;

        /// <summary>
        /// Game district grid half-resolution = 512 / 2f = 256f.
        /// </summary>
        internal const float GameDistrictGridHalfResolution = GameDistrictGridResolution / 2f;

        /// <summary>
        /// Expanded district grid half-resolution = 900 / 2f = 450f.
        /// </summary>
        internal const float ExpandedDistrictGridHalfResolution = ExpandedDistrictGridResolution / 2f;

        /// <summary>
        /// Game district grid maximum bound (length - 1) = 512 - 1 = 511.
        /// </summary>
        internal const int GameDistrictGridMax = GameDistrictGridResolution - 1;

        /// <summary>
        /// Expanded district grid maximum bound (length - 1) = 900 - 1 = 899.
        /// </summary>
        internal const int ExpandedDistrictGridMax = ExpandedDistrictGridResolution - 1;

        // Derived constants.
        private const int GameDistrictGridArraySize = GameDistrictGridResolution * GameDistrictGridResolution;
        private const int GameDistrictGridArrayQuarterSize = (int)GameDistrictGridHalfResolution * (int)GameDistrictGridHalfResolution;
        private const int ExpandedDistrictGridArrayQuarterSize = (int)ExpandedDistrictGridHalfResolution * (int)ExpandedDistrictGridHalfResolution;
        private const float GameDistrictAreaDistance = GameDistrictGridResolution * DISTRICTGRID_CELL_SIZE;
        private const float ExpandedDistrictAreaDistance = ExpandedDistrictGridResolution * DISTRICTGRID_CELL_SIZE;
        private const float GameDistrictAreaHalfDistance = GameDistrictGridHalfResolution * DISTRICTGRID_CELL_SIZE;
        private const float ExpandedDistrictAreaHalfDistance = ExpandedDistrictGridHalfResolution * DISTRICTGRID_CELL_SIZE;

        // Equivalents of private game arrays using expanded field sizes.
        private static readonly uint[] DistanceBuffer = new uint[ExpandedDistrictGridArrayQuarterSize];
        private static readonly uint[] IndexBuffer = new uint[ExpandedDistrictGridArrayQuarterSize];
        private static readonly TempDistrictData[] TempData = new TempDistrictData[128];

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
            int skippedCount = 0;
            foreach (CodeInstruction instruction in ReplaceDistrictConstants(instructions))
            {
                if (instruction.LoadsConstant(GameDistrictGridArraySize))
                {
                    // Targeting the m_colorBuffer init.
                    // Skip first two instances assigning m_districtGrid and m_parkGrid.
                    if (++skippedCount > 2)
                    {
                        // District grid size, i.e. 262144 -> 810000.
                        instruction.operand = ExpandedDistrictGridArraySize;
                    }
                }
                else if (instruction.LoadsConstant(GameDistrictGridResolution))
                {
                    // District grid resolution, i.e. 512 -> 900.
                    // Used for texture sizes.
                    instruction.operand = ExpandedDistrictGridResolution;
                }
                else if (instruction.LoadsConstant(GameDistrictGridMax))
                {
                    // Maximum iteration value: district grid resolution - 1 , i.e. 511 -> 899.
                    // Used to set initial district/park modified area max.
                    instruction.operand = ExpandedDistrictGridMax;
                }

                // Don't care about GameDistrictGridArrayQuarterSize, as m_distanceBuffer and m_indexBuffer are replaced by our custom static version.
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
                if (instruction.LoadsConstant(1f / GameDistrictAreaDistance))
                {
                    // 0.000101725258f.
                    instruction.operand = 1f / ExpandedDistrictAreaDistance;
                }
                else if (instruction.LoadsConstant(GameDistrictAreaDistance))
                {
                    // Grid size in metres, i.e. 9830.4f -> 17280.
                    instruction.operand = ExpandedDistrictAreaDistance;
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
        /// Pre-emptive Harmony prefix for DistrictManager.NamesModified to implement 81 tiles functionality using upsided fields and constants.
        /// </summary>
        /// <param name="__instance">DistrictManager instance.</param>
        /// <param name="___m_namesModified">DistrictManager private field m_namesModified.</param>
        /// <returns>Always false (never execute original method).</returns>
        [HarmonyPatch(nameof(DistrictManager.NamesModified), new Type[] { })]
        [HarmonyPrefix]
        private static bool NamesModified1Prefix(DistrictManager __instance, ref bool ___m_namesModified)
        {
            NamesModified2Prefix(__instance.m_districtGrid);
            Vector3 vector = default;
            for (int i = 0; i < 128; i++)
            {
                uint bestLocation = TempData[i].m_bestLocation;
                vector.x = (DISTRICTGRID_CELL_SIZE * (float)(bestLocation % ExpandedDistrictGridHalfResolution) * 2f) - ExpandedDistrictAreaHalfDistance;
                vector.y = 0f;
                vector.z = (DISTRICTGRID_CELL_SIZE * (float)(bestLocation / ExpandedDistrictGridHalfResolution) * 2f) - ExpandedDistrictAreaHalfDistance;
                vector.y = Singleton<TerrainManager>.instance.SampleRawHeightSmoothWithWater(vector, timeLerp: false, 0f);
                __instance.m_districts.m_buffer[i].m_nameLocation = vector;
            }

            ___m_namesModified = true;

            // Don't execute original method.
            return false;
        }

        /// <summary>
        /// Pre-emptive Harmony prefix for DistrictManager.NamesModified to implement 81 tiles functionality using upsided fields and constants.
        /// </summary>
        /// <param name="grid">District cell grid.</param>
        /// <returns>Always false (never execute original method).</returns>
        [HarmonyPatch(nameof(DistrictManager.NamesModified), new Type[] { typeof(Cell[]) })]
        [HarmonyPrefix]
        private static bool NamesModified2Prefix(Cell[] grid)
        {
            // Reverse-engineering and figuring this one out was a right pain.

            // Reset distance buffer.
            for (int i = 0; i < DistanceBuffer.Length; ++i)
            {
                DistanceBuffer[i] = 0;
            }

            // Reset temporary data.
            for (int i = 0; i < 128; ++i)
            {
                TempData[i] = default;
            }

            // Constants.
            const int gridMargin = 2;
            const int doubleGridResolution = ExpandedDistrictGridResolution * 2;
            const int HalfGridResolutionMax = (int)ExpandedDistrictGridHalfResolution - 1;

            // Populate distance and index buffers to calculate district average positions.
            int currentBufferIndex = 0;
            for (int z = 0; z < ExpandedDistrictGridHalfResolution; ++z)
            {
                for (int x = 0; x < ExpandedDistrictGridHalfResolution; ++x)
                {
                    int gridIndex = (z * doubleGridResolution) + (x * gridMargin);
                    byte district = grid[gridIndex].m_district1;
                    if (district != 0 && (
                        x == 0
                        || z == 0
                        || x == HalfGridResolutionMax
                        || z == HalfGridResolutionMax
                        || grid[gridIndex - doubleGridResolution].m_district1 != district
                        || grid[gridIndex - gridMargin].m_district1 != district
                        || grid[gridIndex + gridMargin].m_district1 != district
                        || grid[gridIndex + doubleGridResolution].m_district1 != district))
                    {
                        uint bufferIndex = (uint)((z * (int)ExpandedDistrictGridHalfResolution) + x);
                        DistanceBuffer[bufferIndex] = 1;
                        IndexBuffer[currentBufferIndex] = bufferIndex;
                        currentBufferIndex = (currentBufferIndex + 1) % ExpandedDistrictGridArrayQuarterSize;
                        TempData[district].m_averageX += x;
                        TempData[district].m_averageZ += z;
                        ++TempData[district].m_divider;
                    }
                }
            }

            // Update district averages based on number of records.
            for (int i = 0; i < 128; ++i)
            {
                int divider = TempData[i].m_divider;
                if (divider != 0)
                {
                    TempData[i].m_averageX = (TempData[i].m_averageX + (divider >> 1)) / divider;
                    TempData[i].m_averageZ = (TempData[i].m_averageZ + (divider >> 1)) / divider;
                }
            }

            // Determine best location for name.
            int nextBufferIndex = 0;
            while (nextBufferIndex != currentBufferIndex)
            {
                uint bufferIndex = IndexBuffer[nextBufferIndex];
                nextBufferIndex = (nextBufferIndex + 1) % ExpandedDistrictGridArrayQuarterSize;
                uint x = bufferIndex % (int)ExpandedDistrictGridHalfResolution;
                uint z = bufferIndex / (int)ExpandedDistrictGridHalfResolution;
                uint gridIndex = (uint)((z * doubleGridResolution) + (x * gridMargin));
                byte district = grid[gridIndex].m_district1;
                int deltaX = (int)x - TempData[district].m_averageX;
                int deltaZ = (int)z - TempData[district].m_averageZ;

                // Best score - closest match.
                int bestScore = ExpandedDistrictGridArraySize - ((ExpandedDistrictGridArraySize / 2) / (int)DistanceBuffer[bufferIndex]) - (deltaX * deltaX) - (deltaZ * deltaZ);
                if (bestScore > TempData[district].m_bestScore)
                {
                    TempData[district].m_bestScore = bestScore;
                    TempData[district].m_bestLocation = bufferIndex;
                }

                uint previousBufferIndex = bufferIndex - 1;
                if (x > 0 && DistanceBuffer[previousBufferIndex] == 0 && grid[gridIndex - gridMargin].m_district1 == district)
                {
                    DistanceBuffer[previousBufferIndex] = DistanceBuffer[bufferIndex] + 1;
                    IndexBuffer[currentBufferIndex] = previousBufferIndex;
                    currentBufferIndex = (currentBufferIndex + 1) % ExpandedDistrictGridArrayQuarterSize;
                }

                previousBufferIndex = bufferIndex + 1;
                if (x < HalfGridResolutionMax && DistanceBuffer[previousBufferIndex] == 0 && grid[gridIndex + gridMargin].m_district1 == district)
                {
                    DistanceBuffer[previousBufferIndex] = DistanceBuffer[bufferIndex] + 1;
                    IndexBuffer[currentBufferIndex] = previousBufferIndex;
                    currentBufferIndex = (currentBufferIndex + 1) % ExpandedDistrictGridArrayQuarterSize;
                }

                previousBufferIndex = bufferIndex - (int)ExpandedDistrictGridHalfResolution;
                if (z > 0 && DistanceBuffer[previousBufferIndex] == 0 && grid[gridIndex - doubleGridResolution].m_district1 == district)
                {
                    DistanceBuffer[previousBufferIndex] = DistanceBuffer[bufferIndex] + 1;
                    IndexBuffer[currentBufferIndex] = previousBufferIndex;
                    currentBufferIndex = (currentBufferIndex + 1) % ExpandedDistrictGridArrayQuarterSize;
                }

                previousBufferIndex = bufferIndex + (int)ExpandedDistrictGridHalfResolution;
                if (z < HalfGridResolutionMax && DistanceBuffer[previousBufferIndex] == 0 && grid[gridIndex + doubleGridResolution].m_district1 == district)
                {
                    DistanceBuffer[previousBufferIndex] = DistanceBuffer[bufferIndex] + 1;
                    IndexBuffer[currentBufferIndex] = previousBufferIndex;
                    currentBufferIndex = (currentBufferIndex + 1) % ExpandedDistrictGridArrayQuarterSize;
                }
            }

            // Don't execute original method.
            return false;
        }

        /// <summary>
        /// Pre-emptive Harmony prefix for DistrictManager.ParkNamesModified to implement 81 tiles functionality using upsided fields and constants.
        /// </summary>
        /// <param name="__instance">DistrictManager instance.</param>
        /// <param name="___m_namesModified">DistrictManager private field m_namesModified.</param>
        /// <returns>Always false (never execute original method).</returns>
        [HarmonyPatch(nameof(DistrictManager.ParkNamesModified))]
        [HarmonyPrefix]
        private static bool ParkNamesModifiedPrefix(DistrictManager __instance, ref bool ___m_namesModified)
        {
            NamesModified2Prefix(__instance.m_parkGrid);
            Vector3 vector = default;
            for (int i = 0; i < 128; i++)
            {
                uint bestLocation = TempData[i].m_bestLocation;
                vector.x = (DISTRICTGRID_CELL_SIZE * (float)(bestLocation % ExpandedDistrictGridHalfResolution) * 2f) - ExpandedDistrictAreaHalfDistance;
                vector.y = 0f;
                vector.z = (DISTRICTGRID_CELL_SIZE * (float)(bestLocation / ExpandedDistrictGridHalfResolution) * 2f) - ExpandedDistrictAreaHalfDistance;
                vector.y = Singleton<TerrainManager>.instance.SampleRawHeightSmoothWithWater(vector, timeLerp: false, 0f);
                __instance.m_parks.m_buffer[i].m_nameLocation = vector;
            }

            ___m_namesModified = true;

            // Don't execute original method.
            return false;
        }

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
        private static IEnumerable<CodeInstruction> UpdateNamesTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            // Inverse floating-point constants.
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.LoadsConstant(GameDistrictAreaDistance))
                {
                    // Grid size in metres, i.e. 9830.4f -> 17280.
                    instruction.operand = ExpandedDistrictAreaDistance;
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
                else if (instruction.LoadsConstant(GameDistrictAreaDistance / 2f))
                {
                    // District grid half-size, i.e. 4915.2f -> 8640f.
                    instruction.operand = ExpandedDistrictAreaDistance / 2f;
                }

                yield return instruction;
            }
        }

        /// <summary>
        /// Mirror of game DistrictManager.TempDistrictData private struct, with short fields expanded to int..
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1307:Accessible fields should begin with upper-case letter", Justification = "Mirror game naming")]
        private struct TempDistrictData
        {
            public int m_averageX;

            public int m_averageZ;

            public int m_bestScore;

            public int m_divider;

            public uint m_bestLocation;
        }
    }
}
