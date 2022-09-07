// <copyright file="ElectricityManagerDataPatches.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace EightyOne2.Patches
{
    using System.Collections.Generic;
    using System.Reflection.Emit;
    using HarmonyLib;
    using static ElectricityManager;
    using static ElectricityManagerPatches;

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
                    // ElectricityManager instance reference for storing return value.
                    yield return new CodeInstruction(OpCodes.Ldloc_0);

                    // Call custom method.
                    yield return new CodeInstruction(OpCodes.Ldloc_1);
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ElectricityManager), "m_pulseGroups"));
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ElectricityManager), "m_pulseUnits"));
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ElectricityManagerDataPatches), nameof(CustomDeserialize)));

                    // Store result.
                    yield return new CodeInstruction(OpCodes.Stfld, AccessTools.Field(typeof(ElectricityManager), "m_electricityGrid"));
                }

                yield return instruction;
            }
        }

        /// <summary>
        /// Performs deserialization activites when loading game data.
        /// Converts loaded data into 81 tiles format and ensures correct 81-tile array sizes.
        /// </summary>
        /// <param name="electricityGrid">ElectricityManager m_electricityGrid array.</param>
        /// <param name="pulseGroups">ElectricityManager m_pulseGroups array.</param>
        /// <param name="pulseUnits">ElectricityManager m_pulseUnits array.</param>
        /// <returns>Replacement (resized) m_electricytGrid array.</returns>
        private static Cell[] CustomDeserialize(Cell[] electricityGrid, PulseGroup[] pulseGroups, PulseUnit[] pulseUnits)
        {
            // New eelctricity grid for 81 tiles.
            Cell[] newElectricityGrid = new Cell[ExpandedElectricityGridArraySize];

            // Convert 25-tile data into 81-tile equivalent locations.
            for (int z = 0; z < GameElectricyGridResolution; ++z)
            {
                for (int x = 0; x < GameElectricyGridResolution; ++x)
                {
                    int oldCellIndex = (z * GameElectricyGridResolution) + x;
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

            return newElectricityGrid;
        }
    }
}