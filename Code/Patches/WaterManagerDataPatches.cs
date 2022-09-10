// <copyright file="WaterManagerDataPatches.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace EightyOne2.Patches
{
    using System.Collections.Generic;
    using System.Reflection.Emit;
    using AlgernonCommons;
    using HarmonyLib;
    using UnityEngine;
    using static ExpandedWaterManager;
    using static WaterManager;
    using static WaterManagerPatches;

    /// <summary>
    /// Harmony patches for the water manager's data handling to implement 81 tiles functionality.
    /// </summary>
    [HarmonyPatch(typeof(Data))]
    internal static class WaterManagerDataPatches
    {
        // Data conversion offset - outer margin of 25-tile data when placed in an 81-tile context.
        private const int CellConversionOffset = (int)ExpandedWaterGridHalfResolution - (int)GameWaterGridHalfResolution;

        /// <summary>
        /// Harmony transpiler for WaterManager.Data.AfterDeserialize to replace hardcoded game constants.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(Data.AfterDeserialize))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> AfterDeserializeTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceWaterConstants(instructions);

        /// <summary>
        /// Harmony transpiler for WaterManager.Data.Deserialize to insert call to custom deserialize method.
        /// Done this way instead of via PostFix as we need the original WaterManager instance (Harmomy Postfix will only give WaterManager.Data instance).
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
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(WaterManager), "m_waterPulseUnits"));
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(WaterManager), "m_sewagePulseUnits"));
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(WaterManager), "m_heatingPulseUnits"));
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(WaterManagerDataPatches), nameof(CustomDeserialize)));
                }

                yield return instruction;
            }
        }

        /// <summary>
        /// Harmony transpiler for WaterManager.Data.Serialize to insert call to custom serialize method at the correct spots (serialization of cell arrays).
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(Data.Serialize))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> SerializeTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (CodeInstruction instruction in instructions)
            {
                // Intercept call to stloc.1, storing the water grid array reference.
                if (instruction.opcode == OpCodes.Stloc_1)
                {
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(WaterManager), "m_waterPulseUnits"));
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(WaterManager), "m_sewagePulseGroups"));
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(WaterManager), "m_heatingPulseGroups"));
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(WaterManagerDataPatches), nameof(CustomSerialize)));
                }

                yield return instruction;
            }
        }

        /// <summary>
        /// Performs deserialization activites when loading game data.
        /// Converts loaded data into 81 tiles format and ensures correct 81-tile array sizes.
        /// </summary>
        /// <param name="waterManager">WaterManager instance.</param>
        /// <param name="waterGrid">WaterManager m_waterGrid array.</param>
        /// <param name="waterPulseUnits">WaterManager m_waterPulseUnits array.</param>
        /// <param name="sewagePulseUnits">WaterManager m_sewagePulseUnits array.</param>
        /// <param name="heatingPulseUnits">WaterManager m_heatingPulseUnits array.</param>
        private static void CustomDeserialize(WaterManager waterManager, Cell[] waterGrid, PulseUnit[] waterPulseUnits, PulseUnit[] sewagePulseUnits, PulseUnit[] heatingPulseUnits)
        {
            /*if (Singleton<SimulationManager>.instance.m_serializableDataStorage.TryGetValue(WaterDataSerializer.DataID, out byte[] data))
            {
                Logging.Message("found expanded water data");
                using (MemoryStream stream = new MemoryStream(data))
                {
                    DataSerializer.Deserialize<WaterDataContainer>(stream, DataSerializer.Mode.Memory, LegacyTypeConverter);
                }
            }
            else*/
            {
                Logging.Message("no expanded water data found - coverting vanilla data");

                // New water grid for 81 tiles.
                Cell[] newWaterGrid = new Cell[ExpandedWaterGridArraySize];
                AccessTools.Field(typeof(WaterManager), "m_waterGrid").SetValue(waterManager, newWaterGrid);

                // Convert 25-tile data into 81-tile equivalent locations.
                for (int z = 0; z < GameWaterGridResolution; ++z)
                {
                    for (int x = 0; x < GameWaterGridResolution; ++x)
                    {
                        int oldCellIndex = (z * GameWaterGridResolution) + x;
                        int newCellIndex = ((z + CellConversionOffset) * ExpandedWaterGridResolution) + x + CellConversionOffset;
                        newWaterGrid[newCellIndex] = waterGrid[oldCellIndex];
                    }
                }

                // Convert PulseUnits.
                ConvertVanillaPulseUnits(WaterPulseUnits, waterPulseUnits);
                ConvertVanillaPulseUnits(SewagePulseUnits, sewagePulseUnits);
                ConvertVanillaPulseUnits(HeatingPulseUnits, heatingPulseUnits);
            }
        }

        /// <summary>
        /// Performs serialization activites when saving game data.
        /// Saves the 25-tile subset of 81-tile data.
        /// </summary>
        /// <param name="waterGrid">WaterManager m_waterGrid array.</param>
        /// <param name="waterPulseUnits">WaterManager m_waterPulseUnits array.</param>
        /// <param name="sewagePulseUnits">WaterManager m_sewagePulseUnits array.</param>
        /// <param name="heatingPulseUnits">WaterManager m_heatingPulseUnits array.</param>
        /// <returns>25-tile water grid for serialization.</returns>
        private static Cell[] CustomSerialize(Cell[] waterGrid, PulseUnit[] waterPulseUnits, PulseUnit[] sewagePulseUnits, PulseUnit[] heatingPulseUnits)
        {
            // New (temporary) vanilla water grid for saving.
            Cell[] vanillaWaterGrid = new Cell[GameWaterGridArraySize];

            for (int z = 0; z < GameWaterGridResolution; ++z)
            {
                for (int x = 0; x < GameWaterGridResolution; ++x)
                {
                    int gameGridIndex = (z * GameWaterGridResolution) + x;
                    int expandedGridIndex = ((z + CellConversionOffset) * ExpandedWaterGridResolution) + x + CellConversionOffset;
                    vanillaWaterGrid[gameGridIndex] = waterGrid[expandedGridIndex];
                }
            }

            // Convert PulseUnits.
            ConvertExpandedPulseUnits(WaterPulseUnits, waterPulseUnits);
            ConvertExpandedPulseUnits(SewagePulseUnits, sewagePulseUnits);
            ConvertExpandedPulseUnits(HeatingPulseUnits, heatingPulseUnits);

            return vanillaWaterGrid;
        }

        private static void ConvertVanillaPulseUnits(ExpandedPulseUnit[] expandedPulseUnits, PulseUnit[] pulseUnits)
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

        private static void ConvertExpandedPulseUnits(ExpandedPulseUnit[] expandedPulseUnits, PulseUnit[] pulseUnits)
        {
            for (int i = 0; i < pulseUnits.Length; ++i)
            {
                pulseUnits[i] = new PulseUnit
                {
                    m_group = pulseUnits[i].m_group,
                    m_node = pulseUnits[i].m_node,
                    m_x = (byte)Mathf.Clamp(expandedPulseUnits[i].m_x, 0, byte.MaxValue),
                    m_z = (byte)Mathf.Clamp(expandedPulseUnits[i].m_z, 0, byte.MaxValue),
                };
            }
        }

        /// <summary>
        /// Legacy container type converter.
        /// </summary>
        /// <param name="legacyTypeName">Legacy type name (ignored).</param>
        /// <returns>WaterDataContainer type.</returns>
        //private static Type LegacyTypeConverter(string legacyTypeName) => typeof(WaterDataContainer);
    }
}