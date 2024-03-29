﻿// <copyright file="WaterDataContainer.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace EightyOne2.Serialization
{
    using AlgernonCommons;
    using ColossalFramework;
    using ColossalFramework.IO;
    using HarmonyLib;
    using static Patches.ExpandedWaterManager;
    using static Patches.WaterManagerPatches;
    using static WaterManager;

    /// <summary>
    /// Savegame data container for expanded district data.
    /// Mirroring the original 81 tiles to make backwards-compatibility easier and because no particular reason to change.
    /// </summary>
    public sealed class WaterDataContainer : IDataContainer
    {
        /// <summary>
        /// Saves data to savegame.
        /// Uses legacy 81 Tiles data format.
        /// </summary>
        /// <param name="serializer">Data serializer.</param>
        public void Serialize(DataSerializer serializer)
        {
            // Local references.
            WaterManager waterManager = Singleton<WaterManager>.instance;

            // Water grid.
            Cell[] waterGrid = AccessTools.Field(typeof(WaterManager), "m_waterGrid").GetValue(waterManager) as Cell[];
            int gridLength = waterGrid.Length;
            EncodedArray.Byte encodedBytes = EncodedArray.Byte.BeginWrite(serializer);
            for (int i = 0; i < gridLength; ++i)
            {
                encodedBytes.Write(waterGrid[i].m_conductivity);
            }

            encodedBytes.EndWrite();

            // Heating conductivity.
            encodedBytes = EncodedArray.Byte.BeginWrite(serializer);
            for (int i = 0; i < gridLength; ++i)
            {
                encodedBytes.Write(waterGrid[i].m_conductivity2);
            }

            encodedBytes.EndWrite();

            // Water pressure.
            EncodedArray.Short encodedShorts = EncodedArray.Short.BeginWrite(serializer);
            for (int i = 0; i < gridLength; ++i)
            {
                if (waterGrid[i].m_conductivity != 0)
                {
                    encodedShorts.Write(waterGrid[i].m_currentWaterPressure);
                }
            }

            encodedShorts.EndWrite();

            // Sewage pressure.
            encodedShorts = EncodedArray.Short.BeginWrite(serializer);
            for (int i = 0; i < gridLength; ++i)
            {
                if (waterGrid[i].m_conductivity != 0)
                {
                    encodedShorts.Write(waterGrid[i].m_currentSewagePressure);
                }
            }

            encodedShorts.EndWrite();

            // Heating pressure.
            encodedShorts = EncodedArray.Short.BeginWrite(serializer);
            for (int i = 0; i < gridLength; ++i)
            {
                if (waterGrid[i].m_conductivity2 != 0)
                {
                    encodedShorts.Write(waterGrid[i].m_currentHeatingPressure);
                }
            }

            encodedShorts.EndWrite();

            // Water pulse group references.
            EncodedArray.UShort encodedUShorts = EncodedArray.UShort.BeginWrite(serializer);
            for (int i = 0; i < gridLength; ++i)
            {
                if (waterGrid[i].m_conductivity != 0)
                {
                    encodedUShorts.Write(waterGrid[i].m_waterPulseGroup);
                }
            }

            encodedUShorts.EndWrite();

            // Sewage pulse group references.
            encodedUShorts = EncodedArray.UShort.BeginWrite(serializer);
            for (int i = 0; i < gridLength; ++i)
            {
                if (waterGrid[i].m_conductivity != 0)
                {
                    encodedUShorts.Write(waterGrid[i].m_sewagePulseGroup);
                }
            }

            encodedUShorts.EndWrite();

            // Heating pulse group references.
            encodedUShorts = EncodedArray.UShort.BeginWrite(serializer);
            for (int i = 0; i < gridLength; ++i)
            {
                if (waterGrid[i].m_conductivity2 != 0)
                {
                    encodedUShorts.Write(waterGrid[i].m_heatingPulseGroup);
                }
            }

            encodedUShorts.EndWrite();

            // Water closest pipe segment.
            encodedUShorts = EncodedArray.UShort.BeginWrite(serializer);
            for (int i = 0; i < gridLength; ++i)
            {
                if (waterGrid[i].m_conductivity != 0)
                {
                    encodedUShorts.Write(waterGrid[i].m_closestPipeSegment);
                }
            }

            encodedUShorts.EndWrite();

            // Heating closest pipe segment.
            encodedUShorts = EncodedArray.UShort.BeginWrite(serializer);
            for (int i = 0; i < gridLength; ++i)
            {
                if (waterGrid[i].m_conductivity2 != 0)
                {
                    encodedUShorts.Write(waterGrid[i].m_closestPipeSegment2);
                }
            }

            encodedUShorts.EndWrite();

            // Water states.
            EncodedArray.Bool encodedBools = EncodedArray.Bool.BeginWrite(serializer);
            for (int i = 0; i < gridLength; ++i)
            {
                if (waterGrid[i].m_conductivity != 0)
                {
                    encodedBools.Write(waterGrid[i].m_hasWater);
                }
            }

            encodedBools.EndWrite();

            // Sewage states.
            encodedBools = EncodedArray.Bool.BeginWrite(serializer);
            for (int i = 0; i < gridLength; ++i)
            {
                if (waterGrid[i].m_conductivity != 0)
                {
                    encodedBools.Write(waterGrid[i].m_hasSewage);
                }
            }

            encodedBools.EndWrite();

            // Heating states.
            encodedBools = EncodedArray.Bool.BeginWrite(serializer);
            for (int i = 0; i < gridLength; ++i)
            {
                if (waterGrid[i].m_conductivity2 != 0)
                {
                    encodedBools.Write(waterGrid[i].m_hasHeating);
                }
            }

            encodedBools.EndWrite();

            // Temporary water states.
            encodedBools = EncodedArray.Bool.BeginWrite(serializer);
            for (int i = 0; i < gridLength; ++i)
            {
                if (waterGrid[i].m_conductivity != 0)
                {
                    encodedBools.Write(waterGrid[i].m_tmpHasWater);
                }
            }

            encodedBools.EndWrite();

            // Temporary sewage states.
            encodedBools = EncodedArray.Bool.BeginWrite(serializer);
            for (int i = 0; i < gridLength; ++i)
            {
                if (waterGrid[i].m_conductivity != 0)
                {
                    encodedBools.Write(waterGrid[i].m_tmpHasSewage);
                }
            }

            encodedBools.EndWrite();

            // Temporary heating states.
            encodedBools = EncodedArray.Bool.BeginWrite(serializer);
            for (int i = 0; i < gridLength; ++i)
            {
                if (waterGrid[i].m_conductivity2 != 0)
                {
                    encodedBools.Write(waterGrid[i].m_tmpHasHeating);
                }
            }

            encodedBools.EndWrite();

            // Water pollution.
            encodedBytes = EncodedArray.Byte.BeginWrite(serializer);
            for (int i = 0; i < gridLength; ++i)
            {
                if (waterGrid[i].m_conductivity != 0)
                {
                    encodedBytes.Write(waterGrid[i].m_pollution);
                }
            }

            encodedBytes.EndWrite();

            // Water pulse groups.
            PulseGroup[] waterPulseGroups = AccessTools.Field(typeof(WaterManager), "m_waterPulseGroups").GetValue(waterManager) as PulseGroup[];
            int waterPulseGroupCount = (int)AccessTools.Field(typeof(WaterManager), "m_waterPulseGroupCount").GetValue(waterManager);
            serializer.WriteUInt16((ushort)waterPulseGroupCount);
            WritePulseGroups(serializer, waterPulseGroups, waterPulseGroupCount);

            // Sewage pulse groups.
            PulseGroup[] sewagePulseGroups = AccessTools.Field(typeof(WaterManager), "m_sewagePulseGroups").GetValue(waterManager) as PulseGroup[];
            int sewagePulseGroupCount = (int)AccessTools.Field(typeof(WaterManager), "m_sewagePulseGroupCount").GetValue(waterManager);
            serializer.WriteUInt16((ushort)sewagePulseGroupCount);
            WritePulseGroups(serializer, sewagePulseGroups, sewagePulseGroupCount);

            // Heating pulse groups.
            PulseGroup[] heatingPulseGroups = AccessTools.Field(typeof(WaterManager), "m_heatingPulseGroups").GetValue(waterManager) as PulseGroup[];
            int heatingPulseGroupCount = (int)AccessTools.Field(typeof(WaterManager), "m_heatingPulseGroupCount").GetValue(waterManager);
            serializer.WriteUInt16((ushort)heatingPulseGroupCount);
            WritePulseGroups(serializer, heatingPulseGroups, heatingPulseGroupCount);

            // Water pulse units.
            WritePulseUnits(
                serializer,
                WaterPulseUnits,
                (int)AccessTools.Field(typeof(WaterManager), "m_waterPulseUnitStart").GetValue(waterManager),
                (int)AccessTools.Field(typeof(WaterManager), "m_waterPulseUnitEnd").GetValue(waterManager));

            // Sewage pulse units.
            WritePulseUnits(
                serializer,
                SewagePulseUnits,
                (int)AccessTools.Field(typeof(WaterManager), "m_sewagePulseUnitStart").GetValue(waterManager),
                (int)AccessTools.Field(typeof(WaterManager), "m_sewagePulseUnitEnd").GetValue(waterManager));

            // Heating pulse units.
            WritePulseUnits(
                serializer,
                HeatingPulseUnits,
                (int)AccessTools.Field(typeof(WaterManager), "m_heatingPulseUnitStart").GetValue(waterManager),
                (int)AccessTools.Field(typeof(WaterManager), "m_heatingPulseUnitEnd").GetValue(waterManager));

            // Remaining private fields.
            serializer.WriteInt32((int)AccessTools.Field(typeof(WaterManager), "m_processedCells").GetValue(waterManager));
            serializer.WriteInt32((int)AccessTools.Field(typeof(WaterManager), "m_conductiveCells").GetValue(waterManager));
            serializer.WriteBool((bool)AccessTools.Field(typeof(WaterManager), "m_canContinue").GetValue(waterManager));
        }

        /// <summary>
        /// Reads data from savegame.
        /// Uses legacy 81 Tiles data format.
        /// </summary>
        /// <param name="serializer">Data serializer.</param>
        public void Deserialize(DataSerializer serializer)
        {
            // Local reference.
            WaterManager waterManager = Singleton<WaterManager>.instance;

            // Create expanded water grid array.
            Cell[] waterGrid = new Cell[ExpandedWaterGridArraySize];
            AccessTools.Field(typeof(WaterManager), "m_waterGrid").SetValue(waterManager, waterGrid);

            // Water grid.
            EncodedArray.Byte waterConductivity = EncodedArray.Byte.BeginRead(serializer);
            for (int i = 0; i < ExpandedWaterGridArraySize; ++i)
            {
                waterGrid[i].m_conductivity = waterConductivity.Read();
            }

            waterConductivity.EndRead();

            // Heating grid.
            EncodedArray.Byte heatingConductivity = EncodedArray.Byte.BeginRead(serializer);
            for (int i = 0; i < ExpandedWaterGridArraySize; ++i)
            {
                waterGrid[i].m_conductivity2 = heatingConductivity.Read();
            }

            heatingConductivity.EndRead();

            // Water pressure.
            EncodedArray.Short currentWaterPressures = EncodedArray.Short.BeginRead(serializer);
            for (int i = 0; i < ExpandedWaterGridArraySize; ++i)
            {
                if (waterGrid[i].m_conductivity != 0)
                {
                    waterGrid[i].m_currentWaterPressure = currentWaterPressures.Read();
                }
                else
                {
                    waterGrid[i].m_currentWaterPressure = 0;
                }
            }

            currentWaterPressures.EndRead();

            // Sewage pressure.
            EncodedArray.Short currentSewagePressures = EncodedArray.Short.BeginRead(serializer);
            for (int i = 0; i < ExpandedWaterGridArraySize; ++i)
            {
                if (waterGrid[i].m_conductivity != 0)
                {
                    waterGrid[i].m_currentSewagePressure = currentSewagePressures.Read();
                }
                else
                {
                    waterGrid[i].m_currentSewagePressure = 0;
                }
            }

            currentSewagePressures.EndRead();

            // Heating pressure.
            EncodedArray.Short currentHeatingPressures = EncodedArray.Short.BeginRead(serializer);
            for (int i = 0; i < ExpandedWaterGridArraySize; ++i)
            {
                if (waterGrid[i].m_conductivity2 != 0)
                {
                    waterGrid[i].m_currentHeatingPressure = currentHeatingPressures.Read();
                }
                else
                {
                    waterGrid[i].m_currentHeatingPressure = 0;
                }
            }

            currentHeatingPressures.EndRead();

            // Water pulse group references.
            EncodedArray.UShort waterPulseGroupData = EncodedArray.UShort.BeginRead(serializer);
            for (int i = 0; i < ExpandedWaterGridArraySize; ++i)
            {
                if (waterGrid[i].m_conductivity != 0)
                {
                    waterGrid[i].m_waterPulseGroup = waterPulseGroupData.Read();
                }
                else
                {
                    waterGrid[i].m_waterPulseGroup = 0;
                }
            }

            waterPulseGroupData.EndRead();

            // Sewage pulse group references.
            EncodedArray.UShort sewagePulseGroupData = EncodedArray.UShort.BeginRead(serializer);
            for (int i = 0; i < ExpandedWaterGridArraySize; ++i)
            {
                if (waterGrid[i].m_conductivity != 0)
                {
                    waterGrid[i].m_sewagePulseGroup = sewagePulseGroupData.Read();
                }
                else
                {
                    waterGrid[i].m_sewagePulseGroup = 0;
                }
            }

            sewagePulseGroupData.EndRead();

            // Heating pulse group references.
            EncodedArray.UShort heatingPulseGroupData = EncodedArray.UShort.BeginRead(serializer);
            for (int i = 0; i < ExpandedWaterGridArraySize; ++i)
            {
                if (waterGrid[i].m_conductivity2 != 0)
                {
                    waterGrid[i].m_heatingPulseGroup = heatingPulseGroupData.Read();
                }
                else
                {
                    waterGrid[i].m_heatingPulseGroup = 0;
                }
            }

            heatingPulseGroupData.EndRead();

            // Water closest pipe segment.
            EncodedArray.UShort closestWaterSegments = EncodedArray.UShort.BeginRead(serializer);
            for (int i = 0; i < ExpandedWaterGridArraySize; ++i)
            {
                if (waterGrid[i].m_conductivity != 0)
                {
                    waterGrid[i].m_closestPipeSegment = closestWaterSegments.Read();
                }
            }

            closestWaterSegments.EndRead();

            // Heating closest pipe segment.
            EncodedArray.UShort closestHeatingSegments = EncodedArray.UShort.BeginRead(serializer);
            for (int i = 0; i < ExpandedWaterGridArraySize; ++i)
            {
                if (waterGrid[i].m_conductivity2 != 0)
                {
                    waterGrid[i].m_closestPipeSegment2 = closestHeatingSegments.Read();
                }
            }

            closestHeatingSegments.EndRead();

            // Water states.
            EncodedArray.Bool hasWater = EncodedArray.Bool.BeginRead(serializer);
            for (int i = 0; i < ExpandedWaterGridArraySize; ++i)
            {
                if (waterGrid[i].m_conductivity != 0)
                {
                    waterGrid[i].m_hasWater = hasWater.Read();
                }
                else
                {
                    waterGrid[i].m_hasWater = false;
                }
            }

            hasWater.EndRead();

            // Sewage states.
            EncodedArray.Bool hasSewage = EncodedArray.Bool.BeginRead(serializer);
            for (int i = 0; i < ExpandedWaterGridArraySize; ++i)
            {
                if (waterGrid[i].m_conductivity != 0)
                {
                    waterGrid[i].m_hasSewage = hasSewage.Read();
                }
                else
                {
                    waterGrid[i].m_hasSewage = false;
                }
            }

            hasSewage.EndRead();

            // Heating states.
            EncodedArray.Bool hasHeating = EncodedArray.Bool.BeginRead(serializer);
            for (int i = 0; i < ExpandedWaterGridArraySize; ++i)
            {
                if (waterGrid[i].m_conductivity2 != 0)
                {
                    waterGrid[i].m_hasHeating = hasHeating.Read();
                }
                else
                {
                    waterGrid[i].m_hasHeating = false;
                }
            }

            hasHeating.EndRead();

            // Temporary water states.
            EncodedArray.Bool tmpHasWater = EncodedArray.Bool.BeginRead(serializer);
            for (int i = 0; i < ExpandedWaterGridArraySize; ++i)
            {
                if (waterGrid[i].m_conductivity != 0)
                {
                    waterGrid[i].m_tmpHasWater = tmpHasWater.Read();
                }
                else
                {
                    waterGrid[i].m_tmpHasWater = false;
                }
            }

            tmpHasWater.EndRead();

            // Temporary sewage states.
            EncodedArray.Bool tmpHasSewage = EncodedArray.Bool.BeginRead(serializer);
            for (int i = 0; i < ExpandedWaterGridArraySize; ++i)
            {
                if (waterGrid[i].m_conductivity != 0)
                {
                    waterGrid[i].m_tmpHasSewage = tmpHasSewage.Read();
                }
                else
                {
                    waterGrid[i].m_tmpHasSewage = false;
                }
            }

            tmpHasSewage.EndRead();

            // Temporary heating states.
            EncodedArray.Bool tmpHasHeating = EncodedArray.Bool.BeginRead(serializer);
            for (int i = 0; i < ExpandedWaterGridArraySize; ++i)
            {
                if (waterGrid[i].m_conductivity2 != 0)
                {
                    waterGrid[i].m_tmpHasHeating = tmpHasHeating.Read();
                }
                else
                {
                    waterGrid[i].m_tmpHasHeating = false;
                }
            }

            tmpHasHeating.EndRead();

            // Water pollution.
            EncodedArray.Byte pollution = EncodedArray.Byte.BeginRead(serializer);
            for (int i = 0; i < ExpandedWaterGridArraySize; ++i)
            {
                if (waterGrid[i].m_conductivity != 0)
                {
                    waterGrid[i].m_pollution = pollution.Read();
                }
                else
                {
                    waterGrid[i].m_pollution = 0;
                }
            }

            pollution.EndRead();

            // Water pulse groups.
            PulseGroup[] waterPulseGroups = AccessTools.Field(typeof(WaterManager), "m_waterPulseGroups").GetValue(waterManager) as PulseGroup[];
            AccessTools.Field(typeof(WaterManager), "m_waterPulseGroupCount").SetValue(waterManager, ReadPulseGroups(serializer, waterPulseGroups));

            // Sewage pulse groups.
            PulseGroup[] sewagePulseGroups = AccessTools.Field(typeof(WaterManager), "m_sewagePulseGroups").GetValue(waterManager) as PulseGroup[];
            AccessTools.Field(typeof(WaterManager), "m_sewagePulseGroupCount").SetValue(waterManager, ReadPulseGroups(serializer, sewagePulseGroups));

            // Heating pulse groups.
            PulseGroup[] heatingPulseGroups = AccessTools.Field(typeof(WaterManager), "m_heatingPulseGroups").GetValue(waterManager) as PulseGroup[];
            AccessTools.Field(typeof(WaterManager), "m_heatingPulseGroupCount").SetValue(waterManager, ReadPulseGroups(serializer, heatingPulseGroups));

            // Water pulse units.
            AccessTools.Field(typeof(WaterManager), "m_waterPulseUnitStart").SetValue(waterManager, 0);
            AccessTools.Field(typeof(WaterManager), "m_waterPulseUnitEnd").SetValue(waterManager, ReadPulseUnits(serializer, WaterPulseUnits));

            // Sewage pulse units.
            AccessTools.Field(typeof(WaterManager), "m_sewagePulseUnitStart").SetValue(waterManager, 0);
            AccessTools.Field(typeof(WaterManager), "m_sewagePulseUnitEnd").SetValue(waterManager, ReadPulseUnits(serializer, SewagePulseUnits));

            // Heating pulse units.
            AccessTools.Field(typeof(WaterManager), "m_heatingPulseUnitStart").SetValue(waterManager, 0);
            AccessTools.Field(typeof(WaterManager), "m_heatingPulseUnitEnd").SetValue(waterManager, ReadPulseUnits(serializer, HeatingPulseUnits));

            // Remaining private fields.
            AccessTools.Field(typeof(WaterManager), "m_processedCells").SetValue(waterManager, serializer.ReadInt32());
            AccessTools.Field(typeof(WaterManager), "m_conductiveCells").SetValue(waterManager, serializer.ReadInt32());
            AccessTools.Field(typeof(WaterManager), "m_canContinue").SetValue(waterManager, serializer.ReadBool());
        }

        /// <summary>
        /// Performs any post-deserialization activities.  Nothing to do here (required by IDataContainer).
        /// </summary>
        /// <param name="serializer">Data serializer.</param>
        public void AfterDeserialize(DataSerializer serializer)
        {
        }

        /// <summary>
        /// Deserializes a given pulse group array from savegame.
        /// </summary>
        /// <param name="serializer">Serializer to use.</param>
        /// <param name="pulseGroups">PulseGroup array to deserialize to.</param>
        /// <returns>Pulse group count.</returns>
        private static int ReadPulseGroups(DataSerializer serializer, PulseGroup[] pulseGroups)
        {
            int pulseGroupCount = (int)serializer.ReadUInt16();

            for (int i = 0; i < pulseGroupCount; ++i)
            {
                pulseGroups[i].m_origPressure = serializer.ReadUInt32();
                pulseGroups[i].m_curPressure = serializer.ReadUInt32();
                pulseGroups[i].m_collectPressure = serializer.ReadUInt32();
                pulseGroups[i].m_mergeIndex = (ushort)serializer.ReadUInt16();
                pulseGroups[i].m_mergeCount = (ushort)serializer.ReadUInt16();
                pulseGroups[i].m_node = (ushort)serializer.ReadUInt16();

                // Check for invalid data.
                if (pulseGroups[i].m_node >= NetManager.MAX_NODE_COUNT)
                {
                    Logging.Error("found invalid water PulseGroup node index of ", pulseGroups[i].m_node);
                    pulseGroups[i].m_node = 0;
                }
            }

            return pulseGroupCount;
        }

        /// <summary>
        /// Deserializes a given expanded pulse unit array from savegame.
        /// </summary>
        /// <param name="serializer">Serializer to use.</param>
        /// <param name="pulseUnits">ExpandedPulseUnit array to deserialize to.</param>
        /// <returns>Pulse unit end.</returns>
        private static int ReadPulseUnits(DataSerializer serializer, ExpandedPulseUnit[] pulseUnits)
        {
            int pulseUnitCount = (int)serializer.ReadUInt16();

            Logging.Message("reading PulseUnits with count ", pulseUnitCount);

            // Pulse unit array.
            for (int i = 0; i < pulseUnitCount; ++i)
            {
                pulseUnits[i].m_group = (ushort)serializer.ReadUInt16();
                pulseUnits[i].m_node = (ushort)serializer.ReadUInt16();
                pulseUnits[i].m_x = (ushort)serializer.ReadUInt16();
                pulseUnits[i].m_z = (ushort)serializer.ReadUInt16();

                // Checks for invalid data.
                if (pulseUnits[i].m_group >= WaterManager.MAX_PULSE_GROUPS)
                {
                    Logging.Error("found invalid water PulseUnit group index of ", pulseUnits[i].m_group);
                    pulseUnits[i].m_group = 0;
                }

                // Check for invalid data.
                if (pulseUnits[i].m_node >= NetManager.MAX_NODE_COUNT)
                {
                    Logging.Error("found invalid water PulseUnit node index of ", pulseUnits[i].m_node);
                    pulseUnits[i].m_node = 0;
                }

                // Check for invalid data.
                if (pulseUnits[i].m_x >= ExpandedWaterGridResolution)
                {
                    Logging.Error("found invalid water PulseUnit x coordinate of ", pulseUnits[i].m_x);
                    pulseUnits[i].m_x = 0;
                }

                // Check for invalid data.
                if (pulseUnits[i].m_z >= ExpandedWaterGridResolution)
                {
                    Logging.Error("found invalid water PulseUnit z coordinate of ", pulseUnits[i].m_z);
                    pulseUnits[i].m_z = 0;
                }
            }

            return pulseUnitCount % pulseUnits.Length;
        }

        /// <summary>
        /// Serializes a given pulse group array to savegame.
        /// </summary>
        /// <param name="serializer">Serializer to use.</param>
        /// <param name="pulseGroups">PulseGroup array to serialize.</param>
        /// <param name="pulseGroupCount">Pulse group count.</param>
        private static void WritePulseGroups(DataSerializer serializer, PulseGroup[] pulseGroups, int pulseGroupCount)
        {
            for (int i = 0; i < pulseGroupCount; ++i)
            {
                serializer.WriteUInt32(pulseGroups[i].m_origPressure);
                serializer.WriteUInt32(pulseGroups[i].m_curPressure);
                serializer.WriteUInt32(pulseGroups[i].m_collectPressure);
                serializer.WriteUInt16(pulseGroups[i].m_mergeIndex);
                serializer.WriteUInt16(pulseGroups[i].m_mergeCount);
                serializer.WriteUInt16(pulseGroups[i].m_node);
            }
        }

        /// <summary>
        /// Serializes a given expanded pulse unit array to savegame.
        /// </summary>
        /// <param name="serializer">Serializer to use.</param>
        /// <param name="pulseUnits">ExpandedPulseUnit array to serialize.</param>
        /// <param name="pulseUnitStart">Pulse unit starting index.</param>
        /// <param name="pulseUnitEnd">Pulse unit ending index.</param>
        private static void WritePulseUnits(DataSerializer serializer, ExpandedPulseUnit[] pulseUnits, int pulseUnitStart, int pulseUnitEnd)
        {
            // Calculate pulse unit count.
            int pulseUnitCount = pulseUnitEnd - pulseUnitStart;
            if (pulseUnitCount < 0)
            {
                // If count is negative, wrap around.
                pulseUnitCount += pulseUnits.Length;
            }

            serializer.WriteUInt16((ushort)pulseUnitCount);

            // Pulse unit array - waraparound.
            int pulseUnitIndex = pulseUnitStart;
            while (pulseUnitIndex != pulseUnitEnd)
            {
                serializer.WriteUInt16(pulseUnits[pulseUnitIndex].m_group);
                serializer.WriteUInt16(pulseUnits[pulseUnitIndex].m_node);
                serializer.WriteUInt16(pulseUnits[pulseUnitIndex].m_x);
                serializer.WriteInt16(pulseUnits[pulseUnitIndex].m_z);

                // Index wraps around when it reaches the maximum.
                if (++pulseUnitIndex >= pulseUnits.Length)
                {
                    pulseUnitIndex = 0;
                }
            }
        }
    }
}
