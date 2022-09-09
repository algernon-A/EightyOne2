// <copyright file="ElectricityManagerDataPatches.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace EightyOne2.Patches
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection.Emit;
    using AlgernonCommons;
    using ColossalFramework;
    using ColossalFramework.IO;
    using EightyOne2.Serialization;
    using HarmonyLib;
    using UnityEngine;
    using static ElectricityManager;
    using static ElectricityManagerPatches;
    using static ExpandedElectricityManager;

    /// <summary>
    /// Harmony patches for the electricity manager's data handling to implement 81 tiles functionality.
    /// </summary>
    [HarmonyPatch(typeof(Data))]
    internal static class ElectricityManagerDataPatches
    {
        // Data conversion offset - outer margin of 25-tile data when placed in an 81-tile context.
        private const int CellConversionOffset = (int)ExpandedElectricityGridHalfResolution - (int)GameElectricyGridHalfResolution;

        /// <summary>
        /// Harmony transpiler for ElectricityManager.Data.AfterDeserialize to replace hardcoded game constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(Data.AfterDeserialize))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> AfterDeserializeTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceElectricityConstants(instructions);

        /// <summary>
        /// Harmony transpiler for ElectricityManager.Data.Deserialize to insert call to custom deserialize method.
        /// Done this way instead of via PostFix as we need the original ElectricityManager instance (Harmomy Postfix will only give ElectricityManager.Data instance).
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(Data.Deserialize))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> DeserializeTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            // Insert call to our custom method immediately before the end of the target method (final ret).
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Ret)
                {
                    // Call custom method.
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(OpCodes.Ldloc_1);
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ElectricityManager), "m_pulseGroups"));
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ElectricityManager), "m_pulseUnits"));
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ElectricityManagerDataPatches), nameof(CustomDeserialize)));
                }

                yield return instruction;
            }
        }

        /// <summary>
        /// Harmony transpiler for ElectricityManager.Data.Serialize to insert call to custom serialize method at the correct spots (serialization of cell arrays).
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(Data.Serialize))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> SerializeTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (CodeInstruction instruction in instructions)
            {
                // Intercept call to stloc.1, storing the electricity grid array reference.
                if (instruction.opcode == OpCodes.Stloc_1)
                {
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ElectricityManager), "m_pulseGroups"));
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ElectricityManager), "m_pulseUnits"));
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ElectricityManagerDataPatches), nameof(CustomSerialize)));
                }

                yield return instruction;
            }
        }

        /// <summary>
        /// Performs deserialization activites when loading game data.
        /// Converts loaded data into 81 tiles format and ensures correct 81-tile array sizes.
        /// </summary>
        /// <param name="electricityManager">ElectricityManager instance.</param>
        /// <param name="electricityGrid">ElectricityManager m_electricityGrid array.</param>
        /// <param name="pulseGroups">ElectricityManager m_pulseGroups array.</param>
        /// <param name="pulseUnits">ElectricityManager m_pulseUnits array.</param>
        private static void CustomDeserialize(ElectricityManager electricityManager, Cell[] electricityGrid, PulseGroup[] pulseGroups, PulseUnit[] pulseUnits)
        {
            ExpandedPulseGroup[] newPulseGroups = PulseGroups;
            ExpandedPulseUnit[] newPulseUnits = PulseUnits;

            if (Singleton<SimulationManager>.instance.m_serializableDataStorage.TryGetValue("fakeEM", out byte[] data))
            {
                Logging.Message("found expanded electricity data");
                using (MemoryStream stream = new MemoryStream(data))
                {
                    DataSerializer.Deserialize<ElectricityDataContainer>(stream, DataSerializer.Mode.Memory, LegacyTypeConverter);
                }
            }
            else
            {
                Logging.Message("no expanded district data found - coverting vanilla data");

                // New electricity grid for 81 tiles.
                Cell[] newElectricityGrid = new Cell[ExpandedElectricityGridArraySize];
                AccessTools.Field(typeof(ElectricityManager), "m_electricityGrid").SetValue(electricityManager, newElectricityGrid);

                // Convert 25-tile data into 81-tile equivalent locations.
                for (int z = 0; z < GameElectricityGridResolution; ++z)
                {
                    for (int x = 0; x < GameElectricityGridResolution; ++x)
                    {
                        int oldCellIndex = (z * GameElectricityGridResolution) + x;
                        int newCellIndex = ((z + CellConversionOffset) * ExpandedElectricityGridResolution) + x + CellConversionOffset;
                        newElectricityGrid[newCellIndex] = electricityGrid[oldCellIndex];
                    }
                }

                // Convert PulseGroups.
                ExpandedPulseGroup[] expandedPulseGroups = PulseGroups;
                {
                    for (int i = 0; i < pulseGroups.Length; ++i)
                    {
                        expandedPulseGroups[i] = new ExpandedPulseGroup
                        {
                            m_origCharge = pulseGroups[i].m_origCharge,
                            m_curCharge = pulseGroups[i].m_curCharge,
                            m_mergeIndex = pulseGroups[i].m_mergeIndex,
                            m_mergeCount = pulseGroups[i].m_mergeCount,
                            m_x = pulseGroups[i].m_x,
                            m_z = pulseGroups[i].m_z,
                        };
                    }
                }

                // Convert PulseUnits.
                ExpandedPulseUnit[] expandedPulseUnits = PulseUnits;
                {
                    for (int i = 0; i < pulseUnits.Length; ++i)
                    {
                        expandedPulseUnits[i] = new ExpandedPulseUnit
                        {
                            m_group = pulseUnits[i].m_group,
                            m_node = pulseUnits[i].m_node,
                            m_x = pulseUnits[i].m_x,
                            m_z = pulseUnits[i].m_z,
                        };
                    }
                }
            }
        }

        /// <summary>
        /// Performs serialization activites when saving game data.
        /// Saves the 25-tile subset of 81-tile data.
        /// </summary>
        /// <param name="electricityGrid">ElectricityManager m_electricityGrid array.</param>
        /// <param name="pulseGroups">ElectricityManager m_pulseGroups array.</param>
        /// <param name="pulseUnits">ElectricityManager m_pulseUnits array.</param>
        /// <returns>25-tile electricity grid for serialization.</returns>
        private static Cell[] CustomSerialize(Cell[] electricityGrid, PulseGroup[] pulseGroups, PulseUnit[] pulseUnits)
        {
            // New (temporary) vanilla electricity grid for saving.
            Cell[] vanillaElectricityGrid = new Cell[GameElectricyGridArraySize];

            for (int z = 0; z < GameElectricityGridResolution; ++z)
            {
                for (int x = 0; x < GameElectricityGridResolution; ++x)
                {
                    int gameGridIndex = (z * GameElectricityGridResolution) + x;
                    int expandedGridIndex = ((z + CellConversionOffset) * ExpandedElectricityGridResolution) + x + CellConversionOffset;
                    vanillaElectricityGrid[gameGridIndex] = electricityGrid[expandedGridIndex];
                }
            }

            // Convert PulseGroups.
            ExpandedPulseGroup[] expandedPulseGroups = PulseGroups;
            {
                for (int i = 0; i < pulseGroups.Length; ++i)
                {
                    pulseGroups[i] = new PulseGroup
                    {
                        m_origCharge = expandedPulseGroups[i].m_origCharge,
                        m_curCharge = expandedPulseGroups[i].m_curCharge,
                        m_mergeIndex = expandedPulseGroups[i].m_mergeIndex,
                        m_mergeCount = expandedPulseGroups[i].m_mergeCount,
                        m_x = (byte)Mathf.Clamp(expandedPulseGroups[i].m_x, 0, 255),
                        m_z = (byte)Mathf.Clamp(expandedPulseGroups[i].m_z, 0, 255),
                    };
                }
            }

            // Convert PulseUnits.
            ExpandedPulseUnit[] expandedPulseUnits = PulseUnits;
            {
                for (int i = 0; i < pulseUnits.Length; ++i)
                {
                    pulseUnits[i] = new PulseUnit
                    {
                        m_group = pulseUnits[i].m_group,
                        m_node = pulseUnits[i].m_node,
                        m_x = (byte)Mathf.Clamp(expandedPulseUnits[i].m_x, 0, 255),
                        m_z = (byte)Mathf.Clamp(expandedPulseUnits[i].m_z, 0, 255),
                    };
                }
            }

            return vanillaElectricityGrid;
        }

        /// <summary>
        /// Legacy container type converter.
        /// </summary>
        /// <param name="legacyTypeName">Legacy type name (ignored).</param>
        /// <returns>ElectricityDataContainer type.</returns>
        private static Type LegacyTypeConverter(string legacyTypeName) => typeof(ElectricityDataContainer);
    }
}