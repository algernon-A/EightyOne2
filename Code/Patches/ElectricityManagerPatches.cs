// <copyright file="ElectricityManagerPatches.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace EightyOne2
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using AlgernonCommons;
    using ColossalFramework;
    using HarmonyLib;
    using UnityEngine;
    using static ElectricityManager;

    /// <summary>
    /// Harmomy patches for the game's electricity manager to implement 81 tiles functionality.
    /// </summary>
    // [HarmonyPatch(typeof(ElectricityManager))]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1313:Parameter names should begin with lower-case letter", Justification = "Harmony")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1311:Static readonly fields should begin with upper-case letter", Justification = "Dotnet style")]
    internal static class ElectricityManagerPatches
    {
        /// <summary>
        /// Game electricity grid array size.
        /// </summary>
        internal const int GameElectricyGridSize = ELECTRICITYGRID_RESOLUTION * ELECTRICITYGRID_RESOLUTION;

        /// <summary>
        /// Expanded electricity grid array size.
        /// </summary>
        internal const int ExpandedElectricityGridSize = ExpandedElectricityGridResolution * ExpandedElectricityGridResolution;

        /// <summary>
        /// Game electricity grid height and width.
        /// </summary>
        internal const int GameElectricyGridResolution = ELECTRICITYGRID_RESOLUTION;

        /// <summary>
        /// Expanded electricity grid height and width.
        /// </summary>
        internal const int ExpandedElectricityGridResolution = 462;

        /// <summary>
        /// Game electricty grid half-resolution.
        /// </summary>
        internal const int GameElectricyGridHalfResolution = GameElectricyGridResolution / 2;

        /// <summary>
        /// Expanded electricty grid half-resolution.
        /// </summary>
        internal const int ExpandedElectricityGridHalfResolution = ExpandedElectricityGridResolution / 2;

        // Limits.
        private const int GameElectricityGridMax = GameElectricyGridResolution - 1;
        private const int ExpandedElectricityGridMax = ExpandedElectricityGridResolution - 1;

        // Private arrays.
        private static readonly ExpandedPulseGroup[] s_pulseGroups = new ExpandedPulseGroup[1024];
        private static readonly ExpandedPulseUnit[] s_pulseUnits = new ExpandedPulseUnit[32786];

        /// <summary>
        /// Gets the expanded pulse group array.
        /// </summary>
        internal static ExpandedPulseGroup[] PulseGroups => s_pulseGroups;

        /// <summary>
        /// Gets the expanded pulse unit array.
        /// </summary>
        internal static ExpandedPulseUnit[] PulseUnits => s_pulseUnits;

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
                if (instruction.LoadsConstant(GameElectricyGridResolution))
                {
                    // Electricity grid size, i.e. 256->462.
                    instruction.operand = ExpandedElectricityGridResolution;
                }
                else if (instruction.LoadsConstant(GameElectricityGridMax))
                {
                    // Electricity grid array maximum index i.e. 255->461.
                    instruction.operand = ExpandedElectricityGridMax;
                }
                else if (instruction.LoadsConstant(GameElectricyGridHalfResolution))
                {
                    // Electricity grid array half-size i.e. 128->231.
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

        /*
        /// <summary>
        /// Harmony transpiler for ElectricityManager.ConductToCell to replace hardcoded game constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch("ConductToCell")]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> ConductToCellTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceElectricityConstants(instructions);
        */

        /// <summary>
        /// Pre-emptive Harmony prefix for ElectricityManager.ConductToCell due to the complexity of transpiling this one.
        /// </summary>
        /// <param name="cell">Target electricity cell.</param>
        /// <param name="group">Group ID.</param>
        /// <param name="x">Cell x-coordinate.</param>
        /// <param name="z">Cell z-coordinate.</param>
        /// <param name="limit">Minimum required condutivity.</param>
        /// <param name="___m_electricityGrid">ElectricityManager private field - m_electricityGrid (electricity cell array).</param>
        /// <param name="___m_pulseGroupCount">ElectricityManager private field - m_pulseGroupCount.</param>
        /// <param name="___m_pulseUnitEnd">ElectricityManager private field - m_pulseUnitEnd.</param>
        /// <param name="___m_canContinue">ElectricityManager private field - m_canContinue.</param>
        /// <returns>Always false (never execute original method).</returns>
        [HarmonyPatch("ConductToCell")]
        [HarmonyPrefix]
        private static bool ConductToCellPrefix(ref Cell cell, ushort group, int x, int z, int limit, Cell[] ___m_electricityGrid, int ___m_pulseGroupCount, ref int ___m_pulseUnitEnd, ref bool ___m_canContinue)
        {
            if (cell.m_conductivity < limit)
            {
                // Don't execute original method.
                return false;
            }

            if (cell.m_conductivity < 64)
            {
                bool flag = true;
                bool flag2 = true;
                int num = (z * ExpandedElectricityGridResolution) + x;
                if (x > 0 && ___m_electricityGrid[num - 1].m_conductivity >= 64)
                {
                    flag = false;
                }

                if (x < ExpandedElectricityGridMax && ___m_electricityGrid[num + 1].m_conductivity >= 64)
                {
                    flag = false;
                }

                if (z > 0 && ___m_electricityGrid[num - ExpandedElectricityGridResolution].m_conductivity >= 64)
                {
                    flag2 = false;
                }

                if (z < ExpandedElectricityGridMax && ___m_electricityGrid[num + ExpandedElectricityGridResolution].m_conductivity >= 64)
                {
                    flag2 = false;
                }

                if (flag || flag2)
                {
                    // Don't execute original method.
                    return false;
                }
            }

            if (cell.m_pulseGroup == ushort.MaxValue)
            {
                ExpandedPulseUnit pulseUnit = default;
                pulseUnit.m_group = group;
                pulseUnit.m_node = 0;
                pulseUnit.m_x = (byte)x;
                pulseUnit.m_z = (byte)z;
                s_pulseUnits[___m_pulseUnitEnd] = pulseUnit;
                if (++___m_pulseUnitEnd == s_pulseUnits.Length)
                {
                    ___m_pulseUnitEnd = 0;
                }

                cell.m_pulseGroup = group;
                ___m_canContinue = true;
            }
            else
            {
                 GetRootGroupPrefix(out ushort rootGroup, cell.m_pulseGroup);
                 if (rootGroup != group)
                {
                    MergeGroupsPrefix(group, rootGroup, ___m_pulseGroupCount);
                    cell.m_pulseGroup = group;
                    ___m_canContinue = true;
                }
            }

            // Don't execute original method.
            return false;
        }

        /*
        /// <summary>
        /// Harmony transpiler for ElectricityManager.ConductToCells to replace hardcoded game constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch("ConductToCells")]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> ConductToCellsTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceElectricityConstants(instructions);
        */

        /// <summary>
        /// Pre-emptive Harmony prefix for ElectricityManager.ConductToNode due to the complexity of transpiling this one.
        /// </summary>>
        /// <param name="group">Group ID.</param>
        /// <param name="worldX">World x-coordinate.</param>
        /// <param name="worldZ">World z-coordinate.</param>
        /// <param name="___m_electricityGrid">ElectricityManager private field - m_electricityGrid (electricity cell array).</param>
        /// <param name="___m_pulseGroupCount">ElectricityManager private field - m_pulseGroupCount.</param>
        /// <param name="___m_pulseUnitEnd">ElectricityManager private field - m_pulseUnitEnd.</param>
        /// <param name="___m_canContinue">ElectricityManager private field - m_canContinue.</param>
        /// <returns>Always false (never execute original method).</returns>
        [HarmonyPatch("ConductToCells")]
        [HarmonyPrefix]
        private static bool ConductToCellsPrefix(ushort group, float worldX, float worldZ, Cell[] ___m_electricityGrid, int ___m_pulseGroupCount, ref int ___m_pulseUnitEnd, ref bool ___m_canContinue)
        {
            int num = (int)((worldX / ELECTRICITYGRID_CELL_SIZE) + ExpandedElectricityGridHalfResolution);
            int num2 = (int)((worldZ / ELECTRICITYGRID_CELL_SIZE) + ExpandedElectricityGridHalfResolution);
            if (num >= 0 && num < ExpandedElectricityGridResolution && num2 >= 0 && num2 < ExpandedElectricityGridResolution)
            {
                int num3 = (num2 * ExpandedElectricityGridResolution) + num;
                ConductToCellPrefix(ref ___m_electricityGrid[num3], group, num, num2, 1, ___m_electricityGrid, ___m_pulseGroupCount, ref ___m_pulseUnitEnd, ref ___m_canContinue);
            }

            // Don't execute original method.
            return false;
        }

        /// <summary>
        /// Pre-emptive Harmony prefix for ElectricityManager.ConductToNode due to the complexity of transpiling this one.
        /// </summary>
        /// <param name="__instance">ElectricityManager instance.</param>
        /// <param name="nodeIndex">Node index ID.</param>
        /// <param name="node">Node data.</param>
        /// <param name="group">Group ID.</param>
        /// <param name="minX">Minimum node x-position.</param>
        /// <param name="minZ">Minimum node z-position.</param>
        /// <param name="maxX">Maximum node x-position.</param>
        /// <param name="maxZ">Maximum node z-position.</param>
        /// <param name="___m_pulseGroupCount">ElectricityManager private field - m_pulseGroupCount.</param>
        /// <param name="___m_pulseUnitEnd">ElectricityManager private field - m_pulseUnitEnd.</param>
        /// <param name="___m_canContinue">ElectricityManager private field - m_canContinue.</param>
        /// <returns>Always false (never execute original method).</returns>
        [HarmonyPatch("ConductToNode")]
        [HarmonyPrefix]
        private static bool ConductToNodePrefix(ElectricityManager __instance, ushort nodeIndex, ref NetNode node, ushort group, float minX, float minZ, float maxX, float maxZ, int ___m_pulseGroupCount, ref int ___m_pulseUnitEnd, ref bool ___m_canContinue)
        {
            if (!(node.m_position.x >= minX) || !(node.m_position.z >= minZ) || !(node.m_position.x <= maxX) || !(node.m_position.z <= maxZ))
            {
                // Don't execute original method.
                return false;
            }

            NetInfo info = node.Info;
            if (info.m_class.m_service != ItemClass.Service.Electricity && !(node.Info.m_netAI is ConcourseAI))
            {
                // Don't execute original method.
                return false;
            }

            if (__instance.m_nodeGroups[nodeIndex] == ushort.MaxValue)
            {
                ExpandedPulseUnit pulseUnit = default;
                pulseUnit.m_group = group;
                pulseUnit.m_node = nodeIndex;
                pulseUnit.m_x = 0;
                pulseUnit.m_z = 0;
                s_pulseUnits[___m_pulseUnitEnd] = pulseUnit;
                if (++___m_pulseUnitEnd == s_pulseUnits.Length)
                {
                    ___m_pulseUnitEnd = 0;
                }

                __instance.m_nodeGroups[nodeIndex] = group;
                ___m_canContinue = true;
            }
            else
            {
                GetRootGroupPrefix(out ushort rootGroup, __instance.m_nodeGroups[nodeIndex]);
                if (rootGroup != group)
                {
                    MergeGroupsPrefix(group, rootGroup, ___m_pulseGroupCount);
                    __instance.m_nodeGroups[nodeIndex] = group;
                    ___m_canContinue = true;
                }
            }

            // Don't execute original method.
            return false;
        }

        /// <summary>
        /// Pre-emptive Harmony prefix for ElectricityManager.ConductToNodes due to the complexity of transpiling this one.
        /// </summary>
        /// <param name="__instance">ElectricityManager instance.</param>
        /// <param name="group">Group ID.</param>
        /// <param name="cellX">Cell x-coordinate.</param>
        /// <param name="cellZ">Cell z-coordinate.</param>
        /// <param name="___m_pulseGroupCount">ElectricityManager private field - m_pulseGroupCount.</param>
        /// <param name="___m_pulseUnitEnd">ElectricityManager private field - m_pulseUnitEnd.</param>
        /// <param name="___m_canContinue">ElectricityManager private field - m_canContinue.</param>
        /// <returns>Always false (never execute original method).</returns>
        [HarmonyPatch("ConductToNodes")]
        [HarmonyPrefix]
        private static bool ConductToNodesPrefix(ElectricityManager __instance, ushort group, int cellX, int cellZ, int ___m_pulseGroupCount, ref int ___m_pulseUnitEnd, ref bool ___m_canContinue)
        {
            float num = ((float)cellX - ExpandedElectricityGridHalfResolution) * ELECTRICITYGRID_CELL_SIZE;
            float num2 = ((float)cellZ - ExpandedElectricityGridHalfResolution) * ELECTRICITYGRID_CELL_SIZE;
            float num3 = num + ELECTRICITYGRID_CELL_SIZE;
            float num4 = num2 + ELECTRICITYGRID_CELL_SIZE;
            int num5 = Mathf.Max((int)((num / 64f) + 135f), 0);
            int num6 = Mathf.Max((int)((num2 / 64f) + 135f), 0);
            int num7 = Mathf.Min((int)((num3 / 64f) + 135f), 269);
            int num8 = Mathf.Min((int)((num4 / 64f) + 135f), 269);
            NetManager netManager = Singleton<NetManager>.instance;
            for (int i = num6; i <= num8; i++)
            {
                for (int j = num5; j <= num7; j++)
                {
                    ushort num9 = netManager.m_nodeGrid[(i * 270) + j];
                    int num10 = 0;
                    while (num9 != 0)
                    {
                        ConductToNodePrefix(__instance, num9, ref netManager.m_nodes.m_buffer[num9], group, num, num2, num3, num4, ___m_pulseGroupCount, ref ___m_pulseUnitEnd, ref ___m_canContinue);
                        num9 = netManager.m_nodes.m_buffer[num9].m_nextGridNode;
                        if (++num10 >= 32768)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                            break;
                        }
                    }
                }
            }

            // Don't execute original method.
            return false;
        }

        /// <summary>
        /// Harmony transpiler for ElectricityManager.ConductToNodes to replace hardcoded game constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch("ConductToNodes")]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> ConductToNodesTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceElectricityConstants(instructions);

        /// <summary>
        /// Pre-emptive Harmony prefix for ElectricityManager.GetRootGroup due to the complexity of transpiling this one.
        /// </summary>
        /// <param name="__result">Original method result.</param>
        /// <param name="group">Group ID.</param>
        /// <returns>Always false (never execute original method).</returns>
        [HarmonyPatch("GetRootGroup")]
        [HarmonyPrefix]
        private static bool GetRootGroupPrefix(out ushort __result, ushort group)
        {
            for (ushort mergeIndex = s_pulseGroups[group].m_mergeIndex; mergeIndex != ushort.MaxValue; mergeIndex = s_pulseGroups[group].m_mergeIndex)
            {
                group = mergeIndex;
            }

            __result = group;

            // Don't execute original method.
            return false;
        }

        /// <summary>
        /// Pre-emptive Harmony prefix for ElectricityManager.GetRootGroup due to the complexity of transpiling this one.
        /// </summary>
        /// <param name="root">Root group..</param>
        /// <param name="merged">Merged group ID.</param>
        /// <param name="___m_pulseGroupCount">ElectricityManager private field - m_pulseGroupCount.</param>
        /// <returns>Always false (never execute original method).</returns>
        [HarmonyPatch("MergeGroups")]
        [HarmonyPrefix]
        private static bool MergeGroupsPrefix(ushort root, ushort merged, int ___m_pulseGroupCount)
        {
            ExpandedPulseGroup pulseGroup = s_pulseGroups[root];
            ExpandedPulseGroup pulseGroup2 = s_pulseGroups[merged];
            pulseGroup.m_origCharge += pulseGroup2.m_origCharge;
            if (pulseGroup2.m_mergeCount != 0)
            {
                for (int i = 0; i < ___m_pulseGroupCount; i++)
                {
                    if (s_pulseGroups[i].m_mergeIndex == merged)
                    {
                        s_pulseGroups[i].m_mergeIndex = root;
                        pulseGroup2.m_origCharge -= s_pulseGroups[i].m_origCharge;
                    }
                }

                pulseGroup.m_mergeCount += pulseGroup2.m_mergeCount;
                pulseGroup2.m_mergeCount = 0;
            }

            pulseGroup.m_curCharge += pulseGroup2.m_curCharge;
            pulseGroup2.m_curCharge = 0u;
            pulseGroup.m_mergeCount++;
            pulseGroup2.m_mergeIndex = root;
            s_pulseGroups[root] = pulseGroup;
            s_pulseGroups[merged] = pulseGroup2;

            // Don't execute original method.
            return false;
        }

        /// <summary>
        /// Pre-emptive Harmony prefix for ElectricityManager.SimulationStepImpl due to the complexity of transpiling this one.
        /// </summary>
        /// <param name="__instance">ElectricityManager instance.</param>
        /// <param name="subStep">Simulation sub-step.</param>
        /// <param name="___m_electricityGrid">ElectricityManager private field - m_electricityGrid (electricity cell array).</param>
        /// <param name="___m_pulseGroupCount">ElectricityManager private field - m_pulseGroupCount.</param>
        /// <param name="___m_pulseUnitStart">ElectricityManager private field - m_pulseUnitStart.</param>
        /// <param name="___m_pulseUnitEnd">ElectricityManager private field - m_pulseUnitEnd.</param>
        /// <param name="___m_processedCells">ElectricityManager private field - m_processedCells (number of cells processed).</param>
        /// <param name="___m_conductiveCells">ElectricityManager private field - m_conductiveCells (number of conductive cells).</param>
        /// <param name="___m_canContinue">ElectricityManager private field - m_canContinue.</param>
        /// <returns>Always false (never execute original method).</returns>
        [HarmonyPatch("SimulationStepImpl")]
        [HarmonyPrefix]
        private static bool SimulationStepImplPrefix(
            ElectricityManager __instance,
            int subStep,
            Cell[] ___m_electricityGrid,
            ref int ___m_pulseGroupCount,
            ref int ___m_pulseUnitStart,
            ref int ___m_pulseUnitEnd,
            ref int ___m_processedCells,
            ref int ___m_conductiveCells,
            ref bool ___m_canContinue)
        {
            if (subStep == 0 || subStep == 1000)
            {
                // Don't execute original method.
                return false;
            }

            uint currentFrameIndex = Singleton<SimulationManager>.instance.m_currentFrameIndex;
            int num = (int)(currentFrameIndex & 0xFF);
            if (num < 128)
            {
                if (num == 0)
                {
                    ___m_pulseGroupCount = 0;
                    ___m_pulseUnitStart = 0;
                    ___m_pulseUnitEnd = 0;
                    ___m_processedCells = 0;
                    ___m_conductiveCells = 0;
                    ___m_canContinue = true;
                }

                NetManager netManager = Singleton<NetManager>.instance;
                int num2 = (num * 32768) >> 7;
                int num3 = (((num + 1) * 32768) >> 7) - 1;
                for (int i = num2; i <= num3; i++)
                {
                    if (netManager.m_nodes.m_buffer[i].m_flags != 0)
                    {
                        NetInfo info = netManager.m_nodes.m_buffer[i].Info;
                        if (info.m_class.m_service == ItemClass.Service.Electricity || info.m_netAI is ConcourseAI)
                        {
                            UpdateNodeElectricity(__instance, i, (__instance.m_nodeGroups[i] != ushort.MaxValue) ? 1 : 0);
                            ___m_conductiveCells++;
                        }
                    }

                    __instance.m_nodeGroups[i] = ushort.MaxValue;
                }

                int num4 = (num * 256) >> 7;
                int num5 = (((num + 1) * 256) >> 7) - 1;
                ExpandedPulseGroup pulseGroup = default;
                ExpandedPulseUnit pulseUnit = default;
                for (int j = num4; j <= num5; j++)
                {
                    int num6 = j * ExpandedElectricityGridResolution;
                    for (int k = 0; k < ExpandedElectricityGridResolution; k++)
                    {
                        Cell cell = ___m_electricityGrid[num6];
                        if (cell.m_currentCharge > 0)
                        {
                            if (___m_pulseGroupCount < 1024)
                            {
                                pulseGroup.m_origCharge = (uint)cell.m_currentCharge;
                                pulseGroup.m_curCharge = (uint)cell.m_currentCharge;
                                pulseGroup.m_mergeCount = 0;
                                pulseGroup.m_mergeIndex = ushort.MaxValue;
                                pulseGroup.m_x = (byte)k;
                                pulseGroup.m_z = (byte)j;
                                pulseUnit.m_group = (ushort)___m_pulseGroupCount;
                                pulseUnit.m_node = 0;
                                pulseUnit.m_x = (byte)k;
                                pulseUnit.m_z = (byte)j;
                                cell.m_pulseGroup = (ushort)___m_pulseGroupCount;
                                s_pulseGroups[___m_pulseGroupCount++] = pulseGroup;
                                s_pulseUnits[___m_pulseUnitEnd] = pulseUnit;
                                if (++___m_pulseUnitEnd == s_pulseUnits.Length)
                                {
                                    ___m_pulseUnitEnd = 0;
                                }
                            }
                            else
                            {
                                cell.m_pulseGroup = ushort.MaxValue;
                            }

                            cell.m_currentCharge = 0;
                            ___m_conductiveCells++;
                        }
                        else
                        {
                            cell.m_pulseGroup = ushort.MaxValue;
                            if (cell.m_conductivity >= 64)
                            {
                                ___m_conductiveCells++;
                            }
                        }

                        if (cell.m_tmpElectrified != cell.m_electrified)
                        {
                            cell.m_electrified = cell.m_tmpElectrified;
                        }

                        cell.m_tmpElectrified = cell.m_pulseGroup != ushort.MaxValue;
                        ___m_electricityGrid[num6] = cell;
                        num6++;
                    }
                }

                // Don't execute original method.
                return false;
            }

            int num7 = ((num - 127) * ___m_conductiveCells) >> 7;
            if (num == 255)
            {
                num7 = 1000000000;
            }

            while (___m_canContinue && ___m_processedCells < num7)
            {
               ___m_canContinue = false;
               int pulseUnitEnd = ___m_pulseUnitEnd;
               while (___m_pulseUnitStart != pulseUnitEnd)
                {
                    ExpandedPulseUnit pulseUnit2 = s_pulseUnits[___m_pulseUnitStart];
                    if (++___m_pulseUnitStart == s_pulseUnits.Length)
                    {
                        ___m_pulseUnitStart = 0;
                    }

                    GetRootGroupPrefix(out pulseUnit2.m_group, pulseUnit2.m_group);
                    uint num8 = s_pulseGroups[pulseUnit2.m_group].m_curCharge;
                    if (pulseUnit2.m_node == 0)
                    {
                        int num9 = (pulseUnit2.m_z * ExpandedElectricityGridResolution) + pulseUnit2.m_x;
                        Cell cell2 = ___m_electricityGrid[num9];
                        if (cell2.m_conductivity != 0 && !cell2.m_tmpElectrified && num8 != 0)
                        {
                            int num10 = Mathf.Clamp(-cell2.m_currentCharge, 0, (int)num8);
                            num8 -= (uint)num10;
                            cell2.m_currentCharge += (short)num10;
                            if (cell2.m_currentCharge == 0)
                            {
                                cell2.m_tmpElectrified = true;
                            }

                            ___m_electricityGrid[num9] = cell2;
                            s_pulseGroups[pulseUnit2.m_group].m_curCharge = num8;
                        }

                        if (num8 != 0)
                        {
                            int limit = (cell2.m_conductivity >= 64) ? 1 : 64;
                            ___m_processedCells++;
                            if (pulseUnit2.m_z > 0)
                            {
                                ConductToCellPrefix(ref ___m_electricityGrid[num9 - ExpandedElectricityGridResolution], pulseUnit2.m_group, pulseUnit2.m_x, pulseUnit2.m_z - 1, limit, ___m_electricityGrid, ___m_pulseGroupCount, ref ___m_pulseUnitEnd, ref ___m_canContinue);
                            }

                            if (pulseUnit2.m_x > 0)
                            {
                                ConductToCellPrefix(ref ___m_electricityGrid[num9 - 1], pulseUnit2.m_group, pulseUnit2.m_x - 1, pulseUnit2.m_z, limit, ___m_electricityGrid, ___m_pulseGroupCount, ref ___m_pulseUnitEnd, ref ___m_canContinue);
                            }

                            if (pulseUnit2.m_z < ExpandedElectricityGridResolution)
                            {
                                ConductToCellPrefix(ref ___m_electricityGrid[num9 + ExpandedElectricityGridResolution], pulseUnit2.m_group, pulseUnit2.m_x, pulseUnit2.m_z + 1, limit, ___m_electricityGrid, ___m_pulseGroupCount, ref ___m_pulseUnitEnd, ref ___m_canContinue);
                            }

                            if (pulseUnit2.m_x < ExpandedElectricityGridResolution)
                            {
                                ConductToCellPrefix(ref ___m_electricityGrid[num9 + 1], pulseUnit2.m_group, pulseUnit2.m_x + 1, pulseUnit2.m_z, limit, ___m_electricityGrid, ___m_pulseGroupCount, ref ___m_pulseUnitEnd, ref ___m_canContinue);
                            }

                            ConductToNodesPrefix(__instance, pulseUnit2.m_group, pulseUnit2.m_x, pulseUnit2.m_z, ___m_pulseGroupCount, ref ___m_pulseUnitEnd, ref ___m_canContinue);
                        }
                        else
                        {
                            s_pulseUnits[___m_pulseUnitEnd] = pulseUnit2;
                            if (++___m_pulseUnitEnd == s_pulseUnits.Length)
                            {
                                ___m_pulseUnitEnd = 0;
                            }
                        }
                    }
                    else if (num8 != 0)
                    {
                        ___m_processedCells++;
                        NetNode netNode = Singleton<NetManager>.instance.m_nodes.m_buffer[pulseUnit2.m_node];
                        if (netNode.m_flags == NetNode.Flags.None || netNode.m_buildIndex >= (currentFrameIndex & 0xFFFFFF80u))
                        {
                            continue;
                        }

                        ConductToCellsPrefix(pulseUnit2.m_group, netNode.m_position.x, netNode.m_position.z, ___m_electricityGrid, ___m_pulseGroupCount, ref ___m_pulseUnitEnd, ref ___m_canContinue);
                        for (int l = 0; l < 8; l++)
                        {
                            ushort segment = netNode.GetSegment(l);
                            if (segment != 0)
                            {
                                ushort startNode = Singleton<NetManager>.instance.m_segments.m_buffer[segment].m_startNode;
                                ushort endNode = Singleton<NetManager>.instance.m_segments.m_buffer[segment].m_endNode;
                                ushort num11 = (startNode != pulseUnit2.m_node) ? startNode : endNode;
                                ConductToNodePrefix(__instance, num11, ref Singleton<NetManager>.instance.m_nodes.m_buffer[num11], pulseUnit2.m_group, -100000f, -100000f, 100000f, 100000f, ___m_pulseGroupCount, ref ___m_pulseUnitEnd, ref ___m_canContinue);
                            }
                        }
                    }
                    else
                    {
                        s_pulseUnits[___m_pulseUnitEnd] = pulseUnit2;
                        if (++___m_pulseUnitEnd == s_pulseUnits.Length)
                        {
                            ___m_pulseUnitEnd = 0;
                        }
                    }
                }
            }

            if (num != 255)
            {
                // Don't execute original method.
                return false;
            }

            for (int m = 0; m < ___m_pulseGroupCount; m++)
            {
                ExpandedPulseGroup pulseGroup2 = s_pulseGroups[m];
                if (pulseGroup2.m_mergeIndex != ushort.MaxValue)
                {
                    ExpandedPulseGroup pulseGroup3 = s_pulseGroups[pulseGroup2.m_mergeIndex];
                    pulseGroup2.m_curCharge = (uint)((ulong)((long)pulseGroup3.m_curCharge * (long)pulseGroup2.m_origCharge) / (ulong)pulseGroup3.m_origCharge);
                    pulseGroup3.m_curCharge -= pulseGroup2.m_curCharge;
                    pulseGroup3.m_origCharge -= pulseGroup2.m_origCharge;
                    s_pulseGroups[pulseGroup2.m_mergeIndex] = pulseGroup3;
                    s_pulseGroups[m] = pulseGroup2;
                }
            }

            for (int n = 0; n < ___m_pulseGroupCount; n++)
            {
                ExpandedPulseGroup pulseGroup4 = s_pulseGroups[n];
                if (pulseGroup4.m_curCharge != 0)
                {
                    int num12 = (pulseGroup4.m_z * ExpandedElectricityGridResolution) + pulseGroup4.m_x;
                    Cell cell3 = ___m_electricityGrid[num12];
                    if (cell3.m_conductivity != 0)
                    {
                        cell3.m_extraCharge += (ushort)Mathf.Min((int)pulseGroup4.m_curCharge, 32767 - cell3.m_extraCharge);
                    }

                    ___m_electricityGrid[num12] = cell3;
                }
            }

            // Don't execute original method.
            return false;
        }

        /// <summary>
        /// Harmony transpiler for ElectricityManager.TryDumpElectricity to replace hardcoded game constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(ElectricityManager.TryDumpElectricity), new Type[] { typeof(Vector3), typeof(int), typeof(int) }, new ArgumentType[] { ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal })]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> TryDumpElectricity1Transpiler(IEnumerable<CodeInstruction> instructions) => ReplaceElectricityConstants(instructions);

        /// <summary>
        /// Harmony transpiler for ElectricityManager.TryDumpElectricity to replace hardcoded game constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch("TryDumpElectricity", new Type[] { typeof(int), typeof(int), typeof(int), typeof(int) }, new ArgumentType[] { ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal })]
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
        private static IEnumerable<CodeInstruction> UpdateElectricityMappingTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            // Replace inverse constants.
            const float gameZ = 1f / (ELECTRICITYGRID_CELL_SIZE * GameElectricyGridResolution);
            const float expandedZ = 1f / (ELECTRICITYGRID_CELL_SIZE * ExpandedElectricityGridResolution);
            const float gameW = 1f / GameElectricyGridResolution;
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

        /*
        /// <summary>
        /// Reverse patch for ElectricityManager.ConductToCell to access private method of original instance.
        /// </summary>
        /// <param name="instance">ElectricityManager instance.</param>
        /// <param name="cell">Target electricity cell.</param>
        /// <param name="group">Group ID.</param>
        /// <param name="x">Cell x-coordinate.</param>
        /// <param name="z">Cell z-coordinate.</param>
        /// <param name="limit">Minimum required condutivity.</param>
        /// <exception cref="NotImplementedException">Reverse patch wasn't applied.</exception>
        [HarmonyReversePatch(HarmonyReversePatchType.Snapshot)]
        [HarmonyPatch("ConductToCell")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ConductToCell(ElectricityManager instance, ref Cell cell, ushort group, int x, int z, int limit)
        {
            string message = "ConductToCell reverse Harmony patch wasn't applied";
            Logging.Error(message, instance, cell, group, x, z, limit);
            throw new NotImplementedException(message);
        }

        /// <summary>
        /// Reverse patch for ElectricityManager.ConductToCells to access private method of original instance.
        /// </summary>
        /// <param name="instance">ElectricityManager instance.</param>
        /// <param name="group">Group ID.</param>
        /// <param name="worldX">World x-coordinate.</param>
        /// <param name="worldZ">World z-coordinate.</param>
        /// <exception cref="NotImplementedException">Reverse patch wasn't applied.</exception>
        [HarmonyReversePatch(HarmonyReversePatchType.Snapshot)]
        [HarmonyPatch("ConductToCells")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ConductToCells(ElectricityManager instance, ushort group, float worldX, float worldZ)
        {
            string message = "ConductToCells reverse Harmony patch wasn't applied";
            Logging.Error(message, instance, group, worldX, worldZ);
            throw new NotImplementedException(message);
        }

        /// <summary>
        /// Reverse patch for ElectricityManager.ConductToNode to access private method of original instance.
        /// </summary>
        /// <param name="instance">ElectricityManager instance.</param>
        /// <param name="nodeIndex">Node index ID.</param>
        /// <param name="node">Node data.</param>
        /// <param name="group">Group ID.</param>
        /// <param name="minX">Minimum node x-position.</param>
        /// <param name="minZ">Minimum node z-position.</param>
        /// <param name="maxX">Maximum node x-position.</param>
        /// <param name="maxZ">Maximum node z-position.</param>
        /// <exception cref="NotImplementedException">Reverse patch wasn't applied.</exception>
        [HarmonyReversePatch(HarmonyReversePatchType.Snapshot)]
        [HarmonyPatch("ConductToNode")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ConductToNode(ElectricityManager instance, ushort nodeIndex, ref NetNode node, ushort group, float minX, float minZ, float maxX, float maxZ)
        {
            string message = "ConductToNode reverse Harmony patch wasn't applied";
            Logging.Error(message, instance, nodeIndex, node, group, minX, minZ, maxX, maxZ);
            throw new NotImplementedException(message);
        }

        /// <summary>
        /// Reverse patch for ElectricityManager.ConductToNodes to access private method of original instance.
        /// </summary>
        /// <param name="instance">ElectricityManager instance.</param>
        /// <param name="group">Group ID.</param>
        /// <param name="x">Cell x-coordinate.</param>
        /// <param name="z">Cell z-coordinate.</param>
        /// <exception cref="NotImplementedException">Reverse patch wasn't applied.</exception>
        [HarmonyReversePatch(HarmonyReversePatchType.Snapshot)]
        [HarmonyPatch("ConductToNodes")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void ConductToNodes(ElectricityManager instance, ushort group, int x, int z)
        {
            string message = "ConductToNodes reverse Harmony patch wasn't applied";
            Logging.Error(message, instance, group, x, z);
            throw new NotImplementedException(message);
        }

        /// <summary>
        /// Reverse patch for ElectricityManager.GetRootGroup to access private method of original instance.
        /// </summary>
        /// <param name="instance">ElectricityManager instance.</param>
        /// <param name="group">Group ID.</param>
        /// <returns>Root group ID.</returns>
        /// <exception cref="NotImplementedException">Reverse patch wasn't applied.</exception>
        [HarmonyReversePatch(HarmonyReversePatchType.Snapshot)]
        [HarmonyPatch("GetRootGroup")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static ushort GetRootGroup(ElectricityManager instance, ushort group)
        {
            string message = "GetRootGroup reverse Harmony patch wasn't applied";
            Logging.Error(message, instance, group);
            throw new NotImplementedException(message);
        }*/

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
        private static void UpdateNodeElectricity(ElectricityManager instance, int nodeID, int value)
        {
            string message = "UpdateNodeElectricity reverse Harmony patch wasn't applied";
            Logging.Error(message, instance, nodeID, value);
            throw new NotImplementedException(message);
        }

        /// <summary>
        /// Expanded game PulseGroup struct to handle coordinate ranges outside byte limits.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1307:Accessible fields should begin with upper-case letter", Justification = "Uses game names")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:Elements should be documented", Justification = "Game struct")]
        internal struct ExpandedPulseGroup
        {
            public uint m_origCharge;
            public uint m_curCharge;
            public ushort m_mergeIndex;
            public ushort m_mergeCount;
            public ushort m_x;
            public ushort m_z;
        }

        /// <summary>
        /// Expanded game PulseUnit struct to handle coordinate ranges outside byte limits.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1307:Accessible fields should begin with upper-case letter", Justification = "Uses game names")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:Elements should be documented", Justification = "Game struct")]
        internal struct ExpandedPulseUnit
        {
            public ushort m_group;
            public ushort m_node;
            public ushort m_x;
            public ushort m_z;
        }
    }
}
