// <copyright file="WaterManagerPatches.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace EightyOne2.Patches
{
    using System;
    using System.Collections.Generic;
    using System.Reflection.Emit;
    using ColossalFramework;
    using ColossalFramework.Math;
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
        /// Pre-emptive Harmony prefix for WaterManager.UpdateGrid to implement 81 tiles functionality.
        /// </summary>
        /// <param name="__instance">WaterManager instance.</param>
        /// <param name="minX">Minimum X-coordinate of updated area.</param>
        /// <param name="minZ">Minimum Z-coordinate of updated area.</param>
        /// <param name="maxX">Maximum X-coordinate of updated area.</param>
        /// <param name="maxZ">Maximum Z-coordinate of updated area.</param>
        /// <param name="___m_waterGrid">WaterManager private array - m_waterGrid (water cell array).</param>
        /// <returns>Always false (never execute original method).</returns>
        [HarmonyPatch(nameof(WaterManager.UpdateGrid))]
        [HarmonyPrefix]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1313:Parameter names should begin with lower-case letter", Justification = "Harmony")]
        private static bool UpdateGrid(WaterManager __instance, float minX, float minZ, float maxX, float maxZ, Cell[] ___m_waterGrid)
        {
            int num = Mathf.Max((int)((minX / WATERGRID_CELL_SIZE) + ExpandedWaterGridHalfResolution), 0);
            int num2 = Mathf.Max((int)((minZ / WATERGRID_CELL_SIZE) + ExpandedWaterGridHalfResolution), 0);
            int num3 = Mathf.Min((int)((maxX / WATERGRID_CELL_SIZE) + ExpandedWaterGridHalfResolution), ExpandedWaterGridMax);
            int num4 = Mathf.Min((int)((maxZ / WATERGRID_CELL_SIZE) + ExpandedWaterGridHalfResolution), ExpandedWaterGridMax);
            for (int i = num2; i <= num4; i++)
            {
                int num5 = (i * ExpandedWaterGridResolution) + num;
                for (int j = num; j <= num3; j++)
                {
                    ___m_waterGrid[num5].m_conductivity = 0;
                    ___m_waterGrid[num5].m_conductivity2 = 0;
                    ___m_waterGrid[num5].m_closestPipeSegment = 0;
                    ___m_waterGrid[num5].m_closestPipeSegment2 = 0;
                    num5++;
                }
            }

            float num6 = ((num - ExpandedWaterGridHalfResolution) * WATERGRID_CELL_SIZE) - 100f;
            float num7 = ((num2 - ExpandedWaterGridHalfResolution) * WATERGRID_CELL_SIZE) - 100f;
            float num8 = ((num3 - ExpandedWaterGridHalfResolution + 1f) * WATERGRID_CELL_SIZE) + 100f;
            float num9 = ((num4 - ExpandedWaterGridHalfResolution + 1f) * WATERGRID_CELL_SIZE) + 100f;
            int num10 = Mathf.Max((int)((num6 / 64f) + 135f), 0);
            int num11 = Mathf.Max((int)((num7 / 64f) + 135f), 0);
            int num12 = Mathf.Min((int)((num8 / 64f) + 135f), 269);
            int num13 = Mathf.Min((int)((num9 / 64f) + 135f), 269);
            float num14 = 100f;
            Array16<NetNode> nodes = Singleton<NetManager>.instance.m_nodes;
            Array16<NetSegment> segments = Singleton<NetManager>.instance.m_segments;
            ushort[] segmentGrid = Singleton<NetManager>.instance.m_segmentGrid;
            for (int k = num11; k <= num13; k++)
            {
                for (int l = num10; l <= num12; l++)
                {
                    ushort num15 = segmentGrid[(k * 270) + l];
                    int num16 = 0;
                    while (num15 != 0)
                    {
                        NetSegment.Flags flags = segments.m_buffer[num15].m_flags;
                        if ((flags & (NetSegment.Flags.Created | NetSegment.Flags.Deleted)) == NetSegment.Flags.Created)
                        {
                            NetInfo info = segments.m_buffer[num15].Info;
                            if (info.m_class.m_service == ItemClass.Service.Water && info.m_class.m_level <= ItemClass.Level.Level2)
                            {
                                ushort startNode = segments.m_buffer[num15].m_startNode;
                                ushort endNode = segments.m_buffer[num15].m_endNode;
                                Vector2 a = VectorUtils.XZ(nodes.m_buffer[startNode].m_position);
                                Vector2 b = VectorUtils.XZ(nodes.m_buffer[endNode].m_position);
                                float num17 = Mathf.Max(Mathf.Max(num6 - a.x, num7 - a.y), Mathf.Max(a.x - num8, a.y - num9));
                                float num18 = Mathf.Max(Mathf.Max(num6 - b.x, num7 - b.y), Mathf.Max(b.x - num8, b.y - num9));
                                if (num17 < 0f || num18 < 0f)
                                {
                                    int num19 = Mathf.Max((int)(((Mathf.Min(a.x, b.x) - num14) / WATERGRID_CELL_SIZE) + ExpandedWaterGridHalfResolution), num);
                                    int num20 = Mathf.Max((int)(((Mathf.Min(a.y, b.y) - num14) / WATERGRID_CELL_SIZE) + ExpandedWaterGridHalfResolution), num2);
                                    int num21 = Mathf.Min((int)(((Mathf.Max(a.x, b.x) + num14) / WATERGRID_CELL_SIZE) + ExpandedWaterGridHalfResolution), num3);
                                    int num22 = Mathf.Min((int)(((Mathf.Max(a.y, b.y) + num14) / WATERGRID_CELL_SIZE) + ExpandedWaterGridHalfResolution), num4);
                                    for (int m = num20; m <= num22; m++)
                                    {
                                        int num23 = (m * ExpandedWaterGridResolution) + num19;
                                        float y = (m + 0.5f - ExpandedWaterGridHalfResolution) * WATERGRID_CELL_SIZE;
                                        for (int n = num19; n <= num21; n++)
                                        {
                                            float x = (n + 0.5f - ExpandedWaterGridHalfResolution) * WATERGRID_CELL_SIZE;
                                            float f = Segment2.DistanceSqr(a, b, new Vector2(x, y), out var _);
                                            f = Mathf.Sqrt(f);
                                            if (f < num14 + 19.125f)
                                            {
                                                float num24 = ((num14 - f) * (2f / 153f)) + 0.25f;
                                                int num25 = Mathf.Min(255, Mathf.RoundToInt(num24 * 255f));
                                                if (num25 > ___m_waterGrid[num23].m_conductivity)
                                                {
                                                    ___m_waterGrid[num23].m_conductivity = (byte)num25;
                                                    ___m_waterGrid[num23].m_closestPipeSegment = num15;
                                                }

                                                if (info.m_class.m_level == ItemClass.Level.Level2 && num25 > ___m_waterGrid[num23].m_conductivity2)
                                                {
                                                    ___m_waterGrid[num23].m_conductivity2 = (byte)num25;
                                                    ___m_waterGrid[num23].m_closestPipeSegment2 = num15;
                                                }
                                            }

                                            num23++;
                                        }
                                    }
                                }
                            }
                        }

                        num15 = segments.m_buffer[num15].m_nextGridSegment;
                        if (++num16 >= 32768)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                            break;
                        }
                    }
                }
            }

            for (int num26 = num2; num26 <= num4; num26++)
            {
                int num27 = (num26 * ExpandedWaterGridResolution) + num;
                for (int num28 = num; num28 <= num3; num28++)
                {
                    Cell cell = ___m_waterGrid[num27];
                    if (cell.m_conductivity == 0)
                    {
                        cell.m_currentWaterPressure = 0;
                        cell.m_currentSewagePressure = 0;
                        cell.m_currentHeatingPressure = 0;
                        cell.m_waterPulseGroup = ushort.MaxValue;
                        cell.m_sewagePulseGroup = ushort.MaxValue;
                        cell.m_heatingPulseGroup = ushort.MaxValue;
                        cell.m_tmpHasWater = false;
                        cell.m_tmpHasSewage = false;
                        cell.m_tmpHasHeating = false;
                        cell.m_hasWater = false;
                        cell.m_hasSewage = false;
                        cell.m_hasHeating = false;
                        cell.m_pollution = 0;
                        ___m_waterGrid[num27] = cell;
                    }
                    else if (cell.m_conductivity2 == 0)
                    {
                        cell.m_currentHeatingPressure = 0;
                        cell.m_heatingPulseGroup = ushort.MaxValue;
                        cell.m_tmpHasHeating = false;
                        cell.m_hasHeating = false;
                        ___m_waterGrid[num27] = cell;
                    }

                    num27++;
                }
            }

            __instance.AreaModified(num, num2, num3, num4);

            return false;
        }

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
