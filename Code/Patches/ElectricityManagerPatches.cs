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
        internal const int GameElectricyGridArraySize = ELECTRICITYGRID_RESOLUTION * ELECTRICITYGRID_RESOLUTION;

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
        internal const float GameElectricyGridHalfResolution = GameElectricityGridResolution / 2;

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
                else if (instruction.LoadsConstant(GameElectricyGridHalfResolution))
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

        /*
        /// <summary>
        /// Pre-emptive Harmony prefix for ElectricityManager.ConductToCell using expanded structs.
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
                pulseUnit.m_x = (ushort)x;
                pulseUnit.m_z = (ushort)z;
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
        */

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

        /*
        /// <summary>
        /// Pre-emptive Harmony prefix for ElectricityManager.ConductToCells for use in patched execution chains.
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
        */

        /*
        /// <summary>
        /// Pre-emptive Harmony prefix for ElectricityManager.ConductToNode using expanded structs.
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
        */

        /*
        /// <summary>
        /// Pre-emptive Harmony prefix for ElectricityManager.ConductToNodes for use in patched execution chains.
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
            float num = (cellX - ExpandedElectricityGridHalfResolution) * ELECTRICITYGRID_CELL_SIZE;
            float num2 = (cellZ - ExpandedElectricityGridHalfResolution) * ELECTRICITYGRID_CELL_SIZE;
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
        */

        /*
        /// <summary>
        /// Harmony transpiler for ElectricityManager.ConductToNodes to replace hardcoded game constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch("ConductToNodes")]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> ConductToNodesTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceElectricityConstants(instructions);
        */

        /*
        /// <summary>
        /// Pre-emptive Harmony prefix for ElectricityManager.GetRootGroup for use in patched execution chains.
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
        /// Pre-emptive Harmony prefix for ElectricityManager.GetRootGroup using expanded structs.
        /// </summary>
        /// <param name="root">Root group..</param>
        /// <param name="merged">Merged group ID.</param>
        /// <param name="___m_pulseGroupCount">ElectricityManager private field - m_pulseGroupCount.</param>
        /// <returns>Always false (never execute original method).</returns>
        // [HarmonyPatch("MergeGroups")]
        // [HarmonyPrefix]
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
        }*/

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

        /*
        /// <summary>
        /// Pre-emptive Harmony prefix for ElectricityManager.SimulationStepImpl using expanded structs.
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

                // Net nodes.
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

                int num4 = (num * ExpandedElectricityGridResolution) >> 7;
                int num5 = (((num + 1) * ExpandedElectricityGridResolution) >> 7) - 1;
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
                                pulseGroup.m_x = (ushort)k;
                                pulseGroup.m_z = (ushort)j;
                                pulseUnit.m_group = (ushort)___m_pulseGroupCount;
                                pulseUnit.m_node = 0;
                                pulseUnit.m_x = (ushort)k;
                                pulseUnit.m_z = (ushort)j;
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

                            if (pulseUnit2.m_z < ExpandedElectricityGridMax)
                            {
                                ConductToCellPrefix(ref ___m_electricityGrid[num9 + ExpandedElectricityGridResolution], pulseUnit2.m_group, pulseUnit2.m_x, pulseUnit2.m_z + 1, limit, ___m_electricityGrid, ___m_pulseGroupCount, ref ___m_pulseUnitEnd, ref ___m_canContinue);
                            }

                            if (pulseUnit2.m_x < ExpandedElectricityGridMax)
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
        */

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

        /*
        /// <summary>
        /// Pre-emptive Harmony prefix for ElectricityManager.UpdateGrid due to the complexity of transpiling this one.
        /// </summary>
        /// <param name="__instance">ElectricityManager instance.</param>
        /// <param name="minX">Minimum x-coordinate of updated area.</param>
        /// <param name="minZ">Minimum z-coordinate of updated area.</param>
        /// <param name="maxX">Maximum x-coordinate of updated area.</param>
        /// <param name="maxZ">Maximum z-coordinate of updated area.</param>
        /// <param name="___m_electricityGrid">ElectricityManager private array - m_electricityGrid.</param>
        /// <returns>Always false (never execute original method).</returns>
        [HarmonyPatch(nameof(ElectricityManager.UpdateGrid))]
        [HarmonyPrefix]
        private static bool UpdateGridPrefix(ElectricityManager __instance, float minX, float minZ, float maxX, float maxZ, Cell[] ___m_electricityGrid)
        {
            int num = Mathf.Max((int)((minX / ELECTRICITYGRID_CELL_SIZE) + ExpandedElectricityGridHalfResolution), 0);
            int num2 = Mathf.Max((int)((minZ / ELECTRICITYGRID_CELL_SIZE) + ExpandedElectricityGridHalfResolution), 0);
            int num3 = Mathf.Min((int)((maxX / ELECTRICITYGRID_CELL_SIZE) + ExpandedElectricityGridHalfResolution), ExpandedElectricityGridMax);
            int num4 = Mathf.Min((int)((maxZ / ELECTRICITYGRID_CELL_SIZE) + ExpandedElectricityGridHalfResolution), ExpandedElectricityGridMax);
            for (int i = num2; i <= num4; i++)
            {
                int num5 = (i * ExpandedElectricityGridResolution) + num;
                for (int j = num; j <= num3; j++)
                {
                    ___m_electricityGrid[num5++].m_conductivity = 0;
                }
            }

            int num6 = Mathf.Max((int)((((((float)num - ExpandedElectricityGridHalfResolution) * ELECTRICITYGRID_CELL_SIZE) - 96f) / 64f) + 135f), 0);
            int num7 = Mathf.Max((int)((((((float)num2 - ExpandedElectricityGridHalfResolution) * ELECTRICITYGRID_CELL_SIZE) - 96f) / 64f) + 135f), 0);
            int num8 = Mathf.Min((int)((((((float)num3 - ExpandedElectricityGridHalfResolution + 1f) * ELECTRICITYGRID_CELL_SIZE) + 96f) / 64f) + 135f), 269);
            int num9 = Mathf.Min((int)((((((float)num4 - ExpandedElectricityGridHalfResolution + 1f) * ELECTRICITYGRID_CELL_SIZE) + 96f) / 64f) + 135f), 269);
            Array16<Building> buildings = Singleton<BuildingManager>.instance.m_buildings;
            ushort[] buildingGrid = Singleton<BuildingManager>.instance.m_buildingGrid;
            Vector3 vector7 = default;
            for (int k = num7; k <= num9; k++)
            {
                for (int l = num6; l <= num8; l++)
                {
                    ushort num10 = buildingGrid[(k * 270) + l];
                    int num11 = 0;
                    while (num10 != 0)
                    {
                        Building.Flags flags = buildings.m_buffer[num10].m_flags;
                        if ((flags & (Building.Flags.Created | Building.Flags.Deleted)) == Building.Flags.Created)
                        {
                            buildings.m_buffer[num10].GetInfoWidthLength(out var info, out var width, out var length);
                            if (info != null)
                            {
                                float num12 = info.m_buildingAI.ElectricityGridRadius();
                                if (num12 > 0.1f)
                                {
                                    Vector3 position = buildings.m_buffer[num10].m_position;
                                    float angle = buildings.m_buffer[num10].m_angle;
                                    Vector3 vector = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
                                    Vector3 vector2 = new Vector3(vector.z, 0f, 0f - vector.x);
                                    Vector3 vector3 = position - (width * 4 * vector) - (length * 4 * vector2);
                                    Vector3 vector4 = position + (width * 4 * vector) - (length * 4 * vector2);
                                    Vector3 vector5 = position + (width * 4 * vector) + (length * 4 * vector2);
                                    Vector3 vector6 = position - (width * 4 * vector) + (length * 4 * vector2);
                                    minX = Mathf.Min(Mathf.Min(vector3.x, vector4.x), Mathf.Min(vector5.x, vector6.x)) - num12;
                                    maxX = Mathf.Max(Mathf.Max(vector3.x, vector4.x), Mathf.Max(vector5.x, vector6.x)) + num12;
                                    minZ = Mathf.Min(Mathf.Min(vector3.z, vector4.z), Mathf.Min(vector5.z, vector6.z)) - num12;
                                    maxZ = Mathf.Max(Mathf.Max(vector3.z, vector4.z), Mathf.Max(vector5.z, vector6.z)) + num12;
                                    int num13 = Mathf.Max(num, (int)((minX / ELECTRICITYGRID_CELL_SIZE) + ExpandedElectricityGridHalfResolution));
                                    int num14 = Mathf.Min(num3, (int)((maxX / ELECTRICITYGRID_CELL_SIZE) + ExpandedElectricityGridHalfResolution));
                                    int num15 = Mathf.Max(num2, (int)((minZ / ELECTRICITYGRID_CELL_SIZE) + ExpandedElectricityGridHalfResolution));
                                    int num16 = Mathf.Min(num4, (int)((maxZ / ELECTRICITYGRID_CELL_SIZE) + ExpandedElectricityGridHalfResolution));
                                    for (int m = num15; m <= num16; m++)
                                    {
                                        for (int n = num13; n <= num14; n++)
                                        {
                                            vector7.x = ((float)n + 0.5f - ExpandedElectricityGridHalfResolution) * ELECTRICITYGRID_CELL_SIZE;
                                            vector7.y = position.y;
                                            vector7.z = ((float)m + 0.5f - ExpandedElectricityGridHalfResolution) * ELECTRICITYGRID_CELL_SIZE;
                                            float num17 = Mathf.Max(0f, Mathf.Abs(Vector3.Dot(vector, vector7 - position)) - (float)(width * 4));
                                            float num18 = Mathf.Max(0f, Mathf.Abs(Vector3.Dot(vector2, vector7 - position)) - (float)(length * 4));
                                            float num19 = Mathf.Sqrt((num17 * num17) + (num18 * num18));
                                            if (num19 < num12 + 19.125f)
                                            {
                                                float num20 = ((num12 - num19) * (2f / 153f)) + 0.25f;
                                                int num21 = Mathf.Min(255, Mathf.RoundToInt(num20 * 255f));
                                                int num22 = (m * ExpandedElectricityGridResolution) + n;
                                                if (num21 > ___m_electricityGrid[num22].m_conductivity)
                                                {
                                                    ___m_electricityGrid[num22].m_conductivity = (byte)num21;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        num10 = buildings.m_buffer[num10].m_nextGridBuilding;
                        if (++num11 >= 49152)
                        {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                            break;
                        }
                    }
                }
            }

            for (int num23 = num2; num23 <= num4; num23++)
            {
                int num24 = (num23 * ExpandedElectricityGridResolution) + num;
                for (int num25 = num; num25 <= num3; num25++)
                {
                    Cell cell = ___m_electricityGrid[num24];
                    if (cell.m_conductivity == 0)
                    {
                        cell.m_currentCharge = 0;
                        cell.m_extraCharge = 0;
                        cell.m_pulseGroup = ushort.MaxValue;
                        cell.m_tmpElectrified = false;
                        cell.m_electrified = false;
                        ___m_electricityGrid[num24] = cell;
                    }

                    num24++;
                }
            }

            __instance.AreaModified(num, num2, num3, num4);
            return false;
        }
        */

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
    }
}
