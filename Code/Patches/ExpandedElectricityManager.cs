// <copyright file="ExpandedElectricityManager.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace EightyOne2.Patches
{
    using System;
    using ColossalFramework;
    using UnityEngine;
    using static ElectricityManager;
    using static ElectricityManagerPatches;

    /// <summary>
    /// Custom electricty manager components for 81-tile operation.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.NamingRules", "SA1311:Static readonly fields should begin with upper-case letter", Justification = "Dotnet runtime style")]
    internal static class ExpandedElectricityManager
    {
        // Expanded arrays.
        private static readonly ExpandedPulseGroup[] s_pulseGroups = new ExpandedPulseGroup[MAX_PULSE_GROUPS];
        private static readonly ExpandedPulseUnit[] s_pulseUnits = new ExpandedPulseUnit[32786];

        // Electric roads status.
        private static bool s_electricRoads = false;

        /// <summary>
        /// Gets or sets a value indicating whether the 'no powerlines' functionality is enabled.
        /// </summary>
        internal static bool ElectricRoadsEnabled { get => s_electricRoads; set => s_electricRoads = value; }

        /// <summary>
        /// Gets the expanded pulse group array.
        /// </summary>
        internal static ExpandedPulseGroup[] PulseGroups => s_pulseGroups;

        /// <summary>
        /// Gets the expanded pulse unit array.
        /// </summary>
        internal static ExpandedPulseUnit[] PulseUnits => s_pulseUnits;

        /// <summary>
        /// Substitute for ElectricityManager.SimulationStepImpl using expanded structs.
        /// </summary>
        /// <param name="instance">ElectricityManager instance.</param>
        /// <param name="subStep">Simulation sub-step.</param>
        /// <param name="electricityGrid">ElectricityManager private field - m_electricityGrid (electricity cell array).</param>
        /// <param name="pulseGroupCount">ElectricityManager private field - m_pulseGroupCount.</param>
        /// <param name="pulseUnitStart">ElectricityManager private field - m_pulseUnitStart.</param>
        /// <param name="pulseUnitEnd">ElectricityManager private field - m_pulseUnitEnd.</param>
        /// <param name="processedCells">ElectricityManager private field - m_processedCells (number of cells processed).</param>
        /// <param name="conductiveCells">ElectricityManager private field - m_conductiveCells (number of conductive cells).</param>
        /// <param name="canContinue">ElectricityManager private field - m_canContinue.</param>
        internal static void SimulationStepImpl(
            ElectricityManager instance,
            int subStep,
            Cell[] electricityGrid,
            ref int pulseGroupCount,
            ref int pulseUnitStart,
            ref int pulseUnitEnd,
            ref int processedCells,
            ref int conductiveCells,
            ref bool canContinue)
        {
            if (subStep == 0 || subStep == 1000)
            {
                return;
            }

            ExpandedPulseGroup[] pulseGroups = PulseGroups;
            ExpandedPulseUnit[] pulseUnits = PulseUnits;
            ushort[] nodeGroups = instance.m_nodeGroups;

            uint currentFrameIndex = Singleton<SimulationManager>.instance.m_currentFrameIndex;
            int num = (int)(currentFrameIndex & 0xFF);
            if (num < 128)
            {
                if (num == 0)
                {
                    pulseGroupCount = 0;
                    pulseUnitStart = 0;
                    pulseUnitEnd = 0;
                    processedCells = 0;
                    conductiveCells = 0;
                    canContinue = true;
                }

                // Net nodes.
                NetManager netManager = Singleton<NetManager>.instance;
                NetNode[] netNodes = netManager.m_nodes.m_buffer;
                int num2 = (num * 32768) >> 7;
                int num3 = (((num + 1) * 32768) >> 7) - 1;
                for (int i = num2; i <= num3; i++)
                {
                    if (netNodes[i].m_flags != 0)
                    {
                        // Electric roads modification.
                        NetInfo netInfo = netNodes[i].Info;
                        if (netInfo.m_class.m_service == ItemClass.Service.Electricity || netInfo.m_netAI is ConcourseAI || CheckElectricRoad(netInfo))
                        {
                            UpdateNodeElectricity(i, (nodeGroups[i] == ushort.MaxValue) ? 0 : 1, netManager, netNodes, netInfo);
                            conductiveCells++;
                        }
                    }

                    nodeGroups[i] = ushort.MaxValue;
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
                        Cell cell = electricityGrid[num6];
                        if (cell.m_currentCharge > 0)
                        {
                            if (pulseGroupCount < 1024)
                            {
                                pulseGroup.m_origCharge = (uint)cell.m_currentCharge;
                                pulseGroup.m_curCharge = (uint)cell.m_currentCharge;
                                pulseGroup.m_mergeCount = 0;
                                pulseGroup.m_mergeIndex = ushort.MaxValue;
                                pulseGroup.m_x = (ushort)k;
                                pulseGroup.m_z = (ushort)j;
                                pulseUnit.m_group = (ushort)pulseGroupCount;
                                pulseUnit.m_node = 0;
                                pulseUnit.m_x = (ushort)k;
                                pulseUnit.m_z = (ushort)j;
                                cell.m_pulseGroup = (ushort)pulseGroupCount;
                                pulseGroups[pulseGroupCount++] = pulseGroup;
                                pulseUnits[pulseUnitEnd] = pulseUnit;
                                if (++pulseUnitEnd == pulseUnits.Length)
                                {
                                    pulseUnitEnd = 0;
                                }
                            }
                            else
                            {
                                cell.m_pulseGroup = ushort.MaxValue;
                            }

                            cell.m_currentCharge = 0;
                            conductiveCells++;
                        }
                        else
                        {
                            cell.m_pulseGroup = ushort.MaxValue;
                            if (cell.m_conductivity >= 64)
                            {
                                conductiveCells++;
                            }
                        }

                        if (cell.m_tmpElectrified != cell.m_electrified)
                        {
                            cell.m_electrified = cell.m_tmpElectrified;
                        }

                        cell.m_tmpElectrified = cell.m_pulseGroup != ushort.MaxValue;
                        electricityGrid[num6] = cell;
                        num6++;
                    }
                }

                return;
            }

            int num7 = ((num - 127) * conductiveCells) >> 7;
            if (num == 255)
            {
                num7 = 1000000000;
            }

            while (canContinue && processedCells < num7)
            {
                canContinue = false;
                int initialPulseUnitEnd = pulseUnitEnd;
                while (pulseUnitStart != initialPulseUnitEnd)
                {
                    ExpandedPulseUnit pulseUnit = pulseUnits[pulseUnitStart];
                    if (++pulseUnitStart == pulseUnits.Length)
                    {
                        pulseUnitStart = 0;
                    }

                    pulseUnit.m_group = GetRootGroup(pulseUnit.m_group);
                    uint num8 = pulseGroups[pulseUnit.m_group].m_curCharge;
                    if (pulseUnit.m_node == 0)
                    {
                        int num9 = (pulseUnit.m_z * ExpandedElectricityGridResolution) + pulseUnit.m_x;
                        Cell cell2 = electricityGrid[num9];
                        if (cell2.m_conductivity != 0 && !cell2.m_tmpElectrified && num8 != 0u)
                        {
                            int num10 = Mathf.Clamp(-cell2.m_currentCharge, 0, (int)num8);
                            num8 -= (uint)num10;
                            cell2.m_currentCharge += (short)num10;
                            if (cell2.m_currentCharge == 0)
                            {
                                cell2.m_tmpElectrified = true;
                            }

                            electricityGrid[num9] = cell2;
                            pulseGroups[pulseUnit.m_group].m_curCharge = num8;
                        }

                        if (num8 != 0u)
                        {
                            int limit = (cell2.m_conductivity < 64) ? 64 : 1;
                            processedCells++;
                            if (pulseUnit.m_z > 0)
                            {
                                ConductToCell(
                                    ref electricityGrid[num9 - ExpandedElectricityGridResolution],
                                    pulseUnit.m_group,
                                    pulseUnit.m_x,
                                    pulseUnit.m_z - 1,
                                    limit,
                                    electricityGrid,
                                    pulseGroupCount,
                                    ref pulseUnitEnd,
                                    ref canContinue);
                            }

                            if (pulseUnit.m_x > 0)
                            {
                                ConductToCell(
                                    ref electricityGrid[num9 - 1],
                                    pulseUnit.m_group,
                                    pulseUnit.m_x - 1,
                                    pulseUnit.m_z,
                                    limit,
                                    electricityGrid,
                                    pulseGroupCount,
                                    ref pulseUnitEnd,
                                    ref canContinue);
                            }

                            if (pulseUnit.m_z < ExpandedElectricityGridMax)
                            {
                                ConductToCell(
                                    ref electricityGrid[num9 + ExpandedElectricityGridResolution],
                                    pulseUnit.m_group,
                                    pulseUnit.m_x,
                                    pulseUnit.m_z + 1,
                                    limit,
                                    electricityGrid,
                                    pulseGroupCount,
                                    ref pulseUnitEnd,
                                    ref canContinue);
                            }

                            if (pulseUnit.m_x < ExpandedElectricityGridMax)
                            {
                                ConductToCell(
                                    ref electricityGrid[num9 + 1],
                                    pulseUnit.m_group,
                                    pulseUnit.m_x + 1,
                                    pulseUnit.m_z,
                                    limit,
                                    electricityGrid,
                                    pulseGroupCount,
                                    ref pulseUnitEnd,
                                    ref canContinue);
                            }

                            ConductToNodes(pulseUnit.m_group, pulseUnit.m_x, pulseUnit.m_z, nodeGroups, pulseGroupCount, ref pulseUnitEnd, ref canContinue);
                        }
                        else
                        {
                            pulseUnits[pulseUnitEnd] = pulseUnit;
                            if (++pulseUnitEnd == pulseUnits.Length)
                            {
                                pulseUnitEnd = 0;
                            }
                        }
                    }
                    else if (num8 != 0u)
                    {
                        processedCells++;

                        NetManager netManager = Singleton<NetManager>.instance;
                        NetSegment[] netSegments = netManager.m_segments.m_buffer;
                        NetNode[] netNodes = netManager.m_nodes.m_buffer;

                        ref NetNode netNode = ref netNodes[pulseUnit.m_node];
                        if (netNode.m_flags == NetNode.Flags.None || netNode.m_buildIndex >= (currentFrameIndex & 0xFFFFFF80u))
                        {
                            continue;
                        }

                        ConductToCells(
                            pulseUnit.m_group,
                            netNode.m_position.x,
                            netNode.m_position.z,
                            electricityGrid,
                            pulseGroupCount,
                            ref pulseUnitEnd,
                            ref canContinue);
                        for (int l = 0; l < 8; l++)
                        {
                            ushort segment = netNode.GetSegment(l);
                            if (segment != 0)
                            {
                                ushort startNode = netSegments[segment].m_startNode;
                                ushort endNode = netSegments[segment].m_endNode;
                                ushort nodeIndex = (startNode != pulseUnit.m_node) ? startNode : endNode;
                                ConductToNode(
                                    nodeIndex,
                                    ref netNodes[nodeIndex],
                                    pulseUnit.m_group,
                                    -100000f,
                                    -100000f,
                                    100000f,
                                    100000f,
                                    nodeGroups,
                                    pulseGroupCount,
                                    ref pulseUnitEnd,
                                    ref canContinue);
                            }
                        }
                    }
                    else
                    {
                        pulseUnits[pulseUnitEnd] = pulseUnit;
                        if (++pulseUnitEnd == pulseUnits.Length)
                        {
                            pulseUnitEnd = 0;
                        }
                    }
                }
            }

            if (num != 255)
            {
                return;
            }

            for (int m = 0; m < pulseGroupCount; m++)
            {
                ExpandedPulseGroup pulseGroup2 = pulseGroups[m];
                if (pulseGroup2.m_mergeIndex != ushort.MaxValue)
                {
                    ref ExpandedPulseGroup pulseGroup3 = ref pulseGroups[pulseGroup2.m_mergeIndex];
                    pulseGroup2.m_curCharge = (uint)(pulseGroup3.m_curCharge * (ulong)pulseGroup2.m_origCharge / pulseGroup3.m_origCharge);
                    pulseGroup3.m_curCharge -= pulseGroup2.m_curCharge;
                    pulseGroup3.m_origCharge -= pulseGroup2.m_origCharge;
                    pulseGroups[pulseGroup2.m_mergeIndex] = pulseGroup3;
                    pulseGroups[m] = pulseGroup2;
                }
            }

            for (int n = 0; n < pulseGroupCount; n++)
            {
                ref ExpandedPulseGroup pulseGroup4 = ref pulseGroups[n];
                if (pulseGroup4.m_curCharge != 0u)
                {
                    int num12 = (pulseGroup4.m_z * ExpandedElectricityGridResolution) + pulseGroup4.m_x;
                    Cell cell3 = electricityGrid[num12];
                    if (cell3.m_conductivity != 0)
                    {
                        cell3.m_extraCharge += (ushort)Mathf.Min((int)pulseGroup4.m_curCharge, 32767 - cell3.m_extraCharge);
                    }

                    electricityGrid[num12] = cell3;
                }
            }
        }

        /// <summary>
        /// Substitute for ElectricityManager.UpdateGrid to integrate with custom electricity manager.
        /// </summary>
        /// <param name="instance">ElectricityManager instance.</param>
        /// <param name="minX">Minimum x-coordinate of updated area.</param>
        /// <param name="minZ">Minimum z-coordinate of updated area.</param>
        /// <param name="maxX">Maximum x-coordinate of updated area.</param>
        /// <param name="maxZ">Maximum z-coordinate of updated area.</param>
        /// <param name="electricityGrid">ElectricityManager private array - m_electricityGrid.</param>
        internal static void UpdateGrid(ElectricityManager instance, float minX, float minZ, float maxX, float maxZ, Cell[] electricityGrid)
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
                    electricityGrid[num5++].m_conductivity = 0;
                }
            }

            // Buildings.
            int num6 = Mathf.Max((int)(((((num - ExpandedElectricityGridHalfResolution) * ELECTRICITYGRID_CELL_SIZE) - 96f) / 64f) + 135f), 0);
            int num7 = Mathf.Max((int)(((((num2 - ExpandedElectricityGridHalfResolution) * ELECTRICITYGRID_CELL_SIZE) - 96f) / 64f) + 135f), 0);
            int num8 = Mathf.Min((int)(((((num3 - ExpandedElectricityGridHalfResolution + 1f) * ELECTRICITYGRID_CELL_SIZE) + 96f) / 64f) + 135f), 269);
            int num9 = Mathf.Min((int)(((((num4 - ExpandedElectricityGridHalfResolution + 1f) * ELECTRICITYGRID_CELL_SIZE) + 96f) / 64f) + 135f), 269);

            // Local references.
            BuildingManager buildingManager = Singleton<BuildingManager>.instance;
            Building[] buildings = buildingManager.m_buildings.m_buffer;
            ushort[] buildingGrid = buildingManager.m_buildingGrid;

            Vector3 vector7 = default;
            for (int k = num7; k <= num9; k++)
            {
                for (int l = num6; l <= num8; l++)
                {
                    ushort num10 = buildingGrid[(k * 270) + l];
                    while (num10 != 0)
                    {
                        Building.Flags flags = buildings[num10].m_flags;
                        if ((flags & (Building.Flags.Created | Building.Flags.Deleted)) == Building.Flags.Created)
                        {
                            buildings[num10].GetInfoWidthLength(out BuildingInfo buildingInfo, out int width, out int length);
                            if (buildingInfo != null)
                            {
                                float num12 = buildingInfo.m_buildingAI.ElectricityGridRadius();
                                if (num12 > 0.1f)
                                {
                                    Vector3 position = buildings[num10].m_position;
                                    float angle = buildings[num10].m_angle;
                                    Vector3 vector = new Vector3((float)Math.Cos(angle), 0f, (float)Math.Sin(angle));
                                    Vector3 vector2 = new Vector3(vector.z, 0f, -vector.x);
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
                                            vector7.x = (n + 0.5f - ExpandedElectricityGridHalfResolution) * ELECTRICITYGRID_CELL_SIZE;
                                            vector7.y = position.y;
                                            vector7.z = (m + 0.5f - ExpandedElectricityGridHalfResolution) * ELECTRICITYGRID_CELL_SIZE;
                                            float num17 = Mathf.Max(0f, Mathf.Abs(Vector3.Dot(vector, vector7 - position)) - (width * 4));
                                            float num18 = Mathf.Max(0f, Mathf.Abs(Vector3.Dot(vector2, vector7 - position)) - (length * 4));
                                            float num19 = Mathf.Sqrt((num17 * num17) + (num18 * num18));
                                            if (num19 < num12 + 19.125f)
                                            {
                                                float num20 = ((num12 - num19) * (2f / 153f)) + 0.25f;
                                                int num21 = Mathf.Min(255, Mathf.RoundToInt(num20 * 255f));
                                                int num22 = (m * ExpandedElectricityGridResolution) + n;
                                                if (num21 > electricityGrid[num22].m_conductivity)
                                                {
                                                    electricityGrid[num22].m_conductivity = (byte)num21;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        num10 = buildings[num10].m_nextGridBuilding;
                    }
                }
            }

            for (int num23 = num2; num23 <= num4; num23++)
            {
                int num24 = (num23 * ExpandedElectricityGridResolution) + num;
                for (int num25 = num; num25 <= num3; num25++)
                {
                    Cell cell = electricityGrid[num24];
                    if (cell.m_conductivity == 0)
                    {
                        cell.m_currentCharge = 0;
                        cell.m_extraCharge = 0;
                        cell.m_pulseGroup = ushort.MaxValue;
                        cell.m_tmpElectrified = false;
                        cell.m_electrified = false;
                        electricityGrid[num24] = cell;
                    }

                    num24++;
                }
            }

            instance.AreaModified(num, num2, num3, num4);
        }

        /// <summary>
        /// Substitute for ElectricityManager.ConductToCell using expanded structs.
        /// </summary>
        /// <param name="cell">Target electricity cell.</param>
        /// <param name="group">Group ID.</param>
        /// <param name="x">Cell x-coordinate.</param>
        /// <param name="z">Cell z-coordinate.</param>
        /// <param name="limit">Minimum required condutivity.</param>
        /// <param name="electricityGrid">ElectricityManager private field - m_electricityGrid (electricity cell array).</param>
        /// <param name="pulseGroupCount">ElectricityManager private field - m_pulseGroupCount.</param>
        /// <param name="pulseUnitEnd">ElectricityManager private field - m_pulseUnitEnd.</param>
        /// <param name="canContinue">ElectricityManager private field - m_canContinue.</param>
        private static void ConductToCell(ref Cell cell, ushort group, int x, int z, int limit, Cell[] electricityGrid, int pulseGroupCount, ref int pulseUnitEnd, ref bool canContinue)
        {
            if (cell.m_conductivity < limit)
            {
                return;
            }

            if (cell.m_conductivity < 64)
            {
                bool flag = true;
                bool flag2 = true;
                int num = (z * ExpandedElectricityGridResolution) + x;
                if (x > 0 && electricityGrid[num - 1].m_conductivity >= 64)
                {
                    flag = false;
                }

                if (x < ExpandedElectricityGridMax && electricityGrid[num + 1].m_conductivity >= 64)
                {
                    flag = false;
                }

                if (z > 0 && electricityGrid[num - ExpandedElectricityGridResolution].m_conductivity >= 64)
                {
                    flag2 = false;
                }

                if (z < ExpandedElectricityGridMax && electricityGrid[num + ExpandedElectricityGridResolution].m_conductivity >= 64)
                {
                    flag2 = false;
                }

                if (flag || flag2)
                {
                    return;
                }
            }

            if (cell.m_pulseGroup == ushort.MaxValue)
            {
                ExpandedPulseUnit pulseUnit = default;
                pulseUnit.m_group = group;
                pulseUnit.m_node = 0;
                pulseUnit.m_x = (ushort)x;
                pulseUnit.m_z = (ushort)z;
                s_pulseUnits[pulseUnitEnd] = pulseUnit;
                if (++pulseUnitEnd == s_pulseUnits.Length)
                {
                    pulseUnitEnd = 0;
                }

                cell.m_pulseGroup = group;
                canContinue = true;
            }
            else
            {
                ushort rootGroup = GetRootGroup(cell.m_pulseGroup);
                if (rootGroup != group)
                {
                    MergeGroups(group, rootGroup, pulseGroupCount);
                    cell.m_pulseGroup = group;
                    canContinue = true;
                }
            }
        }

        /// <summary>
        /// Substitute for ElectricityManager.ConductToCells using expanded structs.
        /// </summary>>
        /// <param name="group">Group ID.</param>
        /// <param name="worldX">World x-coordinate.</param>
        /// <param name="worldZ">World z-coordinate.</param>
        /// <param name="electricityGrid">ElectricityManager private field - m_electricityGrid (electricity cell array).</param>
        /// <param name="pulseGroupCount">ElectricityManager private field - m_pulseGroupCount.</param>
        /// <param name="pulseUnitEnd">ElectricityManager private field - m_pulseUnitEnd.</param>
        /// <param name="canContinue">ElectricityManager private field - m_canContinue.</param>
        private static void ConductToCells(ushort group, float worldX, float worldZ, Cell[] electricityGrid, int pulseGroupCount, ref int pulseUnitEnd, ref bool canContinue)
        {
            int num = (int)((worldX / ELECTRICITYGRID_CELL_SIZE) + ExpandedElectricityGridHalfResolution);
            int num2 = (int)((worldZ / ELECTRICITYGRID_CELL_SIZE) + ExpandedElectricityGridHalfResolution);
            if (num >= 0 && num < ExpandedElectricityGridResolution && num2 >= 0 && num2 < ExpandedElectricityGridResolution)
            {
                int num3 = (num2 * ExpandedElectricityGridResolution) + num;
                ConductToCell(ref electricityGrid[num3], group, num, num2, 1, electricityGrid, pulseGroupCount, ref pulseUnitEnd, ref canContinue);
            }
        }

        /// <summary>
        /// Substitute for ElectricityManager.ConductToNode using expanded structs.
        /// </summary>
        /// <param name="nodeIndex">Node index ID.</param>
        /// <param name="node">Node data.</param>
        /// <param name="group">Group ID.</param>
        /// <param name="minX">Minimum node x-position.</param>
        /// <param name="minZ">Minimum node z-position.</param>
        /// <param name="maxX">Maximum node x-position.</param>
        /// <param name="maxZ">Maximum node z-position.</param>
        /// <param name="nodeGroups">ElectricityManager node groups array.</param>
        /// <param name="pulseGroupCount">ElectricityManager private field - m_pulseGroupCount.</param>
        /// <param name="pulseUnitEnd">ElectricityManager private field - m_pulseUnitEnd.</param>
        /// <param name="canContinue">ElectricityManager private field - m_canContinue.</param>
        private static void ConductToNode(ushort nodeIndex, ref NetNode node, ushort group, float minX, float minZ, float maxX, float maxZ, ushort[] nodeGroups, int pulseGroupCount, ref int pulseUnitEnd, ref bool canContinue)
        {
            if (!(node.m_position.x >= minX) || !(node.m_position.z >= minZ) || !(node.m_position.x <= maxX) || !(node.m_position.z <= maxZ))
            {
                return;
            }

            // Electric road modification.
            NetInfo netInfo = node.Info;
            if (netInfo.m_class.m_service != ItemClass.Service.Electricity && !(node.Info.m_netAI is ConcourseAI) && !CheckElectricRoad(netInfo))
            {
                return;
            }

            if (nodeGroups[nodeIndex] == ushort.MaxValue)
            {
                ExpandedPulseUnit pulseUnit = default;
                pulseUnit.m_group = group;
                pulseUnit.m_node = nodeIndex;
                pulseUnit.m_x = 0;
                pulseUnit.m_z = 0;
                s_pulseUnits[pulseUnitEnd] = pulseUnit;
                if (++pulseUnitEnd == s_pulseUnits.Length)
                {
                    pulseUnitEnd = 0;
                }

                nodeGroups[nodeIndex] = group;
                canContinue = true;
            }
            else
            {
                ushort rootGroup = GetRootGroup(nodeGroups[nodeIndex]);
                if (rootGroup != group)
                {
                    MergeGroups(group, rootGroup, pulseGroupCount);
                    nodeGroups[nodeIndex] = group;
                    canContinue = true;
                }
            }
        }

        /// <summary>
        /// Substitute for ElectricityManager.ConductToNodes to integrate with custom electricity manager.
        /// </summary>
        /// <param name="group">Group ID.</param>
        /// <param name="cellX">Cell x-coordinate.</param>
        /// <param name="cellZ">Cell z-coordinate.</param>
        /// <param name="nodeGroups">ElectricityManager node groups array.</param>
        /// <param name="pulseGroupCount">ElectricityManager private field - m_pulseGroupCount.</param>
        /// <param name="pulseUnitEnd">ElectricityManager private field - m_pulseUnitEnd.</param>
        /// <param name="canContinue">ElectricityManager private field - m_canContinue.</param>
        private static void ConductToNodes(ushort group, int cellX, int cellZ, ushort[] nodeGroups, int pulseGroupCount, ref int pulseUnitEnd, ref bool canContinue)
        {
            float num = (cellX - ExpandedElectricityGridHalfResolution) * ELECTRICITYGRID_CELL_SIZE;
            float num2 = (cellZ - ExpandedElectricityGridHalfResolution) * ELECTRICITYGRID_CELL_SIZE;
            float num3 = num + ELECTRICITYGRID_CELL_SIZE;
            float num4 = num2 + ELECTRICITYGRID_CELL_SIZE;
            int num5 = Mathf.Max((int)((num / 64f) + 135f), 0);
            int num6 = Mathf.Max((int)((num2 / 64f) + 135f), 0);
            int num7 = Mathf.Min((int)((num3 / 64f) + 135f), 269);
            int num8 = Mathf.Min((int)((num4 / 64f) + 135f), 269);

            // Local references.
            NetManager netManager = Singleton<NetManager>.instance;
            ushort[] netNodeGrid = netManager.m_nodeGrid;
            NetNode[] netNodes = netManager.m_nodes.m_buffer;
            for (int i = num6; i <= num8; i++)
            {
                for (int j = num5; j <= num7; j++)
                {
                    ushort num9 = netNodeGrid[(i * 270) + j];
                    while (num9 != 0)
                    {
                        ConductToNode(num9, ref netNodes[num9], group, num, num2, num3, num4, nodeGroups, pulseGroupCount, ref pulseUnitEnd, ref canContinue);
                        num9 = netNodes[num9].m_nextGridNode;
                    }
                }
            }
        }

        /// <summary>
        /// Substitute for ElectricityManager.GetRootGroup for use in patched execution chains.
        /// </summary>
        /// <param name="group">Group ID.</param>
        /// <returns>Root group ID.</returns>
        private static ushort GetRootGroup(ushort group)
        {
            for (ushort mergeIndex = s_pulseGroups[group].m_mergeIndex; mergeIndex != ushort.MaxValue; mergeIndex = s_pulseGroups[group].m_mergeIndex)
            {
                group = mergeIndex;
            }

            return group;
        }

        /// <summary>
        /// Substitute for ElectricityManager.MergeGroups using expanded structs.
        /// </summary>
        /// <param name="root">Root group..</param>
        /// <param name="merged">Merged group ID.</param>
        /// <param name="pulseGroupCount">ElectricityManager private field - m_pulseGroupCount.</param>
        private static void MergeGroups(ushort root, ushort merged, int pulseGroupCount)
        {
            ExpandedPulseGroup pulseGroup = s_pulseGroups[root];
            ExpandedPulseGroup pulseGroup2 = s_pulseGroups[merged];
            pulseGroup.m_origCharge += pulseGroup2.m_origCharge;
            if (pulseGroup2.m_mergeCount != 0)
            {
                for (int i = 0; i < pulseGroupCount; i++)
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
        }

        /// <summary>
        /// Checks to see whether the given netinfo is an electric road.
        /// </summary>
        /// <param name="netInfo">NetInfo to check.</param>
        /// <returns>True if this network should counduct electricity, false otherwise.</returns>
        private static bool CheckElectricRoad(NetInfo netInfo) => s_electricRoads && (netInfo.m_laneTypes & NetInfo.LaneType.Vehicle) != 0;

        /// <summary>
        /// Substitute for ElectricityManager.UpdateNodeElectricity with support for electric roads.
        /// </summary>
        /// <param name="nodeID">Node ID.</param>
        /// <param name="value">Updated electricity value.</param>
        /// <param name="netManager">NetManager instance.</param>
        /// <param name="netNodes">Network node buffer.</param>
        /// <param name="netInfo">Network info.</param>
        private static void UpdateNodeElectricity(int nodeID, int value, NetManager netManager, NetNode[] netNodes, NetInfo netInfo)
        {
            InfoManager.InfoMode currentMode = Singleton<InfoManager>.instance.CurrentMode;
            bool flag = false;
            NetNode.Flags flags = netManager.m_nodes.m_buffer[nodeID].m_flags;

            // Electric road addition.
            if (s_electricRoads)
            {
                if ((flags & NetNode.Flags.Transition) != 0 && netInfo.m_class.m_service == ItemClass.Service.Electricity)
                {
                    netNodes[nodeID].m_flags &= ~NetNode.Flags.Transition;
                    return;
                }
            }
            else
            {
                if ((flags & NetNode.Flags.Transition) != 0)
                {
                    netNodes[nodeID].m_flags &= ~NetNode.Flags.Transition;
                    return;
                }
            }

            ushort building = netManager.m_nodes.m_buffer[nodeID].m_building;
            if (building != 0)
            {
                BuildingManager buildingManager = Singleton<BuildingManager>.instance;
                Building[] buildings = buildingManager.m_buildings.m_buffer;
                if (buildings[building].m_electricityBuffer != value)
                {
                    buildings[building].m_electricityBuffer = (ushort)value;
                    flag = currentMode == InfoManager.InfoMode.Electricity;
                }

                if (flag)
                {
                    buildingManager.UpdateBuildingColors(building);
                }
            }

            NetNode.Flags flags2 = flags & ~NetNode.Flags.Electricity;
            if (value != 0)
            {
                flags2 |= NetNode.Flags.Electricity;
            }

            if (flags2 != flags)
            {
                netNodes[nodeID].m_flags = flags2;
                flag = currentMode == InfoManager.InfoMode.Electricity;
            }

            if (!flag)
            {
                return;
            }

            netManager.UpdateNodeColors((ushort)nodeID);
            for (int i = 0; i < 8; i++)
            {
                ushort segment = netNodes[nodeID].GetSegment(i);
                if (segment != 0)
                {
                    netManager.UpdateSegmentColors(segment);
                }
            }
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
