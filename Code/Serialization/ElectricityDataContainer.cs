// <copyright file="ElectricityDataContainer.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace EightyOne2.Serialization
{
    using ColossalFramework;
    using ColossalFramework.IO;
    using HarmonyLib;
    using static ElectricityManager;
    using static Patches.ElectricityManagerPatches;
    using static Patches.ExpandedElectricityManager;

    /// <summary>
    /// Savegame data container for expanded district data.
    /// Mirroring the original 81 tiles to make backwards-compatibility easier and because no particular reason to change.
    /// </summary>
    public sealed class ElectricityDataContainer : IDataContainer
    {
        /// <summary>
        /// Saves data to savegame.
        /// Uses legacy 81 Tiles data format.
        /// </summary>
        /// <param name="serializer">Data serializer.</param>
        public void Serialize(DataSerializer serializer)
        {
            // Local references.
            ElectricityManager electricityManager = Singleton<ElectricityManager>.instance;

            // Electricity grid.
            Cell[] electricityGrid = AccessTools.Field(typeof(ElectricityManager), "m_electricityGrid").GetValue(electricityManager) as Cell[];
            EncodedArray.Byte encodedBytes = EncodedArray.Byte.BeginWrite(serializer);
            for (int i = 0; i < ExpandedElectricityGridArraySize; ++i)
            {
                encodedBytes.Write(electricityGrid[i].m_conductivity);
            }

            encodedBytes.EndWrite();

            // Current charges.
            EncodedArray.Short encodedShorts = EncodedArray.Short.BeginWrite(serializer);
            for (int i = 0; i < ExpandedElectricityGridArraySize; ++i)
            {
                if (electricityGrid[i].m_conductivity != 0)
                {
                    encodedShorts.Write(electricityGrid[i].m_currentCharge);
                }
            }

            encodedShorts.EndWrite();

            // Extra charges.
            EncodedArray.UShort encodedUShorts = EncodedArray.UShort.BeginWrite(serializer);
            for (int i = 0; i < ExpandedElectricityGridArraySize; ++i)
            {
                if (electricityGrid[i].m_conductivity != 0)
                {
                    encodedUShorts.Write(electricityGrid[i].m_extraCharge);
                }
            }

            encodedUShorts.EndWrite();

            // Pulse group references.
            encodedUShorts = EncodedArray.UShort.BeginWrite(serializer);
            for (int i = 0; i < ExpandedElectricityGridArraySize; ++i)
            {
                if (electricityGrid[i].m_conductivity != 0)
                {
                    encodedUShorts.Write(electricityGrid[i].m_pulseGroup);
                }
            }

            encodedUShorts.EndWrite();

            // Electrified states.
            EncodedArray.Bool encodedBools = EncodedArray.Bool.BeginWrite(serializer);
            for (int i = 0; i < ExpandedElectricityGridArraySize; ++i)
            {
                if (electricityGrid[i].m_conductivity != 0)
                {
                    encodedBools.Write(electricityGrid[i].m_electrified);
                }
            }

            encodedBools.EndWrite();

            // Temporary states.
            encodedBools = EncodedArray.Bool.BeginWrite(serializer);
            for (int i = 0; i < ExpandedElectricityGridArraySize; ++i)
            {
                if (electricityGrid[i].m_conductivity != 0)
                {
                    encodedBools.Write(electricityGrid[i].m_tmpElectrified);
                }
            }

            encodedBools.EndWrite();

            AlgernonCommons.Logging.Message("writing pulseGroups");

            // Pulse groups.
            ExpandedPulseGroup[] pulseGroups = PulseGroups;
            int pulseGroupCount = (int)AccessTools.Field(typeof(ElectricityManager), "m_pulseGroupCount").GetValue(electricityManager);
            serializer.WriteUInt16((ushort)pulseGroupCount);

            // Pulse group array.
            for (int i = 0; i < pulseGroupCount; ++i)
            {
                serializer.WriteUInt32(pulseGroups[i].m_origCharge);
                serializer.WriteUInt32(pulseGroups[i].m_curCharge);
                serializer.WriteUInt16(pulseGroups[i].m_mergeIndex);
                serializer.WriteUInt16(pulseGroups[i].m_mergeCount);
                serializer.WriteUInt16(pulseGroups[i].m_x);
                serializer.WriteUInt16(pulseGroups[i].m_z);
            }

            AlgernonCommons.Logging.Message("writing pulseUnits");

            // Pulse units.
            ExpandedPulseUnit[] pulseUnits = PulseUnits;
            int pulseUnitLength = pulseUnits.Length;
            int pulseUnitStart = (int)AccessTools.Field(typeof(ElectricityManager), "m_pulseUnitStart").GetValue(electricityManager);
            int pulseUnitEnd = (int)AccessTools.Field(typeof(ElectricityManager), "m_pulseUnitEnd").GetValue(electricityManager);

            // Calculate pulse unit count.
            int pulseUnitCount = pulseUnitEnd - pulseUnitStart;
            if (pulseUnitCount < 0)
            {
                // If count is negative, wrap around.
                pulseUnitCount += pulseUnitLength;
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
                if (++pulseUnitIndex >= pulseUnitLength)
                {
                    pulseUnitIndex = 0;
                }
            }

            // Node groups.
            ushort[] nodeGroups = electricityManager.m_nodeGroups;
            encodedUShorts = EncodedArray.UShort.BeginWrite(serializer);
            for (int i = 0; i < 32768; ++i)
            {
                encodedUShorts.Write(nodeGroups[i]);
            }

            encodedUShorts.EndWrite();

            // Remaining private fields.
            serializer.WriteInt32((int)AccessTools.Field(typeof(ElectricityManager), "m_processedCells").GetValue(electricityManager));
            serializer.WriteInt32((int)AccessTools.Field(typeof(ElectricityManager), "m_conductiveCells").GetValue(electricityManager));
            serializer.WriteBool((bool)AccessTools.Field(typeof(ElectricityManager), "m_canContinue").GetValue(electricityManager));
        }

        /// <summary>
        /// Reads data from savegame.
        /// Uses legacy 81 Tiles data format.
        /// </summary>
        /// <param name="serializer">Data serializer.</param>
        public void Deserialize(DataSerializer serializer)
        {
            // Local reference.
            ElectricityManager electricityManager = Singleton<ElectricityManager>.instance;

            // Create expanded electricity grid array.
            Cell[] electricityGrid = new Cell[ExpandedElectricityGridArraySize];
            AccessTools.Field(typeof(ElectricityManager), "m_electricityGrid").SetValue(electricityManager, electricityGrid);

            // Electricity grid.
            EncodedArray.Byte encodedBytes = EncodedArray.Byte.BeginRead(serializer);
            for (int i = 0; i < ExpandedElectricityGridArraySize; ++i)
            {
                electricityGrid[i].m_conductivity = encodedBytes.Read();
            }

            encodedBytes.EndRead();

            // Current charges.
            EncodedArray.Short encodedShorts = EncodedArray.Short.BeginRead(serializer);
            for (int i = 0; i < ExpandedElectricityGridArraySize; ++i)
            {
                if (electricityGrid[i].m_conductivity != 0)
                {
                    electricityGrid[i].m_currentCharge = encodedShorts.Read();
                }
                else
                {
                    electricityGrid[i].m_currentCharge = 0;
                }
            }

            encodedShorts.EndRead();

            // Extra charges.
            EncodedArray.UShort encodedUShorts = EncodedArray.UShort.BeginRead(serializer);
            for (int i = 0; i < ExpandedElectricityGridArraySize; ++i)
            {
                if (electricityGrid[i].m_conductivity != 0)
                {
                    electricityGrid[i].m_extraCharge = encodedUShorts.Read();
                }
                else
                {
                    electricityGrid[i].m_extraCharge = 0;
                }
            }

            encodedUShorts.EndRead();

            // Pulse group references.
            encodedUShorts = EncodedArray.UShort.BeginRead(serializer);
            for (int i = 0; i < ExpandedElectricityGridArraySize; ++i)
            {
                if (electricityGrid[i].m_conductivity != 0)
                {
                    electricityGrid[i].m_pulseGroup = encodedUShorts.Read();
                }
                else
                {
                    electricityGrid[i].m_pulseGroup = ushort.MaxValue;
                }
            }

            encodedUShorts.EndRead();

            // Electrified states.
            EncodedArray.Bool encodedBools = EncodedArray.Bool.BeginRead(serializer);
            for (int i = 0; i < ExpandedElectricityGridArraySize; ++i)
            {
                if (electricityGrid[i].m_conductivity != 0)
                {
                    electricityGrid[i].m_electrified = encodedBools.Read();
                }
                else
                {
                    electricityGrid[i].m_electrified = false;
                }
            }

            encodedBools.EndRead();

            // Temporary states.
            encodedBools = EncodedArray.Bool.BeginRead(serializer);
            for (int i = 0; i < ExpandedElectricityGridArraySize; ++i)
            {
                if (electricityGrid[i].m_conductivity != 0)
                {
                    electricityGrid[i].m_tmpElectrified = encodedBools.Read();
                }
                else
                {
                    electricityGrid[i].m_tmpElectrified = false;
                }
            }

            encodedBools.EndRead();

            // Pulse groups.
            ExpandedPulseGroup[] pulseGroups = PulseGroups;
            int pulseGroupCount = (int)serializer.ReadUInt16();
            AccessTools.Field(typeof(ElectricityManager), "m_pulseGroupCount").SetValue(electricityManager, pulseGroupCount);

            // Pulse group array.
            for (int i = 0; i < pulseGroupCount; ++i)
            {
                pulseGroups[i].m_origCharge = serializer.ReadUInt32();
                pulseGroups[i].m_curCharge = serializer.ReadUInt32();
                pulseGroups[i].m_mergeIndex = (ushort)serializer.ReadUInt16();
                pulseGroups[i].m_mergeCount = (ushort)serializer.ReadUInt16();
                pulseGroups[i].m_x = (ushort)serializer.ReadUInt16();
                pulseGroups[i].m_z = (ushort)serializer.ReadUInt16();
            }

            // Pulse units.
            ExpandedPulseUnit[] pulseUnits = PulseUnits;
            int pulseUnitCount = (int)serializer.ReadUInt16();
            AccessTools.Field(typeof(ElectricityManager), "m_pulseUnitStart").SetValue(electricityManager, 0);
            AccessTools.Field(typeof(ElectricityManager), "m_pulseUnitEnd").SetValue(electricityManager, pulseUnitCount % pulseUnits.Length);

            // Pulse unit array.
            for (int i = 0; i < pulseUnitCount; ++i)
            {
                pulseUnits[i].m_group = (ushort)serializer.ReadUInt16();
                pulseUnits[i].m_node = (ushort)serializer.ReadUInt16();
                pulseUnits[i].m_x = (ushort)serializer.ReadUInt16();
                pulseUnits[i].m_z = (ushort)serializer.ReadUInt16();
            }

            // Node groups.
            ushort[] nodeGroups = electricityManager.m_nodeGroups;
            encodedUShorts = EncodedArray.UShort.BeginRead(serializer);
            for (int i = 0; i < 32768; ++i)
            {
                nodeGroups[i] = encodedUShorts.Read();
            }

            encodedUShorts.EndRead();

            // Remaining private fields.
            AccessTools.Field(typeof(ElectricityManager), "m_processedCells").SetValue(electricityManager, serializer.ReadInt32());
            AccessTools.Field(typeof(ElectricityManager), "m_conductiveCells").SetValue(electricityManager, serializer.ReadInt32());
            AccessTools.Field(typeof(ElectricityManager), "m_canContinue").SetValue(electricityManager, serializer.ReadBool());
        }

        /// <summary>
        /// Performs any post-deserialization activities.  Nothing to do here (required by IDataContainer).
        /// </summary>
        /// <param name="serializer">Data serializer.</param>
        public void AfterDeserialize(DataSerializer serializer)
        {
        }
    }
}
