// <copyright file="DistrictManagerDataPatches.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace EightyOne2.Patches
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Reflection.Emit;
    using AlgernonCommons;
    using ColossalFramework;
    using ColossalFramework.IO;
    using EightyOne2.Serialization;
    using HarmonyLib;
    using static DistrictManager;
    using static DistrictManagerPatches;

    /// <summary>
    /// Harmony patches for the district manager's data handling to implement 81 tiles functionality.
    /// </summary>
    [HarmonyPatch(typeof(Data))]
    internal static class DistrictManagerDataPatches
    {
        // Legacy 81 tiles data ID.
        private const string DataID = "fakeDM";

        // Data conversion offset - outer margin of 25-tile data when placed in an 81-tile context.
        private const int CellConversionOffset = (int)ExpandedDistrictGridHalfResolution - (int)GameDistrictGridHalfResolution;

        /// <summary>
        /// Harmony transpiler for DistrictManager.Data.Deserialize to insert call to custom deserialize method.
        /// Done this way instead of via Postfix as we need the original DistrictManager instance (Harmomy Postfix will only give DistrictManager.Data instance).
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(Data.Deserialize))]
        [HarmonyTranspiler]
        internal static IEnumerable<CodeInstruction> DeserializeTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Ret)
                {
                    // Insert call to our custom post-deserialize method immediately before the end of the target method (final ret).
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(DistrictManagerDataPatches), nameof(CustomDeserialize)));
                }

                yield return instruction;
            }
        }

        /// <summary>
        /// Initializes a new district cell array with correct initial values.
        /// </summary>
        /// <param name="districtCellArray">Array to initialize.</param>
        internal static void InitializeDistrictCellArray(Cell[] districtCellArray)
        {
            // Properly initialize each cell.
            for (int i = 0; i < districtCellArray.Length; ++i)
            {
                // Initial values per DistrictManager.Awake().
                districtCellArray[i] = new Cell
                {
                    m_district1 = 0,
                    m_district2 = 1,
                    m_district3 = 2,
                    m_district4 = 3,
                    m_alpha1 = byte.MaxValue,
                    m_alpha2 = 0,
                    m_alpha3 = 0,
                    m_alpha4 = 0,
                };
            }
        }

        /// <summary>
        /// Harmony transpiler for DistrictManager.Data.Serialize to insert call to custom serialize method at the correct spots (serialization of cell arrays).
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch(nameof(Data.Serialize))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> SerializeTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            // Tag methods.
            MethodInfo beginWrite = AccessTools.Method(typeof(EncodedArray.Byte), nameof(EncodedArray.Byte.BeginWrite));
            MethodInfo endWrite = AccessTools.Method(typeof(EncodedArray.Byte), nameof(EncodedArray.Byte.EndWrite));
            MethodInfo customSerialize = AccessTools.Method(typeof(DistrictManagerDataPatches), nameof(CustomSerialize));

            // Transpiling counter.
            int beginWriteCount = 0;

            IEnumerator<CodeInstruction> instructionEnumerator = instructions.GetEnumerator();
            while (instructionEnumerator.MoveNext())
            {
                CodeInstruction instruction = instructionEnumerator.Current;

                // Skip first call, which is varsity colors.
                if (instruction.Calls(beginWrite) && beginWriteCount++ > 0)
                {
                    yield return instruction;

                    // Insert call to custom method, keeping EncodedArray.Byte instance on top of stack.
                    yield return new CodeInstruction(OpCodes.Dup);

                    // First pass is districtGrid (local 3), second is parkGrid (local 4).
                    if (beginWriteCount == 2)
                    {
                        yield return new CodeInstruction(OpCodes.Ldloc_3);
                    }
                    else
                    {
                        yield return new CodeInstruction(OpCodes.Ldloc_S, 4);
                    }

                    yield return new CodeInstruction(OpCodes.Call, customSerialize);

                    // Skip forward until we find the call to EndWrite.
                    do
                    {
                        instructionEnumerator.MoveNext();
                        instruction = instructionEnumerator.Current;
                    }
                    while (!instruction.Calls(endWrite));
                }

                yield return instruction;
            }
        }

        /// <summary>
        /// Performs deserialization activites when loading game data.
        /// Converts loaded data into 81 tiles format and ensures correct 81-tile array sizes.
        /// </summary>
        /// <param name="instance">DistrictManager instance.</param>
        private static void CustomDeserialize(DistrictManager instance)
        {
            // See if this save contains any extended 81 tiles data.
            if (Singleton<SimulationManager>.instance.m_serializableDataStorage.TryGetValue(DataID, out byte[] data))
            {
                using (MemoryStream stream = new MemoryStream(data))
                {
                    Logging.Message("Found expanded district data");
                    DataSerializer.Deserialize<DistrictDataContainer>(stream, DataSerializer.Mode.Memory, LegacyTypeConverter);
                }
            }
            else
            {
                Logging.Message("No expanded district data found - coverting vanilla data");

                // New area grid for 81 tiles.
                Cell[] newDistrictGrid = new Cell[ExpandedDistrictGridArraySize];
                Cell[] newParkGrid = new Cell[ExpandedDistrictGridArraySize];

                // Initialize arrays to proper defaults.
                // Yes, this does mean that we'll end up overwriting ~32% of this,
                // but reliability is more important than shaving off a couple of milliseconds on a first-time load.
                InitializeDistrictCellArray(newDistrictGrid);
                InitializeDistrictCellArray(newParkGrid);

                // Convert 25-tile data into 81-tile equivalent locations.
                for (int z = 0; z < GameDistrictGridResolution; ++z)
                {
                    for (int x = 0; x < GameDistrictGridResolution; ++x)
                    {
                        int gameGridIndex = (z * GameDistrictGridResolution) + x;
                        int expandedGridIndex = ((z + CellConversionOffset) * ExpandedDistrictGridResolution) + x + CellConversionOffset;

                        Cell districtGridCell = instance.m_districtGrid[gameGridIndex];
                        newDistrictGrid[expandedGridIndex] = districtGridCell;

                        Cell parkGridCell = instance.m_parkGrid[gameGridIndex];
                        newParkGrid[expandedGridIndex] = parkGridCell;
                    }
                }

                // Replace existing fields with 81 tiles replacements.
                instance.m_districtGrid = newDistrictGrid;
                instance.m_parkGrid = newParkGrid;
            }
        }

        /// <summary>
        /// Performs serialization activites when saving game data.
        /// Saves the 25-tile subset of 81-tile data.
        /// </summary>
        /// <param name="encodedArray">Encoded array to write to.</param>
        /// <param name="districtCellArray">District cell array to write.</param>
        private static void CustomSerialize(EncodedArray.Byte encodedArray, Cell[] districtCellArray)
        {
            // Get 25-tile subset of 81-tile data.
            // Serialization is by field at a time.
            for (int z = 0; z < GameDistrictGridResolution; ++z)
            {
                for (int x = 0; x < GameDistrictGridResolution; ++x)
                {
                    int expandedGridIndex = ((z + CellConversionOffset) * ExpandedDistrictGridResolution) + x + CellConversionOffset;
                    encodedArray.Write(districtCellArray[expandedGridIndex].m_district1);
                }
            }

            for (int z = 0; z < GameDistrictGridResolution; ++z)
            {
                for (int x = 0; x < GameDistrictGridResolution; ++x)
                {
                    int expandedGridIndex = ((z + CellConversionOffset) * ExpandedDistrictGridResolution) + x + CellConversionOffset;
                    encodedArray.Write(districtCellArray[expandedGridIndex].m_district2);
                }
            }

            for (int z = 0; z < GameDistrictGridResolution; ++z)
            {
                for (int x = 0; x < GameDistrictGridResolution; ++x)
                {
                    int expandedGridIndex = ((z + CellConversionOffset) * ExpandedDistrictGridResolution) + x + CellConversionOffset;
                    encodedArray.Write(districtCellArray[expandedGridIndex].m_district3);
                }
            }

            for (int z = 0; z < GameDistrictGridResolution; ++z)
            {
                for (int x = 0; x < GameDistrictGridResolution; ++x)
                {
                    int expandedGridIndex = ((z + CellConversionOffset) * ExpandedDistrictGridResolution) + x + CellConversionOffset;
                    encodedArray.Write(districtCellArray[expandedGridIndex].m_district4);
                }
            }

            for (int z = 0; z < GameDistrictGridResolution; ++z)
            {
                for (int x = 0; x < GameDistrictGridResolution; ++x)
                {
                    int expandedGridIndex = ((z + CellConversionOffset) * ExpandedDistrictGridResolution) + x + CellConversionOffset;
                    encodedArray.Write(districtCellArray[expandedGridIndex].m_alpha1);
                }
            }

            for (int z = 0; z < GameDistrictGridResolution; ++z)
            {
                for (int x = 0; x < GameDistrictGridResolution; ++x)
                {
                    int expandedGridIndex = ((z + CellConversionOffset) * ExpandedDistrictGridResolution) + x + CellConversionOffset;
                    encodedArray.Write(districtCellArray[expandedGridIndex].m_alpha2);
                }
            }

            for (int z = 0; z < GameDistrictGridResolution; ++z)
            {
                for (int x = 0; x < GameDistrictGridResolution; ++x)
                {
                    int expandedGridIndex = ((z + CellConversionOffset) * ExpandedDistrictGridResolution) + x + CellConversionOffset;
                    encodedArray.Write(districtCellArray[expandedGridIndex].m_alpha3);
                }
            }

            for (int z = 0; z < GameDistrictGridResolution; ++z)
            {
                for (int x = 0; x < GameDistrictGridResolution; ++x)
                {
                    int expandedGridIndex = ((z + CellConversionOffset) * ExpandedDistrictGridResolution) + x + CellConversionOffset;
                    encodedArray.Write(districtCellArray[expandedGridIndex].m_alpha4);
                }
            }
        }

        /// <summary>
        /// Legacy container type converter.
        /// </summary>
        /// <param name="legacyTypeName">Legacy type name (ignored).</param>
        /// <returns>DistrictDataContainer type.</returns>
        private static Type LegacyTypeConverter(string legacyTypeName) => typeof(DistrictDataContainer);
    }
}